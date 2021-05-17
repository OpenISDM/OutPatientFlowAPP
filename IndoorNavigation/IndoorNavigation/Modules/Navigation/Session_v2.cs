using IndoorNavigation.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IndoorNavigation.Modules.IPSClients;
using IndoorNavigation.Models.NavigaionLayer;
using static IndoorNavigation.Modules.Session;

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

        private int TotalProgress = 0;
        private int CurrentProgress = 0;

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
            return _currentRegionID.Equals(_destinationRegionID) && _currentWaypointID.Equals(_destinationWaypointID);
        }

        //TODO Need to consider bus stop scenario.
        private void NavigatorProgram()
        {
            _nextWaypointStep = -1;
            _currentRegionID = new Guid();
            _currentWaypointID = new Guid();
            RegionWaypointPoint currentWaypoint = new RegionWaypointPoint();

            while (_isKeepDetecting && !isArrivedDestination())
            {
                // to wait for arriving next waypoint event.
                // the thread pause.
                _pauseNavigationEvent.Wait();
                _nextWaypointEvent.Wait();

                currentWaypoint._regionID = _currentRegionID;
                currentWaypoint._waypointID = _currentWaypointID;

                if (isArrivedDestination())
                {
                    _isKeepDetecting = false;
                    _IPSModule.CloseAllActiveClient();
                    break;
                }

                if (_nextWaypointStep == -1)
                {
                    _navigaionGraph.GenerateRoute(_currentRegionID, _currentWaypointID, _destinationRegionID, _destinationWaypointID);

                    _nextWaypointStep++;
                    NavigateToNextWaypoint(_nextWaypointStep);
                }
                else if (_navigaionGraph.getWaypointOnRoute(_nextWaypointStep).Equals(currentWaypoint)) // when user arrive at next waypoint at route.
                {
                    Console.WriteLine("Arrived region/waypoint : {0}/{1}", _currentRegionID, _currentWaypointID);

                    _nextWaypointStep++;
                    NavigateToNextWaypoint(_nextWaypointStep);
                }
                //TODO need a condition to prevent it from calling duplicately.
                else if (_nextWaypointStep >= 1 && _navigaionGraph.isGetWrongWay(_nextWaypointStep - 1, currentWaypoint)) // when user arrive at wrong way.
                {
                    Console.WriteLine("Wrong way");

                    _navigaionGraph.GenerateRoute(_currentRegionID, _currentWaypointID, _destinationRegionID, _destinationWaypointID);
                }
                _nextWaypointEvent.Reset();
            }
        }

        // for set up ips client switch.
        private void NavigateToNextWaypoint(int nextStep)
        {
            //TODO this is for check the "nextStep" and "_nextWaypointStep" value.
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

                if (_nextWaypointStep >= 1 && _navigaionGraph.getWrongWaypoints(_navigaionGraph.getWaypointOnRoute(_nextWaypointStep - 1)) != null)
                {
                    foreach (RegionWaypointPoint waypoint in _navigaionGraph.getWrongWaypoints(_navigaionGraph.getWaypointOnRoute(_nextWaypointStep - 1)))
                    {
                        _IPSModule.AddMonitorBeacon(waypoint._regionID, waypoint._waypointID);
                    }
                }

                _IPSModule.SetMonitorBeaconList(nextStep);
            }
        }

        private void InvokeIPSWork() { }
        public void CheckArrivedWaypoint(object sender, EventArgs e)
        {
            RegionWaypointPoint currentWaypoint = (e as WaypointSignalEventArgs)._detectedRegionWaypoint;

            _currentRegionID = currentWaypoint._regionID;
            _currentWaypointID = currentWaypoint._waypointID;

            NavigationInstruction navigationInstruction = new NavigationInstruction();

            if (isArrivedDestination())
            {
                //send arrive destination event

                _event.OnEventCall(new NavigationEventArgs
                {
                    _result = NavigationResult.Arrival
                }) ;
            }
            //next
            else if(_nextWaypointStep > 0 && _navigaionGraph.getWaypointOnRoute(_nextWaypointStep).Equals(currentWaypoint))
            {
                //setup progress.

                //
            }
            // next next
            else if (_nextWaypointStep +1 < _navigaionGraph.getWaypointsCountOnRoute()) 
            {
                if (_navigaionGraph.getWaypointOnRoute(_nextWaypointStep + 1).Equals(currentWaypoint))
                {

                }
            }
            
            _nextWaypointEvent.Set();
        }

        private void setInstruction(NavigationInstruction instruction, RegionWaypointPoint currentWaypoint, NavigationResult result)
        {
            instruction = new NavigationInstruction();

            instruction._currentWaypointName = _navigaionGraph.GetWaypointName(currentWaypoint);
            instruction._progress = CurrentProgress;
            instruction._progressBar = string.Format($"{CurrentProgress}/{TotalProgress}");
            _event.OnEventCall(new NavigationEventArgs
            {
                _result = result,
                _nextInstruction = instruction
            });

        }

        public void PauseSession() { }
        public void ResumeSession() { }
        public void CloseSession() { }

    }
}
