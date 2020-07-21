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
            Intent intent = new Intent(Android.Provider.Settings.ActionNetworkOperatorSettings);//(Android.Provider.Settings.ActionWirelessSettings);
            Console.WriteLine("Construct setting page");
            //intent.AddFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
            var chooseIntent = Intent.CreateChooser(intent, "Go to Setting");
            chooseIntent.SetFlags(ActivityFlags.ClearWhenTaskReset | ActivityFlags.NewTask);

            Android.App.Application.Context.StartActivity(chooseIntent);
            Console.WriteLine("StarActivity");
        }

        public Task<bool> CheckInternetConnect()
        {
            try
            {
                string CheckUrl = "https://www.google.com/";
                HttpWebRequest iNetRequest = (HttpWebRequest)WebRequest.Create(CheckUrl);

                iNetRequest.Timeout = 2000;
                WebResponse iNetResponse = iNetRequest.GetResponse();

                iNetResponse.Close();
                //Console.WriteLine("The network is all fine.");
                //PopupNavigation.Instance.PushAsync(new DisplayAlertPopupPage("the network is work fine now."));
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }
    }
}