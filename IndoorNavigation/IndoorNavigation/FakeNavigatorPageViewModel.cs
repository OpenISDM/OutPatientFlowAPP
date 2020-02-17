//using System;
//using System.Collections.Generic;
//using System.Reflection;
//using System.Text;
//using IndoorNavigation.Models.NavigaionLayer;
//using IndoorNavigation.Models;
//using MvvmHelpers;
//using System.Resources;
//using IndoorNavigation.Resources.Helpers;
//using System.Globalization;
//using Plugin.Multilingual;
//using IndoorNavigation.Modules.Utilities;
//using System.Threading.Tasks;
//using System.Threading;
//using Xamarin.Forms;
//using Xamarin.Essentials;
//using System.Linq;
//using static IndoorNavigation.Modules.Session;
//using Dijkstra.NET.Model;
//using Dijkstra.NET.Extensions;
//using Rg.Plugins.Popup.Services;
//using IndoorNavigation.Models.NavigaionLayer;
//namespace IndoorNavigation
//{
//    class FakeNavigatorPageViewModel : BaseViewModel
//    {
//        #region define objects and variables
//        private Graph<Guid, string> _graphRegionGraph = new Graph<Guid, string>();
//        private List<RegionWaypointPoint> _waypointsOnRoute;
//        private NavigationGraph _navigationGraph;
//        private ConnectionType[] _avoidConnectionTypes;

//        private const string _resourceID = "IndoorNavigation.Resources.AppResources";
//        private ResourceManager _resourceManager = new ResourceManager(_resourceID, typeof(TranslateExtension).GetTypeInfo().Assembly);

//        private App app = (App)Application.Current;

//        private Guid _destinationWaypointID;
//        private Guid _destinationRegionID;
//        private Guid _currentWaypointID;
//        private Guid _currentRegionID;
//        //private NavigationGraph navigationGraph;
//        private string _navigationGraphName;
//        private XMLInformation _informationXML;
//        private PhoneInformation _phoneInformation;
//        #endregion

//        public FakeNavigatorPageViewModel(string navigationGraphName, Guid destinationRegionID, Guid destinationWaypointID,
//            string destinationWaypointName, XMLInformation informationXml)
//        {

//            _destinationRegionID = destinationRegionID;
//            _destinationWaypointID = destinationWaypointID;
//            DestinationWaypointName = destinationWaypointName;

//            _navigationGraphName = navigationGraphName;
//            _informationXML = informationXml;
//            _phoneInformation = new PhoneInformation();

//            _navigationGraph = NavigraphStorage.LoadNavigationGraphXML(_phoneInformation.GiveCurrentMapName(_navigationGraphName));
//            if (app._tmpCurrentRegionID == Guid.Empty && app._tmpCurrentWaypointID == Guid.Empty)
//            {
//                app._tmpCurrentRegionID = new Guid("22222222-2222-2222-2222-222222222222");
//                app._tmpCurrentWaypointID = new Guid("00000000-0000-0000-0000-000000000016");
//            }
//            setAvoidConnection();

//            _currentRegionID = app._tmpCurrentRegionID;
//            _currentWaypointID = app._tmpCurrentWaypointID;
//            Console.WriteLine($"current waypoint info waypointID={_currentWaypointID}, RegionID={_currentRegionID}");

//            _graphRegionGraph = _navigationGraph.GenerateRegionGraph(_avoidConnectionTypes);
//            GenerateRoute(_currentRegionID, _currentWaypointID, _destinationRegionID, _destinationWaypointID);
//            int i = 0;
//            //foreach(RegionWaypointPoint point in _waypointsOnRoute)
//            //{
//            //    i++;
//            //    Console.WriteLine($"route[{i}] region id={point._regionID}, waypoint id ={point._waypointID}");
//            //}


//        }

