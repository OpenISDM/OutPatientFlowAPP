using System;
using System.Collections.Generic;
using System.Text;

namespace IndoorNavigation.Models
{
    public class WaypointSignalEventArgs : EventArgs
    {
        public RegionWaypointPoint _detectedRegionWaypoint { get; set; }
        public SignalMode _signalMode { get; set; }
    }
    public class WaypointRssiEventArgs : EventArgs
    {
        public int _scanBeaconRssi { get; set; }
        public int _BeaconThreshold { get; set; }
    }

    public enum SignalMode
    {
        Detecting = 0,
        Monitor
    }
}
