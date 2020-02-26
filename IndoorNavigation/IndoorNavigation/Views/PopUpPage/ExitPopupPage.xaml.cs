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
 *      ExitPopupPage.xaml.cs
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Rg.Plugins.Popup.Pages;
using System.Resources;
using IndoorNavigation.Resources.Helpers;
using System.Reflection;
using System.Collections.ObjectModel;
using Rg.Plugins.Popup;
using Rg.Plugins.Popup.Services;
using Plugin.Multilingual;
using System.Globalization;
namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ExitPopupPage :PopupPage
    {
        const string _resourceId = "IndoorNavigation.Resources.AppResources";
        ResourceManager _resourceManager =
            new ResourceManager(_resourceId, 
								typeof(TranslateExtension)
								.GetTypeInfo().Assembly);

        App app = (App)Application.Current;
        string _navigationGraphName;
        ExitPopupViewModel _viewmodel;
        CultureInfo currentLanguage = 
			CrossMultilingual.Current.CurrentCultureInfo;
        

        public ExitPopupPage(string navigationGraphName)
        {
            InitializeComponent();
            BackgroundColor = Color.FromRgba(150, 150, 150, 70);
            _navigationGraphName = navigationGraphName;
            _viewmodel = new ExitPopupViewModel(_navigationGraphName);                        

            BindingContext = _viewmodel;            
        }

         protected override bool OnBackButtonPressed()
        {
            DisplayAlert(_resourceManager
							.GetString("MESSAGE_STRING",currentLanguage),
						 _resourceManager
							.GetString("SELECT_EXIT_STRING", currentLanguage),
						 _resourceManager
							.GetString("OK_STRING",currentLanguage));
									
            return true;
			// Return true if you don't want to close this popup page when a 
			// back button is pressed
        }

         protected override bool OnBackgroundClicked()
        {
            
            DisplayAlert(_resourceManager
							.GetString("MESSAGE_STRING", 
									   currentLanguage), 
						 _resourceManager
							.GetString("SELECT_EXIT_STRING", 
									   currentLanguage), 
					     _resourceManager
							.GetString("OK_STRING", 
									   currentLanguage));         
            return false;
			// Return false if you don't want to close this popup page when a 
			// background of the popup page is clicked
        }

        
    }
}