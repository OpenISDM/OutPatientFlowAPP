/*
 * 2020 © Copyright (c) BiDaE Technology Inc. 
 * Provided under BiDaE SHAREWARE LICENSE-1.0 in the LICENSE.
 *
 * Project Name:
 *
 *      IndoorNavigation
 *
 *      
 * Version:
 *
 *      1.0.0, 20190605
 * 
 * File Name:
 *
 *      Session.cs
 *
 * Abstract:
 *
 *    Calculate the best route and use waypoint and region to connect to a 
 *    route, and add the intersted waypoints that include next waypoint, 
 *    next next waypoint and wrong waypoints.
 *    When we get the matched waypoint and region, we will check if it is 
 *    correct waypoint or wrong waypoint.
 *   
 *
 * Authors:
 *
 *      Eric Lee, ericlee@iis.sinica.edu.tw
 */

using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Dijkstra.NET.Model;
using Dijkstra.NET.Extensions;
using IndoorNavigation.Models.NavigaionLayer;
using IndoorNavigation.Models;
using IndoorNavigation.Modules.IPSClients;

namespace IndoorNavigation.Modules
{
    public class Session_tmp
    {
        #region Attributes and Objects
        private int _nextWaypointStep;

        private List<RegionWaypointPoint> _waypointsOnRoute =
            new List<RegionWaypointPoint>();

        private Dictionary<RegionWaypointPoint, List<RegionWaypointPoint>>
            _waypointsOnWrongWay =
                new Dictionary<RegionWaypointPoint,
                    List<RegionWaypointPoint>>();

        private Graph<Guid, string> _graphRegionGraph =
            new Graph<Guid, string>();

        private NavigationGraph _navigationGraph;
        private Guid _destinationRegionID;
        private Guid _destinationWaypointID;

        private Thread _waypointDetectionThread;
        private Thread _navigationControllerThread;

        private const int _remindDistance = 50;
        private int _accumulateStraightDistance = 0;

        private bool _isKeepDetection;
        private Guid _currentRegionID;
        private Guid _currentWaypointID;

        private ConnectionType[] _avoidConnectionTypes;

        private ManualResetEventSlim _nextWaypointEvent =
            new ManualResetEventSlim(false);

        private ManualResetEventSlim _pauseThreadEvent =
            new ManualResetEventSlim(true);

        public NavigationEvent _event { get; private set; }

        private Dictionary<Guid, Region> _regiongraphs =
            new Dictionary<Guid, Region>();

        private IPSModule_v3 _iPSModules;
        private const int _tooCLoseDistance = 8;

        #region wait for test variable
        private int TmpCurrentProgress = 0;
        private int TmpTotalProgress = 0;
        private bool _DetectWrongWaypoint = true;
        #endregion
        #endregion

        public Session_tmp(NavigationGraph navigationGraph,
                       Guid destinationRegionID,
                       Guid destinationWaypointID,
                       ConnectionType[] avoidConnectionTypes)
        {
            _event = new NavigationEvent();

            _navigationGraph = navigationGraph;

            _destinationRegionID = destinationRegionID;
            _destinationWaypointID = destinationWaypointID;
            _accumulateStraightDistance = 0;
            _avoidConnectionTypes = avoidConnectionTypes;
            // construct region graph (across regions) which we can use to 
            // generate route

            _graphRegionGraph =
                navigationGraph.GenerateRegionGraph(avoidConnectionTypes);

            _regiongraphs = _navigationGraph.GetRegions();
            _nextWaypointStep = -1;
            _isKeepDetection = true;
            _iPSModules = new IPSModule_v3(_navigationGraph);
            _iPSModules._event._eventHandler +=
                new EventHandler(CheckArrivedWaypoint);

            _waypointDetectionThread = new Thread(() => InvokeIPSWork());
            _waypointDetectionThread.Start();

            _navigationControllerThread = new Thread(() => NavigatorProgram());
            _navigationControllerThread.Start();
        }

