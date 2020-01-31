using System;
using System.Collections.Generic;
using System.Text;
using MvvmHelpers;
using System.ComponentModel;
namespace IndoorNavigation
{
    class FakeNavigatorPageViewModel:BaseViewModel
    {
        public FakeNavigatorPageViewModel(string destinationName)
        {

        }
        #region Binding data
        private double _navigatorProgress;

        public double NavigatorProgress
        {
            get { return _navigatorProgress; }
            set
            {
                if (value != _navigatorProgress)
                {
                    _navigatorProgress = value;
                    SetProperty(ref _navigatorProgress, value);
                }
            }
        }
        #endregion
    }
}
