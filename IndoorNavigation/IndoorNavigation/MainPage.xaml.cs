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
using Rg.Plugins.Popup.Services;
using System.Globalization;
using IndoorNavigation.Utilities;
using IndoorNavigation.Models;
using IndoorNavigation.Modules.Utilities;
using System.Linq;
using Xamarin.Essentials;
using Location = IndoorNavigation.ViewModels.Location;
using static IndoorNavigation.Utilities.Storage;
using System.Threading.Tasks;
using System.Windows.Input;
using static IndoorNavigation.Utilities.TmperorayStatus;
using IndoorNavigation.Resources;
using Rg.Plugins.Popup.Events;

namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : CustomToolbarContentPage
    {
        MainPageViewModel _viewModel;                 
        ViewCell lastCell = null;
        bool isButtonPressed = false; //to prevent multi-click       

        public MainPage()
        {
            InitializeComponent();

            NavigationPage.SetBackButtonTitle(this,
                GetResourceString("HOME_STRING"));
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

            ((NavigationPage)Application.Current.MainPage).BarBackgroundColor =
                Color.FromHex("#3F51B5");
            ((NavigationPage)Application.Current.MainPage).BarTextColor =
                Color.White;

            _viewModel = new MainPageViewModel();
            BindingContext = _viewModel;
            RefreshToolbarOptions();
            //RefreshListView();
        }

        private INetworkSetting setting;
        private CloudDownload _download = new CloudDownload();

        async void SettingBtn_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingTableViewPage());
        }

        async private Task CheckVersionAndUpdate_(Location location,
            NavigationGraph navigationGraph)
        {
            if (_serverResources != null &&
                CheckVersionNumber(location.sourcePath,
                navigationGraph.GetVersion(),
                AccessGraphOperate.CheckCloudVersion))
            {
                AlertDialogPopupPage page = new AlertDialogPopupPage
                    (GetResourceString("UPDATE_MAP_STRING"),
                    GetResourceString("OK_STRING"),
                    GetResourceString("CANCEL_STRING"));

                IndicatorPopupPage busyPage = new IndicatorPopupPage();

                bool wantToUpdate = await page.show();

                if (wantToUpdate)
                {
                    await PopupNavigation.Instance.PushAsync(busyPage);

                    try { CloudGenerateFile(location.sourcePath); }
                    catch (Exception exc)
                    {
                        Console.WriteLine("updated error - " + exc.Message);

                        await PopupNavigation.Instance.RemovePageAsync
                            (busyPage);

                        if (Connectivity.NetworkAccess ==
                            NetworkAccess.Internet)
                        {
                            Console.WriteLine("network is fine but error");

                            AlertDialogPopupPage noResponsePage =
                                new AlertDialogPopupPage(
                                    GetResourceString("HAPPEND_ERROR_STRING"),
                                    GetResourceString("OK_STRING"),
                                    GetResourceString("CANCEL_STRING"));

                            bool WantRetry = await noResponsePage.show();

                            if (WantRetry)
                            {
                                await CheckVersionAndUpdate_(location,
                                    navigationGraph);
                            }
                        }
                        else
                        {
                            AlertDialogPopupPage noNetworkPage =
                                new AlertDialogPopupPage(
                                    GetResourceString("BAD_NETWORK_STRING"),
                                    GetResourceString("OK_STRING"),
                                    GetResourceString("CANCEL_STRING"));

                            bool wantRetry = await noNetworkPage.show();

                            if (wantRetry)
                            {
                                setting.OpenSettingPage();

                                await CheckVersionAndUpdate_(location,
                                    navigationGraph);
                            }
                        }
                    }

                    if (PopupNavigation.Instance.PopupStack.Contains(busyPage))
                        await PopupNavigation.Instance.RemovePageAsync
                            (busyPage);
                }

            }
            await Task.CompletedTask;
        }

        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
            if (e.Item is Location location)
            {
                NavigationGraph navigationGraph =
                    LoadNavigationGraphXml(location.sourcePath);

                if (!FirstTimeUse)
                {
                    AlertDialogPopupPage alertPage =
                        new AlertDialogPopupPage
                        (GetResourceString
                        ("WELCOME_USE_START_TO_ADJUST_STRING"),
                        AppResources.OK_STRING);

                    await alertPage.show();

                    AutoAdjustPopupPage autoAdjustPage =
                        new AutoAdjustPopupPage(location.sourcePath);

                    FirstTimeUse = await autoAdjustPage.Show(); ;
                    Console.WriteLine("FirstTime use : " + FirstTimeUse);
                }
                await CheckVersionAndUpdate_(location, navigationGraph);
                {
                    if (isButtonPressed) return;
                    isButtonPressed = true;

                    switch (navigationGraph.GetIndustryServer())
                    {

                        case "hospital":
                            if (location.UserNaming ==
                                GetResourceString
                                ("YUANLIN_CHRISTIAN_HOSPITAL_STRING"))

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
            Device.BeginInvokeOnMainThread(async () =>
            {
                bool isAgree =
                    await DisplayAlert
                        (GetResourceString("MESSAGE_STRING"),
                         GetResourceString("ASK_LEAVE_APP_STRING"),
                         GetResourceString("LEAVE_STRING"),
                         GetResourceString("CANCEL_STRING"));

                if (isAgree)
                {
                    ((App)Application.Current).OnStop();
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

        async private void Item_Delete(object sender, EventArgs e)
        {
            var item = (Location)((MenuItem)sender).CommandParameter;
            if (item != null)
            {

                if (_viewModel.NavigationGraphFiles.Count() <= 1)
                {
                    await PopupNavigation.Instance.PushAsync
                        (new AlertDialogPopupPage
                        (GetResourceString("AT_LEAST_ONE_STRING"),
                        //AppResources.AT_LEAST_ONE_STRING,
                        AppResources.OK_STRING));
                    return;
                }

                await PopupNavigation.Instance.PushAsync
                    (new AlertDialogPopupPage
                    (string.Format(
                        //AppResources.DO_YOU_WANT_TO_DELETE_IS_STRING,
                        GetResourceString("DO_YOU_WANT_TO_DELETE_IS_STRING"),
                        item.UserNaming),
                        AppResources.YES_STRING,
                        AppResources.NO_STRING, "ConfirmDelete"));

                MessagingCenter.Subscribe<AlertDialogPopupPage, bool>(this, "ConfirmDelete", (msgSender, msgArgs) =>
                {
                    Console.WriteLine("the return Args is :" + (bool)msgArgs);
                    if ((bool)msgArgs) Storage.DeleteBuildingGraph(item.sourcePath);

                    _viewModel.LoadNavigationGraph();
                    MessagingCenter.Unsubscribe<AlertDialogPopupPage, bool>(this, "ConfirmDelete");
                });
            }
        }

        #region iOS SecondToolBarItem Attributes

        async private Task AddSiteItemMethod_()
        {
            IndicatorPopupPage busyPage = new IndicatorPopupPage();

            if(Connectivity.NetworkAccess != NetworkAccess.Internet) 
            {
                setting = DependencyService.Get<INetworkSetting>();

                AlertDialogPopupPage noNetworkPage = new AlertDialogPopupPage(
                    GetResourceString("BAD_NETWORK_STRING"),
                    GetResourceString("OK_STRING"),
                    GetResourceString("CANCEL_STRING"));
                bool wantToSetting = await noNetworkPage.show();

                if (wantToSetting)
                {
                    setting.OpenSettingPage();
                }
            }
            else if(Connectivity.NetworkAccess == NetworkAccess.Internet) 
            {
                Console.WriteLine("The network is fine");

                await PopupNavigation.Instance.PushAsync(busyPage);

                _download = new CloudDownload();
                try
                {
                    string supportXmlString =
                        _download.Download(_download.getSupportListUrl());

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(supportXmlString);
                    _serverResources = GraphInfoReader(doc);

                    await Navigation.PushAsync(new EditLocationPage());
                }
                catch(Exception exc)
                {
                    Console.WriteLine("Enter add site error - " + exc.Message);
                    AlertDialogPopupPage noResponsePage =
                        new AlertDialogPopupPage
                        (GetResourceString("HAPPEND_ERROR_STRING"), 
                        GetResourceString("OK_STRING"), 
                        GetResourceString("CANCEL_STRING"));

                    bool wantRetry = await noResponsePage.show();

                    if (wantRetry)
                        await AddSiteItemMethod_();
                }
                finally
                {
                    await PopupNavigation.Instance.RemovePageAsync(busyPage);
                }
            }
        }
        
        public ICommand SettingCommand { get; set; }
        public ICommand AddSiteCommand { get; set; }
        private void RefreshToolbarOptions()
        {
            ToolbarItems.Clear();
            SettingCommand = new Command(async () =>await SettingItemMethod());
            AddSiteCommand = new Command(async () =>await AddSiteItemMethod_());

            TestItemCommand = new Command(async () => await TestItemMethod());

            ToolbarItem SettingItem = new ToolbarItem
            {
                Text = GetResourceString("SETTING_STRING"),
                Command = SettingCommand,
                Order = ToolbarItemOrder.Secondary
            };

            ToolbarItem NewSiteToolbarItem = new ToolbarItem
            {
                Text = GetResourceString("NEW_STRING"),
                Command =AddSiteCommand,
                Order = ToolbarItemOrder.Primary
            };

            ToolbarItem TestToolbarItem = new ToolbarItem
            {
                Text = "Test",
                Command = TestItemCommand,
                Order = ToolbarItemOrder.Secondary
            };

            ToolbarItems.Add(SettingItem);
            ToolbarItems.Add(NewSiteToolbarItem);
            //ToolbarItems.Add(TestToolbarItem);
            OnToolbarItemAdded();
        }
        async private Task SettingItemMethod()
        {
            Console.WriteLine(">>SettingItemMethod");

            await Navigation.PushAsync(new SettingTableViewPage());

            await Task.CompletedTask;
        }

        protected void OnToolbarItemAdded()
        {
            Console.WriteLine("call onToolbarItemAdded");
            EventHandler e = ToolbarItemAdded;
            e?.Invoke(this, new EventArgs());
        }

        public override event EventHandler ToolbarItemAdded;
        public override Color CellBackgroundColor => Color.White;
        public override Color CellTextColor => Color.Black;
        public override Color MenuBackgroundColor => Color.White;
        public override float RowHeight => 56;
        public override Color ShadowColor => Color.Black;
        public override float ShadowOpacity => 0.3f;
        public override float ShadowRadius => 5.0f;
        public override float ShadowOffsetDimension => 5.0f;
        public override float TableWidth => 250;
        #endregion

        #region TestItem 

        public ICommand TestItemCommand { get; set; }

        async private Task TestItemMethod()
        {
            await PopupNavigation.Instance.PushAsync
                (new AutoAdjustPopupPage("CCH_Debug"));
            await Task.CompletedTask;
        }

        #endregion
    }
}