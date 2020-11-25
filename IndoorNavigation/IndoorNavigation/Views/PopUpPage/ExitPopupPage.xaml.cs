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
using System.Linq;
using IndoorNavigation.Resources;
using IndoorNavigation.Models;

namespace IndoorNavigation.Views.PopUpPage
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ExitPopupPage : PopupPage
    {
        App app = (App)Application.Current;
        string _navigationGraphName;
        private XMLInformation _nameInformation;
        private Dictionary<Guid, DestinationItem> ElevatorPosition;

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
        private Guid allFFFFGuid =
            new Guid("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF");
        
        private void LoadElevator()
        {
            ElevatorPosition = new Dictionary<Guid, DestinationItem>();
            XmlDocument doc = XmlReader("Yuanlin_OPFM.ElevatorsMap.xml");
            XmlNodeList ElevatorNodeList = doc.GetElementsByTagName("elevator");

            foreach (XmlNode elevatorNode in ElevatorNodeList)
            {
                DestinationItem item = new DestinationItem();

                item._regionID =
                    new Guid(elevatorNode.Attributes["region_id"].Value);
                item._waypointID =
                    new Guid(elevatorNode.Attributes["waypoint_id"].Value);

                item._floor = elevatorNode.Attributes["floor"].Value;
                item._waypointName = elevatorNode.Attributes["name"].Value;

                ElevatorPosition.Add(item._regionID, item);
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
            DisplayAlert(
                GetResourceString("MESSAGE_STRING"),
                GetResourceString("SELECT_EXIT_STRING"),
                GetResourceString("OK_STRING"));

            return true;
            // Return true if you don't want to close this popup page when a 
            // back button is pressed
        }

        protected override bool OnBackgroundClicked()
        {
            DisplayAlert(
                GetResourceString("MESSAGE_STRING"), 
                GetResourceString("SELECT_EXIT_STRING"), 
                GetResourceString("OK_STRING"));
            
            return false;
            // Return false if you don't want to close this popup page when a 
            // background of the popup page is clicked
        }

        async private void ExitCancelBtn_Clicked(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.RemovePageAsync(this);
            MessagingCenter.Send(this, "ExitCancel", false);

        }

        async private void ExitConfirmBtn_Clicked(object sender, EventArgs e)
        {
            foreach (RadioButton radio in ExitRadioGroup.Children)
            {
                if (radio.IsChecked)
                {
                    string radioName = radio.Text;
                    int order =
                       app.OrderDistrict.ContainsKey(0) ?
                       app.OrderDistrict[0] : 0;
                    DestinationItem item = ExitItems[radioName];

                    if (item._regionID == allFFFFGuid &&
                        item._waypointID == allFFFFGuid)
                    {
                        LoadElevator();
                        Console.WriteLine("this range of item haven't been supported now.");

                        await PopupNavigation.Instance.PushAsync
                            (new AlertDialogPopupPage(
                                GetResourceString(
                                    "WILL_BRING_YOU_TO_ELEVATOR_STRING"
                                    ),
                                GetResourceString("OK_STRING")));
                        DestinationItem elevatorItem;

                        try
                        {
                            elevatorItem = 
                                ElevatorPosition[app.lastFinished._regionID];
                        }
                        catch
                        {
                            elevatorItem = ElevatorPosition.First().Value;
                        }

                        app.records.Add(new RgRecord
                        {
                            _waypointName = elevatorItem._waypointName,
                            _regionID = elevatorItem._regionID,
                            _waypointID = elevatorItem._waypointID,
                            isComplete = true,
                            DptName = radioName,
                            _groupID = 0,
                            type = RecordType.Exit
                        });

                        app._globalNavigatorPage = new NavigatorPage
                            (_navigationGraphName,
                             elevatorItem._regionID,
                             elevatorItem._waypointID,
                             elevatorItem._waypointName,
                             _nameInformation);
                        Page page = Application.Current.MainPage;
                        await PopupNavigation.Instance.RemovePageAsync(this);
                        await page.Navigation.PushAsync
                            (app._globalNavigatorPage);
                        return;
                    }

                    app.records.Add(new RgRecord
                    {
                        _waypointName = radioName,
                        type = RecordType.Exit,
                        _regionID = item._regionID,
                        _waypointID = item._waypointID,
                        _floor = item._floor,
                        isComplete = true,
                        DptName = radioName,
                        order = order
                    });
                    app._globalNavigatorPage =
                        new NavigatorPage(_navigationGraphName,
                        item._regionID,
                        item._waypointID,
                        radioName,
                        _nameInformation);
                    await Navigation.PushAsync(app._globalNavigatorPage);
                    await PopupNavigation.Instance.RemovePageAsync(this);
                    return;
                }
            }
            await DisplayAlert(
                GetResourceString("MESSAGE_STRING"), 
                GetResourceString("SELECT_EXIT_STRING"), 
                GetResourceString("OK_STRING")
                );

            MessagingCenter.Send(this, "ExitCancel", true);
        }
    }
}