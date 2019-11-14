using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using IndoorNavigation.Resources.Helpers;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Plugin.InputKit.Shared.Controls;
using Plugin.Multilingual;

namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddPopupPage_v2 : PopupPage
    {
        App app = (App)Application.Current;
        const string _resourceId = "IndoorNavigation.Resources.AppResources";
        ResourceManager _resourceManager =
            new ResourceManager(_resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);

        Dictionary<string, List<CheckBox>> checkBoxes;
        Dictionary<string,AddItems> _items;
        List<String> examinationTypes;
        CheckBox RevisitBox;

        public AddPopupPage_v2()
        {
            InitializeComponent();
            checkBoxes = new Dictionary<string, List<CheckBox>>();
            _items = new Dictionary<string, AddItems>();
            examinationTypes = new List<string>();
            RevisitBox = new CheckBox();
            LoadTypes();
            LoadItems();
            LoadBoxes();
        }
        public void LoadTypes()
        {
            examinationTypes.Add("X光");
            examinationTypes.Add("超音波");
            examinationTypes.Add("抽血處");
        }
        public void LoadItems()
        {
            foreach (String type in examinationTypes)
            {
                AddItems item = new AddItems();
                item._examinationType = type;

                for (int i = 0; i < 5; i++)
                {
                    item._addItems.Add(new RgRecord
                    {
                        _regionID = new Guid("11111111-1111-1111-1111-111111111111"),
                        _waypointID = new Guid("00000000-0000-0000-0000-000000000002"),
                        _waypointName=$"{i+1}樓",
                        DptName=$"{i+1}樓{type}",
                        Key="AddItem"
                    });
                }
                _items.Add(type, item);
            }
        }
        public void LoadBoxes()
        {
            int j = 0;
            foreach(string type in examinationTypes)
            {
                AddItems items = _items[type];
                j++;

                StackLayout layout = new StackLayout
                {
                    Orientation =StackOrientation.Horizontal
                };

                StackLayout picturelayout = new StackLayout();

                picturelayout.Children.Add(new Image
                {
                    Source=$"AddItem_{j}", HeightRequest=90,
                    WidthRequest=90, Aspect=Aspect.AspectFit
                });

                picturelayout.Children.Add(new Label
                {
                    Text=type,FontSize=16,TextColor=Color.White,BackgroundColor=Color.Black,
                    HorizontalTextAlignment =TextAlignment.Center,VerticalTextAlignment=TextAlignment.Center,
                });
                layout.Children.Add(picturelayout);
                
                StackLayout CheckboxLayout = new StackLayout();
                List<CheckBox> boxes = new List<CheckBox>();

                int i = 0;
                foreach (RgRecord item in items._addItems)
                {
                    
                    CheckBox box = new CheckBox
                    {
                        Text=item._waypointName,TextFontSize=16,Margin=new Thickness(8,0),
                        Type=CheckBox.CheckType.Check,TextColor=Color.Black,Key=i
                    };

                    CheckboxLayout.Children.Add(box);
                    boxes.Add(box);
                    i++;

                    item._waypointName = $"{item._waypointName}{type}";

                    //let layout template
                    if (i % 3==0)
                    {
                        layout.Children.Add(CheckboxLayout);
                        CheckboxLayout = new StackLayout();
                    }
                }

                layout.Children.Add(CheckboxLayout);
                checkBoxes.Add(items._examinationType, boxes);
                PictureCheckBoxStacklayout.Children.Add(layout);
                PictureCheckBoxStacklayout.Children.Add(new BoxView
                {
                    HeightRequest = 1, Color = Color.FromHex("3f51b5"), Margin=4
                });
            }
        }

        async private void AddConfirmButton_Clicked(object sender, EventArgs e)
        {
            int index = (app.roundRecord == null && !RevisitBox.IsChecked) ? (app.records.Count - 1) : (app.records.IndexOf(app.roundRecord) + 1);
            int count = 0;
            var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
            
            foreach(String type in examinationTypes)
            {
                
                List<CheckBox> boxes = checkBoxes[type];
                foreach(CheckBox box in boxes)
                {
                    if (!box.IsChecked) continue;
                    count++;
                    var isDuplicate = app.records.Any(p => p.DptName == _items[type]._addItems[box.Key].DptName && !p.isAccept);

                    if (isDuplicate) continue;
                    app.records.Insert(index,_items[type]._addItems[box.Key]);
                }
            }
            if (RevisitBox.IsChecked)
            {
                count++;
                app.records.Insert(index++, new RgRecord
                {
                    DptName=$"回診{app.roundRecord.DptName}", _regionID = app.roundRecord._regionID,
                    _waypointID = app.roundRecord._waypointID, Key = "AddItem",
                    _waypointName = app.roundRecord._waypointName

                });
                app.roundRecord = null;
            }
            if (count == 0)
            {
                await DisplayAlert(_resourceManager.GetString("MESSAGE_STRING", currentLanguage), _resourceManager.GetString("NO_SELECT_DESTINATION_STRING", currentLanguage)
                    , _resourceManager.GetString("OK_STRING", currentLanguage));
            }

            await PopupNavigation.Instance.PopAllAsync();
        }

        async private void AddCancelButton_Clicked(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PopAsync();
        }
 
    }
}