using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using IndoorNavigation.Resources.Helpers;
using System.Resources;
using System.Reflection;
using MvvmHelpers;
using Plugin.InputKit;
using Plugin.InputKit.Shared.Controls;

namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NewItemPage : ContentPage
    {
      /*  const string _resourceId = "IndoorNavigation.Resources.AppResources";
        ResourceManager _resourceManager =
            new ResourceManager(_resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);
        App app = (App)Application.Current;
        ObservableCollection<AddDestinationItem> AddItemPreSelect=new ObservableCollection<AddDestinationItem>();
        ObservableCollection<AddDestinationItem> items;
        public NewItemPage()
        {
            InitializeComponent();
            items = LoadData();
            AddItemPreSelect.Clear();

            AddItemListView.ItemsSource = from item in items
                                           group item by item.Key into itemsGroup
                                           orderby itemsGroup.Key
                                           select new Grouping<string, DestinationItem>(itemsGroup.Key, itemsGroup);
        }
        private ObservableCollection<AddDestinationItem> LoadData()
        {
            ObservableCollection<AddDestinationItem> items = new ObservableCollection<AddDestinationItem>();
            items.Add(new AddDestinationItem
            {
                _waypointID = new Guid(),
                _regionID = new Guid(),
                _waypointName = "內視鏡",
                
                Key = "examination"
            });

            items.Add(new AddDestinationItem
            {
                _regionID = new Guid(),
                _waypointID = new Guid(),
                _waypointName = "超音波",
                Key = "examination"
            });
            items.Add(new AddDestinationItem
            {
                _regionID = new Guid(),
                _waypointID = new Guid(),
                _waypointName = "回診",
                Key = "Hosiptal round"
            });
            items.Add(new AddDestinationItem
            {
                _waypointID = new Guid(),
                _regionID = new Guid(),
                _waypointName = "內視鏡",
                Key = "examination"
            });

            items.Add(new AddDestinationItem
            {
                _regionID = new Guid(),
                _waypointID = new Guid(),
                _waypointName = "超音波",
                Key = "examination"
            });
            items.Add(new AddDestinationItem
            {
                _regionID = new Guid(),
                _waypointID = new Guid(),
                _waypointName = "回診",
                Key = "Hosiptal round"
            });
            items.Add(new AddDestinationItem
            {
                _waypointID = new Guid(),
                _regionID = new Guid(),
                _waypointName = "內視鏡",
                Key = "examination"
            });

            items.Add(new AddDestinationItem
            {
                _regionID = new Guid(),
                _waypointID = new Guid(),
                _waypointName = "超音波",
                Key = "examination"
            });
            items.Add(new AddDestinationItem
            {
                _regionID = new Guid(),
                _waypointID = new Guid(),
                _waypointName = "回診",
                Key = "Hosiptal round"
            });
            return items;
        }

        private async void AddItemCancelBtn_Clicked(object sender,EventArgs e)
        {
            if(AddItemListView.SelectedItem!=null)
                AddItemListView.SelectedItem = null;
            await Navigation.PopAsync();
        }
        private async void AddItemOKBtn_Clicked(object sender,EventArgs e)
        {
            if(AddItemPreSelect.Count>0)
            {

                foreach(AddDestinationItem o in  AddItemPreSelect){
                    app.records.Add(new RgRecord
                    {
                        _regionID = o._regionID,
                        _waypointID = o._waypointID,
                        _waypointName = o._waypointName,
                        Key = "AddItem",
                        DptName = o._waypointName
                    });
                }

                //var o = AddItemListView.SelectedItem as DestinationItem;

             /  app.records.Add(new RgRecord
                {
                    _regionID=o._regionID,
                    _waypointID=o._waypointID,
                    _waypointName=o._waypointName,
                    Key= "AddItem",
                    DptName =o._waypointName
                });

                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("message", "plz select a destination", "ok");
                
            }
        }

        private void AddItemListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
               var o = e.Item as AddDestinationItem;

               if (!AddItemPreSelect.Contains(o) && !o.isSelect)
               {
                  (e.Item as AddDestinationItem).BoxLabelString="\u2611";
                   AddItemPreSelect.Add(o);
                   (e.Item as AddDestinationItem).isSelect=true;

               }
               else
               {
                  (e.Item as AddDestinationItem).BoxLabelString="\u2610";
                   AddItemPreSelect.Remove(o);
                   (e.Item as AddDestinationItem).isSelect = false;
               }
        }

        private void CheckBox_CheckChanged(object sender, EventArgs e)
        {
           
        }*/
    }
}