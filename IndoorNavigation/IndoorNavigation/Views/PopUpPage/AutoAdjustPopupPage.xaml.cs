using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Rg.Plugins.Popup.Pages;

using IndoorNavigation.Modules;
using IndoorNavigation.Models.NavigaionLayer;
using static IndoorNavigation.Utilities.Storage;
using static IndoorNavigation.Utilities.TmperorayStatus;
using System.Threading;
using System.Net.Http.Headers;
using IndoorNavigation.Modules.IPSClients;
using Rg.Plugins.Popup.Services;
using MvvmHelpers;
using IndoorNavigation.Resources;
using Rg.Plugins.Popup.Extensions;
using System.Globalization;
using Plugin.Multilingual;
using System.Resources;
using IndoorNavigation.Resources.Helpers;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace IndoorNavigation.Views.PopUpPage
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AutoAdjustPopupPage : PopupPage
    {
        /*  1. enlarge the rssi option to detect the strongest signal beacon 
         *     to know where user is.
         *     BTW : if we can't detect any beacon belong to use -> 
         *     return exception
         *  2. Show current position in view. And show "detecting..." string.
         *  3. scan about 30 sec and store all rssi in every scan, then 
         *     calculate result(average? median?)
         *  4. accroding to result, apply to "rssi options number".
         *  5. if user thinks it is still to fast or slow, user can adjust 
         *     again or user can manual adjust.
         */

        #region Varialbes and structures
        //current waypoint
        private Guid _currentWaypointID;
        private Guid _currentRegionID;
        private int _currentBeaconRssi;
        private double ProgressValue = 0.66;
        private string naviGraphName;
        private bool _isKeepDetection = true;
        private bool _positionIsCorrect = false;

        private IPSmodule_ _ipsModules;        
        private NavigationGraph _navigationGraph;        
        private Thread _detectWaypointThread;
        private Thread _detectPositionControllThread;
        private List<int> _AllRssiList;
        private XMLInformation _nameInformation;
        private ManualResetEventSlim _detectThreadEvent;
        private ManualResetEventSlim _askCorrectEvent;
        private ManualResetEventSlim _startToScanRssi;
        

        private TaskCompletionSource<bool> _tcs = null;

        #endregion

        #region Page life cycle
        public AutoAdjustPopupPage(string _naviGraphName)
        {
            InitializeComponent();
            Console.WriteLine(">>AutoAdjustPopupPage : Constructor");
            naviGraphName = _naviGraphName;
            _navigationGraph = LoadNavigationGraphXml(naviGraphName);

            _detectThreadEvent = new ManualResetEventSlim(false);
            _askCorrectEvent = new ManualResetEventSlim(false);
            _startToScanRssi = new ManualResetEventSlim(false);

            _ipsModules = new IPSmodule_(_navigationGraph);
            _AllRssiList = new List<int>();

            _nameInformation = LoadXmlInformation(_naviGraphName);

            _currentRegionID = new Guid();
            _currentWaypointID = new Guid();

            _ipsModules._event._eventHandler +=
                new EventHandler(DetecWaypointResult);

            _detectWaypointThread = new Thread(() => InvokeIPSWork());
            _detectPositionControllThread = new Thread(() => ThreadWork());//ScanPosition());

            Console.WriteLine("<<AutoAdjustPopupPage : Constructor");
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();


        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _isKeepDetection = false;
            _ipsModules.CloseAllActiveClient();
        }
        #endregion


        #region scan position and rssi functions
        private void ThreadWork()
        {
            while (_isKeepDetection)
            {
                _ipsModules.InitialStep_DetectAllBeacon
                    (_navigationGraph.GetAllRegionIDs());

                _ipsModules.StartAllExistClient();
                _detectThreadEvent.Wait();


                if (isEmptyGuid(_currentRegionID) || isEmptyGuid(_currentWaypointID))
                {
                    Console.WriteLine("WaypointID is empty.");                   
                    _detectThreadEvent.Reset();
                    continue;                        
                }
                Console.WriteLine("Detect region ID : " + _currentRegionID);
                Console.WriteLine("Detect waypoint ID : " + _currentWaypointID);

                Device.BeginInvokeOnMainThread(() => SetCheckPositionCorrect());

                _askCorrectEvent.Wait();

                if (!_positionIsCorrect)
                {
                    Device.BeginInvokeOnMainThread(() => SetStartScanView());

                    _detectThreadEvent.Reset();
                    _askCorrectEvent.Reset();

                    _currentRegionID = Guid.Empty;
                    _currentWaypointID = Guid.Empty;
                    continue;
                }
                break;
            }

            string currentPosition =
                _nameInformation.GiveWaypointName(_currentWaypointID);

            Device.BeginInvokeOnMainThread(() => SetScanRssiView(currentPosition));
            _ipsModules.CloseAllActiveClient();
            DetectPositionThreshold();
        }

        private void DetecWaypointResult(object sender, EventArgs args)
        {
            Console.WriteLine(">>DetectWaypointResult");
            if (args is WaypointSignalEventArgs signalArgs)
            {
                _currentRegionID =
                  signalArgs._detectedRegionWaypoint
                  ._regionID;

                _currentWaypointID =
                    signalArgs._detectedRegionWaypoint
                    ._waypointID;

                _ipsModules.CloseAllActiveClient();
            } else if (args is WaypointRssiEventArgs RssiArgs)
            {
                _currentBeaconRssi = RssiArgs._BeaconThreshold;
                _AllRssiList.Add(RssiArgs._scanBeaconRssi);
                Console.WriteLine("Current scan rssi : " + RssiArgs._scanBeaconRssi);
                Console.WriteLine("Current beacon threshold : " + RssiArgs._BeaconThreshold);
            }
            _detectThreadEvent.Set();
            Console.WriteLine("<<DetectWaypointResult");
        }       

        //To scan current beacon rssi
        private void DetectPositionThreshold()
        {
            _ipsModules.SetRssiMonitor(_currentRegionID, _currentWaypointID);
            int count = 0;


            Device.StartTimer(TimeSpan.FromMilliseconds(500), () =>
            {
                //_detectThreadEvent.Wait();
                Console.WriteLine(">>StartTimer, count : " + count);
                _ipsModules.OpenRssiScaning();
                ProgressValue += 0.01;
                //AutoAdjustProgressBar.SetValue()
                AutoAdjustProgressBar.ProgressTo(ProgressValue, 250, Easing.Linear);
                if (count++ == 33)
                {
                    SetRssiOption();
                    _ipsModules.CloseAllActiveClient();
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        //show finish scan page                     
                        await PopupNavigation.Instance.RemovePageAsync(this);

                        AlertDialogPopupPage alertPage =
                            new AlertDialogPopupPage
                            (
                                getResourceString("IF_NOT_STABLEGO_TO_PREFER_STRING"),
                            //AppResources.IF_NOT_STABLEGO_TO_PREFER_STRING,
                            AppResources.OK_STRING);

                        bool isReturn = await alertPage.show();

                        _tcs?.SetResult(true);
                    });


                    return false;
                }
                return true;
            });
            Console.WriteLine("<<DetectPositionThreshold");
        }

        private void InvokeIPSWork()
        {
            while (_isKeepDetection == true)
            {
                Console.WriteLine(">>InvokeIPSWork");
                //keep detection.
                Thread.Sleep(500);
                _ipsModules.OpenBeaconScanning();
                Console.WriteLine("<<InvokeIPSWork");
            }
        }
        #endregion


        async private void CancelBtn_Clicked(object sender, EventArgs e)
        {
            ConfirmBtn.Clicked -= CancelBtn_Clicked;
            _tcs?.SetResult(false);
            await PopupNavigation.Instance.RemovePageAsync(this);
        }


        private void StartBtn_Clicked(object sender, EventArgs e)
        {
            SetStartScanView();
            // start to scan position
            ConfirmBtn.Clicked -= StartBtn_Clicked;
            _detectPositionControllThread.Start();
            _detectWaypointThread.Start();
        }

        private void SetStartScanView()
        {            
            AutoAdjustLayout.Children.Clear();

            #region Button Control
            //ConfirmBtn.Clicked -= StartBtn_Clicked;
            ConfirmBtn.Clicked += CancelBtn_Clicked;
            ConfirmBtn.Text = "取消";
            #endregion

            AutoAdjustLayout.Children.Add(new Label
            {
                Text = "正在偵測位置",
                FontSize = 32
            });

            AutoAdjustLayout.Children.Add(new Image
            {
                Source = "waittingscan.gif",
                IsAnimationPlaying = true
            });

            AutoAdjustProgressBar.ProgressTo(0.33, 250, Easing.Linear);

        }

        #region to Check Position is correct or wrong with user.
        private void SetCheckPositionCorrect()
        {
            AutoAdjustLayout.Children.Clear();


            #region ConfirmButton
            ConfirmBtn.Clicked -= CancelBtn_Clicked;
            ConfirmBtn.Clicked += CheckPositionCorrectBtn_Clicked;
            ConfirmBtn.Text = "正確";
            #endregion

            #region CancelButton
            CancelBtn.IsVisible = true;
            CancelBtn.Clicked += CheckPositionWrongBtn_Clicked;
            CancelBtn.Text = "錯誤";
            #endregion
            AutoAdjustLayout.Children.Add(
                new Label
                {
                    Text = "目前位置 : " + _nameInformation.GiveWaypointName(_currentWaypointID),
                    //_navigationGraph.GetWaypointNameInRegion(_currentRegionID, _currentWaypointID),
                    FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
                    TextColor = Color.FromHex("#3f51b5")
                }) ;
        }

        private void CheckPositionCorrectBtn_Clicked(object sender, 
            EventArgs e)
        {
            Console.WriteLine(">>CheckPositionCorrect");

            _positionIsCorrect = true;
            _askCorrectEvent.Set();
            _startToScanRssi.Set();
            ConfirmBtn.Clicked -= CheckPositionCorrectBtn_Clicked;
            Console.WriteLine("<<CheckPositionCorrect");
        }

        private void CheckPositionWrongBtn_Clicked(object sender, EventArgs e)
        {
            Console.WriteLine(">>CheckWrongPositionWrong");

            _positionIsCorrect = false;
            _askCorrectEvent.Set();
            CancelBtn.Clicked -= CheckPositionWrongBtn_Clicked;
            Console.WriteLine("<<CheckWrongPositionWrong");
        }

        #endregion

        #region  RssiScan View and Functions
        private void SetScanRssiView(string currentPosition)
        {
            #region Button control
            //ConfirmBtn.Clicked -= CheckPositionCorrectBtn_Clicked;
            ConfirmBtn.Clicked += CancelBtn_Clicked;
            ConfirmBtn.Text = "取消";
            #endregion

            AutoAdjustLayout.Children.Clear();

            AutoAdjustLayout.Children.Add(new Label
            {
                //show current position.
                Text = string.Format(AppResources.CURRENT_LOCATION_STRING,
                currentPosition),
                FontSize = 32
            });

            AutoAdjustLayout.Children.Add(new Label
            {
                Text=getResourceString("SCAN_RSSI_NOW_STRING"),
                //Text = AppResources.SCAN_RSSI_NOW_STRING,
                Margin = new Thickness(10,0,10,0),
                FontSize = 28
            });

            AutoAdjustLayout.Children.Add(new Image
            {
                Source = "waittingscan.gif",
                IsAnimationPlaying = true
            });

            AutoAdjustProgressBar.ProgressTo(0.66, 250, Easing.Linear);
        }

        #endregion

        #region Another functions
        private void SetRssiOption()
        {

            _AllRssiList.Sort();

            int count = _AllRssiList.Count;
            int mid = count / 2;
            int midian = count % 2 != 0 ?
                (int)_AllRssiList[mid] :
                (int)((_AllRssiList[mid] + _AllRssiList[mid + 1]) / 2);

            RssiOption = ((int)_currentBeaconRssi - midian) + 2;
            Console.WriteLine("Result is : " + (_currentBeaconRssi - midian));
        }

        private bool isEmptyGuid(Guid guid)
        {
            return Guid.Empty.Equals(guid);
        }

        private string getResourceString(string key)
        {
            string resourceId = "IndoorNavigation.Resources.AppResources";

            CultureInfo currentLanguage =
                CrossMultilingual.Current.CurrentCultureInfo;
            ResourceManager resourceManager =
                new ResourceManager(resourceId,
                                    typeof(TranslateExtension)
                                    .GetTypeInfo().Assembly);

            return resourceManager.GetString(key, currentLanguage);
        }

        async public Task<bool> Show()
        {
            _tcs = new TaskCompletionSource<bool>();

            await Navigation.PushPopupAsync(this);
            return await _tcs.Task;
        }
        
        #endregion
    }

}