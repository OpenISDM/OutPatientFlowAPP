using System;
using System.Collections.Generic;
using System.Text;

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

        public string BuildingName;        
        #endregion

        #region Methods
        private double GetDistance(Point point1, Point point2) { return 0; }
        public Instruction GetInstruction(int currentStep, Waypoint currentWaypoint, Waypoint nextWaypoint, Waypoint previousWaypoint, ConnectionType[] avoidConnectionTypes) { return new Instruction(); }
        #endregion
     
        public struct NaviGraph //a region
        {           
            public Guid RegionID { get; set; }
            public int Floor { get; set; }
            public string Name { get; set; }
            public List<Guid> WaypointIDs { get; set; } //the waypoint IDs in the region.
            public List<Guid> Neighbors { get; set; } //this is the other region in the same building
        }
        //public NaviGraph() { }

        //#region Private Propeties
        //private List<Waypoint> _waypointOnRoute;
        //private Dictionary<Waypoint, List<Waypoint>> _wrongpointOnRoute;

        //private PreferenceType PreferenceType;
        //private Waypoint _currentWaypoint;
        //private Waypoint _destinationWaypoint;
        //#endregion

        //#region Public Methods
        //public List<Instruction> ReturnInstruction(Waypoint currentWaypoint, Waypoint destinationWaypoint)
        //{
        //    return new List<Instruction>();
        //}
        ////I'm curious about it that should it need to return all route just call it first time?
        //#endregion

        //#region Private Methods
        //private void GenerateRoute() { }
        //private void GenerateWrong() { }
        //#endregion
    }
}
