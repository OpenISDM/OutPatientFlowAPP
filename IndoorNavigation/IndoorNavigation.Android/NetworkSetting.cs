/*
 * 2020 © Copyright (c) BiDaE Technology Inc. 
 * Provided under BiDaE SHAREWARE LICENSE-1.0 in the LICENSE.
 *
 * Project Name:
 *
 *      IndoorNavigation
 *
 * Version:
 *
 *      1.0.0, 20200221
 * 
 * File Name:
 *
 *      AddPopupPage.cs
 *
 * Abstract:
 *      
 *
 *      
 * Authors:
 * 
 *      Jason Chang, jasonchang@bidae.tech 
 *      
 */
using System;
using IndoorNavigation.Droid;
using Android.Content;
using IndoorNavigation.Models;
using System.Net;
using System.Threading.Tasks;

[assembly: Xamarin.Forms.Dependency(typeof(NetworkSetting))]
namespace IndoorNavigation.Droid
{
    public class NetworkSetting : INetworkSetting
    {
        public NetworkSetting() { }

        public void OpenSettingPage()
        {
            Console.WriteLine("Enter openSettingPage function");
            Intent intent =
                new Intent
                (Android.Provider.Settings.ActionNetworkOperatorSettings);
            Console.WriteLine("Construct setting page");

            var chooseIntent = Intent.CreateChooser(intent, "Go to Setting");

            chooseIntent.SetFlags
                (ActivityFlags.ClearWhenTaskReset | ActivityFlags.NewTask);

            Android.App.Application.Context.StartActivity(chooseIntent);
            Console.WriteLine("StarActivity");
        }

        public Task<bool> CheckInternetConnect()
        {
            try
            {
                string CheckUrl = "https://www.google.com/";
                HttpWebRequest iNetRequest =
                    (HttpWebRequest)WebRequest.Create(CheckUrl);

                iNetRequest.Timeout = 3000;
                WebResponse iNetResponse = iNetRequest.GetResponse();

                iNetResponse.Close();
                iNetRequest.Abort();
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
                HttpWebRequest request = WebRequest.CreateHttp(url);
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
        }
    }
}