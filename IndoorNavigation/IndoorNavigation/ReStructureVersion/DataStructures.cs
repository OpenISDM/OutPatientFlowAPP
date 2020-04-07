using System;
using System.Collections.Generic;
using System.Text;
using static IndoorNavigation.Navigation.RegionGraph;

namespace IndoorNavigation.Model
{
    #region DataStructures
    public struct Instruction
    {
        public Direction Direction;
        public RegionWaypointPoint NextWaypoint;
        public RegionWaypointPoint CurrentWaypoint;
        public string Distance;
    }

    public struct RegionWaypointPoint
    {
        public Guid _regionID { get; set; }
        public Guid _waypointID { get; set; }

        public RegionWaypointPoint(Guid regionID, Guid waypointID)
        {
            this._regionID = regionID;
            this._waypointID = waypointID;
        }
    }

    public struct Waypoint
    {
        public Guid _id { get; set; }     
        public string _name { get; set; }
        public LocationType _type { get; set; }
        public CategoryType _category { get; set; }
        public Point _position { get; set; }
        public List<Guid> _neighbors { get; set; }        
    }

    public struct RegionEdge
    {
        public RegionWaypointPoint SourceWaypoint { get; set; }
        public RegionWaypointPoint DestinationWaypoint { get; set; }        
        public Direction Direction { get; set; }
        public ConnectionType ConnectionType { get; set; }
        public double Distance { get; set; }        
    }

    public struct WaypointEdge
    {
        public Guid _sourceID { get; set; }
        public Guid _destinationID { get; set; }
        public double _distance { get; set; }
        public ConnectionType _connectionType { get; set; }
        public Direction _direction { get; set; }
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

    public enum CategoryType
    {

    }
    
    public enum Direction
    {

    }
    #endregion
}
