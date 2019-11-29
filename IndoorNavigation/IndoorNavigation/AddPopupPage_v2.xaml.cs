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
//using Plugin.InputKit.Shared.Controls;
using Plugin.Multilingual;
using System.Globalization;
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
        //Dictionary<string, List<CheckBox>> checkBoxes;
        Dictionary<string, SelectionView> _radioBoxes;
        Dictionary<string,AddItems> _items;
        List<String> examinationTypes;
        bool ButtonLock = false;
       // CheckBox RevisitBox;

        public AddPopupPage_v2()
        {
            InitializeComponent();
           // checkBoxes = new Dictionary<string, List<CheckBox>>();
            _radioBoxes = new Dictionary<string, SelectionView>();
            _items = new Dictionary<string, AddItems>();
            examinationTypes = new List<string>();
          //  RevisitBox = new CheckBox();
            LoadTypes();
            LoadItems();
            //LoadBoxes();
            LoadRadioBoxes();
            
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

        //To generate radio box with getting data
        public void LoadRadioBoxes()
        {
           foreach(string ExaminationType in examinationTypes)
            {
                AddItems items = _items[ExaminationType];
                StackLayout Toplayout = new StackLayout { Orientation = StackOrientation.Horizontal };

                StackLayout IconLayout = new StackLayout { Children =
                    {
                        new Image{Source="AddItem_0", Aspect=Aspect.AspectFit, WidthRequest=90, HeightRequest=90},
                        new Label{Text="X光", FontSize=16, BackgroundColor=Color.Black, TextColor=Color.White, VerticalTextAlignment=TextAlignment.Center, HorizontalTextAlignment=TextAlignment.Center}
                    }};

                SelectionView radioView = new SelectionView
                {
                    SelectionType=SelectionType.RadioButton, ColumnNumber=2, ItemsSource=items._addItems, ColumnSpacing=12, Padding=new Thickness(8,0)
                };

                _radioBoxes.Add(ExaminationType, radioView);

                Toplayout.Children.Add(IconLayout);
                Toplayout.Children.Add(radioView);
                PictureCheckBoxStacklayout.Children.Add(Toplayout);
            }
        }
        async private void AddConfirmButton_Clicked_v2(object sendser, EventArgs args)
        {
            if (ButtonLock) return;

            ButtonLock = true;
            foreach(string ExaminationType in examinationTypes)
            {
                var o = _radioBoxes[ExaminationType].SelectedItem as RgRecord;

                if (o != null)
                {
                    app.records.Add(o);
                }
            }
            await PopupNavigation.Instance.PopAllAsync();
        }

        async private void AddCancelButton_Clicked(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PopAsync();
        }
 
    }
}