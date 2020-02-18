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
        private string _prefs;
        private Style ButtonStyle = new Style(typeof(Button))
        {
            Setters =
            {
                new Setter{ Property=Button.FontSizeProperty, Value=Device.GetNamedSize(NamedSize.Large, typeof(Button))},
                new Setter{ Property=Button.TextColorProperty, Value=Color.FromHex("#3f51b5") },
                new Setter{ Property=Button.HorizontalOptionsProperty, Value=LayoutOptions.End},
                new Setter{Property=Button.VerticalOptionsProperty, Value=LayoutOptions.EndAndExpand},
                new Setter{Property=Button.BackgroundColorProperty, Value=Color.Transparent}
            }
        };

        public ShiftAlertPopupPage()
        {
            InitializeComponent();
            BackgroundColor = Color.FromRgba(150, 150, 150, 70);
        }

        public ShiftAlertPopupPage(string AlertContext, string cancel,string prefs)
        {
            InitializeComponent();
            BackgroundColor = Color.FromRgba(150, 150, 150, 70);
            ShiftAlertLabel.Text = AlertContext;
            _prefs = prefs;
            Button CancelBtn = new Button{ Text = cancel, Style=ButtonStyle };

            CancelBtn.Clicked +=async (sender, args) => {
                isCheck(prefs);
                await PopupNavigation.Instance.PopAllAsync();
            };
            BtnLayout.Children.Add(CancelBtn);
        }

        public ShiftAlertPopupPage(string AlertContext,string confirm,string cancel,string prefs)
        {
            InitializeComponent();    
            BackgroundColor = Color.FromRgba(150, 150, 150, 70);
            ShiftAlertLabel.Text = AlertContext;
            _prefs = prefs;
            Button CancelBtn = new Button{ Text = cancel , Style=ButtonStyle };
            Button ConfirmBtn = new Button{Text = confirm, Style=ButtonStyle };

            ConfirmBtn.Clicked += async (sender, args) =>
             {
                 isCheck(prefs);
                 MessagingCenter.Send(this, prefs, true);
                 await PopupNavigation.Instance.PopAsync();
             };
            CancelBtn.Clicked += async (sender, args) =>
            {
                isCheck(prefs);
                MessagingCenter.Send(this, prefs, false) ;
                await PopupNavigation.Instance.PopAsync();
            };
            BtnLayout.Children.Add(ConfirmBtn);
            BtnLayout.Children.Add(CancelBtn);
        }
        private void isCheck(string prefs)
        {            
            if (CheckNeverShow.IsChecked)
                Preferences.Set(prefs, true);
        }

        //protected override bool OnBackButtonPressed()
        //{
        //    MessagingCenter.Send(this, _prefs, false);
        //    return base.OnBackButtonPressed();
        //}

        //private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        //{
        //    isCheck(_prefs);
        //    MessagingCenter.Send(this, "AlertBack", true);
        //    PopupNavigation.Instance.PopAsync();
        //}
    }
}