//        private void GenerateRoute(Guid sourceRegionID,
//                                   Guid sourceWaypointID,
//                                   Guid destinationRegionID,
//                                   Guid destinationWaypointID)
//        {
//            // generate path between regions (from sourceRegionID to destnationRegionID)
//            Console.WriteLine("bbbbbbb");
//            uint region1Key = _graphRegionGraph
//                              .Where(node => node.Item.Equals(sourceRegionID))
//                              .Select(node => node.Key).First();
//            uint region2Key = _graphRegionGraph
//                              .Where(node => node.Item.Equals(destinationRegionID))
//                              .Select(node => node.Key).First();
//            var pathRegions = _graphRegionGraph.Dijkstra(region1Key, region2Key).GetPath();
//            if (0 == pathRegions.Count())
//            {
//                Console.WriteLine("No path. Need to change avoid connection type");
//                PopupNavigation.Instance.PushAsync(new AlertDialogPopupPage("No route"));
//                return;
//            }

//            // store the generate Dijkstra path across regions
//            List<Guid> regionsOnRoute = new List<Guid>();
//            for (int i = 0; i < pathRegions.Count(); i++)
//            {
//                regionsOnRoute.Add(_graphRegionGraph[pathRegions.ToList()[i]].Item);
//            }

//            // generate the path of the region/waypoint checkpoints across regions
//            _waypointsOnRoute = new List<RegionWaypointPoint>();
//            _waypointsOnRoute.Add(new RegionWaypointPoint
//            {
//                _regionID = sourceRegionID,
//                _waypointID = sourceWaypointID
//            });

//            for (int i = 0; i < _waypointsOnRoute.Count(); i++)
//            {
//                RegionWaypointPoint checkPoint = _waypointsOnRoute[i];
//                Console.WriteLine("check index = {0}, count = {1}, region {2} waypoint {3}",
//                                  i,
//                                  _waypointsOnRoute.Count(),
//                                  checkPoint._regionID,
//                                  checkPoint._waypointID);
//                if (regionsOnRoute.IndexOf(checkPoint._regionID) + 1 <
//                    regionsOnRoute.Count())
//                {
//                    LocationType waypointType =
//                        _navigationGraph.GetWaypointTypeInRegion(checkPoint._regionID,
//                                                                 checkPoint._waypointID);

//                    Guid nextRegionID =
//                        regionsOnRoute[regionsOnRoute.IndexOf(checkPoint._regionID) + 1];

//                    PortalWaypoints portalWaypoints =
//                        _navigationGraph.GetPortalWaypoints(checkPoint._regionID,
//                                                            checkPoint._waypointID,
//                                                            nextRegionID,
//                                                            _avoidConnectionTypes);

//                    if (LocationType.portal != waypointType)
//                    {
//                        _waypointsOnRoute.Add(new RegionWaypointPoint
//                        {
//                            _regionID = checkPoint._regionID,
//                            _waypointID = portalWaypoints._portalWaypoint1
//                        });
//                    }
//                    else if (LocationType.portal == waypointType)
//                    {
//                        if (!checkPoint._waypointID.Equals(portalWaypoints._portalWaypoint1))
//                        {
//                            _waypointsOnRoute.Add(new RegionWaypointPoint
//                            {
//                                _regionID = checkPoint._regionID,
//                                _waypointID = portalWaypoints._portalWaypoint1
//                            });
//                        }
//                        else
//                        {
//                            _waypointsOnRoute.Add(new RegionWaypointPoint
//                            {
//                                _regionID = nextRegionID,
//                                _waypointID = portalWaypoints._portalWaypoint2
//                            });
//                        }
//                    }
//                }
//            }
//            int indexLastCheckPoint = _waypointsOnRoute.Count() - 1;
//            if (!(_destinationRegionID.
//                Equals(_waypointsOnRoute[indexLastCheckPoint]._regionID) &&
//                _destinationWaypointID.
//                Equals(_waypointsOnRoute[indexLastCheckPoint]._waypointID)))
//            {
//                _waypointsOnRoute.Add(new RegionWaypointPoint
//                {
//                    _regionID = _destinationRegionID,
//                    _waypointID = _destinationWaypointID
//                });
//            }

//            foreach (RegionWaypointPoint checkPoint in _waypointsOnRoute)
//            {
//                Console.WriteLine("region-graph region/waypoint = {0}/{1}",
//                                  checkPoint._regionID,
//                                  checkPoint._waypointID);
//            }



//            // fill in all the path between waypoints in the same region / navigraph
//            for (int i = 0; i < _waypointsOnRoute.Count() - 1; i++)
//            {
//                RegionWaypointPoint currentCheckPoint = _waypointsOnRoute[i];
//                RegionWaypointPoint nextCheckPoint = _waypointsOnRoute[i + 1];

