using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using IndoorNavigation.Models.NavigaionLayer;
using IndoorNavigation.Models;
using MvvmHelpers;
using System.Resources;
using IndoorNavigation.Resources.Helpers;
using System.Globalization;
using Plugin.Multilingual;
using IndoorNavigation.Modules.Utilities;
namespace IndoorNavigation
{
    class FakeNavigatorPageViewModel_v2:BaseViewModel
    {
        #region define objects and variables
        private PhoneInformation _phoneInformation;
        private const string _resourceID = "IndoorNavigation.Resources.AppResources";
        private ResourceManager _resourcemanager;
        private XMLInformation _xmlInformation;
        private NavigationGraph _navigationGraph;
        private CultureInfo currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
        private FirstDirectionInstruction _fistDirectionInstruction;

        private Guid _destinationWaypointID;
        private Guid _destinationRegionID;
        private string _buildingName;
        #endregion
        public FakeNavigatorPageViewModel_v2(string graphName,string destination, Guid waypointID, Guid RegionID, XMLInformation informationXML)
        {
            #region graph initalize
            _resourcemanager = new ResourceManager(_resourceID, typeof(TranslateExtension).GetTypeInfo().Assembly);
            _phoneInformation = new PhoneInformation();

            _fistDirectionInstruction = NavigraphStorage.LoadFirstDirectionXML(_phoneInformation.GiveCurrentMapName(graphName) + "_" + 
                _phoneInformation.GiveCurrentLanguage() + ".xml");
            _navigationGraph = NavigraphStorage.LoadNavigationGraphXML(_phoneInformation.GiveCurrentMapName(graphName));
            _xmlInformation = informationXML;
            #endregion

            _destinationRegionID = RegionID;
            _destinationWaypointID = waypointID;
            _buildingName = graphName;

            CurrentWaypointName = _resourcemanager.GetString("NULL_STRING",currentLanguage);
            CurrentStepText = _resourcemanager.GetString("NO_SIGNAL_STRING", currentLanguage);
            CurrentStepImage = "waittingscan.gif";
            ProgressBar = "0/0";
        }
        private void GeneratePath()
        {

        }
        #region Data binding Args
        private string _destinationWaypointName;
        private string _currentWaypointName;
        private string _currentStepText;
        private string _currentStepImage;
        private string _progressBar;
        private double _navigationProgress;
        private string _firstDirectionImage;
        public string DestinationWaypointName
        {
            get { return _destinationWaypointName; }
            set { SetProperty(ref _destinationWaypointName, value); }
        }
        public string CurrentWaypointName
        {
            get { return _currentWaypointName; }
            set { SetProperty(ref _currentWaypointName, value); }
        }
        public string CurrentStepText
        {
            get { return _currentStepText; }
            set { SetProperty(ref _currentStepText,value); }
        }
        public string CurrentStepImage
        {
            get { return _currentStepImage; }
            set { SetProperty(ref _currentStepImage, value); }
        }
        public string ProgressBar
        {
            get { return _progressBar; }
            set { SetProperty(ref _progressBar,value); }
        }        
        public double NavigationProgress
        {
            get { return _navigationProgress; }
            set { SetProperty(ref _navigationProgress, value); }
        }
        public string FirstDirectionImage
        {
            get { return _firstDirectionImage; }
            set { SetProperty(ref _firstDirectionImage, value); }
        }

        #endregion
    }
}
