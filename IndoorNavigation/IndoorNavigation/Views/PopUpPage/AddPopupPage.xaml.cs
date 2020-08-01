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
 *      AddPopupPage.cs
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
using System.Reflection;
using System.Resources;
using IndoorNavigation.Resources.Helpers;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Xml;
using Plugin.Multilingual;
using System.Globalization;
using IndoorNavigation.Modules.Utilities;
using System.IO;
using Plugin.InputKit.Shared.Controls;
using CheckBox = Plugin.InputKit.Shared.Controls.CheckBox;
using IndoorNavigation.Utilities;
using IndoorNavigation.Models;
using System.Collections.ObjectModel;
using ZXing;

namespace IndoorNavigation.Views.PopUpPage
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddPopupPage : PopupPage
    {
        App app = (App)Application.Current;
        const string _resourceId = "IndoorNavigation.Resources.AppResources";
        ResourceManager _resourceManager =
            new ResourceManager(_resourceId,
                typeof(TranslateExtension).GetTypeInfo().Assembly);

        CultureInfo currentLanguage =
            CrossMultilingual.Current.CurrentCultureInfo;

        bool isButtonPressed = false;
        Dictionary<string, List<CheckBox>> BoxesDict;
        Dictionary<string, List<AddExaminationItem>> _examinationItemDict;
        //Dictionary<string, List<AddExaminationItem>> _clinicItemDict;
        List<string> DepartmentList;
        List<string> ClinicList;
        public AddPopupPage(string graphName)
        {
            InitializeComponent();
            BoxesDict =
                new Dictionary<string, List<CheckBox>>();

            DepartmentList =
                new List<string>();
            ClinicList =
                new List<string>();

            _examinationItemDict =
                new Dictionary<string, List<AddExaminationItem>>();
            //_clinicItemDict = 
            //    new Dictionary<string, List<AddExaminationItem>>();

            LoadData();
        }

        private void LoadData()
        {
            XmlDocument doc =
                Storage.XmlReader("Yuanlin_OPFM.ExaminationRoomMap.xml");

            #region Clinics part
            XmlNodeList ClinicNodeList = 
                doc.SelectNodes("navigation_graph/Clinics/Department");

            foreach (XmlNode clinicfloorNode in ClinicNodeList)
            {
                XmlNodeList clinicsNodeList = clinicfloorNode.ChildNodes;

                string clinicFloorName = clinicfloorNode.Attributes["name"].Value;
                string clinicFloor = clinicfloorNode.Attributes["floor"].Value;

                ClinicList.Add(clinicFloorName);
                List<AddExaminationItem> items = new List<AddExaminationItem>();

                foreach (XmlNode room in clinicsNodeList)
                {
                    Console.WriteLine("displayname = " + room.Attributes["displayname"].Value);
                    AddExaminationItem item = new AddExaminationItem();

                    item.DisplayName = room.Attributes["displayname"].Value;

                    item._regionID = 
                        new Guid(room.Attributes["region_id"].Value);

                    item._waypointID = 
                        new Guid(room.Attributes["waypoint_id"].Value);

                    item._floor = clinicFloor;
                    item.type = RecordType.AddItem;
                    item._waypointName = room.Attributes["name"].Value;

                    items.Add(item);
                }
                //ClinicList.Add(clinicFloorName);
                _examinationItemDict.Add(clinicFloorName, items);
            }
            #endregion
            #region Examination Part
            XmlNodeList ExaminationDepartmentLists =
                doc.SelectNodes("navigation_graph/Department");

            foreach (XmlNode department in ExaminationDepartmentLists)
            {                
                XmlNodeList examinationRooms = department.ChildNodes;
                string departName = department.Attributes["name"].Value;
                Console.WriteLine("DepartName = " + departName);
                string departmentFloor = department.Attributes["floor"].Value;
                DepartmentList.Add(departName);

                List<AddExaminationItem> items = new List<AddExaminationItem>();
                foreach (XmlNode room in examinationRooms)
                {
                    AddExaminationItem item = new AddExaminationItem();
                    
                    item.DisplayName = room.Attributes["displayname"].Value;

                    item._regionID =
                        new Guid(room.Attributes["region_id"].Value);

                    item._waypointID =
                        new Guid(room.Attributes["waypoint_id"].Value);

                    item._floor = departmentFloor;
                    item.type = RecordType.AddItem;
                    item._waypointName = room.Attributes["name"].Value;

                    items.Add(item);
                }
                _examinationItemDict.Add(departName, items);
            }
            #endregion
            GenerateBox();
            Console.WriteLine("<<LoadData");
        }

        List<CheckBox> processBoxes = new List<CheckBox>();

        //for generate selection with input data, and make layout.
        private void GenerateBox()
        {
            Console.WriteLine(">>GenerateBox");
            List<CheckBox> boxList;
            StackLayout BoxLayout;
            Label DptNameLabel;
            Grid outSideGrid;
            Image image;
            int key = 0;
            int PictureCount = 1;
            mainStackLayout.Children.Add(new BoxView
            {
                BackgroundColor = Color.FromHex("#3f51b5"),
                HeightRequest = 1
            });

            #region part of Suit Process Checkbox
            HospitalProcessParse processParse = new HospitalProcessParse();

            StackLayout processStackLayout = new StackLayout();
            outSideGrid = getGridLayout();
            List<ProcessOption> processOptions =
                processParse.GetProcessOption();
            image = new Image
            {
                Source = "ProcessExamination.png",
                Aspect = Aspect.AspectFit,
                WidthRequest = 80,
                HeightRequest = 80,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };
            DptNameLabel = new Label
            {
                Text = 
                _resourceManager.GetString("COMBO_PROCESS_STRING", 
                currentLanguage),
                FontSize =
                        Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                VerticalTextAlignment =
                        TextAlignment.Center,
                HorizontalTextAlignment =
                        TextAlignment.Center
            };
            foreach (ProcessOption option in processOptions)
            {
                Console.WriteLine("Generate one time process box");
                CheckBox optionBox = new CheckBox
                {
                    Key = int.Parse(option.processID),
                    Text = option.processName,
                    TextFontSize =
                     Device.GetNamedSize(NamedSize.Large, typeof(CheckBox)),
                    Margin = new Thickness(0, -3),
                    Type = CheckBox.CheckType.Check
                };
                processStackLayout.Children.Add(optionBox);
                processBoxes.Add(optionBox);
            }
            ScrollView processScrollView = 
                new ScrollView { 
                    Content = processStackLayout, 
                    Orientation= ScrollOrientation.Horizontal,
                    HorizontalScrollBarVisibility= ScrollBarVisibility.Always
                };
            outSideGrid.Children.Add(image, 0, 2, 0, 4);
            outSideGrid.Children.Add(DptNameLabel, 0, 2, 4, 5);
            outSideGrid.Children.Add(processScrollView, 2, 5, 0, 5);
            mainStackLayout.Children.Add(outSideGrid);

            //blue line for dividing every depart
            mainStackLayout.Children.Add(new BoxView
            {
                BackgroundColor = Color.FromHex("#3f51b5"),
                HeightRequest = 1
            });
            #endregion

            #region part of Clinic checkbox 
            foreach(string clinicfloorName in ClinicList)
            {
                outSideGrid = getGridLayout();
                string Floor = clinicfloorName.Substring(0, 2);
                Console.WriteLine("Floor : " + Floor);

                image = new Image
                {
                    Source = $"Clinic_{Floor}.png",
                    Aspect = Aspect.AspectFit,
                    WidthRequest = 80,
                    HeightRequest = 80,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center
                };

                DptNameLabel = new Label
                {
                    Text = clinicfloorName,
                    FontSize =
                        Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                    VerticalTextAlignment =
                        TextAlignment.Center,
                    HorizontalTextAlignment =
                        TextAlignment.Center
                };                
                boxList = new List<CheckBox>();
                key = 0;

                StackLayout CheckboxStackLayout = 
                   new StackLayout {Orientation = StackOrientation.Horizontal };
                StackLayout CheckboxInsideStackLayout = new StackLayout
                {
                    Padding = new Thickness(0, 0, 15, 0)
                };
                int CheckboxCount = 0;
                foreach(AddExaminationItem item in _examinationItemDict[clinicfloorName])
                {
                    CheckBox box = new CheckBox
                    {
                        Text = item.DisplayName,
                        TextFontSize =
                         Device.GetNamedSize(NamedSize.Large, typeof(CheckBox)),
                        Margin = new Thickness(0, -3),
                        Key = key++,
                        Type = CheckBox.CheckType.Check
                    };

                    CheckboxInsideStackLayout.Children.Add(box);
                    CheckboxCount++;

                    if(CheckboxCount==3)
                    {
                        CheckboxStackLayout.Children.Add(CheckboxInsideStackLayout);
                        CheckboxInsideStackLayout = new StackLayout 
                        {
                            Padding = new Thickness(0 ,0 ,15 ,0)
                        };
                        CheckboxCount = 0;
                    }
                    boxList.Add(box);
                }

                if (CheckboxCount != 0)
                    CheckboxStackLayout.Children.Add(CheckboxInsideStackLayout);

                ScrollView clinicScrollView = new ScrollView
                {
                    Content = CheckboxStackLayout,
                    Orientation= ScrollOrientation.Horizontal,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Always
                };
                outSideGrid.Children.Add(image, 0, 2, 0, 4);
                outSideGrid.Children.Add(DptNameLabel, 0, 2, 4, 5);
                outSideGrid.Children.Add(clinicScrollView, 2, 5, 0, 5);
                BoxesDict.Add(clinicfloorName, boxList);
                mainStackLayout.Children.Add(outSideGrid);

                mainStackLayout.Children.Add(new BoxView
                {
                    BackgroundColor = Color.FromHex("#3f51b5"),
                    HeightRequest = 1
                });
            }

            #endregion            

            #region part of Examination Room
            foreach (string dptName in DepartmentList)
            {
                outSideGrid = getGridLayout();
                image = new Image
                {
                    Source = $"Add_Item{PictureCount++}",
                    Aspect = Aspect.AspectFit,
                    WidthRequest = 80,
                    HeightRequest = 80,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center
                };
                DptNameLabel = new Label
                {
                    Text = dptName,
                    FontSize =
                        Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                    VerticalTextAlignment =
                        TextAlignment.Center,
                    HorizontalTextAlignment =
                        TextAlignment.Center
                };

                BoxLayout = new StackLayout();
                boxList = new List<CheckBox>();
                key = 0;
                foreach (AddExaminationItem item in _examinationItemDict[dptName])
                {
                    CheckBox box = new CheckBox
                    {
                        Text = item.DisplayName,
                        TextFontSize =
                          Device.GetNamedSize(NamedSize.Large, typeof(CheckBox)),
                        Margin = new Thickness(0, -3),
                        Key = key++,
                        Type = CheckBox.CheckType.Check
                    };
                    BoxLayout.Children.Add(box);
                    boxList.Add(box);
                }

                outSideGrid.Children.Add(image, 0, 2, 0, 4);
                outSideGrid.Children.Add(DptNameLabel, 0, 2, 4, 5);
                outSideGrid.Children.Add(BoxLayout, 2, 5, 0, 5);

                BoxesDict.Add(dptName, boxList);
                mainStackLayout.Children.Add(outSideGrid);

                //blue line for dividing every depart
                mainStackLayout.Children.Add(new BoxView
                {
                    BackgroundColor = Color.FromHex("#3f51b5"),
                    HeightRequest = 1
                });
            }
            #endregion

            #region part of Revisit checkbox
            //boxList = new List<CheckBox>();
            //outSideGrid = getGridLayout();
            //BoxLayout = new StackLayout();

            //key = 0;
            //DptNameLabel = new Label
            //{
            //    Text =
            //        _resourceManager.GetString("REVISIT_STRING", currentLanguage),
            //    FontSize =
            //        Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
            //    VerticalTextAlignment =
            //        TextAlignment.Center,

            //    HorizontalTextAlignment =
            //        TextAlignment.Center,
            //};

            //image = new Image
            //{
            //    Source = $"Add_Item{PictureCount++}",
            //    Aspect = Aspect.AspectFit,
            //    WidthRequest = 80,
            //    HeightRequest = 80,
            //    VerticalOptions = LayoutOptions.Center,
            //    HorizontalOptions = LayoutOptions.Center
            //};
            //foreach (RgRecord record in app._TmpRecords)
            //{
            //    CheckBox box = new CheckBox
            //    {
            //        Text = record.DptName,
            //        TextFontSize =
            //            Device.GetNamedSize(NamedSize.Large, typeof(CheckBox)),
            //        Margin = new Thickness(0, -3),
            //        Key = key++,
            //        Type = CheckBox.CheckType.Check
            //    };
            //    boxList.Add(box);
            //    BoxLayout.Children.Add(box);
            //}
            //outSideGrid.Children.Add(image, 0, 2, 0, 4);
            //outSideGrid.Children.Add(DptNameLabel, 0, 2, 4, 5);
            //outSideGrid.Children.Add(
            //    new ScrollView { Content = BoxLayout }, 2, 5, 0, 5
            //);
            //BoxesDict.Add("revisit", boxList);
            //mainStackLayout.Children.Add(outSideGrid);

            //mainStackLayout.Children.Add(new BoxView
            //{
            //    BackgroundColor = Color.FromHex("#3f51b5"),
            //    HeightRequest = 1
            //});
            #endregion
            Console.WriteLine("<<GenerateBox");          
        }

        //to generate Grid layout for CheckBox 
        private Grid getGridLayout()
        {
            Grid tmp = new Grid();
            for (int i = 0; i < 4; i++)
            {
                tmp.RowDefinitions.Add(new RowDefinition
                { Height = new GridLength(1, GridUnitType.Star) });
                tmp.ColumnDefinitions.Add(new ColumnDefinition
                { Width = new GridLength(1, GridUnitType.Star) });
            }
            return tmp;
        }

        private void AddCancelButton_Clicked(object sender, EventArgs e)
        {
            if (isButtonPressed) return;
            isButtonPressed = true;

            PopAddPage(true);
        }

        async private void AddConfirmButton_Clicked(object sender, EventArgs e)
        {

            int count = 0;
            int index =
                (app.roundRecord == null ||
                !app.lastFinished.type.Equals(RecordType.Queryresult)) ?
                (app.records.Count) :
                (app.records.IndexOf(app.roundRecord) + 1);
            int dumplicateCount = 0;
            #region Part of Process combo
            foreach (CheckBox optionBox in processBoxes)
            {
                Console.WriteLine(">>CheckBox optionBox in processBoxes");
                if (optionBox.IsChecked)
                {
                    Console.WriteLine("Checked process Name : " + optionBox.Text);
                    Console.WriteLine("Checked process Key : " + optionBox.Key);

                    if (((App)Application.Current).OrderDistrict.ContainsKey(optionBox.Key))
                    {
                        Console.WriteLine("This item is dumplicate.");

                        await PopupNavigation.Instance.PushAsync(new
                            AlertDialogPopupPage(
                            string.Format(_resourceManager.GetString
                            ("SELECT_DUMPLICATE_CONTENT_STRING", currentLanguage), optionBox.Text),
                            _resourceManager.GetString
                            ("OK_STRING", currentLanguage))
                       );
                        return;
                    }

                    HospitalProcessParse parse = new HospitalProcessParse();
                    ObservableCollection<RgRecord> SuitProcess =
                        parse.ParseProcess(new ProcessOption
                        {
                            processID = optionBox.Key.ToString(),
                            processName = optionBox.Text
                        });
                    foreach (RgRecord record in SuitProcess)
                    {
                        ((App)Application.Current).records.Add(record);
                    }
                    ((App)Application.Current).OrderDistrict
                        .Add(optionBox.Key, 0);
                    count++;
                }
            }
            #endregion
            #region Part of Clinic 
            foreach (string floorClinic in ClinicList)
            {
                List<AddExaminationItem> items = _examinationItemDict[floorClinic];

                List<CheckBox> Boxes = BoxesDict[floorClinic];

                foreach(CheckBox box in Boxes)
                {
                    if (!box.IsChecked) continue;
                    dumplicateCount++;
                    bool isDuplicate =
                        app.records.Any(p =>
                        p._waypointName == items[box.Key]._waypointName &&
                        p.DptName == items[box.Key].DisplayName &&
                        p.isAccept == false
                        );
                    if (isDuplicate) continue;

                    int order = 1;

                    app.records.Add(new RgRecord
                    {
                        order = order,
                        _waypointID = items[box.Key]._waypointID,
                        _regionID = items[box.Key]._regionID,
                        type = RecordType.AddItem,
                        _groupID = 0,
                        _waypointName = items[box.Key]._waypointName,
                        DptName = items[box.Key].DisplayName,
                        _subtitleName = items[box.Key]._waypointName
                    }) ;
                    count++;
                }
            }
            #endregion
            #region Part of Examination
            foreach (string dptName in DepartmentList)
            {
                List<AddExaminationItem> items = _examinationItemDict[dptName];
                List<CheckBox> Boxes = BoxesDict[dptName];

                foreach (CheckBox box in Boxes)
                {
                    if (!box.IsChecked) continue;
                    dumplicateCount++;
                    var isDuplicate =
                        app.records.Any(p =>
                        (p.DptName == dptName && 
                         p._waypointName == items[box.Key].DisplayName &&
                         p.isAccept == false));

                    if (isDuplicate)
                        continue;

                    int order =
                        app.OrderDistrict.ContainsKey(0) ?
                        app.OrderDistrict[0] : 1;

                    //app.records.Insert(index++, new RgRecord
                    app.records.Add(new RgRecord
                    {
                        _waypointID = items[box.Key]._waypointID,
                        _regionID = items[box.Key]._regionID,
                        _waypointName = items[box.Key]._waypointName,
                        _subtitleName = items[box.Key].DisplayName,
                        type = RecordType.AddItem,
                        DptName = dptName,
                        _groupID = 0,
                        order = order ++
                    });
                    count++;
                }
            }
            #endregion
            #region  part of revisit 
            //foreach (CheckBox box in BoxesDict["revisit"])
            //{

            //    if (box.IsChecked == false) continue;
            //    var isDumplicate =
            //        app.records.Any(a => a.DptName ==
            //        (_resourceManager.GetString("REVISIT_STRING",
            //                                    currentLanguage)
            //         + "-" + app._TmpRecords[box.Key].DptName) && !a.isAccept);

            //    dumplicateCount++;
            //    if (isDumplicate) continue;
            //    app.records.Insert(index++, new RgRecord
            //    {
            //        type = RecordType.AddItem,
            //        _waypointID = app._TmpRecords[box.Key]._waypointID,
            //        _regionID = app._TmpRecords[box.Key]._regionID,
            //        _waypointName = app._TmpRecords[box.Key]._waypointName,
            //        CareRoom = app._TmpRecords[box.Key].CareRoom,
            //        DptName =
            //            _resourceManager.GetString("REVISIT_STRING",
            //                                       currentLanguage)
            //            + "-" + app._TmpRecords[box.Key].DptName
            //    });
            //    count++;
            //}
            #endregion
           

            if (count == 0)
            {
                Console.WriteLine("Count = 0");
                if (dumplicateCount == 0)
                    await PopupNavigation.Instance.PushAsync
                        (new AlertDialogPopupPage(_resourceManager.GetString
                            ("NO_SELECT_DESTINATION_STRING", currentLanguage),
                        _resourceManager.GetString("OK_STRING", currentLanguage)));
                else if (dumplicateCount >= 1)
                    await PopupNavigation.Instance.PushAsync
                        (new AlertDialogPopupPage(_resourceManager.GetString
                        ("SELECT_DUPLICATE_EXAM_STRING", currentLanguage),
                       _resourceManager.GetString("OK_STRING", currentLanguage)));
                return;
            }

            PopAddPage(false);
        }

        private class AddExaminationItem : DestinationItem
        {
            public string DisplayName;
            public override string ToString() => DisplayName;

        };


        protected override bool OnBackButtonPressed()
        {
            PopAddPage(false);
            return base.OnBackButtonPressed();
        }
        protected override bool OnBackgroundClicked()
        {
            PopAddPage(false);
            return base.OnBackgroundClicked();
        }

        private void PopAddPage(bool isCancel)
        {
            MessagingCenter.Send(this, "isCancel", isCancel);
            PopupNavigation.Instance.PopAsync();
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            OnBackgroundClicked();
        }
    }
}