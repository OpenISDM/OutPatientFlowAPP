/*
 * Copyright (c) 2019 Academia Sinica, Institude of Information Science
 *
 * License:
 *      GPL 3.0 : The content of this file is subject to the terms and
 *      conditions defined in file 'COPYING.txt', which is part of this source
 *      code package.
 *
 * Project Name:
 *
 *      IndoorNavigation
 *
 * File Description:
 *
 *      This view model implements properties and commands for NavigatorPage
 *      to build the data.
 *      It will create NavigationModule and subscribe to the needed event.
 *
 * Version:
 *
 *      1.0.0, 20190605
 *
 * File Name:
 *
 *      NavigatorPageViewModel.cs
 *
 * Abstract:
 *
 *      Waypoint-based navigator is a mobile Bluetooth navigation application
 *      that runs on smart phones. It is structed to support anywhere
 *      navigation indoors in areas covered by different indoor positioning
 *      system (IPS) and outdoors covered by GPS.In particilar, it can rely on
 *      BeDIS (Building/environment Data and Information System) for indoor
 *      positioning. This IPS provides a location beacon at every waypoint. The
 *      beacon brocasts its own coordinates; Consequesntly, the navigator does
 *      not need to continuously monitor its own position.
 *      This version makes use of Xamarin.Forms, which is a cross-platform UI
 *      tookit that runs on both iOS and Android.
 *
 * Authors:
 *
 *      Paul Chang, paulchang@iis.sinica.edu.tw
 *      Chun-Yu Lai, chunyu1202@gmail.com
 *
 */

using IndoorNavigation.Models;
using IndoorNavigation.Models.NavigaionLayer;
using IndoorNavigation.Modules;
using IndoorNavigation.Resources.Helpers;
using IndoorNavigation.Utilities;
using IndoorNavigation.Views.Navigation;
using MvvmHelpers;
using Plugin.Multilingual;
using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Timers;
using Xamarin.Forms;
using static IndoorNavigation.Modules.Session;
using static IndoorNavigation.Utilities.Storage;

namespace IndoorNavigation.ViewModels.Navigation
{
    public class NavigatorPageViewModel : BaseViewModel, IDisposable
    {
        #region Consts
        private const string _pictureType = "picture";
        private const string _specialType = "special";
        private const int _originalInstructionLocation = 2;
        private const int _firstDirectionInstructionLocation = 4;
        private const int _firstDirectionInstructionScale = 2;
        private const int _originalInstructionScale = 4;        
        private const int _initialFaceDirection = 0;
        private const int _initialBackDirection = 1;       
        #endregion

        #region Other Structures
        private bool _disposedValue = false; // To detect redundant calls
        public ResourceManager _resourceManager;
        private FirstDirectionInstruction _firstDirectionInstruction;
        private NavigationGraph _navigationGraph;
        private XMLInformation _xmlInformation;
        private Guid _destinationID;
        private NavigationModule _navigationModule;
        private CultureInfo currentLanguage =
            CrossMultilingual.Current.CurrentCultureInfo;
        #endregion

        #region Private Data Binding
        private bool _isplaying;
        private bool _isfinished = false;
        private string _currentStepLabelName = " ";
        private string _currentStepImageName;
        private string _firstDirectionPicture;
        private string _naviGraphName;
        private int _firstDirectionRotationValue;
        private int _firsrDirectionInstructionScaleVale;
        private int _instructionLocation;
        private string _currentWaypointName;
        private string _destinationWaypointName;
        private string _progressBar;
        private double _navigationProgress;
        private LayoutOptions _instructionLabVerticalOption;       
        private bool _stepImageIsVisible;

        private int _fdPictureHeightScaleValue;
        private int _fdPictureHeightSpanValue;
        private int _instructionWidthScaleValue;
        private int _instructionWidthSpanValue;
        #endregion

        public NavigatorPageViewModel(string navigationGraphName,
                                      Guid destinationRegionID,
                                      Guid destinationWaypointID,
                                      string destinationWaypointName,
                                      XMLInformation informationXML)

