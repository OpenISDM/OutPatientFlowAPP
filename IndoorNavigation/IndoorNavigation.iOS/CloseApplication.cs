using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using Foundation;
using UIKit;
using System.Threading.Tasks;
using System.Threading;

namespace IndoorNavigation.iOS
{
    class CloseApplication:ICloseApplication
    {
        //Thread.CurrentThread.Abort();
        public void closeApplication()
        {
            System.Diagnostics.Process.GetCurrentProcess().CloseMainWindow();
        }
       
    }
}