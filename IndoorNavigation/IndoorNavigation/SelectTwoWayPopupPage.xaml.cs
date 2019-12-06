using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using IndoorNavigation.Views.Navigation;
namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SelectTwoWayPopupPage : PopupPage
    {
        String _locationName;
        bool isButtonPressed = false;
        public SelectTwoWayPopupPage(string BuildingName)
        {
            InitializeComponent();
            BackgroundColor = Color.FromRgba(150, 150, 150, 70);
            frame.BackgroundColor = Color.FromRgba(240, 240, 240, 200);
            _locationName = BuildingName;
        }

        async private void ToNavigationBtn_Clicked(object sender, EventArgs e)
        {
            if (isButtonPressed) return;
            isButtonPressed = true;
            //await Navigation.PushAsync(new RigisterList(_locationName));
            await Navigation.PushAsync(new NavigationHomePage(_locationName));
            await PopupNavigation.Instance.PopAllAsync();
        }

        async private void ToOPFM_Clicked(object sender, EventArgs e)
        {
            if (isButtonPressed) return;
            isButtonPressed = true;
            PopupNavigation.Instance.PopAsync();
            await Navigation.PushAsync(new RigisterList(_locationName));
            //await PopupNavigation.Instance.PopAllAsync();
            
        }
    }
}