using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using System.Resources;
using IndoorNavigation.Resources.Helpers;
using System.Reflection;
using IndoorNavigation.Views.Navigation;
using Plugin.Multilingual;
using IndoorNavigation.Models.NavigaionLayer;
using IndoorNavigation.Modules.Utilities;
using Rg.Plugins.Popup.Services;
using IndoorNavigation.ViewModels;
using System.Globalization;

using SuaveControls.Views;
namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RigisterList : ContentPage
    {
        RegisterListViewModel _viewmodel;
        private string _navigationGraphName;
        private NavigationGraph _navigationGraph;
        public ResourceManager _resourceManager = new ResourceManager(resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);
        private XMLInformation _nameInformation;
        //private bool HavePayment = false;
        Object tmp=null;
        App app = (App)Application.Current;
        private bool isButtonPressed = false; //to prevent button multi-tap from causing some error
        private ViewCell lastCell=null;
        private RgRecord lastFinished = null;
        const string resourceId = "IndoorNavigation.Resources.AppResources";
        private HttpRequest request;
        CultureInfo currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
        PhoneInformation phoneInformation;

        public RigisterList(string navigationGraphName)
        {
            InitializeComponent();
            app._TmpRecords = new ObservableCollection<RgRecord>();
            phoneInformation = new PhoneInformation();
            _navigationGraphName = navigationGraphName;
            Console.WriteLine("wwwwwwwwww");
            _navigationGraph = NavigraphStorage.LoadNavigationGraphXML(phoneInformation.GiveCurrentMapName(_navigationGraphName));                       
            request = new HttpRequest();
            Console.WriteLine("ttttttttttttt");
            _nameInformation = NavigraphStorage.LoadInformationML(phoneInformation.GiveCurrentMapName(_navigationGraphName) + "_info_" + phoneInformation.GiveCurrentLanguage() + ".xml");
            //if (CrossMultilingual.Current.CurrentCultureInfo.ToString() == "en" || CrossMultilingual.Current.CurrentCultureInfo.ToString() == "en-US")
            //{
            //    _nameInformation = NavigraphStorage.LoadInformationML(navigationGraphName + "_info_en-US.xml");
            //    FontSizeSet(Device.GetNamedSize(NamedSize.Small,typeof(Button)));
            //}
            //else if (CrossMultilingual.Current.CurrentCultureInfo.ToString() == "zh" || CrossMultilingual.Current.CurrentCultureInfo.ToString() == "zh-TW")
            //{
            //    _nameInformation = NavigraphStorage.LoadInformationML(navigationGraphName + "_info_zh.xml");
            //    FontSizeSet(Device.GetNamedSize(NamedSize.Medium, typeof(Button)));
            //}
            PaymemtListBtn.IsEnabled = (app.FinishCount + 1 == app.records.Count);
            PaymemtListBtn.IsVisible = (app.FinishCount + 1 == app.records.Count);
            BindingContext = _viewmodel;
        } 
      
        /*this function is to push page to NavigatorPage */
        async private void RgListView_ItemTapped(object sender, ItemTappedEventArgs e)  
        {
            if (isButtonPressed) return;
            isButtonPressed = true;
            
            
            
            if (e.Item is DestinationItem destination)
            {
                Console.WriteLine(">> Handle_ItemTapped in DestinationPickPage");
                var index = app.records.IndexOf(e.Item as RgRecord);
                var o = e.Item as RgRecord;

                if(o.Key.Equals("Pharmacy") && !lastFinished.Key.Equals("Cashier"))
                {    
                    await PopupNavigation.Instance.PushAsync(new DisplayAlertPopupPage(_resourceManager.GetString("PHARMACY_ALERT_STRING", currentLanguage)));
                    ((ListView)sender).SelectedItem = null;
                    RgListView.ItemsSource = null;
                    RgListView.ItemsSource = app.records;
                    isButtonPressed = false;
                    return;
                }
                o.isComplete = true;
               
                await Navigation.PushAsync(new NavigatorPage(_navigationGraphName,
                                                            destination._regionID,
                                                            destination._waypointID,
                                                            destination._waypointName,
                                                            _nameInformation
                                                             ));
            }

            //Deselect Item
            ((ListView)sender).SelectedItem = null;
            RgListView.ItemsSource = null;
            RgListView.ItemsSource = app.records;
        }

        /*this function is to implement a simply shift function.  
          when shift button is clicked, the function will become the listview tapped event.*/
        private void RgListViewShift_ItemTapped(object sender,ItemTappedEventArgs e)
        {
                if (tmp == null)
                {
                    tmp = e.Item as RgRecord;
                }
                else
                {
                    var o = e.Item as RgRecord;
                   
                    int index1 = app.records.IndexOf(tmp as RgRecord);
                    int index2 = app.records.IndexOf(o as RgRecord);
                    //swap
                    app.records[index1] = o as RgRecord;
                    app.records[index2] = tmp as RgRecord;
                    // retrieve original function.
                    RgListView.ItemTapped -= RgListViewShift_ItemTapped;
                    RgListView.ItemTapped += RgListView_ItemTapped;
                    tmp = null;
                    Buttonable();
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
                    await PopupNavigation.Instance.PushAsync(new ShiftAlertPopupPage());
                }
                RgListView.ItemTapped -= RgListView_ItemTapped;

                RgListView.ItemTapped += RgListViewShift_ItemTapped;

                Buttonable();
            }
            
        }

        /*the function is to disable those two float button to keep from triggering something wrong.*/
        private void Buttonable()
        {
            ShiftBtn.IsEnabled = !ShiftBtn.IsEnabled;
            ShiftBtn.IsVisible = !ShiftBtn.IsVisible;
            AddBtn.IsEnabled = !AddBtn.IsEnabled;
            AddBtn.IsVisible = !AddBtn.IsVisible;
            //NavigationPageButton.IsEnabled = !NavigationPageButton.IsEnabled;
            //NavigationPageButton.IsVisible = !NavigationPageButton.IsVisible;
            return;
        }

        /*the function is a button event, which could push page to SignInPage*/
        async private void SignInItem_Clicked(object sender, EventArgs e)
        {
            if (isButtonPressed) return;

            isButtonPressed = true;
            await Navigation.PushAsync(new SignInPage());
        }

        /*the function is a button event to add payment and medicine recieving route to listview*/
        async private void PaymemtListBtn_Clicked(object sender, EventArgs e)
        {
            Buttonable();
            //app.records.Insert(app.FinishCount, new RgRecord
            //{
            //    DptName="領藥",
            //    _waypointID=new Guid("00000000-0000-0000-0000-000000000002"),
            //    _regionID=new Guid("11111111-1111-1111-1111-111111111111"),
            //    _waypointName= "領藥櫃臺",
            //    Key = "Pharmacy"
            //});
            //app.records.Insert(app.FinishCount,new RgRecord
            //{
            //    DptName="批價",
            //    _waypointID = new Guid("00000000-0000-0000-0000-000000000002"),
            //    _regionID = new Guid("11111111-1111-1111-1111-111111111111"),
            //    _waypointName = "批價櫃臺",
            //    Key = "Cashier"
            //});
            //PaymemtListBtn.IsEnabled = false;
            //PaymemtListBtn.IsVisible = false;

            if (isButtonPressed) return;
            isButtonPressed = true;
            PaymemtListBtn.IsEnabled = false;
            PaymemtListBtn.IsVisible = false;
            await PopupNavigation.Instance.PushAsync(new PickCashierPopupPage());

            MessagingCenter.Subscribe<PickCashierPopupPage, bool>(this, "GetCashierorNot", (Messagesender, Messageargs) =>
            {
                Console.WriteLine("Subscribe recive message!");
                bool Message = (bool)Messageargs;
                if (Message) Buttonable();
                PaymemtListBtn.IsEnabled = Message;
                PaymemtListBtn.IsVisible = Message;
                isButtonPressed = false;
            });
            //MessagingCenter.Unsubscribe<PickCashierPopupPage, bool>(this, "GetCashierorNot");
          
        }

        /*to show popup page for add route to listview*/
        async private void AddBtn_Clicked(object sender, EventArgs e)
        {
            if (isButtonPressed) return;

            isButtonPressed = true;
            PaymemtListBtn.IsEnabled = false;
            PaymemtListBtn.IsVisible = false;
            await PopupNavigation.Instance.PushAsync(new AddPopupPage());
            //await Navigation.PushPopupAsync(new AddPopupPage());
            MessagingCenter.Subscribe<AddPopupPage,bool>(this, "AddAnyOrNot",(Messagesender,Messageargs)=> 
            {
                bool Message = (bool)Messageargs;
                if (Message == false) app.HaveCashier = false;
                PaymemtListBtn.IsEnabled = Message;
                PaymemtListBtn.IsVisible = Message;
            });

            MessagingCenter.Subscribe<AddPopupPage, bool>(this, "isBack", (MessageSender, MessageArgs) => 
            {
                isButtonPressed = false;
            });
        }
        
        /*to refresh listview Template and check whether user have sign in or not.*/
        protected override void OnAppearing()
        {      
            base.OnAppearing();

            //if (_viewmodel==null && !app.isRigistered)
            if(!app.isRigistered)
            {
                _viewmodel = new RegisterListViewModel();
            }
            //to refresh listview template 
            RgListView.ItemsSource = null;      
            RgListView.ItemsSource = app.records;

            isButtonPressed = false;
            //app.records = ToObservableCollection<RgRecord>(app.records.Distinct());
        }
        /*this function is a button event, which is to check user whether have arrive at destination.*/
        async private void YetFinishBtn_Clicked(object sender, EventArgs e)
        {
            var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
            var o = (Button)sender;
            var index = o.CommandParameter as RgRecord;

            if (index.Key.Equals("register"))
            {
                ReadXml(1);
                //await PopupNavigation.Instance.PushAsync(new DisplayAlertPopupPage("您今日掛號了兩個門診，包含家醫科跟耳鼻喉科。建議先看家醫科 張曉明醫師"));
                await PopupNavigation.Instance.PushAsync(new AskOrderPopupPage(_navigationGraphName));
                index.isAccept = true;
                index.isComplete = true;
                app.FinishCount++;
                RgListView.ItemsSource = null;
                RgListView.ItemsSource = app.records;
                
                return;
            }
            else if (index.Key.Equals("exit"))
            {
                //show msg to say goodbye

                string s = string.Format("{0}\n{1}", phoneInformation.GetBuildingName(_navigationGraphName),
                    _resourceManager.GetString("HOPE_STRING", currentLanguage));
                //await PopupNavigation.Instance.PushAsync(new DisplayAlertPopupPage(_resourceManager.GetString("HOPE_STRING", currentLanguage),false)) ;
                await PopupNavigation.Instance.PushAsync(new DisplayAlertPopupPage(s,false));
                await Navigation.PopAsync();
                index.isAccept = true;
                index.isComplete = true;
                RgListView.ItemsSource = null;
                RgListView.ItemsSource = app.records;
                return;
            }
            else if(index.Key.Equals("QueryResult"))
            {
                app.roundRecord = index;
            }

            if(index != null)   // click finish button , it will refresh certain cell template
            {
                index.isComplete = true;
                index.isAccept = true;
                app.FinishCount++;
                RgListView.ItemsSource = null;
                RgListView.ItemsSource = app.records;
            }

            if (app.FinishCount+1 == (app.records.Count)) //when all item is finished, enable pay/get medicine button
            {
                if (app.HaveCashier && !PaymemtListBtn.IsEnabled)
                {
                    app.HaveCashier = false;

                    await DisplayAlert(_resourceManager.GetString("MESSAGE_STRING", currentLanguage),_resourceManager.GetString("FINISH_SCHEDULE_STRING",currentLanguage),
                        _resourceManager.GetString("OK_STRING", currentLanguage));
                   
                    await PopupNavigation.Instance.PushAsync(new ExitPopupPage(_navigationGraphName));
                }
                else
                {
                    PaymemtListBtn.IsVisible = true;
                    PaymemtListBtn.IsEnabled = true;
                    app.HaveCashier = true;
                }
            }
            lastFinished = index;
        }

        async private void InfoItem_Clicked(object sender, EventArgs e)
        {
            if (isButtonPressed) return;

            isButtonPressed = true;
            await Navigation.PushAsync(new NavigatorSettingPage());
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

        private void FontSizeSet(double i)
        {
            //DeleteBtn.FontSize = i;
            ShiftBtn.FontSize = i;
            AddBtn.FontSize = i;
            //NavigationPageButton.FontSize = i;
        }

        private void MenuItem_Clicked(object sender, EventArgs e)
        {
            var item = (RgRecord)((MenuItem)sender).CommandParameter;

            if (item != null)
            {
                app.records.Remove(item);
            }
        }

        private void ReadXml(int i)
        {
            int index = app.records.Count - 1;
            app.FinishCount = 0;
            RgRecord record1 = new RgRecord
            {
                DptName = "耳鼻喉科",
                DrName = "李曉華",
                _waypointID = new Guid("11111111-1111-1111-1111-111111111111"),
                _regionID = new Guid("11111111-1111-1111-1111-111111111111"),
                Key = "QueryResult",
                Shift = "上午",
                CareRoom = "243診",
                SeeSeq = "17",
                _waypointName = "243診"
            };
            RgRecord record2 = new RgRecord
            {
                DptName = "家庭醫學科",
                DrName = "張曉明",
                _waypointID = new Guid("11111111-1111-1111-1111-111111111111"),
                _regionID = new Guid("11111111-1111-1111-1111-111111111111"),
                Key = "QueryResult",
                Shift = "上午",
                CareRoom = "203診",
                SeeSeq = "6",
                _waypointName = "203診"
            };
            app.records.Insert(index++, record2);
            app.records.Insert(index, record1);

            app._TmpRecords.Add(record2);
            app._TmpRecords.Add(record1);
            
            RgListView.ItemsSource = null;
            RgListView.ItemsSource = app.records;
        }

        private void ReadXml()
        {
            Console.WriteLine("Now Excution is::: ReadXml");
            if (app._TmpRecords.Count > 0)
            {
                foreach (RgRecord tmprecord in app._TmpRecords)
                {
                    //if(!app.records[app.records.IndexOf(tmprecord)].isAccept) return;
                    if (app.records.Contains(tmprecord)) app.records.Remove(tmprecord);
                    
                }
            }
            app._TmpRecords.Clear();
            Console.WriteLine("Now Excution is::: Todo request to server");
            request.GetXMLBody();
            request.RequestData();
            request.ResponseXmlParse();
            Console.WriteLine("Now Excution is::: Get insert index");
            int index = (app.records.Count == 0) ? 0 : app.records.Count - 1;
            Console.WriteLine("Now Excution is::: GetParse And put data to listView");
            foreach (RgRecord record in app._TmpRecords)
            {
                app.records.Insert(index, record);
            }
            RgListView.ItemsSource = null;
            RgListView.ItemsSource = app.records;
        }

        async private void PreViewLayoutItem_Clicked(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PushAsync(new PickCashierPopupPage());
        }

        async private void NavigationPageButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new NavigationHomePage(_navigationGraphName));
        }
    }
}