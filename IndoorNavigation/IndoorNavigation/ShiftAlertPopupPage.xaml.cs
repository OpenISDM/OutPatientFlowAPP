using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace IndoorNavigation.Views.Navigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ShiftAlertPopupPage : PopupPage
    {
        public ShiftAlertPopupPage()
        {
            InitializeComponent();
            BackgroundColor = Color.FromRgba(150, 150, 150, 70);
        }

        async private void NeverShowButton_Clicked(object sender, EventArgs e)
        {
            if (CheckNeverShow.IsChecked)
                Preferences.Set("isCheckedNeverShow", true);

            await PopupNavigation.Instance.PopAllAsync();
        }
    }
}