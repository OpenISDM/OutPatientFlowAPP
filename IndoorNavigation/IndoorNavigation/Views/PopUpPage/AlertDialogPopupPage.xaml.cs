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

        Style ButtonStyle = new Style(typeof(Button))
        {
            Setters ={
                new Setter{Property=Button.FontSizeProperty, Value=Device.GetNamedSize(NamedSize.Large,typeof(Button))},
                new Setter{Property=Button.TextColorProperty,Value=Color.FromHex("#3f51b5")},
                new Setter{Property=Button.HorizontalOptionsProperty,Value=LayoutOptions.End},
                new Setter{Property=Button.VerticalOptionsProperty,Value=LayoutOptions.EndAndExpand},
                new Setter{Property=Button.BackgroundColorProperty,Value=Color.Transparent}
            }
        };

        #region for no button view that it will close itself   
        public AlertDialogPopupPage(string context)
        {
            InitializeComponent();

            _backClick = NoButton_Back;
            TempMessage.Text = context;
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
        #endregion

        #region for the view have both of cancel and confirm.
        //---------------------two buttons------------------------------------------------
        string _message;
        public AlertDialogPopupPage(string context, string confirm, string cancel, string message)
        {
            InitializeComponent();
            TempMessage.Text = context;
            _message = message;
            _backClick = TwoButton_Back;

            Button ConfirmBtn = new Button { Style = ButtonStyle, Text = confirm };
            Button CancelBtn = new Button { Style = ButtonStyle, Text = cancel };
            CancelBtn.Clicked += CancelPageClicked;
            ConfirmBtn.Clicked += ConfirmPageClicked;
            buttonLayout.Children.Add(ConfirmBtn);
            buttonLayout.Children.Add(CancelBtn);
        }
        private void TwoButton_Back()
        {
            MessagingCenter.Send(this, _message, false);
            PopupNavigation.Instance.PopAsync();
        }

        #endregion

        #region for view with only one button that is cancel.
        public AlertDialogPopupPage(string context, string cancel)
        {
            InitializeComponent();
            TempMessage.Text = context;

            _backClick = NoButton_Back;

            Button CancelBtn = new Button { Style = ButtonStyle, Text = cancel };

            CancelBtn.Clicked += CancelPageClicked;
            buttonLayout.Children.Add(CancelBtn);
        }
        #endregion

        #region common code

        private void CancelPageClicked(Object sender, EventArgs args)
        {
            _backClick();
        }

        private void ConfirmPageClicked(Object sender, EventArgs args)
        {
            MessagingCenter.Send(this, _message, true);
            PopupNavigation.Instance.PopAsync();
        } 

        protected override bool OnBackButtonPressed() //待測試，可能會錯
        {
            _backClick();
            return false;
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            _backClick();
        }
        #endregion
    }
}