using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IndoorNavigation.iOS;
using IndoorNavigation;
using Foundation;
using UIKit;
[assembly:Xamarin.Forms.Dependency(typeof(NetworkSetting))]
namespace IndoorNavigation.iOS
{
    class NetworkSetting:INetworkSetting
    {
        void INetworkSetting.OpenSettingPage()
        {
            var url = new NSUrl("App-prefs:root=Bluetooth");
            if (UIApplication.SharedApplication.CanOpenUrl(url))
            {
                UIApplication.SharedApplication.OpenUrl(url);
            }
            else
            {
                UIApplication.SharedApplication.OpenUrl(new NSUrl("Prefs:root=Bluetooth")); 
            }
        }
    }
}