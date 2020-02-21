//using System;
//using System.Collections.Generic;
//using System.Text;

//using Dijkstra.NET.Model;
//using Dijkstra.NET.Extensions;
//using IndoorNavigation.Models.NavigaionLayer;
//using IndoorNavigation.Models;
//using IndoorNavigation.Modules.IPSClients;
//using System.Linq;

//namespace IndoorNavigation.Modules.Navigation
//{
//    class Graph_ :IDisposable
//    {
//        #region define variables
//        private Guid _destinationWaypointID;
//        private Guid _destinationRegionID;
//        private Guid _currentWaypointID;
//        private Guid _currentRegionID;
//        private int _nextWaypointStep = -1;

//        private IPSModules _iPSModules;        

//        private List<RegionWaypointPoint> _waypointsOnRoute;
//        private Dictionary<RegionWaypointPoint, List<RegionWaypointPoint>>
//            _waypointsOnWrongWay = new Dictionary<RegionWaypointPoint, List<RegionWaypointPoint>>();
//        private Graph<Guid, string> _graphRegionGraph = new Graph<Guid, string>();
//        private NavigationGraph _navigationGraph;

//        private const int _remindDistance = 50;
//        private int _accumulateStraightDistance = 0;
//        private ConnectionType[] _avoidConnectionTypes;
//        #endregion

     

//        public Graph_(Guid destinationRegionID, Guid destinationWaypointID)
//        {

//            _destinationRegionID = destinationRegionID;
//            _destinationWaypointID = destinationWaypointID;
//        }

      

//        public void CheckArrivedWaypoint(object sender, EventArgs args)
//        {
//            Console.WriteLine(">> CheckArrivedWaypoint ");

//            _currentWaypointID = (args as WaypointSignalEventArgs)._detectedRegionWaypoint._waypointID;
//            _currentRegionID = (args as WaypointSignalEventArgs)._detectedRegionWaypoint._regionID;
//            //Console.WriteLine("CheckArrived currentWaypoint : " + _currentWaypointID);
//            //Console.WriteLine("CheckArrived currentRegion : " + _currentRegionID);
//            RegionWaypointPoint detectWrongWay = new RegionWaypointPoint();
//            detectWrongWay._waypointID = _currentWaypointID;
//            detectWrongWay._regionID = _currentRegionID;
          

//            if (_nextWaypointStep == -1)
//            {
//                //Console.WriteLine("current Waypoint : " + _currentWaypointID);
//                _accumulateStraightDistance = 0;

//                //_iPSModules.CloseStartAllExistClient();
//                //_iPSModules.CompareToCurrentAndNextIPSType(_currentRegionID, _currentRegionID, _nextWaypointStep);

//                if (_currentRegionID.Equals(_destinationRegionID) &&
//                    _currentWaypointID.Equals(_destinationWaypointID))
//                {
//                    Console.WriteLine("---- [case: arrived destination] .... ");
//                    int tempProgress = _waypointsOnRoute.Count() - 1;
//                    if (tempProgress <= 0)
//                    {
//                        tempProgress = 0;
//                    }
//                    navigationInstruction._progressBar = tempProgress + " / " + tempProgress;
//                    _accumulateStraightDistance = 0;
                    
//                    //result arrival
//                }
//                _nextWaypointEvent.Set();
//            }
//            else
//            {
//                if (_currentRegionID.Equals(_destinationRegionID) &&
//                    _currentWaypointID.Equals(_destinationWaypointID))
//                {
//                    int tempProgress = _waypointsOnRoute.Count() - 1;
//                    navigationInstruction._progressBar = tempProgress + " / " + tempProgress;
//                    Console.WriteLine("---- [case: arrived destination] .... ");
//                    _accumulateStraightDistance = 0;

//                    //_event.OnEventCall(new NavigationEventArgs
//                    //{
//                    //    _result = NavigationResult.Arrival,
//                    //    _nextInstruction = navigationInstruction
//                    //});
//                    //reslut
//                }
//                else if (_currentRegionID.Equals(
//                             _waypointsOnRoute[_nextWaypointStep]._regionID) &&
//                         _currentWaypointID.Equals(
//                             _waypointsOnRoute[_nextWaypointStep]._waypointID))
//                {
//                    Console.WriteLine("---- [case: arrived waypoint] .... ");

