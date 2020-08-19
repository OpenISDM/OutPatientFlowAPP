using System;
using System.Collections.Generic;
using System.Text;
using IndoorNavigation.Models;
using IndoorNavigation.Models.NavigaionLayer;
using IndoorNavigation.Modules;
using IndoorNavigation.Modules.IPSClients;
using Prism.Navigation.Xaml;

namespace IndoorNavigation
{
    public class IPSmodule_ : IDisposable
    {
        private NavigationGraph _navigationGraph;

        private Dictionary<IPSType, IpsClient> _multiClients;

        public NavigationEvent _event { get; private set; }
        public IPSmodule_(NavigationGraph navigationGraph)
        {
            _navigationGraph = navigationGraph;

            _multiClients = new Dictionary<IPSType, IpsClient>();

            #region LBeacon part Initial
            _multiClients.Add(IPSType.LBeacon,
                new IpsClient
                {
                    ContainType = false,
                    type = IPSType.LBeacon,
                    client = new WaypointClient(),
                    _monitorMappings = new List<WaypointBeaconsMapping>()
                });

            #endregion

            #region iBeacon part Initial
            _multiClients.Add(IPSType.iBeacon,
                new IpsClient
                {
                    ContainType = false,
                    type = IPSType.iBeacon,
                    client = new IBeaconClient(),
                    _monitorMappings = new List<WaypointBeaconsMapping>()
                });
            #endregion

            #region GPS part Initial
            //do nothing now.
            #endregion
        }

        private void AddMonitorBeaconList(IPSType type,
            Guid regionID,
            List<Guid> waypointIDs)
        {
            try
            {
                _multiClients[type].ContainType = true;
                _multiClients[type]._monitorMappings.AddRange(
                    GetBeaconWaypointMapping(regionID, waypointIDs));
            }
            catch (Exception exc)
            {
                //maybe generate Dictionary exception.
                Console.WriteLine("AddMonitorBeaconList error : "
                    + exc.Message);
                throw exc;
            }
        }        

        // interface for Session to access to add
        // the function is a little bit dumplicate.
        public void AddMonitorWaypoint(Guid regionID, Guid waypointID)
        {
            IPSType type = _navigationGraph.GetRegionIPSType(regionID);

            AddMonitorBeaconList(type, regionID, new List<Guid> {waypointID});
        }

        private List<WaypointBeaconsMapping> GetBeaconWaypointMapping(
            Guid regionID, List<Guid> waypointIDs)
        {
            List<WaypointBeaconsMapping> BeaconWaypointMapping =
                new List<WaypointBeaconsMapping>();

            foreach (Guid waypointID in waypointIDs)
            {
                List<Guid> beaconIDs =
                    _navigationGraph.GetAllBeaconIDInOneWaypointOfRegion
                    (regionID, waypointID);

                Dictionary<Guid, int> beaconThresholdMapping =
                    new Dictionary<Guid, int>();

                foreach (Guid beaconID in beaconIDs)
                {
                    beaconThresholdMapping.Add(
                        beaconID,
                        _navigationGraph.GetBeaconRSSIThreshold
                            (regionID, beaconID)
                        );
                }

                BeaconWaypointMapping.Add(new WaypointBeaconsMapping
                {
                    _Beacons = beaconIDs,
                    _BeaconThreshold = beaconThresholdMapping,
                    _WaypointIDAndRegionID =
                        new RegionWaypointPoint(regionID, waypointID)
                });
            }

            return BeaconWaypointMapping;
        }
        public void InitialStep_DetectAllBeacon(List<Guid> regionIDs)
        {
            foreach (Guid regionID in regionIDs)
            {
                IPSType type = _navigationGraph.GetRegionIPSType(regionID);

                List<Guid> waypointIDs =
                    _navigationGraph.GetAllWaypointIDInOneRegion(regionID);

                AddMonitorBeaconList(type, regionID, waypointIDs);
            }
        }


        

        public void StartAllExistClient()
        {
            foreach (KeyValuePair<IPSType, IpsClient> pair in
                _multiClients)
            {
                if (pair.Value.ContainType)
                {
                    pair.Value.client._event._eventHandler +=
                        new EventHandler(PassMatchedWaypointEvent);
                    pair.Value.client.SetWaypointList
                        (pair.Value._monitorMappings);
                }
            }
        }

        private void OpenIPSClint(IPSType type)
        {
            _multiClients[type].client._event._eventHandler +=
                new EventHandler(PassMatchedWaypointEvent);
            _multiClients[type].ContainType = true;
        }

        public void PassMatchedWaypointEvent(Object sender, EventArgs args)
        {
            CleanMappingBeaconList();
            _event.OnEventCall(new WaypointSignalEventArgs
            {
                _detectedRegionWaypoint = (args as WaypointSignalEventArgs)
                ._detectedRegionWaypoint
            });

        }

        public void CleanMappingBeaconList()
        {
            foreach(KeyValuePair<IPSType, IpsClient> pair in
                _multiClients)
            {
                pair.Value._monitorMappings.Clear();
            }
        }

        public void Close()
        {

        }

        public class IpsClient
        {
            public IPSType type { get; set; }
            public bool ContainType { get; set; }
            public IIPSClient client { get; set; }
            public List<WaypointBeaconsMapping> _monitorMappings { get; set; }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
