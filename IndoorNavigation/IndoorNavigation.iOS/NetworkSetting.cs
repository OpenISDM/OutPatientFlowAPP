using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IndoorNavigation.iOS;
using IndoorNavigation;
using Foundation;
using UIKit;
using System.Net;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using IndoorNavigation.Models;
[assembly:Xamarin.Forms.Dependency(typeof(NetworkSetting))]
namespace IndoorNavigation.iOS
{
    class NetworkSetting:INetworkSetting
    {
        public NetworkSetting() { }
        public void OpenSettingPage()
        {
            var url = new NSUrl("App-prefs:root=Bluetooth");
            if (UIApplication.SharedApplication.CanOpenUrl(url))
            {
                UIApplication.SharedApplication.OpenUrl(url);
            }
            else
            {
                UIApplication.SharedApplication.OpenUrl(new NSUrl("app-settings:root=General&path=USAGE/CELLULAR_USAGE")); 
            }
        }

        public bool CheckInternetConnect()
        {
            try
            {
                string Checkurl = "https://www.google.com/";
                HttpWebRequest CheckConnectRequest = (HttpWebRequest)WebRequest.Create(Checkurl);

                CheckConnectRequest.Timeout = 5000;
                WebResponse CheckConnectResponse = CheckConnectRequest.GetResponse();

                CheckConnectResponse.Close();
                //Console.WriteLine("The network is all fine.");                
                //PopupNavigation.Instance.PushAsync(new DisplayAlertPopupPage("the network is work fine now."));
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}