        private void NavigatorProgram()
        {
            _nextWaypointStep = -1;
            _currentRegionID = new Guid();
            _currentWaypointID = new Guid();
            RegionWaypointPoint checkWrongRegionWaypoint =
                new RegionWaypointPoint();

            NavigateToNextWaypoint(_nextWaypointStep);

            while (true == _isKeepDetection &&
                   !(_currentRegionID.Equals(_destinationRegionID) &&
                     _currentWaypointID.Equals(_destinationWaypointID)))
            {
                _pauseThreadEvent.Wait();
                Console.WriteLine("Continue to navigate to next step, current"
                    + "location {0}/{1}", _currentRegionID, _currentWaypointID);

                _nextWaypointEvent.Wait();

                checkWrongRegionWaypoint._regionID = _currentRegionID;
                checkWrongRegionWaypoint._waypointID = _currentWaypointID;

                Console.WriteLine("current waypointID : " + _currentWaypointID);

                if (_currentRegionID.Equals(_destinationRegionID) &&
                    _currentWaypointID.Equals(_destinationWaypointID))
                {
                    Console.WriteLine("Arrived destination! {0}/{1}",
                                      _destinationRegionID,
                                      _destinationWaypointID);

                    _isKeepDetection = false;
                    _iPSModules.CloseAllActiveClient();
                    break;
                }

                if (_nextWaypointStep == -1)
                {
                    Console.WriteLine("Detected start waypoint: "
                                      + _currentWaypointID);
                    // Detection of starting waypoing:
                    // Detected the waypoint most closed to user.
                    GenerateRoute(_currentRegionID,
                                  _currentWaypointID,
                                  _destinationRegionID,
                                  _destinationWaypointID);

                    _nextWaypointStep++;

                    NavigateToNextWaypoint(_nextWaypointStep);
                }

                else if (_currentRegionID.Equals(
                         _waypointsOnRoute[_nextWaypointStep]._regionID) &&
                         _currentWaypointID.Equals(
                         _waypointsOnRoute[_nextWaypointStep]._waypointID))
                {
                    Console.WriteLine("Arrived region/waypoint: {0}/{1}",
                                      _currentRegionID,
                                      _currentWaypointID);
                    _nextWaypointStep++;

                    NavigateToNextWaypoint(_nextWaypointStep);
                }
                else if (_nextWaypointStep >= 1 &&
                    _waypointsOnWrongWay
                    [_waypointsOnRoute[_nextWaypointStep - 1]]
                    .Contains(checkWrongRegionWaypoint) == true &&
                    _DetectWrongWaypoint == false)
                {

                    Console.WriteLine("In Program Wrong, going to Re-calculate"
                        + "the route");
                    _nextWaypointStep = 0;

                    GenerateRoute(
                                    _currentRegionID,
                                    _currentWaypointID,
                                    _destinationRegionID,
                                    _destinationWaypointID);
                    _DetectWrongWaypoint = true;
                    Console.WriteLine("Finish Construct New Route");

                    Guid previousRegionID = new Guid();
                    Guid previousWaypointID = new Guid();

                    // Add this function can avoid that when users go to the 
                    // worong waypoint, the instuction will jump to fast.
                    RegionWaypointPoint regionWaypointPoint =
                        new RegionWaypointPoint();
                    regionWaypointPoint._regionID = _currentRegionID;
                    regionWaypointPoint._waypointID = _currentWaypointID;

                    NavigationInstruction navigationInstruction =
                        new NavigationInstruction();

                    navigationInstruction._currentWaypointName =
                        _navigationGraph.
                        GetWaypointNameInRegion(_currentRegionID,
                                                _currentWaypointID);

                    navigationInstruction._nextWaypointName =
                        _navigationGraph
                        .GetWaypointNameInRegion(_waypointsOnRoute[1]._regionID,
                                              _waypointsOnRoute[1]._waypointID);


                    navigationInstruction._progress =
                        TmpCurrentProgress / --TmpTotalProgress;
                    navigationInstruction._progressBar =
                        string.Format("{0}/{1}", TmpCurrentProgress,
                        TmpTotalProgress);

                    try
                    {
                        navigationInstruction._information = _navigationGraph
                            .GetInstructionInformation(
                                    _nextWaypointStep,
                                    previousRegionID,
                                    previousWaypointID,
                                    _currentRegionID,
                                    _currentWaypointID,
                                    _waypointsOnRoute[_nextWaypointStep + 1]
                                    ._regionID,
                                    _waypointsOnRoute[_nextWaypointStep + 1]
                                    ._waypointID,
                                    _waypointsOnRoute[_nextWaypointStep + 2]
                                    ._regionID,
                                    _waypointsOnRoute[_nextWaypointStep + 2]
                                    ._waypointID,
                                    _avoidConnectionTypes
                                    );
                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine("GetInstructionError - "
                            + exc.Message);
                        navigationInstruction._information = _navigationGraph
                            .GetInstructionInformation
                            (
                                _nextWaypointStep,
                                previousRegionID, previousWaypointID,
                                _currentRegionID, _currentWaypointID,
                                _waypointsOnRoute[_nextWaypointStep + 1]
                                ._regionID,
                                _waypointsOnRoute[_nextWaypointStep + 1]
                                ._waypointID,
                                new Guid(),
                                new Guid(),
                                _avoidConnectionTypes
                            );
                    }
                    navigationInstruction._currentWaypointGuid =
                        _currentWaypointID;

                    navigationInstruction._nextWaypointGuid =
                        _waypointsOnRoute[_nextWaypointStep + 1]._waypointID;

                    navigationInstruction._currentRegionGuid =
                        _currentRegionID;

                    navigationInstruction._nextRegionGuid =
                        _waypointsOnRoute[_nextWaypointStep + 1]._regionID;

                    if (navigationInstruction._information._nextDirection ==
                        TurnDirection.FirstDirection)
                    {
                        navigationInstruction._turnDirectionDistance =
                            _navigationGraph.GetDistanceOfLongHallway(
                                2,
                                _waypointsOnRoute,
                                _avoidConnectionTypes);
                    }
                    else
                    {
                        navigationInstruction._turnDirectionDistance =
                        _navigationGraph
                        .GetDistanceOfLongHallway(1,
                                                  _waypointsOnRoute,
                                                  _avoidConnectionTypes);
                    }

                    _event.OnEventCall(new NavigationEventArgs
                    {

                        _result = NavigationResult.Run,
                        _nextInstruction = navigationInstruction

                    });
                    _accumulateStraightDistance =

                        _accumulateStraightDistance +
                        navigationInstruction._information._distance;
                    _nextWaypointStep++;
                }
                _nextWaypointEvent.Reset();
            }
            Console.WriteLine("<<Navigator Program");
        }

        private void NavigateToNextWaypoint(int nextStep)
        {
            if (nextStep == -1)
            {
                _iPSModules.InitialStep_DetectAllBeacon(_navigationGraph.GetAllRegionIDs());
                _iPSModules.SetMonitorBeaconList(nextStep);
            }
            else
            {
                Console.WriteLine("NavigateProgram");
                RegionWaypointPoint checkPoint = _waypointsOnRoute[nextStep];

                //_iPSModules.CompareToCurrentAndNextIPSType(_currentRegionID,
                //                                           checkPoint._regionID,
                //                                           _nextWaypointStep);

                //_iPSModules.PSTurnOFF();

                _iPSModules.AddMonitorBeacon
                    (checkPoint._regionID, checkPoint._waypointID);


                if (_nextWaypointStep + 1 < _waypointsOnRoute.Count())
                {

                    if (_waypointsOnRoute[_nextWaypointStep + 1]._regionID ==
                        _currentRegionID)
                    {
                        //_iPSModules
                        //.AddNextNextWaypointInterestedGuid
                        //(_waypointsOnRoute[_nextWaypointStep + 1]._regionID,
                        // _waypointsOnRoute[_nextWaypointStep + 1]._waypointID);
                        _iPSModules.AddMonitorBeacon
                            (_waypointsOnRoute[_nextWaypointStep + 1]
                            ._regionID,
                            _waypointsOnRoute[_nextWaypointStep + 1]
                            ._waypointID);

                    }
                }


                if (_nextWaypointStep >= 1)
                {
                    if (_waypointsOnWrongWay
                        [_waypointsOnRoute[_nextWaypointStep - 1]] != null)
                    {

                        foreach (RegionWaypointPoint items
                            in _waypointsOnWrongWay
                            [_waypointsOnRoute[_nextWaypointStep - 1]])
                        {

                            //_iPSModules
                            //.AddWrongWaypointInterestedGuid(items._regionID,
                            //                                items._waypointID);
                            _iPSModules.AddMonitorBeacon
                                (items._regionID, items._waypointID);
                        }
                    }
                }
                _iPSModules.SetMonitorBeaconList(nextStep);
            }

        }

