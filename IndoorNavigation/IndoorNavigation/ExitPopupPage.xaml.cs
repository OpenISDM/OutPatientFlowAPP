using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Rg.Plugins.Popup.Pages;
using System.Resources;
using IndoorNavigation.Resources.Helpers;
using System.Reflection;
using System.Collections.ObjectModel;
using Rg.Plugins.Popup;
using Rg.Plugins.Popup.Services;
using Plugin.Multilingual;
using Plugin.InputKit.Shared.Controls;
namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ExitPopupPage :PopupPage
    {
        const string _resourceId = "IndoorNavigation.Resources.AppResources";
        ResourceManager _resourceManager =
            new ResourceManager(_resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);

        App app = (App)Application.Current;

        ExitPopupViewModel _viewmodel;

        public ExitPopupPage()
        {
            InitializeComponent();
            _viewmodel = new ExitPopupViewModel();
            BindingContext = _viewmodel;
            
        }
        private ObservableCollection<DestinationItem> LoadExitData()
        {
            ObservableCollection<DestinationItem> data = new ObservableCollection<DestinationItem>();

            data.Add(new DestinationItem
            {
                _waypointName = "前門出口",
                _waypointID=new Guid("00000000-0000-0000-0000-000000000002"),
                _regionID=new Guid("11111111-1111-1111-1111-111111111111"),
                Key = "exit"
            });

            data.Add(new DestinationItem
            {
                _waypointName = "停車場",
                _waypointID = new Guid("00000000-0000-0000-0000-000000000002"),
                _regionID = new Guid("11111111-1111-1111-1111-111111111111"),
                Key = "exit"
            });

            data.Add(new DestinationItem
            {
                _waypointName = "側門出口",
                _waypointID = new Guid("00000000-0000-0000-0000-000000000002"),
                _regionID = new Guid("11111111-1111-1111-1111-111111111111"),
                Key = "exit"
            });

            return data;
        }

         protected override bool OnBackButtonPressed()
        {
            var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
            DisplayAlert(_resourceManager.GetString("MESSAGE_STRING",currentLanguage),_resourceManager.GetString("SELET_EXIT_STRING", currentLanguage),_resourceManager.GetString("OK_STRING",currentLanguage));
            //return base.OnBackButtonPressed();
            return true;
        }

         protected override bool OnBackgroundClicked()
        {
            var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
            DisplayAlert(_resourceManager.GetString("MESSAGE_STRING", currentLanguage), _resourceManager.GetString("SELET_EXIT_STRING", currentLanguage), _resourceManager.GetString("OK_STRING", currentLanguage));
            //return base.OnBackgroundClicked();
            return false;
        }
    }
}