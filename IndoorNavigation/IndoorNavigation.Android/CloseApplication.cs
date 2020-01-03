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
using Java.Lang;
using IndoorNavigation.Droid;

[assembly: Xamarin.Forms.Dependency(typeof(CloseApplication))]
namespace IndoorNavigation.Droid
{
    class CloseApplication:ICloseApplication
    {        
        public void closeApplication()
        {
            //Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
            //var activity = (Activity)Forms.Context;
            //activity.FinishAffinity();
            Activity activity = (Activity)Android.App.Application.Context;
            activity.FinishAffinity();
            JavaSystem.Exit(0);
        }
    }
}