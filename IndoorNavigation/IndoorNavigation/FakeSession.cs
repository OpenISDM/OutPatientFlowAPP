//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading.Tasks;
//using System.Threading;

//using Dijkstra.NET.Model;
//using IndoorNavigation.Modules;
//using IndoorNavigation.Models;
//using IndoorNavigation.Models.NavigaionLayer;
//using System.Linq;
//using static IndoorNavigation.Modules.Session;
//using Dijkstra.NET.Extensions;

//namespace IndoorNavigation
//{
//    class FakeSession
//    {
//        private List<RegionWaypointPoint> _waypointsOnRoute;
//        private int _nextWaypointStep=-1;

//        private Guid _destinationRegionID;
//        private Guid _destinationWaypointID;
//        private Guid _currentRegionID = new Guid();
//        private Guid _currentWaypointID = new Guid();

//        private ConnectionType[] _avoidConnectionTypes;
//        private ManualResetEventSlim _nextWaypointEvent = new ManualResetEventSlim(false);
//        private Graph<Guid, string> _graphRegionGraph;

//        public NavigationEvent _event { get; private set; }

//        public FakeSession()
//        {

//            _waypointsOnRoute = new List<RegionWaypointPoint>();
//            _graphRegionGraph = new Graph<Guid, string>();
//        }
//        private void GenerateRoute(Guid sourceRegionID,
//                                 Guid sourceWaypointID,
//                                 Guid destinationRegionID,
//                                 Guid destinationWaypointID)
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
//                _event.OnEventCall(new NavigationEventArgs
//                {
//                    _result = NavigationResult.NoRoute
//                });
//                return;
//            }

//            // store the generate Dijkstra path across regions
//            List<Guid> regionsOnRoute = new List<Guid>();
//            for (int i = 0; i < pathRegions.Count(); i++)
//            {
//                regionsOnRoute.Add(_graphRegionGraph[pathRegions.ToList()[i]].Item);
//            }

//            // generate the path of the region/waypoint checkpoints across regions
//            _waypointsOnRoute = new List<RegionWaypointPoint>();
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

//            foreach (RegionWaypointPoint checkPoint in _waypointsOnRoute)
//            {
//                Console.WriteLine("region-graph region/waypoint = {0}/{1}",
//                                  checkPoint._regionID,
//                                  checkPoint._waypointID);
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

//            // display the resulted full path of region/waypoint between source and destination
//            foreach (RegionWaypointPoint checkPoint in _waypointsOnRoute)
//            {
//                Console.WriteLine("full-path region/waypoint = {0}/{1}",
//                                  checkPoint._regionID,
//                                  checkPoint._waypointID);
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

//            //Print All Possible Wrong Way
//            foreach (KeyValuePair<RegionWaypointPoint, List<RegionWaypointPoint>> item in _waypointsOnWrongWay)
//            {
//                Console.WriteLine("Region ID : " + item.Key._regionID);
//                Console.WriteLine("Waypoint ID : " + item.Key._waypointID);
//                Console.WriteLine("All possible Wrong : ");
//                foreach (RegionWaypointPoint items in item.Value)
//                {
//                    Console.WriteLine("Region Guid : " + items._regionID);
//                    Console.WriteLine("Waypoint Guid : " + items._waypointID);
//                }
//                Console.WriteLine("\n");
//            }


//        }

//        class RegionWaypointPoint
//        {
//            public Guid _regionID { get; set; }
//            public Guid _waypointID { get; set; }
//        }
//    }
//}
