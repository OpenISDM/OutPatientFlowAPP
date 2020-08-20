//// for tmp new code 

#region RigistList.xaml.cs
//using IndoorNavigation.Models;
//using IndoorNavigation.Models.NavigaionLayer;
//using IndoorNavigation.Resources.Helpers;
//using IndoorNavigation.ViewModels;
//using IndoorNavigation.Views.Navigation;
//using Plugin.Multilingual;
//using Rg.Plugins.Popup.Services;
//using System;
//using System.Globalization;
//using System.Reflection;
//using System.Resources;
//using System.Threading.Tasks;
//using System.Windows.Input;
//using Xamarin.Forms;
//using Xamarin.Forms.Xaml;
//using System.Collections.Generic;
//using System.Xml;
//using System.Linq;
//using IndoorNavigation.Views.PopUpPage;
//using static IndoorNavigation.Utilities.Storage;
//using IndoorNavigation.Utilities;
//using Xamarin.Essentials;
//using System.Net.Http.Headers;

//namespace IndoorNavigation.Views.OPFM
//{
//    [XamlCompilation(XamlCompilationOptions.Compile)]
//    public partial class RigisterList : CustomToolbarContentPage
//    {
//        #region variable declaration        
//        RegisterListViewModel _viewmodel;
//        private string _navigationGraphName;

//        private XMLInformation _nameInformation;
//        private App app = (App)Application.Current;
//        private Guid allF_Guid =
//            new Guid("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF");

//        Dictionary<Guid, DestinationItem> ElevatorPosition;
//        //to prevent button multi-tap from causing error
//        private bool isButtonPressed = false;
//        private ViewCell lastCell = null;
//        private bool ShiftButtonPressed = false;

//        delegate void MulitItemFinish(RgRecord FinishRecord);
//        MulitItemFinish _multiItemFinish;
//        private RgRecord _shiftTmpRecord = null;
//        #endregion

//        #region Page lifecycle
//        public RigisterList(string navigationGraphName)
//        {
//            InitializeComponent();
//            Console.WriteLine("initalize graph info");

//            _navigationGraphName = navigationGraphName;

//            //FakeHISRequest = new YunalinHttpRequestFake();
//            _nameInformation = LoadXmlInformation(navigationGraphName);
//            //NetworkSettings = DependencyService.Get<INetworkSetting>();

//            ExitAddBtn.IsEnabled =
//                app.records.Count() > 0 &&
//                (app.FinishCount == app.records.Count) &&
//                (app.HaveCashier) &&
//                !(app.records.Count() == 1 && app.records[0].type == RecordType.Register);
//            ExitAddBtn.IsVisible = app.records.Count() > 0 &&
//                (app.FinishCount == app.records.Count) &&
//                (app.HaveCashier) &&
//                !(app.records.Count() == 1 && app.records[0].type == RecordType.Register);
//            LoadPositionData();
//            BindingContext = _viewmodel;

//        }
//        protected override void OnAppearing()
//        {
//            base.OnAppearing();

//            Console.WriteLine(">>OnAppearing");
//            _viewmodel = new RegisterListViewModel(_navigationGraphName);

//            RefreshListView();

//            //AddBtn.CornerRadius =
//            //    (int)(Math.Min(AddBtn.Height, AddBtn.Width) / 2);
//            //ShiftBtn.CornerRadius =
//            //    (int)(Math.Min(ShiftBtn.Height, ShiftBtn.Width) / 2);

//            if (app.HaveCashier && !ExitAddBtn.IsEnabled)
//                Buttonable(false);

//            ExitAddBtn.IsEnabled =
//                app.records.Count() > 0 &&
//                (app.FinishCount == app.records.Count) &&
//                (!app.HaveCashier) &&
//                !(app.records.Count() == 1 && app.records[0].type == RecordType.Register);
//            ExitAddBtn.IsVisible =
//                app.records.Count() > 0 &&
//                (app.FinishCount == app.records.Count) &&
//                (!app.HaveCashier) &&
//                !(app.records.Count() == 1 && app.records[0].type == RecordType.Register);

//            if (app.lastFinished != null && !app.HaveCashier)
//            {
//                RgListView.ScrollTo(app.lastFinished, ScrollToPosition.MakeVisible, false);
//            }
//            else if (app.HaveCashier)
//            {
//                RgListView.ScrollTo(app.records[app.records.Count - 1], ScrollToPosition.MakeVisible, false);
//            }

//            isButtonPressed = false;
//            RefreshToolbarOptions();
//        }
//        protected override void OnDisappearing()
//        {
//            base.OnDisappearing();

//            if (ShiftButtonPressed)
//            {
//                RgListView.ItemTapped -= RgListView_ShiftTapped;
//                RgListView.ItemTapped += RgListView_ItemTapped;
//                _shiftTmpRecord = null;
//                ShiftButtonPressed = false;
//                ReturnWhiteBackground();
//            }
//        }
//        #endregion

//        #region Clinck Event
//        /*this function is to push page to NavigatorPage */
//        async private void RgListView_ItemTapped(object sender,
//                                                 ItemTappedEventArgs e)
//        {
//            Console.WriteLine("the Record type is : " + ((RgRecord)e.Item).type);
//            if (isButtonPressed) return;
//            isButtonPressed = true;

//            #region For Detect Current Time
//            DateTime CurrentDate = DateTime.Now;
//            TimeSpan CurrentTimespan = DateTime.Now.TimeOfDay;
//            Console.WriteLine("CurrentTimeSpan : " + CurrentTimespan);
//            #endregion           
//            if (e.Item is RgRecord record)
//            {
//                if (app.OrderDistrict.ContainsKey(record._groupID) && !(app.OrderDistrict[record._groupID] == record.order || app.OrderDistrict[record._groupID] == record.order - 1)
//                    || (!app.OrderDistrict.ContainsKey(record._groupID) && (record.order != 1)))
//                //if (CanDoThisItem(record))
//                {
//                    Console.WriteLine("please do something first");
//                    string BannerName = "";
//                    for (int i = app.records.IndexOf(record); i >= 0; i--)
//                    {
//                        Console.WriteLine("Loop i = {0}, dptname = {1}", i, app.records[i]);
//                        if (app.records[i].order == record.order - 1 &&
//                            app.records[i]._groupID == record._groupID)
//                        {
//                            if (string.IsNullOrEmpty(BannerName))
//                            {
//                                BannerName = app.records[i].DptName;
//                            }
//                            else
//                            {
//                                BannerName =
//                                    BannerName + "," + app.records[i].DptName;
//                            }
//                        }

//                    }

