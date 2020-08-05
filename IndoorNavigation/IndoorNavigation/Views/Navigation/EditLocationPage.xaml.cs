using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static IndoorNavigation.Utilities.Storage;
using IndoorNavigation.Views.PopUpPage;
using Rg.Plugins.Popup.Services;
using System.Threading;
using IndoorNavigation.Models;
using IndoorNavigation.Modules.Utilities;
using Dijkstra.NET.Model;
using System.Xml;

namespace IndoorNavigation.Views.Navigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditLocationPage : ContentPage
    {
        List<GraphInfo> _allNewSiteItems;

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
            //Device.BeginInvokeOnMainThread(async () =>
            //{
            //    await LoadData();
            //    RefreshListView();
            //});
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
                //AddNewSiteListView.ItemsSource = listviewItemSources;
                RefreshListView();
            }
            else
            {
                AddNewSiteListView.ItemsSource=
                    _allNewSiteItems.Where(p => p._displayName.Contains(e.NewTextValue));                
            }
            AddNewSiteListView.EndRefresh();
        }                
        
     
        private void AddNewSiteListView_ItemSelected(object sender, 
            SelectedItemChangedEventArgs e)
        {
            AddNewSiteListView.SelectedItem = null;
        }

        async private void AddNewSiteListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {           
            if(e.Item is GraphInfo selectedItem)
            {
                if (isOlderVersion(selectedItem))
                {
                    await PopupNavigation.Instance
                       .PushAsync(new AlertDialogPopupPage
                       (string.Format("現有版本大於或等於伺服器上的版本，是否要繼續下載? : {0}",
                       selectedItem._displayName),
                       "沒錯",
                       "不是",
                       "Version is older"));

                    MessagingCenter.Subscribe<AlertDialogPopupPage, bool>
                        (this, "Version is older",async(MsgSender, MsgArgs) =>
                     {
                         if ((bool)MsgArgs)
                         {
                             await DownloadSiteFile(selectedItem);
                         }

                         MessagingCenter
                         .Unsubscribe<AlertDialogPopupPage, bool>
                         (this, "Version is older");
                     });
                }
                else 
                {
                    await PopupNavigation.Instance
                        .PushAsync(new AlertDialogPopupPage
                        (string.Format("您要下載的是 : {0}",
                        selectedItem._displayName),
                        "沒錯",
                        "不是",
                        "Doyouwant to download it"));

                    MessagingCenter.Subscribe<AlertDialogPopupPage, bool>(this,
                    "Doyouwant to download it", async (MsgSender, MsgArgs) =>
                    {
                        if ((bool)MsgArgs)
                        {
                            await DownloadSiteFile(selectedItem);
                        }
                        MessagingCenter.Unsubscribe<AlertDialogPopupPage, bool>
                        (this, "Doyouwant to download it");
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

            await PopupNavigation.Instance
            .PushAsync(busyPage);

            try
            {
                Console.WriteLine("Download from server");
                CloudGenerateFile(selectedItem._graphName);
                await PopupNavigation.Instance.PushAsync
                    (new AlertDialogPopupPage("Download success", "Ok"));
            }
            catch (Exception exc)
            {
                Console.WriteLine("Download error - "
                    + exc.Message);
                await PopupNavigation.Instance.PushAsync
                    (new AlertDialogPopupPage("download error", "Ok"));
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
    }
    #region Classes and Enums    
   
    #endregion
}