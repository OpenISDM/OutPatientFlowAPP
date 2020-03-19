using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using IndoorNavigation.Models;
using IndoorNavigation.Droid;

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.Essentials;
using Android.Support.V4.Content;


[assembly: Xamarin.Forms.Dependency(typeof(PermissioCheck))]
namespace IndoorNavigation.Droid
{
    public class PermissionCheckDroid:  PermissioCheck
    {
        public bool  CameraPermissionCheck() 
        {
        //    var status = await per
            return true;
        }
        public void CameraPermissionAsk() 
        {

        }
        public bool LocationPermissionCheck() { return true; }
        public void LocationPermissionAsk() { }
    }
}