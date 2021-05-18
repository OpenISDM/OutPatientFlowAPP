using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Rg.Plugins.Popup.Pages;
using IndoorNavigation.Modules;
using IndoorNavigation.Models.NavigaionLayer;
using static IndoorNavigation.Utilities.Storage;
using static IndoorNavigation.Utilities.TmperorayStatus;
using System.Threading;
using IndoorNavigation.Modules.IPSClients;
using Rg.Plugins.Popup.Services;
using IndoorNavigation.Resources;
using Rg.Plugins.Popup.Extensions;

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
        private Guid _currentWaypointID;
        private Guid _currentRegionID;
        private int _currentBeaconRssi;

        private double ProgressValue = 0.66;
        private string naviGraphName;
        private bool _isKeepDetection = true;

        private IPSModule _ipsModules;
        private NavigationGraph _navigationGraph;
        private Thread _detectWaypointThread;
        private Thread _detectPositionControllThread;
        private List<int> _AllRssiList;
        private XMLInformation _nameInformation;
        private ManualResetEventSlim _detectThreadEvent;
        private ManualResetEventSlim _askCorrectEvent;

        private int TmpRssiOption;
        private bool isKeepDetectionRssi = true;
        private TaskCompletionSource<bool> _tcs = null;
        private bool IsCancel = false;
        #endregion

        #region Page life cycle
        public AutoAdjustPopupPage(string _naviGraphName)
        {
            InitializeComponent();
            Console.WriteLine(">>AutoAdjustPopupPage : Constructor");
            naviGraphName = _naviGraphName;
            _navigationGraph = LoadNavigationGraphXml(naviGraphName);


            TmpRssiOption = RssiOption;
            RssiOption = 15;

            _detectThreadEvent = new ManualResetEventSlim(false);
            _askCorrectEvent = new ManualResetEventSlim(false);
            //_startToScanRssi = new ManualResetEventSlim(false);

            _ipsModules = new IPSModule(_navigationGraph);
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

        protected override bool OnBackButtonPressed()
        {
            return true;
        }
        #endregion


        #region scan position and rssi functions
        private void ThreadWork()
        {
            while (_isKeepDetection)
            {
                _ipsModules.InitialStep_DetectAllBeacon
                    (_navigationGraph.GetAllRegionIDs());
                _ipsModules.SetMonitorBeaconList(-1);
                //_ipsModules.StartAllExistClient();
                _detectThreadEvent.Wait();


                if (isEmptyGuid(_currentRegionID) || isEmptyGuid(_currentWaypointID))
                {
                    Console.WriteLine("WaypointID is empty.");
                    _detectThreadEvent.Reset();
                    continue;
                }


                Device.BeginInvokeOnMainThread(() => SetCheckPositionCorrect());
                _isKeepDetection = false;
                _askCorrectEvent.Wait();
            }

            string currentPosition =
                _nameInformation.GetWaypointName(_currentWaypointID);

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
            }
            else if (args is WaypointRssiEventArgs RssiArgs)
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
            double progressTmp = ProgressValue;

            Device.StartTimer(TimeSpan.FromMilliseconds(500), () =>
            {
                Console.WriteLine(">>StartTimer, count : " + _AllRssiList.Count);

                _ipsModules.OpenRssiScanning();
                ProgressValue = progressTmp + _AllRssiList.Count * 0.01;
                ProgressPercentLab.Text = (ProgressValue * 100).ToString() + "%";
                Console.WriteLine("ProgressValue : " + ProgressValue);
                Console.WriteLine("Count/33 : " + _AllRssiList.Count * 0.01);
                AutoAdjustProgressBar.ProgressTo(ProgressValue, 250, Easing.Linear);
                if (_AllRssiList.Count == 34)
                {
                    if (PopupNavigation.Instance.PopupStack.Contains(this) && !IsCancel)
                    {
                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            try
                            {
                                _ipsModules?.Dispose();
                                //show finish scan page                    
                                await PopupNavigation.Instance.RemovePageAsync(this);
                                AlertDialogPopupPage alertPage =
                                    new AlertDialogPopupPage
                                    (
                                        GetResourceString("IF_NOT_STABLEGO_TO_PREFER_STRING"),
                                    AppResources.OK_STRING);
                                bool isReturn = await alertPage.show();

                                _tcs?.SetResult(true);
                            }
                            catch (Exception exc)
                            {
                                Console.WriteLine("Auto adjustment page error : "
                                    + exc.Message);
                            }
                        });

                        SetRssiOption();
                    }
                    return false;
                }
                return isKeepDetectionRssi;
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
                //this is initial step, so the value put the "-1".
                _ipsModules.OpenBeaconScanning(-1); 
                Console.WriteLine("<<InvokeIPSWork");
            }
        }
        #endregion


        async private void CancelBtn_Clicked(object sender, EventArgs e)
        {
            ConfirmBtn.Clicked -= CancelBtn_Clicked;
            RssiOption = TmpRssiOption;
            IsCancel = true;
            isKeepDetectionRssi = false;
            _isKeepDetection = false;
            _ipsModules.CloseAllActiveClient();
            _ipsModules?.Dispose();
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
                Text = GetResourceString("DETECT_SIGNAL_NOW_STRING"),
                FontSize = 32,
                TextColor = Color.Black
            });

            AutoAdjustLayout.Children.Add(new Image
            {
                Source = "waittingscan.gif",
                IsAnimationPlaying = true
            });

            AutoAdjustProgressBar.ProgressTo(0.33, 250, Easing.Linear);
            ProgressPercentLab.Text = "33%";
        }

        #region to Check Position is correct or wrong with user.
        private void SetCheckPositionCorrect()
        {
            AutoAdjustLayout.Children.Clear();


            #region ConfirmButton
            ConfirmBtn.Clicked -= CancelBtn_Clicked;
            ConfirmBtn.Clicked += CheckPositionCorrectBtn_Clicked;
            ConfirmBtn.Text = GetResourceString("OK_STRING");
            #endregion          

            string _positionName = _nameInformation.GetWaypointName(_currentWaypointID);
            AutoAdjustLayout.Children.Add(
                new Label
                {
                    //Text = string.Format(AppResources.DETECT_POSITION_CLOSE_TO_STRING, _positionName,_positionName),
                    Text = string.Format(GetResourceString("DETECT_POSITION_CLOSE_TO_STRING"),
                    _positionName, _positionName),
                    //_navigationGraph.GetWaypointNameInRegion(_currentRegionID, _currentWaypointID),
                    FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
                    TextColor = Color.Black
                });
        }

        private void CheckPositionCorrectBtn_Clicked(object sender,
            EventArgs e)
        {
            Console.WriteLine(">>CheckPositionCorrect");
            _askCorrectEvent.Set();
            ConfirmBtn.Clicked -= CheckPositionCorrectBtn_Clicked;
            Console.WriteLine("<<CheckPositionCorrect");
        }

        #endregion

        #region  RssiScan View and Functions
        private void SetScanRssiView(string currentPosition)
        {
            #region Button control
            //ConfirmBtn.Clicked -= CheckPositionCorrectBtn_Clicked;
            ConfirmBtn.Clicked += CancelBtn_Clicked;
            ConfirmBtn.Text = GetResourceString("CANCEL_STRING");
            #endregion

            AutoAdjustLayout.Children.Clear();

            AutoAdjustLayout.Children.Add(new Label
            {
                //show current position.
                Text = string.Format(AppResources.CURRENT_LOCATION_STRING,
                currentPosition),
                FontSize = 32,
                TextColor = Color.Black
            });

            AutoAdjustLayout.Children.Add(new Label
            {
                Text = GetResourceString("SCAN_RSSI_NOW_STRING"),
                Margin = new Thickness(10, 0, 10, 0),
                FontSize = 24,
                TextColor = Color.Black
            });

            AutoAdjustLayout.Children.Add(new Image
            {
                Source = "waittingscan.gif",
                IsAnimationPlaying = true
            });

            AutoAdjustProgressBar.ProgressTo(0.66, 250, Easing.Linear);
            ProgressPercentLab.Text = "66%";
        }

        #endregion

        #region Another functions
        private void SetRssiOption()
        {
            try
            {
                _AllRssiList.Sort();

                int count = _AllRssiList.Count;
                int mid = count / 2;
                int midian = count % 2 != 0 ?
                    _AllRssiList[mid] :
                    (_AllRssiList[mid] + _AllRssiList[mid + 1]) / 2;

                int result = (_currentBeaconRssi - midian) + 2;
                //int result = -57 - midian;
                if (result >= 15)
                    RssiOption = 15;
                else if (result <= -15)
                    RssiOption = -15;
                else
                    RssiOption = result;
                Console.WriteLine("Rssi option result : " + RssiOption);
            }
            catch (Exception exc)
            {
                Console.WriteLine("set Rssi option error - " + exc.Message);

                RssiOption = TmpRssiOption;
            }
        }

        private bool isEmptyGuid(Guid guid)
        {
            return Guid.Empty.Equals(guid);
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


