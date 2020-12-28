using System;
using System.Collections.Generic;
using System.Text;

using MvvmHelpers;
using IndoorNavigation.Models.NavigaionLayer;
namespace IndoorNavigation.ViewModels
{
    public class OPPAListPageViewModel : BaseViewModel
    {
        NavigationGraph _navigationGraph;
        string _naviGraphName;
        public OPPAListPageViewModel(string NaviGraphName, NavigationGraph NaviGraph)
        {

        }
        public bool HaveFirstVisitCenter()
        {
            return true;
        }

        #region First visit 

        #endregion

        #region Re- visit

        #endregion

        #region Custom options

        #endregion
    }
}