        {
            Console.WriteLine(">> NavigatorPageViewModel constructor");
            _firsrDirectionInstructionScaleVale = 1;
            _destinationID = destinationWaypointID;
            DestinationWaypointName = destinationWaypointName;
            CurrentStepImage = "waittingscan.gif";
            isPlaying = true;
            StepImgIsVisible = true;
            InstructionLabVerticalOption = LayoutOptions.CenterAndExpand;

            FDPictureHeightScaleValue = 2;
            FDPictureHeightSpanValue = 3;
            InstructionWidthScaleValue = 1;
            InstructionWidthSpanValue = 2;
            InstructionLocationValue = 1;

            _progressBar = "0/0";
            _instructionLocation = _originalInstructionLocation;

            _navigationModule = new NavigationModule(navigationGraphName,
                                                     destinationRegionID,
                                                     destinationWaypointID);
            _navigationModule._event._eventHandler += GetNavigationResultEvent;
            const string resourceId = 
                "IndoorNavigation.Resources.AppResources";

            _resourceManager =
                new ResourceManager(resourceId,
                                    typeof(TranslateExtension)
                                    .GetTypeInfo().Assembly);

            _naviGraphName = navigationGraphName;

            CurrentWaypointName =
                _resourceManager.GetString("NULL_STRING", currentLanguage);
            CurrentStepLabel =
                _resourceManager.GetString("NO_SIGNAL_STRING", currentLanguage);

            _firstDirectionInstruction = Storage.LoadFDXml(navigationGraphName);      

            _navigationGraph = Storage.LoadNavigationGraphXml(navigationGraphName);

            _xmlInformation = informationXML;
            Console.WriteLine("<< NavigatorPageViewModel constructor");
        }

        public void Stop()
        {
            _navigationModule.onStop();
        }

        public void onPause()
        {
            _navigationModule.onPause();
        }

        public void onResume()
        {
            _navigationModule.onResume();
        }

        /// <summary>
        /// According to each navigation status displays the text and image 
        /// instructions in UI.
        /// </summary>
        /// <param name="args">Arguments.</param>
        private void DisplayInstructions(EventArgs args)
        {

            Console.WriteLine(">> DisplayInstructions");
            NavigationInstruction instruction =
                (args as Session.NavigationEventArgs)._nextInstruction;
            string currentStepImage;
            string currentStepLabel;

            string firstDirectionPicture = null;
            int rotationValue = 0;
            int locationValue = _originalInstructionLocation;
            int instructionScale = _originalInstructionScale;
            //Vibration.Vibrate(500);
            switch ((args as Session.NavigationEventArgs)._result)
            {
                case NavigationResult.Run:
                    {                        
                        SetInstruction(instruction,
                                       out currentStepLabel,
                                       out currentStepImage,
                                       out firstDirectionPicture,
                                       out rotationValue,
                                       out locationValue,
                                       out instructionScale);
                        CurrentStepLabel = currentStepLabel;
                        CurrentStepImage = currentStepImage + ".png";
                        FirstDirectionPicture = firstDirectionPicture;                        
                        RotationValue = rotationValue;
                        InstructionScaleValue = instructionScale;
                        InstructionLocationValue = locationValue;
                        isPlaying = false;
                        CurrentWaypointName =
                            _xmlInformation.GiveWaypointName(instruction
                                                             ._currentWaypointGuid);
                        NavigationProgress = instruction._progress;
                        ProgressBar = instruction._progressBar;
                        Utility._textToSpeech.Speak(
                            CurrentStepLabel,
                            _resourceManager.GetString("CULTURE_VERSION_STRING",
                                                       currentLanguage));
                        break;
                    }
                case NavigationResult.ArrivaIgnorePoint:
                    {
                        CurrentWaypointName =
                            _xmlInformation.GiveWaypointName(instruction
                                                             ._currentWaypointGuid);
                        NavigationProgress = instruction._progress;
                        ProgressBar = instruction._progressBar;
                        FirstDirectionPicture = null;
                        isPlaying = false;
                        isFinished = true;
                        break;
                    }
                case NavigationResult.AdjustRoute:
                    {
                        Console.WriteLine("Get a wrong way.");                        
                        break;
                    }
                case NavigationResult.Arrival:
                    {
                        #region  For default Layout
                        FDPictureHeightScaleValue = 2;
                        FDPictureHeightSpanValue = 3;
                        InstructionWidthScaleValue = 1;
                        InstructionWidthSpanValue = 2;
                        InstructionLocationValue = 2;
                        #endregion

                        CurrentWaypointName =
                          _xmlInformation.GiveWaypointName(_destinationID);
                        CurrentStepLabel =
                            _resourceManager.GetString("DIRECTION_ARRIVED_STRING",
                                                       currentLanguage);
                        CurrentStepImage = "Arrived.png";
                        FirstDirectionPicture = null;
                        NavigationProgress = 100;
                        ProgressBar = instruction._progressBar;
                        isPlaying = false;
                        isFinished = true;

                        Utility._textToSpeech.Speak(
                            CurrentStepLabel,
                            _resourceManager.GetString("CULTURE_VERSION_STRING",
                                                       currentLanguage));
                        Stop();
                        break;
                    }
                case NavigationResult.NoRoute:
                    {
                        Console.WriteLine("No Route");
                        GoAdjustAvoidType();
                        Stop();
                        break;
                    }
                case NavigationResult.ArriveVirtualPoint:
                    {
                        SetInstruction(instruction,
                                       out currentStepLabel,
                                       out currentStepImage,
                                       out firstDirectionPicture,
                                       out rotationValue,
                                       out locationValue,
                                       out instructionScale);
                        CurrentStepLabel =
                            string.Format(_resourceManager
                            .GetString("DIRECTION_ARRIVED_VIRTUAL_STRING",
                                       currentLanguage),
                            currentStepLabel,
                            Environment.NewLine);
                        CurrentStepImage = "Arrived.png";
                        NavigationProgress = 100;
                        ProgressBar = instruction._progressBar;
                        CurrentWaypointName =
                            _xmlInformation.GiveWaypointName(instruction.
                                                             _currentWaypointGuid);
                        FirstDirectionPicture = firstDirectionPicture;
                        InstructionLocationValue = locationValue;
                        isPlaying = false;
                        RotationValue = rotationValue;
                        Utility._textToSpeech.Speak(
                            CurrentStepLabel,
                            _resourceManager.GetString("CULTURE_VERSION_STRING",
                                                       currentLanguage));
                        Stop();
                        break;
                    }
            }
        }

