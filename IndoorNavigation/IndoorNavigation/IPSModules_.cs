using System;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Dijkstra.NET.Model;
using Dijkstra.NET.Extensions;
using IndoorNavigation.Models.NavigaionLayer;
using IndoorNavigation.Models;
using IndoorNavigation.Modules.IPSClients;
using System.Diagnostics.Contracts;
namespace IndoorNavigation
{
    class IPSModules_ : IDisposable
    {
        private Dictionary<IPSType, BeaconClient> _multiClient;
        public NavigationEvent _event { get; private set; }
        private NavigationGraph _navigationGraph;

        public IPSModules_(NavigationGraph navigationGraph)
        {
            _navigationGraph = navigationGraph;
            _multiClient = new Dictionary<IPSType, BeaconClient>();
            InitialClientDict();
            _event = new NavigationEvent();

        }
        private void InitialClientDict()
        {
            _multiClient.Add(IPSType.LBeacon,
               new BeaconClient
               {
                   haveIPSKind = false,
                   _moniterBeaconGuid = new List<WaypointBeaconsMapping>()
               });
            _multiClient.Add(IPSType.iBeacon,
                new BeaconClient
                {
                    haveIPSKind = false,
                    _moniterBeaconGuid = new List<WaypointBeaconsMapping>()
                });
        }
        public void StartAllExistClient()
        {
            foreach (IPSType type in Enum.GetValues(typeof(IPSType)))
            {
                if (_multiClient[type].haveIPSKind)
                {
                    _multiClient[type].client = new WaypointClient();
                    _multiClient[type].client._event._eventHandler += new EventHandler(PassMatchedWaypointAndRegionToSession);
                    _multiClient[type].client.SetWaypointList(_multiClient[type]._moniterBeaconGuid);
                }
            }
        }

        public void CompareToCurrentAndNextIPSType(Guid currentRegionGuid, Guid nextRegionGuid, int firstStep)
        {
            IPSType currentIPSType = _navigationGraph.GetRegionIPSType(currentRegionGuid);
            IPSType nextIPSType = _navigationGraph.GetRegionIPSType(nextRegionGuid);
            if (!nextIPSType.Equals(currentIPSType) || firstStep == -1)
            {
                BeaconTypeAllFalse();



                OpenCurrentIPSClient(nextIPSType);
                OpenCurrentIPSClient(currentIPSType);
            }
            Console.WriteLine($">>IPSModules CompareToCurrentAndNextIPSType, currentType={currentIPSType}, nextType={nextIPSType}");
        }

        public void PassMatchedWaypointAndRegionToSession(object sender, EventArgs args)
        {
            CleanAllMappingBeaconList();
            RegionWaypointPoint matchedWaypointAndRegion = new RegionWaypointPoint();
            matchedWaypointAndRegion._waypointID = (args as WaypointSignalEventArgs)._detectedRegionWaypoint._waypointID;
            matchedWaypointAndRegion._regionID = (args as WaypointSignalEventArgs)._detectedRegionWaypoint._regionID;
            _event.OnEventCall(new WaypointSignalEventArgs
            {
                _detectedRegionWaypoint = matchedWaypointAndRegion
            });
            return;
        }

        private void BeaconTypeAllFalse()
        {
            foreach (IPSType type in Enum.GetValues(typeof(IPSType)))
            {
                _multiClient[type].haveIPSKind = false;
            }
        }
        private void CleanAllMappingBeaconList()
        {
            _multiClient[IPSType.iBeacon].client = new IBeaconClient();
            _multiClient[IPSType.LBeacon].client = new WaypointClient();
        }

        public void OpenCurrentIPSClient(IPSType currentIPSType)
        {
            switch (currentIPSType)
            {
                case IPSType.LBeacon:
                    _multiClient[IPSType.LBeacon].client = new WaypointClient();                   
                    break;
                case IPSType.iBeacon:
                    _multiClient[IPSType.iBeacon].client = new IBeaconClient();                    
                    break;
            }
            _multiClient[currentIPSType].haveIPSKind = true;
        }

        public IPSType getIPSType(Guid regionGuid)
        {
            return _navigationGraph.GetRegionIPSType(regionGuid);
        }

        public void OpenBeaconScanning()
        {
            foreach(IPSType type in Enum.GetValues(typeof(IPSType)))
            {
                _multiClient[type].client.DetectWaypoints();
            }
        }

        #region used in session.cs
        public void AtStarting_ReadALLIPSType(List<Guid> allRegionGuid)
        {
            foreach (Guid regionGuid in allRegionGuid)
            {
                List<Guid> waypointGuids = new List<Guid>();
                waypointGuids = _navigationGraph.GetAllWaypointIDInOneRegion(regionGuid);
                AddInterestedBeacon(regionGuid, waypointGuids);
            }

            StartAllExistClient();
        }
        public void AddNextWaypointInterestedGuid(Guid regionGuid, Guid waypointGuid)
        {
            foreach (IPSType type in Enum.GetValues(typeof(IPSType)))
            {
                AddInterestedBeacon(regionGuid, new List<Guid> { waypointGuid });
            }
        }
        #endregion

        public void AddInterestedBeacon(Guid regionID, List<Guid> waypointGuids)
        {
            foreach (IPSType type in Enum.GetValues(typeof(IPSType)))
            {
                if (_navigationGraph.GetRegionIPSType(regionID) == type)
                {
                    _multiClient[type].haveIPSKind = true;
                    _multiClient[type]._moniterBeaconGuid.AddRange(FindTheMappingOfWaypointAndItsBeacon(regionID, waypointGuids));
                }
            }
        }
        public List<WaypointBeaconsMapping> FindTheMappingOfWaypointAndItsBeacon(Guid regionGuid, List<Guid> waypoints)
        {
            List<WaypointBeaconsMapping> waypointBeaconsMappings = new List<WaypointBeaconsMapping>();
            foreach (Guid waypointID in waypoints)
            {
                RegionWaypointPoint regionWaypointPoint = new RegionWaypointPoint();
                regionWaypointPoint._regionID = regionGuid;
                regionWaypointPoint._waypointID = waypointID;
                List<Guid> beaconIDs = new List<Guid>();
                beaconIDs = _navigationGraph.GetAllBeaconIDInOneWaypointOfRegion(regionGuid, waypointID);
                Dictionary<Guid, int> beaconThresholdMapping = new Dictionary<Guid, int>();
                for (int i = 0; i < beaconIDs.Count(); i++)
                {
                    beaconThresholdMapping.Add(beaconIDs[i], _navigationGraph.GetBeaconRSSIThreshold(regionGuid, beaconIDs[i]));
                }
                waypointBeaconsMappings.Add(new WaypointBeaconsMapping
                {
                    _WaypointIDAndRegionID = regionWaypointPoint,
                    _Beacons = beaconIDs,
                    _BeaconThreshold = beaconThresholdMapping
                });
            }
            return waypointBeaconsMappings;
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void CloseModule()
        {
            Console.WriteLine(">>IPSmodule :: Close");


        }
        private class BeaconClient
        {
            public List<WaypointBeaconsMapping> _moniterBeaconGuid;
            public bool haveIPSKind;
            public IIPSClient client;
        }
    }


}
