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
using IndoorNavigation.ViewModels;
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

        private bool HaveCheckRegister=false;

        const string resourceId = "IndoorNavigation.Resources.AppResources";

        public RigisterList(string navigationGraphName,QueryResult result)
        {
            InitializeComponent();
          //  _viewmodel = new RegisterListViewModel();
            app.FinishCount = 0; 
          
            _navigationGraphName = navigationGraphName;
            _navigationGraph = NavigraphStorage.LoadNavigationGraphXML(navigationGraphName);
            app.roundRecord = null;

            if (CrossMultilingual.Current.CurrentCultureInfo.ToString() == "en" || CrossMultilingual.Current.CurrentCultureInfo.ToString() == "en-US")
            {
                _nameInformation = NavigraphStorage.LoadInformationML(navigationGraphName + "_info_en-US.xml");
            }
            else if (CrossMultilingual.Current.CurrentCultureInfo.ToString() == "zh" || CrossMultilingual.Current.CurrentCultureInfo.ToString() == "zh-TW")
            {
                _nameInformation = NavigraphStorage.LoadInformationML(navigationGraphName + "_info_zh.xml");
            }

            

       /*     app.records = LoadData();
            RgListView.ItemsSource = app.records;*/
            
        }

        ObservableCollection<RgRecord> LoadData()   //for fake data
         {
            ObservableCollection<RgRecord> rgs = new ObservableCollection<RgRecord>();

            rgs.Add(new RgRecord
            {
                Date = "10-10",
                DptName = "心臟內科",
                Shift = "50",
                CareRoom = "0205",
                DptTime = "8:30~10:00",
                _waypointName="心臟內科",
                DrName = "waston",
                SeeSeq = "50",
                Key = "QueryResult",
                isAccept = false,
                isComplete=false
            }); ;

            rgs.Add(new RgRecord
            {
                Date="10-11",
                DptName="復健醫學科",
                DptTime = "8:30~10:00",
                Shift ="55",
                CareRoom="102",
                DrName="gary",
                SeeSeq="2",
                Key= "QueryResult",
                
                isAccept = false,
                isComplete = false
            });

       

            rgs.Add(new RgRecord
            {
                Date = "10-12",
                DptName = "婦產科",
                DptTime = "8:30~10:00",
                Shift = "1",
                DrName = "pp",
                CareRoom="202",
                SeeSeq = "83",
                Key= "QueryResult",
                isComplete = true,
                isAccept = false
            }) ;
            rgs.Add(new RgRecord
            {
                Key = "NULL"
            });

            rgs.Add(new RgRecord
            {
                Key = "NULL"
            });
            return rgs;
         }


        async private void RgListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
           
            if (e.Item is DestinationItem destination)
            {
                Console.WriteLine(">> Handle_ItemTapped in DestinationPickPage");
                var index = app.records.IndexOf(e.Item as RgRecord);
               
                await Navigation.PushAsync(new TestPage(e.Item as DestinationItem, index));
                /*await Navigation.PushAsync(new NavigatorPage(_navigationGraphName,
                                                             destination._regionID,
                                                             destination._waypointID,
                                                             destination._waypointName,
                                                             _nameInformation
                                                             ));*/
            }
           
            //Deselect Item
            ((ListView)sender).SelectedItem = null;
           
            //var test = ((ListView)sender).
        }
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

                    app.records[index1] = o as RgRecord;
                    app.records[index2] = tmp as RgRecord;

                    RgListView.ItemTapped -= RgListViewShift_ItemTapped;
                    RgListView.ItemTapped += RgListView_ItemTapped;
                    tmp = null;
                    Buttonable();
                }  
        }
         async private void ShiftBtn_Clicked(object sender, EventArgs e)
        {
            var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
            if (app.FinishCount+2 >= app.records.Count - 1)
            {
                await DisplayAlert(_resourceManager.GetString("MESSAGE_STRING", currentLanguage), _resourceManager.GetString("NO_SHIFT_STRING", currentLanguage),
                    _resourceManager.GetString("OK_STRING", currentLanguage));
            }
            else
            {
                RgListView.ItemTapped -= RgListView_ItemTapped;

                RgListView.ItemTapped += RgListViewShift_ItemTapped;

                Buttonable();
            }
        }

        private void Buttonable()
        {
            ShiftBtn.IsEnabled = !ShiftBtn.IsEnabled;
            ShiftBtn.IsVisible = !ShiftBtn.IsVisible;
            AddBtn.IsEnabled = !AddBtn.IsEnabled;
            AddBtn.IsVisible = !AddBtn.IsVisible;
        }

        async private void SignInItem_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SignInPage());
        }

        private void PaymemtListBtn_Clicked(object sender, EventArgs e)
        {
            Buttonable();
            app.records.Insert(app.FinishCount, new RgRecord
            {
                DptName="批價",
                Key= "AddItem"
            });
            app.records.Insert(app.FinishCount,new RgRecord
            {
                DptName="領藥",
                Key= "AddItem"
            });
            PaymemtListBtn.IsEnabled = false;
        }

        async private void AddBtn_Clicked(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PushAsync(new AddPopupPage());
        }

        protected override void OnAppearing()
        {      
            base.OnAppearing();

            if (HaveCheckRegister)
                _viewmodel = new RegisterListViewModel();
            else
                HaveCheckRegister = true;

            //to refresh listview template 
            RgListView.ItemsSource = null;      
            RgListView.ItemsSource = app.records;       
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }

        async private void YetFinishBtn_Clicked(object sender, EventArgs e)
        {
            var o = (Button)sender;
            var index = o.CommandParameter as RgRecord;

            if(index != null)   // click finish button , it will refresh certain cell template
            {
                index.isComplete = true;
                index.isAccept = true;
                app.FinishCount++;
                RgListView.ItemsSource = null;
                RgListView.ItemsSource = app.records;
            }

            if (app.FinishCount+2 == (app.records.Count)) //when all item is finished, enable pay/get medicine button
            {
                if (HavePayment && !PaymemtListBtn.IsEnabled)
                {
                    HavePayment = false;
                    var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
                    await DisplayAlert(_resourceManager.GetString("MESSAGE_STRING", currentLanguage),_resourceManager.GetString("FINISH_SCHEDULE_STRING",currentLanguage),
                        _resourceManager.GetString("OK_STRING", currentLanguage));
                    await PopupNavigation.Instance.PushAsync(new ExitPopupPage());
                }
                else
                {
                    PaymemtListBtn.IsEnabled = true;
                    HavePayment = true;
                }
                
            }
           
            
        }
        async private void ToQureryData()
        {
            await DisplayAlert("訊息", "尚未製作", "OK");
        }
        protected override bool OnBackButtonPressed()
        {
            return base.OnBackButtonPressed();
        }
    }
}