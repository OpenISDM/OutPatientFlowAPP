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

using System.Xml;
using IndoorNavigation.Modules.Utilities;
using Plugin.InputKit.Shared.Controls;
using IndoorNavigation.Views.Navigation;
using IndoorNavigation.Models;
using IndoorNavigation.Models.NavigaionLayer;

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
        //ExitPopupViewModel _viewmodel;
        CultureInfo currentLanguage = 
			CrossMultilingual.Current.CurrentCultureInfo;

        private XMLInformation _nameInformation;
        private PhoneInformation phoneInformation;


        public ExitPopupPage(string navigationGraphName)
        {
            InitializeComponent();
            BackgroundColor = Color.FromRgba(150, 150, 150, 70);
            _navigationGraphName = navigationGraphName;
            phoneInformation = new PhoneInformation();

            _nameInformation = NavigraphStorage.LoadInformationML
                (phoneInformation.GiveCurrentMapName(_navigationGraphName) +
                "_info_" +
                phoneInformation.GiveCurrentLanguage() +
                ".xml");

            LoadData();
            setRadioButton();

            //_viewmodel = new ExitPopupViewModel(_navigationGraphName);                        

            //BindingContext = _viewmodel;            
        }


        #region Load data from resource and set selection.

        private const string filename = "Yuanlin_OPFM.ExitMap.xml";
        private Dictionary<string ,DestinationItem> ExitItems;
        private void LoadData()
        {
            XmlDocument doc = NavigraphStorage.XmlReader(filename);
            ExitItems = new Dictionary<string, DestinationItem>();
            try
            {
                XmlNodeList exitNodes = doc.GetElementsByTagName("exit");
                foreach(XmlNode node in exitNodes)
                {
                    DestinationItem item = new DestinationItem();
                    item._regionID = new Guid(node.Attributes["region_id"].Value);
                    item._waypointID = new Guid(node.Attributes["waypoint_id"].Value);
                    item._waypointName = node.Attributes["name"].Value;
                    item._floor = node.Attributes["floor"].Value;
                    item.type = RecordType.Exit;
                    ExitItems.Add(item._waypointName, item);
                }
            }
            catch(Exception exc)
            {
                Console.WriteLine(exc.StackTrace);
            }
        }        

        private void setRadioButton()
        {
            foreach(KeyValuePair<string, DestinationItem> item in ExitItems)
            {
                RadioButton radioBtn = new RadioButton
                {
                    Text = item.Key,
                    TextFontSize = 28,
                    CircleColor = Color.FromRgb(63, 81, 181),
                    HorizontalOptions=LayoutOptions.StartAndExpand,
                    VerticalOptions=LayoutOptions.StartAndExpand
                };
                ExitRadioGroup.Children.Add(radioBtn);
            }
        }

        #endregion

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

        async private void ExitPopup_Clicked(object sender, EventArgs e)
        {
            foreach(RadioButton radio in ExitRadioGroup.Children)
            {
                if (radio.IsChecked)
                {
                    string radioName = radio.Text;
                    app.records.Insert(app.FinishCount, new RgRecord
                    {
                        _waypointName = radioName,
                        type = RecordType.Exit,
                        _regionID = ExitItems[radioName]._regionID,
                        _waypointID = ExitItems[radioName]._waypointID,
                        _floor = ExitItems[radioName]._floor,
                        isComplete = true,
                        DptName = radioName
                    });
                    await Navigation.PushAsync(new NavigatorPage(_navigationGraphName, ExitItems[radioName]._regionID, ExitItems[radioName]._waypointID, radioName, _nameInformation)); ;
                    await PopupNavigation.Instance.PopAsync();
                    return;
                }
            }

            await DisplayAlert(_resourceManager.GetString("MESSAGE_STRING",
                                                currentLanguage),
                     _resourceManager.GetString("SELECT_EXIT_STRING",
                                                currentLanguage),
                     _resourceManager.GetString("OK_STRING",
                                                currentLanguage));
        }
    }
}