/*
 * 2020 © Copyright (c) BiDaE Technology Inc. 
 * Provided under BiDaE SHAREWARE LICENSE-1.0 in the LICENSE.
 *
 * Project Name:
 *
 *      IndoorNavigation
 *
 * File Description:
 * 
 *      This view model implements properties and commands for MainPage
 *      can bind the data.
 *      It will display the list of locations according to the Navigation 
 *      graph in phone's storage which user has downloaded.
 *      
 * Version:
 *
 *      1.0.0, 20190605
 * 
 * File Name:
 *
 *      MainPageViewModel.cs
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
using System.Collections.Generic;
using System.Linq;
using MvvmHelpers;
using IndoorNavigation.Utilities;
using static IndoorNavigation.Utilities.Constants;
using static IndoorNavigation.Utilities.Helper;

namespace IndoorNavigation.ViewModels
{
    public class MainPageViewModel : BaseViewModel
    {
        private ObservableRangeCollection<Location> _locations;
        // IEnumerable of Locations which used by search method
        private IEnumerable<Location> _returnedLocations;
        private Location _selectedItem;
        private string _searchedText;

        public MainPageViewModel()
        {
            _returnedLocations = new ObservableRangeCollection<Location>();
            LoadNavigationGraph();
        }

        public void LoadNavigationGraph()
        {
            _locations = new ObservableRangeCollection<Location>();

            foreach (Location location in
                     Storage.GetAllNaviGraphName())
            {
                _locations.Add(new Location
                {
                    sourcePath = location.sourcePath,
                    UserNaming = location.UserNaming,
                    displayTextColor = GetListTextColor(location.UserNaming)                
                }); 
            }
            if (_locations.Any())
            {
                NavigationGraphFiles = _locations;
            }
        }        

        public IList<Location> NavigationGraphFiles
        {
            get
            {
                return (from location in _returnedLocations
                        orderby location.UserNaming[0]
                        select location).ToList();
            }
            set
            {
                SetProperty(ref _returnedLocations, value);
            }
        }

        public Location SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    OnPropertyChanged("SelectedItem");
                    _selectedItem = null;
                }
            }
        }

        public string SearchedText
        {
            get
            {
                return _searchedText;
            }

            set
            {
                _searchedText = value;
                OnPropertyChanged("SearchedText");

                // Search waypoints
                var searchedWaypoints =
                    string.IsNullOrEmpty(value) ?
                    _locations : _locations
                    .Where(c => c.UserNaming.ToUpper().Contains(value.ToUpper()));
                NavigationGraphFiles = searchedWaypoints.ToList();
            }
        }
    }

    public class Location
    {
        public string UserNaming { get; set; }
        public string sourcePath { get; set; }

        public string displayTextColor { get; set; }
        public override string ToString() => UserNaming;
    }
}