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

        public RegisterListViewModel()
        {
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
            //if (NeedtoRegister)
            //{
            //    app.records.Add(new RgRecord
            //    {
            //        DptName = "導航至掛號台",
            //        _regionID = new Guid("11111111-1111-1111-1111-111111111111"),
            //        _waypointID = new Guid("00000000-0000-0000-0000-000000000002"),
            //        _waypointName = "掛號台",
            //        Key = "register"
            //    });
            //    app.records.Add(new RgRecord { Key = "NULL" });
            //}
            //else
            //{
            //    //load data from server
            //    app.records.Add(new RgRecord
            //    {
            //        DptName = "心臟血管科",
            //        _waypointName = "心臟科",
            //        _regionID = new Guid("11111111-1111-1111-1111-111111111111"),
            //        _waypointID = new Guid("00000000-0000-0000-0000-000000000002"),
            //        Shift = "50",
            //        CareRoom = "0205",
            //        DptTime = "8:30~10:00",
            //        SeeSeq = "50",
            //        Key = "QueryResult",
            //        isAccept = false,
            //        isComplete = false
            //    });
            //    app.records.Add(new RgRecord { Key = "NULL" });
            //}
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
