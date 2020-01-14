//using System;
//using System.Collections.Generic;
//using System.Text;
//using Xamarin.Forms;
//using IndoorNavigation.Models;
//using IndoorNavigation;
//using IndoorNavigation.ViewModels;
//using System.Resources;
//using IndoorNavigation.Models.NavigaionLayer;
//using IndoorNavigation.Resources.Helpers;
//using Plugin.Multilingual;
//using System.Globalization;
//using IndoorNavigation.Modules.Utilities;
//using Rg.Plugins.Popup.Services;
//using System.Threading.Tasks;
//using IndoorNavigation.Views.Navigation;
//using System.Windows.Input;
//using System.Reflection;

//namespace IndoorNavigation.Views.OPFM
//{
    
//    public partial class RegisterListPage:CustomToolbarContentPage
//    {        
//        RegisterListViewModel _viewmodel;
//        #region For Read Graph files
//        private string _navigationGraphName;
//        public ResourceManager _resourceManager = new ResourceManager(resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);
//        const string resourceId = "IndoorNavigation.Resources.AppResources";
//        private XMLInformation _nameInformation;

//        CultureInfo currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
//        PhoneInformation phoneInformation;
//        #endregion

//        private INetworkSetting _networkSetting;
//        private App app = (App)Application.Current;
//        private ViewCell lastCell = null;
//        private Object tmp = null;
//        private bool ButtonClicked=false;

//        public RegisterListPage(string buildingName)
//        {
//            //InitializeComponent();
//            Console.WriteLine("Construct page");
//            _navigationGraphName = buildingName;
//            _nameInformation = NavigraphStorage.LoadInformationML(phoneInformation.GiveCurrentMapName(_navigationGraphName) + "_info_" + phoneInformation.GiveCurrentLanguage() + ".xml");

//            _networkSetting = DependencyService.Get<INetworkSetting>();
//        }
//        protected override void OnAppearing()
//        {
//            base.OnAppearing();
//            _viewmodel = new RegisterListViewModel();
//            BindingContext = _viewmodel;
//        }
//        async private void RegisterListView_ItemTapped(object sender,ItemTappedEventArgs e) {
//            if (ButtonClicked) return;
//            ButtonClicked = true;

//            if(e.Item is RgRecord record)
//            {
//                int index = app.records.IndexOf(record);
//                if (record.Key.Equals("Pharmacy") && !app.lastFinished.Key.Equals("Cashier"))
//                {
//                    //show msg say: must finish cashier, or user couldn't go pharmacy
//                    ButtonClicked = true;
//                    return;
//                }

//            }
            
//        }
//        private void RegisterListView_ShiftItemTapped(object sender, ItemTappedEventArgs e) { }
//        async private void AddBtn_Click(object sender,EventArgs e) { }
//        async private void FinishBtn_Click(object sender,EventArgs e) { }
//        private void ViewCell_Tapped(object sender,EventArgs e) { }
//        private void MenuItem_Clicked(object sender, EventArgs e) { }
//        private void ReadXml(string buildingName) { }

//        #region iOS secondary toolbaritem implement
//        public override event EventHandler ToolbarItemAdded;
//        //public ICommand Item1Command { get; set; }
//        public ICommand SignInCommand { get; set; }
//        public ICommand InfoItemCommand { get; set; }
//        public ICommand TestItemCommand { get; set; }
//        private void RefreshToolbarOptions()
//        {
//            //var viewModel = BindingContext as RegisterListViewModel;

//            //Item1Command = new Command(async () => await Item1CommandMethod());
//            SignInCommand = new Command(async () => await SignInItemMethod());
//            InfoItemCommand = new Command(async () => await InfoItemMethod());
//            TestItemCommand = new Command(async () => await TestItemMethod());
//            ToolbarItems.Clear();

//            //if (_viewmodel != null)
//            {
//                ToolbarItem SignInItem = new ToolbarItem { Text = _resourceManager.GetString("ACCOUNT_STRING", currentLanguage), Command = SignInCommand, Order = ToolbarItemOrder.Secondary };
//                ToolbarItem InfoItem = new ToolbarItem { Text = _resourceManager.GetString("PREFERENCES_STRING", currentLanguage), Command = InfoItemCommand, Order = ToolbarItemOrder.Secondary };
//                ToolbarItem TestItem = new ToolbarItem { Text = "test", Command = TestItemCommand, Order = ToolbarItemOrder.Secondary };
//                ToolbarItems.Add(SignInItem);
//                ToolbarItems.Add(InfoItem);
//                ToolbarItems.Add(TestItem);
//                OnToolbarItemAdded();
//            }
//        }

//        private async Task TestItemMethod()
//        {
//            Console.WriteLine("Test Item click");

