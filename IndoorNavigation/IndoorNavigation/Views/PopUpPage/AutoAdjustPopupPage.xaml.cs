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

namespace IndoorNavigation.Views.PopUpPage
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AutoAdjustPopupPage : PopupPage
    {
        /*  1. enlarge the rssi option to detect the strongest signal beacon 
         *     to know where user is.
         *     BTW : if we can't to detect our beacon -> return exception
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

        #endregion
        public AutoAdjustPopupPage(string _naviGraphName)
        {
            InitializeComponent();

            naviGraphName = _naviGraphName;
            _navigationGraph = LoadNavigationGraphXml(naviGraphName);
            _ipsModules = new IPSModules(_navigationGraph);
            

        }

        //To know where user is.
        private void StartToScanPosition()
        {

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
            }
        }
    }
}