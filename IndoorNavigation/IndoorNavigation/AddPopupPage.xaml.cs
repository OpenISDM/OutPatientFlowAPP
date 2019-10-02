﻿using System;
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
        ObservableCollection<RgRecord> items;
        ObservableCollection<RgRecord> ItemPreSelect = new ObservableCollection<RgRecord>();
        public AddPopupPage()
        {
            InitializeComponent();
             items = LoadData();
            ItemPreSelect.Clear();
            if (app.roundRecord == null)
            {
                RevisitCheckBox.IsEnabled = false;
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
                DptName = "內視鏡",
                Key = "examination"
            });
            items.Add(new RgRecord
            {
                _waypointID = new Guid(),
                _regionID = new Guid(),
                _waypointName = "X光",
                DptName = "X光",
                Key = "examination"
            });
            items.Add(new RgRecord
            {
                _regionID = new Guid(),
                _waypointID = new Guid(),
                _waypointName = "超音波",
                DptName = "超音波",
                Key = "examination"
            });


            return items;
        }

        async private void AddCancelPopBtn_Clicked(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PopAsync();
        }

        private async void AddOKPopBtn_Clicked(object sender, EventArgs e)
        {
            int index =(app.roundRecord==null)?(app.records.Count):(app.records.IndexOf(app.roundRecord)+1);
            var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
            if (ItemPreSelect.Count > 0 || RevisitCheckBox.IsChecked)
            {
                foreach (RgRecord o in ItemPreSelect)
                {
                    app.records.Insert(index++, new RgRecord
                    {
                        _regionID = o._regionID,
                        _waypointID = o._waypointID,
                        _waypointName = o._waypointName,
                        Key = "AddItem",
                        DptName = o.DptName
                    });

                }

                if (RevisitCheckBox.IsChecked)
                    app.records.Insert(index,new RgRecord
                    {
                        _regionID=app.roundRecord._regionID,
                        _waypointID=app.roundRecord._waypointID,
                        _waypointName=app.roundRecord._waypointName,
                        Key="AddItem",
                        DptName=string.Format("回診({0})",app.roundRecord.DptName)
                    });

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
            AddRemovePreSelectItem(o.IsChecked, destinationName);
        }
        private void AddRemovePreSelectItem(bool isChecked, string destinationName)
        {
            
         /*   if (destinationName.Equals("回診") || destinationName.Equals("revisit"))
            {
                string str = string.Format("回診({0})", app.roundRecord.DptName);
                RgRecord record = new RgRecord
                {
                    DptName=str,
                    _regionID=app.roundRecord._regionID,
                    _waypointID=app.roundRecord._waypointID,
                    Key="AddItem",
                    _waypointName=app.roundRecord._waypointName
                };

                if (isChecked)
                {

                    ItemPreSelect.Add(record);
                }
                else
                    foreach (RgRecord o in ItemPreSelect)
                    {
                        if (o.DptName.Equals(str))
                        {
                            ItemPreSelect.Remove(o);
                            break;
                        }
                    }
            }
            else*/
            {
                foreach (RgRecord item in items)
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