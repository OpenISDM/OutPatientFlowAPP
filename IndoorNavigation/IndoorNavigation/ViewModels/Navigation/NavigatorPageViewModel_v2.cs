using System;
using System.Collections.Generic;
using System.Text;
using static IndoorNavigation.Utilities.Storage;
using Xamarin.Forms;
using MvvmHelpers;
using IndoorNavigation.Modules;
using static IndoorNavigation.Modules.Session;
using IndoorNavigation.Models;
using NavigationEventArgs = IndoorNavigation.Models.NavigationEventArgs;
using IndoorNavigation.Views.Navigation;

namespace IndoorNavigation.ViewModels.Navigation
{
    class NavigatorPageViewModel_v2 : BaseViewModel
    {
        #region variables and structures
        private Guid _destinationWaypointID;
        private Guid _destinationRegionID;
        private string _naviGraphName;
        #endregion

        public NavigatorPageViewModel_v2(Guid destinationRegionID, Guid destinationWaypointID)
        {

        }

        #region Other functions
        public void AdjustAvoidType()
        {
            Page CurrentPage = Application.Current.MainPage;
            Device.BeginInvokeOnMainThread(async () =>
            {
                for (int PageIndex = CurrentPage.Navigation.NavigationStack.Count - 1; PageIndex > 1; PageIndex--)
                {
                    CurrentPage.Navigation.RemovePage(CurrentPage.Navigation.NavigationStack[PageIndex]);
                }
                await CurrentPage.Navigation.PushAsync
                    (new NavigatorSettingPage(_naviGraphName), true);
                await CurrentPage.DisplayAlert(
                    GetResourceString("WARN_STRING"),
                    GetResourceString("PLEASE_ADJUST_AVOID_ROUTE_STRING"),
                    GetResourceString("OK_STRING"));
            });
        }
        #endregion

        #region UI update

        #region Layout control
        private void SetDefaultLayout()
        {
            FDPictureHeightScaleValue = 2;
            FDPictureHeightSpanValue = 3;
            InstructionWidthScaleValue = 1;
            InstructionWidthSpanValue = 2;

        }
        private void SetPicturesLayout()
        {
            FDPictureHeightScaleValue = 1;
            FDPictureHeightSpanValue = 4;
            InstructionWidthScaleValue = 0;
            InstructionWidthSpanValue = 3;
        }
        #endregion

        private void SetInstruction()
        {

        }

        private void GetNavigationResultEvent(object sender, EventArgs args)
        {
            Console.WriteLine("recieved event raised from NavigationModule.");

            NavigationInstruction nextInstruction = (args as NavigationEventArgs)._nextInstruction;

            switch ((args as NavigationEventArgs)._result)
            {
                case NavigationResult.Run:
                    CurrentStepLabel = nextInstruction._currentInstruction;
                    ProgressRatio = nextInstruction._progress;
                    ProgressText = nextInstruction._progressBar;
                    break;
                case NavigationResult.ArrivaIgnorePoint:
                    break;
                case NavigationResult.ArriveVirtualPoint:
                    break;
                case NavigationResult.Arrival:
                    break;
                case NavigationResult.NoRoute:
                    break;
                default:
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
        private string _currentWaypointName;
        public string CurrentWaypointName
        {
            get => _currentWaypointName;
            set => SetProperty(ref _currentWaypointName, value);
        }

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

        #region Layout properties
        private int _fdPictureHeightSpanValue;
        public int FDPictureHeightSpanValue
        {
            get => _fdPictureHeightSpanValue;
            set => SetProperty(ref _fdPictureHeightSpanValue, value);
        }

        private int _fdPictureHeightScaleValue;
        public int FDPictureHeightScaleValue
        {
            get => _fdPictureHeightScaleValue;
            set => SetProperty(ref _fdPictureHeightScaleValue, value);
        }

        private int _instructionWidthSpanValue;
        public int InstructionWidthSpanValue
        {
            get => _instructionWidthSpanValue;
            set => SetProperty(ref _instructionWidthSpanValue, value);
        }

        private LayoutOptions _instructionLabVerticalOption;
        public LayoutOptions InstructionLabVerticalOption
        {
            get => _instructionLabVerticalOption;
            set => SetProperty(ref _instructionLabVerticalOption, value);
        }

        //TODO the attribute name need to be replace in .xaml
        private bool _isArrowImgVisible;
        public bool IsArrowImgVisible
        {
            get => _isArrowImgVisible;
            set => SetProperty(ref _isArrowImgVisible, value);
        }

        private int _instructionWidthScaleValue;
        public int InstructionWidthScaleValue
        {
            get => _instructionWidthScaleValue;
            set => SetProperty(ref _instructionWidthScaleValue, value);

        }
        #endregion

        #endregion

        #region IDisposable Support
        private bool _disposedValue = false; // to prevent redundant call.
        protected virtual void Dispose(bool disposing)
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