        //private int GetTurnDirection(int FaceOrBack, int )
        //{

        //}

        private void SetInstruction(NavigationInstruction instruction,
                                    out string stepLabel,
                                    out string stepImage,
                                    out string firstDirectionImage,
                                    out int rotation,
                                    out int location,
                                    out int instructionValue)
        {
            Console.WriteLine("PictureDirection = " + 
                instruction._information._directionPicture);
            string connectionTypeString = "";
            StepImgIsVisible = true;
            string nextWaypointName = instruction._nextWaypointName;
            nextWaypointName =
                _xmlInformation.GiveWaypointName(instruction._nextWaypointGuid);
            string nextRegionName = instruction._information._regionName;
            InstructionLabVerticalOption = LayoutOptions.CenterAndExpand;
            firstDirectionImage = null;
            rotation = 0;
            stepImage = "";
            instructionValue = _originalInstructionScale;
            location = _originalInstructionLocation;

            #region  For default Layout
            FDPictureHeightScaleValue = 2;
            FDPictureHeightSpanValue = 3;
            InstructionWidthScaleValue = 1;
            InstructionWidthSpanValue = 2;
            #endregion

            IImageChecker checker = DependencyService.Get<IImageChecker>();            

            if (checker.DoesImageExist
                (instruction._information._directionPicture))
            {
                Console.WriteLine(">> direction picture is not null");

                #region Direction Picture Layout Change
                location = 1;
                FDPictureHeightScaleValue = 1;
                FDPictureHeightSpanValue = 4;
                InstructionWidthScaleValue = 0;
                InstructionWidthSpanValue = 3;
                #endregion

                StepImgIsVisible = false;
                stepLabel =
                    _resourceManager
                    .GetString("PLEASE_FOLLOW_PICTURE_STRING",currentLanguage);

                stepImage = 
                    null;

                firstDirectionImage = 
                    instruction._information._directionPicture;

                return;
            }
            switch (instruction._information._turnDirection)
            {

                case TurnDirection.FirstDirection:
                    #region first direction part

                    string firstDirection_Landmark =
                        _firstDirectionInstruction
                        .returnLandmark(instruction._currentWaypointGuid);

                    int faceDirection = (int)_firstDirectionInstruction
                        .returnDirection(instruction._currentWaypointGuid);

                    int turnDirection =
                        (int)instruction._information
                             ._relatedDirectionOfFirstDirection;

                    string initialDirectionString = "";

                    int directionFaceorBack =
                        _firstDirectionInstruction
                        .returnFaceOrBack(instruction._currentWaypointGuid);

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

                    CardinalDirection cardinalDirection =
                        (CardinalDirection)turnDirection;
                    #region
                    switch (cardinalDirection)
                    {                        
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
                            "TURN_LEFT_REAR_STRING",
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

                        case CardinalDirection.North:
                            instructionDirection = _resourceManager.GetString(
                            "GO_STRAIGHT_STRING",
                            currentLanguage);
                            stepImageString = "Arrow_up";
                            break;
                    }
                    #endregion
                    if (instruction._previousRegionGuid != Guid.Empty &&
                        instruction._previousRegionGuid !=
                        instruction._currentRegionGuid)
                    {
                        stepLabel = string.Format(
                            _resourceManager.GetString(
                            "DIRECTION_INITIAIL_CROSS_REGION_STRING", currentLanguage), instructionDirection,
                            Environment.NewLine,
                            Environment.NewLine,
                            instruction._turnDirectionDistance);

                        stepImage = stepImageString;
                        break;
                    }
                    else if (firstDirection_Landmark == _pictureType)
                    {
                        string pictureName;

                        string regionString =
                            instruction._currentRegionGuid.ToString();
                        string waypointString =
                            instruction._currentWaypointGuid.ToString();

                        pictureName =
                            _navigationGraph.GetBuildingName() +
                            regionString.Substring(33, 3) +
                            waypointString.Substring(31, 5);

                        stepLabel = string.Format(
                            initialDirectionString,
                            _resourceManager.GetString("PICTURE_DIRECTION_STRING", currentLanguage),
                            Environment.NewLine,
                            instructionDirection,
                            Environment.NewLine,
                            instruction._turnDirectionDistance);

                        firstDirectionImage = pictureName;
                        stepImage = stepImageString;
                        rotation = 75;
                        location = _originalInstructionLocation;
                        instructionValue = _firstDirectionInstructionScale;
                        break;
                    }
                    else if (firstDirection_Landmark == _specialType)
                    {
                        Console.WriteLine(">> specialType");
                        string specialString =
                            _firstDirectionInstruction.GetSpecialString
                            (instruction._currentWaypointGuid,
                            instruction._nextWaypointGuid);                      
                        string tmpString = "";
                        stepLabel =
                            string.Format(_resourceManager
                            .GetString("SPECIAL_INSTRUCTION_STRING",
                                       currentLanguage),
                            specialString,
                            instructionDirection,
                            instruction._turnDirectionDistance);

                        for (int i = 0; i < stepLabel.Length; i++)
                        {
                            Console.WriteLine("StepLabel lenght : {0}, ch : {1}"
                                , i, stepLabel[i]);

                            tmpString += stepLabel[i];
                            if (stepLabel[i] == '，' ||
                                stepLabel[i] == ','  ||
                                stepLabel[i] == '.')
                            {
                                Console.WriteLine(">>stepLabel char equal ,");
                                tmpString += Environment.NewLine;
                            }
                        }
                        stepLabel = tmpString;                        
                        InstructionLabVerticalOption =
                            LayoutOptions.StartAndExpand;
                        location = 3;
                        stepImage = stepImageString;
                        break;
                    }
                    else
                    {

                        //stepLabel = string.Format(
                        //    initialDirectionString,
                        //    firstDirection_Landmark,
                        //    Environment.NewLine,
                        //    instructionDirection,
                        //    Environment.NewLine,
                        //    instruction._turnDirectionDistance);
                        if (directionFaceorBack == _initialFaceDirection)
                        {
                            if (cardinalDirection == CardinalDirection.North) 
                            {
                                stepLabel = 
                                    string.Format(GetResourceString
                                    ("DIRECTION_FACE_STRAIGHT_STRING"), 
                                    firstDirection_Landmark, 
                                    instruction._turnDirectionDistance);
                            }
                            else 
                            {
                                stepLabel =
                                    string.Format(GetResourceString
                                    ("DIRECTION_FACE_TURN_STRING"),
                                    firstDirection_Landmark,
                                    instructionDirection,
                                    instruction._turnDirectionDistance);
                            }
                        }
                        else  //(directionFaceorBack == _initialBackDirection) 
                        {
                            if(cardinalDirection == CardinalDirection.North)
                            {
                                stepLabel = 
                                    string.Format(GetResourceString
                                    ("DIRECTION_BACK_STRAIGHT_STRING"),
                                    firstDirection_Landmark,
                                    instruction._turnDirectionDistance);
                            }
                            else
                            {
                                stepLabel =
                                    string.Format(GetResourceString
                                    ("DIRECTION_BACL_TURN_STRING"),
                                    firstDirection_Landmark,
                                    instructionDirection,
                                    instruction._turnDirectionDistance);
                            }
                        }


                        stepImage = stepImageString;
                        break;
                    }
                #endregion
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
                            Environment.NewLine,
                            instruction._turnDirectionDistance);
                    stepImage = "Arrow_frontleft";

                    break;

                case TurnDirection.Up:
                    switch (instruction._information._connectionType)
                    {
                        case ConnectionType.Elevator:
                            connectionTypeString =
                                _resourceManager.GetString("ELEVATOR_STRING",
                                                           currentLanguage);
                            stepImage = "Elevator_up";
                            break;
                        case ConnectionType.Escalator:
                            connectionTypeString =
                                _resourceManager.GetString("ESCALATOR_STRING",
                                                           currentLanguage);
                            stepImage = "Stairs_up";
                            break;
                        case ConnectionType.Stair:
                            connectionTypeString =
                                _resourceManager.GetString("STAIR_STRING",
                                                           currentLanguage);

                            stepImage = "Stairs_up";
                            break;
                        case ConnectionType.NormalHallway:
                            connectionTypeString =
                                _resourceManager.GetString(
                                "NORMALHALLWAY_STRING", currentLanguage);

                            stepImage = "Stairs_up";
                            break;
                    }
                    stepLabel = SwitchCombineInstruction(instruction,
                        //instruction._information,
                                _resourceManager.GetString(
                                "DIRECTION_UP_STRING",
                                _currentCulture),
                                connectionTypeString,
                                nextRegionName);                    

                    break;

                case TurnDirection.Down:
                    switch (instruction._information._connectionType)
                    {
                        case ConnectionType.Elevator:
                            connectionTypeString =
                                _resourceManager.GetString("ELEVATOR_STRING",
                                                           currentLanguage);

                            stepImage = "Elevtor_down";
                            break;
                        case ConnectionType.Escalator:
                            connectionTypeString =
                                _resourceManager.GetString("ESCALATOR_STRING",
                                                           currentLanguage);
                            stepImage = "Stairs_down";
                            break;
                        case ConnectionType.Stair:
                            connectionTypeString =
                                _resourceManager.GetString("STAIR_STRING",
                                                           currentLanguage);
                            stepImage = "Stairs_down";
                            break;
                        case ConnectionType.NormalHallway:
                            connectionTypeString =
                                _resourceManager.GetString
                                ("NORMALHALLWAY_STRING", currentLanguage);

                            stepImage = "Stairs_down";
                            break;
                    }
                    stepLabel = SwitchCombineInstruction(instruction, 
                                _resourceManager.GetString(
                                "DIRECTION_DOWN_STRING",
                                _currentCulture), 
                                connectionTypeString, 
                                nextRegionName);
                        

                    break;
                default:
                    stepLabel = "You're get ERROR status";
                    stepImage = "Warning";
                    break;
            }
        }

