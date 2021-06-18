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
 *      WaypointClient.cs
 *
 * Abstract:
 *
 *      This file will call the bluetooth scanning code in both
 *      IOS and Android. In Waypoint.cs, we match the LBeacon.
 *      We will sort the guid we get and check if the beacon guid which has the
 *      strongest threshold is our interested beacon or not, if not check the
 *      second strongest threshold
 *
 * Authors:
 *
 *      Kenneth Tang, kenneth@gm.nssh.ntpc.edu.tw
 *      Paul Chang, paulchang@iis.sinica.edu.tw
 *      Bo-Chen Huang, m10717004@yuntech.edu.tw
 *      Chun-Yu Lai, chunyu1202@gmail.com
 *      Eric Lee, ericlee@iis.sinica.edu.tw
 *
 */
using System;
using System.Linq;
using System.Collections.Generic;
using IndoorNavigation.Models;
using Xamarin.Forms;
using static IndoorNavigation.Utilities.TmperorayStatus;
using static IndoorNavigation.Utilities.Constants;

namespace IndoorNavigation.Modules.IPSClients
{
    class WaypointClient : IIPSClient
    {
        private List<WaypointBeaconsMapping> _detectedWaypointsList =
            new List<WaypointBeaconsMapping>();
        private List<WaypointBeaconsMapping> _monitoryWaypointsList =
            new List<WaypointBeaconsMapping>();

        private object _bufferLock;
        private readonly EventHandler _beaconScanEventHandler;
        public NavigationEvent _event { get; private set; }
        private List<BeaconSignalModel> _beaconSignalBuffer =
            new List<BeaconSignalModel>();

        private int rssiOption;
        private System.Diagnostics.Stopwatch watch =
            new System.Diagnostics.Stopwatch();

        public WaypointClient()
        {
            _event = new NavigationEvent();
            Utility._lbeaconScan = DependencyService.Get<LBeaconScan>();
            _beaconScanEventHandler = new EventHandler(HandleBeaconScan);
            Utility._lbeaconScan._event._eventHandler +=
                _beaconScanEventHandler;

            rssiOption = 0;
            _bufferLock = new object();
            watch.Start();
        }
        public void SetWaypointList
            (List<WaypointBeaconsMapping> DetectedWaypointsList, 
            List<WaypointBeaconsMapping> MonitorWaypointsList)
        {
            Console.WriteLine(">>WaypointClient : SetWaypointList");

            rssiOption = RssiOption;
            _detectedWaypointsList = DetectedWaypointsList;
            _monitoryWaypointsList = MonitorWaypointsList;            
            Utility._lbeaconScan.StartScan();
            Console.WriteLine("<<WaypointClient : SetWaypointList");
        }

        public void DetectWaypoints()
        {
            Console.WriteLine(">> In DetectWaypoints LBeacon");
            // Remove the obsolete data from buffer
            List<BeaconSignalModel> removeSignalBuffer =
            new List<BeaconSignalModel>();

            lock (_bufferLock)
            {
                removeSignalBuffer.AddRange(
                _beaconSignalBuffer.Where(c =>
                c.Timestamp < DateTime.Now.AddMilliseconds(-500)));

                if (watch.Elapsed.TotalMilliseconds >= BEACON_DETECTED_CLOCK_RESET_TIME)
                {
                    WatchReset();
                    Utility._lbeaconScan.StopScan();
                    Utility._lbeaconScan.StartScan();
                }

                foreach (var obsoleteBeaconSignal in removeSignalBuffer)
                    _beaconSignalBuffer.Remove(obsoleteBeaconSignal);

                //Sort beacons through their RSSI, to let the stronger beacon 
                //can get in first
                _beaconSignalBuffer.Sort((x, y) =>
                { return y.RSSI.CompareTo(x.RSSI); });

                foreach (BeaconSignalModel beacon in _beaconSignalBuffer)
                {
                    foreach (WaypointBeaconsMapping waypointBeaconsMapping in _detectedWaypointsList)
                    {
                        foreach (Guid beaconGuid in waypointBeaconsMapping._Beacons)
                        {
                            if (beacon.UUID.Equals(beaconGuid))
                            {
                                if (beacon.RSSI >
                                    (waypointBeaconsMapping._BeaconThreshold
                                    [beacon.UUID] - rssiOption))
                                {
                                    WatchReset();
                                    _event.OnEventCall
                                        (new WaypointSignalEventArgs
                                        {
                                            _detectedRegionWaypoint =
                                        waypointBeaconsMapping
                                        ._WaypointIDAndRegionID
                                        });
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            Console.WriteLine("<< In DetectWaypoints LBeacon");
        }

        public void MonitorWaypoints() { }

        public void DetectWaypointRssi(WaypointBeaconsMapping mapping)
        {
            Console.WriteLine(">>In DetectWaypoint LBeacon Rssi");
            List<BeaconSignalModel> removeSignalBuffer =
           new List<BeaconSignalModel>();

            lock (_bufferLock)
            {
                removeSignalBuffer.AddRange(
                _beaconSignalBuffer.Where(c =>
                c.Timestamp < DateTime.Now.AddMilliseconds(-500)));

                if (watch.Elapsed.TotalMilliseconds >= BEACON_DETECTED_CLOCK_RESET_TIME)
                {
                    WatchReset();
                    Utility._lbeaconScan.StopScan();
                    Utility._lbeaconScan.StartScan();
                }

                foreach (var obsoleteBeaconSignal in removeSignalBuffer)
                    _beaconSignalBuffer.Remove(obsoleteBeaconSignal);

                _beaconSignalBuffer.Sort((x, y) =>
                    { return y.RSSI.CompareTo(x.RSSI); });
                foreach (BeaconSignalModel beacon in _beaconSignalBuffer)
                {
                    foreach (Guid beaconGuid in mapping._Beacons)
                    {
                        if (beacon.UUID.Equals(beaconGuid) &&
                            beacon.RSSI >
                            mapping._BeaconThreshold[beacon.UUID])
                        {
                            Console.WriteLine("Mapping LBeacon!");
                            WatchReset();
                            _event.OnEventCall(new WaypointRssiEventArgs
                            {
                                _scanBeaconRssi = beacon.RSSI,
                                _BeaconThreshold =
                                    mapping._BeaconThreshold[beacon.UUID]
                            });
                            return;
                        }
                    }
                }
            }
            Console.WriteLine("<< In DetectWaypoint LBeacon Rssi");
        }
        private void WatchReset()
        {
            watch.Stop();
            watch.Reset();
            watch.Start();
        }
        public void OnRestart()
        {
            watch.Start();
            Utility._lbeaconScan.StartScan();
            Utility._lbeaconScan._event._eventHandler +=
                _beaconScanEventHandler;
        }
        private void HandleBeaconScan(object sender, EventArgs e)
        {
            IEnumerable<BeaconSignalModel> signals =
            (e as BeaconScanEventArgs)._signals;

            lock (_bufferLock)
                _beaconSignalBuffer.AddRange(signals);
        }

        public void Stop()
        {
            Utility._lbeaconScan.StopScan();
            _beaconSignalBuffer.Clear();
            _detectedWaypointsList.Clear();
            Utility._lbeaconScan._event._eventHandler -=
                _beaconScanEventHandler;
            watch.Stop();
        }
    }
}
