using System;
using System.Collections.Generic;
using System.Text;

namespace NavigatorStruture
{
    // it's in User Interface Layer
    public class Session
    {
        #region Properties
        private List<Instruction> _instructions;
        #endregion
       

        #region  Methods
        private void RequestNotificationOnArrival() { }
        //todo : Request PS Interface to 
        private void CheckArrivedWaypoint(object sender, EventArgs args) { }
        //todo : Read the Event from PS Interface to check arrived waypoint or not.        
        #endregion
        

        #region Constructors
        public Session(Guid DestinationWaypointID, Guid DestinationRegionID) { }
        //will be called by Viewmodel.
        #endregion
    }
   
}