//                if (currentCheckPoint._regionID.Equals(nextCheckPoint._regionID))
//                {
//                    Graph<Guid, string> _graphNavigraph =
//                        _navigationGraph.GenerateNavigraph(currentCheckPoint._regionID,
//                                                           _avoidConnectionTypes);

//                    // generate path between two waypoints in the same region / navigraph
//                    uint waypoint1Key = _graphNavigraph
//                                        .Where(node => node.Item
//                                               .Equals(currentCheckPoint._waypointID))
//                                        .Select(node => node.Key).First();
//                    uint waypoint2Key = _graphNavigraph
//                                        .Where(node => node.Item
//                                               .Equals(nextCheckPoint._waypointID))
//                                        .Select(node => node.Key).First();

//                    var pathWaypoints =
//                        _graphNavigraph.Dijkstra(waypoint1Key, waypoint2Key).GetPath();

//                    for (int j = pathWaypoints.Count() - 1; j > 0; j--)
//                    {
//                        if (j != 0 && j != pathWaypoints.Count() - 1)
//                        {
//                            _waypointsOnRoute.Insert(i + 1, new RegionWaypointPoint
//                            {
//                                _regionID = currentCheckPoint._regionID,
//                                _waypointID = _graphNavigraph[pathWaypoints.ToList()[j]].Item
//                            });
//                        }
//                    }
//                }
//            }

//            // display the resulted full path of region/waypoint between source and destination
//            foreach (RegionWaypointPoint checkPoint in _waypointsOnRoute)
//            {
//                Console.WriteLine("full-path region/waypoint = {0}/{1}",
//                                  checkPoint._regionID,
//                                  checkPoint._waypointID);
//            }

//        }

//        private void setAvoidConnection()
//        {
//            List<ConnectionType> avoidList = new List<ConnectionType>();
//            Console.WriteLine("-- setup preference --- ");
//            if (Application.Current.Properties.ContainsKey("AvoidStair"))
//            {
//                avoidList.Add(
//                        (bool)Application.Current.Properties["AvoidStair"] ?
//                         ConnectionType.Stair : ConnectionType.NormalHallway);
//                avoidList.Add(
//                        (bool)Application.Current.Properties["AvoidElevator"] ?
//                        ConnectionType.Elevator : ConnectionType.NormalHallway);
//                avoidList.Add(
//                        (bool)Application.Current.Properties["AvoidEscalator"] ?
//                        ConnectionType.Escalator : ConnectionType.NormalHallway);

//                avoidList = avoidList.Distinct().ToList();
//                avoidList.Remove(ConnectionType.NormalHallway);
//            }
//            _avoidConnectionTypes = avoidList.ToArray();
//        }

//        private void DisplayInstructions(EventArgs args)
//        {

//            Console.WriteLine(">> DisplayInstructions");
//            // NavigationInstruction instruction = (args as Session.NavigationEventArgs)._nextInstruction;
//            var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
//            string currentStepImage;
//            string currentStepLabel;

//            string firstDirectionPicture = null;
//            int rotationValue = 0;
//            int locationValue = _originalInstructionLocation;
//            int instructionScale = _originalInstructionScale;
//            //Vibration.Vibrate(500);
//            switch ((args as Session.NavigationEventArgs)._result)
//            {
//                case NavigationResult.Run:
//                    SetInstruction(instruction, out currentStepLabel, out currentStepImage, out firstDirectionPicture, out rotationValue, out locationValue, out instructionScale);
//                    CurrentStepLabel = currentStepLabel;
//                    CurrentStepImage = currentStepImage + ".png";
//                    FirstDirectionPicture = firstDirectionPicture;
//                    InstructionLocationValue = locationValue;
//                    RotationValue = rotationValue;
//                    InstructionScaleValue = instructionScale;
//                    isPlaying = false;
//                    CurrentWaypointName = _xmlInformation.GiveWaypointName(instruction._currentWaypointGuid);
//                    NavigationProgress = instruction._progress;
//                    //FirstDirectionPicture = null;
//                    ProgressBar = instruction._progressBar;
//                    Utility._textToSpeech.Speak(
//                        CurrentStepLabel,
//                        _resourceManager.GetString("CULTURE_VERSION_STRING", currentLanguage));
//                    break;

