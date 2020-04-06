using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
namespace NavigatorStruture
{
    // it's in User Interface Layer
    public class Session
    {
        #region Properties
        private List<Instruction> _instructions;
        private Thread _navigationThread;
        private RegionGraph _regionGraph;
        #endregion
       

        #region  Methods
        private void RequestNotificationOnArrival() { }
        //todo : Request PS Interface to 
        private void CheckArrivedWaypoint(object sender, EventArgs args) { }
        //todo : Read the Event from PS Interface to check arrived waypoint or not.        
        private void NavigationProgram() { }
        //todo : the function for navigationThread work to do the UI update, destination detect, wrong way detect.
        #endregion
        

        #region Constructors
        public Session(Guid DestinationWaypointID, Guid DestinationRegionID) { }
        //will be called by Viewmodel.
        #endregion
    }
   
}
