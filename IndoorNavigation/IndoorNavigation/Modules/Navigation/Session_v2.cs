using IndoorNavigation.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IndoorNavigation.Modules.IPSClients;
using IndoorNavigation.Models.NavigaionLayer;

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
        private IPSModule _IPSModule;
        private ManualResetEventSlim _nextWaypointEvent;
        private ManualResetEventSlim _pauseNavigationEvent;
        private NavigationGraph_v2 _navigaionGraph;
        public NavigationEvent _event { get; private set; }

        public Session_v2(NavigationGraph_v2 navigationGraph, Guid destinationRegionID, Guid destinationWaypointID)
        {
            _event = new NavigationEvent();
            _navigaionGraph = navigationGraph;
            _destinationRegionID = destinationRegionID;
            _destinationWaypointID = destinationWaypointID;
            _isKeepDetecting = true;
            _nextWaypointStep = -1;

            _IPSModule._event._eventHandler += new EventHandler(CheckArrivedWaypoint);

            _waypointDetectionThread = new Thread(() => InvokeIPSWork());
            _waypointDetectionThread.Start();

            _navigationControlThread = new Thread(() => NavigatorProgram());
            _navigationControlThread.Start();
        }

        private bool isArrivedDestination()
        {
            return _currentRegionID.Equals(_destinationRegionID) && _currentWaypointID.Equals(_destinationWaypointID));
        }
        private void NavigatorProgram()
        {
            _nextWaypointStep = -1;
            _currentRegionID = new Guid();
            _currentWaypointID = new Guid();

            while (_isKeepDetecting && !isArrivedDestination())
            {
                // to wait for arriving next waypoint event.
                // the thread pause.
                _pauseNavigationEvent.Wait();
                _nextWaypointEvent.Wait();

                if (isArrivedDestination())
                {
                    _isKeepDetecting = false;
                    _IPSModule.CloseAllActiveClient();
                    break;
                }

                if (_nextWaypointStep == -1) { }
                else if (true) // when user arrive at next waypoint at route.
                { }
                else if (true) // when user arrive at wrong way.
                { }
                _nextWaypointEvent.Reset();
            }
        }

        // for set up ips client switch.
        private void NavigateToNextWaypoint(int nextStep)
        {
            Console.WriteLine("next Step in local : " + nextStep);
            Console.WriteLine("nextStep in global : " + _nextWaypointStep);
            if (nextStep == -1) // when user is at initial step. to scan all beacon.
            {
                _IPSModule.InitialStep_DetectAllBeacon(_navigaionGraph.GetAllRegionIDs());
                _IPSModule.SetMonitorBeaconList(nextStep);
            }
            else
            {
                Console.WriteLine("NavigateToNextWaypoint : " + nextStep);

                RegionWaypointPoint checkPoint = _navigaionGraph.getWaypointOnRoute(nextStep);

                _IPSModule.AddMonitorBeacon(checkPoint._regionID, checkPoint._waypointID);
                //TODO　to think of how to reduce the codes.
                if (_nextWaypointStep + 1 < _navigaionGraph.getWaypointsCountOnRoute() && _navigaionGraph.getWaypointOnRoute(_nextWaypointStep + 1)._regionID == _currentRegionID)
                {
                    RegionWaypointPoint nextnextWaypoint = _navigaionGraph.getWaypointOnRoute(_nextWaypointStep + 1);
                    _IPSModule.AddMonitorBeacon(nextnextWaypoint._regionID, nextnextWaypoint._waypointID);
                }
                
                if (_nextWaypointStep >= 1)
                {
                    if (_navigaionGraph.getWrongWaypoints(_navigaionGraph.getWaypointOnRoute(_nextWaypointStep - 1)) != null)
                    {
                        
                    }
                }
            }
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