//                    await PopupNavigation.Instance.PushAsync
//                        (new AlertDialogPopupPage(string.Format
//                        (getResourceString("PLEASE_DO_SOMETHING_FIRST_STRING"),
//                        BannerName),
//                        getResourceString("OK_STRING")));
//                }
//                #region if the clinic has open timing.
//                else if (record.OpeningHours != null &&
//                    !isCareRoomOpening(record.OpeningHours))
//                {
//                    Console.WriteLine("The service is not available now.");
//                    //do some alert implementation

//                    await PopupNavigation.Instance.PushAsync
//                        (new AlertDialogPopupPage
//                        (getResourceString("AVAILABLE_CLINICS_NOW_STRING"),
//                        getResourceString("GO_STRING"),
//                       getResourceString("NO_STRING"),
//                        "Still go to careroom"));

//                    MessagingCenter.Subscribe<AlertDialogPopupPage, bool>
//                        (this, "Still go to careroom",
//                        async (MsgSender, MsgArgs) =>
//                        {
//                            Console.WriteLine("Get Subscribe string");

//                            if ((bool)MsgArgs)
//                            {
//                                await PopupNavigation.Instance.PopAllAsync();
//                                await Navigation.PushAsync
//                                    (new NavigatorPage(_navigationGraphName,
//                                               record._regionID,
//                                               record._waypointID,
//                                               record._waypointName,
//                                            _nameInformation));
//                                record.isComplete = true;
//                            }
//                            else
//                            {
//                                //isButtonPressed = false;
//                                await PopupNavigation.Instance.PopAllAsync();
//                            }
//                            MessagingCenter
//                            .Unsubscribe<AlertDialogPopupPage, bool>
//                            (this, "Still go to careroom");
//                        });
//                }
//                #endregion
//                else
//                {
//                    #region If the record has additional string.
//                    if (!string.IsNullOrEmpty(record.AdditionalMsg))
//                    {
//                        await PopupNavigation.Instance.PushAsync
//                            (new AlertDialogPopupPage(record.AdditionalMsg,
//                            getResourceString("OK_STRING")));
//                    }
//                    #endregion

//                    #region If destination is out of range, like CCH 3F、5F


//                    if (record._regionID.Equals(allF_Guid) && record._waypointID.Equals(allF_Guid))
//                    {
//                        Console.WriteLine("This range of item haven't been supported now");

//                        //do select waypoint in elevator.
//                        await PopupNavigation.Instance.PushAsync(new AlertDialogPopupPage(
//                            "該樓層不在導航範圍內，\n因此將帶您到電梯廳，\n並搭電梯前往。" +
//                            "\n結束後請回到B1~2F樓層，將能繼續使用導航。", "確定"));
//                        DestinationItem ElevatorItem;
//                        try
//                        {
//                            ElevatorItem =
//                                ElevatorPosition[app.lastFinished._regionID];
//                        }
//                        catch
//                        {
//                            ElevatorItem = ElevatorPosition.First().Value;
//                        }

//                        record._regionID = ElevatorItem._regionID;
//                        record._waypointID = ElevatorItem._waypointID;
//                        record._waypointName = ElevatorItem._waypointName;
//                    }

//                    #endregion

//                    await Navigation.PushAsync
//                        (new NavigatorPage(_navigationGraphName,
//                                           record._regionID,
//                                           record._waypointID,
//                                           record._waypointName,
//                                           _nameInformation)
//                        );
//                    record.isComplete = true;
//                }

//            }
//            RefreshListView();
//            ((ListView)sender).SelectedItem = null;

//        }
//        async private void RgListView_ShiftTapped(object sender, ItemTappedEventArgs e)
//        {
//            Console.WriteLine(">>RgListView_ShiftTapped");

//            #region First Tapped
//            if (_shiftTmpRecord == null)
//            {
//                _shiftTmpRecord = e.Item as RgRecord;
//            }
//            #endregion
//            #region Second Tapped
//            else
//            {
//                var selectItem = e.Item as RgRecord;
//                //if(_shiftTmpRecord._groupID == selectItem._groupID && )
//                if (OrderIsHigher(_shiftTmpRecord, selectItem))
//                {
//                    Console.WriteLine("Order is higher");

//                    //swap(app.records, app.records.IndexOf(selectItem), app.records.IndexOf(_shiftTmpRecord));
//                    RgRecord tmp = selectItem;

//                    int index1 = app.records.IndexOf(selectItem);
//                    int index2 = app.records.IndexOf(_shiftTmpRecord);

//                    app.records[index1] = _shiftTmpRecord;
//                    app.records[index2] = selectItem;
//                }
//                else
//                {
//                    Console.WriteLine("Order is lower");
//                    await PopupNavigation.Instance.PushAsync(new AlertDialogPopupPage(getResourceString("YOU_CANNOT_SWAP_THEM_STRING"), "OK"));
//                    _shiftTmpRecord = null;
//                    RefreshListView();
//                    ReturnWhiteBackground();
//                    return;
//                }

//                RgListView.ItemTapped -= RgListView_ShiftTapped;
//                RgListView.ItemTapped += RgListView_ItemTapped;

//                _shiftTmpRecord = null;
//                ShiftButtonPressed = false;
//                ReturnWhiteBackground();
//                RefreshListView();
//                RefreshToolbarOptions();
//                Buttonable(true);
//            }
//            #endregion
//            #region Group swap
//            //Console.WriteLine("_shiftTmpRecords is null : " + (_shiftTmpRecords == null));
//            //#region First Tapped
//            //if(_shiftTmpRecords == null)
//            //{
//            //    var TappedItem = e.Item as RgRecord;
//            //    _shiftTmpRecords = new List<RgRecord>();

//            //    if(TappedItem._groupID != 0)
//            //    {
//            //        foreach(RgRecord item in app.records)
//            //        {
//            //            if(TappedItem._groupID == item._groupID)
//            //            {
//            //                item.selectedGroupColor = Color.FromHex("FFFF88");
//            //                _shiftTmpRecords.Add(item);
//            //                Console.WriteLine("Add to _shiftTmpRecords == " + item.DptName);
//            //            }
//            //        }
//            //        RefreshListView();
//            //    }
//            //    else
//            //    {
//            //        _shiftTmpRecords.Add(TappedItem);
//            //    }                
//            //}
//            //#endregion

//            //#region Second Tapped
//            //else
//            //{
//            //    var TappedItem = e.Item as RgRecord;

