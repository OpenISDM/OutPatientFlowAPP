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
        bool AllFinished;
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
            string fileName = "Yuanlin_OPFM.ExaminationRoomMap.xml";        
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
            StackLayout BoxLayout;
            Label DptNameLabel;
            Grid outSideGrid;
            Image image;
            int key = 0;
            int PictureCount = 1;
            mainStackLayout.Children.Add(new BoxView { BackgroundColor = Color.FromHex("#3f51b5"), HeightRequest = 1 });
            foreach (string dptName in DepartmentList)
            {
                outSideGrid = getGridLayout();

                image = new Image { Source = $"Add_Item{PictureCount++}", Aspect = Aspect.AspectFit, WidthRequest=80, HeightRequest=80,
                    VerticalOptions=LayoutOptions.Center, HorizontalOptions=LayoutOptions.Center
                };
                DptNameLabel = new Label { Text = dptName, FontSize =dptName.Length<=5?Device.GetNamedSize(NamedSize.Medium, typeof(Label)):Device.GetNamedSize(NamedSize.Small,typeof(Label)),
                    VerticalTextAlignment=TextAlignment.Center, HorizontalTextAlignment=TextAlignment.Center
                };

                BoxLayout = new StackLayout();
                boxList = new List<CheckBox>();
                key = 0;
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

            boxList = new List<CheckBox>();
            outSideGrid = getGridLayout();
            BoxLayout = new StackLayout();
            
            key = 0;
            DptNameLabel = new Label
            {
                Text = "回診",
                FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalTextAlignment = TextAlignment.Center,
            };

            image = new Image
            {
                Source = $"Add_Item{PictureCount++}",
                Aspect = Aspect.AspectFit,
                WidthRequest = 80,
                HeightRequest = 80,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };
            foreach (RgRecord record in app._TmpRecords)
            {
                CheckBox box = new CheckBox
                {
                    Text = record.DptName,
                    TextFontSize = Device.GetNamedSize(NamedSize.Large, typeof(CheckBox)),
                    Margin = new Thickness(0, -3),
                    Key = key++,
                    Type = CheckBox.CheckType.Check
                };
                box.CheckChanged += CheckBox_Changed;
                boxList.Add(box);
                BoxLayout.Children.Add(box);
            }
            outSideGrid.Children.Add(image, 0, 2, 0, 4);
            outSideGrid.Children.Add(DptNameLabel, 0, 2, 4, 5);
            outSideGrid.Children.Add(new ScrollView {Content=BoxLayout}, 2, 5, 0, 5);
            BoxesDict.Add("revisit", boxList);
            mainStackLayout.Children.Add(outSideGrid);
            mainStackLayout.Children.Add(new BoxView { BackgroundColor = Color.FromHex("#3f51b5"), HeightRequest = 1 });
        }

        private void CheckBox_Changed(object sender, EventArgs args)
        {
            Console.WriteLine($"CheckBox name is {((CheckBox)sender).Text}, Box key is{((CheckBox)sender).Key} ");
        }

        private Grid getGridLayout()
        {
            Grid tmp = new Grid();
            for (int i = 0; i < 4; i++)
            {
                tmp.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                tmp.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }
            return tmp;
        }
        private void AddCancelButton_Clicked(object sender, EventArgs e)
        {
            if (isButtonPressed) return;
            isButtonPressed = true;

            if (app.FinishCount + 1 == app.records.Count)
                AllFinished = true;
            else
                AllFinished = false;

            GobackPage(AllFinished);

        }

        async private void AddConfirmButton_Clicked(object sender, EventArgs e)
        {
            //if (isButtonPressed) return;
            //isButtonPressed = true;
            int count = 0;
            int index = (app.roundRecord == null || !app.lastFinished.Key.Equals("QueryResult")) ? (app.records.Count - 1) : (app.records.IndexOf(app.roundRecord) + 1);
            int dumplicateCount = 0;
            foreach (string dptName in DepartmentList)
            {
                List<AddExaminationItem> items = _examinationItemDict[dptName];
                List<CheckBox> Boxes = BoxesDict[dptName];
                Console.WriteLine($"dptName is {dptName}");

                foreach(CheckBox box in Boxes)
                {
                    if (!box.IsChecked) continue;
                    dumplicateCount++;
                    var isDuplicate = app.records.Any(p => (p.DptName==dptName+"-"+items[box.Key].DisplayName) && p.isAccept == false);
                    //Console.WriteLine($"isDuplicate is {isDuplicate}");
                    if (isDuplicate)
                        continue;

                    app.records.Insert(index++, new RgRecord {
                        _waypointID=items[box.Key]._waypointID,
                        _regionID=items[box.Key]._regionID,
                        _waypointName=items[box.Key]._waypointName,
                        Key="AddItem",
                        DptName=dptName+"-"+items[box.Key].DisplayName
                    });
                    count++;
                }
            }

            foreach(CheckBox box in BoxesDict["revisit"])
            {
                
                if (box.IsChecked == false) continue;
                Console.WriteLine($"now deal is {box.Text} key is {box.Key}");
                var isDumplicate = app.records.Any(a =>a.DptName==("回診-"+app._TmpRecords[box.Key].DptName) && !a.isAccept);

                dumplicateCount++;
                if (isDumplicate) continue;
                Console.WriteLine($"it's not dumplicate:{box.Text} key is {box.Key}");
                app.records.Insert(index++, new RgRecord {
                    Key="AddItem",
                    _waypointID=app._TmpRecords[box.Key]._waypointID,
                    _regionID = app._TmpRecords[box.Key]._regionID,
                    _waypointName=app._TmpRecords[box.Key]._waypointName,
                    DptName="回診-"+app._TmpRecords[box.Key].DptName
                });
                count++;
            }

            if (count == 0)
            {
                if (dumplicateCount == 0)
                    await PopupNavigation.Instance.PushAsync(new AlertDialogPopupPage(_resourceManager.GetString("NO_SELECT_DESTINATION_STRING", currentLanguage),
                        _resourceManager.GetString("OK_STRING", currentLanguage)));
                else if (dumplicateCount >= 1)
                    await PopupNavigation.Instance.PushAsync(new AlertDialogPopupPage(_resourceManager.GetString("SELECT_DUPLICATE_EXAM_STRING",currentLanguage), 
                        _resourceManager.GetString("OK_STRING",currentLanguage)));
                return;
            }
            GobackPage(false);
        }

        private class AddExaminationItem: DestinationItem
        {
            public string DisplayName;
            public override string ToString() => DisplayName;
           
        };
        protected override bool OnBackButtonPressed()
        {      
            if (app.FinishCount + 1 == app.records.Count)
                AllFinished = true;
            else
                AllFinished = false;

            GobackPage(AllFinished);
            return base.OnBackButtonPressed();
        }
        protected override bool OnBackgroundClicked()
        {
            if (app.FinishCount + 1 == app.records.Count)
                AllFinished = true;
            else
                AllFinished = false;
            GobackPage(AllFinished);
            return base.OnBackgroundClicked();
        }

        private void GobackPage(bool ConfirmOrCancel)
        {
            MessagingCenter.Send(this, "AddAnyOrNot", ConfirmOrCancel);
            MessagingCenter.Send(this, "isBack", true);
            PopupNavigation.Instance.PopAsync();
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            OnBackgroundClicked();
        }       
    }
}