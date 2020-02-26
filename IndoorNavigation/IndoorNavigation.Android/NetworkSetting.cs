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
using System.Net;
using Rg.Plugins.Popup.Services;
using System.Threading.Tasks;

[assembly: Xamarin.Forms.Dependency(typeof(NetworkSetting))]
namespace IndoorNavigation.Droid
{
    public class NetworkSetting:INetworkSetting
    {
        public NetworkSetting() { }
        public void OpenSettingPage()
        {
            Console.WriteLine("Enter openSettingPage function");
            Intent intent = 
				new Intent(Android.Provider.Settings
						   .ActionNetworkOperatorSettings);
            Android.App.Application.Context.StartActivity(intent);
            Console.WriteLine("StarActivity");
        }

        public Task<bool> CheckInternetConnect()
        {
            try
            {
                string CheckUrl = "https://www.google.com/";
                HttpWebRequest iNetRequest = 
					(HttpWebRequest)WebRequest.Create(CheckUrl);

                iNetRequest.Timeout = 2000;
                WebResponse iNetResponse = iNetRequest.GetResponse();

                iNetResponse.Close();                
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }
    }
}