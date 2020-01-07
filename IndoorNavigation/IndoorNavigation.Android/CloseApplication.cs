using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Views;
using IndoorNavigation.Droid;
using Java.Lang;
using Plugin.Permissions;
using System;
using System.Threading;

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
            
            //activity.Finish();
            activity.FinishAffinity();
            JavaSystem.Exit(0);
            Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
        }
    }
}