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
using Rg.Plugins.Popup.Services;
namespace IndoorNavigation.ViewModels
{
    class RegisterListViewModel:BaseViewModel
    {
        const string _resourceId = "IndoorNavigation.Resources.AppResources";
        ResourceManager _resourceManager =
            new ResourceManager(_resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);
        App app = (App)Application.Current;

        public RegisterListViewModel()
        {
            if (app.IDnumber.Equals(string.Empty))
            {
                CheckSignIn();
            }
            else
            {
                CheckRegister();
                app.isRigistered = true;
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