//                    Console.WriteLine("current region/waypoint: {0}/{1}",
//                                      _currentRegionID,
//                                      _currentWaypointID);
//                    Console.WriteLine("next region/waypoint: {0}/{1}",
//                                      _waypointsOnRoute[_nextWaypointStep + 1]._regionID,
//                                      _waypointsOnRoute[_nextWaypointStep + 1]._waypointID);
//                    navigationInstruction._currentWaypointName =
//                        _navigationGraph.GetWaypointNameInRegion(_currentRegionID,
//                                                                 _currentWaypointID);
//                    navigationInstruction._nextWaypointName =
//                        _navigationGraph.GetWaypointNameInRegion(
//                            _waypointsOnRoute[_nextWaypointStep + 1]._regionID,
//                            _waypointsOnRoute[_nextWaypointStep + 1]._waypointID);

//                    Guid previousRegionID = new Guid();
//                    Guid previousWaypointID = new Guid();
//                    if (_nextWaypointStep - 1 >= 0)
//                    {
//                        previousRegionID =
//                            _waypointsOnRoute[_nextWaypointStep - 1]._regionID;
//                        previousWaypointID =
//                            _waypointsOnRoute[_nextWaypointStep - 1]._waypointID;
//                    }

//                    navigationInstruction._information =
//                        _navigationGraph
//                        .GetInstructionInformation(
//                            _nextWaypointStep,
//                            _currentRegionID,
//                            _currentWaypointID,
//                            previousRegionID,
//                            previousWaypointID,
//                            _waypointsOnRoute[_nextWaypointStep + 1]._regionID,
//                            _waypointsOnRoute[_nextWaypointStep + 1]._waypointID,
//                            _avoidConnectionTypes);
//                    navigationInstruction._currentWaypointGuid = _currentWaypointID;
//                    navigationInstruction._nextWaypointGuid = _waypointsOnRoute[_nextWaypointStep + 1]._waypointID;
//                    navigationInstruction._currentRegionGuid = _currentRegionID;
//                    navigationInstruction._nextRegionGuid = _waypointsOnRoute[_nextWaypointStep + 1]._regionID;
//                    navigationInstruction._turnDirectionDistance = _navigationGraph.GetDistanceOfLongHallway((args as WaypointSignalEventArgs)._detectedRegionWaypoint, _nextWaypointStep + 1, _waypointsOnRoute, _avoidConnectionTypes);
//                    Console.WriteLine("navigation_turn : " + navigationInstruction._turnDirectionDistance);
//                    //Get the progress
//                    Console.WriteLine("calculate progress: {0}/{1}",
//                                      _nextWaypointStep,
//                                      _waypointsOnRoute.Count);

//                    navigationInstruction._progress =
//                        (double)Math.Round(100 * ((decimal)_nextWaypointStep /
//                                           (_waypointsOnRoute.Count - 1)), 3);
//                    int tempStep = _nextWaypointStep;
//                    if (tempStep == -1)
//                    {
//                        tempStep = 0;
//                    }
//                    int tempProgress = _waypointsOnRoute.Count() - 1;
//                    navigationInstruction._progressBar = tempStep + " / " + tempProgress;
//                    navigationInstruction._previousRegionGuid = previousRegionID;
//                    // Raise event to notify the UI/main thread with the result
//                    //if()
//                    if (navigationInstruction._information._connectionType == ConnectionType.VirtualHallway)
//                    {
//                        _accumulateStraightDistance = 0;
//                        navigationInstruction._progressBar = tempProgress + " / " + tempProgress;
//                        _event.OnEventCall(new NavigationEventArgs
//                        {
//                            _result = NavigationResult.ArriveVirtualPoint,
//                            _nextInstruction = navigationInstruction
//                        });
//                    }
//                    else
//                    {
//                        if (navigationInstruction._information._turnDirection == TurnDirection.Forward && _nextWaypointStep != -1 && _accumulateStraightDistance >= _remindDistance)
//                        {
//                            _accumulateStraightDistance = _accumulateStraightDistance + navigationInstruction._information._distance;
//                            _event.OnEventCall(new NavigationEventArgs
//                            {
//                                _result = NavigationResult.ArrivaIgnorePoint,
//                                _nextInstruction = navigationInstruction
//                            });
//                        }
//                        else
//                        {