//            INetworkSetting setting = DependencyService.Get<INetworkSetting>();
//            Console.WriteLine("Setting is ready");
//            //setting.OpenSettingPage();
//            Console.WriteLine("Finish call setting");
//            //setting.CheckInternetConnect();
//            //Console.WriteLine($"Finish call check internet connect, result is {setting.CheckInternetConnect()}");

//            ActivityIndicator indicator = new ActivityIndicator();
//            indicator.Color = Color.FromHex("#3f51b5");

//            //RigisterListAbsoluteLayout.Children.Add(indicator);

//            indicator.IsRunning = true;

//            await Task.CompletedTask;
//        }
//        private async Task SignInItemMethod()
//        {
//            Console.WriteLine("Sign In Item click");
//            await Navigation.PushAsync(new SignInPage());

//            MessagingCenter.Subscribe<AskRegisterPopupPage, bool>(this, "isReset", (msgSender, msgArgs) =>
//            {
//                //PaymemtListBtn.IsEnabled = (app.FinishCount + 1 == app.records.Count);
//                //PaymemtListBtn.IsVisible = (app.FinishCount + 1 == app.records.Count);

//                //Buttonable(true);
//                MessagingCenter.Unsubscribe<AskRegisterPopupPage, bool>(this, "isRest");
//            });

//            await Task.CompletedTask;
//        }
//        private async Task InfoItemMethod()
//        {
//            Console.WriteLine("Preference item click");
//            await Navigation.PushAsync(new NavigatorSettingPage());
//            await Task.CompletedTask;
//        }

//        protected void OnToolbarItemAdded()
//        {
//            Console.WriteLine("call onToolbarItemAdded");
//            var e = ToolbarItemAdded;
//            e?.Invoke(this, new EventArgs());
//        }
//        public override Color CellBackgroundColor => Color.White;
//        public override Color CellTextColor => Color.Black;
//        public override Color MenuBackgroundColor => Color.White;
//        public override float RowHeight => 56;
//        public override Color ShadowColor => Color.Black;
//        public override float ShadowOpacity => 0.3f;
//        public override float ShadowRadius => 5.0f;
//        public override float ShadowOffsetDimension => 5.0f;
//        public override float TableWidth => 250;
//        #endregion
//    }
//}


////if (index.Key.Equals("register"))
////{
////    var NetworkConnect = NetworkSettings.CheckInternetConnect();

////    if (NetworkConnect)
////    {
////        _viewmodel.Isbusy = true;
////        ReadXml();
////        _viewmodel.Isbusy = false;
////    }
////    else
////    {
////        //await PopupNavigation.Instance.PushAsync(new AlertDialogPopupPage("you have a bad network now or you don't turn on the network. would you want to go to setting page?","yes","))
////        var BadNetworkChecked=await DisplayAlert("info","You have a bad network or you don't turn on the network. would you want to go to setting page?", "yes", "no");
////        if (BadNetworkChecked)
////        {   
////            NetworkSettings.OpenSettingPage();
////            return;
////        }
////        else
////        {
////            await Navigation.PopToRootAsync();
////            return;
////        }

////    }            

////    ReadXml();

////    index.isAccept = true;
////    index.isComplete = true;
////    app.FinishCount++;
////    RgListView.ItemsSource = null;
////    RgListView.ItemsSource = app.records;       
////    return;
////}
////else if (index.Key.Equals("exit"))
////{
////    //show msg to say goodbye
////    string s = string.Format("{0}\n{1}", phoneInformation.GetBuildingName(_navigationGraphName),
////        _resourceManager.GetString("HOPE_STRING", currentLanguage));
////    await PopupNavigation.Instance.PushAsync(new DisplayAlertPopupPage(s,false));
////    await Navigation.PopAsync();
////    index.isAccept = true;
////    index.isComplete = true;
////    RgListView.ItemsSource = null;
////    RgListView.ItemsSource = app.records;
////    return;
////}
////else if(index.Key.Equals("QueryResult"))
////{
////    app.roundRecord = index;
////}

////if(index != null)   // click finish button , it will refresh certain cell template
////{
////    index.isComplete = true;
////    index.isAccept = true;
////    app.FinishCount++;
////    RgListView.ItemsSource = null;
////    RgListView.ItemsSource = app.records;
////}

////if (app.FinishCount+1 == (app.records.Count)) //when all item is finished, enable pay/get medicine button
////{
////    if (app.HaveCashier && !PaymemtListBtn.IsEnabled)
////    {
////        await DisplayAlert(_resourceManager.GetString("MESSAGE_STRING", currentLanguage),_resourceManager.GetString("FINISH_SCHEDULE_STRING",currentLanguage),
////            _resourceManager.GetString("OK_STRING", currentLanguage));

////        await PopupNavigation.Instance.PushAsync(new ExitPopupPage(_navigationGraphName));
////    }
////    else
////    {
////        PaymemtListBtn.IsVisible = true;
////        PaymemtListBtn.IsEnabled = true;
////        app.HaveCashier = true;
////    }
////}
////app.lastFinished = index;