        private void InvokeIPSWork()
        {
            Console.WriteLine("---- InvokeIPSWork ----");
            while (true == _isKeepDetection)
            {
                _pauseThreadEvent.Wait();
                Thread.Sleep(500);
                _iPSModules.OpenBeaconScanning(_nextWaypointStep);
            }
        }

        #region GeneratePath Methods
        public void GenerateRoute(Guid sourceRegionID,
                                   Guid sourceWaypointID,
                                   Guid destinationRegionID,
                                   Guid destinationWaypointID)
        {
            #region Generate route


            #region Generate rounte between region
            // generate path between regions (from sourceRegionID to 
            // destnationRegionID)
            uint region1Key = _graphRegionGraph
                              .Where(node => node.Item.Equals(sourceRegionID))
                              .Select(node => node.Key).First();
            uint region2Key = _graphRegionGraph
                              .Where(node => node.Item.Equals
                              (destinationRegionID)).Select(node => node.Key)
                              .First();

            var pathRegions = _graphRegionGraph
                             .Dijkstra(region1Key, region2Key)
                             .GetPath();

            if (0 == pathRegions.Count())
            {
                Console.WriteLine("No path.Need to change avoid connection" +
                                  " type");
                _event.OnEventCall(new NavigationEventArgs
                {
                    _result = NavigationResult.NoRoute
                });
                return;
            }

            // store the generate Dijkstra path across regions
            List<Guid> regionsOnRoute = new List<Guid>();
            for (int i = 0; i < pathRegions.Count(); i++)
            {
                regionsOnRoute.Add(_graphRegionGraph[pathRegions.ToList()[i]]
                                   .Item);
            }

            // generate the path of the region/waypoint checkpoints across 
            // regions
            _waypointsOnRoute = new List<RegionWaypointPoint>();
            _waypointsOnRoute.Add(new RegionWaypointPoint
            {
                _regionID = sourceRegionID,
                _waypointID = sourceWaypointID
            });

            for (int i = 0; i < _waypointsOnRoute.Count(); i++)
            {
                RegionWaypointPoint checkPoint = _waypointsOnRoute[i];
                Console.WriteLine("check index = {0}, count = {1}, region {2}"
                    + " waypoint {3}",
                    i,
                    _waypointsOnRoute.Count(),
                    checkPoint._regionID,
                    checkPoint._waypointID);
                if (regionsOnRoute.IndexOf(checkPoint._regionID) + 1 <
                    regionsOnRoute.Count())
                {
                    LocationType waypointType =
                        _navigationGraph
                        .GetWaypointTypeInRegion(checkPoint._regionID,
                                                 checkPoint._waypointID);

                    Guid nextRegionID =
                        regionsOnRoute[regionsOnRoute.
                                       IndexOf(checkPoint._regionID) + 1];

                    PortalWaypoints portalWaypoints =
                        _navigationGraph
                        .GetPortalWaypoints(checkPoint._regionID,
                                            checkPoint._waypointID,
                                            nextRegionID,
                                            _avoidConnectionTypes);

                    if (LocationType.portal != waypointType)
                    {
                        _waypointsOnRoute.Add(new RegionWaypointPoint
                        {
                            _regionID = checkPoint._regionID,
                            _waypointID = portalWaypoints._portalWaypoint1
                        });
                    }
                    else if (LocationType.portal == waypointType)
                    {
                        if (!checkPoint._waypointID.Equals(portalWaypoints
                                                           ._portalWaypoint1))
                        {
                            _waypointsOnRoute.Add(new RegionWaypointPoint
                            {
                                _regionID = checkPoint._regionID,
                                _waypointID = portalWaypoints._portalWaypoint1
                            });
                        }
                        else
                        {
                            _waypointsOnRoute.Add(new RegionWaypointPoint
                            {
                                _regionID = nextRegionID,
                                _waypointID = portalWaypoints._portalWaypoint2
                            });
                        }
                    }
                }
            }
            int indexLastCheckPoint = _waypointsOnRoute.Count() - 1;
            if (!(_destinationRegionID.
                Equals(_waypointsOnRoute[indexLastCheckPoint]._regionID) &&
                destinationWaypointID.
                Equals(_waypointsOnRoute[indexLastCheckPoint]._waypointID)))
            {
                _waypointsOnRoute.Add(new RegionWaypointPoint
                {
                    _regionID = destinationRegionID,
                    _waypointID = destinationWaypointID
                });
            }

            foreach (RegionWaypointPoint checkPoint in _waypointsOnRoute)
            {
                Console.WriteLine("region-graph region/waypoint = {0}/{1}",
                                  checkPoint._regionID,
                                  checkPoint._waypointID);
            }
            #endregion

            #region Generate path in a region
            // fill in all the path between waypoints in the
            // same region / navigraph
            for (int i = 0; i < _waypointsOnRoute.Count() - 1; i++)
            {
                RegionWaypointPoint currentCheckPoint = _waypointsOnRoute[i];
                RegionWaypointPoint nextCheckPoint = _waypointsOnRoute[i + 1];

                if (currentCheckPoint._regionID.Equals
                    (nextCheckPoint._regionID))
                {
                    Graph<Guid, string> _graphNavigraph =
                        _navigationGraph
                        .GenerateNavigraph(currentCheckPoint._regionID,
                                           _avoidConnectionTypes);

                    // generate path between two waypoints in the
                    // same region / navigraph
                    uint waypoint1Key = _graphNavigraph
                                        .Where(node => node.Item
                                        .Equals(currentCheckPoint._waypointID))
                                        .Select(node => node.Key).First();
                    uint waypoint2Key = _graphNavigraph
                                        .Where(node => node.Item
                                        .Equals(nextCheckPoint._waypointID))
                                        .Select(node => node.Key).First();

                    var pathWaypoints =
                        _graphNavigraph.Dijkstra(waypoint1Key, waypoint2Key)
                                       .GetPath();

                    for (int j = pathWaypoints.Count() - 1; j > 0; j--)
                    {
                        if (j != 0 && j != pathWaypoints.Count() - 1)
                        {
                            _waypointsOnRoute.Insert(i + 1,
                                                     new RegionWaypointPoint
                                                     {
                                                         _regionID =
                                                         currentCheckPoint
                                                         ._regionID,
                                                         _waypointID =
                                    _graphNavigraph[pathWaypoints.ToList()[j]]
                                    .Item
                                                     });
                        }
                    }
                }
            }
            #endregion

            #endregion

            #region To remove dumplicate point
            int x = 0;
            int y = 1;
            while (x < y && y < _waypointsOnRoute.Count)
            {
                RegionWaypointPoint point1 = _waypointsOnRoute[x];
                RegionWaypointPoint point2 = _waypointsOnRoute[y];
                List<Guid> waypointIDs1 =
                    _navigationGraph.GetAllBeaconIDInOneWaypointOfRegion
                    (point1._regionID, point1._waypointID);
                List<Guid> waypointIDs2 =
                    _navigationGraph.GetAllBeaconIDInOneWaypointOfRegion
                    (point2._regionID, point2._waypointID);

                LocationType locationType1 =
                    _navigationGraph.GetWaypointTypeInRegion
                    (point1._regionID, point1._waypointID);

                LocationType locationTyp2 =
                    _navigationGraph.GetWaypointTypeInRegion
                    (point2._regionID, point2._waypointID);
                if (waypointIDs1.Count == 1 && waypointIDs2.Count == 1
                    && waypointIDs1[0].Equals(waypointIDs2[0]))
                {
                    if (locationType1 == locationTyp2 && locationType1 !=
                        LocationType.portal)
                    {
                        _waypointsOnRoute.RemoveAt(y);
                    }
                    else if (point2._waypointID.Equals(_destinationWaypointID) &&
                       _waypointsOnRoute.Count == 2)
                    {
                        Console.WriteLine("dddddd");
                        _waypointsOnRoute.RemoveAt(x);
                    }
                    else if (point2._waypointID.Equals(_destinationWaypointID)
                        && locationType1 == locationTyp2 &&
                        locationTyp2 == LocationType.portal)
                    {
                        Console.WriteLine("wwwwww");
                        _waypointsOnRoute.RemoveAt(x);
                    }
                    else if (locationType1 != locationTyp2)
                    {
                        Console.WriteLine("eeeee");
                        if (locationTyp2 == LocationType.portal)
                        {
                            _waypointsOnRoute.RemoveAt(x);
                        }

                        else if (locationType1 == LocationType.portal)
                        {
                            _waypointsOnRoute.RemoveAt(y);
                        }
                    }
                }
                x++;
                y++;
            }
            #endregion

            #region Wrong Point decide
            int nextStep = 1;
            _waypointsOnWrongWay =
                new Dictionary<RegionWaypointPoint, List<RegionWaypointPoint>>
                ();
            Region tempRegion = new Region();
            List<Guid> neighborGuid = new List<Guid>();

            //For each waypoint in _waypointsOnRoute, decide their wrong 
            //waypoint.
            foreach (RegionWaypointPoint locationRegionWaypoint
                     in _waypointsOnRoute)
            {
                //Console.WriteLine("Important Current Waypoint : "
                //                  + locationRegionWaypoint._waypointID);
                //Get the neighbor of all wapoint in _waypointOnRoute.
                neighborGuid =
                    _navigationGraph
                    .GetNeighbor(locationRegionWaypoint._regionID,
                    locationRegionWaypoint._waypointID);

                tempRegion = _regiongraphs[locationRegionWaypoint._regionID];

                LocationType locationType =
                        _navigationGraph
                        .GetWaypointTypeInRegion
                        (
                         locationRegionWaypoint._regionID,
                         locationRegionWaypoint._waypointID
                        );
                // If the waypoints are portal, we need to get its related 
                // portal waypoints in other regions.
                if (locationType.ToString() == "portal")
                {
                    AddPortalWrongWaypoint(tempRegion,
                                           locationRegionWaypoint,
                                           nextStep,
                                           locationRegionWaypoint._waypointID);
                }
                //Get the waypoint neighbor's Guids and add them in
                //_waypointsOnWrongWay except next Waypoint Guid.
                //We know just consider One-Step Wrong Way.
                foreach (Guid guid in neighborGuid)
                {
                    if (_waypointsOnRoute.Count() > nextStep)
                    {
                        if (_waypointsOnRoute[nextStep]._waypointID != guid)
                        {
                            double distanceBetweenCurrentAndNeighbor =
                                _navigationGraph
                                .StraightDistanceBetweenWaypoints(
                                       locationRegionWaypoint._regionID,
                                       locationRegionWaypoint._waypointID,
                                       guid);

                            double distanceBetweenNextAndNeighbor = 0;

                            //If current region == next region, we can get get 
                            //the straight distance of the neighbors of current 
                            //waypoint and next waypoint.

                            //If current region != next region, we just 
                            //consider the distance between cuurent and its 
                            // neighbers, therefore, we give 
                            // distanceBetweenNextAndNeighbor

                            //the same value of 
                            //distanceBetweenCurrentAndNeighbor.
                            if (locationRegionWaypoint._regionID
                                == _waypointsOnRoute[nextStep]._regionID)
                            {
                                distanceBetweenNextAndNeighbor =
                                    _navigationGraph
                                    .StraightDistanceBetweenWaypoints(
                                       locationRegionWaypoint._regionID,
                                       _waypointsOnRoute[nextStep]._waypointID,
                                       guid);
                            }
                            else
                            {
                                distanceBetweenNextAndNeighbor =
                                    distanceBetweenCurrentAndNeighbor;
                            }
                            //If the distance of current and its neighbors and 
                            //the distance between next and current's neighbors
                            //are far enough, we add them into 
                            //_waypointOnWrongWay, else if the distance between 
                            //current and its neighbors are too close, we need 
                            //to find one more step.
                            if (distanceBetweenCurrentAndNeighbor >=
                                _tooCLoseDistance &&
                                distanceBetweenNextAndNeighbor >=
                                _tooCLoseDistance)
                            {
                                if (nextStep >= 2)
                                {
                                    if (_waypointsOnRoute[nextStep - 2]
                                        ._waypointID != guid)
                                    {
                                        AddWrongWaypoint(guid,
                                                        locationRegionWaypoint
                                                        ._regionID,
                                                        locationRegionWaypoint);
                                    }
                                }
                                else
                                {
                                    AddWrongWaypoint(guid,
                                                    locationRegionWaypoint
                                                    ._regionID,
                                                    locationRegionWaypoint);
                                }

                            }
                            else if (distanceBetweenCurrentAndNeighbor <
                                     _tooCLoseDistance)
                            {
                                OneMoreLayer(guid,
                                            locationRegionWaypoint,
                                            nextStep);
                            }

                            if (nextStep >= 2)
                            {
                                if (_waypointsOnRoute[nextStep - 2]
                                    ._waypointID == guid)
                                {
                                    OneMoreLayer(guid,
                                                 locationRegionWaypoint,
                                                 nextStep);
                                }
                            }
                        }
                        else
                        {
                            if (!_waypointsOnWrongWay.Keys
                                .Contains(locationRegionWaypoint))
                            {
                                _waypointsOnWrongWay
                                .Add(locationRegionWaypoint,
                                     new List<RegionWaypointPoint> { });
                            }
                        }
                    }
                    else
                        break;
                }
                nextStep++;
            }
            #endregion


            #region Generate IPS-table
            List<HashSet<IPSType>> IPSTable =
                new List<HashSet<IPSType>>();

            foreach (RegionWaypointPoint waypoint in _waypointsOnRoute)
            {
                HashSet<IPSType> subtable = new HashSet<IPSType>();

                subtable.Add
                    (_navigationGraph.GetRegionIPSType(waypoint._regionID));

                try
                {
                    foreach (RegionWaypointPoint wrongWaypoint in
                        _waypointsOnWrongWay[waypoint])
                    {
                        subtable.Add(_navigationGraph
                            .GetRegionIPSType(wrongWaypoint._regionID));
                    }
                }
                catch (Exception exc)
                {
                    Console.WriteLine("generate ips table exception : " +
                        exc.Message + ", waypoint id : " +
                        waypoint._waypointID);
                }
                IPSTable.Add(subtable);
            }
            _iPSModules.SetIPStable(IPSTable);

            #endregion



            #region For Calculate progress 
            TmpTotalProgress = _waypointsOnRoute.Count + TmpCurrentProgress;

            #endregion

            #region Display Result
            // display the resulted full path of region/waypoint between 
            // source and destination

            #region display route
            foreach (RegionWaypointPoint checkPoint in _waypointsOnRoute)
            {
                Console.WriteLine("full-path region/waypoint = {0}/{1}",
                                  checkPoint._regionID,
                                  checkPoint._waypointID);
            }
            #endregion
            #region display possible wrong way
            //Print All Possible Wrong Way
            foreach (KeyValuePair<RegionWaypointPoint, List<RegionWaypointPoint>>
                     item in _waypointsOnWrongWay)
            {
                Console.WriteLine("Region ID : " + item.Key._regionID);
                Console.WriteLine("Waypoint ID : " + item.Key._waypointID);
                Console.WriteLine("All possible Wrong : ");
                foreach (RegionWaypointPoint items in item.Value)
                {
                    Console.WriteLine
                        ($"{item.Key._waypointID}'s possible wrong " +
                        "Region Guid : " + items._regionID +
                        ", Waypoint Guid : " + items._waypointID);
                }
                Console.WriteLine("\n");
            }
            #endregion

            #region display used ips type       
            for (int i = 0; i < IPSTable.Count; i++)
            {
                foreach (IPSType type in IPSTable[i])
                {
                    Console.WriteLine($"{i}th path ips type : " + type);
                }
            }
            #endregion

            #endregion
        }

