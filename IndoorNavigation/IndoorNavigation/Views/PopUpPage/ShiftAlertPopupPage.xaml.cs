﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Rg.Plugins.Popup.Extensions;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace IndoorNavigation.Views.Navigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ShiftAlertPopupPage : PopupPage
    {
        private string _prefs;
        private TaskCompletionSource<bool> _tcs;

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
            Button CancelBtn = new Button
            {
                Text = cancel,
                FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Button)),
                TextColor = Color.FromHex("#3f51b5"),
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.EndAndExpand,
                BackgroundColor = Color.Transparent
            };

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
            Button CancelBtn = new Button
            {
                Text = cancel,
                FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Button)),
                TextColor = Color.FromHex("#3f51b5"),
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.EndAndExpand,
                BackgroundColor = Color.Transparent
            };
            Button ConfirmBtn = new Button
            {
                Text = confirm,
                FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Button)),
                TextColor = Color.FromHex("#3f51b5"),
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.EndAndExpand,
                BackgroundColor = Color.Transparent
            };

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

            _tcs?.SetResult(true);
        }

        protected override bool OnBackButtonPressed()
        {
            MessagingCenter.Send(this, "AlertBack", false);
            return base.OnBackButtonPressed();
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            isCheck(_prefs);
            //MessagingCenter.Send(this, _prefs, true);
            MessagingCenter.Send(this, "AlertBack", true);
            PopupNavigation.Instance.PopAsync();
        }

        async public Task<bool> Show()
        {
            _tcs = new TaskCompletionSource<bool>();
            await Navigation.PushPopupAsync(this);
            return await _tcs.Task;
        }
    }
}