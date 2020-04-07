using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using IndoorNavigation.Model;

namespace IndoorNavigation.UI
{
    // it's in User Interface Layer
    class NavigatorPageViewModel
    {
        #region Properties
        private Session _session;
        #endregion       

        #region  Methods
        private void StartToNavigate() { }

        private void Stop()
        {
            _session.CloseSession();
        }

        //it will recieve the data from Session.
        private void NavigationResultHandler(object sender, EventArgs args)
        {
            
        }
        #endregion

        #region Constructor
        public NavigatorPageViewModel(RegionWaypointPoint DestinationWaypoint) { }
        #endregion
    }
}
