using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using IndoorNavigation.Model;
using IndoorNavigation.Navigation;
using IndoorNavigation.PS;
namespace IndoorNavigation.UI
{
    // it's in User Interface Layer
    public class Session
    {
        #region Properties
        private List<Instruction> _instructions;
        private Thread _navigationThread;
        private RegionGraph _regionGraph;
        private PSInterface _psInterface;
        private int _step = -1;
        #endregion
       

        #region  Methods
        private void RequestNotificationOnArrival() { }
        //todo : Request PS Interface to 
        private void CheckArrivedWaypoint(object sender, EventArgs args) { }
        //todo : Read the Event from PS Interface to check arrived waypoint or not.        
        private void NavigationProgram() { }
        //todo : the function for navigationThread work to do the UI update, destination detect, wrong way detect.

        
        
        public void CloseSession() 
        {
            _navigationThread.Abort();
            _psInterface.ClosePSInterface();
        }
        #endregion
        

        #region Constructors
        public Session(Guid DestinationWaypointID, Guid DestinationRegionID) { }
        //will be called by Viewmodel.
        #endregion

        #region Classes

        public class NavigationArgs : EventArgs
        {
            
        }

        #endregion
    }

}
