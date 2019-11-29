using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DisplayAlertPopupPage : PopupPage
    {
        public DisplayAlertPopupPage(string msg)
        {
            InitializeComponent();
            BackgroundColor = Color.FromRgba(150, 150, 150, 70);
            TempMessage.Text = msg;
        }

        public DisplayAlertPopupPage(string msg,bool needButton)
        {
            InitializeComponent();
            BackgroundColor = Color.FromRgba(150, 150, 150, 70);
            TempMessage.Text = msg;
            
            AlertFrame.WidthRequest = 280;
            AlertFrame.HeightRequest = 110;

            TempOKBtn.IsEnabled = needButton;
            TempOKBtn.IsVisible = needButton;
            
        }
        
        private void TempOKBtn_Clicked(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();
        }
    }
}