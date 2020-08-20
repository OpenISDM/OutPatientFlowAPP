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
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Rg.Plugins.Popup.Pages;
using System.Resources;
using IndoorNavigation.Resources.Helpers;
using System.Reflection;
using Rg.Plugins.Popup.Services;
using Plugin.Multilingual;
using System.Globalization;
using System.Xml;
using IndoorNavigation.Views.Navigation;
using IndoorNavigation.Models.NavigaionLayer;
using static IndoorNavigation.Utilities.Storage;
using IndoorNavigation.Utilities;
using RadioButton = Plugin.InputKit.Shared.Controls.RadioButton;

namespace IndoorNavigation.Views.PopUpPage
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ExitPopupPage : PopupPage
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

        public ExitPopupPage(string navigationGraphName)
        {
            InitializeComponent();
            _navigationGraphName = navigationGraphName;
            _nameInformation = LoadXmlInformation(_navigationGraphName);         
            LoadData();
            setRadioButton();
        }


        #region Load data from resource and set selection.

        private const string filename = "Yuanlin_OPFM.ExitMap.xml";
        private Dictionary<string, DestinationItem> ExitItems;
        private void LoadData()
        {
            XmlDocument doc = Storage.XmlReader(filename);
            ExitItems = new Dictionary<string, DestinationItem>();
            try
            {
                XmlNodeList exitNodes = doc.GetElementsByTagName("exit");
                foreach (XmlNode node in exitNodes)
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
            catch (Exception exc)
            {
                Console.WriteLine(exc.StackTrace);
            }
        }

        private void setRadioButton()
        {
            foreach (KeyValuePair<string, DestinationItem> item in ExitItems)
            {
                RadioButton radioBtn = new RadioButton
                {
                    Text = item.Key,
                    TextFontSize = 28,
                    CircleColor = Color.FromRgb(63, 81, 181),
                    HorizontalOptions = LayoutOptions.StartAndExpand,
                    VerticalOptions = LayoutOptions.StartAndExpand
                };
                ExitRadioGroup.Children.Add(radioBtn);
            }
        }

        #endregion

        protected override bool OnBackButtonPressed()
        {
            DisplayAlert(_resourceManager
                            .GetString("MESSAGE_STRING", currentLanguage),
                         _resourceManager
                            .GetString("SELECT_EXIT_STRING", currentLanguage),
                         _resourceManager
                            .GetString("OK_STRING", currentLanguage));

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
            foreach (RadioButton radio in ExitRadioGroup.Children)
            {
                if (radio.IsChecked)
                {
                    string radioName = radio.Text;
                    int order =
                       app.OrderDistrict.ContainsKey(0) ?
                       app.OrderDistrict[0] : 0;
                    //app.records.Insert(app.FinishCount, new RgRecord
                    app.records.Add(new RgRecord
                    {
                        _waypointName = radioName,
                        type = RecordType.Exit,
                        _regionID = ExitItems[radioName]._regionID,
                        _waypointID = ExitItems[radioName]._waypointID,
                        _floor = ExitItems[radioName]._floor,
                        isComplete = true,
                        DptName = radioName,
                        order=order
                    });
                    app._globalNavigatorPage = 
                        new NavigatorPage(_navigationGraphName, 
                        ExitItems[radioName]._regionID, 
                        ExitItems[radioName]._waypointID, 
                        radioName, 
                        _nameInformation);
                    await Navigation.PushAsync(app._globalNavigatorPage);
                    //await Navigation.PushAsync(new NavigatorPage(_navigationGraphName, ExitItems[radioName]._regionID, ExitItems[radioName]._waypointID, radioName, _nameInformation)); ;
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