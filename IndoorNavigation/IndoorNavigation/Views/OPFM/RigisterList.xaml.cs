using IndoorNavigation.Models;
using IndoorNavigation.Models.NavigaionLayer;
using IndoorNavigation.Modules.Utilities;
using IndoorNavigation.Resources.Helpers;
using IndoorNavigation.ViewModels;
using IndoorNavigation.Views.Navigation;
using Plugin.Multilingual;
using Rg.Plugins.Popup.Services;
using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Collections.Generic;
using System.Xml;
using System.Linq;
using IndoorNavigation.Views.Controls;
using IndoorNavigation.Views.PopUpPage;

namespace IndoorNavigation.Views.OPFM
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RigisterList : CustomToolbarContentPage
    {
        #region variable declaration
        RegisterListViewModel _viewmodel;
        private string _navigationGraphName;        

        private XMLInformation _nameInformation;
        private App app = (App)Application.Current;

        Dictionary<Guid, DestinationItem> CashierPosition;
        Dictionary<Guid, DestinationItem> PharmacyPostition;

        //to prevent button multi-tap from causing error
        private bool isButtonPressed = false; 		
        private ViewCell lastCell=null;

        private INetworkSetting NetworkSettings;
        private HttpRequest request;
        PhoneInformation phoneInformation;        

        delegate void MulitItemFinish(RgRecord FinishRecord);
        MulitItemFinish _multiItemFinish;

        #endregion

        public RigisterList(string navigationGraphName)
        {
            InitializeComponent();          
            Console.WriteLine("initalize graph info");
            phoneInformation = new PhoneInformation();
            _navigationGraphName = navigationGraphName;
            request = new HttpRequest();

            _nameInformation = 
				NavigraphStorage.LoadInformationML
				(phoneInformation.GiveCurrentMapName(_navigationGraphName) + 
				 "_info_" + phoneInformation.GiveCurrentLanguage() + ".xml");

            NetworkSettings = DependencyService.Get<INetworkSetting>();

            PaymemtListBtn.IsEnabled = app.FinishCount == app.records.Count;
            PaymemtListBtn.IsVisible = app.FinishCount == app.records.Count;
            //RgListView.ScrollTo()
            LoadCashierData();
            BindingContext = _viewmodel;

        }
        /*this function is to push page to NavigatorPage */
        async private void RgListView_ItemTapped(object sender, 
												 ItemTappedEventArgs e)  
        {
            Console.WriteLine("the Record type is : "+((RgRecord)e.Item).type);
            if (isButtonPressed) return;
            isButtonPressed = true;

            if (e.Item is RgRecord record)
            {                       
                if(record.type.Equals(RecordType.Pharmacy) && 
				   (app.lastFinished==null || 
				   !app.lastFinished.type.Equals(RecordType.Cashier)))
                {
                    await PopupNavigation.Instance.PushAsync
						( new AlertDialogPopupPage
							(getResourceString("PHARMACY_ALERT_STRING"), 
							 getResourceString("OK_STRING")
							)
						);
                    RefreshListView();
                    ((ListView)sender).SelectedItem = null;
                    isButtonPressed = false;
                    return;
                }                               
                await Navigation.PushAsync
					(new NavigatorPage(_navigationGraphName, 
									   record._regionID, 
									   record._waypointID, 
									   record._waypointName, 
									   _nameInformation)
					);
                record.isComplete = true;
            }
            RefreshListView();
            ((ListView)sender).SelectedItem = null;
        }

        //the function is to control the button whether it is visible 
        private void Buttonable(bool enable)
        {
            AddBtn.IsEnabled = enable;
            AddBtn.IsVisible = enable;
        }    

        //the function is a button event to add payment and medicine recieving 
		//route to listview
        private void PaymemtListBtn_Clicked(object sender, EventArgs e)
        {
            Buttonable(false);    
            if (isButtonPressed) return;
            isButtonPressed = true;
            PaymemtListBtn.IsEnabled = false;
            PaymemtListBtn.IsVisible = false;
            app.HaveCashier = true;
            DestinationItem cashier, pharmacy;
            try
            {
                cashier= CashierPosition[app.lastFinished._regionID];
            }
            catch 
            {
                cashier = CashierPosition.First().Value;
            }
            try
            {
                pharmacy = PharmacyPostition[app.lastFinished._regionID];
            }
            catch
            {
                pharmacy = PharmacyPostition.First().Value;
            }
           
            app.records.Add(new RgRecord { 
                    _waypointID=cashier._waypointID,
                    _regionID=cashier._regionID,
                    _waypointName=cashier._waypointName,
                    type=RecordType.Cashier,
                    DptName=cashier._waypointName
                });
            app.records.Add(new RgRecord { 
                    _waypointID=pharmacy._waypointID,
                    _regionID=pharmacy._regionID,
                    _waypointName=pharmacy._waypointName,
                    type=RecordType.Pharmacy,
                    DptName=pharmacy._waypointName
                });

            RgListView.ScrollTo(app.records[app.records.Count - 1], ScrollToPosition.MakeVisible, true);
            isButtonPressed = false;           
        }
        
        public void LoadCashierData() 
        {
            CashierPosition = new Dictionary<Guid, DestinationItem>();
            PharmacyPostition = new Dictionary<Guid, DestinationItem>();
            XmlDocument doc = NavigraphStorage.XmlReader("Yuanlin_OPFM.CashierStation.xml");
            XmlNodeList CashiernodeList = doc.GetElementsByTagName("Cashierstation");
            XmlNodeList PharmacyNodeList = doc.GetElementsByTagName("Pharmacystation");
            foreach(XmlNode node in CashiernodeList)
            {
                DestinationItem item = new DestinationItem();

                item._regionID = new Guid(node.Attributes["region_id"].Value);
                item._waypointID = new Guid(node.Attributes["waypoint_id"].Value);
                item._floor =node.Attributes["floor"].Value;
                item._waypointName =node.Attributes["name"].Value;

                Console.WriteLine(item._waypointName +" region id:"+ item._regionID+ ", waypoint id: "+item._waypointID);

                CashierPosition.Add(new Guid(node.Attributes["region_id"].Value), item);
            }

            foreach(XmlNode node in PharmacyNodeList)
            {
                DestinationItem item = new DestinationItem();
                item._regionID = new Guid(node.Attributes["region_id"].Value);
                item._waypointID = new Guid(node.Attributes["waypoint_id"].Value);
                item._floor = node.Attributes["floor"].Value;
                item._waypointName = node.Attributes["name"].Value;
               
                PharmacyPostition.Add(new Guid(node.Attributes["region_id"].Value), item);
            }

            return;
        }

        /*to show popup page for add route to listview*/
        async private void AddBtn_Clicked(object sender, EventArgs e)
        {
            if (isButtonPressed) return;

            isButtonPressed = true;
            PaymemtListBtn.IsEnabled = false;
            PaymemtListBtn.IsVisible = false;
            await PopupNavigation.Instance.PushAsync
				(new AddPopupPage(_navigationGraphName));

            MessagingCenter.Subscribe<AddPopupPage, bool>(this, "isCancel", 
			  (Messagesender, Messageargs) =>
              {
                  PaymemtListBtn.IsEnabled = app.FinishCount == app.records.Count && !app.HaveCashier;
                  PaymemtListBtn.IsVisible = app.FinishCount==app.records.Count && !app.HaveCashier;
                isButtonPressed = false;
                MessagingCenter.Unsubscribe<AddPopupPage, bool>
					(this, "isCancel");                 
              });
          
        }
               

        // to refresh listview Template and check whether user have sign in or 
		// not.
        protected override void OnAppearing()
        {      
            base.OnAppearing();
           
            _viewmodel = new RegisterListViewModel(_navigationGraphName);

            RefreshListView();
    
            AddBtn.CornerRadius = 
				(int)(Math.Min(AddBtn.Height,AddBtn.Width) / 2);

            if (app.HaveCashier && ! PaymemtListBtn.IsEnabled) 
				Buttonable(false);

            PaymemtListBtn.IsEnabled = app.FinishCount == app.records.Count && app.HaveCashier;            
            PaymemtListBtn.IsVisible = app.FinishCount == app.records.Count && app.HaveCashier;

            if (app.lastFinished != null && !app.HaveCashier)
            {
                RgListView.ScrollTo(app.lastFinished, ScrollToPosition.MakeVisible, false);
            }
            else if(app.HaveCashier)
            {
                RgListView.ScrollTo(app.records[app.records.Count - 1], ScrollToPosition.MakeVisible, false);
            }

            isButtonPressed = false;
            RefreshToolbarOptions();
        }

        // this function is a button event, which is to check user whether have 
		// arrive at destination.
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
               
                if (app.FinishCount == app.records.Count &&
                app.lastFinished.type != RecordType.Register)
                {
                    if(app.HaveCashier && !PaymemtListBtn.IsEnabled)
                    {
                        await DisplayAlert(getResourceString("MESSAGE_STRING"),
									getResourceString("FINISH_SCHEDULE_STRING"), 
									getResourceString("OK_STRING"));
									
                        await PopupNavigation.Instance.PushAsync
							(new ExitPopupPage(_navigationGraphName));
                    }else if (!app.HaveCashier)
                    {
                        PaymemtListBtn.IsEnabled = true;
                        PaymemtListBtn.IsVisible = true;
                    }
                }
            }            
        }               

         async private Task ReadXml()
         {
            request.GetXMLBody();
            await request.RequestData();          
            RefreshListView();
         }

        private string getResourceString(string key)
        {
            string resourceId = "IndoorNavigation.Resources.AppResources";
            CultureInfo currentLanguage = 
				CrossMultilingual.Current.CurrentCultureInfo;
            ResourceManager resourceManager= 
				new ResourceManager(resourceId, 
									typeof(TranslateExtension)
									.GetTypeInfo().Assembly);

            return resourceManager.GetString(key, currentLanguage);
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

                //Device.StartTimer(TimeSpan.FromSeconds(1.5), () =>
                //{
                //    viewCell.View.BackgroundColor = Color.Transparent;
                //    return false;
                //});
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

            RefreshListView();
        }

        async private void RegisterFinish(RgRecord record)
        {
            //this part might happend bugs
           
            BusyIndicatorShow(true);
            Console.WriteLine("Register Finished");
            bool NetworkConnectAbility = 
				await NetworkSettings.CheckInternetConnect();
            if (NetworkConnectAbility)
            {
                await ReadXml();
                Console.WriteLine("ReadXml finished");
                ItemFinishFunction(record);
            }
            else
            {
                var CheckWantToSetting = 
					await DisplayAlert(getResourceString("MESSAGE_STRING"), 
									   getResourceString("BAD_NETWORK_STRING"),
									   getResourceString("OK_STRING"),
									   getResourceString("NO_STRING"));
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
            string HopeString = 
                $"{_navigationGraphName}"+
				$"\n{getResourceString("HOPE_STRING")}";
            await PopupNavigation.Instance.PushAsync
				(new AlertDialogPopupPage(HopeString));
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

            ToolbarItems.Clear();       
            
            ToolbarItem SignInItem = 
				new ToolbarItem { Text = getResourceString("ACCOUNT_STRING"), 
								  Command = SignInCommand, 
								  Order = ToolbarItemOrder.Secondary };
								  
            ToolbarItem InfoItem = 
				new ToolbarItem { Text =getResourceString("PREFERENCES_STRING"), 
								  Command =InfoItemCommand, 
								  Order = ToolbarItemOrder.Secondary };
								  
            ToolbarItem TestItem = 
				new ToolbarItem { Text = "test", 
								  Command = TestItemCommand, 
								  Order = ToolbarItemOrder.Secondary };
            ToolbarItems.Add(SignInItem);
            ToolbarItems.Add(InfoItem);
			
            OnToolbarItemAdded();
            
        }        		
		
        private async Task SignInItemMethod()
        {           
            await PopupNavigation.Instance.PushAsync(new SignInPopupPage(_navigationGraphName));
            MessagingCenter.Subscribe<AskRegisterPopupPage, bool>
				(this, "isReset", (msgSender, msgArgs) =>
				{
                    PaymemtListBtn.IsEnabled = app.FinishCount == app.records.Count;
                    PaymemtListBtn.IsVisible = app.FinishCount == app.records.Count;

					Buttonable(true);
					MessagingCenter.Unsubscribe<AskRegisterPopupPage, bool>
						(this, "isRest");
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