using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Net.Wifi;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using IndoorNavigation;
namespace IndoorNavigation.Droid
{
    public class WifiConnect : IWifiConnect
    {
        void IWifiConnect.ConnectToWifi(string ssid,string password)
        {
            //get wifi service
            var wifiManager = (WifiManager)Android.App.Application.Context.GetSystemService(Context.WifiService);
            //to make ssid and password looks like: "ssid", "password" format
            var formattedSSid = $"\"{ssid}\"";
            var formattedPassword = $"\"{password}\"";

            //to new a configuration that contains both of ssid and password
            var wifiConfig = new WifiConfiguration
            {
                Ssid=formattedSSid, PreSharedKey=formattedPassword
            };
            //to add configuration to wifimanager
            var addNetwork = wifiManager.AddNetwork(wifiConfig);

            var network = wifiManager.ConfiguredNetworks.FirstOrDefault(n => n.Ssid == ssid);

            if (network == null)
            {
                Console.WriteLine("Cannot connect to network:{ssid}");
                return;
            }
        }
        void IWifiConnect.DisconnectToWifi()
        {
            var wifiManager = (WifiManager)Android.App.Application.Context.GetSystemService(Context.WifiService);
            wifiManager.Disconnect();
            //var enableNetwork = wifiManager.EnableNetwork();
        }
    }
}