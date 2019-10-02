using IndoorNavigation.Resources.Helpers;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Collections.ObjectModel;
using Plugin.Multilingual;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Plugin.InputKit.Shared.Controls;

namespace IndoorNavigation
{
    class TmpDocument : ContentPage
    {
        const string _resourceId = "IndoorNavigation.Resources.AppResources";
        ResourceManager _resourceManager =
            new ResourceManager(_resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);

        App app = (App)App.Current;

        ObservableCollection<RgRecord> items;  //for store the waypoint data object array
        ObservableCollection<RgRecord> ItemPreSelect;

        public TmpDocument()
        {
            items = LoadData();
            ItemPreSelect = new ObservableCollection<RgRecord>();
            if (app.roundRecord == null) ;
                
        }
        private async void AddOKPopBtn_Clicked(object sender, EventArgs e)
        {
            var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
            int index = app.FinishCount;
            if (ItemPreSelect.Count > 0)
            {
                foreach(RgRecord record in ItemPreSelect)
                {
                    app.records.Insert(index++, new RgRecord
                    {
                        _regionID = record._regionID,
                        _waypointID=record._waypointID,
                        _waypointName=record._waypointName,
                        Key="AddItem",
                        DptName=record.DptName
                    }) ;
                }
                await PopupNavigation.Instance.PopAsync();
            }

            else
            {
                await DisplayAlert("1", "2", "3");
            }
         
        }

        private ObservableCollection<RgRecord> LoadData() //to load test layout file
        {
            ObservableCollection<RgRecord> items = new ObservableCollection<RgRecord>();
            items.Add(new RgRecord
            {
                _waypointID = new Guid(),
                _regionID = new Guid(),
                _waypointName = "內視鏡",
                DptName ="內視鏡",
                Key = "examination"
            });
            items.Add(new RgRecord
            {
                _waypointID = new Guid(),
                _regionID = new Guid(),
                _waypointName = "X光",
                DptName="X光",
                Key = "examination"
            });
            items.Add(new RgRecord
            {
                _regionID = new Guid(),
                _waypointID = new Guid(),
                _waypointName = "超音波",
                DptName="超音波",
                Key = "examination"
            });


            return items;
        }
        private void CheckBox_CheckChanged(object sender, EventArgs e)
        {
            var o = (CheckBox)sender;
            string destinationName = o.Text;
            //if (app.roundRecord != null && (destinationName.Equals("revisit") || (destinationName.Equals("")))
           
                AddRemovePreSelectItem(o.IsChecked, destinationName);
            
            //else
         
        }


        private void AddRemovePreSelectItem(bool isChecked, string destinationName)
        {
            RgRecord record;
            if(destinationName.Equals("回診") || destinationName.Equals("revisit"))
            {
                record = app.roundRecord;
                record.DptName = string.Format("回診({0})", record.DptName);

                if (isChecked)
                    ItemPreSelect.Add(record);
                else
                    ItemPreSelect.Remove(record);
            }
            else
            {
                foreach(RgRecord item in items)
                {
                    if (item.DptName.Equals(destinationName))
                    {
                        if (isChecked)
                            ItemPreSelect.Add(item);
                        else
                            ItemPreSelect.Remove(item);
                    }
                }
            }
        }
    }
}