        private string SwitchCombineInstruction
            (NavigationInstruction instruction,
            string DownOrUp, 
            string connectionType,
            string nextRegionName)
        {
            string instructionString = "";
            switch (instruction._information._nextDirection)
            {
                case TurnDirection.Null:
                {
                    return string.Format(
                              DownOrUp,
                              connectionType,
                              Environment.NewLine,
                              nextRegionName);
                }
                case TurnDirection.FirstDirection:
                    {
                        int _turnDirection = 
                            (int)instruction._information
                            ._relatedDirectionOfFirstDirection;

                        int _fD_direction =
                            (int)_firstDirectionInstruction.returnDirection
                            (instruction._nextWaypointGuid);

                        if (_fD_direction > _turnDirection)
                            _turnDirection =
                                (_turnDirection + 8) - _fD_direction;
                        else
                            _turnDirection = _turnDirection - _fD_direction;

                        int directionFaceOrBack = 
                            _firstDirectionInstruction.returnFaceOrBack
                            (instruction._nextWaypointGuid);

                        if(directionFaceOrBack == _initialBackDirection)
                        {
                            if (_turnDirection < 4)
                                _turnDirection += 4;
                            else
                                _turnDirection -= 4;
                        }
                        

                        CardinalDirection turnDirection =
                            (CardinalDirection)_turnDirection;
                            //instruction.._relatedDirectionOfFirstDirection;
                        Console.WriteLine("turn Direction = " + turnDirection.ToString());
                        string instructionDirection="";
                        switch(turnDirection){
                            case CardinalDirection.North:
                                {
                                    instructionDirection =
                                        _resourceManager.GetString(
                                        "GO_STRAIGHT_STRING",
                                        currentLanguage);
                                    break;
                                }
                            case CardinalDirection.Northeast:
                                {
                                    instructionDirection=
                                        _resourceManager.GetString(
                                        "GO_RIGHT_FRONT_STRING",
                                        currentLanguage);
                                    break;
                                }
                            case CardinalDirection.East:
                                {
                                    instructionDirection=
                                        _resourceManager.GetString(
                                        "TURN_RIGHT_STRING",
                                        currentLanguage);
                                    break;
                                }
                            case CardinalDirection.Southeast:
                                {
                                    instructionDirection=
                                        _resourceManager.GetString(
                                        "TURN_RIGHT_REAR_STRING",
                                        currentLanguage);
                                    break;
                                }
                            case CardinalDirection.South:
                                {
                                    instructionDirection=
                                        _resourceManager.GetString(
                                        "TURN_BACK_STRING",
                                        currentLanguage);
                                    break;
                                }
                            case CardinalDirection.Southwest:
                                {
                                    instructionDirection=
                                        _resourceManager.GetString(
                                        "TURN_LEFT_REAR_STRING",
                                        currentLanguage);

                                    break;
                                }
                            case CardinalDirection.West:
                                {
                                    instructionDirection=
                                        _resourceManager.GetString(
                                        "TURN_LEFT_STRING",
                                        currentLanguage);
                                    break;
                                }
                            case CardinalDirection.Northwest:
                                {
                                    instructionDirection=
                                        _resourceManager.GetString(
                                        "TURN_LEFT_FRONT_STRING",
                                        currentLanguage);
                                    break;
                                }
                            
                        }


                        if (instruction._information._turnDirection == TurnDirection.Up)
                        {
                            return string.Format(_resourceManager
                                .GetString
                                ("DIRECTION_UP_COMBINE_DIRECTION_STRING"
                                , currentLanguage),
                                 connectionType,
                                 Environment.NewLine,
                                 nextRegionName,
                                 Environment.NewLine,
                                 instructionDirection,
                                 Environment.NewLine,
                                 instruction._information._distance
                                );
                        }
                        else
                        {
                            return string.Format(_resourceManager
                                .GetString
                                ("DIRECTION_DOWN_COMBINE_DIRECTION_STRING",
                                currentLanguage),
                                connectionType,
                                Environment.NewLine,
                                nextRegionName,
                                Environment.NewLine,
                                instructionDirection,
                                Environment.NewLine,
                                instruction._information._distance);
                        }                        
                    }
                default:
                    return instructionString;
            }
        }