//            //    if(_shiftTmpRecords.Contains(TappedItem))
//            //    {
//            //        await PopupNavigation.Instance.PushAsync
//            //            (new AlertDialogPopupPage
//            //            (getResourceString("CANNOT_SHIFT_SAME_GROUP_STRING"), 
//            //            getResourceString("OK_STRING")));
//            //        return;
//            //    }
//            //    else if (TappedItem._groupID != 0)
//            //    {
//            //        List<RgRecord> _selectTmpRecord2 = 
//            //            app.records.Where(p => p._groupID == TappedItem._groupID)
//            //            .Select(p=>
//            //            {
//            //                p.selectedGroupColor = Color.FromHex("FFFF88");
//            //                return p;
//            //            }).ToList();                    
//            //        List<RgRecord> tmpRecord = app.records.ToList();

//            //        int first1 = tmpRecord.IndexOf(_selectTmpRecord2[0]);
//            //        int first2 = tmpRecord.IndexOf(_shiftTmpRecords[0]);
//            //        int last1 = tmpRecord.IndexOf(_selectTmpRecord2.Last());
//            //        int last2 = tmpRecord.IndexOf(_shiftTmpRecords.Last());
//            //        swapRgRecord( ref tmpRecord, first1, last1, first2,last2);
//            //        app.records = ToObservableCollection(tmpRecord);
//            //    }
//            //    else
//            //    {                   
//            //        List<RgRecord> tmpRecord = app.records.ToList();

//            //        int first1 = tmpRecord.IndexOf(_shiftTmpRecords[0]);
//            //        int first2 = tmpRecord.IndexOf(TappedItem);
//            //        int last1 = tmpRecord.IndexOf(_shiftTmpRecords.Last());
//            //        int last2 = first2;

//            //        swapRgRecord(ref tmpRecord, first1, last1, first2, last2);
//            //        app.records = ToObservableCollection(tmpRecord);
//            //    }

//            //    //int index1 = app.records.IndexOf(_shiftTmpRecords[0]);                
//            //    RgListView.ItemTapped -= RgListView_ShiftTapped;
//            //    RgListView.ItemTapped += RgListView_ItemTapped;
//            //    _shiftTmpRecords = null;
//            //    ReturnWhiteBackground();
//            //    RefreshListView();
//            //    RefreshToolbarOptions();
//            //    Buttonable(true);
//            //}
//            //#endregion            
//            #endregion
//        }
//        private bool OrderIsHigher(RgRecord item1, RgRecord item2)
//        {
//            if (item1._groupID == 0 && item2._groupID == 0) return true;
//            int index1 = app.records.IndexOf(item1);
//            int index2 = app.records.IndexOf(item2);

//            List<RgRecord> tmp = app.records.ToList();

//            swap(ref tmp, index1, index2);

//            Dictionary<int, int> checkValue = new Dictionary<int, int>();
//            foreach (RgRecord record in tmp)
//            {
//                if (record._groupID == 0) continue;
//                if (!checkValue.ContainsKey(record._groupID))
//                {
//                    checkValue.Add(record._groupID, record.order);
//                    continue;
//                }

//                if (checkValue[record._groupID] > record.order) return false;
//                checkValue[record._groupID] = record.order;
//            }

//            return true;
//        }
//        //the function is a button event to add payment and medicine recieving 
//        //route to listview
//        //private void PaymemtListBtn_Clicked(object sender, EventArgs e)
//        //{
//        //    Buttonable(false);
//        //    if (isButtonPressed) return;
//        //    isButtonPressed = true;
//        //    ExitAddBtn.IsEnabled = false;
//        //    ExitAddBtn.IsVisible = false;
//        //    app.HaveCashier = true;
//        //    DestinationItem cashier, pharmacy;
//        //    try
//        //    {
//        //        cashier = CashierPosition[app.lastFinished._regionID];
//        //    }
//        //    catch
//        //    {
//        //        cashier = CashierPosition.First().Value;
//        //    }
//        //    try
//        //    {
//        //        pharmacy = PharmacyPostition[app.lastFinished._regionID];
//        //    }
//        //    catch
//        //    {
//        //        pharmacy = PharmacyPostition.First().Value;
//        //    }

//        //    int order =
//        //        app.OrderDistrict.ContainsKey(0) ? app.OrderDistrict[0] : 0;

//        //    app.records.Add(new RgRecord
//        //    {
//        //        _waypointID = cashier._waypointID,
//        //        _regionID = cashier._regionID,
//        //        _waypointName = cashier._waypointName,
//        //        type = RecordType.Cashier,
//        //        DptName = cashier._waypointName,
//        //        order = order + 1,
//        //        _groupID = 0
//        //    });
//        //    app.records.Add(new RgRecord
//        //    {
//        //        _waypointID = pharmacy._waypointID,
//        //        _regionID = pharmacy._regionID,
//        //        _waypointName = pharmacy._waypointName,
//        //        type = RecordType.Pharmacy,
//        //        DptName = pharmacy._waypointName,
//        //        order = order + 2,
//        //        _groupID = 0
//        //    });

//        //    RgListView.ScrollTo(app.records[app.records.Count - 1],
//        //        ScrollToPosition.MakeVisible, true);
//        //    isButtonPressed = false;
//        //}

//        /*to show popup page for add route to listview*/
//        async private void AddBtn_Clicked(object sender, EventArgs e)
//        {
//            if (isButtonPressed) return;

//            isButtonPressed = true;
//            ExitAddBtn.IsEnabled = false;
//            ExitAddBtn.IsVisible = false;

//            IndicatorPopupPage isBusyPage = new IndicatorPopupPage();

//            await PopupNavigation.Instance.PushAsync(isBusyPage);

//            await PopupNavigation.Instance.PushAsync
//                (new AddPopupPage(_navigationGraphName));

//            if (PopupNavigation.Instance.PopupStack.Contains(isBusyPage))
//                await PopupNavigation.Instance.RemovePageAsync(isBusyPage);

//            MessagingCenter.Subscribe<AddPopupPage, bool>(this, "isCancel",
//              (Messagesender, Messageargs) =>
//              {
//                  ExitAddBtn.IsEnabled =
//                  (app.FinishCount == app.records.Count) &&
//                  !app.HaveCashier &&
//                   app.records.Count() > 0 &&
//                !(app.records.Count() == 1 &&
//                app.records[0].type == RecordType.Register);

//                  ExitAddBtn.IsVisible =
//                  (app.FinishCount == app.records.Count) &&
//                  !app.HaveCashier &&
//                   app.records.Count() > 0 &&
//                !(app.records.Count() == 1 && app.records[0].type == RecordType.Register); ;

//                  Console.WriteLine("isButtonPressed in messagingCenter :" + isButtonPressed);
//                  MessagingCenter.Unsubscribe<AddPopupPage, bool>
//                      (this, "isCancel");
//              });
//            isButtonPressed = false;
//        }