//                case NavigationResult.ArrivaIgnorePoint:
//                    CurrentWaypointName = _xmlInformation.GiveWaypointName(instruction._currentWaypointGuid);
//                    NavigationProgress = instruction._progress;
//                    ProgressBar = instruction._progressBar;
//                    //FirstDirectionPicture = null;
//                    isPlaying = false;
//                    break;

//                case NavigationResult.AdjustRoute:
//                    Console.WriteLine("Wrong");
//                    CurrentStepLabel =
//                        _resourceManager.GetString("DIRECTION_WRONG_WAY_STRING", currentLanguage);
//                    CurrentStepImage = "Waiting.gif";
//                    isPlaying = true;
//                    Utility._textToSpeech.Speak(
//                        CurrentStepLabel,
//                        _resourceManager.GetString("CULTURE_VERSION_STRING", currentLanguage));
//                    break;

//                case NavigationResult.Arrival:
//                    CurrentWaypointName = _xmlInformation.GiveWaypointName(_destinationID);
//                    CurrentStepLabel =
//                        _resourceManager.GetString("DIRECTION_ARRIVED_STRING", currentLanguage);
//                    CurrentStepImage = "Arrived.png";
//                    //FirstDirectionPicture = null;
//                    NavigationProgress = 100;
//                    ProgressBar = instruction._progressBar;
//                    isPlaying = false;
//                    //_progressBar = _i
//                    Utility._textToSpeech.Speak(
//                        CurrentStepLabel,
//                        _resourceManager.GetString("CULTURE_VERSION_STRING", currentLanguage));


//                    string tmpBuildingName = _resourceManager.GetString("YUANLIN_CHRISTIAN_HOSPITAL_STRING", currentLanguage);
//                    //string tmpBuildingName = _resourceManager.GetString("LAB_STRING", currentLanguage);

//                    Console.WriteLine($"tmpBuildingName={tmpBuildingName} in NavigatorPageViewModel");

//                    if (_buildingName == tmpBuildingName && !string.IsNullOrEmpty(_floor) && (_floor != "2" && _floor != "1" && _floor != "-1"))
//                    {
//                        //do show msg
//                        Console.WriteLine($"enter the arrival popup pages in NavigatorPageViewModel.");
//                        string msg = string.Format(_resourceManager.GetString("ELEVATOR_TO_FLOOR_STRING", currentLanguage), _floor);
//                        PopupNavigation.Instance.PushAsync(new AlertDialogPopupPage(msg, _resourceManager.GetString("OK_STRING", currentLanguage)));
//                    }
//                    //PopupNavigation.Instance.PushAsync(new AlertDialogPopupPage("please go to 5 floor via elevator","OK"));
//                    Stop();
//                    break;

//                case NavigationResult.NoRoute:
//                    Console.WriteLine("No Route");
//                    GoAdjustAvoidType();
//                    Stop();
//                    break;
//                case NavigationResult.ArriveVirtualPoint:
//                    SetInstruction(instruction, out currentStepLabel, out currentStepImage, out firstDirectionPicture, out rotationValue, out locationValue, out instructionScale);
//                    //CurrentStepLabel = currentStepLabel;
//                    CurrentStepLabel = string.Format(_resourceManager.GetString("DIRECTION_ARRIVED_VIRTUAL_STRING", currentLanguage), currentStepLabel, Environment.NewLine);
//                    CurrentStepImage = "Arrived.png";
//                    NavigationProgress = 100;
//                    ProgressBar = instruction._progressBar;
//                    CurrentWaypointName = _xmlInformation.GiveWaypointName(instruction._currentWaypointGuid);
//                    FirstDirectionPicture = firstDirectionPicture;
//                    InstructionLocationValue = locationValue;
//                    isPlaying = false;
//                    RotationValue = rotationValue;
//                    Utility._textToSpeech.Speak(
//                        CurrentStepLabel,
//                        _resourceManager.GetString("CULTURE_VERSION_STRING", currentLanguage));
//                    Stop();
//                    break;

//            }
//        }