        /// <summary>
        /// Gets the navigation status event.
        /// </summary>
        private void GetNavigationResultEvent(object sender, EventArgs args)
        {
            Console.WriteLine("recevied event raised from NavigationModule");
            DisplayInstructions(args);
        }


        #region NavigatorPage Binding Args

        public void GoAdjustAvoidType()
        {
            var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
            Page tempMainPage = Application.Current.MainPage;

            Device.BeginInvokeOnMainThread(async () =>
            {
                for (int PageIndex = tempMainPage.Navigation.NavigationStack.Count - 1; PageIndex > 1; PageIndex--)
                {
                    tempMainPage.Navigation.RemovePage(tempMainPage.Navigation.NavigationStack[PageIndex]);
                }
                await tempMainPage.Navigation.PushAsync
                    (new NavigatorSettingPage(_naviGraphName), true);
                await tempMainPage.DisplayAlert(
                    _resourceManager.GetString("WARN_STRING", currentLanguage),
                    _resourceManager.GetString
                    ("PLEASE_ADJUST_AVOID_ROUTE_STRING", currentLanguage),
                    _resourceManager.GetString("OK_STRING", currentLanguage));
            });
        }

        public int FDPictureHeightSpanValue
        {
            get { return _fdPictureHeightSpanValue; }
            set { SetProperty(ref _fdPictureHeightSpanValue, value); }
        }
        public int FDPictureHeightScaleValue
        {
            get { return _fdPictureHeightScaleValue; }
            set { SetProperty(ref _fdPictureHeightScaleValue, value); }
        }
        public int InstructionWidthSpanValue
        {
            get { return _instructionWidthSpanValue; }
            set { SetProperty(ref _instructionWidthSpanValue, value); }
        }
        public int InstructionWidthScaleValue
        {
            get { return _instructionWidthScaleValue; }
            set { SetProperty(ref _instructionWidthScaleValue, value); }
        }

