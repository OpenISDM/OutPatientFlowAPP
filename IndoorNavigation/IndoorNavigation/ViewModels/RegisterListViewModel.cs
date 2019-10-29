using System;
using System.Collections.Generic;
using System.Text;
using MvvmHelpers;
using Plugin.Multilingual;
using Xamarin.Forms;
using System.Resources;
using Xamarin.Essentials;
using IndoorNavigation.Resources.Helpers;
using System.Reflection;
using Rg.Plugins.Popup.Events;
namespace IndoorNavigation.ViewModels
{
    class RegisterListViewModel:BaseViewModel
    {
        Rg.Plugins.Popup.Contracts.IPopupNavigation navigation;
        const string _resourceId = "IndoorNavigation.Resources.AppResources";
        ResourceManager _resourceManager =
            new ResourceManager(_resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);

        private int _popupStackCount;

        public RegisterListViewModel(bool isFirstTime)
        {
          //  CheckRegister(isFirstTime);
            CheckSignIn();
           // navigation.PopupStack
        }
        public RegisterListViewModel()
        {
            CheckSignIn();
        }
        public async void CheckRegister(bool isFirstTime)
        {
            
            var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;

            if (!isFirstTime)
            {
                var page = Application.Current.MainPage;
                bool isRegister = await page.DisplayAlert(_resourceManager.GetString("MESSAGE_STRING", currentLanguage),
                  _resourceManager.GetString("NEED_REGISTER_STRING", currentLanguage),
                  _resourceManager.GetString("OK_STRING", currentLanguage),
                  _resourceManager.GetString("CANCEL_STRING", currentLanguage));
                if (isRegister)
                {
                    await page.Navigation.PushAsync(new TestPage());
                }
            }
        }
        public int PopupStackCount
        {
            get { return _popupStackCount; }
            set
            {
                if (_popupStackCount != value)
                {
                    _popupStackCount = value;
                    OnPropertyChanged("SelectedItem");
                    
                }
            }
        }
        public async void CheckSignIn()
        {
            string IDnum = Preferences.Get("ID_NUMBER_STRING", string.Empty);
            string patientID = Preferences.Get("PATIENT_ID_STRING", string.Empty);

            if (IDnum.Equals(string.Empty) || patientID.Equals(string.Empty))
            {
                var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;

                Page mainPage = Application.Current.MainPage;

                await mainPage.DisplayAlert(
                  _resourceManager.GetString("MESSAGE_STRING", currentLanguage),
                                        _resourceManager.GetString("ALERT_LOGIN_STRING", currentLanguage),
                                        _resourceManager.GetString("OK_STRING", currentLanguage));

                await mainPage.Navigation.PushAsync(new SignInPage());
            }
        }
    }
}
