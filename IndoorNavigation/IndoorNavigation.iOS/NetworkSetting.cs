/*
 * Copyright (c) 2019 Academia Sinica, Institude of Information Science
 *
 * License:
 *      GPL 3.0 : The content of this file is subject to the terms and
 *      conditions defined in file 'COPYING.txt', which is part of this source
 *      code package.
 *
 * Project Name:
 *
 *      IndoorNavigation
 *
 * 
 *     
 *      
 * Version:
 *
 *      1.0.0, 20200221
 * 
 * File Name:
 *
 *      NetworkSetting.cs
 *
 * Abstract:
 *      
 *
 *      
 * Authors:
 * 
 *      Jason Chang, jasonchang@iis.sinica.edu.tw    
 */  
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
using System.Threading.Tasks;
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
                UIApplication.SharedApplication.OpenUrl
				(new NSUrl
					("app-settings:root=General&path=USAGE/CELLULAR_USAGE")); 
            }
        }
			
        public Task<bool> CheckInternetConnect()
        {
            try
            {
                string Checkurl = "https://www.google.com/";
                HttpWebRequest CheckConnectRequest = 
					(HttpWebRequest)WebRequest.Create(Checkurl);

                CheckConnectRequest.Timeout = 5000;
                WebResponse CheckConnectResponse = 
					CheckConnectRequest.GetResponse();

                CheckConnectResponse.Close();                
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }
    }
}