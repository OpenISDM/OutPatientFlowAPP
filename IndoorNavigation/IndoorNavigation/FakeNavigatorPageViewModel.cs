using System;
using System.Collections.Generic;
using System.Text;
using MvvmHelpers;
using System.ComponentModel;
using System.Resources;
using IndoorNavigation.Resources.Helpers;
using System.Reflection;
using System.Globalization;
using Plugin.Multilingual;
using Xamarin.Forms;
using System.Threading.Tasks;
using System.Threading;

namespace IndoorNavigation
{
    class FakeNavigatorPageViewModel:BaseViewModel
    {
        private const int _timeSpan = 5;
        private const string _resourceid = "IndoorNavigation.Resources.AppResources";
        private ResourceManager _resourceManager = new ResourceManager(_resourceid, typeof(TranslateExtension).GetTypeInfo().Assembly);
        private Random rand = new Random(Guid.NewGuid().GetHashCode());
        private CultureInfo currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;

        private List<StepInstruction> _insructions;
        private List<string> _waypointNames;
        private List<StepInstruction> _firstDirections;
        private List<NavigateStep> _steps;
        private string _destinationName;
        private int StepsCount;
        private App app = (App)Application.Current;
        //private List<>
        public FakeNavigatorPageViewModel(string destinationName)
        {
            _destinationName = destinationName;
            _insructions = new List<StepInstruction>();
            _firstDirections = new List<StepInstruction>();
            _waypointNames = new List<string>();
            _steps = new List<NavigateStep>();
            DestinationWaypointName = destinationName;
            if (string.IsNullOrEmpty(app.LastWaypointName))
            {
                app.LastWaypointName = "前門大廳";
                //CurrentWaypointName = "前門大廳";
            }
            CurrentWaypointName = "";
            CurrentStepImage = "Waiting";
            CurrentStepText = _resourceManager.GetString("NO_SIGNAL_STRING", currentLanguage);
            StepsCount = rand.Next(4, 12);
            Console.WriteLine("Start to load files");
            LoadWaypointNames();
            LoadSteps();
            LoadFirstDirection();
            

            RandomGeneratePath();

            Thread thread = new Thread(() => NavigatorProgram());
            thread.Start();
        }

        private void NavigatorProgram()
        {
            //Thread.Sleep(rand.Next(2000, 5000));
            //CurrentWaypointName = app.LastWaypointName;
            


            Console.WriteLine("Start to navigator");
            for (int i = 0; i < _steps.Count; i++)
            {
                Thread.Sleep(rand.Next(2000, 5000));
                NavigatorProgress = (double)Math.Round(100 * ((decimal)i /
                                           (_steps.Count - 1)), 3);
                CurrentProgress = $"{i}/{_steps.Count-1}";
                CurrentStepImage = _steps[i].instruction.StepImageName;
                CurrentStepText = _steps[i].instruction.StepText;
                CurrentWaypointName = _steps[i].currentWaypointName;
                app.LastWaypointName = _steps[i].currentWaypointName;
                Console.WriteLine(CurrentStepText);
                //Task.Delay(rand.Next(2000, 5000));
                
            }
        }
        //private void NavigatorProgram()
        //{
        //    for(int i = 0; i < _steps.Count; i++)
        //    {
        //        Console.WriteLine($"Current step:{_steps[i].currentWaypointName}");
        //        Console.WriteLine($"Current Img:{_steps[i].instruction.StepImageName}");
        //        Console.WriteLine($"Current Instruct text:{_steps[i].instruction.StepText}");
        //    }
        //}

        private void RandomGeneratePath()
        {
            //first direction->normal hall way->arrived destination.

            //_steps.Add(new NavigateStep())
            //to add first direction
            _steps.Add(new NavigateStep(_firstDirections[rand.Next(0,3)],app.LastWaypointName));
            //to random generate direction path
            for(int i = 0; i < StepsCount - 2; i++)
            {
                _steps.Add(new NavigateStep(_insructions[rand.Next(0, _insructions.Count - 1)], _waypointNames[rand.Next(0, _waypointNames.Count - 1)]));
            }
            //to to add arrival destination process
            _steps.Add(new NavigateStep(new StepInstruction(_resourceManager.GetString("DIRECTION_ARRIVED_STRING", currentLanguage), "Arrived"),_destinationName));
        }

        #region Classes
        class NavigateStep
        {
            public StepInstruction instruction;
            public string currentWaypointName;

            public NavigateStep(StepInstruction instruction, string waypointName)
            {
                this.instruction = instruction;
                currentWaypointName = waypointName;
            }
        }

        class StepInstruction
        {
            public string StepText;
            public string StepImageName;

