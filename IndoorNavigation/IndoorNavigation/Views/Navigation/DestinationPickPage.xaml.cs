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
 *      The file contains the class for the page that displays the names of 
 *      destinations of all categories of the user is to reflect a destination 
 *      from the displayed list.
 *      
 * Version:
 *
 *      1.0.0, 20190605
 * 
 * File Name:
 *
 *      DestinationPickPage.xaml.cs
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
 *      Chun-Yu Lai, chunyu1202@gmail.com
 *
 */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using MvvmHelpers;
using IndoorNavigation.Models.NavigaionLayer;
using System.Linq;
using IndoorNavigation.Models;
using static IndoorNavigation.Utilities.Storage;
using Plugin.Geolocator;
using IndoorNavigation.Views.PopUpPage;
namespace IndoorNavigation.Views.Navigation
{
    public partial class DestinationPickPage : ContentPage
    {
        private string _navigationGraphName;
        private NavigationGraph _navigationGraph;

        public ObservableCollection<string> _items { get; set; }
        private App app;
        public ObservableCollection<DestinationItem> _destinationItems { get; set; }
        private XMLInformation _nameInformation;

        private ICheckLocationEnable checkLocationEnable;
        public DestinationPickPage(string navigationGraphName, CategoryType category)
        {
            InitializeComponent();
            _destinationItems = new ObservableCollection<DestinationItem>();

            _navigationGraphName = navigationGraphName;
            _navigationGraph = LoadNavigationGraphXml(_navigationGraphName);
            _nameInformation = LoadXmlInformation(_navigationGraphName);
            app = (App)Application.Current;

            NavigationPage.SetBackButtonTitle(this, GetResourceString("BACK_STRING"));

            foreach (KeyValuePair<Guid, Models.Region> pairRegion in _navigationGraph.GetRegions())
            {
                string floorName = _nameInformation.GetRegionName(pairRegion.Value._id);
                if (pairRegion.Value._waypointsByCategory.ContainsKey(category))
                {
                    foreach (Waypoint waypoint in pairRegion.Value._waypointsByCategory[category])
                    {
                        string waypointName = waypoint._name;
                        waypointName = _nameInformation.GetWaypointName(waypoint._id);
                        if (waypoint._type == LocationType.terminal || 
                            waypoint._type == LocationType.landmark ||
                            (waypoint._type == LocationType.portal && 
                            waypoint._isVirtualPoint == true))
                        {
                            Console.WriteLine("check type : " + waypoint._type.ToString());
                            _destinationItems.Add(new DestinationItem
                            {
                                _regionID = pairRegion.Key,
                                _waypointID = waypoint._id,
                                _waypointName = waypointName,
                                _floor = floorName
                            });
                        }
                        else
                        {
                            Console.WriteLine("Portal, No need to add!!");
                        }

                    }

                }
            }

            MyListView.ItemsSource = 
                from waypoint in _destinationItems
                group waypoint by waypoint._floor into waypointGroup
                orderby waypointGroup.Key
                select new Grouping<string, DestinationItem>(waypointGroup.Key,
                waypointGroup);

            checkLocationEnable = 
                DependencyService.Get<ICheckLocationEnable>();
        }

        private bool IsGPSEnable()
        {           
            return CrossGeolocator.Current.IsGeolocationEnabled;
        }

        private bool IsBluetoothEnable()
        {
            return checkLocationEnable.IsBluetoothEnable();
        }
        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (!IsGPSEnable() && Device.RuntimePlatform == Device.Android)
            {
                //to ask user to open GPS
                AlertDialogPopupPage alertPage = 
                    new AlertDialogPopupPage
                    (GetResourceString("NO_GPS_GO_SETTING_STRING"), 
                    GetResourceString("OK_STRING"),
                    GetResourceString("CANCEL_STRING"));

                if(await alertPage.show())
                    checkLocationEnable.OpenLocationSetting();
                
                return; 
            }
            if (!IsBluetoothEnable() && Device.RuntimePlatform == Device.Android)
            {
                //to ask user to open BT
                AlertDialogPopupPage alertPage = 
                    new AlertDialogPopupPage
                    (GetResourceString("NO_BLUETOOTH_GO_SETTING_STRING"), 
                    GetResourceString("OK_STRING"), 
                    GetResourceString("CANCEL_STRING"));

                if(await alertPage.show())
                    checkLocationEnable.OpenBluetoothSetting();
                return;
            }
            if (e.Item is DestinationItem destination)
            {
                Console.WriteLine(">> Handle_ItemTapped in DestinationPickPage");
                app._globalNavigatorPage = 
                    new NavigatorPage(_navigationGraphName,
                                      destination._regionID,
                                      destination._waypointID,
                                      destination._waypointName,
                                      _nameInformation);


                await Navigation.PushAsync(app._globalNavigatorPage);
            }
            ((ListView)sender).SelectedItem = null;
        }

    }
}
