﻿using IndoorNavigation.Models;
using IndoorNavigation.Models.NavigaionLayer;
using IndoorNavigation.Modules.Utilities;
using IndoorNavigation.Resources.Helpers;
using IndoorNavigation.ViewModels;
using IndoorNavigation.Views.Navigation;
using Plugin.Multilingual;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.ComponentModel;

namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RigisterList : CustomToolbarContentPage
    {
        RegisterListViewModel _viewmodel;
        private string _navigationGraphName;

        public ResourceManager _resourceManager = new ResourceManager
            (resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);
        private XMLInformation _nameInformation;

        Object ShifitTmp=null;
        App app = (App)Application.Current;
        private bool isButtonPressed = false; //to prevent button multi-tap from causing error
        private ViewCell lastCell=null;
        const string resourceId = "IndoorNavigation.Resources.AppResources";
        private HttpRequest request;
        CultureInfo currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
        PhoneInformation phoneInformation;
        //-----------for network check----------------------------
        INetworkSetting NetworkSettings;
        delegate void MulitItemFinish(RgRecord FinishRecord);
        MulitItemFinish _multiItemFinish;
        #region part of delegate implement
        delegate void LoadFiles(string buildingName);
        delegate void FinishItem(string buildingName);

        LoadFiles _loadFiles;
        FinishItem _finishItem;

        private void Init()
        {
            switch (_navigationGraphName)
            {
                case "員林基督教醫院":
                case "Yuanlin Christian Hospital":
                    _loadFiles = CCH_loadFiles;
                    _finishItem = CCH_FinishItem;
                    break;
                case "NTUH Yunlin Branch":
                case "台大醫院雲林分院":
                    _loadFiles = NTUH_Yunlin_loadFiles;
                    _finishItem = NTUH_Yunlin_FinishItem;
                    break;
                default:
                    throw new Exception();
                    
            }
               
        }
        //CCH part
        private void CCH_loadFiles(string buildingname)
        {
            Console.WriteLine("it's calling cch load files function....");            
        } 
        private void CCH_FinishItem(string buildingname)
        {
            Console.WriteLine("it's calling cch finishitem function....");
        }
        //NTUH part
        private void NTUH_Yunlin_loadFiles(string buildingname)
        {
            Console.WriteLine("it's calling NTUH_Yunlin load files function");
        }
        private void NTUH_Yunlin_FinishItem(string buildingname)
        {
            Console.WriteLine("it's calling NTUH_Yunlin Finish item function");
        }
        #endregion
        public RigisterList(string navigationGraphName)
        {
            InitializeComponent();          
            Console.WriteLine("initalize graph info");
            phoneInformation = new PhoneInformation();
            _navigationGraphName = navigationGraphName;      
            _nameInformation = NavigraphStorage.LoadInformationML(phoneInformation.GiveCurrentMapName(_navigationGraphName) + "_info_" + phoneInformation.GiveCurrentLanguage() + ".xml");

            Console.WriteLine("initialize http request");
            request = new HttpRequest();
            NetworkSettings = DependencyService.Get<INetworkSetting>();
            PaymemtListBtn.IsEnabled = (app.FinishCount + 1 == app.records.Count);
            PaymemtListBtn.IsVisible = (app.FinishCount + 1 == app.records.Count);

            BindingContext = _viewmodel;

        } 
      
        /*this function is to push page to NavigatorPage */
        async private void RgListView_ItemTapped(object sender, ItemTappedEventArgs e)  
        {
            if (isButtonPressed) return;
            isButtonPressed = true;
            if (e.Item is RgRecord record)
            {       
                if(record.Key.Equals("Pharmacy") && !app.lastFinished.Key.Equals("Cashier"))
                {    
                    await PopupNavigation.Instance.PushAsync(new DisplayAlertPopupPage(_resourceManager.GetString("PHARMACY_ALERT_STRING", currentLanguage)));
                    ((ListView)sender).SelectedItem = null;
                    isButtonPressed = false;
                    return;
                }                               
                await Navigation.PushAsync(new NavigatorPage(_navigationGraphName, record._regionID, record._waypointID, record._waypointName, _nameInformation));
                record.isComplete = true;
            }
            ((ListView)sender).SelectedItem = null;
            RgListView.ItemsSource = null;
            RgListView.ItemsSource = app.records;
        }

        /*this function is to implement a simply shift function.  
          when shift button is clicked, the function will become the listview tapped event.*/
        private void RgListViewShift_ItemTapped(object sender,ItemTappedEventArgs e)
        {
                if (ShifitTmp == null)
                {
                    ShifitTmp = e.Item as RgRecord;
                }
                else
                {
                    var o = e.Item as RgRecord;
                   
                    int index1 = app.records.IndexOf(ShifitTmp as RgRecord);
                    int index2 = app.records.IndexOf(o as RgRecord);
                    //swap
                    app.records[index1] = o as RgRecord;
                    app.records[index2] = ShifitTmp as RgRecord;
                    // retrieve original function.
                    RgListView.ItemTapped -= RgListViewShift_ItemTapped;
                    RgListView.ItemTapped += RgListView_ItemTapped;
                    ShifitTmp = null;
                    Buttonable(true);
                }  
        }

        /* the function is a button event which is to change listview tapped event*/
         async private void ShiftBtn_Clicked(object sender, EventArgs e)
        {
            bool isCheck = Preferences.Get("isCheckedNeverShow", false); 
            if (app.FinishCount+1 >= app.records.Count - 1)
            {
                await PopupNavigation.Instance.PushAsync(new DisplayAlertPopupPage(_resourceManager.GetString("NO_SHIFT_STRING", currentLanguage)));
                return;
            }
            else
            {
                if (!isCheck)
                {
                    //await PopupNavigation.Instance.PushAsync(new ShiftAlertPopupPage());
                    await PopupNavigation.Instance.PushAsync(new ShiftAlertPopupPage(_resourceManager.GetString("SHIFT_DESCRIPTION_STRING",currentLanguage),
                        _resourceManager.GetString("OK_STRING",currentLanguage), "isCheckedNeverShow"));
                }
                RgListView.ItemTapped -= RgListView_ItemTapped;

                RgListView.ItemTapped += RgListViewShift_ItemTapped;

                Buttonable(false);
            }
            
        }

        /*the function is to disable those two float button to keep from triggering something wrong.*/
        private void Buttonable(bool enable)
        {
            ShiftBtn.IsEnabled = enable;
            ShiftBtn.IsVisible = enable;
            AddBtn.IsEnabled = enable;
            AddBtn.IsVisible = enable;
            return;
        }

        /*the function is a button event, which could push page to SignInPage*/
        async private void SignInItem_Clicked(object sender, EventArgs e)
        {
            if (isButtonPressed) return;

            isButtonPressed = true;
            await Navigation.PushAsync(new SignInPage());
            MessagingCenter.Subscribe<AskRegisterPopupPage, bool>(this, "isReset", (msgSender, msgArgs) =>
            {
                PaymemtListBtn.IsEnabled = (app.FinishCount + 1 == app.records.Count);
                PaymemtListBtn.IsVisible = (app.FinishCount + 1 == app.records.Count);

                Buttonable(true);
                MessagingCenter.Unsubscribe<AskRegisterPopupPage,bool>(this, "isRest");
            });
        }

        /*the function is a button event to add payment and medicine recieving route to listview*/
        async private void PaymemtListBtn_Clicked(object sender, EventArgs e)
        {
            Buttonable(false);    
            if (isButtonPressed) return;
            isButtonPressed = true;
                PaymemtListBtn.IsEnabled = false;
            PaymemtListBtn.IsVisible = false;
            await PopupNavigation.Instance.PushAsync(new PickCashierPopupPage());
            MessagingCenter.Subscribe<PickCashierPopupPage, bool>(this, "GetCashierorNot", (Messagesender, Messageargs) =>
            {
                bool Message = (bool)Messageargs;
                if (Message) Buttonable(Message);
                PaymemtListBtn.IsEnabled = Message;
                PaymemtListBtn.IsVisible = Message;
                isButtonPressed = false;

                MessagingCenter.Unsubscribe<PickCashierPopupPage, bool>(this, "GetCashierorNot");
            });
            MessagingCenter.Subscribe<PickCashierPopupPage, bool>(this, "isBack", (messagesender, args) => 
            {
                Console.WriteLine("isBack recieve the message~");
                isButtonPressed = false;
                MessagingCenter.Unsubscribe<PickCashierPopupPage, bool>(this, "isBack");
            });          
        }

        /*to show popup page for add route to listview*/
        async private void AddBtn_Clicked(object sender, EventArgs e)
        {
            if (isButtonPressed) return;

            isButtonPressed = true;
            PaymemtListBtn.IsEnabled = false;
            PaymemtListBtn.IsVisible = false;
            await PopupNavigation.Instance.PushAsync(new AddPopupPage_v2(_navigationGraphName));
            //await Navigation.PushPopupAsync(new AddPopupPage());
            MessagingCenter.Subscribe<AddPopupPage_v2,bool>(this, "AddAnyOrNot",(Messagesender,Messageargs)=> 
            {
                bool Message = (bool)Messageargs;
                if (Message == false) app.HaveCashier = false;
                PaymemtListBtn.IsEnabled = Message;
                PaymemtListBtn.IsVisible = Message;

                MessagingCenter.Unsubscribe<AddPopupPage_v2, bool>(this, "AddAnyOrNot");
            });

            MessagingCenter.Subscribe<AddPopupPage_v2, bool>(this, "isBack", (MessageSender, MessageArgs) => 
            {
                isButtonPressed = false;
                MessagingCenter.Unsubscribe<AddPopupPage_v2, bool>(this, "isBack");
            });
        }
        
        /*to refresh listview Template and check whether user have sign in or not.*/
        protected override void OnAppearing()
        {      
            base.OnAppearing();
           
            _viewmodel = new RegisterListViewModel();
            //to refresh listview template 
            RgListView.ItemsSource = null;      
            RgListView.ItemsSource = app.records;
            ShiftBtn.CornerRadius = (int)(ShiftBtn.Height / 2);
            AddBtn.CornerRadius = (int)(AddBtn.Height / 2);

            if (app.HaveCashier && ! PaymemtListBtn.IsEnabled) Buttonable(false);

            PaymemtListBtn.IsEnabled = (app.FinishCount + 1 == app.records.Count);
            PaymemtListBtn.IsVisible = (app.FinishCount + 1 == app.records.Count);
            isButtonPressed = false;
            RefreshToolbarOptions();
        }
        /*this function is a button event, which is to check user whether have arrive at destination.*/
        async private void YetFinishBtn_Clicked(object sender, EventArgs e)
        {
            var o = (Button)sender;
            var FinishBtnClickItem= o.CommandParameter as RgRecord;
            if (FinishBtnClickItem != null)
            {
                switch (FinishBtnClickItem.Key)
                {
                    case "register":
                        _multiItemFinish = RegisterFinish;
                        break;
                    case "exit":
                        _multiItemFinish = ExitFinish;
                        break;
                    case "QueryResult":
                        _multiItemFinish = QueryResultFinish;
                        break;
                    default:
                        break;
                }

                _multiItemFinish(FinishBtnClickItem);


                if (app.FinishCount + 1 == app.records.Count)
                {
                    if(app.HaveCashier && !PaymemtListBtn.IsEnabled)
                    {
                        await DisplayAlert(_resourceManager.GetString("MESSAGE_STRING", currentLanguage), _resourceManager.GetString("FINISH_SCHEDULE_STRING", currentLanguage), _resourceManager.GetString("OK_STRING", currentLanguage));
                        await PopupNavigation.Instance.PushAsync(new ExitPopupPage(_navigationGraphName));
                    }else if (!app.HaveCashier)
                    {
                        PaymemtListBtn.IsEnabled = true;
                        PaymemtListBtn.IsVisible = true;
                        app.HaveCashier = true;
                    }
                }
            }
            #region temporary
            //if (index.Key.Equals("register"))
            //{
            //    var NetworkConnect = NetworkSettings.CheckInternetConnect();

            //    if (NetworkConnect)
            //    {
            //        _viewmodel.Isbusy = true;
            //        ReadXml();
            //        _viewmodel.Isbusy = false;
            //    }
            //    else
            //    {
            //        //await PopupNavigation.Instance.PushAsync(new AlertDialogPopupPage("you have a bad network now or you don't turn on the network. would you want to go to setting page?","yes","))
            //        var BadNetworkChecked=await DisplayAlert("info","You have a bad network or you don't turn on the network. would you want to go to setting page?", "yes", "no");
            //        if (BadNetworkChecked)
            //        {   
            //            NetworkSettings.OpenSettingPage();
            //            return;
            //        }
            //        else
            //        {
            //            await Navigation.PopToRootAsync();
            //            return;
            //        }

            //    }            

            //    ReadXml();

            //    index.isAccept = true;
            //    index.isComplete = true;
            //    app.FinishCount++;
            //    RgListView.ItemsSource = null;
            //    RgListView.ItemsSource = app.records;       
            //    return;
            //}
            //else if (index.Key.Equals("exit"))
            //{
            //    //show msg to say goodbye
            //    string s = string.Format("{0}\n{1}", phoneInformation.GetBuildingName(_navigationGraphName),
            //        _resourceManager.GetString("HOPE_STRING", currentLanguage));
            //    await PopupNavigation.Instance.PushAsync(new DisplayAlertPopupPage(s,false));
            //    await Navigation.PopAsync();
            //    index.isAccept = true;
            //    index.isComplete = true;
            //    RgListView.ItemsSource = null;
            //    RgListView.ItemsSource = app.records;
            //    return;
            //}
            //else if(index.Key.Equals("QueryResult"))
            //{
            //    app.roundRecord = index;
            //}

            //if(index != null)   // click finish button , it will refresh certain cell template
            //{
            //    index.isComplete = true;
            //    index.isAccept = true;
            //    app.FinishCount++;
            //    RgListView.ItemsSource = null;
            //    RgListView.ItemsSource = app.records;
            //}

            //if (app.FinishCount+1 == (app.records.Count)) //when all item is finished, enable pay/get medicine button
            //{
            //    if (app.HaveCashier && !PaymemtListBtn.IsEnabled)
            //    {
            //        await DisplayAlert(_resourceManager.GetString("MESSAGE_STRING", currentLanguage),_resourceManager.GetString("FINISH_SCHEDULE_STRING",currentLanguage),
            //            _resourceManager.GetString("OK_STRING", currentLanguage));

            //        await PopupNavigation.Instance.PushAsync(new ExitPopupPage(_navigationGraphName));
            //    }
            //    else
            //    {
            //        PaymemtListBtn.IsVisible = true;
            //        PaymemtListBtn.IsEnabled = true;
            //        app.HaveCashier = true;
            //    }
            //}
            //app.lastFinished = index;
            #endregion
        }

        private void ItemFinishFunction(RgRecord record)
        {
            record.isAccept = true;
            record.isComplete = true;
           
            app.FinishCount++;
            app.lastFinished = record;

            //to refresh listview to make sure template is work.
            RgListView.ItemsSource = null;
            RgListView.ItemsSource = app.records;
        }

        private void ViewCell_Tapped(object sender, EventArgs e)
        {
            if (lastCell != null)
            {
                lastCell.View.BackgroundColor = Color.Transparent;
            }
            var viewCell = (ViewCell)sender;
            if (viewCell.View != null)
            {
                viewCell.View.BackgroundColor = Color.FromHex("FFFF88");
            }
        }

        private void MenuItem_Clicked(object sender, EventArgs e)
        {
            var item = (RgRecord)((MenuItem)sender).CommandParameter;
            if (item != null)
                app.records.Remove(item);
            

        }
        //to load test data
        //private void ReadXml(int i)
        //{
        //    int index = app.records.Count - 1;
        //    app.FinishCount = 0;
        //    RgRecord record1 = new RgRecord
        //    {
        //        DptName = "耳鼻喉科",
        //        DrName = "李曉華",
        //        _waypointID = new Guid("11111111-1111-1111-1111-111111111111"),
        //        _regionID = new Guid("11111111-1111-1111-1111-111111111111"),
        //        Key = "QueryResult",
        //        Shift = "上午",
        //        CareRoom = "243診",
        //        SeeSeq = "17",
        //        _waypointName = "243診"
        //    };
        //    RgRecord record2 = new RgRecord
        //    {
        //        DptName = "家庭醫學科",
        //        DrName = "張曉明",
        //        _waypointID = new Guid("11111111-1111-1111-1111-111111111111"),
        //        _regionID = new Guid("11111111-1111-1111-1111-111111111111"),
        //        Key = "QueryResult",
        //        Shift = "上午",
        //        CareRoom = "203診",
        //        SeeSeq = "6",
        //        _waypointName = "203診"
        //    };
        //    app.records.Insert(index++, record2);
        //    app.records.Insert(index, record1);

        //    app._TmpRecords.Add(record2);
        //    app._TmpRecords.Add(record1);
            
        //    RgListView.ItemsSource = null;
        //    RgListView.ItemsSource = app.records;
        //}

        private void ReadXml()
        {
            Console.WriteLine("Now Excution is::: ReadXml");
            //_loadFiles(_navigationGraphName);
            Console.WriteLine("Now Excution is::: Todo request to server");
            request.GetXMLBody();
            request.RequestData();
            
            RgListView.ItemsSource = null;
            RgListView.ItemsSource = app.records;
        }

        async private void RegisterFinish(RgRecord record)
        {
            //this part might happend bugs
            bool NetworkConnectAbility = NetworkSettings.CheckInternetConnect();

            if (NetworkConnectAbility)
            {
                _viewmodel.Isbusy = true;
                ReadXml();
                _viewmodel.Isbusy = false;
                ItemFinishFunction(record);
            }
            else
            {
                var CheckWantToSetting = await DisplayAlert("info", "You have a bad network or you don't turn on the network. would you want to go to setting page?", "yes", "no");
                if (CheckWantToSetting)
                {
                    NetworkSettings.OpenSettingPage();
                    return;
                }
                else
                {
                    await Navigation.PopToRootAsync();
                    return;
                }
            }
        }
        async private void ExitFinish(RgRecord record)
        {
            string HopeString = string.Format($"{phoneInformation.GetBuildingName(_navigationGraphName)}\n{_resourceManager.GetString("HOPE_STRING",currentLanguage)}");
            await PopupNavigation.Instance.PushAsync(new AlertDialogPopupPage(HopeString));
            await Navigation.PopAsync();
            app.FinishCount--;
            ItemFinishFunction(record);
        }
        private void QueryResultFinish(RgRecord record)
        {
            app.roundRecord = record;
            ItemFinishFunction(record);
        }
        #region iOS secondary toolbaritem implement
        public override event EventHandler ToolbarItemAdded;
        //public ICommand Item1Command { get; set; }
        public ICommand SignInCommand { get; set; }
        public ICommand InfoItemCommand { get; set; }
        public ICommand TestItemCommand { get; set; }
        private void RefreshToolbarOptions()
        {
            //var viewModel = BindingContext as RegisterListViewModel;

            //Item1Command = new Command(async () => await Item1CommandMethod());
            SignInCommand = new Command(async () => await SignInItemMethod());
            InfoItemCommand = new Command(async () => await InfoItemMethod());
            TestItemCommand = new Command(async () => await TestItemMethod());
            ToolbarItems.Clear();

            //if (_viewmodel != null)
            {
                ToolbarItem SignInItem = new ToolbarItem { Text = _resourceManager.GetString("ACCOUNT_STRING", currentLanguage), Command=SignInCommand, Order = ToolbarItemOrder.Secondary };
                ToolbarItem InfoItem = new ToolbarItem { Text = _resourceManager.GetString("PREFERENCES_STRING", currentLanguage), Command =InfoItemCommand, Order = ToolbarItemOrder.Secondary };
                ToolbarItem TestItem = new ToolbarItem { Text = "test", Command = TestItemCommand, Order = ToolbarItemOrder.Secondary };
                ToolbarItems.Add(SignInItem);
                ToolbarItems.Add(InfoItem);
                ToolbarItems.Add(TestItem);
                OnToolbarItemAdded();
            }
        }
        
        private async Task TestItemMethod()
        {
            Console.WriteLine("Test Item click");

            INetworkSetting setting = DependencyService.Get<INetworkSetting>();
            Console.WriteLine("Setting is ready");
            //setting.OpenSettingPage();
            Console.WriteLine("Finish call setting");
            //setting.CheckInternetConnect();
            //Console.WriteLine($"Finish call check internet connect, result is {setting.CheckInternetConnect()}");

            ActivityIndicator indicator = new ActivityIndicator();
            indicator.Color = Color.FromHex("#3f51b5");

            RigisterListAbsoluteLayout.Children.Add(indicator);
            
            indicator.IsRunning = true;
            
            await Task.CompletedTask;
        }
        private async Task SignInItemMethod()
        {
            await Navigation.PushAsync(new SignInPage());

            MessagingCenter.Subscribe<AskRegisterPopupPage, bool>(this, "isReset", (msgSender, msgArgs) =>
            {
                PaymemtListBtn.IsEnabled = (app.FinishCount + 1 == app.records.Count);
                PaymemtListBtn.IsVisible = (app.FinishCount + 1 == app.records.Count);

                Buttonable(true);
                MessagingCenter.Unsubscribe<AskRegisterPopupPage, bool>(this, "isRest");
            });

            await Task.CompletedTask;
        }
        private async Task InfoItemMethod()
        {
            Console.WriteLine("Preference item click");
            await Navigation.PushAsync(new NavigatorSettingPage());
            await Task.CompletedTask;
        }
        protected void OnToolbarItemAdded()
        {
            Console.WriteLine("call onToolbarItemAdded");
            var e = ToolbarItemAdded;
            e?.Invoke(this, new EventArgs());
        }
        public override Color CellBackgroundColor => Color.White;
        public override Color CellTextColor => Color.Black;
        public override Color MenuBackgroundColor => Color.White;
        public override float RowHeight => 56;
        public override Color ShadowColor => Color.Black;
        public override float ShadowOpacity => 0.3f;
        public override float ShadowRadius => 5.0f;
        public override float ShadowOffsetDimension => 5.0f;
        public override float TableWidth => 250;
        #endregion
    }
}