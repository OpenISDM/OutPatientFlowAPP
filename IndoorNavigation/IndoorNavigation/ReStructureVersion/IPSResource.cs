using IndoorNavigation.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;


namespace IndoorNavigation.ReStructureVersion
{
    public class IPSResource
    {
        #region Properties
        private string _naviGraphName;
        private Dictionary<Guid, BeaconsGraph> _beaconGraph;
       
        #endregion

        #region Methods
        public IPSType GetRegionIPSType(Guid RegionID)
        {
            return IPSType.iBeacon;
        }

        public List<Guid> GetAllBeaconIDInOneWaypointOfRegion(RegionWaypointPoint waypoint)
        {
            return new List<Guid>();
        }

        public int GetBeaconRSSIThreshold(RegionWaypointPoint waypoint)
        {
            return 0;
        }

        public List<Guid> GetAllWaypointIDInOneRegion(Guid regionID)
        {
            return new List<Guid>();
        }
        #endregion

        #region Constructor

       // public IPSResource(string naviGraphName) { }

        public IPSResource(XmlDocument xmlDocument)
        {
            
        }
        #endregion

        #region Classes and Enums
        public class BeaconsGraph
        {
            //the Guid is waypoint's ID.
            public Dictionary<Guid, List<Guid>> _beacons { get; set; }
            public Dictionary<Guid, int> _beaconRSSIThreshold { get; set; }
        }

        public enum IPSType
        {
            LBeacon = 0,
            iBeacon,
            GPS
        }
        #endregion
        //#region Structs and Classes
        //public struct IPSInfo
        //{
        //    public Dictionary<Guid, int> _beaconRSSIThreshold { get; set; }
        //    public Dictionary<Guid, List<Guid>> _beacons { get; set; }
        //}
        //#endregion
    }


}