        public void AddPortalWrongWaypoint(Region tempRegion,
                                    RegionWaypointPoint locationRegionWaypoint,
                                    int nextStep,
                                    Guid guid)
        {
            foreach (Guid regionNeighborGuid in tempRegion._neighbors)
            {
                RegionWaypointPoint portalWaypointRegionGuid =                   
                    _navigationGraph.GiveNeighborWaypointInNeighborRegion(
                                locationRegionWaypoint._regionID,
                                guid,
                                regionNeighborGuid);
                if (portalWaypointRegionGuid._waypointID != Guid.Empty)
                {
                    if (_waypointsOnRoute.Count() > nextStep)
                    {
                        if (_waypointsOnRoute[nextStep]._waypointID !=
                            portalWaypointRegionGuid._waypointID)
                        {
                            AddWrongWaypoint(portalWaypointRegionGuid
                                             ._waypointID,
                                             portalWaypointRegionGuid._regionID,
                                             locationRegionWaypoint);
                        }
                        else
                        {
                            if (!_waypointsOnWrongWay.Keys
                                 .Contains(locationRegionWaypoint))
                            {
                                _waypointsOnWrongWay
                                .Add(locationRegionWaypoint,
                                     new List<RegionWaypointPoint> { });
                            }
                        }
                    }
                }
            }
        }

