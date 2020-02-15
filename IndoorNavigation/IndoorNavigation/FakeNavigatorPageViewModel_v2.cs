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
using System.Threading.Tasks;
using System.Threading;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.Linq;
using static IndoorNavigation.Modules.Session;

namespace IndoorNavigation
{
    class FakeNavigatorPageViewModel_v2:BaseViewModel
    {
        #region define objects and variables

        private Guid _destinationID;
        //private NavigationModule _navigationModule;
        private const string _pictureType = "picture";
        private const int _originalInstructionLocation = 3;
        private const int _firstDirectionInstructionLocation = 4;
        private const int _firstDirectionInstructionScale = 2;
        private const int _originalInstructionScale = 4;
        private const int _millisecondsTimeoutForTwoSecond = 2000;
        private const int _millisecondsTimeoutForOneSecond = 1000;
        private const int _initialFaceDirection = 0;
        private const int _initialBackDirection = 1;

        private FirstDirectionInstruction _firstDirectionInstruction;
        private PhoneInformation _phoneInformation;
        private const string _resourceID = "IndoorNavigation.Resources.AppResources";
        private ResourceManager _resourceManager;
        private XMLInformation _xmlInformation;
        private NavigationGraph _navigationGraph;
        private CultureInfo currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
        private FirstDirectionInstruction _fistDirectionInstruction;

        private List<RegionWaypointPoint> _waypointOnRoute;
        private FakeSession _session;
        private Guid _destinationWaypointID;
        private Guid _destinationRegionID;
        private string _buildingName;

        private Random rand;
        #endregion
        public FakeNavigatorPageViewModel_v2(string graphName,string destination, Guid waypointID, Guid RegionID, XMLInformation informationXML)
        {
            #region graph initalize
            _resourceManager = new ResourceManager(_resourceID, typeof(TranslateExtension).GetTypeInfo().Assembly);
            _phoneInformation = new PhoneInformation();

            _fistDirectionInstruction = NavigraphStorage.LoadFirstDirectionXML(_phoneInformation.GiveCurrentMapName(graphName) + "_" + 
                _phoneInformation.GiveCurrentLanguage() + ".xml");
            _navigationGraph = NavigraphStorage.LoadNavigationGraphXML(_phoneInformation.GiveCurrentMapName(graphName));
            _xmlInformation = informationXML;
            #endregion

            _destinationRegionID = RegionID;
            _destinationWaypointID = waypointID;
            _buildingName = graphName;

            CurrentWaypointName = _resourceManager.GetString("NULL_STRING",currentLanguage);
            CurrentStepText = _resourceManager.GetString("NO_SIGNAL_STRING", currentLanguage);
            CurrentStepImage = "waittingscan.gif";
            ProgressBar = "0/0";

            rand = new Random(Guid.NewGuid().GetHashCode());

            ConstructSession();
              
        }

        //public void NavigatorProgram()
        //{
        //    for(int )

        //}

