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
 * File Description:
 *
 *      This file contains the class for the settingpage that includes download/
 *      delete navigation graph, language selection and feedback feature.
 *      
 * Version:
 *
 *      1.0.0, 20190605
 * 
 * File Name:
 *
 *      SettingTableViewPage.xaml.cs
 *
 * Abstract:
 *
 *      Waypoint-based navigator is a mobile Bluetooth navigation application
 *      that runs on smart phones. It is structed to support anywhere 
 *      navigation indoors in areas covered by different indoor positioning 
 *      system (IPS) and outdoors covered by GPS.In particilar, it can rely on
 *      BeDIS (Building/environment Data and Information System) for indoor 
 *      positioning. This IPS provides a location beacon at every waypoint. The 
 *      beacon brocasts its own coordinates; Consequesntly, the navigator does 
 *      not need to continuously monitor its own position.
 *      This version makes use of Xamarin.Forms, which is a cross-platform UI 
 *      tookit that runs on both iOS and Android.
 *
 * Authors:
 *
 *      Kenneth Tang, kenneth@gm.nssh.ntpc.edu.tw
 *      Paul Chang, paulchang@iis.sinica.edu.tw
 *      Eric Lee, ericlee@iis.sinica.edu.tw
 *
 */
using IndoorNavigation.Modules;
using IndoorNavigation.Resources;
using IndoorNavigation.Resources.Helpers;
using IndoorNavigation.Views.Settings.LicensePages;
using Plugin.Multilingual;
using Prism.Commands;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Essentials;
using static IndoorNavigation.Utilities.Storage;
namespace IndoorNavigation.Views.Settings
{
    public partial class SettingTableViewPage : ContentPage
    {
        #region Variables and Objects

        private const string _resourceId = "IndoorNavigation.Resources.AppResources";
        private ResourceManager _resourceManager =
            new ResourceManager(_resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);
        public IList _selectNaviGraphItems { get; } = new ObservableCollection<string>();
        public IList _cleanNaviGraphItems { get; } = new ObservableCollection<string>();
        public IList _languageItems { get; } = new ObservableCollection<string>();
        public ICommand PrivatePolicyCommand => new Command(async() => 
        {
            await Launcher.TryOpenAsync(new Uri
                ("https://ec2-18-183-238-222.ap-northeast-1.compute.amazonaws.com/policy"));
        });

        public ICommand AboutAppCommand => new Command(async () =>
          await Navigation.PushAsync(new AboutSeeing_I_GOPage()));

        public ICommand ThirdPartyCommand => new Command(async () =>
         {
             await Navigation.PushAsync(new ThirdPartyUsagePage());
         });

        public ICommand VersionTapCommand => new Command(() => VersionTapped());
        #endregion

        #region Command defined
        public ICommand _changeLanguageCommand => new DelegateCommand(HandleChangeLanguage);
        #endregion

        #region Initial
        public SettingTableViewPage()
        {
            InitializeComponent();
            VersionTracking.Track();
            BindingContext = this;

            ((NavigationPage)Application.Current.MainPage).BarBackgroundColor = Color.FromHex("#3F51B5");
            ((NavigationPage)Application.Current.MainPage).BarTextColor = Color.White;

            var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
            _languageItems.Add(_resourceManager.GetString("CHINESE_STRING", currentLanguage));
            _languageItems.Add(_resourceManager.GetString("ENGLISH_STRING", currentLanguage));

            if (Application.Current.Properties.ContainsKey("LanguagePicker"))
            {
                LanguagePicker.SelectedItem = Application.Current.Properties["LanguagePicker"].ToString();
            }
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            VersionNumberCell.Title = VersionTracking.CurrentVersion;
        }
        #endregion

        #region Tapped Event
        async void LicenseBtn_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LicenseMainPage());
        }

        void SpeechTestBtn_Tapped(object sender, EventArgs e)
        {
            Utility._textToSpeech.Speak(GetResourceString("VOICE_SPEAK_STRING"), 
                GetResourceString("CULTURE_VERSION_STRING"));
        }
        #endregion

        #region Command Handle
        private int _tapCount=0;
        private void VersionTapped()
        {
            if (_tapCount >= 5)
                DevelopmentSection.IsVisible = true;
            _tapCount++;
        }

        private async void HandleChangeLanguage()
        {
            switch (LanguagePicker.SelectedItem.ToString())
            {
                case "英文":
                case "English":
                    CrossMultilingual.Current.CurrentCultureInfo = new CultureInfo("en-US");
                    break;
                case "中文":
                case "Chinese":
                    CrossMultilingual.Current.CurrentCultureInfo = new CultureInfo("zh-TW");
                    break;

                default:
                    break;
            }
            Console.WriteLine("Current Culture is : " + CultureInfo.CurrentCulture.Name);
            AppResources.Culture = CrossMultilingual.Current.CurrentCultureInfo;
            _currentCulture = AppResources.Culture;
            await Navigation.PushAsync(new MainPage());
        }

        protected override void OnDisappearing()
        {
            if (LanguagePicker.SelectedItem != null)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    var ci = CrossMultilingual.Current.CurrentCultureInfo;
                    var languageSelected = "";

                    switch (LanguagePicker.SelectedItem.ToString())
                    {
                        case "英文":
                        case "English":
                            languageSelected = _resourceManager.GetString("ENGLISH_STRING", ci);
                            break;
                        case "中文":
                        case "Chinese":
                            languageSelected = _resourceManager.GetString("CHINESE_STRING", ci);
                            break;
                    }
                    Application.Current.Properties["LanguagePicker"] = languageSelected;
                    await Application.Current.SavePropertiesAsync();
                });
            }
            base.OnDisappearing();
        }
        #endregion
    }
}