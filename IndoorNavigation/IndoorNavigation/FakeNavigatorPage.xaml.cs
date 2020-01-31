using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Resources;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using IndoorNavigation.Resources.Helpers;
using System.Reflection;

namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FakeNavigatorPage : ContentPage
    {
        List<NavigateInstruction> _instructions;

        
        private const int _timeSpan=5;
        private const string _resourceid = "IndoorNavigation.Resources.AppResources";
        private ResourceManager _resourceManager = new ResourceManager(_resourceid, typeof(TranslateExtension).GetTypeInfo().Assembly);
        private Random rand = new Random(Guid.NewGuid().GetHashCode());

        string _currentLocationName;
        string _destinationWaypointName;
        string _firstDirectionImageName;
        FakeNavigatorPageViewModel _viewmodel;

        public FakeNavigatorPage(string destinationName)
        {
            InitializeComponent();
            _instructions = new List<NavigateInstruction>();
            _viewmodel = new FakeNavigatorPageViewModel(destinationName);
            BindingContext = _viewmodel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        private void LoadData()
        {

        }       

        class NavigateInstruction
        {
            string _StepImage;
            string _StepString;            
        }

        class NavigateStep
        {
            public string _currentLocation;
            public NavigateInstruction _instruction;            
        }
    }
}