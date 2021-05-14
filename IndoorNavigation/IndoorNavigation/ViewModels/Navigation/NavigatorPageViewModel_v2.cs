using System;
using System.Collections.Generic;
using System.Text;
using static IndoorNavigation.Utilities.Storage;
using Xamarin.Forms;
using MvvmHelpers;
using IndoorNavigation.Modules;
using static IndoorNavigation.Modules.Session;
using NavigationEventArgs = IndoorNavigation.Modules.Session.NavigationEventArgs;

namespace IndoorNavigation.ViewModels.Navigation
{
    class NavigatorPageViewModel_v2 : BaseViewModel
    {
        #region variables and structures
        private Guid _destinationWaypointID;
        private Guid _destinationRegionID;

        #endregion

        public NavigatorPageViewModel_v2(Guid destinationRegionID, Guid destinationWaypointID)
        {

        }

        #region Other functions?

        #endregion

        #region UI update
        private void SetInstruction()
        {

        }

        private void GetNavigationResultEvent(object sender, EventArgs args)
        {
            Console.WriteLine("recieved event raised from NavigationModule.");

            switch ((args as NavigationEventArgs)._result)
            {
                case NavigationResult.Run:
                    break;
                case NavigationResult.ArrivaIgnorePoint:
                    break;
                case NavigationResult.ArriveVirtualPoint:
                    break;
                case NavigationResult.Arrival:
                    break;
                case NavigationResult.NoRoute:
                    break;

            }
        }
        #endregion

        #region life cycle
        public void Stop() { }
        public void onPause() { }
        public void onResume() { }
        #endregion

        #region Data Binding
        private string _currentStepLabel;
        public string CurrentStepLabel
        {
            get => _currentStepLabel;
            set => SetProperty(ref _currentStepLabel, value);
        }

        private string _destinationWaypointName;
        public string DestinationWaypointName
        {
            get => _destinationWaypointName;
            set => SetProperty(ref _destinationWaypointName, value);
        }

        private string _progressText;
        public string ProgressText
        {
            get => _progressText;
            set => SetProperty(ref _progressText, value);
        }
        private bool _isAnimated;
        public bool IsAnimated
        {
            get => _isAnimated;
            set => SetProperty(ref _isAnimated, value);
        }

        private double _progressRatio;
        public double ProgressRatio
        {
            get => _progressRatio;
            set => SetProperty(ref _progressRatio, value);
        }

        #region Page layout

        #endregion
        #endregion

        #region IDisposable Support
        private bool _disposedValue = false; // to prevent redundant call.
        protected virtual void Dipose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {

                }
                _disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