        public void AddWrongWaypoint(Guid waypointID,
                                     Guid regionID,
                                     RegionWaypointPoint locationRegionWaypoint)
        {
            if (!_waypointsOnWrongWay.Keys.Contains(locationRegionWaypoint))
            {                    
                _waypointsOnWrongWay
                .Add(locationRegionWaypoint,
                     new List<RegionWaypointPoint> { 
                         new RegionWaypointPoint(regionID, waypointID)
                        }
                     );
            }
            else
            {                
                _waypointsOnWrongWay[locationRegionWaypoint]
                .Add(new RegionWaypointPoint(regionID, waypointID));
            }
        }

        public void OneMoreLayer(Guid guid,
                                 RegionWaypointPoint locationRegionWaypoint,
                                 int nextStep)
        {
            LocationType currentType =
                _navigationGraph
                .GetWaypointTypeInRegion(locationRegionWaypoint._regionID, guid);
            Region nearWaypointRegion =
                _regiongraphs[locationRegionWaypoint._regionID];

            if (currentType == LocationType.portal)
            {
                AddPortalWrongWaypoint(nearWaypointRegion,
                                       locationRegionWaypoint,
                                       nextStep,
                                       guid);
            }

            List<Guid> nearNonePortalWaypoint = _navigationGraph
                                    .GetNeighbor(locationRegionWaypoint
                                                 ._regionID,
                                                 guid);

            foreach (Guid nearWaypointofSameRegion in nearNonePortalWaypoint)
            {
                if (_waypointsOnRoute.Count() > nextStep)
                {
                    double distanceBetweenCurrentAndNearNeighbor =
                        _navigationGraph.StraightDistanceBetweenWaypoints(
                            locationRegionWaypoint._regionID,
                            locationRegionWaypoint._waypointID,
                            nearWaypointofSameRegion
                        );

                    double distanceBetweenNextAndNearNeighbor = 0;
                    if (locationRegionWaypoint._regionID ==
                        _waypointsOnRoute[nextStep]._regionID)
                    {
                        distanceBetweenNextAndNearNeighbor =
                            _navigationGraph.StraightDistanceBetweenWaypoints(
                                locationRegionWaypoint._regionID,
                                _waypointsOnRoute[nextStep]._waypointID,
                                nearWaypointofSameRegion
                            );
                    }
                    else
                    {
                        distanceBetweenNextAndNearNeighbor =
                            distanceBetweenCurrentAndNearNeighbor;
                    }

                    if (_waypointsOnRoute[nextStep]._waypointID !=
                        nearWaypointofSameRegion &&
                        nearWaypointofSameRegion != guid &&
                        distanceBetweenCurrentAndNearNeighbor >=
                        _tooCLoseDistance &&
                        distanceBetweenNextAndNearNeighbor >= _tooCLoseDistance)
                    {
                        if (nextStep >= 2)
                        {
                            if (_waypointsOnRoute[nextStep - 2]._waypointID
                                != nearWaypointofSameRegion)
                            {
                                AddWrongWaypoint(nearWaypointofSameRegion,
                                                 locationRegionWaypoint
                                                 ._regionID,
                                                 locationRegionWaypoint);
                            }
                        }
                        else
                        {
                            AddWrongWaypoint(nearWaypointofSameRegion,
                                             locationRegionWaypoint._regionID,
                                             locationRegionWaypoint);
                        }
                    }
                    else if (!_waypointsOnWrongWay.Keys
                             .Contains(locationRegionWaypoint))
                    {

                        _waypointsOnWrongWay
                        .Add(locationRegionWaypoint,
                             new List<RegionWaypointPoint> { });

                    }
                }
            }
        }

