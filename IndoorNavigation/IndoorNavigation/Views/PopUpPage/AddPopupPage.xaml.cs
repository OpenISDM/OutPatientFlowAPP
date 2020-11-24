/*
 * 2020 © Copyright (c) BiDaE Technology Inc. 
 * Provided under BiDaE SHAREWARE LICENSE-1.0 in the LICENSE.
 *
 * Project Name:
 *
 *      IndoorNavigation
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
 *      Jason Chang, jasonchang@bidae.tech 
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
using CheckBox = Plugin.InputKit.Shared.Controls.CheckBox;
using IndoorNavigation.Utilities;
using IndoorNavigation.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

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

        Dictionary<string, List<AddExaminationItem>> _examinationItemDict;
        List<string> DepartmentList;
        List<string> ClinicList;
        List<string> OtherItemList;
        Dictionary<SearchBarKey, Grid> _allContentInAdd =
            new Dictionary<SearchBarKey, Grid>();
        public AddPopupPage(string graphName)
        {
            InitializeComponent();
            DepartmentList =
                new List<string>();
            ClinicList =
                new List<string>();

            OtherItemList =
                new List<string>();

            _examinationItemDict =
                new Dictionary<string, List<AddExaminationItem>>();

            LoadData();
        }

        async private void LoadData()
        {
            XmlDocument doc =
                Storage.XmlReader("Yuanlin_OPFM.ExaminationRoomMap.xml");
            #region Other Item part
            XmlNodeList OtherItemNodeList =
                doc.SelectNodes("navigation_graph/OtherDestination/Department");

            foreach (XmlNode OtherItemNode in OtherItemNodeList)
            {
                XmlNodeList otherItemNodeList = OtherItemNode.ChildNodes;

                string ItemTypeName = OtherItemNode.Attributes["name"].Value;
                RecordType type =
                    (RecordType)Enum.Parse(typeof(RecordType),
                    OtherItemNode.Attributes["type"].Value);

                List<AddExaminationItem> items =
                    new List<AddExaminationItem>();

                foreach (XmlNode room in otherItemNodeList)
                {
                    AddExaminationItem item = new AddExaminationItem();

                    item.DisplayName = room.Attributes["name"].Value;
                    item._regionID =
                        new Guid(room.Attributes["region_id"].Value);
                    item._waypointID =
                        new Guid(room.Attributes["waypoint_id"].Value);
                    item.type = type;
                    item._waypointName = room.Attributes["name"].Value;

                    items.Add(item);
                }

                OtherItemList.Add(ItemTypeName);

                _examinationItemDict.Add(ItemTypeName, items);
            }

            #endregion
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
            await Task.CompletedTask;
        }

        List<CheckBox> processBoxes = new List<CheckBox>();

        //for generate selection with input data, and make layout.
        async private void GenerateBox()
        {
            Console.WriteLine(">>GenerateBox");
            StackLayout BoxLayout;
            Label DptNameLabel;
            String ImageSourcePath;
            Grid outSideGrid;
            Image image;
            int key = 0;
            int PictureCount = 1;
            List<string> clinicNames = new List<string>();
            SearchBarKey keys;

            #region parot of Other Items           

            foreach (string otherItemName in OtherItemList)
            {
                BoxLayout = new StackLayout();
                outSideGrid = getGridLayout();
                clinicNames = new List<string>();
                key = 0;
                image = new Image
                {
                    Source = _examinationItemDict[otherItemName].First()
                    .type.ToString() + "_OPPA",
                    Aspect = Aspect.AspectFit,
                    WidthRequest = 80,
                    HeightRequest = 80,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center
                };

                DptNameLabel = new Label
                {
                    Text = otherItemName,
                    FontSize =
                    Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                    VerticalTextAlignment = TextAlignment.Center,
                    HorizontalTextAlignment = TextAlignment.Center
                };
                foreach (AddExaminationItem item in
                    _examinationItemDict[otherItemName])
                {
                    CheckBox box = new CheckBox
                    {
                        Text = item.DisplayName,
                        TextFontSize =
                        Device.GetNamedSize(NamedSize.Large, typeof(CheckBox)),
                        Margin = new Thickness(0, -3),
                        Type = CheckBox.CheckType.Check,
                        Key = key++
                    };
                    BoxLayout.Children.Add(box);
                    item._checkbox = box;
                    clinicNames.Add(item.DisplayName);
                }

                keys = new SearchBarKey
                {
                    _departNameLabelText = otherItemName,
                    _clinicNameText = clinicNames
                };
                outSideGrid.Children.Add(image, 0, 2, 0, 4);
                outSideGrid.Children.Add(DptNameLabel, 0, 2, 4, 5);
                outSideGrid.Children.Add(BoxLayout, 2, 5, 0, 5);

                mainStackLayout.Children.Add(outSideGrid);
                mainStackLayout.Children.Add(new BoxView
                {
                    HeightRequest = 1,
                    Color = Color.FromHex("#3f51b5")
                });

                _allContentInAdd.Add(keys, outSideGrid);
            }
            #endregion

            #region part of Suit Process Checkbox
            HospitalProcessParse processParse = new HospitalProcessParse();
            clinicNames = new List<string>();
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
                clinicNames.Add(option.processName);
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
                new ScrollView
                {
                    Content = processStackLayout,
                    Orientation = ScrollOrientation.Horizontal,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Always
                };

            keys = new SearchBarKey
            {
                _clinicNameText = clinicNames,
                _departNameLabelText = DptNameLabel.Text
            };
            outSideGrid.Children.Add(image, 0, 2, 0, 4);
            outSideGrid.Children.Add(DptNameLabel, 0, 2, 4, 5);
            outSideGrid.Children.Add(processScrollView, 2, 5, 0, 5);
            _allContentInAdd.Add(keys, outSideGrid);
            mainStackLayout.Children.Add(outSideGrid);


            mainStackLayout.Children.Add(new BoxView
            {
                HeightRequest = 1,
                Color = Color.FromHex("#3f51b5")
            });

            #endregion

            #region part of Clinic checkbox 
            foreach (string clinicfloorName in ClinicList)
            {
                outSideGrid = getGridLayout();
                string Floor = clinicfloorName.Substring(0, 2);
                clinicNames = new List<string>();

                Console.WriteLine("Floor : " + Floor);
                ImageSourcePath = $"Clinic_{Floor}.png";

                image = new Image
                {
                    Source = ImageSourcePath,
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

                key = 0;
                StackLayout CheckboxStackLayout =
                   new StackLayout
                   { Orientation = StackOrientation.Horizontal };
                BoxLayout = new StackLayout
                {
                    Padding = new Thickness(0, 0, 15, 0)
                };
                int CheckboxCount = 0;
                foreach (AddExaminationItem item in
                    _examinationItemDict[clinicfloorName])
                {
                    CheckBox box = new CheckBox
                    {
                        Text = item.DisplayName,
                        TextFontSize =
                         Device.GetNamedSize(NamedSize.Large,
                         typeof(CheckBox)),
                        Margin = new Thickness(0, -3),
                        Key = key++,
                        Type = CheckBox.CheckType.Check
                    };
                    item._checkbox = box;
                    clinicNames.Add(item.DisplayName);
                    BoxLayout.Children.Add(box);
                    CheckboxCount++;

                    if (CheckboxCount == 3)
                    {
                        CheckboxStackLayout.Children.Add(BoxLayout);
                        BoxLayout = new StackLayout
                        {
                            Padding = new Thickness(0, 0, 15, 0)
                        };
                        CheckboxCount = 0;
                    }
                }

                if (CheckboxCount != 0)
                    CheckboxStackLayout.Children.Add(BoxLayout);

                ScrollView clinicScrollView = new ScrollView
                {
                    Content = CheckboxStackLayout,
                    Orientation = ScrollOrientation.Horizontal,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Always
                };
                keys = new SearchBarKey
                {
                    _clinicNameText = clinicNames,
                    _departNameLabelText = DptNameLabel.Text
                };
                outSideGrid.Children.Add(image, 0, 2, 0, 4);
                outSideGrid.Children.Add(DptNameLabel, 0, 2, 4, 5);
                outSideGrid.Children.Add(clinicScrollView, 2, 5, 0, 5);
                _allContentInAdd.Add(keys, outSideGrid);
                mainStackLayout.Children.Add(outSideGrid);

                mainStackLayout.Children.Add(new BoxView
                {
                    HeightRequest = 1,
                    Color = Color.FromHex("#3f51b5")
                });
            }

            #endregion            

            #region part of Examination Room
            foreach (string dptName in DepartmentList)
            {
                clinicNames = new List<string>();
                outSideGrid = getGridLayout();
                ImageSourcePath = $"Add_Item{PictureCount++}";
                image = new Image
                {
                    Source = ImageSourcePath,
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
                key = 0;
                foreach (AddExaminationItem item in
                    _examinationItemDict[dptName])
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
                    clinicNames.Add(item.DisplayName);
                    item._checkbox = box;
                    BoxLayout.Children.Add(box);
                }
                keys = new SearchBarKey
                {
                    _departNameLabelText = DptNameLabel.Text,
                    _clinicNameText = clinicNames
                };
                outSideGrid.Children.Add(image, 0, 2, 0, 4);
                outSideGrid.Children.Add(DptNameLabel, 0, 2, 4, 5);
                outSideGrid.Children.Add(BoxLayout, 2, 5, 0, 5);
                _allContentInAdd.Add(keys, outSideGrid);
                //BoxesDict.Add(dptName, boxList);
                mainStackLayout.Children.Add(outSideGrid);

                mainStackLayout.Children.Add(new BoxView
                {
                    HeightRequest = 1,
                    Color = Color.FromHex("#3f51b5")
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
            await Task.CompletedTask;
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

            #region Part of Others
            foreach (string otherItemName in OtherItemList)
            {
                List<AddExaminationItem> items =
                    _examinationItemDict[otherItemName];

                foreach (AddExaminationItem item in items)
                {
                    if (!item._checkbox.IsChecked) continue;

                    bool isDuplicate =
                        app.records.Any
                        (p => p._waypointName == item._waypointName &&
                        p.isAccept == false && p.DptName == item._waypointName)
                        ;

                    dumplicateCount++;
                    if (isDuplicate) continue;

                    app.records.Add(new RgRecord
                    {
                        order = 1,
                        _groupID = 0,
                        type = item.type,
                        _waypointID = item._waypointID,
                        _regionID = item._regionID,
                        _waypointName = item._waypointName,
                        DptName = item._waypointName
                    });
                    count++;
                }
            }
            #endregion

            #region Part of Process combo
            foreach (CheckBox optionBox in processBoxes)
            {
                Console.WriteLine(">>CheckBox optionBox in processBoxes");
                if (optionBox.IsChecked)
                {
                    if (((App)Application.Current).OrderDistrict
                        .ContainsKey(optionBox.Key))
                    {
                        Console.WriteLine("This item is dumplicate.");

                        await PopupNavigation.Instance.PushAsync(new
                            AlertDialogPopupPage(
                            string.Format(_resourceManager.GetString
                            ("SELECT_DUMPLICATE_CONTENT_STRING",
                            currentLanguage), optionBox.Text),
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
                List<AddExaminationItem> items =
                    _examinationItemDict[floorClinic];
                foreach (AddExaminationItem item in items)
                {
                    if (!item._checkbox.IsChecked) continue;

                    dumplicateCount++;
                    bool isDuplicate =
                        app.records.Any(p =>
                        p._waypointName == item._waypointName &&
                        p.DptName == item.DisplayName &&
                        p.isAccept == false
                        );
                    if (isDuplicate) continue;

                    int order = 1;

                    app.records.Add(new RgRecord
                    {
                        order = order,
                        _waypointID = item._waypointID,
                        _regionID = item._regionID,
                        type = RecordType.AddItem,
                        _groupID = 0,
                        _waypointName = item._waypointName,
                        DptName = item.DisplayName,
                        _subtitleName = item._waypointName
                    });
                    count++;
                }
            }
            #endregion
            #region Part of Examination
            foreach (string dptName in DepartmentList)
            {
                List<AddExaminationItem> items = _examinationItemDict[dptName];
                // List<CheckBox> Boxes = BoxesDict[dptName];

                foreach (AddExaminationItem item in items)
                {
                    if (!item._checkbox.IsChecked) continue;
                    dumplicateCount++;
                    var isDuplicate =
                        app.records.Any(p =>
                        (p.DptName == dptName &&
                         p._waypointName == item.DisplayName &&
                         p.isAccept == false));

                    if (isDuplicate)
                        continue;

                    int order =
                        app.OrderDistrict.ContainsKey(0) ?
                        app.OrderDistrict[0] : 1;

                    //app.records.Insert(index++, new RgRecord
                    app.records.Add(new RgRecord
                    {
                        _waypointID = item._waypointID,
                        _regionID = item._regionID,
                        _waypointName = item._waypointName,
                        _subtitleName = item.DisplayName,
                        type = RecordType.AddItem,
                        DptName = dptName,
                        _groupID = 0,
                        order = order++
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
                        _resourceManager.GetString("OK_STRING",
                        currentLanguage)));
                else if (dumplicateCount >= 1)
                    await PopupNavigation.Instance.PushAsync
                        (new AlertDialogPopupPage(_resourceManager.GetString
                        ("SELECT_DUPLICATE_EXAM_STRING", currentLanguage),
                       _resourceManager.GetString("OK_STRING",
                       currentLanguage)));
                return;
            }

            PopAddPage(false);
        }

        private class AddExaminationItem : DestinationItem
        {
            public string DisplayName;
            public CheckBox _checkbox;
            public override string ToString() => DisplayName;

        };

        private class SearchBarKey
        {
            public string _departNameLabelText { get; set; }
            public List<string> _clinicNameText { get; set; }
        }

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

        async private void PopAddPage(bool isCancel)
        {
            MessagingCenter.Send(this, "isCancel", isCancel);
            //PopupNavigation.Instance.PopAsync();
            await PopupNavigation.Instance.RemovePageAsync(this);
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            OnBackgroundClicked();
        }

        private void AddSearchBar_TextChanged(object sender,
            TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.NewTextValue))
            {
                mainStackLayout.Children.Clear();

                _allContentInAdd.All(p =>
                {
                    mainStackLayout.Children.Add(p.Value);
                    return true;
                });
            }
            else
            {
                mainStackLayout.Children.Clear();

                foreach (KeyValuePair<SearchBarKey, Grid> pair in _allContentInAdd)
                {
                    if (pair.Key._departNameLabelText.Contains(e.NewTextValue))
                    {
                        mainStackLayout.Children.Add(pair.Value);
                        continue;
                    }
                    else
                    {
                        foreach (string name in pair.Key._clinicNameText)
                        {
                            if (name.Contains(e.NewTextValue))
                            {
                                mainStackLayout.Children.Add(pair.Value);
                            }
                        }
                    }
                }

            }
        }
    }
}