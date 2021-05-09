using IndoorNavigation.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IndoorNavigation.Modules.Navigation
{
    public class Session_v2
    {
        private int _nextWaypointStep;
        private Guid _currentWaypointID, _currentRegionID;
        private Guid _destinationWaypointID, _destinationRegionID;
        private bool _isKeepDetecting;
        private Thread _waypointDetectionThread;
        private Thread _navigationControlThread;

        private ManualResetEventSlim _nextWaypointEvent;
        private ManualResetEventSlim _pauseNavigationEvent;
        public NavigationEvent _event { get; private set; }
        private NavigationGraph_v2 _navigaionGraph;

        public Session_v2() { }

        private void NavigatorProgram()
        {
            _nextWaypointStep = -1;
            _currentRegionID = new Guid();
            _currentWaypointID = new Guid();

        }

        private void NavigateToNextWaypoint(int nextStep)
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