//        private void SetInstruction(NavigationInstruction instruction,
//                                    out string stepLabel,
//                                    out string stepImage,
//                                    out string firstDirectionImage,
//                                    out int rotation,
//                                    out int location,
//                                    out int instructionValue)
//        {
//            var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
//            string connectionTypeString = "";
//            string nextWaypointName = instruction._nextWaypointName;
//            nextWaypointName = _xmlInformation.GiveWaypointName(instruction._nextWaypointGuid);
//            string nextRegionName = instruction._information._regionName;
//            firstDirectionImage = null;
//            rotation = 0;
//            stepImage = "";
//            instructionValue = _originalInstructionScale;
//            location = _originalInstructionLocation;
//            nextRegionName = _xmlInformation.GiveRegionName(instruction._nextRegionGuid);
//            switch (instruction._information._turnDirection)
//            {

//                case TurnDirection.FirstDirection:
//                    #region first direction part
//                    string firstDirection_Landmark = _firstDirectionInstruction.returnLandmark(instruction._currentWaypointGuid);
//                    CardinalDirection firstDirection_Direction = _firstDirectionInstruction.returnDirection(instruction._currentWaypointGuid);
//                    int faceDirection = (int)firstDirection_Direction;
//                    int turnDirection = (int)instruction._information._relatedDirectionOfFirstDirection;
//                    string initialDirectionString = "";
//                    int directionFaceorBack = _firstDirectionInstruction.returnFaceOrBack(instruction._currentWaypointGuid);

//                    if (faceDirection > turnDirection)
//                    {
//                        turnDirection = (turnDirection + 8) - faceDirection;
//                    }
//                    else
//                    {
//                        turnDirection = turnDirection - faceDirection;
//                    }

//                    if (directionFaceorBack == _initialFaceDirection)
//                    {
//                        initialDirectionString = _resourceManager.GetString(
//                        "DIRECTION_INITIAIL_FACE_STRING",
//                        currentLanguage);

//                    }
//                    else if (directionFaceorBack == _initialBackDirection)
//                    {

//                        initialDirectionString = _resourceManager.GetString(
//                        "DIRECTION_INITIAIL_BACK_STRING",
//                        currentLanguage);
//                        if (turnDirection < 4)
//                        {
//                            turnDirection = turnDirection + 4;
//                        }
//                        else if (turnDirection >= 4)
//                        {
//                            turnDirection = turnDirection - 4;
//                        }
//                    }
//                    string instructionDirection = "";
//                    string stepImageString = "";

//                    CardinalDirection cardinalDirection = (CardinalDirection)turnDirection;
//                    switch (cardinalDirection)
//                    {
//                        case CardinalDirection.North:
//                            instructionDirection = _resourceManager.GetString(
//                            "GO_STRAIGHT_STRING",
//                            currentLanguage);
//                            stepImageString = "Arrow_up";
//                            break;
//                        case CardinalDirection.Northeast:
//                            instructionDirection = _resourceManager.GetString(
//                            "GO_RIGHT_FRONT_STRING",
//                            currentLanguage);
//                            stepImageString = "Arrow_frontright";
//                            break;
//                        case CardinalDirection.East:
//                            instructionDirection = _resourceManager.GetString(
//                            "TURN_RIGHT_STRING",
//                            currentLanguage);
//                            stepImageString = "Arrow_right";
//                            break;
//                        case CardinalDirection.Southeast:
//                            instructionDirection = _resourceManager.GetString(
//                            "TURN_RIGHT_REAR_STRING",
//                            currentLanguage);
//                            stepImageString = "Arrow_rearright";
//                            break;
//                        case CardinalDirection.South:
//                            instructionDirection = _resourceManager.GetString(
//                            "TURN_BACK_STRING",
//                            currentLanguage);
//                            stepImageString = "Arrow_down";
//                            break;
//                        case CardinalDirection.Southwest:
//                            instructionDirection = _resourceManager.GetString(
//                            "TURN_RIGHT_REAR_STRING",
//                            currentLanguage);
//                            stepImageString = "Arrow_rearleft";
//                            break;
//                        case CardinalDirection.West:
//                            instructionDirection = _resourceManager.GetString(
//                            "TURN_LEFT_STRING",
//                            currentLanguage);
//                            stepImageString = "Arrow_left";
//                            break;
//                        case CardinalDirection.Northwest:
//                            instructionDirection = _resourceManager.GetString(
//                            "TURN_LEFT_FRONT_STRING",
//                            currentLanguage);
//                            stepImageString = "Arrow_frontleft";
//                            break;
//                    }
//                    if (instruction._previousRegionGuid != Guid.Empty && instruction._previousRegionGuid != instruction._currentRegionGuid)
//                    {
//                        stepLabel = string.Format(
//                            _resourceManager.GetString(
//                            "DIRECTION_INITIAIL_CROSS_REGION_STRING",
//                            currentLanguage),
//                            instructionDirection,
//                            Environment.NewLine,
//                            Environment.NewLine,
//                            instruction._turnDirectionDistance);
//                        stepImage = stepImageString;
//                        break;
//                    }
//                    else if (firstDirection_Landmark == _pictureType)
//                    {
//                        string pictureName;

