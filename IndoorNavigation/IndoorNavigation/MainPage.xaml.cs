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
using IndoorNavigation.Views.PopUpPage;
using IndoorNavigation.ViewModels;
using Plugin.Multilingual;
using System.Resources;
using IndoorNavigation.Resources.Helpers;
using System.Reflection;
using IndoorNavigation.Models.NavigaionLayer;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using Rg.Plugins.Popup.Services;
using System.Globalization;
using Newtonsoft.Json;
using IndoorNavigation.Utilities;
using IndoorNavigation.Models;
using IndoorNavigation.Modules.Utilities;
using Prism.Modularity;
using System.Linq;
using Xamarin.Essentials;
using Location = IndoorNavigation.ViewModels.Location;
using System.Collections.ObjectModel;
using static IndoorNavigation.Utilities.Storage;
using System.Data.Common;

namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        MainPageViewModel _viewModel;
        internal static readonly string _versionRoute = 
			"IndoorNavigation.Resources.Map_Version.xml";
			
        const string _resourceId = 
			"IndoorNavigation.Resources.AppResources";
			
        internal static readonly string _versionRouteInPhone
             = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.
							LocalApplicationData), "Version");
							
        ResourceManager _resourceManager =
            new ResourceManager(_resourceId, 
								typeof(TranslateExtension).GetTypeInfo()
								.Assembly);
								
        private bool updateMapOrNot;
        private static PhoneInformation _phoneInformation = 
			new PhoneInformation();

        ViewCell lastCell = null;
        bool isButtonPressed = false; //to prevent multi-click
        CultureInfo currentLanguage = 
			CrossMultilingual.Current.CurrentCultureInfo;

        public MainPage()
        {
            InitializeComponent();
			
            NavigationPage.SetBackButtonTitle(this, 
											  _resourceManager
											  .GetString("HOME_STRING", 
														 currentLanguage));
            NavigationPage.SetHasBackButton(this, false);
            updateMapOrNot = false;
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

            ((NavigationPage)Application.Current.MainPage).BarBackgroundColor = 
				Color.FromHex("#3F51B5");
            ((NavigationPage)Application.Current.MainPage).BarTextColor = 
				Color.White;

            _viewModel = new MainPageViewModel();
            BindingContext = _viewModel;
           
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

        #region To implement 
        private INetworkSetting setting;
        private CloudDownload _download = new CloudDownload();
        private CurrentMapInfos mapInfosFromServer = new CurrentMapInfos();
        async void SettingBtn_Clicked(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PushAsync(new IndicatorPopupPage());
            setting = DependencyService.Get<INetworkSetting>();
            bool Connectable = await setting.CheckInternetConnect();

            if (Connectable)
            {
                //it will be a xml format
                string SupportList = _download.Download(_download.getSupportListUrl());
                Console.WriteLine("SupporList context : " + SupportList);
                #region when server not response. I know the code 很母湯，but it's temporary haha.
                if (string.IsNullOrEmpty(SupportList))
                {
                    await (Navigation.PushAsync(new SettingTableViewPage()));
                }
                else
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(SupportList);
                    Dictionary<string, GraphInfo> SupportListDict = Storage.GraphInfoReader(doc);
                    _serverResources = SupportListDict;
                    await Navigation.PushAsync(new SettingTableViewPage(SupportListDict));
                }
                #endregion
            }
            else 
                await Navigation.PushAsync(new SettingTableViewPage());

            await PopupNavigation.Instance.PopAllAsync();
        }        
        #endregion

        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
             var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
            if (e.Item is Location location)
            {              
                NavigationGraph navigationGraph =
                    Storage.LoadNavigationGraphXml(location.sourcePath);

                if(Storage.CheckVersionNumber(location.sourcePath, navigationGraph.GetVersion(),AccessGraphOperate.CheckLocalVersion))
                {
                    if (await DisplayAlert(
                        _resourceManager.GetString("UPDATE_MAP_STRING", _currentCulture), 
                        location.UserNaming, 
                        _resourceManager.GetString("OK_STRING", _currentCulture),
                        _resourceManager.GetString("CANCEL_STRING", _currentCulture))
                    )
                    {
                        EmbeddedGenerateFile(location.sourcePath);
                    }
                }


                #region Version Compare
                //         XmlDocument xmlDocument = new XmlDocument();
                //         using (var stream = 
                //Assembly.GetExecutingAssembly()
                //.GetManifestResourceStream(_versionRoute))
                //         {
                //             StreamReader tr = new StreamReader(stream);
                //             string fileContents = tr.ReadToEnd();
                //             Console.WriteLine(">>MainPage :: HandleItemTapped");
                //             Console.WriteLine(fileContents);
                //             xmlDocument.LoadXml(fileContents);
                //         }


                //         ReadVersion readVersion = new ReadVersion(xmlDocument);
                //         double newVersion = 
                //readVersion.ReturnVersion(navigationGraph.GetBuildingName());

                //         if (navigationGraph.GetVersion() != newVersion)
                //         {                    
                //             var answser = await DisplayAlert(
                //                         _resourceManager
                //	.GetString("UPDATE_MAP_STRING",currentLanguage),
                //                         location.UserNaming, 
                //	_resourceManager
                //	.GetString("OK_STRING", currentLanguage),
                //                         _resourceManager
                //	.GetString("CANCEL_STRING", currentLanguage));


                //             if (answser)
                //             {

                //                 List<string> generateName = 
                //_phoneInformation
                //.GiveGenerateMapName(location.UserNaming);

                //                 Storage.EmbeddedGenerateFile(generateName[1]);
                //                 updateMapOrNot = true;
                //             }
                //             else
                //             {

                //                 updateMapOrNot = false;
                //             }
                //         }
                //         else
                //         {
                //             updateMapOrNot = true;
                //         }

                //         if(updateMapOrNot == true)
                #endregion
                {
                    if (isButtonPressed) return;
                    isButtonPressed = true;

                    switch (navigationGraph.GetIndustryServer())
                    {

                        case "hospital":
                            if (location.UserNaming == 
								_resourceManager
								.GetString("YUANLIN_CHRISTIAN_HOSPITAL_STRING", 
										   currentLanguage))
                                await PopupNavigation.Instance
									  .PushAsync(new SelectTwoWayPopupPage
									  (location));
                            else
                                await Navigation.PushAsync
								  (new NavigationHomePage(location));
                            
                            break;

                        case "city_hall":
                            await Navigation.PushAsync
								  (new CityHallHomePage(location));
                            
                            break;

                        default:
                            Console.WriteLine("Unknown _industryService");
                            break;
                    }
                    isButtonPressed = false;
                    ((ListView)sender).SelectedItem = null;
                }
                
            }
        }

        protected override bool OnBackButtonPressed()
        {
            Device.BeginInvokeOnMainThread(async ()=>
            {
                bool isAgree = 
					await DisplayAlert
						(_resourceManager
						 .GetString("MESSAGE_STRING",currentLanguage), 
						 _resourceManager
						 .GetString("ASK_LEAVE_APP_STRING", currentLanguage), 
						 _resourceManager
						 .GetString("LEAVE_STRING",currentLanguage),
						 _resourceManager
						 .GetString("CANCEL_STRING",currentLanguage));

                if (isAgree)
                {                    
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                }
            });
            return true;
        }

        void LocationListView_ItemSelected(object sender, 
										   SelectedItemChangedEventArgs e)
        {
            // disable it
            LocationListView.SelectedItem = null;
        }

        void LocationListView_Refreshing(object sender, EventArgs e)
        {
            LocationListView.EndRefresh();
        }
    
        private void ViewCell_Tapped(object sender, EventArgs e)
        {
            if (lastCell != null)
            {
                lastCell.View.BackgroundColor = Color.Transparent;
            }
            var viewCell = (ViewCell)sender;
            if (viewCell.View != null)
            {
                viewCell.View.BackgroundColor = Color.FromHex("FFFF88");
                Device.StartTimer(TimeSpan.FromSeconds(1.5), () => {
                    viewCell.View.BackgroundColor = Color.Transparent;
                    return false;
                });                
            }
        }

        async private void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new TestPage_Listview());            
            //await PopupNavigation.Instance.PushAsync(new PickDownloadPopupPage());
        }
    }
}
