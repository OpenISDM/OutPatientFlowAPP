/*
 * 2020 © Copyright (c) BiDaE Technology Inc. 
 * Provided under BiDaE SHAREWARE LICENSE-1.0 in the LICENSE.
 *
 * Project Name:
 *
 *      IndoorNavigation
 *
 * Version:
 *
 *      1.0.0, 20200221
 * 
 * File Name:
 *
 *      RigisterList.xaml.cs
 *
 * Abstract:
 *      
 *
 *      
 * Authors:
 * 
 *      Jason Chang, jasonchang@bidae.tech 
 *      
 */
using IndoorNavigation.Models;
using IndoorNavigation.Models.NavigaionLayer;
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
using IndoorNavigation.Views.PopUpPage;
using static IndoorNavigation.Utilities.Storage;
using IndoorNavigation.Utilities;
using Xamarin.Essentials;
using IndoorNavigation.Resources;
using static IndoorNavigation.Utilities.TmperorayStatus;
namespace IndoorNavigation.Views.OPFM
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OPPAListPage : CustomToolbarContentPage
    {
        #region variable declaration        
        RegisterListViewModel _viewmodel;
        private string _navigationGraphName;
        private XMLInformation _nameInformation;
        //to prevent button multi-tap from causing error
        private bool isButtonPressed = false;
        private ViewCell lastCell = null;
        delegate void MulitItemFinish(RgRecord FinishRecord);
        MulitItemFinish _multiItemFinish;

        App app = (App)Application.Current;
        #endregion

        #region Page lifecycle
        public OPPAListPage(string navigationGraphName, NavigationGraph naviGraph)
        {
            InitializeComponent();
            Console.WriteLine("initalize graph info");

            _navigationGraphName = navigationGraphName;
            _nameInformation = LoadXmlInformation(navigationGraphName);
            BindingContext = _viewmodel;

        }
        protected override void OnAppearing()
        {
            base.OnAppearing();

            Console.WriteLine(">>OnAppearing");
            _viewmodel = new RegisterListViewModel(_navigationGraphName);

            RefreshListView();

            isButtonPressed = false;
            RefreshToolbarOptions();
        }

        #endregion

        #region Clinck Event
        /*this function is to push page to NavigatorPage */
        async private void RgListView_ItemTapped(object sender,
                                                 ItemTappedEventArgs e)
        {
            if (isButtonPressed) return;
            isButtonPressed = true;

            DateTime CurrentDate = DateTime.Now;
            TimeSpan CurrentTimespan = DateTime.Now.TimeOfDay;

            if (e.Item is RgRecord record)
            {
                if (app.OrderDistrict.ContainsKey(record._groupID) &&
                    !(app.OrderDistrict[record._groupID] == record.order ||
                    app.OrderDistrict[record._groupID] == record.order - 1)
                    || (!app.OrderDistrict.ContainsKey(record._groupID) && (record.order != 1)))
                {
                    string BannerName = "";
                    for (int i = app.records.IndexOf(record); i >= 0; i--)
                    {
                        if (app.records[i].order == record.order - 1 &&
                            app.records[i]._groupID == record._groupID)
                        {
                            if (string.IsNullOrEmpty(BannerName))
                            {
                                BannerName = app.records[i].DptName;
                            }
                            else
                            {
                                BannerName =
                                    BannerName + "," + app.records[i].DptName;
                            }
                        }

                    }

                    await PopupNavigation.Instance.PushAsync
                        (new AlertDialogPopupPage(string.Format
                        (GetResourceString("PLEASE_DO_SOMETHING_FIRST_STRING"),
                        BannerName),
                        GetResourceString("OK_STRING")));
                }
                #region if the clinic has open timing.
                else if (record.OpeningHours != null &&
                    !isCareRoomOpening(record.OpeningHours))
                {
                    //do some alert implementation
                    await PopupNavigation.Instance.PushAsync
                        (new AlertDialogPopupPage
                        (GetResourceString("AVAILABLE_CLINICS_NOW_STRING"),
                        GetResourceString("GO_STRING"),
                       GetResourceString("NO_STRING"),
                        "Still go to careroom"));

                    MessagingCenter.Subscribe<AlertDialogPopupPage, bool>
                        (this, "Still go to careroom",
                        async (MsgSender, MsgArgs) =>
                        {
                            Console.WriteLine("Get Subscribe string");

                            if ((bool)MsgArgs)
                            {
                                await Navigation.PushAsync
                                    (new NavigatorPage(_navigationGraphName,
                                               record._regionID,
                                               record._waypointID,
                                               record._waypointName,
                                            _nameInformation));
                                record.isComplete = true;
                            }
                            MessagingCenter
                            .Unsubscribe<AlertDialogPopupPage, bool>
                            (this, "Still go to careroom");
                        });
                }
                #endregion
                else
                {
                    #region If the record has additional string.
                    if (!string.IsNullOrEmpty(record.AdditionalMsg))
                    {
                        await PopupNavigation.Instance.PushAsync
                            (new AlertDialogPopupPage(record.AdditionalMsg,
                            GetResourceString("OK_STRING")));
                    }
                    #endregion
                    await Navigation.PushAsync
                        (new NavigatorPage(_navigationGraphName,
                                           record._regionID,
                                           record._waypointID,
                                           record._waypointName,
                                           _nameInformation)
                        );
                    record.isComplete = true;
                }

            }
            RefreshListView();
            ((ListView)sender).SelectedItem = null;

        }


        /*to show popup page for add route to listview*/
        async private void AddBtn_Clicked(object sender, EventArgs e)
        {
            if (isButtonPressed) return;
            isButtonPressed = true;

            await PopupNavigation.Instance.PushAsync
                (new AddPopupPage(_navigationGraphName));

            isButtonPressed = false;
        }

        private bool CanBeShifted()
        {
            int AddItemNotCompleteCount =
                app.records.Where(p => !p.isAccept && p._groupID == 0).Count();

            if (AddItemNotCompleteCount >= 2)
                return true;

            int GroupNotCompleteCount =
                app.records.Where(p => p._groupID != 0 && !p.isAccept).Count();

            if (AddItemNotCompleteCount >= 1 && GroupNotCompleteCount >= 1)
                return true;
            int currentGroupID = 0;
            int TotalNotComplete = 0;
            foreach (RgRecord record in app.records)
            {
                if (record._groupID == 0) continue;
                if (record._groupID != currentGroupID)
                {
                    int CurrentGroupNotComplete =
                        app.records.Where(p => p._groupID == record._groupID && !p.isAccept).Count();
                    if (CurrentGroupNotComplete > 0)
                        TotalNotComplete++;

                    currentGroupID = record._groupID;
                }
            }
            if (TotalNotComplete >= 2) return true;

            return false;
        }

        async private void ListviewItem_Delete(object sender, EventArgs args)
        {
            var item = (RgRecord)((MenuItem)sender).CommandParameter;

            if (item != null)
            {
                if (item.type == RecordType.Exit || item._groupID != 0)
                {
                    await PopupNavigation.Instance.PushAsync
                        (new AlertDialogPopupPage
                        (GetResourceString("THIS_ITEM_CANT_BE_REMOVE_STRING"),
                        AppResources.OK_STRING));
                }
                else if (app.records.Contains(item))
                    app.records.Remove(item);
            }
        }

        async private void YetFinishBtn_Clicked(object sender, EventArgs e)
        {
            var o = (Button)sender;
            var FinishBtnClickItem = o.CommandParameter as RgRecord;
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
                Console.WriteLine("Current Finish count : " + app.FinishCount);

                if (app.FinishCount == app.records.Count &&
                (app.lastFinished.type != RecordType.Register ||
                (app.lastFinished.type != RecordType.Exit && app.HaveCashier)))
                {
                    if (app.HaveCashier && !LeaveHospitalBtn.IsVisible)
                    {
                        await DisplayAlert(GetResourceString("MESSAGE_STRING"),
                                    GetResourceString("FINISH_SCHEDULE_STRING"),
                                    GetResourceString("OK_STRING"));

                        await PopupNavigation.Instance.PushAsync
                            (new ExitPopupPage(_navigationGraphName));
                    }
                    else if (!app.HaveCashier)
                    {
                        LeaveHospitalBtn.IsEnabled = true;
                        LeaveHospitalBtn.IsVisible = true;
                    }
                }
                int index = app.records.IndexOf(FinishBtnClickItem);

                #region To check finish-able items
                if (app.OrderDistrict.ContainsKey(FinishBtnClickItem._groupID))
                {
                    app.OrderDistrict[FinishBtnClickItem._groupID] = FinishBtnClickItem.order;
                }
                else
                {
                    app.OrderDistrict.Add(FinishBtnClickItem._groupID, FinishBtnClickItem.order);
                }

                for (int i = index; i < app.records.Count; i++)
                {
                    if (FinishBtnClickItem._groupID == 0) break;
                    if (app.records[i].order >= FinishBtnClickItem.order + 2
                        && app.records[i]._groupID == FinishBtnClickItem._groupID)
                        break;

                    if (app.records[i].order == FinishBtnClickItem.order && app.records[i]._groupID == FinishBtnClickItem._groupID
                        && (!app.records[i].isAccept && app.records[i].isComplete))
                        break;
                    if ((app.records[i].order <= FinishBtnClickItem.order + 1) &&
                     app.records[i]._groupID == FinishBtnClickItem._groupID)
                    {
                        app.records[i].isComplete = true;
                    }
                }
                #endregion
                RefreshListView();
            }
        }

        #endregion

        #region  For Get Value
        private Week GetDayofWeek()
        {
            switch (DateTime.Now.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    return Week.Sunday;
                case DayOfWeek.Saturday:
                    return Week.Saturday;
                default:
                    return Week.Workingday;
            }
        }
        private bool isCareRoomOpening(List<OpeningTime> openingTimes)
        {
            // TimeSpan compare function rule
            // t1 < t2 return -1
            // t1 = t2 return 0
            // t1 > t2 return 1
            if (openingTimes.Count <= 0) return true;

            TimeSpan CurrentTime = DateTime.Now.TimeOfDay;
            Week TodayOfWeekDay = GetDayofWeek();

            foreach (OpeningTime openTime in openingTimes)
            {
                if (TodayOfWeekDay != openTime.dayOfWeek)
                    continue;
                int StartTimeCompare =
                    TimeSpan.Compare(CurrentTime, openTime.startTime);
                int EndTimeCompare =
                    TimeSpan.Compare(openTime.endTime, CurrentTime);

                if (StartTimeCompare == 1 && EndTimeCompare == 1)
                    return true;
            }

            return false;
        }
        #endregion

        #region UI View Control
        private void Buttonable(bool enable)
        {
            AddBtn.IsEnabled = enable;
            AddBtn.IsVisible = enable;
            ShiftBtn.IsEnabled = enable;
            ShiftBtn.IsVisible = enable;
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

        private void RefreshListView()
        {
            isButtonPressed = false;
            RgListView.ItemsSource = null;
            RgListView.ItemsSource = app.records;
            SetExitBtn();
        }
        private void ReturnWhiteBackground()
        {
            app.records.All(p =>
            {
                p.selectedGroupColor = Color.Transparent;
                return true;
            });
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
        private void RegisterFinish(RgRecord record)
        {
            ItemFinishFunction(record);
            // here pop up select clinic page.

            RefreshListView();
        }
        async private void ExitFinish(RgRecord record)
        {

            string HopeString =
              $"{GetDisplayName(_navigationGraphName)}" +
              $"\n{GetResourceString("HOPE_STRING")}";
            await PopupNavigation.Instance.PushAsync
                (new AlertDialogPopupPage(HopeString));
            await Navigation.PopAsync();

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
            InfoItemCommand = new Command(async () => await InfoItemMethod());
            ClearItemCommand = new Command(async () => await ClearItemMethod());
            ToolbarItems.Clear();

            ToolbarItem SignInItem =
                new ToolbarItem
                {
                    Text = GetResourceString("ACCOUNT_STRING"),
                    Command = SignInCommand,
                    Order = ToolbarItemOrder.Secondary
                };

            ToolbarItem InfoItem =
                new ToolbarItem
                {
                    Text = GetResourceString("PREFERENCES_STRING"),
                    Command = InfoItemCommand,
                    Order = ToolbarItemOrder.Secondary
                };
            ToolbarItem ClearItem =
                new ToolbarItem
                {
                    Text = GetResourceString("CLEAR_STRING"),
                    Command = ClearItemCommand,
                    Order = ToolbarItemOrder.Primary,
                };
         
            ToolbarItems.Add(InfoItem);
            ToolbarItems.Add(ClearItem);
            OnToolbarItemAdded();

        }
        private async Task InfoItemMethod()
        {
            Console.WriteLine("Preference item click");
            await Navigation.PushAsync
                (new NavigatorSettingPage(_navigationGraphName));
            await Task.CompletedTask;
        }
        private async Task ClearItemMethod()
        {
            if (isButtonPressed) return;
            isButtonPressed = true;

            Console.WriteLine("Clear item click");
            await PopupNavigation.Instance.PushAsync(new AlertDialogPopupPage(GetResourceString("ARE_YOU_SURE_TO_CLEAR_STRING"), GetResourceString("CLEAR_STRING"), GetResourceString("NO_STRING"), "ClearOrNot"));

            MessagingCenter.Subscribe<AlertDialogPopupPage, bool>(this, "ClearOrNot", (MsgSender, MsgArgs) =>
            {
                Console.WriteLine("Get ClearOrNot message.");

                if ((bool)MsgArgs)
                {
                    app.records.Clear();
                    app._TmpRecords.Clear();
                    app.OrderDistrict.Clear();
                    app.FinishCount = 0;
                    app.HaveCashier = false;
                    app.lastFinished = null;
                    app.isRigistered = false;
                    app.getRigistered = false;
                    app.roundRecord = null;
                    TmperorayStatus.ClearAllState();
                    Buttonable(true);
                    OnAppearing();
                }
                else
                {
                    //do nothing;
                }
                MessagingCenter.Unsubscribe<AlertDialogPopupPage, bool>(this, "ClearOrNot");
            });
            isButtonPressed = false;
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
        public ICommand ClearItemCommand { get; set; }
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

        #region New Process of OPPA       
        public ICommand LeaveHospitalCommand { get; set; }

        private void SetExitBtn()
        {
            LeaveHospitalCommand =
                new Command(async () => await ExitBtnMethod());

            LeaveHospitalBtn.Command = LeaveHospitalCommand;
            LeaveHospitalBtn.Text =
                GetResourceString("EXIT_HOSPITAL_STRING");
        }

        private async Task ExitBtnMethod()
        {
            Buttonable(false);

            if (isButtonPressed) return;
            isButtonPressed = true;

            LeaveHospitalBtn.IsVisible = false;
            LeaveHospitalBtn.IsEnabled = false;

            app.HaveCashier = true;

            //show select exit popup page. 
            await PopupNavigation.Instance.PushAsync
                (new ExitPopupPage(_navigationGraphName));
            //remember to implement cancel messagingcenter  

            MessagingCenter.Subscribe<ExitPopupPage, bool>(this, "ExitCancel",
                (MsgSender, MsgArgs) =>
                {
                    if (!(bool)MsgArgs)
                    {
                        app.HaveCashier = false;
                        Buttonable(true);
                        LeaveHospitalBtn.IsEnabled = true;
                        LeaveHospitalBtn.IsVisible = true;
                    }
                    MessagingCenter.Unsubscribe<ExitPopupPage, bool>
                    (this, "ExitCancel");
                });

            isButtonPressed = false;
            await Task.CompletedTask;
        }
        #endregion

        #region Prevent double-click event
        //private TimeSpan _doubleClickTimespan = new TimeSpan(0, 0, 1);
        //private int _buttonClickCount = 0;


        //private delegate void Button_ClickedEvent(object sender, EventArgs e);
        //private void HandleDoubleClick(Button_ClickedEvent clickEvent)
        //{
        //    if (_buttonClickCount < 1)
        //    {
        //        Device.StartTimer(_doubleClickTimespan, CheckClickCallback(clickEvent)
        //    }
        //}
        //private bool CheckClickCallback(Button_ClickedEvent clickEvent)
        //{
        //    if (_buttonClickCount > 1) 
        //    {
        //        Console
        //    }
        //    else
        //    {

        //    }
        //    _buttonClickCount = 0;
        //    return false;
        //}

        #endregion
    }
}
