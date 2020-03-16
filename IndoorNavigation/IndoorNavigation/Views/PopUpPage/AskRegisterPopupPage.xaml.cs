using System;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using System.Resources;
using IndoorNavigation.Resources.Helpers;
using Plugin.Multilingual;
using System.Globalization;
using IndoorNavigation.Models;
using System.Reflection;
using System.Threading.Tasks;
using IndoorNavigation.Views.Navigation;
using IndoorNavigation.Models.NavigaionLayer;
using IndoorNavigation.Modules.Utilities;
namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AskRegisterPopupPage : PopupPage
    {
        private App app = (App)Application.Current;
        private bool ButtonLock;

        const string _resourceId = "IndoorNavigation.Resources.AppResources";
        ResourceManager _resourceManager =
            new ResourceManager(_resourceId, 
								typeof(TranslateExtension).GetTypeInfo()
								.Assembly);
        CultureInfo currentLanguage = 
			CrossMultilingual.Current.CurrentCultureInfo;
        INetworkSetting networkSettings;

        string _navigationGraphName;
        XMLInformation _XmlInfo;

        PhoneInformation phoneInformation = new PhoneInformation();

        public AskRegisterPopupPage(string navigraphName)
        {
            InitializeComponent();
            //BackgroundColor = Color.FromRgba(150, 150, 150, 70);

            _navigationGraphName = navigraphName;
            _XmlInfo = NavigraphStorage.LoadInformationML
                (phoneInformation.GiveCurrentMapName(_navigationGraphName) +
                 "_info_" + phoneInformation.GiveCurrentLanguage() + ".xml");
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();

            ButtonLock = false;
        }

        private void BusyShow(bool isBusy)
        {
            BusyIndicator.IsRunning = isBusy;
            BusyIndicator.IsVisible = isBusy;
            BusyIndicator.IsEnabled = isBusy;
        }

        async private void RegisterCancelBtn_Clicked(object sender, EventArgs e)
        {
            if (ButtonLock) return;
            ButtonLock = true;

            BusyShow(true);

            networkSettings = DependencyService.Get<INetworkSetting>();
            bool network_ability =await networkSettings.CheckInternetConnect();
            if(network_ability)
                await CancelorClickBack();
            else
            {
                await PopupNavigation.Instance
					  .PushAsync(new AlertDialogPopupPage
					  (_resourceManager.GetString("BAD_NETWORK_STRING",
												  currentLanguage)));
                ButtonLock = false;
                return;
            }
            BusyShow(false);
           PopupNavigation.Instance.PopAllAsync();
        }

        async private void RegisterOKBtn_Clicked(object sender, EventArgs e)
        {
            if (ButtonLock) return;
            ResetAllState();
            ButtonLock = true;
            app.getRigistered = true;

            RgRecord record = new RgRecord
            {
                DptName =
                    _resourceManager.GetString("NAVIGATE_TO_REGISTER_STRING",
                                               currentLanguage),
                _regionID = new Guid("22222222-2222-2222-2222-222222222222"),
                _waypointID = new Guid("00000000-0000-0000-0000-000000000018"),

                _waypointName =
                    _resourceManager.GetString("REGISTERED_COUNTER_STRING",
                                               currentLanguage),
                type = RecordType.Register,
                isComplete = true
            };
            app.records.Add(record);
            //app.records.Add(new RgRecord {type=RecordType.NULL});
            MessagingCenter.Send(this, "isReset", true);
            ButtonLock = true;
            await Navigation.PushAsync(new NavigatorPage(_navigationGraphName,record._regionID,record._waypointID,record._waypointName,_XmlInfo));
            await PopupNavigation.Instance.PopAllAsync();
        }
         
        protected override bool OnBackgroundClicked()
        {
            return false;            
        }
        protected override bool OnBackButtonPressed()
        {
            return true;           
        }

        HttpRequest request = new HttpRequest();
        async private Task CancelorClickBack()
        {
            ResetAllState();
            app.getRigistered = false;

            request.GetXMLBody();
            await request.RequestData();
            MessagingCenter.Send(this, "isReset", true);            
        }
        
        private void ResetAllState()
        {
            app.records.Clear();
            app._TmpRecords.Clear();
            app.HaveCashier = false;
            app.FinishCount = 0;
            app.roundRecord = null;
            app.lastFinished = null;
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            OnBackButtonPressed();
        }
    }
}