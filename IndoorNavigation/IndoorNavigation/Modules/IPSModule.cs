using System;
using System.Collections.Generic;
using IndoorNavigation.Models;
using IndoorNavigation.Models.NavigaionLayer;
using IndoorNavigation.Modules.IPSClients;

namespace IndoorNavigation.Modules
{
    public class IPSModule : IDisposable
    {
        private NavigationGraph _naviGraph;
        public NavigationEvent _event { get; private set; }
        private Dictionary<IPSType, IPSClient> _multiClients;

        private List<IPSType> _usedIPS;
        private List<HashSet<IPSType>> _ipsTable;
        public IPSModule(NavigationGraph naviGraph)
        {
            Console.WriteLine(">>IPSmodule : constructor");
            _naviGraph = naviGraph;
            _event = new NavigationEvent();
            _multiClients = new Dictionary<IPSType, IPSClient>();

            _usedIPS = _naviGraph.GetUsedIPSTyps();

            #region PS type initial
            #region LBeacon part
            if (_usedIPS.Contains(IPSType.LBeacon))
            {
                _multiClients.Add(IPSType.LBeacon, new IPSClient
                {
                    type = IPSType.LBeacon,
                    client = new WaypointClient(),
                    _detectedBeaconMapping = new List<WaypointBeaconsMapping>(),
                    _monitorBeaconMapping = new List<WaypointBeaconsMapping>()
                });

                _multiClients[IPSType.LBeacon].client._event._eventHandler +=
                    PassMatchedWaypointEvent;
            }
            #endregion

            #region iBeacon part
            if (_usedIPS.Contains(IPSType.iBeacon))
            {
                _multiClients.Add(IPSType.iBeacon, new IPSClient
                {
                    type = IPSType.iBeacon,
                    client = new IBeaconClient(),
                    _detectedBeaconMapping = new List<WaypointBeaconsMapping>(),
                    _monitorBeaconMapping = new List<WaypointBeaconsMapping>()
                });

                _multiClients[IPSType.iBeacon].client._event._eventHandler +=
                    PassMatchedWaypointEvent;
            }
            #endregion

            #region GPS part
            //this is not supported now.
            //_multiClient.Add(IPSType.GPS, new IPSClient
            //{
            //    client = new GPSClient(),
            //    type = IPSType.GPS,
            //    _monitorBeaconMapping = new List<WaypointBeaconsMapping>()
            //});
            #endregion

            #endregion

            Console.WriteLine("<<IPSmodule : constructor");
        }

        #region Initial and Session control interface.
        public void SetIPStable(List<HashSet<IPSType>> ipsTable)
        {
            _ipsTable = ipsTable;
        }

        public void InitialStep_DetectAllBeacon(List<Guid> regionIDs)
        {
            Console.WriteLine(">>InitialStep_DetectAllBeacon");

            foreach (Guid regionID in regionIDs)
            {
                List<Guid> waypointIDs =
                    _naviGraph.GetAllWaypointIDInOneRegion(regionID);

                AddDetectedBeaconList(regionID, waypointIDs);
            }

            Console.WriteLine("<<InitialStep_DetectAllBeacon");
        }
        #endregion

        #region Get beacon mapping
        private WaypointBeaconsMapping GetSingleBeaconMapping(Guid regionID,
            Guid waypointID)
        {
            Console.WriteLine(">>IPSmodule : GetSingleBeaconMapping");

            List<Guid> beaconIDs =
                _naviGraph.GetAllBeaconIDInOneWaypointOfRegion(
                    regionID, waypointID);

            Dictionary<Guid, int> beaconThresholdMapping =
                new Dictionary<Guid, int>();

            foreach (Guid beaconID in beaconIDs)
            {
                beaconThresholdMapping.Add(beaconID,
                    _naviGraph.GetBeaconRSSIThreshold(regionID, beaconID)
                    );
            }
            Console.WriteLine("<<IPSmodule : GetSingleBeaconMapping");
            return new WaypointBeaconsMapping
            {
                _beacons = beaconIDs,
                _beaconThreshold = beaconThresholdMapping,
                _waypoint = new RegionWaypointPoint
                {
                    _regionID = regionID,
                    _waypointID = waypointID
                }
            };
        }

        private List<WaypointBeaconsMapping> GetBeaconMapping(Guid regionID,
            List<Guid> waypointIDs)
        {
            Console.WriteLine(">>IPSmodule : GetBeaconWaypointMapping");

            List<WaypointBeaconsMapping> BeaconWaypointMapping =
                new List<WaypointBeaconsMapping>();

            foreach (Guid waypointID in waypointIDs)
            {
                BeaconWaypointMapping.Add
                    (GetSingleBeaconMapping(regionID, waypointID));
            }

            Console.WriteLine("<<IPSmodule : GetBeaconWaypointMapping");
            return BeaconWaypointMapping;
        }
        #endregion