        private void SetInstruction(NavigationInstruction instruction,
                                    out string stepLabel,
                                    out string stepImage,
                                    out string firstDirectionImage,
                                    out int rotation,
                                    out int location,
                                    out int instructionValue)
        {
            var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
            string connectionTypeString = "";
            string nextWaypointName = instruction._nextWaypointName;
            nextWaypointName = _xmlInformation.GiveWaypointName(instruction._nextWaypointGuid);
            string nextRegionName = instruction._information._regionName;
            firstDirectionImage = null;
            rotation = 0;
            stepImage = "";
            instructionValue = _originalInstructionScale;
            location = _originalInstructionLocation;
            nextRegionName = _xmlInformation.GiveRegionName(instruction._nextRegionGuid);
            switch (instruction._information._turnDirection)
            {
                case TurnDirection.FirstDirection:
                    string firstDirection_Landmark = _firstDirectionInstruction.returnLandmark(instruction._currentWaypointGuid);
                    CardinalDirection firstDirection_Direction = _firstDirectionInstruction.returnDirection(instruction._currentWaypointGuid);
                    int faceDirection = (int)firstDirection_Direction;
                    int turnDirection = (int)instruction._information._relatedDirectionOfFirstDirection;
                    string initialDirectionString = "";
                    int directionFaceorBack = _firstDirectionInstruction.returnFaceOrBack(instruction._currentWaypointGuid);

                    if (faceDirection > turnDirection)
                    {
                        turnDirection = (turnDirection + 8) - faceDirection;
                    }
                    else
                    {
                        turnDirection = turnDirection - faceDirection;
                    }

                    if (directionFaceorBack == _initialFaceDirection)
                    {
                        initialDirectionString = _resourceManager.GetString(
                        "DIRECTION_INITIAIL_FACE_STRING",
                        currentLanguage);

                    }
                    else if (directionFaceorBack == _initialBackDirection)
                    {

                        initialDirectionString = _resourceManager.GetString(
                        "DIRECTION_INITIAIL_BACK_STRING",
                        currentLanguage);
                        if (turnDirection < 4)
                        {
                            turnDirection = turnDirection + 4;
                        }
                        else if (turnDirection >= 4)
                        {
                            turnDirection = turnDirection - 4;
                        }
                    }
                    string instructionDirection = "";
                    string stepImageString = "";

                    CardinalDirection cardinalDirection = (CardinalDirection)turnDirection;
                    switch (cardinalDirection)
                    {
                        case CardinalDirection.North:
                            instructionDirection = _resourceManager.GetString(
                            "GO_STRAIGHT_STRING",
                            currentLanguage);
                            stepImageString = "Arrow_up";
                            break;
                        case CardinalDirection.Northeast:
                            instructionDirection = _resourceManager.GetString(
                            "GO_RIGHT_FRONT_STRING",
                            currentLanguage);
                            stepImageString = "Arrow_frontright";
                            break;
                        case CardinalDirection.East:
                            instructionDirection = _resourceManager.GetString(
                            "TURN_RIGHT_STRING",
                            currentLanguage);
                            stepImageString = "Arrow_right";
                            break;
                        case CardinalDirection.Southeast:
                            instructionDirection = _resourceManager.GetString(
                            "TURN_RIGHT_REAR_STRING",
                            currentLanguage);
                            stepImageString = "Arrow_rearright";
                            break;
                        case CardinalDirection.South:
                            instructionDirection = _resourceManager.GetString(
                            "TURN_BACK_STRING",
                            currentLanguage);
                            stepImageString = "Arrow_down";
                            break;
                        case CardinalDirection.Southwest:
                            instructionDirection = _resourceManager.GetString(
                            "TURN_RIGHT_REAR_STRING",
                            currentLanguage);
                            stepImageString = "Arrow_rearleft";
                            break;
                        case CardinalDirection.West:
                            instructionDirection = _resourceManager.GetString(
                            "TURN_LEFT_STRING",
                            currentLanguage);
                            stepImageString = "Arrow_left";
                            break;
                        case CardinalDirection.Northwest:
                            instructionDirection = _resourceManager.GetString(
                            "TURN_LEFT_FRONT_STRING",
                            currentLanguage);
                            stepImageString = "Arrow_frontleft";
                            break;
                    }
                    if (instruction._previousRegionGuid != Guid.Empty && instruction._previousRegionGuid != instruction._currentRegionGuid)
                    {
                        stepLabel = string.Format(
                            _resourceManager.GetString(
                            "DIRECTION_INITIAIL_CROSS_REGION_STRING",
                            currentLanguage),
                            instructionDirection,
                            Environment.NewLine,
                            Environment.NewLine,
                            instruction._turnDirectionDistance);
                        stepImage = stepImageString;
                        break;
                    }
                    else if (firstDirection_Landmark == _pictureType)
                    {
                        string pictureName;

                        string regionString = instruction._currentRegionGuid.ToString();
                        string waypointString = instruction._currentWaypointGuid.ToString();

                        pictureName = _navigationGraph.GetBuildingName() + regionString.Substring(33, 3) + waypointString.Substring(31, 5);

                        stepLabel = string.Format(
                            initialDirectionString,
                            _resourceManager.GetString(
                            "PICTURE_DIRECTION_STRING",
                            currentLanguage),
                            Environment.NewLine,
                            instructionDirection,
                            Environment.NewLine,
                            instruction._turnDirectionDistance);
                        firstDirectionImage = pictureName;
                        stepImage = stepImageString;
                        rotation = 75;
                        location = _firstDirectionInstructionLocation;
                        instructionValue = _firstDirectionInstructionScale;
                        break;
                    }
                    else
                    {

                        stepLabel = string.Format(
                            initialDirectionString,
                            firstDirection_Landmark,
                            Environment.NewLine,
                            instructionDirection,
                            Environment.NewLine,
                            instruction._turnDirectionDistance);
                        stepImage = stepImageString;
                        break;
                    }

                case TurnDirection.Forward:
                    stepLabel = string.Format(
                        _resourceManager.GetString(
                            "DIRECTION_STRAIGHT_STRING", currentLanguage));
                    stepImage = "Arrow_up";

                    break;

                case TurnDirection.Forward_Right:
                    stepLabel = string.Format(
                        _resourceManager.GetString(
                            "DIRECTION_RIGHT_FRONT_STRING",
                            currentLanguage),
                            Environment.NewLine,
                            instruction._turnDirectionDistance);
                    stepImage = "Arrow_frontright";

                    break;

                case TurnDirection.Right:
                    stepLabel = string.Format(
                        _resourceManager.GetString(
                            "DIRECTION_RIGHT_STRING",
                            currentLanguage),
                            Environment.NewLine,
                            instruction._turnDirectionDistance);
                    stepImage = "Arrow_right";

                    break;

                case TurnDirection.Backward_Right:
                    stepLabel = string.Format(
                        _resourceManager.GetString(
                            "DIRECTION_RIGHT_REAR_STRING",
                            currentLanguage),
                            Environment.NewLine,
                            instruction._turnDirectionDistance);
                    stepImage = "Arrow_rearright";

                    break;

                case TurnDirection.Backward:
                    stepLabel = string.Format(
                        _resourceManager.GetString(
                            "DIRECTION_REAR_STRING",
                            currentLanguage),
                            Environment.NewLine,
                            instruction._turnDirectionDistance);
                    stepImage = "Arrow_down";

                    break;

                case TurnDirection.Backward_Left:
                    stepLabel = string.Format(
                        _resourceManager.GetString(
                            "DIRECTION_LEFT_REAR_STRING",
                            currentLanguage),
                            Environment.NewLine,
                            instruction._turnDirectionDistance);
                    stepImage = "Arrow_rearleft";

                    break;

                case TurnDirection.Left:
                    stepLabel = string.Format(
                        _resourceManager.GetString(
                            "DIRECTION_LEFT_STRING",
                            currentLanguage),
                            Environment.NewLine,
                            instruction._turnDirectionDistance);
                    stepImage = "Arrow_left";

                    break;

                case TurnDirection.Forward_Left:
                    stepLabel = string.Format(
                        _resourceManager.GetString(
                            "DIRECTION_LEFT_FRONT_STRING",
                            currentLanguage),
                            Environment.NewLine,
                            instruction._turnDirectionDistance);
                    stepImage = "Arrow_frontleft";

                    break;

                case TurnDirection.Up:
                    switch (instruction._information._connectionType)
                    {
                        case ConnectionType.Elevator:
                            connectionTypeString = _resourceManager.GetString("ELEVATOR_STRING", currentLanguage);
                            stepImage = "Elevator_up";
                            break;
                        case ConnectionType.Escalator:
                            connectionTypeString = _resourceManager.GetString("ESCALATOR_STRING", currentLanguage);
                            stepImage = "Stairs_up";
                            break;
                        case ConnectionType.Stair:
                            connectionTypeString = _resourceManager.GetString("STAIR_STRING", currentLanguage);
                            stepImage = "Stairs_up";
                            break;
                        case ConnectionType.NormalHallway:
                            connectionTypeString = _resourceManager.GetString("NORMALHALLWAY_STRING", currentLanguage);
                            stepImage = "Stairs_up";
                            break;
                    }
                    stepLabel = string.Format(
                        _resourceManager.GetString(
                            "DIRECTION_UP_STRING",
                            currentLanguage),
                            connectionTypeString,
                            Environment.NewLine,
                            nextRegionName);
                    break;

                case TurnDirection.Down:
                    switch (instruction._information._connectionType)
                    {
                        case ConnectionType.Elevator:
                            connectionTypeString = _resourceManager.GetString("ELEVATOR_STRING", currentLanguage);
                            stepImage = "Elevtor_down";
                            break;
                        case ConnectionType.Escalator:
                            connectionTypeString = _resourceManager.GetString("ESCALATOR_STRING", currentLanguage);
                            stepImage = "Stairs_down";
                            break;
                        case ConnectionType.Stair:
                            connectionTypeString = _resourceManager.GetString("STAIR_STRING", currentLanguage);
                            stepImage = "Stairs_down";
                            break;
                        case ConnectionType.NormalHallway:
                            connectionTypeString = _resourceManager.GetString("NORMALHALLWAY_STRING", currentLanguage);
                            stepImage = "Stairs_down";
                            break;
                    }

                    stepLabel = string.Format(
                        _resourceManager.GetString(
                            "DIRECTION_DOWN_STRING",
                            currentLanguage),
                            connectionTypeString,
                            Environment.NewLine,
                            nextRegionName);

                    break;
                default:
                    stepLabel = "You're get ERROR status";
                    stepImage = "Warning";
                    break;
            }
        }

        private void ConstructSession()
        {
            List<ConnectionType> avoidList = new List<ConnectionType>();

            if (Application.Current.Properties.ContainsKey("AvoidStair"))
            {
                avoidList.Add(
                       (bool)Application.Current.Properties["AvoidStair"] ?
                        ConnectionType.Stair : ConnectionType.NormalHallway);
                avoidList.Add(
                        (bool)Application.Current.Properties["AvoidElevator"] ?
                        ConnectionType.Elevator : ConnectionType.NormalHallway);
                avoidList.Add(
                        (bool)Application.Current.Properties["AvoidEscalator"] ?
                        ConnectionType.Escalator : ConnectionType.NormalHallway);

                avoidList = avoidList.Distinct().ToList();
                avoidList.Remove(ConnectionType.NormalHallway);
            }

            _session = new FakeSession(
                NavigraphStorage.LoadNavigationGraphXML(_phoneInformation.GiveCurrentMapName(_buildingName)),
                _destinationRegionID, _destinationWaypointID, avoidList.ToArray() );
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
