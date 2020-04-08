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

        public IPSResource(string naviGraphName) { }

        public IPSResource(XmlDocument xmlDocument)
        {
            
        }
        #endregion

        #region Structs and Classes
        public struct IPSInfo
        {
            public Dictionary<Guid, int> _beaconRSSIThreshold { get; set; }
            public Dictionary<Guid, List<Guid>> _beacons { get; set; }
        }
        #endregion
    }

    public enum IPSType
    {
        LBeacon=0,
        iBeacon,
        GPS
    }
}
