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

namespace IndoorNavigation.Modules
{
    public class Session
    {
        private int _nextWaypointStep;

        private List<RegionWaypointPoint> _waypointsOnRoute;
        private Dictionary<RegionWaypointPoint, List<RegionWaypointPoint>> _waypointsOnWrongWay;

        private NavigationGraph _navigationGraph;

        private Guid _destinationRegionID;
        private Guid _destinationWaypointID;
        private Guid _currentRegionID;
        private Guid _currentWaypointID;

        private Thread _waypointDetectionThread;
        private Thread _navigationControllerThread;

        private const int _remindDistance = 50;
        private int _accumulateStraightDistance = 0;

        private bool _isKeepDetection;
        
        private ConnectionType[] _avoidConnectionTypes;

        private ManualResetEventSlim _nextWaypointEvent = 
			new ManualResetEventSlim(false);

        public NavigationEvent _event { get; private set; }
			
        private IPSModules _iPSModules;
        private RouteInfos _routeInfo;

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
            
            _nextWaypointStep = -1;
            _isKeepDetection = true;
            _iPSModules = new IPSModules("a");
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
            checkWrongRegionWaypoint._regionID = _currentRegionID;
            checkWrongRegionWaypoint._waypointID = _currentWaypointID;

            NavigateToNextWaypoint(_currentRegionID, _nextWaypointStep);

            while (true == _isKeepDetection &&
                   !(_currentRegionID.Equals(_destinationRegionID) &&
                     _currentWaypointID.Equals(_destinationWaypointID)))
            {

                Console.WriteLine("Continue to navigate to next step, current"+
								"location {0}/{1}",
                                  _currentRegionID, _currentWaypointID);

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

                    _routeInfo = _navigationGraph.GenerateRoute(_currentRegionID, _currentWaypointID, _destinationRegionID, _destinationWaypointID);
                    _waypointsOnRoute = _routeInfo._waypointsOnRoute;
                    _waypointsOnWrongWay = _routeInfo._waypointsOnWrongRoute;

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
					.Contains(checkWrongRegionWaypoint) == true)
                {

                    Console.WriteLine("In Program Wrong, going to Re-calculate"+
									  "the route");
                    _nextWaypointStep = 0;
                    _routeInfo = _navigationGraph.GenerateRoute(_currentRegionID, _currentWaypointID, _destinationRegionID, _destinationWaypointID);
                    _waypointsOnRoute = _routeInfo._waypointsOnRoute;
                    _waypointsOnWrongWay = _routeInfo._waypointsOnWrongRoute;                    

                    Console.WriteLine("Finish Construct New Route");

                    Guid previousRegionID = new Guid();
                    Guid previousWaypointID = new Guid();

                    // Add this function can avoid that when users go to the 
					// worong waypoint, the instuction will jump to fast.

                    SpinWait.SpinUntil(() => false, 5000);
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
                Thread.Sleep(500);
                _iPSModules.OpenBeconScanning();
            }
        }


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
                    Console.WriteLine("In next next");
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
							 .Contains(detectWrongWay) == true)
                    {
                        HandleWrongWay();
                    }
                }
                else if (_nextWaypointStep >= 1 && 
						 _waypointsOnWrongWay
						 [_waypointsOnRoute[_nextWaypointStep - 1]].
						 Contains(detectWrongWay) == true)
                {
                    HandleWrongWay();
                }

                _nextWaypointEvent.Set();
            }

            Console.WriteLine("<< CheckArrivedWaypoint ");
        }

        
        private void HandleWrongWay()
        {
            _accumulateStraightDistance = 0;
            Console.WriteLine("---- [case: wrong waypoint] .... ");
            _event.OnEventCall(new NavigationEventArgs
            {
                _result = NavigationResult.AdjustRoute
            });
            Console.WriteLine("Adjust Route");
        }

        public void CloseSession()
        {
            _isKeepDetection = false;
            _nextWaypointStep = -1;
            _iPSModules.Close();

            _nextWaypointEvent.Dispose();
            _waypointDetectionThread.Abort();
            _navigationControllerThread.Abort();
            _waypointsOnWrongWay = 
				new Dictionary<RegionWaypointPoint, List<RegionWaypointPoint>>();
				
            _waypointsOnRoute = new List<RegionWaypointPoint>();
			
            _iPSModules._event._eventHandler -= new EventHandler(CheckArrivedWaypoint);
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
