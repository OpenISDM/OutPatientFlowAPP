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
 *      SelectTwoWayPopupPage.xaml.cs
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
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using IndoorNavigation.Views.Navigation;
using System.Resources;
using IndoorNavigation.Resources.Helpers;
using System.Reflection;
using System.Globalization;
using Plugin.Multilingual;
using IndoorNavigation.Views.PopUpPage;
using IndoorNavigation.Models;
using IndoorNavigation.Views.OPFM;
using IndoorNavigation.ViewModels;

namespace IndoorNavigation.Views.PopUpPage
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SelectTwoWayPopupPage : PopupPage
    {
        String _locationName;
        Location _location;
        bool isButtonPressed = false;        
        Page page = Application.Current.MainPage;

        private const string _resourceId = 
			"IndoorNavigation.Resources.AppResources";

        ResourceManager _resourceManager =
            new ResourceManager(_resourceId, 
								typeof(TranslateExtension)
								.GetTypeInfo().Assembly);

        CultureInfo currentLanguage = 
			CrossMultilingual.Current.CurrentCultureInfo;

        private INetworkSetting setting;

        public SelectTwoWayPopupPage()
        {
            InitializeComponent();
        }

        public SelectTwoWayPopupPage(Location location)
        {
            InitializeComponent();
            //BackgroundColor = Color.FromRgba(150, 150, 150, 70);
            _locationName = location.sourcePath;
            _location = location;
            setting = DependencyService.Get<INetworkSetting>();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            //Console.WriteLine("the width of a button is : " + a.Width);
            //Console.WriteLine("the width of b button is : " + b.Width);
        }

        async private void ToNavigationBtn_Clicked(object sender, EventArgs e)
        {
            if (isButtonPressed) return;
            isButtonPressed = true;  
            
            await PopupNavigation.Instance.PopAllAsync();
            await page.Navigation.PushAsync
				(new NavigationHomePage(_location));
        }

        async private void ToOPFM_Clicked(object sender, EventArgs e)
        {
            if (isButtonPressed) return;
            isButtonPressed = true;

            await PopupNavigation.Instance.PopAsync();

            await PopupNavigation.Instance.PushAsync(new IndicatorPopupPage());
            bool NetworkIsFine = await setting.CheckInternetConnect();
            await PopupNavigation.Instance.PopAllAsync();
            if (NetworkIsFine)
            {
                await page.Navigation.PushAsync(new RigisterList(_locationName));
            }
            else 
            {
                Console.WriteLine(">>SelectTwoWay : No Network");
                await PopupNavigation.Instance.PushAsync(new AlertDialogPopupPage(
                    _resourceManager.GetString("ALERT_IF_YOU_HAVE_NETWORK_STRING", currentLanguage),
                    _resourceManager.GetString("GO_TO_SETTING", currentLanguage),
                    _resourceManager.GetString("CANCEL_STRING",currentLanguage),
                    "GoToSetting"));

                MessagingCenter.Subscribe<AlertDialogPopupPage, bool>(this, "GoToSetting",async (msgSender, msgArgs) => 
                {
                    Console.WriteLine("Go to setting messagingCenter sender is : " + (bool)msgArgs);

                    if ((bool)msgArgs)
                    {
                        INetworkSetting setting = DependencyService.Get<INetworkSetting>();
                        setting.OpenSettingPage();                        
                    }

                    await PopupNavigation.Instance.PushAsync(new SelectTwoWayPopupPage(_location));

                    MessagingCenter.Unsubscribe<AlertDialogPopupPage, bool>(this, "GoToSetting");
                });                
            } 
        }

        protected override bool OnBackButtonPressed()
        {
            return base.OnBackButtonPressed();
        }
        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();
        }
    }
}