//        private bool CanBeShifted()
//        {
//            //int GroupCount = app.OrderDistrict.Keys.Where(p=> p!=0).Count();
//            int AddItemNotCompleteCount =
//                app.records.Where(p => !p.isAccept && p._groupID == 0).Count();

//            if (AddItemNotCompleteCount >= 2)
//                return true;

//            int GroupNotCompleteCount =
//                app.records.Where(p => p._groupID != 0 && !p.isAccept).Count();

//            if (AddItemNotCompleteCount >= 1 && GroupNotCompleteCount >= 1)
//                return true;
//            int currentGroupID = 0;
//            int TotalNotComplete = 0;
//            //foreach(KeyValuePair<int,int> pair in app.OrderDistrict)
//            foreach (RgRecord record in app.records)
//            {
//                if (record._groupID == 0) continue;
//                if (record._groupID != currentGroupID)
//                {
//                    Console.WriteLine("Current dptName = " + record.DptName + "Group ID =" + currentGroupID);
//                    int CurrentGroupNotComplete =
//                        app.records.Where(p => p._groupID == record._groupID && !p.isAccept).Count();
//                    Console.WriteLine("CurrentGroupNotComplete = " + CurrentGroupNotComplete);
//                    if (CurrentGroupNotComplete > 0)
//                        TotalNotComplete++;

//                    currentGroupID = record._groupID;
//                }
//            }
//            Console.WriteLine("TotalNotComplete = " + TotalNotComplete);
//            if (TotalNotComplete >= 2) return true;

//            return false;
//        }

//        private void ListviewItem_Delete(object sender, EventArgs args)
//        {
//            var item = (RgRecord)((MenuItem)sender).CommandParameter;

//            if (item != null && app.records.Contains(item))
//            {
//                app.records.Remove(item);
//            }
//        }
//        async private void ShiftBtn_Clicked(object sender, EventArgs e)
//        {
//            if (isButtonPressed) return;
//            isButtonPressed = true;
//            bool isCheck = Preferences.Get("isCheckedNeverShow", false);

//            //I need to consider this statement
//            if (!CanBeShifted())
//            {
//                await PopupNavigation.Instance.PushAsync
//                   (new AlertDialogPopupPage(getResourceString("NO_SHIFT_STRING"), getResourceString("OK_STRING")));
//                isButtonPressed = false;
//                return;
//            }
//            else
//            {
//                if (!isCheck)
//                {
//                    //await PopupNavigation.Instance.PushAsync(new ShiftAlertPopupPage());
//                    await PopupNavigation.Instance.PushAsync
//                        (new ShiftAlertPopupPage
//                        (getResourceString("SHIFT_DESCRIPTION_STRING"),
//                        getResourceString("OK_STRING"), "isCheckedNeverShow"));
//                }
//                CancelShiftCommand = new Command(async () => await CancelShiftItemMethod());
//                ToolbarItems.Clear();

//                ToolbarItem CancelShiftItem = new ToolbarItem
//                {
//                    Order = ToolbarItemOrder.Primary,
//                    Text = getResourceString("CANCEL_STRING"),
//                    Command = CancelShiftCommand
//                };

//                ToolbarItems.Add(CancelShiftItem);
//                OnToolbarItemAdded();

//                RgListView.ItemTapped -= RgListView_ItemTapped;
//                RgListView.ItemTapped += RgListView_ShiftTapped;
//                ShiftButtonPressed = true;
//                Buttonable(false);
//            }

//            isButtonPressed = false;
//        }
//        // this function is a button event, which is to check user whether have 
//        // arrive at destination.
//        async private void YetFinishBtn_Clicked(object sender, EventArgs e)
//        {
//            var o = (Button)sender;
//            var FinishBtnClickItem = o.CommandParameter as RgRecord;
//            if (FinishBtnClickItem != null)
//            {
//                switch (FinishBtnClickItem.type)
//                {
//                    case RecordType.Register:
//                        _multiItemFinish = RegisterFinish;
//                        break;
//                    case RecordType.Exit:
//                        _multiItemFinish = ExitFinish;
//                        break;
//                    case RecordType.Queryresult:
//                        _multiItemFinish = QueryResultFinish;
//                        break;
//                    default:
//                        _multiItemFinish = DefaultFinish;
//                        break;
//                }
//                _multiItemFinish(FinishBtnClickItem);

//                if (app.FinishCount == app.records.Count &&
//                app.lastFinished.type != RecordType.Register)
//                {
//                    if (app.HaveCashier && !ExitAddBtn.IsEnabled)
//                    {
//                        await DisplayAlert(getResourceString("MESSAGE_STRING"),
//                                    getResourceString("FINISH_SCHEDULE_STRING"),
//                                    getResourceString("OK_STRING"));

//                        await PopupNavigation.Instance.PushAsync
//                            (new ExitPopupPage(_navigationGraphName));
//                    }
//                    else if (!app.HaveCashier)
//                    {
//                        ExitAddBtn.IsEnabled = true;
//                        ExitAddBtn.IsVisible = true;
//                    }
//                }
//                int index = app.records.IndexOf(FinishBtnClickItem);

//                if (app.OrderDistrict.ContainsKey(FinishBtnClickItem._groupID))
//                {
//                    app.OrderDistrict[FinishBtnClickItem._groupID] = FinishBtnClickItem.order;
//                }
//                else
//                {
//                    app.OrderDistrict.Add(FinishBtnClickItem._groupID, FinishBtnClickItem.order);
//                }

//                for (int i = index; i < app.records.Count; i++)
//                {
//                    if (FinishBtnClickItem._groupID == 0) break;
//                    if (app.records[i].order >= FinishBtnClickItem.order + 2
//                        && app.records[i]._groupID == FinishBtnClickItem._groupID)
//                        break;

//                    if (app.records[i].order == FinishBtnClickItem.order && app.records[i]._groupID == FinishBtnClickItem._groupID
//                        && (!app.records[i].isAccept && app.records[i].isComplete))
//                        break;
//                    if ((app.records[i].order <= FinishBtnClickItem.order + 1) &&
//                     app.records[i]._groupID == FinishBtnClickItem._groupID)
//                    {
//                        app.records[i].isComplete = true;
//                    }
//                }
//                RefreshListView();
//            }
//        }

//        #endregion

//        #region  For Get Value
//        public void LoadPositionData()
//        {
//            //CashierPosition = new Dictionary<Guid, DestinationItem>();
//            //PharmacyPostition = new Dictionary<Guid, DestinationItem>();
//            ElevatorPosition = new Dictionary<Guid, DestinationItem>();
//            XmlDocument doc;
//            #region Load Cashier and Pharmacy position
//            //XmlDocument doc = Storage.XmlReader("Yuanlin_OPFM.CashierStation.xml");
//            //XmlNodeList CashiernodeList = doc.GetElementsByTagName("Cashierstation");
//            //XmlNodeList PharmacyNodeList = doc.GetElementsByTagName("Pharmacystation");
//            //foreach (XmlNode node in CashiernodeList)
//            //{
//            //    DestinationItem item = new DestinationItem();

