using IndoorNavigation.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace IndoorNavigation.Modules.Navigation
{
    public class Session_v2
    {
        private int _nextWaypointStep = -1;

        public Session_v2() { }

        private void NavigatorProgram()
        {

        }

        private void NavigateToNextWaypoint() { }
        private void InvokeIPSWork() { }
        public void CheckArrivedWaypoint(object sender, EventArgs e)
        {
            if (_nextWaypointStep == -1) { }
            else { }
        }

        public void PauseSession() { }
        public void ResumeSession() { }
        public void CloseSession() { }

        public enum NavigationResult
        {
            Run = 0,
            Arrival,
            NoRoute,
            ArriveVirtualPoint,
            ArriveIgnorPoint,
        }
        public class NavigationInstruction
        {
            public string _currentWaypointName;
            public RegionWaypointPoint _nextWaypoint;
            public RegionWaypointPoint _currentWaypoint;
            public RegionWaypointPoint _previousWaypoint;

        }
    }
}