            public StepInstruction(string stepText, string stepImageName)
            {
                StepText = stepText;
                StepImageName = stepImageName;
            }
        }
        #endregion
        #region Load instruction, name and first-direction data 
        private void LoadFirstDirection()
        {
            Console.WriteLine("Star to load firstDirection");
            _firstDirections.Add(DirectionFormat_FirstDirection("DIRECTION_INITIAIL_BACK_STRING", 1));
            _firstDirections.Add(DirectionFormat_FirstDirection("DIRECTION_INITIAIL_FACE_STRING", 1));
            _firstDirections.Add(DirectionFormat_FirstDirection("DIRECTION_INITIAIL_BACK_STRING", 2));
            _firstDirections.Add(DirectionFormat_FirstDirection("DIRECTION_INITIAIL_FACE_STRING", 2));
            Console.WriteLine("Finish load Firs direction");
        }
        private StepInstruction DirectionFormat_FirstDirection(string sourceKey,int direction)
        {
            string landmark = app.LastWaypointName;
            string sourceValue = _resourceManager.GetString(sourceKey, currentLanguage);            
            string instructionImg="";
            string format="";
            switch(direction)
            {
                case 1:
                    instructionImg = "Arrow_right";
                    format = string.Format(sourceValue, landmark, Environment.NewLine, _resourceManager.GetString("TURN_RIGHT_STRING", currentLanguage), Environment.NewLine,rand.Next(3,20));
                    Console.WriteLine(format + "in 1");
                    break;               
                case 2:
                    instructionImg = "Arrow_left";
                    format = string.Format(sourceValue, landmark, Environment.NewLine, _resourceManager.GetString("TURN_LEFT_STRING", currentLanguage), Environment.NewLine, rand.Next(3, 20));
                    Console.WriteLine(format + "in 2");
                    break;
                default:
                    //instructionImg = "Arrow_left";
                    Console.WriteLine("Default in random first direction view model");
                    break;
            }
            return new StepInstruction(format,instructionImg);
        }

        private string DirectionFormat(string sourceKey)
        {
            string sourceValue = _resourceManager.GetString(sourceKey, currentLanguage);
            Console.WriteLine(string.Format(sourceValue, Environment.NewLine, rand.Next(3, 30)));
            return string.Format(sourceValue, Environment.NewLine, rand.Next(3, 30));
        }
        private string DirectionFormat_ChangeRegion(string sourceKey)
        {
            string sourceValue = _resourceManager.GetString(sourceKey, currentLanguage);
            Console.WriteLine(string.Format(sourceValue, _resourceManager.GetString("ELEVATOR_STIRNG", currentLanguage),
                rand.Next(1,3),_resourceManager.GetString("FLOOR_STRING",currentLanguage)));
            
            return string.Format(sourceValue,_resourceManager.GetString("ELEVATOR_STRING",currentLanguage),
                rand.Next(1,3),_resourceManager.GetString("FLOOR_STRING",currentLanguage));
        }
        
        private void LoadSteps()
        {            
            _insructions.Add(new StepInstruction(DirectionFormat("DIRECTION_LEFT_FRONT_STRING"), "Arrow_frontleft"));
            _insructions.Add(new StepInstruction(DirectionFormat("DIRECTION_LEFT_REAR_STRING"), "Arrow_rearleft"));
            _insructions.Add(new StepInstruction(DirectionFormat("DIRECTION_LEFT_STRING"), "Arrow_left"));
            _insructions.Add(new StepInstruction(DirectionFormat("DIRECTION_REAR_STRING"), "Arrow_down"));
            _insructions.Add(new StepInstruction(DirectionFormat("DIRECTION_RIGHT_FRONT_STRING"), "Arrow_frontright"));
            _insructions.Add(new StepInstruction(DirectionFormat("DIRECTION_RIGHT_REAR_STRING"),"Arrow_rearright"));
            _insructions.Add(new StepInstruction(DirectionFormat("DIRECTION_RIGHT_STRING"), "Arrow_right"));
            _insructions.Add(new StepInstruction(_resourceManager.GetString("DIRECTION_STRAIGHT_STRING", currentLanguage),"Arrow_up"));
            _insructions.Add(new StepInstruction(DirectionFormat_ChangeRegion("DIRECTION_UP_STRING"), "Elevator_up"));
            _insructions.Add(new StepInstruction(DirectionFormat_ChangeRegion("DIRECTION_DOWN_STRING"), "Elevtor_down"));
            //_insructions.Add(new StepInstruction(_resourceManager.GetString("DIRECTION_ARRIVED_STRING", currentLanguage), "Arrived_down"));

            Console.WriteLine("Finish load base steps");
        }

        private void LoadWaypointNames()
        {
            _waypointNames.Add("外科");
            _waypointNames.Add("內科");
            _waypointNames.Add("手扶梯");
            _waypointNames.Add("牙科");
            _waypointNames.Add("批價掛號櫃檯");
            _waypointNames.Add("放射區");
            _waypointNames.Add("衛教中心");
            _waypointNames.Add("眼科");
            _waypointNames.Add("耳鼻喉科");
            _waypointNames.Add("採血區");

            Console.WriteLine("Finish load waypoint names");
        }
        #endregion
        #region Binding data
        private double _navigatorProgress;
        private string _currentWayponintName;
        private string _currentStepImage;
        private string _currentStepText;
        private string _currentProgress;
        private string _destinationWaypointName;

        public string DestinationWaypointName
        {
            get { return _destinationWaypointName; }
            set { SetProperty(ref _destinationWaypointName, value); }
        }

        public string CurrentProgress
        {
            get { return _currentProgress; }
            set { SetProperty(ref _currentProgress,value); }
        }

        public double NavigatorProgress
        {
            get { return _navigatorProgress; }
            set { SetProperty(ref _navigatorProgress, value); }
        }
       
        public string CurrentWaypointName
        {
            get { return _currentWayponintName; }
            set { SetProperty(ref _currentWayponintName, value); }
        }

        public string CurrentStepImage
        {
            get { return _currentStepImage; }
            set { SetProperty(ref _currentStepImage, value); }
        }

        public string CurrentStepText
        {
            get { return _currentStepText; }
            set { SetProperty(ref _currentStepText, value); }
        }
        #endregion
    }
}
