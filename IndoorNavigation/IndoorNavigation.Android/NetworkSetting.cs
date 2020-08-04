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

                iNetRequest.Timeout = 3000;
                WebResponse iNetResponse = iNetRequest.GetResponse();

                iNetResponse.Close();
                iNetRequest.Abort();
                //Console.WriteLine("The network is all fine.");
                //PopupNavigation.Instance.PushAsync(new DisplayAlertPopupPage("the network is work fine now."));
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }           
                
        }

        public Task<bool> CheckWebSiteAvailable(string url)
        {
            try
            {
                HttpWebRequest request = 
                    (HttpWebRequest)WebRequest.CreateHttp(url);
                request.Timeout = 3000;

                WebResponse response = request.GetResponse();

                response.Close();
                request.Abort();

                return Task.FromResult(true);
            } 
            catch (Exception exc)
            {
                Console.WriteLine("CheckWebSiteAvailable error - " 
                    + exc.Message);
                return Task.FromResult(false);
            }
            //throw new NotImplementedException();
        }
    }
}