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
 *      RegisterListViewModel.cs
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
using IndoorNavigation.Views.PopUpPage;

namespace IndoorNavigation.ViewModels
{
    class RegisterListViewModel:BaseViewModel
    {
        const string _resourceId = "IndoorNavigation.Resources.AppResources";
        ResourceManager _resourceManager =
            new ResourceManager(_resourceId, 
								typeof(TranslateExtension)
								.GetTypeInfo().Assembly);

        CultureInfo currentLanguage = 
			CrossMultilingual.Current.CurrentCultureInfo;
			
        private Page mainPage = Application.Current.MainPage;
        private App app = (App)Application.Current;

        private string _naviGraphName;

        public RegisterListViewModel() { }
        //bool 
        public RegisterListViewModel(string navigationGraphName)
        {
            _naviGraphName = navigationGraphName;
            if (false)//app.IDnumber.Equals(string.Empty))
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
		   await PopupNavigation.Instance.PushAsync(new AskRegisterPopupPage(_naviGraphName));                       
        }               
        private string GetResourceString(string key)
        {
            return _resourceManager.GetString(key, currentLanguage);
        }

        public async void CheckSignIn()
        {
            string IDnum = 
				Preferences.Get("ID_NUMBER_STRING", string.Empty);
				
            string patientID = 
				Preferences.Get("PATIENT_ID_STRING", string.Empty);				

            if (IDnum.Equals(string.Empty) || patientID.Equals(string.Empty))
            {
                Console.WriteLine("Enter No IDnumber");               
                await PopupNavigation.Instance.PushAsync(new SignInPopupPage(_naviGraphName));
            }
        }       
    }

}