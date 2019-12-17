using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rg.Plugins.Popup.Pages;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Rg.Plugins.Popup.Services;
namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AlertDialogPopupPage : PopupPage
    {
        public AlertDialogPopupPage() { InitializeComponent(); }
        public AlertDialogPopupPage(string cotext)
        {
            InitializeComponent();
        }

        public AlertDialogPopupPage(string context,string confirm,string cancel)
        {
            InitializeComponent();
            TempMessage.Text = context;
            Button ConfirmBtn = new Button {
                Text = confirm, FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Button)),
                TextColor = Color.FromHex("#3f51b5"),
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.End,
                BackgroundColor = Color.Transparent
            };
            Button CancelBtn = new Button {
                Text = cancel,
                FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Button)),
                TextColor = Color.FromHex("#3f51b5"),
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.End,
                BackgroundColor = Color.Transparent
            };
            CancelBtn.Clicked += CanelPageClicked;
            ConfirmBtn.Clicked += ConfirmPageClicked;
            buttonLayout.Children.Add(ConfirmBtn);
            buttonLayout.Children.Add(CancelBtn);
        }
        public AlertDialogPopupPage(string context,string cancel)
        {
            InitializeComponent();
            TempMessage.Text = context;
            Button CancelBtn = new Button {
                Text=cancel,
                FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Button)),
                TextColor = Color.FromHex("#3f51b5"),
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.EndAndExpand,
                BackgroundColor = Color.Transparent
            };
            CancelBtn.Clicked += OnlyCancelPageClicked;             
            buttonLayout.Children.Add(CancelBtn);
        }

        private void OnlyCancelPageClicked(Object sender,EventArgs args)
        {
            PopupNavigation.Instance.PopAsync();
        }

        private void CanelPageClicked(Object sender,EventArgs args)
        {
            MessagingCenter.Send(this,"PopupPageMsg",false);
            PopupNavigation.Instance.PopAsync();
        }
        private void ConfirmPageClicked(Object sender,EventArgs args)
        {
            MessagingCenter.Send(this, "PopupPageMsg", true);
            PopupNavigation.Instance.PopAsync();
        }
       
    }
}