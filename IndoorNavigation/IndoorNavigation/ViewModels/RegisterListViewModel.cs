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
using System.Globalization;
using System.Windows.Input;
using IndoorNavigation.Models.NavigaionLayer;
using IndoorNavigation.Models;
namespace IndoorNavigation.ViewModels
{
    class RegisterListViewModel:BaseViewModel
    {
        const string _resourceId = "IndoorNavigation.Resources.AppResources";
        ResourceManager _resourceManager =
            new ResourceManager(_resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);

        CultureInfo currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
        private Page mainPage = Application.Current.MainPage;
        private App app = (App)Application.Current;


        //bool 
        public RegisterListViewModel()
        {                      
            if (app.IDnumber.Equals(string.Empty))
            {
                CheckSignIn();
            }
            else if(!app.isRigistered)
            {
                CheckRegister();
                app.isRigistered = true;
            }            
        }
        async public void CheckRegister()
        {
            await PopupNavigation.Instance.PushAsync(new AskRegisterPopupPage());                       
        }               
        private string GetResourceString(string key)
        {
            return _resourceManager.GetString(key, currentLanguage);
        }

        public async void CheckSignIn()
        {
            string IDnum = Preferences.Get("ID_NUMBER_STRING", string.Empty);
            string patientID = Preferences.Get("PATIENT_ID_STRING", string.Empty);

            if (IDnum.Equals(string.Empty) || patientID.Equals(string.Empty))
            {
                var wantSignIn = await mainPage.DisplayAlert(
                  GetResourceString("MESSAGE_STRING"), GetResourceString("ALERT_LOGIN_STRING"), GetResourceString("OK_STRING"), GetResourceString("CANCEL_STRING"));                                   
                if (wantSignIn)
                    await mainPage.Navigation.PushAsync(new SignInPage());
                else
                {
                    await mainPage.Navigation.PopAsync() ;
                }
            }
        }       
    }

}