        public LayoutOptions InstructionLabVerticalOption
        {
            get { return _instructionLabVerticalOption; }
            set
            {
                SetProperty(ref _instructionLabVerticalOption, value);
            }
        }

        public bool isFinished
        {
            get { return _isfinished; }
            set { SetProperty(ref _isfinished, value); }
        }
        public string CurrentStepLabel
        {
            get
            {
                return _currentStepLabelName;
            }

            set
            {
                SetProperty(ref _currentStepLabelName, value);
            }
        }

        public string CurrentStepImage
        {
            get
            {
                return string.Format("{0}", _currentStepImageName);
            }
            set
            {
                if (_currentStepImageName != value)
                {
                    _currentStepImageName = value;
                    OnPropertyChanged("CurrentStepImage");
                }
            }
        }

        public int RotationValue
        {
            get
            {
                return _firstDirectionRotationValue;
            }
            set
            {
                SetProperty(ref _firstDirectionRotationValue, value);
            }
        }

        public int InstructionScaleValue
        {
            get
            {
                return _firsrDirectionInstructionScaleVale;
            }
            set
            {
                SetProperty(ref _firsrDirectionInstructionScaleVale, value);
            }
        }

        public int InstructionLocationValue
        {
            get
            {
                return _instructionLocation;
            }
            set
            {
                SetProperty(ref _instructionLocation, value);
            }
        }

