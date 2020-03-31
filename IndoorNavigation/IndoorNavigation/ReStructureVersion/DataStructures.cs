using System;
using System.Collections.Generic;
using System.Text;

namespace NavigatorStruture
{
    #region DataStructures
    public struct Instruction
    {
        public Direction Direction;
        public Waypoint NextWaypoint;
        public Waypoint CurrentWaypoint;
        public string Distance;
    }

    public struct Waypoint
    {
        public Guid WaypointID { get; set; }
        public Guid RegionID { get; set; }
        public Waypoint(Guid waypointID, Guid regionID)
        {
            WaypointID = waypointID;
            RegionID = regionID;
        }
    }

    public struct Edge
    {
        public Waypoint SourceWaypoint { get; set; }
        public Waypoint DestinationWaypoint { get; set; }        
        public Direction Direction { get; set; }
        public ConnectionType ConnectionType { get; set; }
        public double Distance { get; set; }

        
    }

    public struct Point
    {
        public double lon { get; set; }
        public double lat { get; set; }
    }
    #endregion

    #region Enums
    public enum ConnectionType
    {

    }

    public enum PreferenceType
    {

    }

    public enum Direction
    {

    }
    #endregion
}