//            //    item._regionID = new Guid(node.Attributes["region_id"].Value);
//            //    item._waypointID = new Guid(node.Attributes["waypoint_id"].Value);
//            //    item._floor = node.Attributes["floor"].Value;
//            //    item._waypointName = node.Attributes["name"].Value;

//            //    Console.WriteLine(item._waypointName + " region id:" + item._regionID + ", waypoint id: " + item._waypointID);

//            //    CashierPosition.Add(new Guid(node.Attributes["region_id"].Value), item);
//            //}

//            //foreach (XmlNode node in PharmacyNodeList)
//            //{
//            //    DestinationItem item = new DestinationItem();
//            //    item._regionID = new Guid(node.Attributes["region_id"].Value);
//            //    item._waypointID = new Guid(node.Attributes["waypoint_id"].Value);
//            //    item._floor = node.Attributes["floor"].Value;
//            //    item._waypointName = node.Attributes["name"].Value;

//            //    PharmacyPostition.Add(new Guid(node.Attributes["region_id"].Value), item);
//            //}
//            #endregion

//            #region Load Elevator
//            doc = Storage.XmlReader("Yuanlin_OPFM.ElevatorsMap.xml");
//            XmlNodeList ElevatorNodeList = doc.GetElementsByTagName("elevator");

//            foreach (XmlNode elevatorNode in ElevatorNodeList)
//            {
//                DestinationItem item = new DestinationItem();

//                item._regionID = new Guid(elevatorNode.Attributes["region_id"].Value);
//                item._waypointID = new Guid(elevatorNode.Attributes["waypoint_id"].Value);
//                item._floor = elevatorNode.Attributes["floor"].Value;
//                item._waypointName = elevatorNode.Attributes["name"].Value;

//                ElevatorPosition.Add(item._regionID, item);
//            }
//            #endregion
//            return;
//        }
//        async private Task ReadXml()
//        {
//            //request.GetXMLBody();
//            //await request.RequestData();          
//            //await FakeHISRequest.RequestFakeHIS();
//            RefreshListView();
//            await Task.CompletedTask;
//        }


//        private void swap<T>(ref List<T> list, int i, int j)
//        {
//            T tmp = list[i];
//            list[i] = list[j];
//            list[j] = tmp;
//            return;
//        }

//        private string getResourceString(string key)
//        {
//            string resourceId = "IndoorNavigation.Resources.AppResources";
//            CultureInfo currentLanguage =
//                CrossMultilingual.Current.CurrentCultureInfo;
//            ResourceManager resourceManager =
//                new ResourceManager(resourceId,
//                                    typeof(TranslateExtension)
//                                    .GetTypeInfo().Assembly);

//            return resourceManager.GetString(key, currentLanguage);
//        }

//        private Week GetDayofWeek()
//        {
//            switch (DateTime.Now.DayOfWeek)
//            {
//                case DayOfWeek.Sunday:
//                    return Week.Sunday;
//                case DayOfWeek.Saturday:
//                    return Week.Saturday;
//                default:
//                    return Week.Workingday;
//            }
//        }

//        private bool isCareRoomOpening(List<OpeningTime> openingTimes)
//        {
//            // TimeSpan compare function rule
//            // t1 < t2 return -1
//            // t1 = t2 return 0
//            // t1 > t2 return 1
//            if (openingTimes.Count <= 0) return true;

//            TimeSpan CurrentTime = DateTime.Now.TimeOfDay;
//            Week TodayOfWeekDay = GetDayofWeek();
//            Console.WriteLine("CurrentTimeSpan : " + CurrentTime);
//            Console.WriteLine("Week : " + TodayOfWeekDay);

//            foreach (OpeningTime openTime in openingTimes)
//            {
//                if (TodayOfWeekDay != openTime.dayOfWeek)
//                    continue;
//                int StartTimeCompare =
//                    TimeSpan.Compare(CurrentTime, openTime.startTime);
//                int EndTimeCompare =
//                    TimeSpan.Compare(openTime.endTime, CurrentTime);

//                if (StartTimeCompare == 1 && EndTimeCompare == 1)
//                {
//                    Console.WriteLine("Current, the room is available now");
//                    return true;
//                }
//            }

//            return false;
//        }
//        #endregion

//        #region UI View Control
//        private void Buttonable(bool enable)
//        {
//            AddBtn.IsEnabled = enable;
//            AddBtn.IsVisible = enable;
//            ShiftBtn.IsEnabled = enable;
//            ShiftBtn.IsVisible = enable;
//        }
//        private void ViewCell_Tapped(object sender, EventArgs e)
//        {
//            if (lastCell != null)
//            {
//                lastCell.View.BackgroundColor = Color.Transparent;
//            }
//            var viewCell = (ViewCell)sender;
//            if (viewCell.View != null)
//            {
//                viewCell.View.BackgroundColor = Color.FromHex("FFFF88");
//            }
//        }

//        private void RefreshListView()
//        {
//            isButtonPressed = false;
//            RgListView.ItemsSource = null;
//            RgListView.ItemsSource = app.records;
//        }
//        private void ReturnWhiteBackground()
//        {
//            app.records.All(p =>
//            {
//                p.selectedGroupColor = Color.Transparent;
//                return true;
//            });
//        }
//        #endregion

//        #region Item Finished delegate fuctions
//        private void ItemFinishFunction(RgRecord record)
//        {
//            record.isAccept = true;
//            record.isComplete = true;

//            app.FinishCount++;
//            app.lastFinished = record;

//            RefreshListView();
//        }
//        async private void RegisterFinish(RgRecord record)
//        {
//            //this part might happend bugs

//            await PopupNavigation.Instance.PushAsync(new IndicatorPopupPage());
//            Console.WriteLine("Register Finished");
//            bool NetworkConnectAbility = true;

//            if (NetworkConnectAbility)
//            {
//                await ReadXml();
//                Console.WriteLine("ReadXml finished");
//                ItemFinishFunction(record);
//            }
//            else
//            {
//                var CheckWantToSetting =
//                    await DisplayAlert(getResourceString("MESSAGE_STRING"),
//                                       getResourceString("BAD_NETWORK_STRING"),
//                                       getResourceString("OK_STRING"),
//                                       getResourceString("NO_STRING"));
//                //BusyIndicatorShow(false);
//                PopupNavigation.Instance.PopAsync();
//                if (CheckWantToSetting)
//                {
//                    //NetworkSettings.OpenSettingPage();
//                    return;
//                }
//                else
//                {
//                    await Navigation.PopToRootAsync();
//                    return;
//                }
//            }

