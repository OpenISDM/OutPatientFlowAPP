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
 *      SignInPage.xaml.cs
 *
 * Abstract:
 *      
 *
 *      
 * Authors:
 * 
 *      Jason Chang, jasonchang@iis.sinica.edu.tw    
 */     
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using System.Globalization;
using System.Reflection;
using Plugin.Multilingual;
using IndoorNavigation.Resources.Helpers;
using System.Resources;

using IndoorNavigation.Views.Controls;
namespace IndoorNavigation.Views.OPFM
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SignInPage : ContentPage
    {
        private const string _resourceId = 
			"IndoorNavigation.Resources.AppResources";
        ResourceManager _resourceManager = 
			new ResourceManager(_resourceId, 
								typeof(TranslateExtension)
								.GetTypeInfo().Assembly);
        CultureInfo currentLanguage = 
			CrossMultilingual.Current.CurrentCultureInfo;
			
        App app = (App)Application.Current;
        
        public SignInPage()
        {
            InitializeComponent();
            IDnumEntry.Keyboard = 
				Keyboard.Create(KeyboardFlags.CapitalizeCharacter);
				
            RgDayPicker.Date = 
				app.RgDate;
				
            IDnumEntry.Text = 
				Preferences.Get("ID_NUMBER_STRING", string.Empty);
        }

        async private void Button_Clicked(object sender, EventArgs e)
        {
            IDnumEntry.Text = IDnumEntry.Text.ToUpper();
            if(IDnumEntry.Text==null || !CheckIDLegal(IDnumEntry.Text))
            {
                await DisplayAlert
					(_resourceManager.GetString("ERROR_STRING",currentLanguage), 
					 _resourceManager.GetString("IDNUM_TYPE_WRONG_STRING", 
												currentLanguage),
                     _resourceManager.GetString("OK_STRING",currentLanguage));
                return;
            }            
            Preferences.Set("ID_NUMBER_STRING", IDnumEntry.Text);
            app.IDnumber = 
				IDnumEntry.Text;
				
            app.RgDate = 
				RgDayPicker.Date;
				
            app.isRigistered = 
				false;

            //await Navigation.PopAsync();
            await Navigation.PopModalAsync();
        }                

        //to check the ID number is a legal one or not.
        public bool CheckIDLegal(string IDnum)
        {
            if (IDnum.Length < 10)
                return false;
            int[] priority = { 1, 8, 7, 6, 5, 4, 3, 2, 1, 1 };
            int count = FirstCharacterNumber(IDnum[0]);

            for (int i = 1; i < IDnum.Length; i++)
            {
                int tmp = IDnum[i] - '0';
                if (!(tmp >= 0 && tmp <= 9))
                    return false;
                count += priority[i] * (tmp);
            }
            if(count%10==0)
                return true;
            return false;
        }
        //first char in identity number 
        private int FirstCharacterNumber(char ch)
        {
            switch (ch)
            {
                case 'A': return 1; case 'P': return 29; 
                case 'B': return 10; case 'Q': return 38; 
                case 'C': return 19; case 'R': return 47; 
                case 'D': return 28; case 'S': return 56;
                case 'E': return 37; case 'T': return 65; 
                case 'F': return 46; case 'U': return 74; 
                case 'G': return 55; case 'V': return 83; 
                case 'H': return 64; case 'W': return 21;
                case 'I': return 39; case 'X': return 3; 
                case 'J': return 73; case 'Y': return 12; 
                case 'K': return 82; case 'Z': return 30; 
                case 'L': return 2; 
                case 'M': return 11; 
                case 'N': return 20; 
                case 'O': return 48;
                default: return 0;
            }
        }


        protected override bool OnBackButtonPressed()
        {
            //Console.Write("aaaa");
            //return base.OnBackButtonPressed();
            //return true;

            Device.BeginInvokeOnMainThread(async() => 
                {
                    await Navigation.PopToRootAsync();
                }
            );
            //return base.OnBackButtonPressed();
            return true;
        }

        async private void ConfirmButton_Clicked(object sender, EventArgs e)
        {
            IDnumEntry.Text = IDnumEntry.Text.ToUpper();
            if (IDnumEntry.Text == null || !CheckIDLegal(IDnumEntry.Text))
            {
                await DisplayAlert
                    (_resourceManager.GetString("ERROR_STRING", currentLanguage),
                     _resourceManager.GetString("IDNUM_TYPE_WRONG_STRING",
                                                currentLanguage),
                     _resourceManager.GetString("OK_STRING", currentLanguage));
                return;
            }
            Preferences.Set("ID_NUMBER_STRING", IDnumEntry.Text);
            app.IDnumber = IDnumEntry.Text;

            app.RgDate = RgDayPicker.Date;

            app.isRigistered = false;
            await Navigation.PopAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            //if (string.IsNullOrEmpty(IDnumEntry.Text) || string.IsNullOrEmpty(app.IDnumber))
            //{
            //    Device.BeginInvokeOnMainThread(async () =>
            //    {
            //        await Navigation.PopToRootAsync();
            //    });
            //}
        }

        async private void CancelButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopToRootAsync();

            //Device.BeginInvokeOnMainThread(() =>
            //{
            //    for(int AsyncPagesIndex=Navigation.NavigationStack.Count-1; AsyncPagesIndex>0; AsyncPagesIndex--)
            //    {
            //        Navigation.RemovePage(Navigation.NavigationStack[AsyncPagesIndex]);
            //    }
            //});
        }
    }
}