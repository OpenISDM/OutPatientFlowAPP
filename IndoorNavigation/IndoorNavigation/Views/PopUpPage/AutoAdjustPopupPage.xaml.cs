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
        private IPSmodule_ _ipsModules;
        private string naviGraphName;
        private NavigationGraph _navigationGraph;

        private bool _isKeepDetection = true;
        private Thread _detectWaypointThread;
        private Thread _detectPositionControllThread;
        private List<int> _AllRssiList;
        private XMLInformation _nameInformation;
        private ManualResetEventSlim _detectThreadEvent;

        private TaskCompletionSource<bool> _tcs = null;
        string _currentPosition = AppResources.CURRENT_LOCATION_STRING;
        #endregion
        public AutoAdjustPopupPage(string _naviGraphName)
        {
            InitializeComponent();
            Console.WriteLine(">>AutoAdjustPopupPage : Constructor");
            naviGraphName = _naviGraphName;
            _navigationGraph = LoadNavigationGraphXml(naviGraphName);
            _detectThreadEvent = new ManualResetEventSlim(false);
            _ipsModules = new IPSmodule_(_navigationGraph);
            _AllRssiList = new List<int>();

            _nameInformation = LoadXmlInformation(_naviGraphName);

            _currentRegionID = new Guid();
            _currentWaypointID = new Guid();

            _ipsModules._event._eventHandler +=
                new EventHandler(DetecWaypointResult);
            
            _detectWaypointThread = new Thread(() => InvokeIPSWork());
            _detectPositionControllThread = new Thread(() => ScanPosition());          

            Console.WriteLine("<<AutoAdjustPopupPage : Constructor");
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();


        }
        private double ProgressValue = 0.66;
        private bool isEmptyGuid(Guid guid)
        {
            return Guid.Empty.Equals(guid);
        }

        //to detect where user is.
        private void ScanPosition()
        {
            _ipsModules.InitialStep_DetectAllBeacon
                (_navigationGraph.GetAllRegionIDs());
            _ipsModules.StartAllExistClient();
            _detectThreadEvent.Wait();

            //if we don't know where user is.
            if(isEmptyGuid(_currentRegionID) &&
                isEmptyGuid(_currentWaypointID))
            {
                _detectThreadEvent.Reset();
                ScanPosition();                
            }

            

            Device.BeginInvokeOnMainThread(() =>
            {
                string currentPosition =
                    _nameInformation.GiveWaypointName(_currentWaypointID);
                SetScanRssiView(currentPosition);
            });

            _isKeepDetection = false;

            _ipsModules.CloseAllActiveClient();

            DetectPositionThreshold();         
        }           

      

        private void DetecWaypointResult(object sender, EventArgs args)
        {
            if (args is WaypointSignalEventArgs signalArgs)
            {
                _currentRegionID =
                  signalArgs._detectedRegionWaypoint
                  ._regionID;

                _currentWaypointID =
                    signalArgs._detectedRegionWaypoint
                    ._waypointID;

                _ipsModules.CloseAllActiveClient();
            }else if(args is WaypointRssiEventArgs RssiArgs)
            {
                _currentBeaconRssi = RssiArgs._BeaconThreshold;
                _AllRssiList.Add(RssiArgs._scanBeaconRssi);
                Console.WriteLine("Current scan rssi : " + RssiArgs._scanBeaconRssi);
                Console.WriteLine("Current beacon threshold : " + RssiArgs._BeaconThreshold);
            }
            _detectThreadEvent.Set();
        }

        private void SetRssiOption()
        {

            _AllRssiList.Sort();

            int count = _AllRssiList.Count;
            int mid = count / 2;
            int midian = count % 2 != 0 ?
                (int)_AllRssiList[mid] :
                (int)((_AllRssiList[mid] + _AllRssiList[mid + 1]) / 2);
         
            RssiOption = (int)_currentBeaconRssi - midian;
            Console.WriteLine("Result is : " + (_currentBeaconRssi - midian));
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
                if(count++ == 33)
                {
                    SetRssiOption();
                    _ipsModules.CloseAllActiveClient();
                    Device.BeginInvokeOnMainThread(async() =>
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
            while(_isKeepDetection == true)
            {
                //keep detection.
                Thread.Sleep(500);
                _ipsModules.OpenBeaconScanning();
            }
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
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _isKeepDetection = false;
            _ipsModules.CloseAllActiveClient();
        }

        #region Click event function
        async private void CancelBtn_Clicked(object sender, EventArgs e)
        {
            _tcs?.SetResult(false);
            await PopupNavigation.Instance.RemovePageAsync(this);
        }

        private void StartBtn_Clicked(object sender, EventArgs e)
        {
            SetStartScanView();

            // start to scan position
            _detectPositionControllThread.Start();
            _detectWaypointThread.Start();
        }

        private void SetStartScanView()
        {
            #region control button show
            StartBtn.IsVisible = false;
            CancelBtn.IsVisible = true;
            #endregion

            AutoAdjustLayout.Children.Clear();

            AutoAdjustLayout.Children.Add(new Label
            {
                Text="正在偵測位置",
                FontSize = 32
            });

            AutoAdjustLayout.Children.Add(new Image
            {
                Source = "waittingscan.gif",
                IsAnimationPlaying = true
            });

            AutoAdjustProgressBar.ProgressTo(0.33, 250, Easing.Linear);
            
        }
       
        private void SetAskPositionCorrect()
        {
            AutoAdjustLayout.Children.Clear();

            AutoAdjustLayout.Children.Add(
                new Label
                {
                    Text="目前位置 : "
                });
        }

        private void SetScanRssiView(string currentPosition)
        {
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

        async public Task<bool> Show()
        {
            _tcs = new TaskCompletionSource<bool>();

            await Navigation.PushPopupAsync(this);
            return await _tcs.Task;
        }
        #endregion
    }

}