//                            _accumulateStraightDistance = 0;
//                            _accumulateStraightDistance = _accumulateStraightDistance + navigationInstruction._information._distance;
//                            _event.OnEventCall(new NavigationEventArgs
//                            {
//                                _result = NavigationResult.Run,
//                                _nextInstruction = navigationInstruction
//                            });
//                        }

//                    }
//                }
//                else if (_nextWaypointStep + 1 < _waypointsOnRoute.Count())
//                {
//                    Console.WriteLine("In next next");
//                    if (_currentRegionID.Equals(
//                             _waypointsOnRoute[_nextWaypointStep + 1]._regionID) &&
//                         _currentWaypointID.Equals(
//                             _waypointsOnRoute[_nextWaypointStep + 1]._waypointID))
//                    {
//                        _nextWaypointStep++;
//                        navigationInstruction._currentWaypointName =
//                       _navigationGraph.GetWaypointNameInRegion(_currentRegionID,
//                                                                _currentWaypointID);
//                        navigationInstruction._nextWaypointName =
//                            _navigationGraph.GetWaypointNameInRegion(
//                                _waypointsOnRoute[_nextWaypointStep + 1]._regionID,
//                                _waypointsOnRoute[_nextWaypointStep + 1]._waypointID);
//                        Guid previousRegionID = new Guid();
//                        Guid previousWaypointID = new Guid();
//                        if (_nextWaypointStep - 1 >= 0)
//                        {
//                            previousRegionID =
//                                _waypointsOnRoute[_nextWaypointStep - 1]._regionID;
//                            previousWaypointID =
//                                _waypointsOnRoute[_nextWaypointStep - 1]._waypointID;
//                        }

//                        navigationInstruction._information =
//                        _navigationGraph
//                        .GetInstructionInformation(
//                            _nextWaypointStep,
//                            _currentRegionID,
//                            _currentWaypointID,
//                            previousRegionID,
//                            previousWaypointID,
//                            _waypointsOnRoute[_nextWaypointStep + 1]._regionID,
//                            _waypointsOnRoute[_nextWaypointStep + 1]._waypointID,
//                            _avoidConnectionTypes);
//                        navigationInstruction._currentWaypointGuid = _currentWaypointID;
//                        navigationInstruction._nextWaypointGuid = _waypointsOnRoute[_nextWaypointStep + 1]._waypointID;
//                        navigationInstruction._currentRegionGuid = _currentRegionID;
//                        navigationInstruction._nextRegionGuid = _waypointsOnRoute[_nextWaypointStep + 1]._regionID;

//                        navigationInstruction._turnDirectionDistance = _navigationGraph.GetDistanceOfLongHallway((args as WaypointSignalEventArgs)._detectedRegionWaypoint, _nextWaypointStep + 1, _waypointsOnRoute, _avoidConnectionTypes);
//                        navigationInstruction._progress =
//                        (double)Math.Round(100 * ((decimal)_nextWaypointStep /
//                                           (_waypointsOnRoute.Count - 1)), 3);
//                        int tempProgress = _waypointsOnRoute.Count() - 1;
//                        int tempStep = _nextWaypointStep;
//                        if (tempStep == -1)
//                        {
//                            tempStep = 0;
//                        }
//                        navigationInstruction._progressBar = tempStep + " / " + tempProgress;

//                        navigationInstruction._previousRegionGuid = previousRegionID;

