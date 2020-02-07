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
      
        private const string _resourceid = "IndoorNavigation.Resources.AppResources";
        private ResourceManager _resourceManager = new ResourceManager(_resourceid, typeof(TranslateExtension).GetTypeInfo().Assembly);

        private string _destinationName;
        private App app = (App)Application.Current;
        FakeNavigatorPageViewModel _viewmodel;

        public FakeNavigatorPage(string destinationName)
        {
            InitializeComponent();

            _destinationName = destinationName;
            _viewmodel = new FakeNavigatorPageViewModel(destinationName);
            BindingContext = _viewmodel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            app.LastWaypointName = _viewmodel.CurrentWaypointName;//_destinationName;
        }
    }
}