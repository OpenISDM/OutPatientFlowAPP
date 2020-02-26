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
 *      ShiftAlertPopupPage.xaml.cs
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
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace IndoorNavigation.Views.Navigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ShiftAlertPopupPage : PopupPage
    {
        private string _prefs;
        private Style ButtonStyle = new Style(typeof(Button))
        {
            Setters =
            {
                new Setter{ Property=Button.FontSizeProperty, 
							Value=Device.GetNamedSize
								(NamedSize.Large, typeof(Button))},
							
                new Setter{ Property=Button.TextColorProperty, 
							Value=Color.FromHex("#3f51b5") },
							
                new Setter{ Property=Button.HorizontalOptionsProperty, 
							Value=LayoutOptions.End},
							
                new Setter{	Property=Button.VerticalOptionsProperty, 
							Value=LayoutOptions.EndAndExpand},
                new Setter{	Property=Button.BackgroundColorProperty, 
							Value=Color.Transparent}
            }
        };

        public ShiftAlertPopupPage()
        {
            InitializeComponent();
            BackgroundColor = Color.FromRgba(150, 150, 150, 70);
        }

        public ShiftAlertPopupPage(	string AlertContext, 
									string cancel,
									string prefs)
        {
            InitializeComponent();
            BackgroundColor = Color.FromRgba(150, 150, 150, 70);
            ShiftAlertLabel.Text = AlertContext;
            _prefs = prefs;
            Button CancelBtn = new Button{ Text = cancel, Style=ButtonStyle };

            CancelBtn.Clicked +=async (sender, args) => {
                isCheck(prefs);
                await PopupNavigation.Instance.PopAllAsync();
            };
            BtnLayout.Children.Add(CancelBtn);
        }

        public ShiftAlertPopupPage(	string AlertContext,
									string confirm,
									string cancel,
									string prefs)
        {
            InitializeComponent();    
            BackgroundColor = Color.FromRgba(150, 150, 150, 70);
            ShiftAlertLabel.Text = AlertContext;
            _prefs = prefs;
            Button CancelBtn = new Button{ Text = cancel , Style=ButtonStyle };
            Button ConfirmBtn = new Button{Text = confirm, Style=ButtonStyle };

            ConfirmBtn.Clicked += async (sender, args) =>
             {
                 isCheck(prefs);
                 MessagingCenter.Send(this, prefs, true);
                 await PopupNavigation.Instance.PopAsync();
             };
            CancelBtn.Clicked += async (sender, args) =>
            {
                isCheck(prefs);
                MessagingCenter.Send(this, prefs, false) ;
                await PopupNavigation.Instance.PopAsync();
            };
            BtnLayout.Children.Add(ConfirmBtn);
            BtnLayout.Children.Add(CancelBtn);
        }
        private void isCheck(string prefs)
        {            
            if (CheckNeverShow.IsChecked)
                Preferences.Set(prefs, true);
        }
        
    }
}