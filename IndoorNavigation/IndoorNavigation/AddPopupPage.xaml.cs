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
using System.Collections.ObjectModel;
using Plugin.InputKit.Shared.Controls;
using Plugin.Multilingual;
using IndoorNavigation.Models.NavigaionLayer;
namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddPopupPage : PopupPage
    {
        App app = (App)Application.Current;
        const string _resourceId = "IndoorNavigation.Resources.AppResources";
        ResourceManager _resourceManager =
            new ResourceManager(_resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);
        string _graphName;

        List<RgRecord> AddItems;
        ObservableCollection<CheckBox> CheckBoxes;
        CheckBox RevisitBox; //it need to be deal specially.
        PhoneInformation _phoneInformation;
        
        public AddPopupPage()
        {   
            InitializeComponent();
            BackgroundColor = Color.FromRgba(150, 150, 150, 70);
            CheckBoxes = new ObservableCollection<CheckBox>();
        }
        public AddPopupPage(String graphName)
        {
            InitializeComponent();
            BackgroundColor = Color.FromRgba(150, 150, 150, 70);
            CheckBoxes = new ObservableCollection<CheckBox>();

            _phoneInformation = new PhoneInformation();
            _graphName = graphName;
            
            
        }
        private List<RgRecord> LoadItems()
        {
            List<RgRecord> items = new List<RgRecord>();
            items.Add(new RgRecord
            {
                _regionID = new Guid("11111111-1111-1111-1111-111111111111"),
                _waypointID = new Guid("00000000-0000-0000-0000-000000000002"),
                _waypointName = "內視鏡",
                DptName = "內視鏡",
                Key = "AddItem"
            });
            items.Add(new RgRecord
            {
                _regionID = new Guid("11111111-1111-1111-1111-111111111111"),
                _waypointID = new Guid("00000000-0000-0000-0000-000000000002"),
                _waypointName = "X光",
                DptName = "X光",
                Key = "AddItem"
            });
            items.Add(new RgRecord
            {
                _regionID = new Guid("11111111-1111-1111-1111-111111111111"),
                _waypointID = new Guid("00000000-0000-0000-0000-000000000002"),
                _waypointName = "超音波",
                DptName = "超音波",
                Key = "AddItem"
            });
            items.Add(new RgRecord
            {
                _regionID = new Guid("11111111-1111-1111-1111-111111111111"),
                _waypointID = new Guid("00000000-0000-0000-0000-000000000002"),
                _waypointName = "抽血處",
                DptName = "抽血處",
                Key = "AddItem"
            });
            //items.Add(new RgRecord
            //{
            //    _regionID = new Guid("11111111-1111-1111-1111-111111111111"),
            //    _waypointID = new Guid("00000000-0000-0000-0000-000000000002"),
            //    DptName = "檢查室",
            //    _waypointName = "檢察室",
            //    Key = "AddItem"
            //});
            return items;
        }
        private void AddCheckBox(List<RgRecord> items)
        {

            int i = 0;// for checkbox id
            foreach(RgRecord item in items)
            {
                CheckBox box = new CheckBox
                {
                    Key = i++,
                    Text = item.DptName,
                    IsChecked=false,
                    IsEnabled=true,
                    TextFontSize=24,
                    Type=CheckBox.CheckType.Check,
                    Color=Color.Black,
                    TextColor=Color.Black
                };
                box.CheckChanged +=CheckBox_Changed;
                CheckBoxStackLayout.Children.Add(box);
                CheckBoxes.Add(box); //to get box group                                         
            }

            RevisitBox = new CheckBox
            {
                Text = "回診",
                IsChecked = false,
                IsEnabled = true,
                TextFontSize = 26,
                Type = CheckBox.CheckType.Check,
                Color=Color.Black,
                TextColor = Color.Black
            };
            RevisitBox.CheckChanged += RevisitBox_Changed;
            CheckBoxStackLayout.Children.Add(RevisitBox);

        }
        async private void RevisitBox_Changed(object sender,EventArgs e)
        {
            var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
            var o = (CheckBox)sender;
            if (app.roundRecord == null)
            {
                await DisplayAlert(_resourceManager.GetString("MESSAGE_STRING",currentLanguage), _resourceManager.GetString("NO_REVISIT_RECORD_STRING", currentLanguage)
                    , _resourceManager.GetString("OK_STRING",currentLanguage));
                
                o.IsChecked = true;
            }
        }

        private void CheckBox_Changed(object sender, EventArgs e)
        {
            var o = (Plugin.InputKit.Shared.Controls.CheckBox)sender;
            Console.WriteLine("item:{0} was tapped\n key={1}", o.Text, o.Key);
        }

        protected override void OnAppearing()
        {
            AddItems = LoadItems();
            AddCheckBox(AddItems);
            base.OnAppearing();
        }

        private void AddItemCancelBtn_Clicked(object sender, EventArgs e)
        {
            bool AllFinisihed;

            if (app.FinishCount + 1 == app.records.Count)
                AllFinisihed = true;
            else
                AllFinisihed = false;

            GobackPage(AllFinisihed);
        }

        async private void AddItemOKBtn_Clicked(object sender, EventArgs e)
        {
            int index = (app.roundRecord == null && !RevisitBox.IsChecked) ? (app.records.Count - 1) : (app.records.IndexOf(app.roundRecord) + 1);
            int count = 0;
            var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
            foreach (CheckBox box in CheckBoxes)
            {
                if (!box.IsChecked) continue;
                count++;
                var isDuplicate = app.records.Any(p => p.DptName == AddItems[box.Key].DptName && p.isAccept==false);
                if (isDuplicate) continue;

                app.records.Insert(index++,AddItems[box.Key]);
            }
            if (RevisitBox.IsChecked)
            {
                count++;
                app.records.Insert(index++,new RgRecord
                {
                    DptName=string.Format("回診({0})",app.roundRecord.DptName),
                    _regionID=app.roundRecord._regionID,
                    _waypointID=app.roundRecord._waypointID,
                    Key= "AddItem",
                    _waypointName=app.roundRecord._waypointName
                });
                app.roundRecord = null;
            }

            if(count==0)
            {
                await DisplayAlert(_resourceManager.GetString("MESSAGE_STRING", currentLanguage),_resourceManager.GetString("NO_SELECT_DESTINATION_STRING", currentLanguage)
                    ,_resourceManager.GetString("OK_STRING",currentLanguage));

                return;
            }
            GobackPage(false);
        }

        protected override bool OnBackgroundClicked()
        {
            bool AllFinisihed;

            if (app.FinishCount + 1 == app.records.Count)
                AllFinisihed = true;
            else
                AllFinisihed = false;
            GobackPage(AllFinisihed);
            return base.OnBackgroundClicked();
        }

        protected override bool OnBackButtonPressed()
        {
            bool AllFinisihed;

            if (app.FinishCount + 1 == app.records.Count)
                AllFinisihed = true;
            else
                AllFinisihed = false;

            GobackPage(AllFinisihed);
            return base.OnBackButtonPressed();
        }

        private void GobackPage(bool ConfirmOrCancel)
        {
            MessagingCenter.Send(this, "AddAnyOrNot", ConfirmOrCancel);
            MessagingCenter.Send(this, "isBack", true);
            PopupNavigation.Instance.PopAsync();
        }

        //const string _resourceId = "IndoorNavigation.Resources.AppResources";
        //ResourceManager _resourceManager =
        //    new ResourceManager(_resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);
        //App app = (App)Application.Current;
        //ObservableCollection<RgRecord> items;
        //List<RgRecord> ItemPreSelect = new List<RgRecord>();
        //public AddPopupPage()
        //{
        //    InitializeComponent();
        //     items = LoadData();
        //    ItemPreSelect.Clear();
        //    if (app.roundRecord == null)
        //    {
        //        RevisitCheckBox.IsEnabled = false;
        //    }
        //}
        //private ObservableCollection<RgRecord> LoadData() //to load test layout file
        //{
        //    ObservableCollection<RgRecord> items = new ObservableCollection<RgRecord>();
        //    items.Add(new RgRecord
        //    {
        //        _regionID = new Guid("11111111-1111-1111-1111-111111111111"),
        //        _waypointID = new Guid("00000000-0000-0000-0000-000000000002"),
        //        _waypointName = "內視鏡",
        //        DptName = "內視鏡",
        //        Key = "AddItem"
        //    });
        //    items.Add(new RgRecord
        //    {
        //        _regionID = new Guid("11111111-1111-1111-1111-111111111111"),
        //        _waypointID = new Guid("00000000-0000-0000-0000-000000000002"),
        //        _waypointName = "X光",
        //        DptName = "X光",
        //        Key = "AddItem"
        //    });
        //    items.Add(new RgRecord
        //    {
        //        _regionID = new Guid("11111111-1111-1111-1111-111111111111"),
        //        _waypointID = new Guid("00000000-0000-0000-0000-000000000002"),
        //        _waypointName = "超音波",
        //        DptName = "超音波",
        //        Key = "AddITem"
        //    });
        //    items.Add(new RgRecord
        //    {
        //        _regionID = new Guid("11111111-1111-1111-1111-111111111111"),
        //        _waypointID = new Guid("00000000-0000-0000-0000-000000000002"),
        //        _waypointName = "抽血處",
        //        DptName = "抽血處",
        //        Key = "AddItem"
        //    });
        //    items.Add(new RgRecord
        //    {
        //        _regionID = new Guid("11111111-1111-1111-1111-111111111111"),
        //        _waypointID = new Guid("00000000-0000-0000-0000-000000000002"),
        //        DptName="檢查室",
        //        _waypointName = "檢察室",
        //        Key = "AddItem"
        //    });

        //    return items;
        //}

        //async private void AddCancelPopBtn_Clicked(object sender, EventArgs e)
        //{
        //    await PopupNavigation.Instance.PopAsync();
        //}

        //private async void AddOKPopBtn_Clicked(object sender, EventArgs e)
        //{


        //    int index =(app.roundRecord==null)?(app.records.Count-1):(app.records.IndexOf(app.roundRecord)+1);
        //    var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;

        //    if (ItemPreSelect.Count > 0 || RevisitCheckBox.IsChecked)
        //    {
        //        foreach (RgRecord o in ItemPreSelect)
        //        {
        //            RgRecord DumplicateCheck = new RgRecord
        //            {
        //                _regionID = o._regionID,
        //                _waypointID = o._waypointID,
        //                _waypointName = o._waypointName,
        //                Key = "AddItem",
        //                DptName = o.DptName
        //            };


        //            var duplicated = app.records;

        //                app.records.Insert(index++, DumplicateCheck);
        //        }

        //        if (RevisitCheckBox.IsChecked)
        //        {
        //            RgRecord RevisitCheck = new RgRecord
        //            {
        //                _regionID = app.roundRecord._regionID,
        //                _waypointID = app.roundRecord._waypointID,
        //                _waypointName = app.roundRecord._waypointName,
        //                Key = "AddItem",
        //                DptName = string.Format("回診({0})", app.roundRecord.DptName)
        //            };
        //           // if (!app.records.Contains(RevisitChedck))
        //           //if(app.records.Any(p=>p.DptName.Equals(RevisitCheck.DptName) && p.Check)
        //                app.records.Insert(index++, RevisitCheck);                  
        //        }
        //        //  app.records.GroupBy(x => x.DptName).Select(x => x.First());
        //        //app.records = ToObservableCollection<RgRecord>(app.records.Distinct());
        //        //app.records = new ObservableCollection<RgRecord>(app.records.Distinct());



        //        await PopupNavigation.Instance.PopAsync();
        //    }
        //    else
        //    {
        //        await DisplayAlert(_resourceManager.GetString("MESSAGE_STRING", currentLanguage),
        //            _resourceManager.GetString("NO_SELECT_DESTINATION_STRING", currentLanguage), _resourceManager.GetString("OK_STRING", currentLanguage));
        //    }
        //}



        //     private void CheckBox_CheckChanged(object sender, EventArgs e)
        //     {
        //         var o = (CheckBox)sender;
        //         string destinationName = o.Text;
        //         AddRemovePreSelectItem(sender);
        //     }
        ///     async private void AddRemovePreSelectItem(object sender)
        //     {
        //         var o = (CheckBox)sender;
        //         {
        //             foreach (RgRecord item in items)
        //             {
        //                 if (o.Text.Equals(item.DptName))
        //                 {
        //                     var isDuplicate = app.records.Any(p => p.DptName == item.DptName);
        //                     if (isDuplicate)
        //                     {
        //                         o.IsChecked = false;
        //                         await DisplayAlert("訊息", "你已經選取重複的檢查。", "確定");
        //                     }
        //                     else
        //                     if (o.IsChecked)
        //                         ItemPreSelect.Add(item);
        //                     else
        //                         ItemPreSelect.Remove(item);
        //                 }
        //             }
        //         }
        //     }
    }
}