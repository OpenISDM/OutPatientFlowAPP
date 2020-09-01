using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static IndoorNavigation.Utilities.Storage;
using IndoorNavigation.Views.PopUpPage;
using Rg.Plugins.Popup.Services;
using IndoorNavigation.Models;
using System.Xml;
using Xamarin.Essentials;
using System.Resources;
using IndoorNavigation.Resources.Helpers;
using System.Reflection;

namespace IndoorNavigation.Views.Navigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditLocationPage : ContentPage
    {
        List<GraphInfo> _allNewSiteItems;
        const string _resourceId =
            "IndoorNavigation.Resources.AppResources";

        ResourceManager _resourceManager =
            new ResourceManager(_resourceId,
                                typeof(TranslateExtension).GetTypeInfo()
                                .Assembly);

        public EditLocationPage()
        {
            InitializeComponent();
            _allNewSiteItems = new List<GraphInfo>();
            LoadData();
            RefreshListView();
        }
        public EditLocationPage(XmlDocument xmlDocument)
        {
            InitializeComponent();

            //When constructor run, we need to load the site item first.            
            //LoadFakeData();
            _allNewSiteItems = new List<GraphInfo>();
            LoadData(xmlDocument);
            RefreshListView();           
        }

        private void LoadData()
        {
            if (_serverResources != null)
            {
                _serverResources.All(p =>
                {
                    _allNewSiteItems.Add(p.Value);
                    return true;
                });
            }
        }
        private void LoadData(XmlDocument doc)
        {
            _serverResources = GraphInfoReader(doc);

            _serverResources.All(p =>
            {
                _allNewSiteItems.Add(p.Value);
                return true;
            });
        }        

        #region View Event define
        private void AddNewSite_TextChanged(object sender, 
            TextChangedEventArgs e)
        {
            AddNewSiteListView.BeginRefresh();
            if (string.IsNullOrEmpty(e.NewTextValue))
            {                
                RefreshListView();
            }
            else
            {
                AddNewSiteListView.ItemsSource=
                    _allNewSiteItems.Where
                    (p => p._displayName.Contains(e.NewTextValue));                
            }
            AddNewSiteListView.EndRefresh();
        }                
        
     
        private void AddNewSiteListView_ItemSelected(object sender, 
            SelectedItemChangedEventArgs e)
        {
            AddNewSiteListView.SelectedItem = null;
        }

        async private void AddNewSiteListView_ItemTapped(object sender, 
            ItemTappedEventArgs e)
        {           
            if(e.Item is GraphInfo selectedItem)
            {
                if (isOlderVersion(selectedItem))
                {
                    await PopupNavigation.Instance
                       .PushAsync(new AlertDialogPopupPage
                       (GetResourceString("ASK_STILL_DOWNLOAD_STRING"),
                       GetResourceString("YES_STRING"),
                       GetResourceString("NO_STRING"),
                       "VersionIsOlder"));

                    MessagingCenter.Subscribe<AlertDialogPopupPage, bool>
                        (this, "VersionIsOlder", async(MsgSender, MsgArgs) =>
                     {
                         if ((bool)MsgArgs)
                         {
                             await DownloadSiteFile(selectedItem);
                         }

                         MessagingCenter
                         .Unsubscribe<AlertDialogPopupPage, bool>
                         (this, "VersionIsOlder");
                     });
                }
                else 
                {
                    await PopupNavigation.Instance
                        .PushAsync(new AlertDialogPopupPage
                        (string.Format(
                            GetResourceString("DOWNLOAD_CHECK_STRING"),
                            selectedItem._displayName),
                        GetResourceString("YES_STRING"),
                        GetResourceString("NO_STRING"),
                        "DoYouWantToDownloadIt"));

                    MessagingCenter.Subscribe<AlertDialogPopupPage, bool>(this,
                    "DoYouWantToDownloadIt", async (MsgSender, MsgArgs) =>
                    {
                        if ((bool)MsgArgs)
                        {
                            await DownloadSiteFile(selectedItem);
                        }
                        MessagingCenter.Unsubscribe<AlertDialogPopupPage, bool>
                        (this, "DoYouWantToDownloadIt");
                    });
                }                
                RefreshListView();
            }          
        }
        #endregion
        private void RefreshListView()
        {
            AddNewSiteListView.ItemsSource = null;
            AddNewSiteListView.ItemsSource = _allNewSiteItems;
        }
        
        async private Task DownloadSiteFile(GraphInfo selectedItem)
        {
            IndicatorPopupPage busyPage =
                                new IndicatorPopupPage();

            await PopupNavigation.Instance.PushAsync(busyPage);
            try
            {
                Console.WriteLine("Download from server");
                CloudGenerateFile(selectedItem._graphName);
                await PopupNavigation.Instance.PushAsync
                    (new AlertDialogPopupPage(GetResourceString
                    ("DOWNLOAD_SUCCESS_STRING"), 
                    GetResourceString
                    ("OK_STRING")));
            }
            catch (Exception exc)
            {
                Console.WriteLine("Download error - "
                    + exc.Message);
                if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    INetworkSetting setting =
                        DependencyService.Get<INetworkSetting>();

                    await PopupNavigation.Instance.PushAsync
                                   (new AlertDialogPopupPage
                                   (GetResourceString("BAD_NETWORK_STRING"),
                                   GetResourceString("GO_TO_SETTING")
                                   , GetResourceString("NO_STRING"), 
                                   "GoToSettingInEdit"));
                    MessagingCenter.Subscribe
                        <AlertDialogPopupPage, bool>
                        (this, "GoToSettingInEdit", (msgSender, msgArgs) =>
                        {
                            if ((bool)msgArgs)
                            {
                                setting =
                                DependencyService
                                .Get<INetworkSetting>();

                                setting.OpenSettingPage();
                            }

                            MessagingCenter.Unsubscribe
                            <AlertDialogPopupPage, bool>
                            (this, "GoToSettingInEdit");
                        });
                }
                else
                {
                    await PopupNavigation.Instance.PushAsync
                             (new AlertDialogPopupPage
                             (GetResourceString("HAPPEND_ERROR_STRING"), 
                             GetResourceString("RETRY_STRING"),
                             GetResourceString("NO_STRING"), 
                             "WantRetryInEdit"));
                    MessagingCenter.Subscribe
                        <AlertDialogPopupPage, bool>
                        (this, "WantRetryInEdit", async (msgSender, msgArgs) =>
                        {
                            if ((bool)msgArgs)
                                await DownloadSiteFile(selectedItem);

                            MessagingCenter.Unsubscribe
                                    <AlertDialogPopupPage, bool>
                                    (this, "WantRetryInEdit");
                        });
                }
            }

            await PopupNavigation.Instance
            .RemovePageAsync(busyPage);

            await Task.CompletedTask;            
        }
        private bool isOlderVersion(GraphInfo selectedItem)
        {
            if(_resources._graphResources.ContainsKey(selectedItem._graphName)
                &&
                selectedItem._currentVersion<=
                _resources._graphResources[selectedItem._graphName]
                ._currentVersion)
            {
                return true;
            }
            return false;
        }

        private string GetResourceString(string key)
        {
            return _resourceManager.GetString(key, _currentCulture);
        }
        
    }  
}