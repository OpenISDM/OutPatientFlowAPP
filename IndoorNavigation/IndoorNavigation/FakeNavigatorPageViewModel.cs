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
        //private List<>
        public FakeNavigatorPageViewModel(string destinationName)
        {
            _destinationName = destinationName;
            _insructions = new List<StepInstruction>();
            _firstDirections = new List<StepInstruction>();
            _waypointNames = new List<string>();
            _steps = new List<NavigateStep>();

            StepsCount = rand.Next(4, 7);

            LoadSteps();
            LoadFirstDirection();
        }

        //public enum Direction
        //{
        //    Straight=1,
        //    Right=2,
        //    Left=3,
        //    ElevatorUp=4,
        //    ElevatorDown=5,
        //    Escalator=6,

        //}
        class NavigateStep
        {
            public StepInstruction instruction;
            public string currentWaypointName;
            
            public NavigateStep(StepInstruction instruction,string waypointName)
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
        private void RandomGeneratePath()
        {
            //_steps.Add(new NavigateStep());
        }
            
        private void LoadFirstDirection()
        {
            _firstDirections.Add(new StepInstruction("Face the hall,",""));
        }
        private void LoadSteps()
        {
            _insructions.Add(new StepInstruction("Go straight","Arrow_up"));
            _insructions.Add(new StepInstruction("Turns right and go straight about{0}m", "Arrow_right"));
            _insructions.Add(new StepInstruction("Turns left", "Arrow_left"));
            _insructions.Add(new StepInstruction("Walking back", "Arrow_down"));
            _insructions.Add(new StepInstruction("Take the escalator up", "Escalator_up"));
            _insructions.Add(new StepInstruction("Take the elevator down", "Elevtor_down"));
            //string str = _resourceManager.GetString("",currentLanguage);

        }

        private void LoadWaypointNames()
        {
            _waypointNames.Add("");
            _waypointNames.Add("");
        }

        #region Binding data
        private double _navigatorProgress;
        private string _currentWayponintName;
        private string _currentStepImage;
        private string _currentStepText;
        private string _currentProgress;

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
