using System;
using System.Collections.Generic;
using System.Text;

using IndoorNavigation.Models;
using IndoorNavigation.Modules.IPSClients;
using Xamarin.Essentials;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using System.Diagnostics;
namespace IndoorNavigation.Modules.IPSClients
{
    public class GPSClient : IIPSClient
    {

        public NavigationEvent _event { get; private set; }

        #region Variables and Objects
        private List<WaypointBeaconsMapping> _waypointBeaconList;
        private object _bufferLock;
        private Stopwatch watch;

        // a const for reset watch, wait for test it we need to set it how much.
        private const int _clockResetTime = 1;
        private Position _currentPosition = null;        

        #endregion

        public GPSClient() 
        {
            _event = new NavigationEvent();
            _bufferLock = new object();
            watch.Start();
        }
        public void SetWaypointList(List<WaypointBeaconsMapping> WaypointList) 
        {
            Console.WriteLine(">>GPS client : SetWaypointList");
            this._waypointBeaconList = WaypointList;           
            Console.WriteLine("<<GPS client : SetWaypointList");
        }
        public void DetectWaypoints() 
        {
            Console.WriteLine(">>DetectWaypoints");

            lock (_bufferLock)
            {                
                foreach(WaypointBeaconsMapping mapping in _waypointBeaconList)
                {
                    foreach(Guid beaconGuid in mapping._Beacons)
                    {
                        // if signal meet the coverage, send arrivesd event
                        // I have not came up with the statement.
                        if (true)
                        {

                            _event.OnEventCall(new WaypointSignalEventArgs
                            {
                                _detectedRegionWaypoint =
                                    mapping._WaypointIDAndRegionID
                            }) ;
                        }
                    }
                }
            }

            Console.WriteLine("<<DetectWaypoints");
        }

        //this item for Beacon rssi adjustment, it's useless here.
        public void DetectWaypointRssi(WaypointBeaconsMapping mapping) 
        {
            Console.WriteLine(">>DetectWaypointRssi");

            //should do something to prevent user stand at GPS environment.

            Console.WriteLine("<<DetectWaypointRssi");
        }    
        
        private void PositionChanged(object sender, PositionEventArgs e)
        {
            Console.WriteLine(">>GPS Client : PositionChanged");

            lock (_bufferLock)
            {
                _currentPosition = e.Position;
            }
            Console.WriteLine("<<GPS Client : PositionChanged");
        }

        

        private void WatchReset()
        {
            watch.Stop();
            watch.Reset();
            watch.Start();
        }

        public void Stop() 
        {
            _waypointBeaconList.Clear();            
        }
        public void OnRestart() 
        {

        }

        async public void TestGps()
        {
            Console.WriteLine(">>Test Gps");

            try
            {
                var location = await Geolocation.GetLocationAsync();

                if(location != null)
                {
                    Console.WriteLine("Lat : {0}, lon:{1}, alt:{2}",
                        location.Latitude, location.Longitude, location.Altitude);
                }
            }
            catch(Exception exc)
            {
                Console.WriteLine("get Gps error - " + exc.Message);
            }

            Console.WriteLine("<<Test Gps");
        }

        
    }
}