//                        if (navigationInstruction._information._connectionType == ConnectionType.VirtualHallway)
//                        {
//                            _accumulateStraightDistance = 0;
//                            navigationInstruction._progressBar = tempProgress + " / " + tempProgress;
//                            _event.OnEventCall(new NavigationEventArgs
//                            {
//                                _result = NavigationResult.ArriveVirtualPoint,
//                                _nextInstruction = navigationInstruction
//                            });
//                        }
//                        else
//                        {
//                            if (navigationInstruction._information._turnDirection == TurnDirection.Forward && _nextWaypointStep != -1 && _accumulateStraightDistance >= _remindDistance)
//                            {
//                                _accumulateStraightDistance = _accumulateStraightDistance + navigationInstruction._information._distance;
//                                _event.OnEventCall(new NavigationEventArgs
//                                {
//                                    _result = NavigationResult.ArrivaIgnorePoint,
//                                    _nextInstruction = navigationInstruction
//                                });
//                            }
//                            else
//                            {
//                                _accumulateStraightDistance = 0;
//                                _accumulateStraightDistance = _accumulateStraightDistance + navigationInstruction._information._distance;
//                                _event.OnEventCall(new NavigationEventArgs
//                                {
//                                    _result = NavigationResult.Run,
//                                    _nextInstruction = navigationInstruction
//                                });
//                            }
//                        }

//                    }
//                    else if (_nextWaypointStep >= 1 && _waypointsOnWrongWay[_waypointsOnRoute[_nextWaypointStep - 1]].Contains(detectWrongWay) == true)
//                    {
//                        HandleWrongWay();
//                    }
//                }
//                else if (_nextWaypointStep >= 1 && _waypointsOnWrongWay[_waypointsOnRoute[_nextWaypointStep - 1]].Contains(detectWrongWay) == true)
//                {
//                    HandleWrongWay();
//                }

//                _nextWaypointEvent.Set();
//            }

//            Console.WriteLine("<< CheckArrivedWaypoint ");
//        }

//        public void HandleWrongWay()
//        {
//            _accumulateStraightDistance = 0;
//            Console.WriteLine("---- [case: wrong waypoint] .... ");
//            _event.OnEventCall(new NavigationEventArgs
//            {
//                _result = NavigationResult.AdjustRoute
//            });
//            Console.WriteLine("Adjust Route");
//        }

//        private void GenerateRoute(Guid sourceRegionID,
//                                  Guid sourceWaypointID,
//                                  Guid destinationRegionID,
//                                  Guid destinationWaypointID)
//        {
//            // generate path between regions (from sourceRegionID to destnationRegionID)
//            uint region1Key = _graphRegionGraph
//                              .Where(node => node.Item.Equals(sourceRegionID))
//                              .Select(node => node.Key).First();
//            uint region2Key = _graphRegionGraph
//                              .Where(node => node.Item.Equals(destinationRegionID))
//                              .Select(node => node.Key).First();

//            var pathRegions = _graphRegionGraph.Dijkstra(region1Key, region2Key).GetPath();

//            if (0 == pathRegions.Count())
//            {
//                Console.WriteLine("No path. Need to change avoid connection type");               
//                //result no route
//                return;
//            }

//            // store the generate Dijkstra path across regions
//            List<Guid> regionsOnRoute = new List<Guid>();
//            for (int i = 0; i < pathRegions.Count(); i++)
//            {
//                regionsOnRoute.Add(_graphRegionGraph[pathRegions.ToList()[i]].Item);
//            }

            
//            _waypointsOnRoute.Add(new RegionWaypointPoint
//            {
//                _regionID = sourceRegionID,
//                _waypointID = sourceWaypointID
//            });

//            for (int i = 0; i < _waypointsOnRoute.Count(); i++)
//            {
//                RegionWaypointPoint checkPoint = _waypointsOnRoute[i];
//                Console.WriteLine("check index = {0}, count = {1}, region {2} waypoint {3}",
//                                  i,
//                                  _waypointsOnRoute.Count(),
//                                  checkPoint._regionID,
//                                  checkPoint._waypointID);
//                if (regionsOnRoute.IndexOf(checkPoint._regionID) + 1 <
//                    regionsOnRoute.Count())
//                {
//                    LocationType waypointType =
//                        _navigationGraph.GetWaypointTypeInRegion(checkPoint._regionID,
//                                                                 checkPoint._waypointID);

//                    Guid nextRegionID =
//                        regionsOnRoute[regionsOnRoute.IndexOf(checkPoint._regionID) + 1];

