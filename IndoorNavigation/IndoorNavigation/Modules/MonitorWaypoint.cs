using System;
using System.Collections.Generic;
using System.Text;

namespace IndoorNavigation.Modules
{
    // <summary>
    // the class is for user to monitor the beacon in the bus, 
    // and keep user not to leave the bus stop.
    // </summary>
    public class MonitorWaypoint:IDisposable
    {

        #region IDisposable support
        private bool disposed = false;
        protected virtual void Dispose(bool disposing) 
        {
            if (!disposed)
            {
                if (disposing)
                {
                    
                }
                disposed = true;
            }
        }
        public void Dispose() 
        {
            Dispose(true);
        }

        #endregion
    }
}
