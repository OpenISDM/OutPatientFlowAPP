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
using System.IO;
using Plugin.InputKit.Shared.Controls;
namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddPopupPage_v2 : PopupPage
    {
        App app = (App)Application.Current;
        const string _resourceId = "IndoorNavigation.Resources.AppResources";
        ResourceManager _resourceManager =
            new ResourceManager(_resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);
        CultureInfo currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
        bool isButtonPressed = false;
        string _graphName;

        Dictionary<string, List<CheckBox>> BoxesDict;
        //Dictionary<string, SelectionView> BoxesDict;
        Dictionary<string, List<AddExaminationItem>> _examinationItemDict;
        List<string> DepartmentList;
        public AddPopupPage_v2(string graphName)
        {
            InitializeComponent();
            _graphName = graphName;
            BoxesDict = new Dictionary<string, List<CheckBox>>();
            DepartmentList = new List<string>();
            _examinationItemDict = new Dictionary<string, List<AddExaminationItem>>();
            LoadData();
            LoadBox();
        }

        private void LoadData()
        {
            string fileName = "ExaminationRoomMap.xml";        
            var assembly = typeof(ExitPopupViewModel).GetTypeInfo().Assembly;
            string content = "";
            Stream stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.{fileName}");

            using(var reader=new StreamReader(stream))
            {
                content = reader.ReadToEnd();
            }

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(content);

            XmlNodeList DepartmentLists = doc.GetElementsByTagName("Department");

            foreach(XmlNode department in DepartmentLists)
            {                
                XmlNodeList examinationRooms = department.ChildNodes;
                string departName = department.Attributes["name"].Value;
                DepartmentList.Add(departName);
                
                List<AddExaminationItem> items=new List<AddExaminationItem>();
                foreach(XmlNode room in examinationRooms)
                {
                    AddExaminationItem item = new AddExaminationItem();
                    item.DisplayName = room.Attributes["displayname"].Value;
                    item._regionID=new Guid (room.Attributes["region_id"].Value);
                    item._waypointID =new Guid (room.Attributes["waypoint_id"].Value);
                    item.Key = "AddItem";
                    item._waypointName = room.Attributes["name"].Value;

                    items.Add(item);
                }
                _examinationItemDict.Add(departName,items);
                Console.WriteLine($"departname={departName}");
            }
        }

        private void LoadBox()
        {
            List<CheckBox> boxList;
            mainStackLayout.Children.Add(new BoxView { BackgroundColor = Color.FromHex("#3f51b5"), HeightRequest = 1 });
            foreach (string dptName in DepartmentList)
            {
                Grid outSideGrid = getGridLayout();

                Image image = new Image { Source = "Arrived.png", Aspect = Aspect.AspectFit, WidthRequest=80, HeightRequest=80,
                    VerticalOptions=LayoutOptions.Center, HorizontalOptions=LayoutOptions.Center
                };
                Label DptNameLabel = new Label { Text = dptName, FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                    VerticalTextAlignment=TextAlignment.Center, HorizontalTextAlignment=TextAlignment.Center,
                };

                StackLayout BoxLayout = new StackLayout();
                boxList = new List<CheckBox>();
                int key = 0;
                foreach(AddExaminationItem item in _examinationItemDict[dptName])
                {
                    CheckBox box = new CheckBox { Text = item.DisplayName, TextFontSize = Device.GetNamedSize(NamedSize.Large, typeof(CheckBox)),
                         Margin = new Thickness(0, -3), Key=key++, Type=CheckBox.CheckType.Check
                    };
                    box.CheckChanged += CheckBox_Changed;
                    BoxLayout.Children.Add(box);
                    boxList.Add(box);
                }
                
                outSideGrid.Children.Add(image, 0, 2, 0, 4);
                outSideGrid.Children.Add(DptNameLabel, 0, 2, 4, 5);
                outSideGrid.Children.Add(/*new ScrollView {Content=BoxLayout}*/BoxLayout, 2,5,0,5);
                BoxesDict.Add(dptName, boxList);
                mainStackLayout.Children.Add(outSideGrid);
                mainStackLayout.Children.Add(new BoxView { BackgroundColor = Color.FromHex("#3f51b5"), HeightRequest=1});
            }
        }

        private void CheckBox_Changed(object sender, EventArgs args)
        {
            Console.WriteLine($"CheckBox name is {((CheckBox)sender).Text}, Box key is{((CheckBox)sender).Key} ");
        }

        private Grid getGridLayout()
        {
            Grid tmp = new Grid();

            tmp.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            tmp.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            tmp.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            tmp.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            tmp.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            tmp.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.5, GridUnitType.Star) });            
            tmp.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            tmp.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            return tmp;
        }
        private void AddCancelButton_Clicked(object sender, EventArgs e)
        {
            if (isButtonPressed) return;
            isButtonPressed = true;          
            PopupNavigation.Instance.PopAsync();
        }

        async private void AddConfirmButton_Clicked(object sender, EventArgs e)
        {
            //if (isButtonPressed) return;
            //isButtonPressed = true;
            int count = 0;

            foreach (string dptName in DepartmentList)
            {
                List<AddExaminationItem> items = _examinationItemDict[dptName];
                List<CheckBox> Boxes = BoxesDict[dptName];
                Console.WriteLine($"dptName is {dptName}");

                foreach(CheckBox box in Boxes)
                {
                    Console.WriteLine($"Box name is{box.Text}, key is{box.Key}");
                    Console.WriteLine($"item in box key is {items[box.Key]}");
                    Console.WriteLine($"item id is {items[box.Key]._waypointID} and {items[box.Key]._regionID}");
                    Console.WriteLine($"item id is {items[box.Key]._waypointName} and {items[box.Key].Key}");
                    if (!box.IsChecked) continue;

                    var isDuplicate = app.records.Any(p => p.DptName==items[box.Key].DisplayName && p.isAccept == false);
                    //Console.WriteLine($"isDuplicate is {isDuplicate}");
                    if (isDuplicate)
                        continue;

                    app.records.Add(new RgRecord {
                        _waypointID=items[box.Key]._waypointID,
                        _regionID=items[box.Key]._regionID,
                        _waypointName=items[box.Key]._waypointName,
                        Key="AddItem",
                        DptName=dptName+"-"+items[box.Key].DisplayName
                    });
                    count++;
                }
            }
            if (count == 0)
            {
                await DisplayAlert(_resourceManager.GetString("MESSAGE_STRING", currentLanguage), _resourceManager.GetString("NO_SELECT_DESTINATION_STRING", currentLanguage)
                , _resourceManager.GetString("OK_STRING", currentLanguage));

                return;
            }
            PopupNavigation.Instance.PopAsync();
        }

        private class AddExaminationItem: DestinationItem
        {
            public string DisplayName;
            public override string ToString() => DisplayName;
           
        };

        // Dictionary<string, SelectionView> _radioBoxes;
        // Dictionary<string,AddItems> _items;
        // List<String> examinationTypes;
        // bool ButtonLock = false;
        //// CheckBox RevisitBox;

        // public AddPopupPage_v2(string graphName)
        // {
        //     InitializeComponent();
        //     //checkBoxes = new Dictionary<string, List<CheckBox>>();
        //     //_radioBoxes = new Dictionary<string, SelectionView>();
        //     //_items = new Dictionary<string, AddItems>();
        //     //examinationTypes = new List<string>();
        //     //RevisitBox = new CheckBox();
        //     //LoadTypes();
        //     //LoadItems();
        //     //LoadBoxes();
        //     //LoadRadioBoxes();

        // }
        // public void LoadTypes()
        // {
        //     examinationTypes.Add("X光");
        //     examinationTypes.Add("超音波");
        //     examinationTypes.Add("抽血處");
        // }
        // public void LoadItems()
        // {
        //     foreach (String type in examinationTypes)
        //     {
        //         AddItems item = new AddItems();
        //         item._examinationType = type;

        //         for (int i = 0; i < 5; i++)
        //         {
        //             item._addItems.Add(new RgRecord
        //             {
        //                 _regionID = new Guid("11111111-1111-1111-1111-111111111111"),
        //                 _waypointID = new Guid("00000000-0000-0000-0000-000000000002"),
        //                 _waypointName=$"{i+1}樓",
        //                 DptName=$"{i+1}樓{type}",
        //                 Key="AddItem"
        //             });
        //         }
        //         _items.Add(type, item);
        //     }
        // }

        // //To generate radio box with getting data
        // public void LoadRadioBoxes()
        // {
        //    foreach(string ExaminationType in examinationTypes)
        //     {
        //         AddItems items = _items[ExaminationType];
        //         StackLayout Toplayout = new StackLayout { Orientation = StackOrientation.Horizontal };

        //         StackLayout IconLayout = new StackLayout { Children =
        //             {
        //                 new Image{Source="AddItem_0", Aspect=Aspect.AspectFit, WidthRequest=90, HeightRequest=90},
        //                 new Label{Text="X光", FontSize=16, BackgroundColor=Color.Black, TextColor=Color.White, VerticalTextAlignment=TextAlignment.Center, HorizontalTextAlignment=TextAlignment.Center}
        //             }};

        //         SelectionView radioView = new SelectionView
        //         {
        //             SelectionType=SelectionType.RadioButton, ColumnNumber=2, ItemsSource=items._addItems, ColumnSpacing=12, Padding=new Thickness(8,0)
        //         };

        //         _radioBoxes.Add(ExaminationType, radioView);

        //         Toplayout.Children.Add(IconLayout);
        //         Toplayout.Children.Add(radioView);
        //         PictureCheckBoxStacklayout.Children.Add(Toplayout);
        //     }
        // }
        // async private void AddConfirmButton_Clicked_v2(object sendser, EventArgs args)
        // {
        //     if (ButtonLock) return;

        //     ButtonLock = true;
        //     foreach(string ExaminationType in examinationTypes)
        //     {
        //         var o = _radioBoxes[ExaminationType].SelectedItem as RgRecord;

        //         if (o != null)
        //         {
        //             app.records.Add(o);
        //         }
        //     }
        //     await PopupNavigation.Instance.PopAllAsync();
        // }

        // async private void AddCancelButton_Clicked(object sender, EventArgs e)
        // {
        //     await PopupNavigation.Instance.PopAsync();
        // }

    }
}