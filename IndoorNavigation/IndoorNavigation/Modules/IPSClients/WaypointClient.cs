/*
 * Copyright (c) 2019 Academia Sinica, Institude of Information Science
 *
 * License:
 *      GPL 3.0 : The content of this file is subject to the terms and
 *      conditions defined in file 'COPYING.txt', which is part of this source
 *      code package.
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
using IndoorNavigation.Models.NavigaionLayer;
using System.Reflection;
using Xamarin.Essentials;
using IndoorNavigation.Utilities;

namespace IndoorNavigation.Modules.IPSClients
{
    class WaypointClient : IIPSClient
    {
        private List<WaypointBeaconsMapping> _waypointBeaconsList = 
            new List<WaypointBeaconsMapping>();

        private object _bufferLock;// = new object();
        private readonly EventHandler _beaconScanEventHandler;
        private const int _clockResetTime = 90000;
        public NavigationEvent _event { get; private set; }
        private List<BeaconSignalModel> _beaconSignalBuffer = 
            new List<BeaconSignalModel>();

        private int rssiOption;
        private System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

        public WaypointClient()
        {
            _event = new NavigationEvent();
            Utility._lbeaconScan = DependencyService.Get<LBeaconScan>();
            _beaconScanEventHandler = new EventHandler(HandleBeaconScan);
            Utility._lbeaconScan._event._eventHandler += _beaconScanEventHandler;
            _waypointBeaconsList = new List<WaypointBeaconsMapping>();
            rssiOption = 0;
            _bufferLock = new object();
            watch.Start();
        }        

        public void SetWaypointList(List<WaypointBeaconsMapping> waypointBeaconsList)
        {
            Console.WriteLine(">>WaypointClient : SetWaypointList");

            //if (Application.Current.Properties.ContainsKey("RSSI_Test_Adjustment"))
            //{
            //    //rssiOption =
            //    //    (int)Application.Current.Properties["RSSI_Test_Adjustment"];
            //    rssiOption = TmperorayStatus.RssiOption;
            //}
            rssiOption = TmperorayStatus.RssiOption;
            Console.WriteLine("rssi option in LbeaconClient =" + rssiOption);

            this._waypointBeaconsList = waypointBeaconsList;
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

                if (watch.Elapsed.TotalMilliseconds >= _clockResetTime)
                {
                    WatchReset();
                    Utility._lbeaconScan.StopScan();
                    Utility._lbeaconScan.StartScan();                   
                }

                foreach (var obsoleteBeaconSignal in removeSignalBuffer)
                    _beaconSignalBuffer.Remove(obsoleteBeaconSignal);

                //Sort beacons through their RSSI, to let the stronger beacon can get in first
                //_beaconSignalBuffer.Sort((x, y) => { return y.RSSI.CompareTo(x.RSSI); });
                _beaconSignalBuffer.Sort((x, y) => { return y.RSSI.CompareTo(x.RSSI); });
                //BeaconSignalModel beaconSignalModel = new BeaconSignalModel();
                //beaconSignalModel.UUID = new Guid("00000015-0000-2503-7431-000021564508");
                
                //_beaconSignalBuffer.Add(beaconSignalModel);

                foreach (BeaconSignalModel beacon in _beaconSignalBuffer)
                {
                    foreach (WaypointBeaconsMapping waypointBeaconsMapping in _waypointBeaconsList)
                    {
                        foreach (Guid beaconGuid in waypointBeaconsMapping._Beacons)
                        {
                            if (beacon.UUID.Equals(beaconGuid))
                            {
                                //Console.WriteLine("Matched waypoint: {0} by detected Beacon {1}",
                                //waypointBeaconsMapping._WaypointIDAndRegionID._waypointID,
                                //beaconGuid);
                                if (beacon.RSSI > (waypointBeaconsMapping._BeaconThreshold[beacon.UUID]-rssiOption))
                                {
                                    WatchReset();
                                    _event.OnEventCall(new WaypointSignalEventArgs
                                    {
                                        _detectedRegionWaypoint = waypointBeaconsMapping._WaypointIDAndRegionID
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
        
        public void DetectWaypointRssi(WaypointBeaconsMapping mapping)
        {
            Console.WriteLine(">>In DetectWaypoint LBeacon Rssi");
            Console.WriteLine("mapping cotent : " + mapping._Beacons[0]);
            Console.WriteLine("mapping aaa " + mapping._WaypointIDAndRegionID._waypointID);
            List<BeaconSignalModel> removeSignalBuffer =
           new List<BeaconSignalModel>();

            lock (_bufferLock)
            {
                removeSignalBuffer.AddRange(
                _beaconSignalBuffer.Where(c =>
                c.Timestamp < DateTime.Now.AddMilliseconds(-500)));

                if (watch.Elapsed.TotalMilliseconds >= _clockResetTime)
                {
                    WatchReset();
                    Utility._lbeaconScan.StopScan();
                    Utility._lbeaconScan.StartScan();
                }

                foreach (var obsoleteBeaconSignal in removeSignalBuffer)
                    _beaconSignalBuffer.Remove(obsoleteBeaconSignal);
               
                _beaconSignalBuffer.Sort((x, y) => 
                    { return y.RSSI.CompareTo(x.RSSI); });
                foreach(BeaconSignalModel beacon in _beaconSignalBuffer)
                {
                    //Console.WriteLine("beacon threshold " + beacon.UUID);
                    foreach(Guid beaconGuid in mapping._Beacons)
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
            //_bufferLock = new object();
            Utility._lbeaconScan.StopScan();
            _beaconSignalBuffer.Clear();
            _waypointBeaconsList.Clear();
            Utility._lbeaconScan._event._eventHandler -= _beaconScanEventHandler;
            watch.Stop();
        }
    }

    public class WaypointSignalEventArgs : EventArgs
    {
        public RegionWaypointPoint _detectedRegionWaypoint { get; set; }
    }

    public class WaypointRssiEventArgs : EventArgs
    {
        public int _scanBeaconRssi { get; set; }
        public int _BeaconThreshold { get; set; }
    }
}
