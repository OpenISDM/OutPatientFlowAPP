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
using IndoorNavigation.Models.NavigaionLayer;

namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FakeNavigatorPage : ContentPage
    {
      
        private const string _resourceid = "IndoorNavigation.Resources.AppResources";
        private ResourceManager _resourceManager = new ResourceManager(_resourceid, typeof(TranslateExtension).GetTypeInfo().Assembly);

        private string _destinationName;
        private App app = (App)Application.Current;
        FakeNavigatorPageViewModel _viewmodel;

        public FakeNavigatorPage(string navigationGraphName, Guid destinationRegionID, Guid destinationWaypointID, string destinationWaypointName, XMLInformation informationXML)
        {
            InitializeComponent();

            _destinationName = destinationWaypointName;
            _viewmodel = new FakeNavigatorPageViewModel(navigationGraphName, destinationRegionID, destinationWaypointID, destinationWaypointName, informationXML);
            BindingContext = _viewmodel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
           // app.LastWaypointName = _viewmodel.CurrentWaypointName;//_destinationName;
        }
    }
}