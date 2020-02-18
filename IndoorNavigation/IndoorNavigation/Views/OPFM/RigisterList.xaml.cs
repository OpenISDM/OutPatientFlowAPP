using IndoorNavigation.Models;
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
using System.Threading;
namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RigisterList : CustomToolbarContentPage
    {
        RegisterListViewModel _viewmodel;
        private string _navigationGraphName;

        #region for language change
        private const string resourceId = "IndoorNavigation.Resources.AppResources";
        CultureInfo currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
        private ResourceManager _resourceManager = new ResourceManager(resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);
        #endregion

        private XMLInformation _nameInformation;

        private Object ShiftTmp=null;
        private App app = (App)Application.Current;
        private bool isButtonPressed = false; //to prevent button multi-tap from causing error
        private bool ShiftButtonPressed = false;
        private ViewCell lastCell=null;

        private INetworkSetting NetworkSettings;

        PhoneInformation phoneInformation;        

        delegate void MulitItemFinish(RgRecord FinishRecord);
        MulitItemFinish _multiItemFinish;

        #region part of branch delegate implement, but it is not start to use.
        //delegate void LoadFiles(string buildingName);
        //delegate void FinishItem(string buildingName);

        //LoadFiles _loadFiles;
        //FinishItem _finishItem;

        //private void Init()
        //{
        //    switch (_navigationGraphName)
        //    {
        //        case "員林基督教醫院":
        //        case "Yuanlin Christian Hospital":
        //            _loadFiles = CCH_loadFiles;
        //            _finishItem = CCH_FinishItem;
        //            break;
        //        case "NTUH Yunlin Branch":
        //        case "台大醫院雲林分院":
        //            _loadFiles = NTUH_Yunlin_loadFiles;
        //            _finishItem = NTUH_Yunlin_FinishItem;
        //            break;
        //        default:
        //            throw new Exception();
                    
        //    }
               
        //}
        ////CCH part
        //private void CCH_loadFiles(string buildingname)
        //{
        //    Console.WriteLine("it's calling cch load files function....");            
        //} 
        //private void CCH_FinishItem(string buildingname)
        //{
        //    Console.WriteLine("it's calling cch finishitem function....");
        //}
        ////NTUH part
        //private void NTUH_Yunlin_loadFiles(string buildingname)
        //{
        //    Console.WriteLine("it's calling NTUH_Yunlin load files function");
        //}
        //private void NTUH_Yunlin_FinishItem(string buildingname)
        //{
        //    Console.WriteLine("it's calling NTUH_Yunlin Finish item function");
        //}
        #endregion
        public RigisterList(string navigationGraphName)
        {
            InitializeComponent();          
            Console.WriteLine("initalize graph info");
            phoneInformation = new PhoneInformation();
            _navigationGraphName = navigationGraphName;      
            _nameInformation = NavigraphStorage.LoadInformationML(phoneInformation.GiveCurrentMapName(_navigationGraphName) + "_info_" + phoneInformation.GiveCurrentLanguage() + ".xml");


            NetworkSettings = DependencyService.Get<INetworkSetting>();
            PaymemtListBtn.IsEnabled = (app.FinishCount + 1 == app.records.Count);
            PaymemtListBtn.IsVisible = (app.FinishCount + 1 == app.records.Count);

            BindingContext = _viewmodel;

        }
        #region ListView Tapped event
        /*this function is to push page to NavigatorPage */
        async private void RgListView_ItemTapped(object sender, ItemTappedEventArgs e)  
        {
            if (isButtonPressed) return;
            isButtonPressed = true;
            if (e.Item is RgRecord record)
            {                       
                if(record.type.Equals(RecordType.Pharmacy) && (app.lastFinished==null || !app.lastFinished.type.Equals(RecordType.Cashier)))
                {
                    await PopupNavigation.Instance.PushAsync(new AlertDialogPopupPage(_resourceManager.GetString("PHARMACY_ALERT_STRING", currentLanguage),
                        _resourceManager.GetString("OK_STRING",currentLanguage)));
                    RefreshListView();
                    ((ListView)sender).SelectedItem = null;
                    isButtonPressed = false;
                    return;
                }                               
                await Navigation.PushAsync(new NavigatorPage(_navigationGraphName,record._floor, record._regionID, record._waypointID, record._waypointName, _nameInformation));
                record.isComplete = true;
            }
            RefreshListView();
            ((ListView)sender).SelectedItem = null;
        }
       

        /*this function is to implement a simply shift function.  
          when shift button is clicked, the function will become the listview tapped event.*/
        private void RgListViewShift_ItemTapped(object sender,ItemTappedEventArgs e)
        {
                if (ShiftTmp == null)
                {
                    ShiftTmp = e.Item as RgRecord;
                }
                else
                {
                    var o = e.Item as RgRecord;
                   
                    int index1 = app.records.IndexOf(ShiftTmp as RgRecord);
                    int index2 = app.records.IndexOf(o as RgRecord);
                    //swap
                    app.records[index1] = o as RgRecord;
                    app.records[index2] = ShiftTmp as RgRecord;
                    // retrieve original function.
                    RgListView.ItemTapped -= RgListViewShift_ItemTapped;
                    RgListView.ItemTapped += RgListView_ItemTapped;
                    ShiftTmp = null;
                    ShiftButtonPressed = false;
                    Buttonable(true);
                }  
        }
        #endregion
        /* the function is a button event which is to change listview tapped event*/
        async private void ShiftBtn_Clicked(object sender, EventArgs e)
        {
            bool isCheck = Preferences.Get("isCheckedNeverShow", false); 
            if (app.FinishCount+1 >= app.records.Count - 1)
            {
                await PopupNavigation.Instance.PushAsync(new AlertDialogPopupPage(_resourceManager.GetString("NO_SHIFT_STRING", currentLanguage), _resourceManager.GetString("OK_STRING",currentLanguage)));
                return;
            }
            else
            {
                if (!isCheck)
                {
                    await PopupNavigation.Instance.PushAsync(new ShiftAlertPopupPage(_resourceManager.GetString("SHIFT_DESCRIPTION_STRING",currentLanguage),
                        _resourceManager.GetString("OK_STRING",currentLanguage), "isCheckedNeverShow"));
                }
                RgListView.ItemTapped -= RgListView_ItemTapped;
                RgListView.ItemTapped += RgListViewShift_ItemTapped;
                ShiftButtonPressed = true;
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
                PaymemtListBtn.IsEnabled = !(bool)Messageargs;
                PaymemtListBtn.IsVisible = !(bool)Messageargs;
                Buttonable(!(bool)Messageargs);

                isButtonPressed = false;
                MessagingCenter.Unsubscribe<PickCashierPopupPage, bool>(this, "GetCashierorNot");
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
 
            MessagingCenter.Subscribe<AddPopupPage_v2,bool>(this, "AddAnyOrNot",(Messagesender,Messageargs)=> 
            {
                
                var somethingchange = PaymemtListBtn.IsEnabled && PaymemtListBtn.IsVisible;
                PaymemtListBtn.IsEnabled = (app.FinishCount + 1 == app.records.Count);
                PaymemtListBtn.IsVisible = (app.FinishCount + 1 == app.records.Count);

                app.HaveCashier =! (somethingchange == (PaymemtListBtn.IsVisible && PaymemtListBtn.IsEnabled));

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
            RefreshListView();
            ShiftBtn.CornerRadius = (int)(Math.Min(ShiftBtn.Height,ShiftBtn.Width) / 2);
            AddBtn.CornerRadius = (int)(Math.Min(AddBtn.Height,AddBtn.Width) / 2);

            if (app.HaveCashier && ! PaymemtListBtn.IsEnabled) Buttonable(false);

            PaymemtListBtn.IsEnabled = (app.FinishCount + 1 == app.records.Count && !app.HaveCashier);
            PaymemtListBtn.IsVisible = (app.FinishCount + 1 == app.records.Count && !app.HaveCashier);
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
                switch (FinishBtnClickItem.type)
                {
                    case RecordType.Register:
                        _multiItemFinish = RegisterFinish;
                        break;
                    case RecordType.Exit:
                        _multiItemFinish = ExitFinish;
                        break;
                    case RecordType.Queryresult:
                        _multiItemFinish = QueryResultFinish;
                        break;
                    default:
                        _multiItemFinish = DefaultFinish;
                        break;
                }
                _multiItemFinish(FinishBtnClickItem);

                if (app.FinishCount + 1 == app.records.Count && app.lastFinished.type!= RecordType.Register)
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
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            if (ShiftButtonPressed)
            {
                ShiftTmp = null;
                RgListView.ItemTapped -= RgListViewShift_ItemTapped;
                RgListView.ItemTapped += RgListView_ItemTapped;
                Buttonable(true);
            }
        }       

        private void RemoveItem_Clicked(object sender, EventArgs e)
        {
            var item = (RgRecord)((MenuItem)sender).CommandParameter;
            if (item != null)
                app.records.Remove(item);
        }    

         async private Task ReadXml()
         {
            HttpRequest.GetXMLBody();
            await HttpRequest.RequestData();          
            RefreshListView();
         }

        #region UI View Control
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

                Device.StartTimer(TimeSpan.FromSeconds(1.5), () =>
                {
                    viewCell.View.BackgroundColor = Color.Transparent;
                    return false;
                });
            }
        }
        public void BusyIndicatorShow(bool isBusy)
        {
            BusyIndicator.IsEnabled = isBusy;
            BusyIndicator.IsRunning = isBusy;
            BusyIndicator.IsVisible = isBusy;
        }
        private void RefreshListView()
        {
            RgListView.ItemsSource = null;
            RgListView.ItemsSource = app.records;
        }
        #endregion

        #region Item Finished delegate fuctions
        private void ItemFinishFunction(RgRecord record)
        {
            record.isAccept = true;
            record.isComplete = true;

            app.FinishCount++;
            app.lastFinished = record;

            //to refresh listview to make sure template is work.
            RefreshListView();
        }

        async private void RegisterFinish(RgRecord record)
        {
            //this part might happend bugs
           
            BusyIndicatorShow(true);

            bool NetworkConnectAbility = await NetworkSettings.CheckInternetConnect();
            if (NetworkConnectAbility)
            {
                await ReadXml();
                ItemFinishFunction(record);
            }
            else
            {
                var CheckWantToSetting = await DisplayAlert(_resourceManager.GetString("MESSAGE_STRING",currentLanguage), _resourceManager.GetString("BAD_NETWORK_STRING",currentLanguage)
                    , _resourceManager.GetString("OK_STRING",currentLanguage), _resourceManager.GetString("NO_STRING",currentLanguage));
                BusyIndicatorShow(false);
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

            BusyIndicatorShow(false);
        }
        async private void ExitFinish(RgRecord record)
        {
            string HopeString = $"{phoneInformation.GetBuildingName(_navigationGraphName)}\n{_resourceManager.GetString("HOPE_STRING",currentLanguage)}";
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
        private void DefaultFinish(RgRecord record)
        {
            ItemFinishFunction(record);
        }
        #endregion


        #region iOS secondary toolbaritem implement

        public override event EventHandler ToolbarItemAdded;
       
        private void RefreshToolbarOptions()
        {       
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

            await Navigation.PushAsync(new FakeNavigatorPage(_navigationGraphName, new Guid("11111111-1111-1111-1111-111111111111"), new Guid("00000000-0000-0000-0000-000000000010"), "健檢中心", _nameInformation));
            //await Navigation.PushAsync(new FakeNavigatorPage(_navigationGraphName));
    
            Console.WriteLine("Finish call setting");
            
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
            EventHandler e = ToolbarItemAdded;
            e?.Invoke(this, new EventArgs());
        }

        #region ToolBarItemAttributes and Commands

        public ICommand SignInCommand { get; set; }
        public ICommand InfoItemCommand { get; set; }
        public ICommand TestItemCommand { get; set; }

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

        #endregion
    }
}