//            //BusyIndicatorShow(false);
//            await PopupNavigation.Instance.PopAsync();
//        }
//        async private void ExitFinish(RgRecord record)
//        {
//            string HopeString =
//                $"{Storage.GetDisplayName(_navigationGraphName)}" +
//                $"\n{getResourceString("HOPE_STRING")}";
//            await PopupNavigation.Instance.PushAsync
//                (new AlertDialogPopupPage(HopeString));
//            await Navigation.PopAsync();
//            app.FinishCount--;

//            ItemFinishFunction(record);
//        }
//        private void QueryResultFinish(RgRecord record)
//        {
//            app.roundRecord = record;
//            ItemFinishFunction(record);
//        }
//        private void DefaultFinish(RgRecord record)
//        {
//            ItemFinishFunction(record);
//        }
//        #endregion

//        #region iOS secondary toolbaritem implement

//        public override event EventHandler ToolbarItemAdded;

//        private void RefreshToolbarOptions()
//        {
//            SignInCommand = new Command(async () => await SignInItemMethod());
//            InfoItemCommand = new Command(async () => await InfoItemMethod());
//            ClearItemCommand = new Command(async () => await ClearItemMethod());
//            ToolbarItems.Clear();

//            ToolbarItem SignInItem =
//                new ToolbarItem
//                {
//                    Text = getResourceString("ACCOUNT_STRING"),
//                    Command = SignInCommand,
//                    Order = ToolbarItemOrder.Secondary
//                };

//            ToolbarItem InfoItem =
//                new ToolbarItem
//                {
//                    Text = getResourceString("PREFERENCES_STRING"),
//                    Command = InfoItemCommand,
//                    Order = ToolbarItemOrder.Secondary
//                };
//            ToolbarItem ClearItem =
//                new ToolbarItem
//                {
//                    Text = getResourceString("CLEAR_STRING"),
//                    Command = ClearItemCommand,
//                    Order = ToolbarItemOrder.Primary,
//                };
//            ToolbarItem TestItem =
//                new ToolbarItem
//                {
//                    Text = "test",
//                    Command = TestItemCommand,
//                    Order = ToolbarItemOrder.Secondary
//                };
//            //ToolbarItems.Add(SignInItem);
//            ToolbarItems.Add(InfoItem);
//            ToolbarItems.Add(ClearItem);
//            OnToolbarItemAdded();

//        }

//        private async Task SignInItemMethod()
//        {
//            await PopupNavigation.Instance.PushAsync(new SignInPopupPage(_navigationGraphName));
//            MessagingCenter.Subscribe<AskRegisterPopupPage, bool>
//                (this, "isReset", (msgSender, msgArgs) =>
//                {
//                    ExitAddBtn.IsEnabled = app.FinishCount == app.records.Count;
//                    ExitAddBtn.IsVisible = app.FinishCount == app.records.Count;

//                    Buttonable(true);
//                    MessagingCenter.Unsubscribe<AskRegisterPopupPage, bool>
//                        (this, "isRest");
//                });

//            await Task.CompletedTask;
//        }
//        private async Task InfoItemMethod()
//        {
//            Console.WriteLine("Preference item click");
//            await Navigation.PushAsync(new NavigatorSettingPage());
//            await Task.CompletedTask;
//        }
//        private async Task ClearItemMethod()
//        {
//            if (isButtonPressed) return;
//            isButtonPressed = true;

//            Console.WriteLine("Clear item click");
//            await PopupNavigation.Instance.PushAsync(new AlertDialogPopupPage(getResourceString("ARE_YOU_SURE_TO_CLEAR_STRING"), getResourceString("CLEAR_STRING"), getResourceString("NO_STRING"), "ClearOrNot"));

//            MessagingCenter.Subscribe<AlertDialogPopupPage, bool>(this, "ClearOrNot", (MsgSender, MsgArgs) =>
//            {
//                Console.WriteLine("Get ClearOrNot message.");

//                if ((bool)MsgArgs)
//                {
//                    app.records.Clear();
//                    app._TmpRecords.Clear();
//                    app.OrderDistrict.Clear();
//                    app.HaveCashier = false;
//                    app.lastFinished = null;
//                    app.isRigistered = false;
//                    app.getRigistered = false;
//                    app.roundRecord = null;
//                    OPPA_TmperorayStatus.ClearAllState();
//                    Buttonable(true);
//                    OnAppearing();
//                }
//                else
//                {
//                    //do nothing;
//                }
//                MessagingCenter.Unsubscribe<AlertDialogPopupPage, bool>(this, "ClearOrNot");
//            });
//            isButtonPressed = false;
//        }
//        async private Task CancelShiftItemMethod()
//        {
//            RgListView.ItemTapped += RgListView_ItemTapped;
//            RgListView.ItemTapped -= RgListView_ShiftTapped;
//            _shiftTmpRecord = null;

//            Buttonable(true);
//            RefreshToolbarOptions();
//            ReturnWhiteBackground();

//            await Task.CompletedTask;
//        }
//        protected void OnToolbarItemAdded()
//        {
//            Console.WriteLine("call onToolbarItemAdded");
//            EventHandler e = ToolbarItemAdded;
//            e?.Invoke(this, new EventArgs());
//        }

//        #region ToolBarItemAttributes and Commands

//        public ICommand SignInCommand { get; set; }
//        public ICommand InfoItemCommand { get; set; }
//        public ICommand TestItemCommand { get; set; }
//        public ICommand ClearItemCommand { get; set; }
//        private ICommand CancelShiftCommand { get; set; }
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

//        #endregion

//        #region New Process of OPPA

//        async private void ExitAddBtn_Clicked(object sender, EventArgs e)
//        {

//            Buttonable(false);

//            if (isButtonPressed) return;
//            isButtonPressed = true;

//            ExitAddBtn.IsVisible = false;
//            ExitAddBtn.IsEnabled = false;

//            app.HaveCashier = true;

//            //show select exit popup page. 
//            await PopupNavigation.Instance.PushAsync
//                (new ExitPopupPage(_navigationGraphName));
//            //remember to implement cancel messagingcenter  

//            MessagingCenter.Subscribe<ExitPopupPage, bool>(this, "ExitCancel",
//                (MsgSender, MsgArgs) =>
//                {
//                    MessagingCenter.Unsubscribe<ExitPopupPage, bool>
//                    (this, "ExitCancel");
//                });

//            isButtonPressed = false;
//        }