        #endregion
        //In this function we get the currentwaypoint and determine whether
        //the users are in the right path or not.
        //And we return a structure called navigationInstruction that 
        //contains four elements that Navigation main and UI need.
        //Moreover, if the users are not on the right path, we reroute and 
        //tell users the new path.
        public void CheckArrivedWaypoint(object sender, EventArgs args)
        {
            Console.WriteLine(">> CheckArrivedWaypoint ");
            _currentWaypointID =
                (args as WaypointSignalEventArgs)._detectedRegionWaypoint
                ._waypointID;

            _currentRegionID =
                (args as WaypointSignalEventArgs)._detectedRegionWaypoint
                ._regionID;

            Console.WriteLine("CheckArrived currentWaypoint : "
                                + _currentWaypointID);

            Console.WriteLine("CheckArrived currentRegion : "
                                + _currentRegionID);

            RegionWaypointPoint detectWrongWay = new RegionWaypointPoint(_currentRegionID, _currentWaypointID);

            //NavigationInstruction is a structure that contains five
            //elements that need to be passed to the main and UI
            NavigationInstruction navigationInstruction =
                new NavigationInstruction();

            if (_nextWaypointStep == -1)
            {
                Console.WriteLine("current Waypoint : " + _currentWaypointID);
                _accumulateStraightDistance = 0;

                if (_currentRegionID.Equals(_destinationRegionID) &&
                    _currentWaypointID.Equals(_destinationWaypointID))
                {
                    Console.WriteLine("---- [case: arrived destination] ... ");
                    navigationInstruction._progressBar =
                        string.Format("{0}/{1}", TmpTotalProgress,
                        TmpTotalProgress);

                    _accumulateStraightDistance = 0;
                    _event.OnEventCall(new NavigationEventArgs
                    {
                        _result = NavigationResult.Arrival,
                        _nextInstruction = navigationInstruction
                    });
                }
                _nextWaypointEvent.Set();
            }
            else
            {
                if (_currentRegionID.Equals(_destinationRegionID) &&
                    _currentWaypointID.Equals(_destinationWaypointID) ||
                    _currentRegionID.Equals(_waypointsOnRoute.Last()._regionID)
                    && _currentWaypointID.Equals(_waypointsOnRoute.Last()
                    ._waypointID))
                {

                    navigationInstruction._progressBar =
                        string.Format("{0}/{1}", TmpTotalProgress,
                        TmpTotalProgress);
                    Console.WriteLine
                        ("---- [case: arrived destination] .... ");
                    _accumulateStraightDistance = 0;

                    _event.OnEventCall(new NavigationEventArgs
                    {
                        _result = NavigationResult.Arrival,
                        _nextInstruction = navigationInstruction
                    });
                }
                else if (_currentRegionID.Equals(
                             _waypointsOnRoute[_nextWaypointStep]._regionID) &&
                         _currentWaypointID.Equals(
                             _waypointsOnRoute[_nextWaypointStep]._waypointID))
                {
                    Console.WriteLine("---- [case: arrived waypoint] .... ");

                    Console.WriteLine("current region/waypoint: {0}/{1}",
                                      _currentRegionID,
                                      _currentWaypointID);

                    navigationInstruction._currentWaypointName =
                        _navigationGraph
                        .GetWaypointNameInRegion(_currentRegionID,
                                                 _currentWaypointID);
                    navigationInstruction._nextWaypointName =
                        _navigationGraph.GetWaypointNameInRegion(
                            _waypointsOnRoute[_nextWaypointStep + 1]._regionID,
                            _waypointsOnRoute[_nextWaypointStep + 1]
                            ._waypointID);

                    Guid previousRegionID = new Guid();
                    Guid previousWaypointID = new Guid();
                    if (_nextWaypointStep - 1 >= 0)
                    {
                        previousRegionID =
                            _waypointsOnRoute[_nextWaypointStep - 1]._regionID;
                        previousWaypointID =
                            _waypointsOnRoute[_nextWaypointStep - 1]
                            ._waypointID;
                    }

                    try
                    {
                        navigationInstruction._information = _navigationGraph
                            .GetInstructionInformation(
                                    _nextWaypointStep,
                                    previousRegionID,
                                    previousWaypointID,
                                    _currentRegionID,
                                    _currentWaypointID,
                                    _waypointsOnRoute[_nextWaypointStep + 1]
                                    ._regionID,
                                    _waypointsOnRoute[_nextWaypointStep + 1]
                                    ._waypointID,
                                    _waypointsOnRoute[_nextWaypointStep + 2]
                                    ._regionID,
                                    _waypointsOnRoute[_nextWaypointStep + 2]
                                    ._waypointID,
                                    _avoidConnectionTypes
                                    );
                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine("GetInstructionError - "
                            + exc.Message);
                        navigationInstruction._information = _navigationGraph
                            .GetInstructionInformation
                            (
                                _nextWaypointStep,
                                previousRegionID, previousWaypointID,
                                _currentRegionID, _currentWaypointID,
                            _waypointsOnRoute[_nextWaypointStep + 1]._regionID,
                          _waypointsOnRoute[_nextWaypointStep + 1]._waypointID,
                                new Guid(),
                                new Guid(),
                                _avoidConnectionTypes
                            );
                    }
                    navigationInstruction._currentWaypointGuid =
                        _currentWaypointID;

                    navigationInstruction._nextWaypointGuid =
                        _waypointsOnRoute[_nextWaypointStep + 1]._waypointID;

                    navigationInstruction._currentRegionGuid =
                        _currentRegionID;

                    navigationInstruction._nextRegionGuid =
                        _waypointsOnRoute[_nextWaypointStep + 1]._regionID;

                    if (navigationInstruction._information._nextDirection ==
                        TurnDirection.FirstDirection)
                    {
                        navigationInstruction._turnDirectionDistance =
                            _navigationGraph.GetDistanceOfLongHallway(
                                _nextWaypointStep + 2, _waypointsOnRoute,
                                _avoidConnectionTypes);
                    }
                    else
                    {
                        navigationInstruction._turnDirectionDistance =
                            _navigationGraph.GetDistanceOfLongHallway(
                                _nextWaypointStep + 1,
                                _waypointsOnRoute,
                                _avoidConnectionTypes);
                    }
                    Console.WriteLine("navigation_turn : "
                                + navigationInstruction
                                ._turnDirectionDistance);

                    //Get the progress
                    Console.WriteLine("calculate progress: {0}/{1}",
                                      _nextWaypointStep,
                                      _waypointsOnRoute.Count);

                    navigationInstruction._progress =
                        GetPercentage(++TmpCurrentProgress, TmpTotalProgress);

                    navigationInstruction._progressBar =
                        string.Format("{0}/{1}", TmpCurrentProgress,
                        TmpTotalProgress);

                    navigationInstruction._previousRegionGuid =
                        previousRegionID;

                    // Raise event to notify the UI/main thread with the result                   
                    if (navigationInstruction._information._connectionType ==
                        ConnectionType.VirtualHallway ||
                        navigationInstruction._information._isVirtualWay ==
                        true)
                    {
                        _accumulateStraightDistance = 0;
                        navigationInstruction._progressBar =
                            string.Format("{0}/{1}", TmpTotalProgress,
                            TmpTotalProgress);

                        _event.OnEventCall(new NavigationEventArgs
                        {
                            _result = NavigationResult.ArriveVirtualPoint,
                            _nextInstruction = navigationInstruction
                        });
                    }
                    else
                    {
                        if (navigationInstruction._information._turnDirection
                            == TurnDirection.Forward &&
                            _nextWaypointStep != -1 &&
                            _accumulateStraightDistance >= _remindDistance)
                        {
                            _accumulateStraightDistance +=
                                navigationInstruction._information._distance;

                            _event.OnEventCall(new NavigationEventArgs
                            {
                                _result = NavigationResult.ArrivaIgnorePoint,
                                _nextInstruction = navigationInstruction
                            });

                        }
                        else
                        {

                            _accumulateStraightDistance =
                                navigationInstruction._information._distance;
                            _event.OnEventCall(new NavigationEventArgs
                            {
                                _result = NavigationResult.Run,
                                _nextInstruction = navigationInstruction
                            });
                        }

                    }
                }
                else if (_nextWaypointStep + 1 < _waypointsOnRoute.Count())
                {

                    if (_currentRegionID.Equals
                        (_waypointsOnRoute[_nextWaypointStep + 1]._regionID)
                        && _currentWaypointID.Equals
                        (_waypointsOnRoute[_nextWaypointStep + 1]._waypointID))
                    {
                        _nextWaypointStep++;
                        navigationInstruction._currentWaypointName =
                       _navigationGraph.GetWaypointNameInRegion
                       (_currentRegionID,
                       _currentWaypointID);

                        navigationInstruction._nextWaypointName =
                            _navigationGraph.GetWaypointNameInRegion(
                                _waypointsOnRoute[_nextWaypointStep + 1]
                                ._regionID,
                                _waypointsOnRoute[_nextWaypointStep + 1]
                                ._waypointID);

                        Guid previousRegionID = new Guid();
                        Guid previousWaypointID = new Guid();
                        if (_nextWaypointStep - 1 >= 0)
                        {
                            previousRegionID =
                                _waypointsOnRoute[_nextWaypointStep - 1]
                                ._regionID;
                            previousWaypointID =
                                _waypointsOnRoute[_nextWaypointStep - 1]
                                ._waypointID;
                        }

                        try
                        {
                            navigationInstruction._information =
                                _navigationGraph
                                .GetInstructionInformation(
                                        _nextWaypointStep,
                                        previousRegionID,
                                        previousWaypointID,
                                        _currentRegionID,
                                        _currentWaypointID,
                                        _waypointsOnRoute
                                        [_nextWaypointStep + 1]._regionID,
                                        _waypointsOnRoute
                                        [_nextWaypointStep + 1]._waypointID,
                                        _waypointsOnRoute
                                        [_nextWaypointStep + 2]._regionID,
                                        _waypointsOnRoute
                                        [_nextWaypointStep + 2]._waypointID,
                                        _avoidConnectionTypes
                                        );
                        }
                        catch (Exception exc)
                        {
                            Console.WriteLine("GetInstructionError - "
                                + exc.Message);
                            navigationInstruction._information =
                                _navigationGraph
                                .GetInstructionInformation
                                (
                                    _nextWaypointStep,
                                    previousRegionID, previousWaypointID,
                                    _currentRegionID, _currentWaypointID,
                                    _waypointsOnRoute[_nextWaypointStep + 1]
                                    ._regionID,
                                    _waypointsOnRoute[_nextWaypointStep + 1]
                                    ._waypointID,
                                    new Guid(),
                                    new Guid(),
                                    _avoidConnectionTypes
                                );
                        }

                        navigationInstruction._currentWaypointGuid =
                            _currentWaypointID;

                        navigationInstruction._nextWaypointGuid =
                            _waypointsOnRoute[_nextWaypointStep + 1]
                            ._waypointID;

                        navigationInstruction._currentRegionGuid =
                            _currentRegionID;

                        navigationInstruction._nextRegionGuid =
                            _waypointsOnRoute[_nextWaypointStep + 1]
                            ._regionID;

                        if (navigationInstruction._information._nextDirection
                            == TurnDirection.FirstDirection)
                        {
                            navigationInstruction._turnDirectionDistance =
                                _navigationGraph.GetDistanceOfLongHallway(
                                    _nextWaypointStep + 2, _waypointsOnRoute,
                                    _avoidConnectionTypes);
                        }
                        else
                        {
                            navigationInstruction._turnDirectionDistance =
                                _navigationGraph.GetDistanceOfLongHallway(
                                    _nextWaypointStep + 1,
                                    _waypointsOnRoute,
                                    _avoidConnectionTypes);
                        }

                        TmpCurrentProgress += 2;
                        navigationInstruction._progress =
                            GetPercentage(TmpCurrentProgress,
                            TmpTotalProgress);

                        navigationInstruction._progressBar =
                            string.Format("{0}/{1}", TmpCurrentProgress,
                            TmpTotalProgress);

                        navigationInstruction._previousRegionGuid =
                            previousRegionID;

                        if (navigationInstruction._information._connectionType
                            == ConnectionType.VirtualHallway ||
                            navigationInstruction._information._isVirtualWay
                            == true)
                        {
                            _accumulateStraightDistance = 0;
                            navigationInstruction._progressBar =
                                string.Format("{0}/{1}",
                                TmpTotalProgress, TmpTotalProgress);

                            _event.OnEventCall(new NavigationEventArgs
                            {
                                _result = NavigationResult.ArriveVirtualPoint,
                                _nextInstruction = navigationInstruction
                            });
                        }
                        else
                        {
                            if (navigationInstruction._information
                                ._turnDirection == TurnDirection.Forward &&
                                _nextWaypointStep != -1 &&
                                _accumulateStraightDistance >= _remindDistance)
                            {
                                _accumulateStraightDistance =
                                    _accumulateStraightDistance +
                                    navigationInstruction._information._distance;
                                _event.OnEventCall(new NavigationEventArgs
                                {
                                    _result = NavigationResult.ArrivaIgnorePoint,
                                    _nextInstruction = navigationInstruction
                                });
                            }
                            else
                            {
                                _accumulateStraightDistance = 0;
                                _accumulateStraightDistance =
                                    _accumulateStraightDistance +
                                    navigationInstruction._information
                                    ._distance;
                                _event.OnEventCall(new NavigationEventArgs
                                {
                                    _result = NavigationResult.Run,
                                    _nextInstruction = navigationInstruction
                                });
                            }
                        }

                    }
                    else if (_nextWaypointStep >= 1 &&
                             _waypointsOnWrongWay
                             [_waypointsOnRoute[_nextWaypointStep - 1]]
                             .Contains(detectWrongWay) == true &&
                             _DetectWrongWaypoint)
                    {
                        HandleWrongWay();
                    }

                    Console.WriteLine("<< In next next");
                }
                else if (_nextWaypointStep >= 1 &&
                         _waypointsOnWrongWay
                         [_waypointsOnRoute[_nextWaypointStep - 1]].
                         Contains(detectWrongWay) == true &&
                         _DetectWrongWaypoint)
                {
                    HandleWrongWay();
                }

                _nextWaypointEvent.Set();
            }

            Console.WriteLine("<< CheckArrivedWaypoint ");
        }

