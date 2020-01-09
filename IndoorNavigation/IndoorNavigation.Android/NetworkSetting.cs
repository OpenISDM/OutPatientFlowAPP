using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IndoorNavigation.Droid;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using IndoorNavigation.Models;

[assembly: Xamarin.Forms.Dependency(typeof(NetworkSetting))]
namespace IndoorNavigation.Droid
{
    public class NetworkSetting:INetworkSetting
    {
        public NetworkSetting() { }
        void INetworkSetting.OpenSettingPage()
        {
            Console.WriteLine("Enter openSettingPage function");
            Intent intent = new Intent(Android.Provider.Settings.ActionNetworkOperatorSettings);//(Android.Provider.Settings.ActionWirelessSettings);
            Console.WriteLine("Construct setting page");
            Android.App.Application.Context.StartActivity(intent);
            Console.WriteLine("StarActivity");
        }
    }
}