        #region For detected waypoint.
        public void OpenBeaconScanning(int nextStep)
        {
            Console.WriteLine(">>IPSModule : OpenBeaconScanning");

            //when nextstep = -1, it will open all position system to know 
            //where user is.
            if (nextStep == -1)
            {
                foreach (IPSType type in _usedIPS)
                {
                    _multiClients[type].client.DetectWaypoints();
                }
            }
            else
            {
                foreach (IPSType type in _ipsTable[nextStep])
                {
                    Console.WriteLine("OpenScanning : " + type);
                    _multiClients[type].client.DetectWaypoints();
                }
            }
            Console.WriteLine("<<IPSModule : OpenBeaconScanning");
        }
        public void AddDetectedBeacon(Guid regionID, Guid waypointID)
        {
            Console.WriteLine(">>IPSModule : AddMonitorBeacon");

            IPSType type = _naviGraph.GetRegionIPSType(regionID);

            _multiClients[type]._detectedBeaconMapping.Add
                (GetSingleBeaconMapping(regionID, waypointID));
            Console.WriteLine("<<IPSModule : AddMonitorBeacon");
        }
        public void AddDetectedBeaconList(Guid regionID, List<Guid> waypointIDs)
        {
            Console.WriteLine(">>IPSModule : AddMonitorBeacon");

            IPSType type = _naviGraph.GetRegionIPSType(regionID);
            _multiClients[type]._detectedBeaconMapping.AddRange
                (GetBeaconMapping(regionID, waypointIDs));

            Console.WriteLine("<<IPSModule : AddMonitorBeacon");
        }
        public void SetDetectedBeaconList(int nextStep)
        {
            //Console.WriteLine("openIPStype count : " + OpenedIPSType.Count);
            Console.WriteLine(">>SetMonitorBeaconList");
            if (nextStep == -1)
            {
                foreach (IPSType type in _usedIPS)
                {
                    //TODO: To think how to fill in the monitor beacon list.
                    _multiClients[type].client.SetDetectedWaypointList
                        (_multiClients[type]._detectedBeaconMapping);
                }
            }
            else
            {
                foreach (IPSType type in _ipsTable[nextStep])
                {
                    //TODO : think how to fill the monitor Beacon List.
                    _multiClients[type].client.SetDetectedWaypointList
                        (_multiClients[type]._detectedBeaconMapping);
                }
            }
            Console.WriteLine("<<SetMonitorBeaconList");
        }
        private void PassMatchedWaypointEvent(object sender, EventArgs args)
        {
            Console.WriteLine(">>PassMatchedWaypointEvent");
            CleanMappingBeaconList();
            _event.OnEventCall(args as WaypointSignalEventArgs);

            //OpenedIPSType.Clear();
            Console.WriteLine("<<PassMatchedWaypointEvent");
        }
        #endregion

        #region For monitor waypoint
        private void GetMonitorWaypointEvent(object sender, EventArgs args)
        {
            Console.WriteLine(">>GetMonitorWaypointEvent");
            //To define what rssi is user leave the waypoints.
            Console.WriteLine("<<GetMonitorWaypointEvent");
        }
        public void AddMonitorBeacon(Guid regionID, Guid waypointID)
        {
            IPSType type = _naviGraph.GetRegionIPSType(regionID);
            _multiClients[type]._monitorBeaconMapping.Add
                (GetSingleBeaconMapping(regionID, waypointID));
        }
        public void SetMonitorBeaconList(int nextStep)
        {
            if (nextStep <= 0) return;
            // TODO : To monitor Beacons, need to consider how to implement.
            foreach (IPSType type in _ipsTable[nextStep - 1]) // to monitor previous one or next one?
            {
                _multiClients[type].client.SetMonitorWaypointList
                    (_multiClients[type]._monitorBeaconMapping); ;
            }
        }
        public void OpenBeaconMonitoring(int previousStep)
        {
            if (previousStep <= 0) return;

            foreach (IPSType type in _ipsTable[previousStep])
            {
                if(_multiClients[type]._monitorBeaconMapping.Count > 0)
                    _multiClients[type].client.MonitorWaypoints();
            }
        }
        #endregion

        #region for detect rssi

        private WaypointBeaconsMapping _rssiMapping;
        private IPSType _rssiIPStype;

        public void SetRssiMonitor(Guid regionID, Guid waypointID)
        {
            _rssiMapping = GetSingleBeaconMapping(regionID, waypointID);
            _rssiIPStype = _naviGraph.GetRegionIPSType(regionID);
            _multiClients[_rssiIPStype].client._event._eventHandler +=
                PassScanRssiValue;
            _multiClients[_rssiIPStype].ContainType = true;
        }
        public void OpenRssiScanning()
        {
            _multiClients[_rssiIPStype].client.OnRestart();
            _multiClients[_rssiIPStype].client
                .DetectWaypointRssi(_rssiMapping);
        }
        public void PassScanRssiValue(object sender, EventArgs args)
        {
            Console.WriteLine(">>IPSmodule : PassScanRssiValue");
            _event.OnEventCall(args as WaypointRssiEventArgs);
        }
        #endregion

        #region Life Cycle control
        public void CloseAllActiveClient()
        {
            Console.WriteLine(">>IPSmodule : CloseAllActiveClient");
            foreach (KeyValuePair<IPSType, IPSClient> pair in _multiClients)
            {
                if (pair.Value.ContainType)
                {
                    pair.Value.ContainType = false;
                    pair.Value._detectedBeaconMapping.Clear();
                    pair.Value.client.Stop();
                    pair.Value.client._event._eventHandler -=
                        new EventHandler(PassMatchedWaypointEvent);
                }
            }
            Console.WriteLine("<<IPSmodule : CloseAllActiveClient");
        }
        public void CleanMappingBeaconList()
        {
            foreach (IPSType type in _usedIPS)
            {
                _multiClients[type]._detectedBeaconMapping.Clear();
                _multiClients[type]._monitorBeaconMapping.Clear();
            }
        }

        #region for disposed function
        private bool _disposedValue = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (!disposing)
                {
                    CloseAllActiveClient();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
        #endregion
        #region Data Structure
        public class IPSClient
        {
            public IPSType type { get; set; }
            public bool ContainType { get; set; } = false;
            public IIPSClient client { get; set; }
            public List<WaypointBeaconsMapping> _detectedBeaconMapping
            { get; set; }
            public List<WaypointBeaconsMapping> _monitorBeaconMapping { get; set; }
        }
        #endregion
    }
}
