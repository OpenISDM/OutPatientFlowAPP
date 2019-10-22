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
 * File Description:
 *
 *      The file contains the Navigator page that all contents and methods are 
 *      binds to the NavigatorPageViewModel fully.
 *      
 * Version:
 *
 *      1.0.0, 20190605
 * 
 * File Name:
 *
 *      NavigatorPage.xaml.cs
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
 *      Paul Chang, paulchang@iis.sinica.edu.tw
 *
 */
using System;
using Xamarin.Forms;
using IndoorNavigation.ViewModels.Navigation;
using IndoorNavigation.Models.NavigaionLayer;
using Rg.Plugins.Popup.Services;
using IndoorNavigation.Resources.Helpers;
using System.Resources;
using System.Reflection;
using Plugin.Multilingual;

namespace IndoorNavigation.Views.Navigation
{
    public partial class NavigatorPage : ContentPage
    {
        private NavigatorPageViewModel _viewModel;
        private string _waypointname;
        private string _graphname;
        private Guid _waypointID;
        private Guid _regionID;
        private XMLInformation _informationXml;
        private string _key;
        private int _index;

        public NavigatorPage(string navigationGraphName,
                             Guid destinationRegionID,
                             Guid destinationWaypointID,
                             string destinationWaypointName,
                             XMLInformation informationXML)
        {
            Console.WriteLine(">> NavigatorPage constructor: {0} {1} {2} {3} ",
                              navigationGraphName,
                              destinationRegionID,
                              destinationWaypointID,
                              destinationWaypointName);

            InitializeComponent();

            _viewModel = new NavigatorPageViewModel(navigationGraphName,
                                                    destinationRegionID,
                                                    destinationWaypointID,
                                                    destinationWaypointName,
                                                    informationXML);
            BindingContext = _viewModel;

            Console.WriteLine("<< NavigatorPage constructor");
        }

        public NavigatorPage(string navigationGraphName,
                            Guid destinationRegionID,
                            Guid destinationWaypointID,
                            string destinationWaypointName,
                            XMLInformation informationXML, 
                            string destinationKey,
                            int destinationIndex)
        {
            Console.WriteLine(">> NavigatorPage constructor: {0} {1} {2} {3} ",
                              navigationGraphName,
                              destinationRegionID,
                              destinationWaypointID,
                              destinationWaypointName);
           /* if (PopupNavigation.Instance.PopupStack.Count > 0)
                PopupNavigation.Instance.PopAsync();*/


            InitializeComponent();

            _waypointID = destinationWaypointID;
            _graphname = navigationGraphName;
            _regionID = destinationRegionID;
            _informationXml = informationXML;
            _key = destinationKey;
            _index = destinationIndex;
            _waypointname = destinationWaypointName;

            _viewModel = new NavigatorPageViewModel(navigationGraphName,
                                                    destinationRegionID,
                                                    destinationWaypointID,
                                                    destinationWaypointName,
                                                    informationXML, 
                                                    destinationKey,
                                                    destinationIndex);
            BindingContext = _viewModel;

            Console.WriteLine("<< NavigatorPage constructor");
        }
        protected override void OnDisappearing()
        {
            _viewModel.Stop();
            _viewModel.Dispose();

            base.OnDisappearing();
        }
      
        async private void FinishButton_Clicked(object sender, EventArgs e)
        {
            const string _resourceId = "IndoorNavigation.Resources.AppResources";
            ResourceManager _resourceManager =
            new ResourceManager(_resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);
            var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
            if (_key.Equals("exit"))
            {
                string s = string.Format("{0}\n{1}", _resourceManager.GetString("THANK_COMING_STRING", currentLanguage),
                    _resourceManager.GetString("HOPE_STRING", currentLanguage));
                await DisplayAlert(_resourceManager.GetString("MESSAGE_STRING"), s, _resourceManager.GetString("OK_STRING", currentLanguage));
                await Navigation.PopAsync();
                //System.Environment.Exit(0);
            }
            else
            await Navigation.PopAsync();
        }

        private void NotFinishButton_Clicked(object sender, EventArgs e)
        {
            _viewModel.Stop();
            _viewModel.Dispose();
            //OnDisappearing();
            
            NavigatorPageViewModel _newviewModel=new NavigatorPageViewModel(_graphname, _regionID, _waypointID, _waypointname, _informationXml, _key, _index);
            BindingContext = _newviewModel;
            //await Navigation.PopAsync();
            //await Navigation.PushAsync(new NavigatorPage(_graphname, _regionID, _waypointID, _waypointname, _informationXml, _key, _index));
        }
    }
}
