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

//        ObservableCollection<DestinationItem> exits;


        ExitPopupViewModel _viewmodel;

        public ExitPopupPage()
        {
            _viewmodel = new ExitPopupViewModel();
            BindingContext = _viewmodel;
            InitializeComponent();
          //  exits = LoadExitData();
          //  AddRadioButton();
          //  ExitListView.ItemsSource = exits;
        }
        /*public void AddRadioButton()
        {
            RadioButtonGroupView groupView = new RadioButtonGroupView ();
            foreach (DestinationItem item in exits)
            {
                RadioButton radio = new RadioButton
                {
                    BindingContext = item,
                    Text = item.ToString(),
                    TextColor = Color.Black
                };
                groupView.Children.Add(radio);
            }
            //GroupView = groupView;
        }*/
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
            // var o = ExitListView.SelectedItem as DestinationItem;
          /*  object o = new DestinationItem();

            if (o != null)
            {
                await Navigation.PushAsync(new TestPage(o, 0));
                await PopupNavigation.Instance.PopAllAsync();
            }
            else
            {
                var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
                await DisplayAlert(_resourceManager.GetString("MESSAGE_STRING",currentLanguage),_resourceManager.GetString("PICK_EXIT_STRING", currentLanguage),_resourceManager.GetString("OK_STRING",currentLanguage));
            }*/
        }
    }
}