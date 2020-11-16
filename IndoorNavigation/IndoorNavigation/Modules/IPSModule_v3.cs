using System;
using System.Collections.Generic;
using System.Text;

using IndoorNavigation.Models;
using IndoorNavigation.Models.NavigaionLayer;
using IndoorNavigation.Modules.IPSClients;

namespace IndoorNavigation.Modules
{
    public class IPSModule_v3 : IDisposable
    {
        private NavigationGraph _naviGraph;
        public NavigationEvent _event { get; private set; }
        private Dictionary<IPSType, IPSClient> _multiClients;
        private HashSet<IPSType> OpenedIPSType;
        public IPSModule_v3(NavigationGraph naviGraph)
        {
            Console.WriteLine(">>IPSmodule : constructor");

            _naviGraph = naviGraph;
            _event = new NavigationEvent();
            OpenedIPSType = new HashSet<IPSType>();

            #region PS type initial
            _multiClients.Add(IPSType.LBeacon, new IPSClient
            {
                type = IPSType.LBeacon,
                client = new WaypointClient(),
                _monitorBeaconMapping = new List<WaypointBeaconsMapping>()
            });

            _multiClients.Add(IPSType.iBeacon, new IPSClient
            {
                type = IPSType.iBeacon,
                client = new IBeaconClient(),
                _monitorBeaconMapping = new List<WaypointBeaconsMapping>()
            });

            //this is not supported now.
            //_multiClient.Add(IPSType.GPS, new IPSClient
            //{

            //});
            #endregion

            Console.WriteLine("<<IPSmodule : constructor");
        }
        private IPSType[] SupportedIPS =
            { IPSType.iBeacon, IPSType.LBeacon, IPSType.GPS };

        #region for detect waypoint

        public void OpenBeaconScanning()
        {
            Console.WriteLine(">>IPSModule : OpenBeaconScanning");
            foreach (IPSType type in OpenedIPSType)
            {
                _multiClients[type].client.DetectWaypoints();
            }
            Console.WriteLine("<<IPSModule : OpenBeaconScanning");
        }
        public void InitialStep_DetectAllBeacon(List<Guid> regionIDs)
        {
            foreach (Guid regionID in regionIDs)
            {
                List<Guid> waypointIDs =
                    _naviGraph.GetAllWaypointIDInOneRegion(regionID);

                AddMonitorBeaconList(regionID, waypointIDs);
            }
        }

        public void PSTurnOFF()
        {
            foreach (IPSType type in SupportedIPS)
            {
                if (!OpenedIPSType.Contains(type))
                {
                    _multiClients[type].client.Stop();
                    _multiClients[type].ContainType = false;
                }
            }
        }

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
                _Beacons = beaconIDs,
                _BeaconThreshold = beaconThresholdMapping,
                _WaypointIDAndRegionID = new RegionWaypointPoint
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
            List<WaypointBeaconsMapping> beaconMapping =
                new List<WaypointBeaconsMapping>();

            List<WaypointBeaconsMapping> BeaconWaypointMapping =
                new List<WaypointBeaconsMapping>();

            foreach (Guid waypointID in waypointIDs)
            {
                BeaconWaypointMapping.Add
                    (GetSingleBeaconMapping(regionID, waypointID));
            }

            Console.WriteLine("<<IPSmodule : GetBeaconWaypointMapping");
            return beaconMapping;
        }

        //public void AddSingleMonitorBeacon(Guid regionID, Guid waypointID)
        //{
        //    Console.WriteLine(">>IPSModule : AddSingleMonitorBeacon");

        //    IPSType type = _naviGraph.GetRegionIPSType(regionID);
        //    _multiClients[type].ContainType = true;
        //    _multiClients[type].client._event._eventHandler +=
        //        PassMatchedWaypointEvent;

        //    //_multiClients[type]._monitorBeaconMapping.AddRange(GetBeaconMapping(regionID, new )

        //    _multiClients[type].client.SetWaypointList(_multiClients[type]._monitorBeaconMapping);
        //    OpenedIPSType.Add(type);
        //    Console.WriteLine(">>IPSModule : AddSingleMonitorBeacon");
        //}

        public void AddMonitorBeacon(Guid regionID, Guid waypointID)
        {
            Console.WriteLine(">>IPSModule : AddMonitorBeacon");

            IPSType type = _naviGraph.GetRegionIPSType(regionID);

            if (!_multiClients[type].ContainType)
            {
                _multiClients[type].ContainType = true;
                _multiClients[type].client._event._eventHandler +=
                    PassMatchedWaypointEvent;
                OpenedIPSType.Add(type);
            }

            _multiClients[type]._monitorBeaconMapping.Add
                (GetSingleBeaconMapping(regionID, waypointID));
            Console.WriteLine("<<IPSModule : AddMonitorBeacon");
        }
        public void AddMonitorBeaconList(Guid regionID, List<Guid> waypointIDs)
        {
            Console.WriteLine(">>IPSModule : AddMonitorBeacon");

            IPSType type = _naviGraph.GetRegionIPSType(regionID);

            if (! OpenedIPSType.Contains(type) && 
                _multiClients[type].ContainType)
            {
                _multiClients[type].ContainType = true;
                _multiClients[type].client._event._eventHandler +=
                    PassMatchedWaypointEvent;
                OpenedIPSType.Add(type);
            }

            _multiClients[type]._monitorBeaconMapping.AddRange
                (GetBeaconMapping(regionID, waypointIDs));
           
            Console.WriteLine("<<IPSModule : AddMonitorBeacon");
        }

        private void PassMatchedWaypointEvent(object sender, EventArgs args)
        {
            CleanMappingBeaconList();
            _event.OnEventCall(args as WaypointSignalEventArgs);
        }       

        public void SetMonitorBeaconList()
        {
            foreach(IPSType type in OpenedIPSType)
            {
                _multiClients[type].client.SetWaypointList
                    (_multiClients[type]._monitorBeaconMapping);
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
        public void CloseAllActiveClient()
        {
            Console.WriteLine(">>IPSmodule : CloseAllActiveClient");
            foreach (KeyValuePair<IPSType, IPSClient> pair in _multiClients)
            {
                if (pair.Value.ContainType)
                {
                    pair.Value.ContainType = false;
                    pair.Value._monitorBeaconMapping.Clear();
                    pair.Value.client.Stop();
                    pair.Value.client._event._eventHandler -=
                        new EventHandler(PassMatchedWaypointEvent);
                }
            }
            Console.WriteLine("<<IPSmodule : CloseAllActiveClient");
        }
        public void CleanMappingBeaconList()
        {
            foreach (IPSType type in OpenedIPSType)
            {
                _multiClients[type]._monitorBeaconMapping.Clear();
            }
            OpenedIPSType.Clear();
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

        #region Data Structure
        public class IPSClient
        {
            public IPSType type { get; set; }
            public bool ContainType { get; set; } = false;
            public IIPSClient client { get; set; }
            public List<WaypointBeaconsMapping> _monitorBeaconMapping
            { get; set; }
        }
        #endregion
    }
}
