using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IndoorNavigation.Models;
using IndoorNavigation.Models.NavigaionLayer;
using IndoorNavigation.Modules.Navigation;
using Xamarin.Forms;
using System.Xml;
namespace IndoorNavigation.Modules
{
    public class NavigationModule_v2 : IDisposable
    {
        private Session_v2 _session;
        private string _navigationGraphName;
        private Guid _destinationRegionID;
        private Guid _destinationWaypointID;

        public NavigationEvent _event { get; private set; }

        public NavigationModule_v2(string navigationGraphName, Guid destinationRegionID, Guid destinationWaypointID)
        {
            _event = new NavigationEvent();

            _navigationGraphName = navigationGraphName;
            _destinationRegionID = destinationRegionID;
            _destinationWaypointID = destinationWaypointID;

            ConstructSession();
        }

        private void ConstructSession()
        {
            //to decide the avoid type.
            List<ConnectionType> avoidList = new List<ConnectionType>();

            Console.WriteLine("-- setup preference --- ");
            if (Application.Current.Properties.ContainsKey("AvoidStair"))
            {
                avoidList.Add(
                    (bool)Application.Current.Properties["AvoidStair"] ?
                    ConnectionType.Stair : ConnectionType.NormalHallway);
                avoidList.Add(
                    (bool)Application.Current.Properties["AvoidElevator"] ?
                    ConnectionType.Elevator : ConnectionType.NormalHallway);
                avoidList.Add(
                    (bool)Application.Current.Properties["AvoidEscalator"] ?
                    ConnectionType.Escalator : ConnectionType.NormalHallway);

                avoidList = avoidList.Distinct().ToList();
                avoidList.Remove(ConnectionType.NormalHallway);
            }
            //TODO : To complete the NavigationGraph constructor.
            _session = new Session_v2(new NavigationGraph_v2(), _destinationRegionID, _destinationWaypointID);
            _session._event._eventHandler += new EventHandler(HandleNavigationResult);
        }

        private void HandleNavigationResult(object sender, EventArgs args)
        {
            _event.OnEventCall(args);
        }

        public void onStop() => _session.CloseSession(); 
        public void onPause() => _session.PauseSession();
        public void onResume() => _session.ResumeSession();

        #region IDisposable Support
        private bool disposed = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                }
                // Free unmanaged resources (unmanaged objects) and override a 
                // finalizer below. 
                // Set large fields to null.
                _session._event._eventHandler -=
                    new EventHandler(HandleNavigationResult);

                disposed = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
