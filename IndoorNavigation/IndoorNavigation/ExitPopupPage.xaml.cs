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

namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ExitPopupPage :PopupPage
    {
        const string _resourceId = "IndoorNavigation.Resources.AppResources";
        ResourceManager _resourceManager =
            new ResourceManager(_resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);
        App app = (App)Application.Current;

        ObservableCollection<DestinationItem> exits;

        public ExitPopupPage()
        {
            InitializeComponent();
            exits = LoadExitData();
            ExitListView.ItemsSource = exits;
        }

        private ObservableCollection<DestinationItem> LoadExitData()
        {
            ObservableCollection<DestinationItem> data=new ObservableCollection<DestinationItem>();

            data.Add(new DestinationItem
            {
                _waypointName="前門出口",
                Key="exit"
            });

            data.Add(new DestinationItem
            {
                _waypointName="停車場",
                Key="exit"
            });

            data.Add(new DestinationItem
            {
                _waypointName="側門出口",
                Key="exit"
            });

            return data;
        }

        async private void ExitPopup_Clicked(object sender, EventArgs e)
        {
            var o = ExitListView.SelectedItem as DestinationItem;

            await Navigation.PushAsync(new TestPage(o, 0));
            await PopupNavigation.Instance.PopAllAsync();
        }
    }
}