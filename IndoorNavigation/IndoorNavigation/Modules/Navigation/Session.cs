/*
 * Copyright (c) 2019 Academia Sinica, Institude of Information Science
 *
 * License:
 *      GPL 3.0 : The content of this file is subject to the terms and
 *      conditions defined in file 'COPYING.txt', which is part of this source
 *      code package.
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
 *    Calculate the best route and use waypoint and region to connect to a route
 *    , and add the intersted waypoints that include next waypoint, next next 
 *	  waypoint and wrongwaypoints.
 *    When we get the matched waypoint and region, we will check if it is correct
 *    waypoint or wrong waypoint.
 *   
 *
 * Authors:
 *
 *      Eric Lee, ericlee@iis.sinica.edu.tw
 *      
 *
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
using Xamarin.Forms.Internals;
using IndoorNavigation.ViewModels;
using Xamarin.Forms;
using Region = IndoorNavigation.Models.Region;


namespace IndoorNavigation.Modules
{
    public class Session
    {
        #region Variables
        private int _nextWaypointStep;

        private List<RegionWaypointPoint> _waypointsOnRoute = 
			new List<RegionWaypointPoint>();

        private Dictionary<RegionWaypointPoint, List<RegionWaypointPoint>>
            _waypointsOnWrongWay = 
				new Dictionary<RegionWaypointPoint,List<RegionWaypointPoint>>();

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
        private Guid _currentRegionID = new Guid();
        private Guid _currentWaypointID = new Guid();

        private ConnectionType[] _avoidConnectionTypes;

        private ManualResetEventSlim _nextWaypointEvent = 
			new ManualResetEventSlim(false);

        private ManualResetEventSlim _wrongWaypointEvent =
            new ManualResetEventSlim(false);

        private ManualResetEventSlim _OnStopThreadEvent =
            new ManualResetEventSlim(true);
        public NavigationEvent _event { get; private set; }
        private Dictionary<Guid, Region> _regiongraphs = 
			new Dictionary<Guid, Region>();
			
        private IPSModules _iPSModules;
        private const int _tooCLoseDistance = 7;



        private const int _tooCloseDistanceForPath = 7;
        #region fix wrong waypoint detected
        private bool _DetectedWrongWaypoint = true;
        private List<RegionWaypointPoint> ignoreWaypoint = 
            new List<RegionWaypointPoint>();
        #endregion 

        #endregion

        public Session(NavigationGraph navigationGraph,
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
            _iPSModules = new IPSModules(_navigationGraph);
            _iPSModules._event._eventHandler += 
				new EventHandler(CheckArrivedWaypoint);

            _waypointDetectionThread = new Thread(() => InvokeIPSWork());
            _waypointDetectionThread.Start();

            _navigationControllerThread = new Thread(() => NavigatorProgram());
            _navigationControllerThread.Start();

            #region Test String
            //Guid sourceWaypoint = new Guid("00000000-0000-0000-0000-000000000328");
            //Guid sourceRegion = new Guid("22222222-2222-2222-2222-222222222222");

            //Console.WriteLine("Fist Try in Generate Path");
            //GenerateRoute(sourceRegion, sourceWaypoint, destinationRegionID, destinationWaypointID);

            //sourceRegion = new Guid("11111111-1111-1111-1111-111111111111");
            //sourceWaypoint = new Guid("00000000-0000-0000-0000-000000000021");

            //Console.WriteLine("Second Try in Generate Path");
            //GenerateRoute(sourceRegion, sourceWaypoint, destinationRegionID, destinationWaypointID);
            #endregion
        }

        private void NavigatorProgram()
        {
            _nextWaypointStep = -1;
            _currentRegionID = new Guid();
            _currentWaypointID = new Guid();
            RegionWaypointPoint checkWrongRegionWaypoint = 
				new RegionWaypointPoint();            

            NavigateToNextWaypoint(_currentRegionID, _nextWaypointStep);

            while (true == _isKeepDetection &&
                   !(_currentRegionID.Equals(_destinationRegionID) &&
                     _currentWaypointID.Equals(_destinationWaypointID)))
            {
                _OnStopThreadEvent.Wait();
                Console.WriteLine("Continue to navigate to next step, current"+
								"location {0}/{1}",
                                  _currentRegionID, _currentWaypointID);

                checkWrongRegionWaypoint._regionID = _currentRegionID;
                checkWrongRegionWaypoint._waypointID = _currentWaypointID;

                _nextWaypointEvent.Wait();
                if (_currentRegionID.Equals(_destinationRegionID) &&
                    _currentWaypointID.Equals(_destinationWaypointID))
                {
                    Console.WriteLine("Arrived destination! {0}/{1}",
                                      _destinationRegionID,
                                      _destinationWaypointID);

                    _isKeepDetection = false;
                    _iPSModules.Close();
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
                    Guid _nextRegionID = 
						_waypointsOnRoute[_nextWaypointStep]._regionID;
						
                    NavigateToNextWaypoint(_nextRegionID, _nextWaypointStep);
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
                    Guid _nextRegionID = 
						_waypointsOnRoute[_nextWaypointStep]._regionID;

                    NavigateToNextWaypoint(_nextRegionID, _nextWaypointStep);
                }
                else if (_nextWaypointStep >= 1 && 
					_waypointsOnWrongWay
					[_waypointsOnRoute[_nextWaypointStep - 1]]
					.Contains(checkWrongRegionWaypoint) == true && _DetectedWrongWaypoint==false)
                {

                    Console.WriteLine("In Program Wrong, going to Re-calculate"+
									  "the route");
                    _nextWaypointStep = 0;

                    GenerateRoute(
                                    _currentRegionID,
                                    _currentWaypointID,
                                    _destinationRegionID,
                                    _destinationWaypointID);

                    Console.WriteLine("Finish Construct New Route");
                    _DetectedWrongWaypoint = true;
                    Guid previousRegionID = new Guid();
                    Guid previousWaypointID = new Guid();

                    // Add this function can avoid that when users go to the 
					// worong waypoint, the instuction will jump to fast.

                    //SpinWait.SpinUntil(() => false, 5000);
                    RegionWaypointPoint regionWaypointPoint = 
						new RegionWaypointPoint();
                    regionWaypointPoint._regionID = _currentRegionID;
                    regionWaypointPoint._waypointID = _currentWaypointID;
                    int tempRoute = 0;
                    tempRoute = _waypointsOnRoute.Count() - 1;
                    _accumulateStraightDistance = 0;

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

                    navigationInstruction._progress = 0;
                    navigationInstruction._progressBar = "0 / " + tempRoute;
                    navigationInstruction._information = _navigationGraph
						.GetInstructionInformation(
                                _nextWaypointStep,
                                _currentRegionID,
                                _currentWaypointID,
                                previousRegionID,
                                previousWaypointID,
                                _waypointsOnRoute[_nextWaypointStep + 1]._regionID,
                                _waypointsOnRoute[_nextWaypointStep + 1]._waypointID,
                                _avoidConnectionTypes
                                );
								
                    navigationInstruction._currentWaypointGuid = 
						_currentWaypointID;
						
                    navigationInstruction._nextWaypointGuid = 
						_waypointsOnRoute[_nextWaypointStep + 1]._waypointID;
						
                    navigationInstruction._currentRegionGuid = 
						_currentRegionID;
						
                    navigationInstruction._nextRegionGuid = 
						_waypointsOnRoute[_nextWaypointStep + 1]._regionID;
						
                    navigationInstruction._turnDirectionDistance = 
						_navigationGraph
						.GetDistanceOfLongHallway(regionWaypointPoint, 
												  1, 
												  _waypointsOnRoute, 
												  _avoidConnectionTypes);
												  
                    _event.OnEventCall(new NavigationEventArgs
                    {

                        _result = NavigationResult.Run,
                        _nextInstruction = navigationInstruction

                    });
                    _accumulateStraightDistance = 
						_accumulateStraightDistance + 
						navigationInstruction._information._distance;
                    _nextWaypointStep++;
                    Guid _nextRegionID = 
						_waypointsOnRoute[_nextWaypointStep]._regionID;
                    NavigateToNextWaypoint(_nextRegionID, _nextWaypointStep);
                }
                _nextWaypointEvent.Reset();
            }
        }

        private void NavigateToNextWaypoint(Guid regionID, int nextStep)
        {
            List<WaypointBeaconsMapping> monitorWaypointList =
                new List<WaypointBeaconsMapping>();
				
            List<WaypointBeaconsMapping> monitorWaypointListInWaypointClient =
                new List<WaypointBeaconsMapping>();
				
            List<WaypointBeaconsMapping> monitorWaypointListInIBeaconClient =
                new List<WaypointBeaconsMapping>();
				
            if (nextStep == -1)
            {

                List<Guid> allRegionIDs = new List<Guid>();
                allRegionIDs = _navigationGraph.GetAllRegionIDs();

                _iPSModules.AtStarting_ReadALLIPSType(allRegionIDs);
            }
            else
            {
                Console.WriteLine("NavigateProgram");
                RegionWaypointPoint checkPoint = _waypointsOnRoute[nextStep];

                _iPSModules.CompareToCurrentAndNextIPSType(_currentRegionID, 
														   checkPoint._regionID, 
														   _nextWaypointStep);
                _iPSModules.AddNextWaypointInterestedGuid(checkPoint._regionID, 
														checkPoint._waypointID);


                if (_nextWaypointStep + 1 < _waypointsOnRoute.Count())
                {

                    if (_waypointsOnRoute[_nextWaypointStep + 1]._regionID == 
						_currentRegionID)
                    {
                        _iPSModules
						.AddNextNextWaypointInterestedGuid
						(_waypointsOnRoute[_nextWaypointStep + 1]._regionID, 
						 _waypointsOnRoute[_nextWaypointStep + 1]._waypointID);
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

                            _iPSModules
							.AddWrongWaypointInterestedGuid(items._regionID, 
															items._waypointID);
                        }
                    }
                }
                _iPSModules.SetMonitorBeaconList();
            }

        }

        private void InvokeIPSWork()
        {
            Console.WriteLine("---- InvokeIPSWork ----");
            while (true == _isKeepDetection)
            {
                _OnStopThreadEvent.Wait();
                //Thread.Sleep(500);
                SpinWait.SpinUntil(() => false, 500);
                _iPSModules.OpenBeconScanning();


            }
        }

        #region GeneratePath Methods
        private void GenerateRoute(Guid sourceRegionID,
                                   Guid sourceWaypointID,
                                   Guid destinationRegionID,
                                   Guid destinationWaypointID)
        {
            #region Generate route

            #region To decide region edge
            //Get Source Region ID index in All Graph Data Structure.
            uint region1Key = _graphRegionGraph
                              .Where(node => node.Item.Equals(sourceRegionID))
                              .Select(node => node.Key).First();

            //Get Destination Region ID index in All Graph Data Structure.
            uint region2Key = _graphRegionGraph
                              .Where(node=>node.Item.Equals(destinationRegionID))
                              .Select(node => node.Key).First();        

            //Use two index in Graph data structure to check whether it have path or not.
            var pathRegions = _graphRegionGraph
							 .Dijkstra(region1Key, region2Key)
							 .GetPath();

            //If not route between two region, return.
            if (0 == pathRegions.Count())
            {
                Console.WriteLine("No path.Need to change avoid connection"+
								  " type");
                _event.OnEventCall(new NavigationEventArgs
                {
                    _result = NavigationResult.NoRoute
                });
                return;
            }

            // to store region id that generate by Dijstkra cross the route
            List<Guid> regionsOnRoute = new List<Guid>();
            for (int i = 0; i < pathRegions.Count(); i++)
            {
                regionsOnRoute.Add(_graphRegionGraph[pathRegions.ToList()[i]]
								   .Item);
            }

            // generate the path of the region/waypoint checkpoints across 
			// regions
            _waypointsOnRoute = new List<RegionWaypointPoint>();

            //to add source waypoint to route.
            _waypointsOnRoute.Add(new RegionWaypointPoint
            {
                _regionID = sourceRegionID,
                _waypointID = sourceWaypointID
            });

            for (int i = 0; i < _waypointsOnRoute.Count(); i++)
            {
                RegionWaypointPoint checkPoint = _waypointsOnRoute[i];
                Console.WriteLine("check index = {0}, count = {1}, region {2}"+
								  " waypoint {3}",
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
                            //Add the waypoint that type is portal at current 
                            // region
                            _waypointsOnRoute.Add(new RegionWaypointPoint
                            {
                                _regionID = checkPoint._regionID,
                                _waypointID = portalWaypoints._portalWaypoint1
                            });
                        }
                        else
                        {
                            //Add the waypoint at the other region
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
                _destinationWaypointID.
                Equals(_waypointsOnRoute[indexLastCheckPoint]._waypointID)))
            {
                _waypointsOnRoute.Add(new RegionWaypointPoint
                {
                    _regionID = _destinationRegionID,
                    _waypointID = _destinationWaypointID
                });
            }

            foreach (RegionWaypointPoint checkPoint in _waypointsOnRoute)
            {
                Console.WriteLine("region-graph region/waypoint = {0}/{1}",
                                  checkPoint._regionID,
                                  checkPoint._waypointID);
            }
            #endregion

            #region To fill the path besides portal point.
            // fill in all the path between waypoints in the
            // same region / navigraph
            for (int i = 0; i < _waypointsOnRoute.Count() - 1; i++)
            {
                RegionWaypointPoint currentCheckPoint = _waypointsOnRoute[i];
                RegionWaypointPoint nextCheckPoint = _waypointsOnRoute[i + 1];
                              
                Console.WriteLine($"currentCheckPoint = {currentCheckPoint._waypointID}");
                Console.WriteLine($"nextCheckPoint = {nextCheckPoint._waypointID}");
                if (currentCheckPoint._regionID.Equals(nextCheckPoint._regionID))
                {                   
                    Graph<Guid, string> _graphNavigraph =
                        _navigationGraph
						.GenerateNavigraph(currentCheckPoint._regionID,
                                           _avoidConnectionTypes);

                    //to get source waypoint index in _graphNavigraph
                    uint waypoint1Key = _graphNavigraph
                                        .Where(node => node.Item
                                        .Equals(currentCheckPoint._waypointID))
                                        .Select(node => node.Key).First();

                    //to get destination waypoint index in _graphNavigraph
                    uint waypoint2Key = _graphNavigraph
                                        .Where(node => node.Item
                                        .Equals(nextCheckPoint._waypointID))
                                        .Select(node => node.Key).First();
                    //to get the path between these two waypoint
                    var pathWaypoints =
                        _graphNavigraph.Dijkstra(waypoint1Key, waypoint2Key)
									   .GetPath();

                    RegionWaypointPoint LastAddToWaypointRoute=new RegionWaypointPoint();

                    //to fill the waypoint that get from dijkstra to all route.
                    for (int j = pathWaypoints.Count() - 1; j > 0; j--)
                    {

                        if (j != 0 && j != pathWaypoints.Count() - 1)
                        {
                            
                            bool isStraight =
                                _navigationGraph.isStraightBetweenWaypoint
                                (
                                    currentCheckPoint._regionID,
                                    _graphNavigraph[pathWaypoints.ToList()[j + 1]].Item,
                                    _graphNavigraph[pathWaypoints.ToList()[j]].Item,
                                    _graphNavigraph[pathWaypoints.ToList()[j - 1]].Item
                                );


                            double distance =
                              _navigationGraph.StraightDistanceBetweenWaypoints
                              (
                                  currentCheckPoint._regionID,
                                  LastAddToWaypointRoute._waypointID.Equals(Guid.Empty)
                                  ? _graphNavigraph[pathWaypoints.ToList()[j + 1]].Item
                                  : LastAddToWaypointRoute._waypointID,
                                      //_graphNavigraph[pathWaypoints.ToList()[j + 1]].Item,
                                  _graphNavigraph[pathWaypoints.ToList()[j]].Item
                               ) ;                            
                            
                            Console.WriteLine("Source waypoint ID = " + _graphNavigraph[pathWaypoints.ToList()[j+1]].Item);
                            Console.WriteLine("Destination waypoint ID = " + _graphNavigraph[pathWaypoints.ToList()[j]].Item);
                            Console.WriteLine("isStraight = " + isStraight + "aaaaa");
                            Console.WriteLine("Distance  ==  " + distance + "bbbb");
                            
                            if ((!isStraight || !(distance < _tooCloseDistanceForPath)))
                            {
                                Console.WriteLine("Add to waypointsOnRoute : " + _graphNavigraph[pathWaypoints.ToList()[j]].Item);
                                LastAddToWaypointRoute = 
                                    new RegionWaypointPoint
                                    (currentCheckPoint._regionID, 
                                    _graphNavigraph[pathWaypoints.ToList()[j]]
                                   .Item);
                                _waypointsOnRoute.Insert(i + 1,
                                new RegionWaypointPoint
                                {
                                    _regionID = currentCheckPoint._regionID,
                                    _waypointID =
                                   _graphNavigraph[pathWaypoints.ToList()[j]]
                                   .Item
                                }
                                );
                            }
                            else
                            {
                                Console.WriteLine("Add to ignore list : " + _graphNavigraph[pathWaypoints.ToList()[j]].Item);
                                ignoreWaypoint.Add(new RegionWaypointPoint
                                {
                                    _regionID = currentCheckPoint._regionID,
                                    _waypointID= 
                                      _graphNavigraph[pathWaypoints.ToList()[j]]
                                      .Item
                                });
                            }
                        }
                    }
                }
            }
            #endregion

            #endregion

            #region Wrong Point decide
            int nextStep = 1;
            _waypointsOnWrongWay = 
				new Dictionary<RegionWaypointPoint, List<RegionWaypointPoint>>();
            List<Guid> neighborGuids = new List<Guid>();

            //For each waypoint in _waypointsOnRoute, decide their wrong 
			//waypoint.
            foreach (RegionWaypointPoint checkWaypoint 
					 in _waypointsOnRoute)
            {
                Console.WriteLine("Important Current Waypoint : " 
								  + checkWaypoint._waypointID);
                // Get the all neighbor wapoints of checkpoint of 
                // _waypointOnRoute.
                neighborGuids = _navigationGraph
							   .GetNeighbor(checkWaypoint._regionID, 
											checkWaypoint._waypointID);                

                LocationType locationType =
                        _navigationGraph.GetWaypointTypeInRegion
						(
						    checkWaypoint._regionID,
					        checkWaypoint._waypointID
						);
                // If the waypoints are portal, we need to get its related portal
				// waypoints in other regions.
                if (locationType== LocationType.portal)
                {
                    Region currentRegion = 
                        _regiongraphs[checkWaypoint._regionID];

                    AddPortalWrongWaypoint(currentRegion, 
										   checkWaypoint, 
										   nextStep, 
										   checkWaypoint._waypointID);
                }
                //Get the waypoint neighbor's Guids and add them in
				//_waypointsOnWrongWay except next Waypoint Guid.
                //We know just consider One-Step Wrong Way.
                foreach (Guid neighborGuid in neighborGuids)
                {
                    if (_waypointsOnRoute.Count() > nextStep)
                    {
                        RegionWaypointPoint CheckIgnorePoint =
                                   new RegionWaypointPoint
                                   (checkWaypoint._regionID, neighborGuid);
                        //Console.WriteLine(" current neighbor id : " + neighborGuid);
                        if (_waypointsOnRoute[nextStep]._waypointID != neighborGuid)
                        {
                            double distanceBetweenCurrentAndNeighbor =
                                _navigationGraph
                                .StraightDistanceBetweenWaypoints(
                                       checkWaypoint._regionID,
                                       checkWaypoint._waypointID,
                                       neighborGuid);

                            double distanceBetweenNextAndNeighbor = 0;

                            //If current region == next region, we can get get 
                            //the straight distance of the neighbors of current 
                            //waypoint and next waypoint.

                            //If current region != next region, we just consider
                            //the distance between current and its neighbers, 
                            //therefore, we give distanceBetweenNextAndNeighbor

                            //the same value of distanceBetweenCurrentAndNeighbor.
                            if (checkWaypoint._regionID
                                == _waypointsOnRoute[nextStep]._regionID)
                            {
                                distanceBetweenNextAndNeighbor =
                                    _navigationGraph
                                    .StraightDistanceBetweenWaypoints(
                                       checkWaypoint._regionID,
                                       _waypointsOnRoute[nextStep]._waypointID,
                                       neighborGuid);
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
                                if (!ignoreWaypoint.Contains(CheckIgnorePoint))
                                {
                                    Console.WriteLine("not contains ingore point");
                                    AddWrongWaypoint(neighborGuid,
                                        checkWaypoint._regionID,
                                        checkWaypoint);
                                }
                                else
                                {
                                    Console.WriteLine("When there contains ignore point");
                                    ignoreWaypoint.Remove(CheckIgnorePoint);

                                    AddWrongWaypoint(neighborGuid, 
                                        checkWaypoint._regionID, 
                                        _waypointsOnRoute[nextStep]);

                                    //ignoreWaypoint.Remove
                                    //    (CheckIgnorePoint);
                                    //OneMoreLayer(neighborGuid,
                                    //    checkWaypoint, nextStep - 1);
                                }
                                #region I have no idea why write this code. Maybe there is something special
                                //if (nextStep >= 2)
                                //{
                                //    if (_waypointsOnRoute[nextStep - 2]
                                //        ._waypointID != neighborGuid)
                                //    {
                                //        AddWrongWaypoint(neighborGuid,
                                //                        checkWaypoint
                                //                        ._regionID,
                                //                        checkWaypoint);
                                //    }
                                //}
                                //else
                                //{
                                //    AddWrongWaypoint(neighborGuid,
                                //                    checkWaypoint
                                //                    ._regionID,
                                //                    checkWaypoint);
                                //}
                                #endregion
                            }
                            else if (distanceBetweenCurrentAndNeighbor <
                                     _tooCLoseDistance)
                            {
                                if (!ignoreWaypoint.Contains(CheckIgnorePoint))
                                {
                                    OneMoreLayer(neighborGuid,
                                              checkWaypoint,
                                              nextStep);
                                }
                                else
                                {
                                    Console.WriteLine("low distance and don't contain ignore point");
                                    Console.WriteLine("ignore point : " + CheckIgnorePoint._waypointID);
                                    Console.WriteLine("next step in ignore : " + nextStep);
                                    ignoreWaypoint.Remove(CheckIgnorePoint);

                                    OneMoreLayer(neighborGuid, checkWaypoint, nextStep-1);
                                }
                            }

                            if (nextStep >= 2)
                            {
                                if (_waypointsOnRoute[nextStep - 2]
                                    ._waypointID == neighborGuid)
                                {
                                    OneMoreLayer(neighborGuid,
                                                 checkWaypoint,
                                                 nextStep);
                                }
                            }
                        }
                        else
                        {
                            if (!_waypointsOnWrongWay.Keys
                                .Contains(checkWaypoint))
                            {
                                _waypointsOnWrongWay
                                .Add(checkWaypoint,
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

            #region Display Result
            // display the resulted full path of region/waypoint between 
            // source and destination
            foreach (RegionWaypointPoint checkPoint in _waypointsOnRoute)
            {
                Console.WriteLine("full-path region/waypoint = {0}/{1}",
                                  checkPoint._regionID,
                                  checkPoint._waypointID);
            }

            //Print All Possible Wrong Way
            foreach (KeyValuePair<RegionWaypointPoint,List<RegionWaypointPoint>> 
					 item in _waypointsOnWrongWay)
            {
                Console.WriteLine("Region ID : " + item.Key._regionID);
                Console.WriteLine("Waypoint ID : " + item.Key._waypointID);
                Console.WriteLine("All possible Wrong : ");
                foreach (RegionWaypointPoint items in item.Value)
                {
                    Console.WriteLine($"{item.Key._waypointID}'s possible wrong "+ "Region Guid : " + items._regionID);
                    Console.WriteLine($"{item.Key._waypointID}'s possible wrong "+"Waypoint Guid : " + items._waypointID);
                }
                Console.WriteLine("\n");
            }

            Console.WriteLine("The total distance between the destination : " + 
                _navigationGraph.TotalRouteDistance(_waypointsOnRoute));
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
									 new List<RegionWaypointPoint> ());
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
            RegionWaypointPoint tmpWaypoint = 
                new RegionWaypointPoint(regionID, waypointID);

            if (!_waypointsOnWrongWay.Keys.Contains(locationRegionWaypoint))
            {                
                _waypointsOnWrongWay
				.Add(locationRegionWaypoint, new List<RegionWaypointPoint> 
                { tmpWaypoint });
            }
            else if(!_waypointsOnWrongWay[locationRegionWaypoint]
                .Contains(tmpWaypoint))
            {
                _waypointsOnWrongWay[locationRegionWaypoint].Add(tmpWaypoint);
            }
        }

        public void OneMoreLayer(Guid neighborGuid, 
								 RegionWaypointPoint locationRegionWaypoint, 
								 int nextStep)
        {
            LocationType currentType =
                _navigationGraph
				.GetWaypointTypeInRegion(locationRegionWaypoint._regionID, 
                neighborGuid);

            Region nearWaypointRegion = 
                _regiongraphs[locationRegionWaypoint._regionID];

            if (currentType == LocationType.portal)
            {
                AddPortalWrongWaypoint(nearWaypointRegion, 
									   locationRegionWaypoint, 
									   nextStep,  
									   neighborGuid);
            }

            List<Guid> nearNonePortalWaypoint
                = _navigationGraph
				  .GetNeighbor(locationRegionWaypoint._regionID, neighborGuid);

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

                    double distanceBetweenNextAndNearNeighbor;

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
                        nearWaypointofSameRegion != neighborGuid &&
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
								
            RegionWaypointPoint detectWrongWay = new RegionWaypointPoint();
            detectWrongWay._waypointID = _currentWaypointID;
            detectWrongWay._regionID = _currentRegionID;

            //NavigationInstruction is a structure that contains five
            //elements that need to be passed to the main and UI
            NavigationInstruction navigationInstruction =
                new NavigationInstruction();

            if (_nextWaypointStep == -1) 
            {
                Console.WriteLine("current Waypoint : " + _currentWaypointID);
                _accumulateStraightDistance = 0;

                _iPSModules.CloseStartAllExistClient();
                _iPSModules
				.CompareToCurrentAndNextIPSType(_currentRegionID, 
												_currentRegionID, 
												_nextWaypointStep);

                if (_currentRegionID.Equals(_destinationRegionID) &&
                    _currentWaypointID.Equals(_destinationWaypointID))
                {
                    Console.WriteLine("---- [case: arrived destination] .... ");
                    int tempProgress = _waypointsOnRoute.Count() - 1;
                    if (tempProgress <= 0)
                    {
                        tempProgress = 0;
                    }
                    navigationInstruction._progressBar = tempProgress + " / " 
														 + tempProgress;
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
                    _currentWaypointID.Equals(_destinationWaypointID))
                {
                    int tempProgress = _waypointsOnRoute.Count() - 1;
                    navigationInstruction._progressBar = tempProgress + " / " 
														+ tempProgress;
                    Console.WriteLine("---- [case: arrived destination] .... ");
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
                    Console.WriteLine("next region/waypoint: {0}/{1}",
                                      _waypointsOnRoute[_nextWaypointStep + 1]
									  ._regionID,
                                      _waypointsOnRoute[_nextWaypointStep + 1]
									  ._waypointID);
                    navigationInstruction._currentWaypointName =
                        _navigationGraph
						.GetWaypointNameInRegion(_currentRegionID,
                                                 _currentWaypointID);
                    navigationInstruction._nextWaypointName =
                        _navigationGraph.GetWaypointNameInRegion(
                            _waypointsOnRoute[_nextWaypointStep + 1]._regionID,
                            _waypointsOnRoute[_nextWaypointStep + 1]._waypointID);

                    Guid previousRegionID = new Guid();
                    Guid previousWaypointID = new Guid();
                    if (_nextWaypointStep - 1 >= 0)
                    {
                        previousRegionID =
                            _waypointsOnRoute[_nextWaypointStep - 1]._regionID;
                        previousWaypointID =
                            _waypointsOnRoute[_nextWaypointStep - 1]._waypointID;
                    }

                    navigationInstruction._information =
                        _navigationGraph
                        .GetInstructionInformation(
                            _nextWaypointStep,
                            _currentRegionID,
                            _currentWaypointID,
                            previousRegionID,
                            previousWaypointID,
                            _waypointsOnRoute[_nextWaypointStep+1]._regionID,
                            _waypointsOnRoute[_nextWaypointStep+1]._waypointID,
                            _avoidConnectionTypes);
                    navigationInstruction._currentWaypointGuid = 
						_currentWaypointID;
						
                    navigationInstruction._nextWaypointGuid = 
						_waypointsOnRoute[_nextWaypointStep + 1]._waypointID;
						
                    navigationInstruction._currentRegionGuid = 
						_currentRegionID;
						
                    navigationInstruction._nextRegionGuid = 
						_waypointsOnRoute[_nextWaypointStep + 1]._regionID;                  
                    navigationInstruction._turnDirectionDistance = 
						_navigationGraph.GetDistanceOfLongHallway(
							(args as WaypointSignalEventArgs)
							._detectedRegionWaypoint, 
							_nextWaypointStep + 1,
							_waypointsOnRoute, 
							_avoidConnectionTypes);
                    Console.WriteLine("navigation_turn : " 
								+ navigationInstruction._turnDirectionDistance);
                    //Get the progress
                    Console.WriteLine("calculate progress: {0}/{1}",
                                      _nextWaypointStep,
                                      _waypointsOnRoute.Count);

                    navigationInstruction._progress =
                        (double)Math.Round(100 * ((decimal)_nextWaypointStep /
                                           (_waypointsOnRoute.Count - 1)), 3);
										   
                    int tempStep = _nextWaypointStep;
					
                    if (tempStep == -1)
                    {
                        tempStep = 0;
                    }
					
                    int tempProgress = _waypointsOnRoute.Count() - 1;
					
                    navigationInstruction._progressBar = tempStep 
														+ " / " + tempProgress;
														
                    navigationInstruction._previousRegionGuid = 
						previousRegionID;
						
                    // Raise event to notify the UI/main thread with the result                   
                    if (navigationInstruction._information._connectionType == 
						ConnectionType.VirtualHallway)
                    {
                        _accumulateStraightDistance = 0;
                        navigationInstruction._progressBar = tempProgress 
						+ " / " + tempProgress;
						
                        _event.OnEventCall(new NavigationEventArgs
                        {
                            _result = NavigationResult.ArriveVirtualPoint,
                            _nextInstruction = navigationInstruction
                        });
                    }
                    else
                    {
                        if (navigationInstruction._information._turnDirection ==
							TurnDirection.Forward && 
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
                    #region Console Msg
                    Console.WriteLine("In next next");

                    Console.WriteLine($"the next waypoint Step is {_nextWaypointStep}");
                    Console.WriteLine($"_waypointOnRoute[_nextWaypointStep-1] = {_waypointsOnRoute[_nextWaypointStep - 1]._waypointID}");

                    foreach (RegionWaypointPoint point in _waypointsOnWrongWay[_waypointsOnRoute[_nextWaypointStep - 1]])
                    {
                        Console.WriteLine($"Possible wrong waypoint in {_nextWaypointStep - 1} = " + point._waypointID);
                    }
                    #endregion

                    if (_currentRegionID.Equals
                        (_waypointsOnRoute[_nextWaypointStep + 1]._regionID)
                        && _currentWaypointID.Equals
                        (_waypointsOnRoute[_nextWaypointStep + 1]._waypointID))
                    {
                        _nextWaypointStep++;
                        navigationInstruction._currentWaypointName =
                       _navigationGraph.GetWaypointNameInRegion(_currentRegionID,
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

                        navigationInstruction._information =
                        _navigationGraph
                        .GetInstructionInformation(
                            _nextWaypointStep,
                            _currentRegionID,
                            _currentWaypointID,
                            previousRegionID,
                            previousWaypointID,
                            _waypointsOnRoute[_nextWaypointStep + 1]._regionID,
                            _waypointsOnRoute[_nextWaypointStep + 1]._waypointID,
                            _avoidConnectionTypes);

                        navigationInstruction._currentWaypointGuid =
                            _currentWaypointID;

                        navigationInstruction._nextWaypointGuid =
                            _waypointsOnRoute[_nextWaypointStep + 1]._waypointID;

                        navigationInstruction._currentRegionGuid =
                            _currentRegionID;

                        navigationInstruction._nextRegionGuid =
                            _waypointsOnRoute[_nextWaypointStep + 1]._regionID;

                        navigationInstruction._turnDirectionDistance =
                        _navigationGraph
                        .GetDistanceOfLongHallway
                        ((args as WaypointSignalEventArgs)
                        ._detectedRegionWaypoint,
                        _nextWaypointStep + 1,
                        _waypointsOnRoute,
                        _avoidConnectionTypes);

                        navigationInstruction._progress =
                        (double)Math.Round(100 * ((decimal)_nextWaypointStep /
                                           (_waypointsOnRoute.Count - 1)), 3);
                        int tempProgress = _waypointsOnRoute.Count() - 1;
                        int tempStep = _nextWaypointStep;
                        if (tempStep == -1)
                        {
                            tempStep = 0;
                        }
                        navigationInstruction._progressBar =
                            tempStep + " / " + tempProgress;

                        navigationInstruction._previousRegionGuid =
                            previousRegionID;

                        if (navigationInstruction._information._connectionType
                            == ConnectionType.VirtualHallway)
                        {
                            _accumulateStraightDistance = 0;
                            navigationInstruction._progressBar =
                                tempProgress + " / " + tempProgress;
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
                                    navigationInstruction._information._distance;
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
                             .Contains(detectWrongWay) == true && _DetectedWrongWaypoint)
                    {
                        Console.WriteLine("In next next wrong way");
                        HandleWrongWay();
                    }

                    Console.WriteLine("<< In next next");
                }
                else if (_nextWaypointStep >= 1 && 
						 _waypointsOnWrongWay
						 [_waypointsOnRoute[_nextWaypointStep - 1]].
						 Contains(detectWrongWay) == true && _DetectedWrongWaypoint)
                {
                    HandleWrongWay();
                }

                _nextWaypointEvent.Set();
            }

            Console.WriteLine("<< CheckArrivedWaypoint ");
        }

        public void HandleWrongWay()
        {
            _accumulateStraightDistance = 0;
            _DetectedWrongWaypoint = false;
            Console.WriteLine("---- [case: wrong waypoint] .... ");
            _event.OnEventCall(new NavigationEventArgs
            {
                _result = NavigationResult.AdjustRoute
            });
            Console.WriteLine("Adjust Route");
        }

        public void PauseSession()
        {
            Console.WriteLine(">>PasueSession");
            _OnStopThreadEvent.Reset();
        }
        public void CloseSession()
        {
            _isKeepDetection = false;
            _nextWaypointStep = -1;
            _iPSModules.Close();
            //_IPSClient.Stop();
            _nextWaypointEvent.Dispose();
            _waypointDetectionThread.Abort();
            _navigationControllerThread.Abort();
    //        _waypointsOnWrongWay = 
				//new Dictionary<RegionWaypointPoint, List<RegionWaypointPoint>>();
				
    //        _waypointsOnRoute = new List<RegionWaypointPoint>();
			
            _iPSModules._event._eventHandler -= new EventHandler(CheckArrivedWaypoint);
        }

        public void ResumeSession()
        {
            _nextWaypointEvent = 
                new ManualResetEventSlim(false);
            _OnStopThreadEvent.Set();
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