//                        string regionString = instruction._currentRegionGuid.ToString();
//                        string waypointString = instruction._currentWaypointGuid.ToString();

//                        pictureName = _navigationGraph.GetBuildingName() + regionString.Substring(33, 3) + waypointString.Substring(31, 5);

//                        stepLabel = string.Format(
//                            initialDirectionString,
//                            _resourceManager.GetString(
//                            "PICTURE_DIRECTION_STRING",
//                            currentLanguage),
//                            Environment.NewLine,
//                            instructionDirection,
//                            Environment.NewLine,
//                            instruction._turnDirectionDistance);
//                        firstDirectionImage = pictureName;
//                        stepImage = stepImageString;
//                        rotation = 75;
//                        location = _firstDirectionInstructionLocation;
//                        instructionValue = _firstDirectionInstructionScale;
//                        break;
//                    }
//                    else
//                    {

//                        stepLabel = string.Format(
//                            initialDirectionString,
//                            firstDirection_Landmark,
//                            Environment.NewLine,
//                            instructionDirection,
//                            Environment.NewLine,
//                            instruction._turnDirectionDistance);
//                        stepImage = stepImageString;
//                        break;
//                    }
//                #endregion
//                case TurnDirection.Forward:
//                    stepLabel = string.Format(
//                        _resourceManager.GetString(
//                            "DIRECTION_STRAIGHT_STRING", currentLanguage));
//                    stepImage = "Arrow_up";

//                    break;

//                case TurnDirection.Forward_Right:
//                    stepLabel = string.Format(
//                        _resourceManager.GetString(
//                            "DIRECTION_RIGHT_FRONT_STRING",
//                            currentLanguage),
//                            Environment.NewLine,
//                            instruction._turnDirectionDistance);
//                    stepImage = "Arrow_frontright";

//                    break;

//                case TurnDirection.Right:
//                    stepLabel = string.Format(
//                        _resourceManager.GetString(
//                            "DIRECTION_RIGHT_STRING",
//                            currentLanguage),
//                            Environment.NewLine,
//                            instruction._turnDirectionDistance);
//                    stepImage = "Arrow_right";

//                    break;

//                case TurnDirection.Backward_Right:
//                    stepLabel = string.Format(
//                        _resourceManager.GetString(
//                            "DIRECTION_RIGHT_REAR_STRING",
//                            currentLanguage),
//                            Environment.NewLine,
//                            instruction._turnDirectionDistance);
//                    stepImage = "Arrow_rearright";

//                    break;

//                case TurnDirection.Backward:
//                    stepLabel = string.Format(
//                        _resourceManager.GetString(
//                            "DIRECTION_REAR_STRING",
//                            currentLanguage),
//                            Environment.NewLine,
//                            instruction._turnDirectionDistance);
//                    stepImage = "Arrow_down";

//                    break;

//                case TurnDirection.Backward_Left:
//                    stepLabel = string.Format(
//                        _resourceManager.GetString(
//                            "DIRECTION_LEFT_REAR_STRING",
//                            currentLanguage),
//                            Environment.NewLine,
//                            instruction._turnDirectionDistance);
//                    stepImage = "Arrow_rearleft";

//                    break;

//                case TurnDirection.Left:
//                    stepLabel = string.Format(
//                        _resourceManager.GetString(
//                            "DIRECTION_LEFT_STRING",
//                            currentLanguage),
//                            Environment.NewLine,
//                            instruction._turnDirectionDistance);
//                    stepImage = "Arrow_left";