        public string FirstDirectionPicture
        {
            get
            {
                if (!string.IsNullOrEmpty(_firstDirectionPicture) &&
                    _firstDirectionPicture.EndsWith(".jpg"))
                    return _firstDirectionPicture;
                else if (!string.IsNullOrEmpty(_firstDirectionPicture) &&
                    _firstDirectionPicture.EndsWith(".png"))

                    return _firstDirectionPicture;
                return string.Format("{0}.png", _firstDirectionPicture);                
            }

            set
            {
                if (_firstDirectionPicture != value)
                {
                    _firstDirectionPicture = value;
                    OnPropertyChanged("FirstDirectionPicture");
                }
            }
        }

        public string CurrentWaypointName
        {
            get
            {
                return _currentWaypointName;
            }

            set
            {
                SetProperty(ref _currentWaypointName, value);
            }
        }

        public string DestinationWaypointName
        {
            get
            {
                return _destinationWaypointName;
            }

            set
            {
                SetProperty(ref _destinationWaypointName, value);
            }
        }

        public string ProgressBar
        {
            get
            {
                return _progressBar;
            }
            set
            {
                SetProperty(ref _progressBar, value);
            }
        }

        public bool isPlaying
        {
            get { return _isplaying; }
            set { SetProperty(ref _isplaying, value); }
        }

        public double NavigationProgress
        {
            get
            {
                return _navigationProgress;
            }

            set
            {
                SetProperty(ref _navigationProgress, value);
            }
        }

        public bool StepImgIsVisible
        {
            get { return _stepImageIsVisible; }
            set { SetProperty(ref _stepImageIsVisible, value); }
        }
        #endregion

        #region IDisposable Support
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _navigationModule._event._eventHandler -=
                        GetNavigationResultEvent;
                    _navigationModule.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and 
                //		 override a finalizer below.
                // TODO: set large fields to null.

                _disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has 
        //code to free unmanaged resources.
        // ~NavigatorPageViewModel()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}
