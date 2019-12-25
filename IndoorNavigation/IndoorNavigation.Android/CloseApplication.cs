using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IndoorNavigation;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
namespace IndoorNavigation.Droid
{
    class CloseApplication:ICloseApplication
    {
        [Obsolete]
        public void closeApplication()
        {
            //Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
            //var activity = (Activity)Forms.Context;
            //activity.FinishAffinity();
           
        }
    }
}