//                    break;

//                case TurnDirection.Forward_Left:
//                    stepLabel = string.Format(
//                        _resourceManager.GetString(
//                            "DIRECTION_LEFT_FRONT_STRING",
//                            currentLanguage),
//                            Environment.NewLine,
//                            instruction._turnDirectionDistance);
//                    stepImage = "Arrow_frontleft";

//                    break;

//                case TurnDirection.Up:
//                    switch (instruction._information._connectionType)
//                    {
//                        case ConnectionType.Elevator:
//                            connectionTypeString = _resourceManager.GetString("ELEVATOR_STRING", currentLanguage);
//                            stepImage = "Elevator_up";
//                            break;
//                        case ConnectionType.Escalator:
//                            connectionTypeString = _resourceManager.GetString("ESCALATOR_STRING", currentLanguage);
//                            stepImage = "Stairs_up";
//                            break;
//                        case ConnectionType.Stair:
//                            connectionTypeString = _resourceManager.GetString("STAIR_STRING", currentLanguage);
//                            stepImage = "Stairs_up";
//                            break;
//                        case ConnectionType.NormalHallway:
//                            connectionTypeString = _resourceManager.GetString("NORMALHALLWAY_STRING", currentLanguage);
//                            stepImage = "Stairs_up";
//                            break;
//                    }
//                    stepLabel = string.Format(
//                        _resourceManager.GetString(
//                            "DIRECTION_UP_STRING",
//                            currentLanguage),
//                            connectionTypeString,
//                            Environment.NewLine,
//                            nextRegionName);
//                    break;

//                case TurnDirection.Down:
//                    switch (instruction._information._connectionType)
//                    {
//                        case ConnectionType.Elevator:
//                            connectionTypeString = _resourceManager.GetString("ELEVATOR_STRING", currentLanguage);
//                            stepImage = "Elevtor_down";
//                            break;
//                        case ConnectionType.Escalator:
//                            connectionTypeString = _resourceManager.GetString("ESCALATOR_STRING", currentLanguage);
//                            stepImage = "Stairs_down";
//                            break;
//                        case ConnectionType.Stair:
//                            connectionTypeString = _resourceManager.GetString("STAIR_STRING", currentLanguage);
//                            stepImage = "Stairs_down";
//                            break;
//                        case ConnectionType.NormalHallway:
//                            connectionTypeString = _resourceManager.GetString("NORMALHALLWAY_STRING", currentLanguage);
//                            stepImage = "Stairs_down";
//                            break;
//                    }

//                    stepLabel = string.Format(
//                        _resourceManager.GetString(
//                            "DIRECTION_DOWN_STRING",
//                            currentLanguage),
//                            connectionTypeString,
//                            Environment.NewLine,
//                            nextRegionName);

//                    break;
//                default:
//                    stepLabel = "You're get ERROR status";
//                    stepImage = "Warning";
//                    break;
//            }
//        }

//        #region Data binding Args
//        private string _destinationWaypointName;
//        private string _currentWaypointName;
//        private string _currentStepText;
//        private string _currentStepImage;
//        private string _progressBar;
//        private double _navigationProgress;
//        private string _firstDirectionImage;
//        public string DestinationWaypointName
//        {
//            get { return _destinationWaypointName; }
//            set { SetProperty(ref _destinationWaypointName, value); }
//        }
//        public string CurrentWaypointName
//        {
//            get { return _currentWaypointName; }
//            set { SetProperty(ref _currentWaypointName, value); }
//        }
//        public string CurrentStepText
//        {
//            get { return _currentStepText; }
//            set { SetProperty(ref _currentStepText, value); }
//        }
//        public string CurrentStepImage
//        {
//            get { return _currentStepImage; }
//            set { SetProperty(ref _currentStepImage, value); }
//        }
//        public string ProgressBar
//        {
//            get { return _progressBar; }
//            set { SetProperty(ref _progressBar, value); }
//        }
//        public double NavigationProgress
//        {
//            get { return _navigationProgress; }
//            set { SetProperty(ref _navigationProgress, value); }
//        }
//        public string FirstDirectionImage
//        {
//            get { return _firstDirectionImage; }
//            set { SetProperty(ref _firstDirectionImage, value); }
//        }

//        #endregion
//    }
//}
