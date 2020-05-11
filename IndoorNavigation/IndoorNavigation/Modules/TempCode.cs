using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Xml;
using IndoorNavigation.Models;
using IndoorNavigation.Models.NavigaionLayer;
using System.Linq;

namespace IndoorNavigation.Modules
{
    class TempCode
    {
        Dictionary<RegionWaypointPoint, List<RegionWaypointPoint>> _waypointsOnWrongWay;
        NavigationGraph _navigationGraph = new NavigationGraph(new XmlDocument());
        Dictionary<Guid, Region> _regiongraphs = new Dictionary<Guid, Region>();
        private void GenerateWrongPath(List<RegionWaypointPoint> waypointOnRoute)
        {
            int nextStep = 1;
            _waypointsOnWrongWay = new Dictionary<RegionWaypointPoint, List<RegionWaypointPoint>>();
            Region tempRegion;
            List<Guid> NeighborGuids;
           
            foreach(RegionWaypointPoint currentWaypoint in waypointOnRoute)
            {
                NeighborGuids = _navigationGraph.GetNeighbor(currentWaypoint._regionID, currentWaypoint._waypointID);
                tempRegion = _regiongraphs[currentWaypoint._regionID];

                LocationType locationType = _navigationGraph.GetWaypointTypeInRegion(currentWaypoint._regionID, currentWaypoint._waypointID);

                if(locationType == LocationType.portal)
                {
                    
                }

                foreach(Guid neighborGuid in NeighborGuids)
                {
                    if (waypointOnRoute.Count() < nextStep)
                        break;

                    if (waypointOnRoute[nextStep]._waypointID != neighborGuid)
                    {
                        #region To caculate distance
                        double distanceOfCurrentAndNeighbor =
                            _navigationGraph.StraightDistanceBetweenWaypoints(currentWaypoint._regionID, currentWaypoint._waypointID, neighborGuid);
                        double distanceOfNextAndNeighbor;

                        if (currentWaypoint._regionID == waypointOnRoute[nextStep]._regionID)
                        {
                            distanceOfNextAndNeighbor =
                                _navigationGraph.StraightDistanceBetweenWaypoints(currentWaypoint._regionID, waypointOnRoute[nextStep]._waypointID, neighborGuid);
                        }
                        else
                        {
                            distanceOfNextAndNeighbor = distanceOfCurrentAndNeighbor;
                        }
                        #endregion
                    }
                    else if (!_waypointsOnWrongWay.ContainsKey(currentWaypoint))
                    {
                        _waypointsOnWrongWay.Add(currentWaypoint, new List<RegionWaypointPoint>());
                    }
                    
                }
                nextStep++;
            }
        }

        private void AddWrongWaypoint(Guid RegionID, Guid WaypointID, RegionWaypointPoint currentWaypoint)
        {
            RegionWaypointPoint waypoint = new RegionWaypointPoint();

            if (_waypointsOnWrongWay.ContainsKey(currentWaypoint))
            {
                waypoint._regionID = RegionID;
                waypoint._waypointID = WaypointID;
                _waypointsOnWrongWay[currentWaypoint].Add(waypoint);
            }
            else
            {
                waypoint._regionID = RegionID;
                waypoint._waypointID = WaypointID;
                _waypointsOnWrongWay.Add(currentWaypoint, new List<RegionWaypointPoint> { waypoint });
            }
        }
    }
}
