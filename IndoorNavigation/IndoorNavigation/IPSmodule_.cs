using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using IndoorNavigation.Models;
using IndoorNavigation.Models.NavigaionLayer;
using IndoorNavigation.Modules.IPSClients;
using Xamarin.Forms;

namespace IndoorNavigation.Modules
{
    public class IPSmodule_ : IDisposable
    {
        private NavigationGraph _navigationGraph;

        private Dictionary<IPSType, IpsClient> _multiClients;
        public NavigationEvent _event { get; private set; }
        public IPSmodule_(NavigationGraph navigationGraph)
        {
            Console.WriteLine(">>IPSmodule : Constructor");
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
            Console.WriteLine(">>IPSmodule : AddMonittorBeaconList");
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

        // interface for Session to access IPSmodule.
        // Maybe we can use it to control which IPStype need to open or close.
        public void AddMonitorBeacon(Guid regionID, Guid waypointID)
        {
            Console.WriteLine("IPSmodule : AddMonitorBeacon ");
            IPSType type = _navigationGraph.GetRegionIPSType(regionID);

            AddMonitorBeaconList(type, regionID, new List<Guid> {waypointID});

            OpenIPSClint(type);
        }

        private WaypointBeaconsMapping GetSingleBeaconMapping(Guid regionID,
            Guid waypointID)
        {
            Console.WriteLine(">>IPSmodule : GetSingleBeaconMapping");

            List<Guid> beaconIDs =
                _navigationGraph.GetAllBeaconIDInOneWaypointOfRegion(
                    regionID, waypointID);

            Dictionary<Guid, int> beaconThresholdMapping =
                new Dictionary<Guid, int>();

            foreach(Guid beaconID in beaconIDs)
            {
                beaconThresholdMapping.Add(beaconID,
                    _navigationGraph.GetBeaconRSSIThreshold(regionID, beaconID)
                    );
            }

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
        private List<WaypointBeaconsMapping> GetBeaconWaypointMapping(
            Guid regionID, List<Guid> waypointIDs)
        {
            Console.WriteLine(">>IPSmodule : GetBeaconWaypointMapping");
            List<WaypointBeaconsMapping> BeaconWaypointMapping =
                new List<WaypointBeaconsMapping>();

            foreach (Guid waypointID in waypointIDs)
            {
                BeaconWaypointMapping.Add
                    (GetSingleBeaconMapping(regionID, waypointID));
                //List<Guid> beaconIDs =
                //    _navigationGraph.GetAllBeaconIDInOneWaypointOfRegion
                //    (regionID, waypointID);

                //Dictionary<Guid, int> beaconThresholdMapping =
                //    new Dictionary<Guid, int>();

                //foreach (Guid beaconID in beaconIDs)
                //{
                //    beaconThresholdMapping.Add(
                //        beaconID,
                //        _navigationGraph.GetBeaconRSSIThreshold
                //            (regionID, beaconID)
                //        );
                //}

                //BeaconWaypointMapping.Add(new WaypointBeaconsMapping
                //{
                //    _Beacons = beaconIDs,
                //    _BeaconThreshold = beaconThresholdMapping,
                //    _WaypointIDAndRegionID =
                //        new RegionWaypointPoint(regionID, waypointID)
                //});
            }

            return BeaconWaypointMapping;
        }
        public void InitialStep_DetectAllBeacon(List<Guid> regionIDs)
        {
            Console.WriteLine(">>IPSmoudle : InitialStep_DetectAllBeacon");
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
            Console.WriteLine(">>IPSmodule : StartAllExistClient");
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
        public void CloseAllActiveClient()
        {
            Console.WriteLine(">>IPSmodule : CloseAllActiveClient ");

            foreach(KeyValuePair<IPSType, IpsClient> pair in
                _multiClients)
            {
                if (pair.Value.ContainType)
                {
                    pair.Value.ContainType = false;
                    pair.Value._monitorMappings.Clear();
                    pair.Value.client.Stop();
                    pair.Value.client._event._eventHandler -=
                        new EventHandler(PassMatchedWaypointEvent);
                }
            }

            Console.WriteLine("<<IPSmodule : CloseAllActiveClient ");
        }
        private void OpenIPSClint(IPSType type)
        {
            Console.WriteLine(">>IPSmodule : OpenIPSClint");
            if (!_multiClients[type].ContainType)
            {
                _multiClients[type].client._event._eventHandler +=
                    new EventHandler(PassMatchedWaypointEvent);
                _multiClients[type].ContainType = true;
            }
        }

        //To pass monitor beacon list to PS client.
        public void SetMonitorBeaconList()
        {
            Console.WriteLine(">>IPSmodule : SetMonitorBeaconList");
            foreach(KeyValuePair<IPSType, IpsClient> pair in _multiClients)
            {
                if (pair.Value.ContainType)
                {
                    pair.Value.client.SetWaypointList(
                        pair.Value._monitorMappings
                        );
                }
            }
        }        

        public void PassMatchedWaypointEvent(Object sender, EventArgs args)
        {
            Console.WriteLine(">>IPSmodule : PassMatchedWaypointEvent");
            //CleanMappingBeaconList();
            CloseAllActiveClient();
            _event.OnEventCall(new WaypointSignalEventArgs
            {
                _detectedRegionWaypoint = (args as WaypointSignalEventArgs)
                ._detectedRegionWaypoint
            });

        }

        public void OpenBeaconScanning()
        {
            Console.WriteLine("IPSmoudle : OpenBeaconScanning");

            foreach(KeyValuePair<IPSType, IpsClient> pair in _multiClients)
            {
                if (pair.Value.ContainType)
                {
                    pair.Value.client.DetectWaypoints();
                }
            }
        }
        //public void CleanMappingBeaconList()
        //{
        //    foreach(KeyValuePair<IPSType, IpsClient> pair in
        //        _multiClients)
        //    {
        //        pair.Value._monitorMappings.Clear();
        //    }
        //}

        //public void Close()
        //{
        //    foreach(KeyValuePair<IPSType, IpsClient> pair in _multiClients)
        //    {
        //        pair.Value._monitorMappings.Clear();
        //        pair.Value.client.Stop();
        //        pair.Value.ContainType = false;
        //    }
        //}

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


        #region  For Rssi auto adjustment

        WaypointBeaconsMapping _rssiMapping;
        IPSType _rssiIPStype;

        public void SetRssiMonitor(Guid regionID, Guid waypointID)
        {
            _rssiMapping = GetSingleBeaconMapping(regionID, waypointID);
            _rssiIPStype = _navigationGraph.GetRegionIPSType(regionID);
        }

        public void OpenRssiScaning() 
        {          
            _multiClients[_rssiIPStype].client
                .DetectWaypointRssi(_rssiMapping);
        }
        public void PassScanRssiValue(Object sender, EventArgs args)
        {
            Console.WriteLine(">> IPSmodule : Pass RssiResult");

            _event.OnEventCall(args as WaypointRssiEventArgs) ;
        }
        #endregion
    }
}
