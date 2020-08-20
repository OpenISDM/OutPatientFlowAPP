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
using static IndoorNavigation.Utilities.OPPA_TmperorayStatus;
using System.Threading;
using System.Net.Http.Headers;
using IndoorNavigation.Modules.IPSClients;

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
        //current waypoint threshold
        private int _threshold;
        private IPSModules _ipsModules;
        private string naviGraphName;
        private NavigationGraph _navigationGraph;

        private bool _isKeepDetection = true;
        private Thread _detectWaypointThread;
        private Thread _detectPositionControllThread;

        private ManualResetEventSlim _detectThreadEvent;
        
        #endregion
        public AutoAdjustPopupPage(string _naviGraphName)
        {
            InitializeComponent();

            naviGraphName = _naviGraphName;
            _navigationGraph = LoadNavigationGraphXml(naviGraphName);
            _detectThreadEvent = new ManualResetEventSlim(false);
            _ipsModules = new IPSModules(_navigationGraph);

            _currentRegionID = new Guid();
            _currentWaypointID = new Guid();

            _ipsModules._event._eventHandler +=
                new EventHandler(DetecWaypointResult);

            _detectWaypointThread = new Thread(() => InvokeIPSWork());
            _detectPositionControllThread = new Thread(() => ScanPosition());
        }

        private bool isEmptyGuid(Guid guid)
        {
            return Guid.Empty.Equals(guid);
        }

        //to detect where user is.
        private void ScanPosition()
        {
            _ipsModules.AtStarting_ReadALLIPSType
                (_navigationGraph.GetAllRegionIDs());

            _detectThreadEvent.Wait();

            //if we don't know where user is.
            if(isEmptyGuid(_currentRegionID) &&
                isEmptyGuid(_currentWaypointID))
            {
                _detectThreadEvent.Reset();
                ScanPosition();                
            }

            //Show result and remove delegate.
            _ipsModules._event._eventHandler -=
                new EventHandler(DetecWaypointResult);
        }
       

        private void DetecWaypointResult(object sender, EventArgs args)
        {
            if (args is WaypointSignalEventArgs signalArgs)
            {
                _currentRegionID =
                  signalArgs._detectedRegionWaypoint
                  ._regionID;

                _currentRegionID =
                    signalArgs._detectedRegionWaypoint
                    ._waypointID;

                _ipsModules.CloseStartAllExistClient();
            }else if(args is WaypointRssiEventArgs RssiArgs)
            {

            }
            _detectThreadEvent.Set();
        }

        //To scan current beacon rssi
        private void DetectPositionThreshold()
        {

        }

        private void Stop()
        {
            //if onSleep, need to handle it?
        }        
        
        
        private void InvokeIPSWork()
        {
            while(_isKeepDetection == true)
            {
                //keep detection.
                _ipsModules.OpenBeconScanning();
            }
        }
    }
}