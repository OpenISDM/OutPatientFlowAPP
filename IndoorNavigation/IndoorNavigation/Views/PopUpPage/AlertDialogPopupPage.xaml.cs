/*
 * Copyright (c) 2019 Academia Sinica, Institude of Information Science
 *
 * License:
 *      GPL 3.0 : The content of this file is subject to the terms and
 *      conditions defined in file 'COPYING.txt', which is part of this source
 *      code package.
 *
 * Project Name:
 *
 *      IndoorNavigation
 *
 * 
 *     
 *      
 * Version:
 *
 *      1.0.0, 20200221
 * 
 * File Name:
 *
 *      AlertDialogPopupPage.xaml.cs
 *
 * Abstract:
 *      
 *
 *      
 * Authors:
 * 
 *      Jason Chang, jasonchang@iis.sinica.edu.tw    
 *      
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rg.Plugins.Popup.Pages;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Rg.Plugins.Popup.Services;
using Rg.Plugins.Popup.Extensions;
using Xamarin.Forms.PlatformConfiguration.TizenSpecific;

namespace IndoorNavigation.Views.PopUpPage
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AlertDialogPopupPage : PopupPage
    {
        delegate void Background_BackButtonClickEvent();
        Background_BackButtonClickEvent _backClick;
        bool isButtonClicked = false;

        Style ButtonStyle = new Style(typeof(Button))
        {
            Setters ={
                new Setter{Property=Button.FontSizeProperty,
                    Value=Device.GetNamedSize(NamedSize.Large,typeof(Button))},
                new Setter{Property=Button.TextColorProperty,
                    Value=Color.FromHex("#3f51b5")},
                new Setter{Property=Button.HorizontalOptionsProperty,
                    Value=LayoutOptions.End},
                new Setter{Property=Button.VerticalOptionsProperty,
                    Value=LayoutOptions.EndAndExpand},
                new Setter{Property=Button.BackgroundColorProperty,
                    Value=Color.Transparent}
            }
        };

        TaskCompletionSource<bool> _tcs = null;

        #region for no button view that it will be closed by page itself   
        public AlertDialogPopupPage(string context)
        {
            InitializeComponent();

            _backClick = NoButton_Back;
            TempMessage.Text = context;
            Device.StartTimer(TimeSpan.FromSeconds(2.2), () =>
            {
                //to prevent from crash issue that user have close the popup 
                //page, then popup page stack is empty.
                if (PopupNavigation.Instance.PopupStack.Count > 0)
                {
                    PopupNavigation.Instance.RemovePageAsync(this);
                    Console.WriteLine("Close the popup page by timer");
                }
                Console.WriteLine("Close the popup page by user");
                return false;
            });
        }
        async private void NoButton_Back()
        {
            await PopupNavigation.Instance.RemovePageAsync(this);
        }
        #endregion

        #region for the view have both of cancel and confirm.
        string _message;
        public AlertDialogPopupPage(string context,
                                    string confirm,
                                    string cancel,
                                    string message)
        {
            InitializeComponent();
            TempMessage.Text = context;
            _message = message;
            _backClick = TwoButton_Back;

            Button ConfirmBtn =
                new Button { Style = ButtonStyle, Text = confirm };
            Button CancelBtn =
                new Button { Style = ButtonStyle, Text = cancel };
            CancelBtn.Clicked += CancelPageClicked;
            ConfirmBtn.Clicked += ConfirmPageClicked;
            buttonLayout.Children.Add(CancelBtn);
            buttonLayout.Children.Add(ConfirmBtn);

        }
        async private void TwoButton_Back()
        {           
            await PopupNavigation.Instance.RemovePageAsync(this);
            MessagingCenter.Send(this, _message, false);
           // await Task.CompletedTask;
        }

        #endregion

        #region for view with only one button that is used to alerting user.
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

        #region
        public AlertDialogPopupPage(string context, string confirm,string cancel)
        {
            InitializeComponent();
            TempMessage.Text = context;
            _backClick = TwoButton_Back_new;

            Button ConfirmButton =
                new Button { Style = ButtonStyle, Text = confirm };
            Button CancelButton =
                new Button { Style = ButtonStyle, Text = cancel };
            CancelButton.Clicked += CancelPageClicked;
            ConfirmButton.Clicked += ConfirmPageClicked_new;

            buttonLayout.Children.Add(CancelButton);
            buttonLayout.Children.Add(ConfirmButton);
        }

        async private void TwoButton_Back_new()
        {
            await PopupNavigation.Instance.RemovePageAsync(this);
        }

        async private void ConfirmPageClicked_new(object sender, EventArgs e)
        {
            if (isButtonClicked) return;
            isButtonClicked = true;

            _tcs?.SetResult(true);
            await PopupNavigation.Instance.RemovePageAsync(this);
            isButtonClicked = false;
        }
        #endregion

        #region common code

        private void CancelPageClicked(Object sender, EventArgs args)
        {
            if (isButtonClicked) return;
            isButtonClicked = true;

            _tcs?.SetResult(false);

            _backClick();
        }

        async private void ConfirmPageClicked(Object sender, EventArgs args)
        {
            if (isButtonClicked) return;
            isButtonClicked = true;


            _tcs?.SetResult(true);
            await PopupNavigation.Instance.RemovePageAsync(this);
            MessagingCenter.Send(this, _message, true);
        }

        protected override bool OnBackButtonPressed()
        {
            _backClick();
            return true;
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            _backClick();
        }

        public async Task<bool> show()
        {
            _tcs = new TaskCompletionSource<bool>();
            await Navigation.PushPopupAsync(this);            
            return await _tcs.Task;
        }
        #endregion
    }
}