//        #endregion
//    }
//}
#endregion

#region RigistList.xaml
//<? xml version="1.0" encoding="utf-8" ?>
//<local:CustomToolbarContentPage
//             xmlns:local="clr-namespace:IndoorNavigation"
//             xmlns="http://xamarin.com/schemas/2014/forms"
//             xmlns:i18n="clr-namespace:IndoorNavigation.Resources.Helpers"
//             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
//             xmlns:d="http://xamarin.com/schemas/2014/forms/design"
//             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
//            Title="{i18n:Translate NAVIGATION_LIST_STRING}"
//             mc:Ignorable="d"
//             x:Class="IndoorNavigation.Views.OPFM.RigisterList">
//    <ContentPage.Resources>
//        <ResourceDictionary>
//            <Style TargetType = "Label" >
//                < Setter Property="TextColor" 
//						Value="Black"/>

//                <Setter Property = "FontSize"

//                        Value="22"/>
//            </Style>
//            <DataTemplate x:Key="YetNavigation">
//                <!--to show haven't started schedule view cell-->
//                <ViewCell Tapped = "ViewCell_Tapped" >
//                    < ViewCell.Height >
//                        < OnPlatform x:TypeArguments="x:Double">
//                            <On Platform = "iOS" Value="170"/>
//                        </OnPlatform>
//                    </ViewCell.Height>
//                    <StackLayout>
//                        <StackLayout Orientation = "Horizontal"

//                                     Margin="10,0,10,0" 
//									 VerticalOptions="CenterAndExpand">

//                            <Label x:Name="DPTNAME" 
//								   Text="{Binding DptName, StringFormat='{0}'}" 
//								   FontSize="26" 
//								   HorizontalOptions="StartAndExpand" 
//								   VerticalOptions="CenterAndExpand" 
//                                   TextColor="Black"/>

//                            <Button x:Name="yetFinishBtn" 
//									Text="{i18n:Translate FINISH_STRING}"
//                                    IsEnabled="{Binding isComplete}" 
//                                    IsVisible="{Binding isComplete}"
//                                    VerticalOptions="CenterAndExpand"
//									FontSize="Large" 									
//                                    HorizontalOptions="EndAndExpand" 
//									CommandParameter="{Binding .}" 
//									Clicked="YetFinishBtn_Clicked" 
//									BorderColor="#3F51B5" 
//									BorderWidth="1" 
//                                    TextColor="White" 
//									BackgroundColor="#3f51b5" 
//									FontAttributes="Bold"/>
//                        </StackLayout>
//                        <StackLayout Orientation = "Vertical"

//                                     VerticalOptions="CenterAndExpand" 
//									 Margin="25,0,25,0">

//                            <Label x:Name="DptTimeText" 
//								   Text=
//								   "{Binding Shift, StringFormat=
//									 {i18n:Translate OUTPATIENT_TIME_STRING}}"/>

//                            <Label x:Name="CareRoomText" 
//								   Text="{Binding CareRoom, StringFormat=
//											{i18n:Translate CAREROOM_STRING}}"/>

//                            <Label x:Name="DrNameText" 
//								   Text="{Binding DrName, StringFormat=
//											  {i18n:Translate DRNAME_STRING}}"/>

//                            <Label x:Name="SeeSeqNumberText" 
//								   Text="{Binding SeeSeq, StringFormat=
//										    {i18n:Translate SEQUENCE_STRING}}"/>
//                        </StackLayout>
//                    </StackLayout>
//                </ViewCell>
//            </DataTemplate>
//            <DataTemplate x:Key="NotComplete">
//                <ViewCell Tapped = "ViewCell_Tapped" Height="60">
//                    <StackLayout >
//                        <StackLayout Orientation = "Horizontal"

//                                     VerticalOptions="CenterAndExpand" 
//									 Margin="10,0,10,0">

//                            <Label x:Name="DptName" 
//								   Text="{Binding DptName, StringFormat='{0}'}" 
//								   FontSize="26" 
//                                   HorizontalOptions="StartAndExpand" 
//								   VerticalOptions="CenterAndExpand" 
//								   TextColor="Black" />

//                            <Button x:Name="FinishBtn" 
//									Text="{i18n:Translate FINISH_STRING}" 
//									HorizontalOptions="EndAndExpand"
//                                    VerticalOptions="CenterAndExpand"
//									FontSize="Large" 
//                                    IsEnabled="{Binding isComplete}" 
//                                    IsVisible="{Binding isComplete}"
//									Clicked="YetFinishBtn_Clicked" 
//									CommandParameter="{Binding .}" 
//									TextColor="White" 
//									BackgroundColor="#3f51b5" 
//									BorderColor="#3f51b5" 
//									BorderWidth="1"/>
//                        </StackLayout>
//                    </StackLayout>
//                </ViewCell>
//            </DataTemplate>
//            <DataTemplate x:Key="Complete">
//                <ViewCell IsEnabled = "False"

//                          Height="60">
//                    <StackLayout BackgroundColor = "#e0e0e0" >
//                        < Label Text="{Binding DptName}" 
//							   FontSize="26" 
//							   TextColor="#c0c0c0" 
//							   Margin="10,0,10,0" 
//							   VerticalOptions="CenterAndExpand"/>
//                    </StackLayout>
//                </ViewCell>
//            </DataTemplate>
//            <DataTemplate x:Key="AddItemTemplate">
//                <ViewCell Tapped = "ViewCell_Tapped" >
//                    < ViewCell.Height >
//                        < OnPlatform x:TypeArguments="x:Double">
//                            <On Platform = "iOS" Value="90"/>
//                        </OnPlatform> 
//                    </ViewCell.Height>
//                    <ViewCell.ContextActions>
//                        <MenuItem Clicked = "ListviewItem_Delete" CommandParameter="{Binding .}"
//                                  Text="Delete" IsDestructive="True"/>
//                    </ViewCell.ContextActions>
//                    <StackLayout BackgroundColor = "{Binding selectedGroupColor}" >
//                        < StackLayout Margin="10,5,10,5">
//                        <Grid>
//                            <Grid.ColumnDefinitions>
//                                <ColumnDefinition Width = "*" />
//                                < ColumnDefinition Width="*"/>
//                                <ColumnDefinition Width = "*" />
//                                < ColumnDefinition Width="*"/>
//                                <ColumnDefinition Width = "*" />
//                            </ Grid.ColumnDefinitions >