        private double GetPercentage(int current, int total)
        {
            return (double)Math.Round(100 * ((decimal)current /
                               (total)), 3);
        }

        public void HandleWrongWay()
        {
            _accumulateStraightDistance = 0;
            _DetectWrongWaypoint = false;
            Console.WriteLine("---- [case: wrong waypoint] .... ");
        }

        public void PauseSession()
        {
            Console.WriteLine(">>Pause Session");
            try
            {
                _pauseThreadEvent.Reset();
                return;
            }
            catch (Exception exc)
            {
                Console.WriteLine("_pauseThreadEvent error - " +
                    exc.Message);
                return;
            }
        }

        public void ResumeSession()
        {
            Console.WriteLine(">>ResumeSession");
            try
            {
                _pauseThreadEvent.Set();
                _nextWaypointEvent.Reset();
                return;
            }
            catch (Exception exc)
            {
                Console.WriteLine("ResuemSession error - " +
                    exc.Message);
                return;
            }
        }

        public void CloseSession()
        {
            try
            {
                _nextWaypointEvent.Dispose();
                _pauseThreadEvent.Dispose();
            }
            catch (Exception exc)
            {
                Console.WriteLine("CloseSession error - " +
                    exc.Message);
            }

            _isKeepDetection = false;
            _nextWaypointStep = -1;
            //_iPSModules.CloseAllActiveClient();

            _waypointDetectionThread.Abort();
            _navigationControllerThread.Abort();
            _waypointsOnWrongWay.Clear();
            _waypointsOnRoute.Clear();
            _iPSModules._event._eventHandler -=
                new EventHandler(CheckArrivedWaypoint);
            _iPSModules.Dispose();
        }

        #region define class and enum
        public enum NavigationResult
        {
            Run = 0,
            AdjustRoute,
            Arrival,
            NoRoute,
            ArriveVirtualPoint,
            ArrivaIgnorePoint
        }

        public class NavigationInstruction
        {
            public string _currentWaypointName;

            public string _nextWaypointName;

            public double _progress;

            public string _progressBar;
            public InstructionInformation _information { get; set; }

            public Guid _currentWaypointGuid;

            public Guid _nextWaypointGuid;

            public Guid _currentRegionGuid;

            public Guid _nextRegionGuid;

            public Guid _previousRegionGuid;

            public int _turnDirectionDistance;
        }
        public class NavigationEventArgs : EventArgs
        {
            public NavigationResult _result { get; set; }
            public NavigationInstruction _nextInstruction { get; set; }

        }
        #endregion
    }

}
