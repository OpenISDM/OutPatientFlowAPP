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
using IndoorNavigation.Models;
using IndoorNavigation.Modules;
using IndoorNavigation.Resources;
using IndoorNavigation.Resources.Helpers;
using IndoorNavigation.Utilities;
using IndoorNavigation.Views.PopUpPage;
using IndoorNavigation.Views.Settings.LicensePages;
using Plugin.Multilingual;
using Prism.Commands;
using Rg.Plugins.Popup.Services;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Essentials;
using Location = IndoorNavigation.ViewModels.Location;
using static IndoorNavigation.Utilities.Storage;
using Dijkstra.NET.Model;
/*Note : remember to edit GetAllGraph method.*/

namespace IndoorNavigation.Views.Settings
{
    public partial class SettingTableViewPage : ContentPage
    {
        #region Variables and Objects

        private const string _resourceId = "IndoorNavigation.Resources.AppResources";
        private ResourceManager _resourceManager =
            new ResourceManager(_resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);

        //private DownloadPopUpPage _downloadPage = new DownloadPopUpPage();
        //private string _downloadURL;
        public IList _selectNaviGraphItems { get; } = new ObservableCollection<string>();
        public IList _cleanNaviGraphItems { get; } = new ObservableCollection<string>();
        public IList _languageItems { get; } = new ObservableCollection<string>();
        public IList _chooseMap { get; } = new ObservableCollection<string>();
        public IList _downloadMap { get; } = new ObservableCollection<string>();

        //private bool _connectable = false;
        public ICommand PrivatePolicyCommand => new Command(() => 
        {
            Device.OpenUri(new Uri("https://ec2-18-183-238-222.ap-northeast-1.compute.amazonaws.com/policy"));
        });
        #endregion

        #region Command defined
        //public ICommand _chooseMapCommand => new DelegateCommand(HandleChooseMap);
        //public ICommand _cleanMapCommand => new DelegateCommand(async () =>
        //{ await HandleCLeanMapAsync(); });
        //public ICommand _downloadItemCommand => new DelegateCommand(ChooseDownloadMap);

        public ICommand _changeLanguageCommand => new DelegateCommand(HandleChangeLanguage);
        #endregion

        #region Initial
        public SettingTableViewPage()
        {
            InitializeComponent();
            //AddMapItems();
            VersionTracking.Track();

            //if (_serverResources != null)
            //{
            //    _connectable = true;

            //    foreach (KeyValuePair<string, GraphInfo> pair in _serverResources)
            //    {
            //        Console.WriteLine("String value is : " + pair.Key);
            //        _downloadMap.Add(pair.Value._displayNames[_currentCulture.Name]);
            //    }
            //}

            //_downloadPage._event.DownloadPopUpPageEventHandler +=
            //    async delegate (object sender, EventArgs e) { await HandleDownloadPageAsync(sender, e); };

            BindingContext = this;

            ((NavigationPage)Application.Current.MainPage).BarBackgroundColor = Color.FromHex("#3F51B5");
            ((NavigationPage)Application.Current.MainPage).BarTextColor = Color.White;

            //ReloadNaviGraphItems();

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
            //DownloadFromServer.IsEnabled = _connectable;
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
            var ci = CrossMultilingual.Current.CurrentCultureInfo;
            Utility._textToSpeech.Speak(_resourceManager.GetString("VOICE_SPEAK_STRING", ci),
                _resourceManager.GetString("CULTURE_VERSION_STRING", ci));
        }

        #endregion

        #region Command Handle
        /// <summary>
        /// Handles the download page event.
        /// </summary>
        /// <returns>The download page async.</returns>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        //private async Task HandleDownloadPageAsync(object sender, EventArgs e)
        //{
        //    var ci = CrossMultilingual.Current.CurrentCultureInfo;
        //    string fileName = (e as DownloadPopUpPageEventArgs).FileName;
        //    if (!string.IsNullOrEmpty(_downloadURL) && !string.IsNullOrEmpty(fileName))
        //    {

        //        if (Utility.DownloadNavigraph(_downloadURL, fileName))
        //        {
        //            await DisplayAlert(_resourceManager.GetString("MESSAGE_STRING", ci),
        //                               _resourceManager.GetString("SUCCESSFULLY_DOWNLOAD_MAP_STRING", ci),
        //                               _resourceManager.GetString("OK_STRING", ci));
        //        }
        //        else
        //        {
        //            await DisplayAlert(_resourceManager.GetString("ERROR_STRING", ci),
        //                               _resourceManager.GetString("FAILED_DOWNLOAD_MAP_STRING", ci),
        //                               _resourceManager.GetString("OK_STRING", ci));
        //        }
        //    }
        //    else
        //    {
        //        await DisplayAlert(_resourceManager.GetString("ERROR_STRING", ci),
        //                           _resourceManager.GetString("FAILED_DOWNLOAD_MAP_STRIN", ci),
        //                           _resourceManager.GetString("OK_STRING", ci));
        //    }
        //    string fileLanguageTaiwanChinese = fileName + "_zh.xml";
        //    //Testing in Lab
        //    string firstDirectionFile_zh_TW = "https://drive.google.com/uc?authuser=0&id=1C_ncshn2Q2veLMVMgvqW81xLP3DJnQpW&export=download";

        //    //Testing in Taipei City hall 2F
        //    // string firstDirectionFile_zh_TW = "https://drive.google.com/uc?authuser=0&id=17AarNw7QqBFlRqSNMTjwU8_WkMK98SPI&export=download";
        //    Utility.DownloadFirstDirectionFile(firstDirectionFile_zh_TW, fileLanguageTaiwanChinese);

        //    string stringfileLanguageUSEnglish = fileName + "_en-US.xml";
        //    //Testing in Lab
        //    string firstDirectionFile_en_US = "https://drive.google.com/uc?authuser=0&id=1dvmo3WjW_2dljvJ0qY1sVK5qX6PNWg_g&export=download";

        //    //Testing in Taipei City Hall 2F
        //    //string firstDirectionFile_en_US = "https://drive.google.com/uc?authuser=0&id=1f8zTIMWJFOsNybVwm-kkSo4enNM7lIKY&export=download";

        //    Utility.DownloadFirstDirectionFile(firstDirectionFile_en_US, stringfileLanguageUSEnglish);

        //    string infoTaiwanChinese = fileName + "_info_zh.xml";
        //    string infoEnglish = fileName + "_info_en-US.xml";


        //    string infoFile_zh_TW = "https://drive.google.com/uc?authuser=0&id=1Fajcicwcrg_GHhabuygEZyhyUJxxDY3f&export=download";
        //    Utility.DownloadInformationFile(infoFile_zh_TW, infoTaiwanChinese);

        //    string infoFile_en_US = "https://drive.google.com/uc?authuser=0&id=1KCbZUDPrfGv5H14OTSX2PaTnREG8Xk94&export=download";
        //    Utility.DownloadInformationFile(infoFile_en_US, infoEnglish);

        //    ReloadNaviGraphItems();
        //}

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
            Storage._currentCulture = AppResources.Culture;
            //AddMapItems();

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