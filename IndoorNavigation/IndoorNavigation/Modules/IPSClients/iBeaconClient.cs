/*
 * 2020 © Copyright (c) BiDaE Technology Inc. 
 * Provided under BiDaE SHAREWARE LICENSE-1.0 in the LICENSE.
 *
 * Project Name:
 *
 *      IndoorNavigation
 *
 * File Description:
 * 
 *      
 *      
 * Version:
 *
 *      1.0.0, 20190719
 * 
 * File Name:
 *
 *      IBeaconClient.cs
 *
 * Abstract:
 *
 *      This file will call the bluetooth scanning code in both
 *      IOS and Android. In IBeaconClient.cs, we will store all the
 *      signals we get in the buffer. And we will cancel some signals
 *      that their threshold are too low. Yhen we calculate the threshold
 *      and get the average.
 *
 * Authors:
 *
 *      Eric Lee, ericlee@iis.sinica.edu.tw
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using IndoorNavigation.Models;
using IndoorNavigation.Utilities;
using Xamarin.Essentials;
using Xamarin.Forms;
using static IndoorNavigation.Utilities.Constants;
namespace IndoorNavigation.Modules.IPSClients
{
    class IBeaconClient : IIPSClient
    {
        private List<WaypointBeaconsMapping> _detectedWaypointsList =
            new List<WaypointBeaconsMapping>();
        private List<WaypointBeaconsMapping> _monitorWaypointsList =
            new List<WaypointBeaconsMapping>();

        private object _bufferLock;

        private readonly EventHandler _beaconScanEventHandler;
        public NavigationEvent _event { get; private set; }

        private const int _moreThanTwoIBeacon = 2;
        private List<BeaconSignalModel> _beaconSignalBuffer =
            new List<BeaconSignalModel>();

        private int rssiOption;

        private System.Diagnostics.Stopwatch watch =
            new System.Diagnostics.Stopwatch();
        public IBeaconClient()
        {
            Console.WriteLine("In Ibeacon Type");

            _event = new NavigationEvent();
            Utility._ibeaconScan =
                DependencyService.Get<IBeaconScan>();

            _bufferLock = new object();
            _beaconScanEventHandler =
                new EventHandler(HandleBeaconScan);

            Utility._ibeaconScan._event._eventHandler +=
                _beaconScanEventHandler;

            rssiOption = 0;

            watch.Start();
        }
        public void SetWaypointList
            (List<WaypointBeaconsMapping> DetectedWaypointsList,
            List<WaypointBeaconsMapping> MonitorWaypointsList)
        {
            rssiOption = TmperorayStatus.RssiOption;
            _detectedWaypointsList = DetectedWaypointsList;
            _monitorWaypointsList = MonitorWaypointsList;
            Utility._ibeaconScan.StartScan();
        }

        public void DetectWaypoints()
        {
            Console.WriteLine(">>In DetectWaypoints IBeacon");
            List<BeaconSignalModel> removeSignalBuffer =
                new List<BeaconSignalModel>();

            lock (_bufferLock)
            {
                removeSignalBuffer.AddRange(
                   _beaconSignalBuffer.Where(c =>
                   c.Timestamp < DateTime.Now.AddMilliseconds(-1500)));

                foreach (var obsoleteBeaconSignal in removeSignalBuffer)
                    _beaconSignalBuffer.Remove(obsoleteBeaconSignal);

                Dictionary<RegionWaypointPoint, List<BeaconSignal>>
                    scannedData = new Dictionary
                    <RegionWaypointPoint, List<BeaconSignal>>();

                Dictionary<RegionWaypointPoint, int> signalAvgValue =
                    new Dictionary<RegionWaypointPoint, int>();

                Dictionary<RegionWaypointPoint, List<BeaconSignal>>
                    correctData =
                    new Dictionary<RegionWaypointPoint, List<BeaconSignal>>();


                if (watch.Elapsed.TotalMilliseconds >= BEACON_DETECTED_CLOCK_RESET_TIME)
                {
                    WatchReset();
                    Utility._ibeaconScan.StopScan();
                    Utility._ibeaconScan.StartScan();

                }


                //In ibsclient, a waypoint has at least two beacon UUIDs,
                //We put all waypoint we get in scannedData               

                //Mapping the interested beacon and cancel the beacon which 
                //has too low threshold
                foreach (BeaconSignalModel beacon in _beaconSignalBuffer)
                {
                    foreach (WaypointBeaconsMapping waypointBeaconsMapping in
                        _detectedWaypointsList)
                    {
                        foreach (Guid beaconGuid in
                            waypointBeaconsMapping._Beacons)
                        {
                            if ((beacon.UUID.Equals(beaconGuid)) &&
                                (beacon.RSSI >
                                (waypointBeaconsMapping
                                ._BeaconThreshold[beacon.UUID] - rssiOption)))
                            {
                                if (!scannedData.Keys
                                    .Contains
                                    (waypointBeaconsMapping
                                    ._WaypointIDAndRegionID))
                                {
                                    scannedData.Add
                                        (waypointBeaconsMapping
                                        ._WaypointIDAndRegionID,
                                        new List<BeaconSignal> { beacon });
                                }
                                else
                                {
                                    scannedData[waypointBeaconsMapping
                                        ._WaypointIDAndRegionID].Add(beacon);
                                }
                            }
                        }
                    }
                }

                // Make sure we have got more than two interested guid in each 
                // waypoint
                foreach (KeyValuePair<RegionWaypointPoint, List<BeaconSignal>>
                    interestedBeacon in scannedData)
                {
                    Dictionary<Guid, List<BeaconSignal>> tempSave =
                        new Dictionary<Guid, List<BeaconSignal>>();

                    foreach (BeaconSignal beaconSignal in interestedBeacon.Value)
                    {

                        if (!tempSave.Keys.Contains(beaconSignal.UUID))
                        {
                            tempSave.Add(beaconSignal.UUID,
                                new List<BeaconSignal> { beaconSignal });
                        }
                        else
                        {
                            tempSave[beaconSignal.UUID].Add(beaconSignal);
                        }
                    }
                    if (tempSave.Keys.Count() >= _moreThanTwoIBeacon)
                    {
                        correctData.Add(interestedBeacon.Key,
                            interestedBeacon.Value);
                    }

                }

                foreach (KeyValuePair<RegionWaypointPoint, List<BeaconSignal>>
                    calculateData in correctData)
                {
                    //If a waypoint has at least two beacon UUIDs,
                    //this waypoint might be our interested waypoint.

                    if (calculateData.Value.Count() >= _moreThanTwoIBeacon)
                    {
                        Dictionary<Guid, List<int>> saveEachBeacons =
                            new Dictionary<Guid, List<int>>();
                        //Sort the beacons by their Rssi
                        calculateData.Value.Sort((x, y) =>
                        { return x.RSSI.CompareTo(y.RSSI); });
                        int avgSignal = 0;
                        //int averageSignal = 0;
                        //List<int> signalOfEachBeacon = new List<int>();
                        //If we have more than ten data, we remove the highest 
                        //10% and the lowest 10%, and calculate their average
                        //If we have not more than 10 data,
                        //we just calculate their average
                        if (calculateData.Value.Count() >= 10)
                        {
                            int min = Convert.ToInt32(scannedData.Count() * 0.1);
                            int max = Convert.ToInt32(scannedData.Count() * 0.9);
                            int minus = max - min;

                            for (int i = min; i < max; i++)
                            {
                                avgSignal += calculateData.Value[i].RSSI;
                            }
                            avgSignal = avgSignal / minus;
                        }
                        else
                        {
                            foreach (BeaconSignal value in calculateData.Value)
                            {
                                avgSignal += value.RSSI;
                            }
                            avgSignal = avgSignal / calculateData.Value.Count();

                        }
                        signalAvgValue.Add(calculateData.Key, avgSignal);
                    }
                }

                int tempValue = -100;
                bool haveThing = false;
                RegionWaypointPoint possibleRegionWaypoint =
                    new RegionWaypointPoint();
                // Compare all data we have, and get the highest Rssi Waypoint 
                // as our interested waypoint
                foreach (KeyValuePair<RegionWaypointPoint, int>
                    calculateMax in signalAvgValue)
                {
                    if (tempValue < calculateMax.Value)
                    {
                        possibleRegionWaypoint = new RegionWaypointPoint();
                        possibleRegionWaypoint = calculateMax.Key;
                        haveThing = true;
                    }
                    tempValue = calculateMax.Value;
                }

                if (haveThing == true)
                {
                    WatchReset();
                    Console.WriteLine("Matched IBeacon");
                    _event.OnEventCall(new WaypointSignalEventArgs
                    {
                        _detectedRegionWaypoint = possibleRegionWaypoint
                    });
                    return;
                }
            }
            Console.WriteLine("<< In DetectWaypoints IBeacon");
        }

        public void MonitorWaypoints() { }

        public void DetectWaypointRssi(WaypointBeaconsMapping mapping)
        {
            Console.WriteLine(">>In DetectWaypoints IBeacon");
            List<BeaconSignalModel> removeSignalBuffer =
                new List<BeaconSignalModel>();

            lock (_bufferLock)
            {
                removeSignalBuffer.AddRange(
                   _beaconSignalBuffer.Where(c =>
                   c.Timestamp < DateTime.Now.AddMilliseconds(-1500)));

                foreach (var obsoleteBeaconSignal in removeSignalBuffer)
                    _beaconSignalBuffer.Remove(obsoleteBeaconSignal);

                Dictionary<RegionWaypointPoint, List<BeaconSignal>>
                    scannedData =
                    new Dictionary<RegionWaypointPoint, List<BeaconSignal>>();

                Dictionary<RegionWaypointPoint, int> signalAvgValue =
                    new Dictionary<RegionWaypointPoint, int>();

                Dictionary<RegionWaypointPoint, List<BeaconSignal>>
                    correctData =
                    new Dictionary<RegionWaypointPoint, List<BeaconSignal>>();


                if (watch.Elapsed.TotalMilliseconds >= BEACON_DETECTED_CLOCK_RESET_TIME)
                {
                    WatchReset();
                    Utility._ibeaconScan.StopScan();
                    Utility._ibeaconScan.StartScan();

                }


                foreach (BeaconSignalModel singalModel in _beaconSignalBuffer)
                {
                    foreach (Guid beaconGuid in mapping._Beacons)
                    {
                        if (singalModel.UUID.Equals(beaconGuid) &&
                            singalModel.RSSI >
                            mapping._BeaconThreshold[singalModel.UUID])
                        {
                            Console.WriteLine("Mapping iBeacon!");

                            WatchReset();
                            _event.OnEventCall(new WaypointRssiEventArgs
                            {
                                _scanBeaconRssi = singalModel.RSSI,
                                _BeaconThreshold =
                                    mapping._BeaconThreshold[singalModel.UUID]
                            });
                            return;
                        }
                    }
                }
            }
            Console.WriteLine("<< In DetectWaypoints IBeacon");
        }

        private void WatchReset()
        {
            watch.Stop();
            watch.Reset();
            watch.Start();
        }

        private void HandleBeaconScan(object sender, EventArgs e)
        {
            IEnumerable<BeaconSignalModel> signals =
                (e as BeaconScanEventArgs)._signals;

            foreach (BeaconSignalModel signal in signals)
            {
                Console.WriteLine("Detected Beacon UUID : " + signal.UUID + " RSSI = " + signal.RSSI);
            }

            lock (_bufferLock)
                _beaconSignalBuffer.AddRange(signals);

        }

        public void OnRestart()
        {
            watch.Start();
            Utility._ibeaconScan.StartScan();
            Utility._ibeaconScan._event._eventHandler +=
                _beaconScanEventHandler;
        }
        public void Stop()
        {
            _bufferLock = new object();
            Utility._ibeaconScan.StopScan();
            _beaconSignalBuffer.Clear();
            _detectedWaypointsList.Clear();
            Utility._ibeaconScan._event._eventHandler -=
                _beaconScanEventHandler;
            watch.Stop();
        }

    }
}

