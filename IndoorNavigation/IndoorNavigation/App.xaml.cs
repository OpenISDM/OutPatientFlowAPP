﻿/*
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
 * File Description:
 *
 *      The file contains the code behind for the App class. The code is 
 *      responsible for instantiating the first page that will be displayed by
 *      the application on each platform, and for handling application 
 *      lifecycle events. Both App.xaml and App.xaml.cs contribute to a class 
 *      named App that derives from Application. This is only one entry point 
 *      when the app launch at first time.
 *      
 * Version:
 *
 *      1.0.0, 20190605
 * 
 * File Name:
 *
 *      App.xaml.cs
 *
 * Abstract:
 *
 *      Waypoint-based navigator is a mobile Bluetooth navigation application
 *      that runs on smart phones. It is structed to support anywhere 
 *      navigation indoors in areas covered by different indoor positioning 
 *      system (IPS) and outdoors covered by GPS.In particilar, it can rely on
 *      BeDIS (Building/environment Data and Information System) for indoor 
 *      positioning. This IPS provides a location beacon at every waypoint. The 
 *      beacon brocasts its own coordinates; Consequesntly, the navigator does 
 *      not need to continuously monitor its own position.
 *      This version makes use of Xamarin.Forms, which is a cross-platform UI 
 *      tookit that runs on both iOS and Android.
 *
 * Authors:
 *
 *      Kenneth Tang, kenneth@gm.nssh.ntpc.edu.tw
 *      Paul Chang, paulchang@iis.sinica.edu.tw
 *
 */
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using IndoorNavigation.Modules;
using IndoorNavigation.Models;
using IndoorNavigation.ViewModels;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using IndoorNavigation.Resources;
using Plugin.Multilingual;
using System.Collections.ObjectModel;
using Xamarin.Essentials;
using System;
using IndoorNavigation.Views.Navigation;

using System.Collections.Generic;
using IndoorNavigation.Utilities;
using static IndoorNavigation.Utilities.Storage;
using System.Xml;
using System.Threading.Tasks;
using System.Linq;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace IndoorNavigation
{
    public partial class App : Application
    {
        public ObservableCollection<RgRecord> records =
            new ObservableCollection<RgRecord>();

        public int FinishCount = 0;
        public bool isRigistered = false;
        public bool getRigistered = false;
        public ObservableCollection<RgRecord> _TmpRecords =
            new ObservableCollection<RgRecord>();
        public string IDnumber =
            Preferences.Get("ID_NUMBER_STRING", string.Empty);

        public bool HaveCashier = false;
        public DateTime RgDate = DateTime.Now;
        public RgRecord roundRecord = null;
        public RgRecord lastFinished = null;

        #region for fix and new functions
        public bool isResume = false;
        public NavigatorPage _globalNavigatorPage = null;
        private NavigationPage navigationPage;
        #endregion
        public int count = 0;

        public Dictionary<int, int> OrderDistrict = new Dictionary<int, int>();
        public App()
        {
            InitializeComponent();

            // Get the current device language
            AppResources.Culture = CrossMultilingual.Current.DeviceCultureInfo;
            if (AppResources.Culture.ToString().Contains("zh-TW"))
            {
                Current.Properties["LanguagePicker"] = "Chinese";
            }
            else if (AppResources.Culture.ToString().Contains("en-US"))
            {
                Current.Properties["LanguagePicker"] = "English";
            }

            navigationPage = new NavigationPage(new MainPage());
            MainPage =
                navigationPage;
        }        

        async private void LoadSupportList()
        {
            CloudDownload _download = new CloudDownload();

            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    string SupportResourceXmlString =
                    _download.Download(_download.getSupportListUrl());
                
                    XmlDocument xmlDoc = new XmlDocument();

                    xmlDoc.LoadXml(SupportResourceXmlString);

                    _serverResources = GraphInfoReader(xmlDoc);

                }
                catch (Exception exc)
                {
                    Console.WriteLine("Parsing SupportList error : "
                        + exc.Message);
                    _serverResources = null;
                }
            }
            await Task.CompletedTask;
        }

        protected override void OnStart()
        {
            Console.WriteLine(">>OnStart");
            // App Center brings together multiple services commonly used by
            // mobile developers into an integrated cloud solution.
            // Developers use App Center to Build, Test.
            // Once the app's deployed, developers monitor the status and 
            // usage of the app using the Analytics and Diagnostics services,
            // and engage with users using the Push service.
            AppCenter.Start("ios=3efcee27-6067-4f41-a94f-87c97d6b8118;" +
             "android=8cc03d85-0d94-4cec-b4b6-808719a60857",
             typeof(Analytics), typeof(Crashes));

            // Beacon scan api must adjust later, it should regist after
            // navigraph is be loaded.
            Utility._ibeaconScan = DependencyService.Get<IBeaconScan>();
            Utility._lbeaconScan = DependencyService.Get<LBeaconScan>();
            Utility._textToSpeech = DependencyService.Get<ITextToSpeech>();
            //Utility.SignalProcess = new SignalProcessModule();

            TmperorayStatus.RestoreAllState();            
            LoadSupportList();            
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
            Console.WriteLine(">>OnSleep");
            
            if(_globalNavigatorPage != null &&
                navigationPage.Navigation.NavigationStack.Last()
                == _globalNavigatorPage)
            {
                _globalNavigatorPage.OnPause();
            }

            TmperorayStatus.StoreAllState();
            base.OnSleep();
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
            Console.WriteLine(">>OnResume");
            Console.WriteLine("current page == navigatorpage :" +
                (navigationPage.Navigation.NavigationStack.Last() == 
                _globalNavigatorPage));

            if(_globalNavigatorPage!= null && 
                navigationPage.Navigation.NavigationStack.Last() ==
                _globalNavigatorPage)
            {
                _globalNavigatorPage.Resume();
            }
            base.OnResume();
        }

        // If we don't implement it, the thread will not be destroyed and stuck
        // here.
        public void OnStop()
        {
            Console.WriteLine("Call Onstop");
            OnSleep();
            if (_globalNavigatorPage != null)
            {
                _globalNavigatorPage.OnStop();
            }
        }                     
    }
}