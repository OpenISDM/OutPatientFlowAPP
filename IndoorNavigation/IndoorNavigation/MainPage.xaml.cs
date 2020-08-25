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

namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : CustomToolbarContentPage
    {
        MainPageViewModel _viewModel;
        internal static readonly string _versionRoute =
            "IndoorNavigation.Resources.Map_Version.xml";

        const string _resourceId =
            "IndoorNavigation.Resources.AppResources";

        internal static readonly string _versionRouteInPhone
             = Path.Combine(Environment.GetFolderPath
                 (Environment.SpecialFolder.LocalApplicationData), "Version");

        ResourceManager _resourceManager =
            new ResourceManager(_resourceId,
                                typeof(TranslateExtension).GetTypeInfo()
                                .Assembly);

        ViewCell lastCell = null;
        bool isButtonPressed = false; //to prevent multi-click
        CultureInfo currentLanguage =
            CrossMultilingual.Current.CurrentCultureInfo;

        public MainPage()
        {
            InitializeComponent();
            Console.WriteLine("Nameof (_viewModel) = " + nameof(_viewModel));
            NavigationPage.SetBackButtonTitle(this,
                                              _resourceManager
                                              .GetString("HOME_STRING",
                                                         currentLanguage));
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
        
        async private void CheckVersionAndUpdate(Location location, 
            NavigationGraph navigationGraph)
        {
            if (_serverResources != null &&
                  CheckVersionNumber(location.sourcePath, 
                  navigationGraph.GetVersion(), 
                  AccessGraphOperate.CheckCloudVersion))
            {
                bool WantUpdate = 
                    await DisplayAlert(
                        _resourceManager.GetString
                        ("INFO_STRING",currentLanguage), 
                    _resourceManager.GetString("UPDATE_MAP_STRING",currentLanguage), 
                    _resourceManager.GetString("UPDATE_STRING",currentLanguage),
                    _resourceManager.GetString("NO_STRING",currentLanguage));

                if (WantUpdate)
                {
                    IndicatorPopupPage busyPage = new IndicatorPopupPage();

                    await PopupNavigation.Instance.PushAsync(busyPage);

                    try
                    {
                        CloudGenerateFile(location.sourcePath);
                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine("updated error - " +
                            exc.Message);
                        await PopupNavigation.Instance.RemovePageAsync(busyPage);
                        //show no network page or server not response error.

                        if(Connectivity.NetworkAccess == NetworkAccess.Internet)
                        {
                            await PopupNavigation.Instance.PushAsync
                                (new AlertDialogPopupPage
                                (_resourceManager.GetString
                               ("HAPPEND_ERROR_STRING", currentLanguage),
                               _resourceManager.GetString
                               ("RETRY_STRING", currentLanguage),
                               _resourceManager.GetString
                               ("NO_STRING", currentLanguage), "WantRetry"));
                            MessagingCenter.Subscribe
                                <AlertDialogPopupPage, bool>
                                (this, "WantRetry", (msgSender, msgArgs) =>
                            {
                                if ((bool)msgArgs) 
                                    CheckVersionAndUpdate
                                    (location, navigationGraph);

                                MessagingCenter.Unsubscribe
                                <AlertDialogPopupPage, bool>
                                (this, "WantRetry");
                            });
                        }
                        else
                        {
                            await PopupNavigation.Instance.PushAsync
                                (new AlertDialogPopupPage
                                (_resourceManager.GetString
                               ("BAD_NETWORK_STRING", currentLanguage),
                               _resourceManager.GetString
                               ("GO_TO_SETTING", currentLanguage),
                               _resourceManager.GetString
                               ("NO_STRING", currentLanguage),  
                               "GoToSetting"));
                            MessagingCenter.Subscribe
                                <AlertDialogPopupPage, bool>
                                (this, "GoToSetting", (msgSender, msgArgs) =>
                                {
                                    if ((bool)msgArgs)
                                    { 
                                        setting = 
                                        DependencyService
                                        .Get<INetworkSetting>();

                                        setting.OpenSettingPage();

                                        CheckVersionAndUpdate
                                        (location, navigationGraph);
                                    }
                                    
                                    MessagingCenter.Unsubscribe
                                    <AlertDialogPopupPage, bool>
                                    (this, "GoToSetting");
                                });
                        }
                    }

                    if(PopupNavigation.Instance.PopupStack.Contains(busyPage))
                        await PopupNavigation.Instance.RemovePageAsync
                            (busyPage);
                }
            }

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
                    Console.WriteLine("it's first Time use!");

                    AlertDialogPopupPage alertPage =
                        new AlertDialogPopupPage
                        (AppResources.WELCOME_USE_START_TO_ADJUST_STRING, 
                        AppResources.OK_STRING);

                    bool isReturn = await alertPage.show();

                    AutoAdjustPopupPage autoAdjustPage =
                        new AutoAdjustPopupPage(location.sourcePath);

                    isReturn = await autoAdjustPage.Show();

                    Console.WriteLine("aaaaaaaaa");
                    FirstTimeUse = true;
                    Console.WriteLine("FirstTime use : " + FirstTimeUse);
                }

                #region
                //this place will implement the check server side resource.
                //if(CheckVersionNumber(location.sourcePath, 
                //    navigationGraph.GetVersion(), 
                //    AccessGraphOperate.CheckLocalVersion))
                //{
                //    bool WantToUpdate = await DisplayAlert("info", "有更新版本，是否要更新?", "是", "否");

                //    if (WantToUpdate) EmbeddedGenerateFile(location.sourcePath);
                //}
                #endregion

                CheckVersionAndUpdate(location, navigationGraph);
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
            Device.BeginInvokeOnMainThread(async () =>
            {
                bool isAgree =
                    await DisplayAlert
                        (_resourceManager
                         .GetString("MESSAGE_STRING", currentLanguage),
                         _resourceManager
                         .GetString("ASK_LEAVE_APP_STRING", currentLanguage),
                         _resourceManager
                         .GetString("LEAVE_STRING", currentLanguage),
                         _resourceManager
                         .GetString("CANCEL_STRING", currentLanguage));

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
                        (AppResources.AT_LEAST_ONE_STRING,
                        AppResources.OK_STRING));
                    return;
                }

                await PopupNavigation.Instance.PushAsync
                    (new AlertDialogPopupPage
                    (string.Format(
                        AppResources.DO_YOU_WANT_TO_DELETE_IS_STRING,
                        item.UserNaming),
                        AppResources.YES_STRING,
                        AppResources.NO_STRING,"ConfirmDelete"));

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
        async private Task AddSiteItemMethod()
        {
            IndicatorPopupPage busyPopupPage = new IndicatorPopupPage();
            _download = new CloudDownload();

            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                setting = DependencyService.Get<INetworkSetting>();

                await PopupNavigation.Instance.PushAsync
                               (new AlertDialogPopupPage
                               (_resourceManager.GetString
                               ("BAD_NETWORK_STRING", currentLanguage),
                               _resourceManager.GetString
                               ("GO_TO_SETTING", currentLanguage),
                               _resourceManager.GetString
                               ("NO_STRING", currentLanguage),
                               "GoToSettingInAdd"));
                MessagingCenter.Subscribe
                    <AlertDialogPopupPage, bool>
                    (this, "GoToSettingInAdd", (msgSender, msgArgs) =>
                    {
                        if ((bool)msgArgs)
                        {
                            setting.OpenSettingPage();
                        }

                        MessagingCenter.Unsubscribe
                        <AlertDialogPopupPage, bool>
                        (this, "GoToSettingInAdd");
                    });
            }
            else if (_serverResources != null)
            {
                Console.WriteLine("_serverResource != null");
                await PopupNavigation.Instance.PushAsync(busyPopupPage);
                await Navigation.PushAsync(new EditLocationPage());
                await PopupNavigation.Instance.RemovePageAsync(busyPopupPage);
            }
            else if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                Console.WriteLine("The network is fine");
                await PopupNavigation.Instance.PushAsync(busyPopupPage);
                try
                {
                    string SupportResourceXmlString =
                        _download.Download(_download.getSupportListUrl());

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(SupportResourceXmlString);
                }
                catch
                {
                    await PopupNavigation.Instance.PushAsync
                               (new AlertDialogPopupPage
                               (_resourceManager.GetString
                               ("HAPPEND_ERROR_STRING", currentLanguage),
                               _resourceManager.GetString
                               ("RETRY_STRING", currentLanguage),
                               _resourceManager.GetString
                               ("NO_STRING", currentLanguage),
                               "WantRetryInAdd"));
                    MessagingCenter.Subscribe
                        <AlertDialogPopupPage, bool>
                        (this, "WantRetryInAdd", async (msgSender, msgArgs) =>
                        {
                            if ((bool)msgArgs)
                            {
                                await PopupNavigation.Instance.
                                RemovePageAsync(busyPopupPage);
                                await AddSiteItemMethod();
                            }
                            else
                            {
                                await PopupNavigation.Instance.
                                RemovePageAsync(busyPopupPage);
                            }
                            MessagingCenter.Unsubscribe
                                    <AlertDialogPopupPage, bool>
                                    (this, "WantRetryInAdd");
                        });
                }
            }

        }
        public ICommand SettingCommand { get; set; }
        public ICommand AddSiteCommand { get; set; }
        private void RefreshToolbarOptions()
        {
            ToolbarItems.Clear();
            SettingCommand = new Command(async () =>await SettingItemMethod());
            AddSiteCommand = new Command(async () =>await AddSiteItemMethod());

            TestItemCommand = new Command(async () => await TestItemMethod());

            ToolbarItem SettingItem = new ToolbarItem
            {
                Text = 
                _resourceManager.GetString("SETTING_STRING", currentLanguage),
                Command = SettingCommand,
                Order = ToolbarItemOrder.Secondary
            };

            ToolbarItem NewSiteToolbarItem = new ToolbarItem
            {
                Text = 
                    _resourceManager.GetString("NEW_STRING",currentLanguage),
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
            //await Navigation.PushAsync(new TestPage());
            //await PopupNavigation.Instance.PushAsync(new SelectPurposePopupPage("Lab"));
            await PopupNavigation.Instance.PushAsync(new AutoAdjustPopupPage("Taipei_City_Hall"));
            //await PopupNavigation.Instance.PushAsync(new DownloadPopUpPage());
            await Task.CompletedTask;
        }

        #endregion
    }
}