using IndoorNavigation.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
namespace IndoorNavigation.Navigation
{
    //indeed: Length, Picture, Route( Distance, Direction, Next point
    
    public class RegionGraph
    {
        #region Properties
        //the Key guid is region id
        private Dictionary<Guid, NaviGraph> _regions { get; set; } //all region in the coverage area
        private Dictionary<Tuple<Guid,Guid>, List<RegionEdge>> _edges { get; set; }

        private List<Guid> _allRegionIDs;

        private const double EARTH_RADIUS = 6378137;

        private string BuildingName;        
        #endregion

        #region Methods
        private double GetDistance(Point point1, Point point2) 
        {
            double radLat1 = Rad(point1.lat);
            double radLng1 = Rad(point1.lon);
            double radLat2 = Rad(point2.lat);
            double radLng2 = Rad(point2.lon);
            double a = radLat1 - radLat2;
            double b = radLng1 - radLng2;

            return  2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) +
                    Math.Cos(radLat1) *
                    Math.Cos(radLat2) *
                    Math.Pow(Math.Sin(b / 2), 2))) * EARTH_RADIUS;;
        }
        private double Rad(double d)
        {
            return (double)d * Math.PI / 180d;
        }
        public string GetWaypointName(RegionWaypointPoint waypoint) { return "name"; }
        public LocationType GetLocationType(RegionWaypointPoint waypoint) { return LocationType.midpath; }
        public double GetDistanceOfLongHallway(Waypoint currentWaypoint, List<Waypoint> allRoute, int nextStep, ConnectionType avoidConnectionType) { return 0; }
        //Todo : to return a straight hall way length totally.
        public Instruction GetInstruction(int currentStep, Waypoint currentWaypoint, Waypoint nextWaypoint, Waypoint previousWaypoint, ConnectionType[] avoidConnectionTypes) { return new Instruction(); }
        public List<Guid> GetAllRegionID()
        {
            List<Guid> regionIDs = new List<Guid>();
            foreach(KeyValuePair<Guid,NaviGraph> regionItems in _regions)
            {
                regionIDs.Add(regionItems.Key);
            }
            return regionIDs;
        }
        //Todo : to return the instruction details.
        public List<Waypoint> GenerateRoute(RegionWaypointPoint currentWaypoint, RegionWaypointPoint destinationWaypoint) 
        { 
            return new List<Waypoint>(); 
        }
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

            public Dictionary<Tuple<Guid, Guid>, WaypointEdge> _edges { get; set; }  //this is the edges between waypoint and waypoint
        }

        #region Enums
        public enum LocationType
        {
            midpath=0   
        }
        #endregion
    }
}
