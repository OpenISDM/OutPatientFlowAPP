using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Resources;
using IndoorNavigation.Resources.Helpers;
using System.Reflection;
using IndoorNavigation.Views.Navigation;
using Plugin.Multilingual;
using IndoorNavigation.Models.NavigaionLayer;
using IndoorNavigation.Modules.Utilities;
using Rg.Plugins.Popup;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Rg.Plugins.Popup.Extensions;
using IndoorNavigation.ViewModels;
using IndoorNavigation.Models;
using System.IO;
using System.Xml.Linq;
using System.Xml;
using System.Globalization;
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
        private bool HavePayment = false;
        Object tmp=null;
        App app = (App)Application.Current;
        private bool isButtonPressed = false; //to prevent button multi-tap from causing some error
        private ViewCell lastCell=null;
        private RgRecord lastFinished = null;
        const string resourceId = "IndoorNavigation.Resources.AppResources";

        //------to test date time----------
        private TaiwanCalendar calendar;
        private DateTime date;
        //-----------------------------------
        public RigisterList(string navigationGraphName,QueryResult result)
        {
            InitializeComponent();
            //_viewmodel = new RegisterListViewModel();
            calendar = new TaiwanCalendar();
            date = new DateTime(2019, 7, 8);
            //date=calendar.add
            app.FinishCount = 0;
            app.records = new ObservableCollection<RgRecord>();


            _navigationGraphName = navigationGraphName;
            _navigationGraph = NavigraphStorage.LoadNavigationGraphXML(navigationGraphName);
            app.roundRecord = null;

            if (CrossMultilingual.Current.CurrentCultureInfo.ToString() == "en" || CrossMultilingual.Current.CurrentCultureInfo.ToString() == "en-US")
            {
                _nameInformation = NavigraphStorage.LoadInformationML(navigationGraphName + "_info_en-US.xml");
                FontSizeSet(Device.GetNamedSize(NamedSize.Small,typeof(Button)));
            }
            else if (CrossMultilingual.Current.CurrentCultureInfo.ToString() == "zh" || CrossMultilingual.Current.CurrentCultureInfo.ToString() == "zh-TW")
            {
                _nameInformation = NavigraphStorage.LoadInformationML(navigationGraphName + "_info_zh.xml");
                FontSizeSet(Device.GetNamedSize(NamedSize.Medium, typeof(Button)));
            }
            
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
                    var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
                    //await DisplayAlert(_resourceManager.GetString("MESSAGE_STRING",currentLanguage), _resourceManager.GetString("PHARMACY_ALERT_STRING", currentLanguage), _resourceManager.GetString("OK_STRING",currentLanguage));
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
            //var test = ((ListView)sender).
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
            var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
            if (app.FinishCount+1 >= app.records.Count - 1)
            {
                //await DisplayAlert(_resourceManager.GetString("MESSAGE_STRING", currentLanguage), _resourceManager.GetString("NO_SHIFT_STRING", currentLanguage),
                //    _resourceManager.GetString("OK_STRING", currentLanguage));
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
            //DeleteBtn.IsEnabled = !DeleteBtn.IsEnabled;
            //DeleteBtn.IsVisible = !DeleteBtn.IsVisible;
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
        private void PaymemtListBtn_Clicked(object sender, EventArgs e)
        {
            Buttonable();
            app.records.Insert(app.FinishCount, new RgRecord
            {
                DptName="領藥",
                _waypointID=new Guid("00000000-0000-0000-0000-000000000002"),
                _regionID=new Guid("11111111-1111-1111-1111-111111111111"),
                _waypointName= "領藥櫃臺",
                Key = "Pharmacy"
            });
            app.records.Insert(app.FinishCount,new RgRecord
            {
                DptName="批價",
                _waypointID = new Guid("00000000-0000-0000-0000-000000000002"),
                _regionID = new Guid("11111111-1111-1111-1111-111111111111"),
                _waypointName = "批價櫃臺",
                Key = "Cashier"
            });
            PaymemtListBtn.IsEnabled = false;
            PaymemtListBtn.IsVisible = false;
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
                if (Message == false) HavePayment = false;
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

            if (_viewmodel==null || !_viewmodel.isCheckRegister)
            {
                _viewmodel = new RegisterListViewModel();
            }

            //app.records = app.records.Distinct().ToList;
           // app.records.GroupBy(x => x.DptName).Select(x => x.First());

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
                ReadXml();
                //app.records.Add(new RgRecord { Key = "NULL" });
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

                string s = string.Format("{0}\n{1}", _resourceManager.GetString("THANK_COMING_STRING", currentLanguage),
                    _resourceManager.GetString("HOPE_STRING", currentLanguage));
                //await DisplayAlert(_resourceManager.GetString("MESSAGE_STRING"), s, _resourceManager.GetString("OK_STRING", currentLanguage));
                await PopupNavigation.Instance.PushAsync(new DisplayAlertPopupPage(s));
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
                if (HavePayment && !PaymemtListBtn.IsEnabled)
                {
                    HavePayment = false;

                    await DisplayAlert(_resourceManager.GetString("MESSAGE_STRING", currentLanguage),_resourceManager.GetString("FINISH_SCHEDULE_STRING",currentLanguage),
                        _resourceManager.GetString("OK_STRING", currentLanguage));
                    //Console.WriteLine("Stack count is:" + PopupNavigation.Instance.PopupStack.Count);
                    //await PopupNavigation.Instance.PushAsync(new DisplayAlertPopupPage(_resourceManager.GetString("FINISH_SCHEDULE_STRING",currentLanguage)));
                    
                    //Console.WriteLine("Stack count is:" + PopupNavigation.Instance.PopupStack.Count);
                    //int i = PopupNavigation.Instance.PopupStack.Count;
                    //while (i > 0) i = PopupNavigation.Instance.PopupStack.Count;
                    await PopupNavigation.Instance.PushAsync(new ExitPopupPage());
                }
                else
                {
                    PaymemtListBtn.IsVisible = true;
                    PaymemtListBtn.IsEnabled = true;
                    HavePayment = true;
                }
            }
            lastFinished = index;
        }


        protected override bool OnBackButtonPressed()
        {
           
            return base.OnBackButtonPressed();
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

        //private void DeleteBtn_Clicked(object sender, EventArgs e)
        //{
        //    RgListView.ItemTapped -= RgListView_ItemTapped;
        //    RgListView.ItemTapped += DeletList_ItemTapped;
        //    Buttonable();
        //}

        private void DeletList_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var item = e.Item as RgRecord;

            if (item != null)
            {
                app.records.Remove(item);
            }
            RgListView.ItemTapped -= DeletList_ItemTapped;
            RgListView.ItemTapped += RgListView_ItemTapped;
            Buttonable();
            return;
        }
        private void FontSizeSet(double i)
        {
            //DeleteBtn.FontSize = i;
            ShiftBtn.FontSize = i;
            AddBtn.FontSize = i;
        }

        private void MenuItem_Clicked(object sender, EventArgs e)
        {
            var item = (RgRecord)((MenuItem)sender).CommandParameter;

            if (item != null)
            {
                app.records.Remove(item);
            }
        }

        private void ReadXml()
        {
            string filename = "PatientData.xml";
            var assembly = typeof(RigisterList).GetTypeInfo().Assembly;
            bool isEmpty = (app.records.Count == 0);

            Stream stream=assembly.GetManifestResourceStream($"{assembly.GetName().Name}.{filename}");
            using (var reader=new StreamReader(stream))
            {
                var xmlString = reader.ReadToEnd();
                Console.WriteLine(xmlString);

                XDocument xd = XDocument.Parse(xmlString);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlString);

                XmlNodeList records = doc.GetElementsByTagName("RgRecord");

                for(int i=0;i<records.Count;i++)
                {
                    RgRecord record = new RgRecord();

                    record.OpdDate = records[i].ChildNodes[0].InnerText;
                    record.DptName = records[i].ChildNodes[1].InnerText;
                    record.Shift = records[i].ChildNodes[2].InnerText;
                    record.CareRoom = records[i].ChildNodes[3].InnerText;
                    record.DrName=records[i].ChildNodes[4].InnerText;
                    record.SeeSeq = records[i].ChildNodes[5].InnerText;
                    record.Key = "QueryResult";
                    record._waypointName = record.DptName;
                    record._regionID = new Guid("11111111-1111-1111-1111-111111111111");
                    record._waypointID = new Guid("00000000-0000-0000-0000-000000000002");
                    if (isEmpty)
                        app.records.Add(record);
                    else
                        app.records.Insert(app.records.Count - 1, record);
                }

               
            }
        }
    }
}