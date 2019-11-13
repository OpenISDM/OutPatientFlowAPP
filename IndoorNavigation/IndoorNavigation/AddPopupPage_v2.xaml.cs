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
namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddPopupPage_v2 : PopupPage
    {
        App app = (App)Application.Current;
        const string _resourceId = "IndoorNavigation.Resources.AppResources";
        ResourceManager _resourceManager =
            new ResourceManager(_resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);

        List<List<CheckBox>> checkBoxes;
        List<List<DestinationItem>> AddItems;

        public AddPopupPage_v2()
        {
            InitializeComponent();
            checkBoxes = new List<List<CheckBox>>();
            AddItems = new List<List<DestinationItem>>();

            LoadItems();
            LoadBoxes();
        }

        public void LoadItems()
        {
            for(int i = 1; i < 4; i++)
            {
                List<DestinationItem> items=new List<DestinationItem>();
                for(int j = 1; j < 5; j++)
                {
                    DestinationItem item = new DestinationItem
                    {
                        _waypointName = $"{i*j}樓",
                        Key="AddItem"
                    };

                    items.Add(item);
                }
                AddItems.Add(items);
            }
        }
        public void LoadBoxes()
        {
            int j = 0;
            foreach(List<DestinationItem> items in AddItems)
            {
                j++;
                Console.WriteLine("1111111111111111");
                StackLayout layout = new StackLayout
                {
                    Orientation =StackOrientation.Horizontal
                };
                //layout.Children.Add(new BoxView {
                //    WidthRequest =90,
                //    Color=Color.Blue
                //});
                layout.Children.Add(new Image
                {
                    Source=$"AddItem_{j}",
                    HeightRequest=90,
                    WidthRequest=90,
                    Aspect=Aspect.AspectFit
                });
                int i=0;
                StackLayout CheckboxLayout = new StackLayout();
                foreach(DestinationItem item in items)
                {
                    i++;
                    Console.WriteLine("22222222");
                    CheckBox box = new CheckBox
                    {
                        Text=item._waypointName,
                        TextFontSize=16,
                        Margin=new Thickness(8,0),
                        Type=CheckBox.CheckType.Check,
                        TextColor=Color.Black
                    };
                    CheckboxLayout.Children.Add(box);

                    if (i % 3==0)
                    {
                        layout.Children.Add(CheckboxLayout);
                        CheckboxLayout = new StackLayout();
                    }
                }
                layout.Children.Add(CheckboxLayout);
                PictureCheckBoxStacklayout.Children.Add(layout);
                PictureCheckBoxStacklayout.Children.Add(new BoxView
                {
                    HeightRequest = 1,
                    Color = Color.FromHex("3f51b5"),
                    Margin=4
                });
            }
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();


        }
    }
}