//                    PortalWaypoints portalWaypoints =
//                        _navigationGraph.GetPortalWaypoints(checkPoint._regionID,
//                                                            checkPoint._waypointID,
//                                                            nextRegionID,
//                                                            _avoidConnectionTypes);

//                    if (LocationType.portal != waypointType)
//                    {
//                        _waypointsOnRoute.Add(new RegionWaypointPoint
//                        {
//                            _regionID = checkPoint._regionID,
//                            _waypointID = portalWaypoints._portalWaypoint1
//                        });
//                    }
//                    else if (LocationType.portal == waypointType)
//                    {
//                        if (!checkPoint._waypointID.Equals(portalWaypoints._portalWaypoint1))
//                        {
//                            _waypointsOnRoute.Add(new RegionWaypointPoint
//                            {
//                                _regionID = checkPoint._regionID,
//                                _waypointID = portalWaypoints._portalWaypoint1
//                            });
//                        }
//                        else
//                        {
//                            _waypointsOnRoute.Add(new RegionWaypointPoint
//                            {
//                                _regionID = nextRegionID,
//                                _waypointID = portalWaypoints._portalWaypoint2
//                            });
//                        }
//                    }
//                }
//            }
//            int indexLastCheckPoint = _waypointsOnRoute.Count() - 1;
//            if (!(_destinationRegionID.
//                Equals(_waypointsOnRoute[indexLastCheckPoint]._regionID) &&
//                _destinationWaypointID.
//                Equals(_waypointsOnRoute[indexLastCheckPoint]._waypointID)))
//            {
//                _waypointsOnRoute.Add(new RegionWaypointPoint
//                {
//                    _regionID = _destinationRegionID,
//                    _waypointID = _destinationWaypointID
//                });
//            }        


//            // fill in all the path between waypoints in the same region / navigraph
//            for (int i = 0; i < _waypointsOnRoute.Count() - 1; i++)
//            {
//                RegionWaypointPoint currentCheckPoint = _waypointsOnRoute[i];
//                RegionWaypointPoint nextCheckPoint = _waypointsOnRoute[i + 1];

//                if (currentCheckPoint._regionID.Equals(nextCheckPoint._regionID))
//                {
//                    Graph<Guid, string> _graphNavigraph =
//                        _navigationGraph.GenerateNavigraph(currentCheckPoint._regionID,
//                                                           _avoidConnectionTypes);

//                    // generate path between two waypoints in the same region / navigraph
//                    uint waypoint1Key = _graphNavigraph
//                                        .Where(node => node.Item
//                                               .Equals(currentCheckPoint._waypointID))
//                                        .Select(node => node.Key).First();
//                    uint waypoint2Key = _graphNavigraph
//                                        .Where(node => node.Item
//                                               .Equals(nextCheckPoint._waypointID))
//                                        .Select(node => node.Key).First();

//                    var pathWaypoints =
//                        _graphNavigraph.Dijkstra(waypoint1Key, waypoint2Key).GetPath();

//                    for (int j = pathWaypoints.Count() - 1; j > 0; j--)
//                    {
//                        if (j != 0 && j != pathWaypoints.Count() - 1)
//                        {
//                            _waypointsOnRoute.Insert(i + 1, new RegionWaypointPoint
//                            {
//                                _regionID = currentCheckPoint._regionID,
//                                _waypointID = _graphNavigraph[pathWaypoints.ToList()[j]].Item
//                            });
//                        }
//                    }
//                }
//            }         



//            int nextStep = 1;
//            _waypointsOnWrongWay = new Dictionary<RegionWaypointPoint, List<RegionWaypointPoint>>();
//            Region tempRegion = new Region();
//            List<Guid> neighborGuid = new List<Guid>();

//            //For each waypoint in _waypointsOnRoute, decide their wrong waypoint.
//            foreach (RegionWaypointPoint locationRegionWaypoint in _waypointsOnRoute)
//            {
//                RegionWaypointPoint tempRegionWaypoint = new RegionWaypointPoint();
//                Console.WriteLine("Important Current Waypoint : " + locationRegionWaypoint._waypointID);
//                //Get the neighbor of all wapoint in _waypointOnRoute.
//                neighborGuid = new List<Guid>();
//                neighborGuid = _navigationGraph.GetNeighbor(locationRegionWaypoint._regionID, locationRegionWaypoint._waypointID);

