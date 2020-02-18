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
        int _recordsNumber;
        bool _cashierAndpharmacy;
        bool _isBusy = false;
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
            Page nowPage = Application.Current.MainPage;                       
            await PopupNavigation.Instance.PushAsync(new AskRegisterPopupPage());            
        }
        

        public async void CheckSignIn()
        {
            string IDnum = Preferences.Get("ID_NUMBER_STRING", string.Empty);
            string patientID = Preferences.Get("PATIENT_ID_STRING", string.Empty);

            if (IDnum.Equals(string.Empty) || patientID.Equals(string.Empty))
            {               
                Page mainPage = Application.Current.MainPage;

                var  wantSignIn=await mainPage.DisplayAlert(
                  _resourceManager.GetString("MESSAGE_STRING", currentLanguage),
                                        _resourceManager.GetString("ALERT_LOGIN_STRING", currentLanguage),
                                        _resourceManager.GetString("OK_STRING", currentLanguage),_resourceManager.GetString("CANCEL_STRING",currentLanguage));
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
