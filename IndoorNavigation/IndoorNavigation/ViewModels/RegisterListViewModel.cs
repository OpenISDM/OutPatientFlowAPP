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
using Rg.Plugins.Popup.Services;
namespace IndoorNavigation.ViewModels
{
    class RegisterListViewModel:BaseViewModel
    {
        const string _resourceId = "IndoorNavigation.Resources.AppResources";
        ResourceManager _resourceManager =
            new ResourceManager(_resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);
        App app = (App)Application.Current;
        private bool isSignIn = false;
        public bool isCheckRegister = false;
        public double pixelwidth = Application.Current.MainPage.Width;
        public double pixelheight = Application.Current.MainPage.Height;

        public double layoutwidth { get; set; }
        public double layoutheight { get; set; }


        public RegisterListViewModel()
        {
            layoutheight = pixelheight / 6.1;
            layoutwidth = layoutheight;
            string IDnum = Preferences.Get("ID_NUMBER_STRING", string.Empty);
            string patientID = Preferences.Get("PATIENT_ID_STRING", string.Empty);
            if (!(IDnum.Equals(string.Empty) || patientID.Equals(string.Empty)))// || birthday.Equals(DateTime.Now))
            {
                isSignIn = true;
            }

            if (!isSignIn)
                CheckSignIn();
            else
            {
                CheckRegister();
                isCheckRegister = true;
            }

            
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
    
        
        async public  void CheckRegister()
        {
            var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
            Page nowPage = Application.Current.MainPage;
           
            //var NeedtoRegister =await nowPage.DisplayAlert(_resourceManager.GetString("MESSAGE_STRING", currentLanguage), _resourceManager.GetString("NEED_REGISTER_STRING", currentLanguage), _resourceManager.GetString("OK_STRING", currentLanguage),_resourceManager.GetString("CANCEL_STRING",currentLanguage));
            await PopupNavigation.Instance.PushAsync(new AskRegisterPopupPage());
            
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