//                tempRegion = _regiongraphs[locationRegionWaypoint._regionID];

//                LocationType locationType =
//                        _navigationGraph.GetWaypointTypeInRegion(locationRegionWaypoint._regionID,
//                                                                 locationRegionWaypoint._waypointID);
//                //If the waypoints are portal, we need to get its related portal waypoints in other regions.
//                if (locationType.ToString() == "portal")
//                {
//                    AddPortalWrongWaypoint(tempRegion, locationRegionWaypoint, nextStep, tempRegionWaypoint, locationRegionWaypoint._waypointID);
//                }
//                //Get the waypoint neighbor's Guids and add them in _waypointsOnWrongWay except next Waypoint Guid.
//                //We know just consider One-Step Wrong Way.
//                foreach (Guid guid in neighborGuid)
//                {
//                    if (_waypointsOnRoute.Count() > nextStep)
//                    {
//                        if (_waypointsOnRoute[nextStep]._waypointID != guid)
//                        {
//                            double distanceBetweenCurrentAndNeighbor = _navigationGraph.StraightDistanceBetweenWaypoints(
//                                       locationRegionWaypoint._regionID,
//                                       locationRegionWaypoint._waypointID,
//                                       guid);
//                            double distanceBetweenNextAndNeighbor = 0;
//                            //If current region == next region, we can get get the straight distance of the neighbors of current waypoint and next waypoint.
//                            //If current region != next region, we just consider the distance between cuurent and its neighbers, therefore, we give distanceBetweenNextAndNeighbor
//                            //the same value of distanceBetweenCurrentAndNeighbor.
//                            if (locationRegionWaypoint._regionID == _waypointsOnRoute[nextStep]._regionID)
//                            {
//                                distanceBetweenNextAndNeighbor = _navigationGraph.StraightDistanceBetweenWaypoints(
//                                       locationRegionWaypoint._regionID,
//                                       _waypointsOnRoute[nextStep]._waypointID,
//                                       guid);
//                            }
//                            else
//                            {
//                                distanceBetweenNextAndNeighbor = distanceBetweenCurrentAndNeighbor;
//                            }
//                            //If the distance of current and its neighbors and the distance between next and current's neighbors
//                            //are far enough, we add them into _waypointOnWrongWay, else if the distance between current and its neighbors
//                            //are too close, we need to find one more step.
//                            if (distanceBetweenCurrentAndNeighbor >= _tooCLoseDistance && distanceBetweenNextAndNeighbor >= _tooCLoseDistance)
//                            {
//                                if (nextStep >= 2)
//                                {
//                                    if (_waypointsOnRoute[nextStep - 2]._waypointID != guid)
//                                    {
//                                        AddWrongWaypoint(guid, locationRegionWaypoint._regionID, locationRegionWaypoint, tempRegionWaypoint);
//                                    }
//                                }
//                                else
//                                {
//                                    AddWrongWaypoint(guid, locationRegionWaypoint._regionID, locationRegionWaypoint, tempRegionWaypoint);
//                                }

//                            }
//                            else if (distanceBetweenCurrentAndNeighbor < _tooCLoseDistance)
//                            {
//                                OneMoreLayer(guid, locationRegionWaypoint, nextStep, tempRegionWaypoint);
//                            }

//                            if (nextStep >= 2)
//                            {
//                                if (_waypointsOnRoute[nextStep - 2]._waypointID == guid)
//                                {
//                                    OneMoreLayer(guid, locationRegionWaypoint, nextStep, tempRegionWaypoint);
//                                }
//                            }
//                        }
//                        else
//                        {
//                            if (!_waypointsOnWrongWay.Keys.Contains(locationRegionWaypoint))
//                            {
//                                _waypointsOnWrongWay.Add(locationRegionWaypoint, new List<RegionWaypointPoint> { });
//                            }
//                        }
//                    }
//                }
//                nextStep++;
//            }
                       


//        }

//        public void Dispose()
//        {

//        }
//    }
//}
