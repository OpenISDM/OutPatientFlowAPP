using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using IndoorNavigation.Resources.Helpers;
using Rg.Plugins.Popup;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Collections.ObjectModel;
using IndoorNavigation.Views.Navigation;
using MvvmHelpers;
using Plugin.InputKit;
using Plugin.InputKit.Shared.Controls;
using CheckBox = Plugin.InputKit.Shared.Controls.CheckBox;
using Plugin.Multilingual;

namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddPopupPage : PopupPage
    {
        const string _resourceId = "IndoorNavigation.Resources.AppResources";
        ResourceManager _resourceManager =
            new ResourceManager(_resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);
        App app = (App)Application.Current;
        ObservableCollection<DestinationItem> items;
        ObservableCollection<DestinationItem> ItemPreSelect = new ObservableCollection<DestinationItem>();
        public AddPopupPage()
        {
            InitializeComponent();
             items = LoadData();
            ItemPreSelect.Clear();
         
        }
        private ObservableCollection<DestinationItem> LoadData() //to load test layout file
        {
            ObservableCollection<DestinationItem> items = new ObservableCollection<DestinationItem>();
            items.Add(new DestinationItem
            {
                _waypointID=new Guid(),
                _regionID=new Guid(),
                _waypointName="內視鏡",
                Key="examination"
            });
            items.Add(new DestinationItem
            {
                _waypointID = new Guid(),
                _regionID = new Guid(),
                _waypointName = "X光",
                Key = "examination"
            });

         /*   StackLayout layot = new StackLayout();

            CheckBox box = new CheckBox
            {
                Text = "Wtf",
                Type = CheckBox.CheckType.Check,
                TextFontSize = 24,
                IsChecked = false
            };

            box.CheckChanged += Box_CheckChanged1;*/
            /*

            
            CheckBoxStackLayout.Children.Add(new Plugin.InputKit.Shared.Controls.CheckBox
            {
                Text = "內視鏡",
                Type = CheckBox.CheckType.Check,
                IsChecked = false,
                TextFontSize = 24
            });

            CheckBoxStackLayout.Children.Add(new Plugin.InputKit.Shared.Controls.CheckBox
            {
                Text = "X光",
                Type = CheckBox.CheckType.Check,
                IsChecked = false,
                TextFontSize = 24
            });
            CheckBoxStackLayout.Children.Add(new Plugin.InputKit.Shared.Controls.CheckBox
            {
                Text = "超音波",
                Type = CheckBox.CheckType.Check,
                IsChecked = false,
                TextFontSize = 24
            });*/
            items.Add(new DestinationItem
            {
                _regionID=new Guid(),
                _waypointID=new Guid(),
                _waypointName="超音波",
                Key="examination"
            });
            items.Add(new DestinationItem
            {
                _regionID=new Guid(),
                _waypointID=new Guid(),
                _waypointName="回診",
                Key= "Hosiptal round"
            });
            items.Add(new DestinationItem
            {
                _waypointID = new Guid(),
                _regionID = new Guid(),
                _waypointName = "內視鏡",
                Key = "examination"
            });

            items.Add(new DestinationItem
            {
                _regionID = new Guid(),
                _waypointID = new Guid(),
                _waypointName = "超音波",
                Key = "examination"
            });
            items.Add(new DestinationItem
            {
                _regionID = new Guid(),
                _waypointID = new Guid(),
                _waypointName = "回診",
                Key = "Hosiptal round"
            });
            items.Add(new DestinationItem
            {
                _waypointID = new Guid(),
                _regionID = new Guid(),
                _waypointName = "內視鏡",
                Key = "examination"
            });

            items.Add(new DestinationItem
            {
                _regionID = new Guid(),
                _waypointID = new Guid(),
                _waypointName = "超音波",
                Key = "examination"
            });
            items.Add(new DestinationItem
            {
                _regionID = new Guid(),
                _waypointID = new Guid(),
                _waypointName = "回診",
                Key = "Hosiptal round"
            });
            return items;
        }

        private void Box_CheckChanged1(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
        private void Box_CheckChanged(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
        async private void AddCancelPopBtn_Clicked(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PopAsync();
        }

        private async void AddOKPopBtn_Clicked(object sender, EventArgs e)
        {
            var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
            if (ItemPreSelect.Count > 0)
            {
                foreach (DestinationItem o in ItemPreSelect)
                {
                    app.records.Add(new RgRecord
                    {
                        _regionID = o._regionID,
                        _waypointID = o._waypointID,
                        _waypointName = o._waypointName,
                        Key = "AddItem",
                        DptName = o._waypointName
                    });
                }
                await PopupNavigation.Instance.PopAsync();
            }
            else
            {
                await DisplayAlert(_resourceManager.GetString("MESSAGE_STRING",currentLanguage),
                    _resourceManager.GetString("NO_SELECT_DESTINATION_STRING",currentLanguage),_resourceManager.GetString("OK_STRING",currentLanguage));
            }
        }


        private void CheckBox_CheckChanged(object sender, EventArgs e)
        {
            var o = (CheckBox)sender;
            string destinationName = o.Text;
            if (!o.IsChecked)
            {
              foreach(DestinationItem item in items)
                {
                    if(item._waypointName.Equals(destinationName))
                    {
                        ItemPreSelect.Remove(item);
                        break;
                    }
                }
            }
            else
            {
                foreach (DestinationItem item in items)
                {
                    if (item._waypointName.Equals(destinationName))
                    {
                        ItemPreSelect.Add(item);
                        break;
                    }
                }
            }
        }
    }
}