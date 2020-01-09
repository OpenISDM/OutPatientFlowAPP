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
        delegate void Background_BackButtonClickEvent();
        Background_BackButtonClickEvent _backClick;
//-----------------no button-------------------------------------------        
        public AlertDialogPopupPage(string cotext)
        {
            InitializeComponent();

            _backClick = NoButton_Back;

            Device.StartTimer(TimeSpan.FromSeconds(2.2), () =>
            {
                //to prevent from crash issue that user have close the popup page then popup stack is empty.
                if (PopupNavigation.Instance.PopupStack.Count > 0)
                {
                    PopupNavigation.Instance.PopAsync();
                    Console.WriteLine("Close the popup page by timer");
                }
                Console.WriteLine("Close the popup page by user");
                return false;
            });
        }
        async private void NoButton_Back()
        {
            await PopupNavigation.Instance.PopAsync();
        }
//---------------------two buttons------------------------------------------------
        public AlertDialogPopupPage(string context,string confirm,string cancel)
        {
            InitializeComponent();           
            TempMessage.Text = context;
            _backClick = TwoButton_Back;
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
            CancelBtn.Clicked += CancelPageClicked;
            ConfirmBtn.Clicked += ConfirmPageClicked;
            buttonLayout.Children.Add(ConfirmBtn);
            buttonLayout.Children.Add(CancelBtn);
        }
        private void TwoButton_Back()
        {
            MessagingCenter.Send(this, "PopupPageMsg", false);
            PopupNavigation.Instance.PopAsync();
        }
        private void CancelPageClicked(Object sender, EventArgs args)
        {
            _backClick();
        }
        private void ConfirmPageClicked(Object sender, EventArgs args)
        {
            MessagingCenter.Send(this, "PopupPageMsg", true);
            PopupNavigation.Instance.PopAsync();
        }
        //----------------------one button------------------------------------------------------
        public AlertDialogPopupPage(string context,string cancel)
        {
            InitializeComponent();            
            TempMessage.Text = context;

            _backClick = NoButton_Back;

            Button CancelBtn = new Button {
                Text=cancel,
                FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Button)),
                TextColor = Color.FromHex("#3f51b5"),
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.EndAndExpand,
                BackgroundColor = Color.Transparent
            };
            CancelBtn.Clicked += CancelPageClicked ;             
            buttonLayout.Children.Add(CancelBtn);
        }       
//----------------------Common code------------------------------------------------
        protected override bool OnBackButtonPressed() //待測試，可能會錯
        {
            _backClick();
            return false;
            //return base.OnBackButtonPressed();
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            _backClick();
        }
    }
}