//                            < Label x:Name="DptNameLabel" 
//                                   Text="{Binding DptName, StringFormat='{0}'}" 
//                                   FontSize="28"
//                                   TextColor="Black"
//                                   VerticalOptions="CenterAndExpand"
//                                   Grid.Column="0" 
//                                   Grid.ColumnSpan="3"
//                                    />
//                            <Button x:Name="FinishButton" 
//                                    Text="{i18n:Translate FINISH_STRING}"
//                                    FontSize="Medium"
//                                    BackgroundColor="#3f51b5"
//                                    TextColor="White"
//                                    VerticalOptions="CenterAndExpand"
//                                    IsEnabled="{Binding isComplete}"
//                                    IsVisible="{Binding isComplete}"
//                                    CommandParameter="{Binding .}"
//                                    Clicked="YetFinishBtn_Clicked"
//                                    Grid.Column="3"
//                                    />
//                        </Grid>
//                        <Label x:Name="ToDoNameLabel"
//                               Text="{Binding _subtitleName, StringFormat='{0}'}"
//                               Margin="25,0,25,0"
//                               VerticalOptions="Start"
//                               HorizontalOptions="Start"
//                               Padding="0,5,0,5"
//                               FontSize="22"
//                               TextColor="Black"/>
//                    </StackLayout>
//                    </StackLayout>
//                </ViewCell>
//            </DataTemplate>
//            <DataTemplate x:Key="_NotComplete">
//                <ViewCell Height = "60" Tapped="ViewCell_Tapped">
//                    <ViewCell.ContextActions>
//                        <MenuItem Clicked = "ListviewItem_Delete" CommandParameter="{Binding .}"
//                                  Text="Delete" IsDestructive="True"/>
//                    </ViewCell.ContextActions>
//                    <StackLayout BackgroundColor = "{Binding selectedGroupColor}" >
//                        < Grid Margin="10,0,10,0" VerticalOptions="CenterAndExpand">
//                            <Grid.ColumnDefinitions>
//                                <ColumnDefinition Width = "*" />
//                                < ColumnDefinition Width="*"/>
//                                <ColumnDefinition Width = "*" />
//                                < ColumnDefinition Width="*"/>
//                                <ColumnDefinition Width = "*" />
//                            </ Grid.ColumnDefinitions >
//                            < Label x:Name="DptNameLab" 
//								   Text="{Binding DptName, StringFormat='{0}'}"
//                                   VerticalOptions="CenterAndExpand"
//								   FontSize="28"                                     
//								   TextColor="Black" 
//                                   Grid.Column="0" Grid.ColumnSpan="3"/>

//                            <Button x:Name="FinishBtn" 
//									Text="{i18n:Translate FINISH_STRING}" 									
//									FontSize="Medium" 
//                                    IsEnabled="{Binding isComplete}" 
//                                    IsVisible="{Binding isComplete}"
//									Clicked="YetFinishBtn_Clicked" 
//									CommandParameter="{Binding .}" 
//									TextColor="White" 
//									BackgroundColor="#3f51b5" 
//									BorderColor="#3f51b5"
//                                    Grid.Column="3"/>

//                        </Grid>
//                    </StackLayout>
//                </ViewCell>
//            </DataTemplate>
//            <local:RegistrationDataTemplateSelect x:Key="RegistrationDataTemplateSelecter"   
//						NotCompleteTemplate="{StaticResource _NotComplete}"
//                        AddItemDataTemplate="{StaticResource AddItemTemplate}"
//						CompleteTemplate="{StaticResource Complete}" 
//						YetNavigationTemplate="{StaticResource YetNavigation}"/>
//        </ResourceDictionary>
//    </ContentPage.Resources>

//    <ContentPage.Padding>
//        <OnPlatform x:TypeArguments="Thickness">
//            <OnPlatform.Platforms>
//                <On Platform = "iOS" Value="0, 20, 0, 0" />
//                <On Platform = "Android" Value="0, 0, 0, 0" />
//            </OnPlatform.Platforms>
//        </OnPlatform>
//    </ContentPage.Padding>
//    <ContentPage.Content>
//        <AbsoluteLayout x:Name="RigisterListAbsoluteLayout">
//            <Grid x:Name="RigistListGrid">
//                <Grid.RowDefinitions>
//                    <RowDefinition Height = "*" />
//                    < RowDefinition Height="19*"/>

//                </Grid.RowDefinitions>
//                <Label Text = "{i18n:Translate COMMEND_ORDER_STRING}" TextColor="#3f51b5" BackgroundColor="#f2e4e4" 
//                   VerticalOptions="CenterAndExpand" FontSize="Large" Grid.Row="0" Padding="5,0,5,0"/>

//                <!--  <BoxView BackgroundColor = "#3f51b5" HeightRequest="1" VerticalOptions="StartAndExpand" Grid.Row="1"/>-->
//                <ListView Grid.Row="1" x:Name= "RgListView" HasUnevenRows= "True" ItemTapped= "RgListView_ItemTapped"
//                          VerticalScrollBarVisibility= "Always"
//                          ItemTemplate= "{StaticResource RegistrationDataTemplateSelecter}" SeparatorColor= "#3f51b5" >

//                </ ListView >
//            </ Grid >
//            < Button BackgroundColor= "#3f51b5"
//                    x:Name= "AddBtn"
//                    Text= "{i18n:Translate NEW_STRING}"
//                    FontSize= "Medium"
//                    TextColor= "White"
//                    Clicked= "AddBtn_Clicked"
//                    AbsoluteLayout.LayoutFlags= "PositionProportional"
//                    AbsoluteLayout.LayoutBounds= "0.96,0.9,70,70"
//                    FontAttributes= "Bold"
//                    CornerRadius= "35"
//                    />
//            < Button x:Name= "ShiftBtn"
//                    Text= "{i18n:Translate EXCHANGE_STRING}"
//                    BackgroundColor= "#3f51b5"
//                    FontSize= "Medium"
//                    TextColor= "White"
//                    FontAttributes= "Bold"
//                    Clicked= "ShiftBtn_Clicked"
//                    AbsoluteLayout.LayoutFlags= "PositionProportional"
//                    CornerRadius= "35"
//                    AbsoluteLayout.LayoutBounds= "0.96,0.74,70,70" />


//            < Button x:Name= "ExitAddBtn" FontSize= "Large"
//                    Text= "離開醫院"
//                    Clicked= "ExitAddBtn_Clicked"
//                    AbsoluteLayout.LayoutFlags= "All"
//                    AbsoluteLayout.LayoutBounds= "0,1,1,0.1"
//                    BorderColor= "#3f51b5"
//                    BackgroundColor= "#3f51b5"
//                    TextColor= "White" />
//            < !--Text = "{i18n:Translate PAYMENT_MEDICINE_STRING}"-- >
//        </ AbsoluteLayout >
//    </ ContentPage.Content >
//</ local:CustomToolbarContentPage>
#endregion