using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
namespace NavigatorStruture
{
    //indeed: Length, Picture, Route( Distance, Direction, Next point
    
    public class RegionGraph
    {
        #region Properties
        //the Key guid is region id
        private Dictionary<Guid, NaviGraph> _regions; //all region in the coverage area
        private List<Guid> _allRegionIDs;
        private const double EARTH_RADIUS = 6378137;

        private string BuildingName;        
        #endregion

        #region Methods
        private double GetDistance(Point point1, Point point2) { return 0; }
        public string GetWaypointName(Waypoint waypoint) { return "name"; }
        public LocationType GetLocationType(Waypoint waypoint) { return LocationType.midpath; }
        public double GetDistanceOfLongHallway(Waypoint currentWaypoint, List<Waypoint> allRoute, int nextStep, ConnectionType avoidConnectionType) { return 0; }
        //Todo : to return a straight hall way length totally.
        public Instruction GetInstruction(int currentStep, Waypoint currentWaypoint, Waypoint nextWaypoint, Waypoint previousWaypoint, ConnectionType[] avoidConnectionTypes) { return new Instruction(); }
        //Todo : to return the instruction that 
        public List<Waypoint> GenerateRoute(Waypoint currentWaypoint, Waypoint destinationWaypoint) { return new List<Waypoint>(); }
        #endregion

        #region Constructor
        public RegionGraph(XmlDocument inputxmlDocument  /*or input a string .etc*/) { }
        #endregion
        public struct NaviGraph //a region
        {           
            public Guid RegionID { get; set; }
            public int Floor { get; set; }
            public string Name { get; set; }
            public List<Guid> WaypointIDs { get; set; } //the waypoint IDs in the region.
            public List<Guid> Neighbors { get; set; } //this is the other region in the same building
        }

        #region Enums
        public enum LocationType
        {
            midpath=0   
        }
        #endregion
    }
}
