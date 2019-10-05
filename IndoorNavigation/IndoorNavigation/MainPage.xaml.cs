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
 *      The file contains the class for the main page that contains the 
 *      listview of locations that are waypoints defined by the navigation 
 *      grash in use.
 *      
 * Version:
 *
 *      1.0.0, 20190605
 * 
 * File Name:
 *
 *      MainPage.xaml.cs
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
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using IndoorNavigation.Views.Settings;
using IndoorNavigation.Views.Navigation;
using IndoorNavigation.ViewModels;
using Plugin.Multilingual;
using System.Resources;
using IndoorNavigation.Resources.Helpers;
using System.Reflection;
using IndoorNavigation.Modules.Utilities;
using IndoorNavigation.Models.NavigaionLayer;
using Xamarin.Essentials;
using System.Diagnostics;

namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        MainPageViewModel _viewModel;

        const string _resourceId = "IndoorNavigation.Resources.AppResources";
        ResourceManager _resourceManager =
            new ResourceManager(_resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);

        public MainPage()
        {
            InitializeComponent();

            var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
            NavigationPage.SetBackButtonTitle(this, _resourceManager.GetString("HOME_STRING", currentLanguage));
            NavigationPage.SetHasBackButton(this, false);

            switch (Device.RuntimePlatform)
            {
                case Device.Android:
                    NaviSearchBar.BackgroundColor = Color.White;
                    break;

                default:
                    break;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            ((NavigationPage)Application.Current.MainPage).BarBackgroundColor = Color.FromHex("#3F51B5");
            ((NavigationPage)Application.Current.MainPage).BarTextColor = Color.White;

            _viewModel = new MainPageViewModel();
            BindingContext = _viewModel;

            // This will remove all the pages in the navigation stack excluding the Main Page
            // and another one page
            //Console.WriteLine("NavigationStack : " +Navigation.NavigationStack.Count);
            //for (int PageIndex = Navigation.NavigationStack.Count-2; PageIndex > 0; PageIndex--)
            //{
            //    Console.WriteLine("PageIndex : " +PageIndex);
            //    Navigation.RemovePage(Navigation.NavigationStack[PageIndex]);
            //}

            switch (Device.RuntimePlatform)
            {
                case Device.Android:
                    //NavigatorButton.Padding = new Thickness(30, 1, 1, 1);
                    //AbsoluteLayout.SetLayoutBounds(NavigatorButton,
                    //    new Rectangle(0.5, 0.52, 0.7, 0.1));
                    break;

                case Device.iOS:
                    // customize CurrentInstruction UI for iPhone 5s/SE
                    if (Height < 600)
                    {
                        //WelcomeLabel.FontSize = 36;
                        //BeDISLabel.FontSize = 39;
                        //SloganLabel.Text = "";
                        //AbsoluteLayout.SetLayoutBounds(NavigatorButton,
                        //    new Rectangle(0.5, 0.47, 0.7, 0.12));
                    }
                    break;

                default:
                    break;
            }
        }

        async void SettingBtn_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingTableViewPage());
        }

        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
             var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
            if (e.Item is ViewModels.Location location)
            {
                NavigationGraph navigationGraph = NavigraphStorage.LoadNavigationGraphXML(location.UserNaming);

                switch (navigationGraph.GetIndustryServer())
                {
                    case "hospital":
                        var answser = await DisplayAlert(
                            _resourceManager.GetString("GO_NAVIGATION_HOME_PAGE_STRING", currentLanguage),
                            location.UserNaming, _resourceManager.GetString("OK_STRING", currentLanguage),
                            _resourceManager.GetString("CANCEL_STRING", currentLanguage));
                        string IDnum = Preferences.Get("ID_NUMBER_STRING", string.Empty);
                        string patientID = Preferences.Get("PATIENT_ID_STRING", string.Empty);
                        DateTime birthday = Preferences.Get("BIRTHDAY_DATETIME", DateTime.Now);
                        Debug.WriteLine(string.Format("location name is :{0}", location.UserNaming));
                       XMLInformation _nameInformation = NavigraphStorage.LoadInformationML(location.UserNaming + "_info_zh.xml");

                   /*     if (CrossMultilingual.Current.CurrentCultureInfo.ToString() == "en" || CrossMultilingual.Current.CurrentCultureInfo.ToString() == "en-US")
                        {
                            _nameInformation = NavigraphStorage.LoadInformationML(location.UserNaming + "_info_en-US.xml");
                        }
                        else if (CrossMultilingual.Current.CurrentCultureInfo.ToString() == "zh" || CrossMultilingual.Current.CurrentCultureInfo.ToString() == "zh-TW")
                        {
                            _nameInformation = NavigraphStorage.LoadInformationML(location.UserNaming + "_info_zh.xml");
                        }*/



                        if (answser)
                        {
                            await Navigation.PushAsync(new RigisterList(location.UserNaming, new QueryResult()));
                            bool isRigister = await DisplayAlert(_resourceManager.GetString("MESSAGE_STRING", currentLanguage),
                                  _resourceManager.GetString("NEED_REGISTER_STRING", currentLanguage),
                                  _resourceManager.GetString("OK_STRING", currentLanguage),
                                  _resourceManager.GetString("CANCEL_STRING", currentLanguage));
                            
                            if (isRigister)
                            {
                                   await Navigation.PushAsync(new  NavigatorPage(location.UserNaming,
                                       new Guid("11111111-1111-1111-1111-111111111111"), 
                                       new Guid("00000000-0000-0000-0000-000000000003"),
                                       "服務台",  _nameInformation,"register",0));

                                //await Navigation.PushAsync(new TestPage(new DestinationItem { Key = "register" },0));
                            }
                            
                            else if (IDnum.Equals(string.Empty) || patientID.Equals(string.Empty))// || birthday.Equals(DateTime.Now))
                            {
                                await DisplayAlert(_resourceManager.GetString("MESSAGE_STRING", currentLanguage),
                                    _resourceManager.GetString("ALERT_LOGIN_STRING", currentLanguage), _resourceManager.GetString("OK_STRING", currentLanguage));
                            }
                            else
                            {
                                //query server data to observablecollection
                            }

                        }
                        break;

                    case "city_hall":
                        var answser_city_hall = await DisplayAlert(
                            _resourceManager.GetString("GO_NAVIGATION_HOME_PAGE_STRING", currentLanguage),
                            location.UserNaming, _resourceManager.GetString("OK_STRING", currentLanguage),
                            _resourceManager.GetString("CANCEL_STRING", currentLanguage));
                        if (answser_city_hall)
                        {
                            await Navigation.PushAsync(new CityHallHomePage(location.UserNaming));
                        }
                        break;

                    default:
                        Console.WriteLine("Unknown _industryService");
                        break;
                }
            }
        }

        void LocationListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            // disable it
            LocationListView.SelectedItem = null;
        }

        void LocationListView_Refreshing(object sender, EventArgs e)
        {
            LocationListView.EndRefresh();
        }

        void Item_Delete(object sender, EventArgs e)
        {
            var item = (ViewModels.Location)((MenuItem)sender).CommandParameter;

            if (item != null)
            {
                NavigraphStorage.DeleteNavigationGraph(item.UserNaming);
                _viewModel.LoadNavigationGraph();
            }
        }
    }
}
