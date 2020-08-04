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

            //When constructor run, we need to load the site item first.            
            //LoadFakeData();

            Device.BeginInvokeOnMainThread(async () =>
            {
                await LoadData();
                RefreshListView();
            });
        }

        async private Task LoadData()
        {
            CloudDownload _download = new CloudDownload();
            INetworkSetting networkSetting = 
                DependencyService.Get<INetworkSetting>();
            _allNewSiteItems = new List<GraphInfo>();

            IndicatorPopupPage busyPopupPage = new IndicatorPopupPage();

            await PopupNavigation.Instance.PushAsync(busyPopupPage);
            bool isConnectable = await networkSetting.CheckInternetConnect();

            if (isConnectable)
            {
                string SupportResourceXmlString = 
                    _download.Download(_download.getSupportListUrl());

                try
                {
                    XmlDocument xmlDoc = new XmlDocument();

                    xmlDoc.LoadXml(SupportResourceXmlString);

                    _serverResources = GraphInfoReader(xmlDoc);

                    _serverResources.All(p =>
                    {
                        Console.WriteLine("Current add : " + p.Key);
                        Console.WriteLine("Current add : " + 
                            p.Value._displayName);

                        _allNewSiteItems.Add(p.Value);
                        return true;
                    });
                }
                catch(Exception exc)
                {
                    Console.WriteLine("Parsing SupportList error : " 
                        + exc.Message);

                    //show error string.
                    await Navigation.PopAsync();
                }

                finally
                {
                    await PopupNavigation.Instance.RemovePageAsync
                        (busyPopupPage);
                }
            }
            else // if no network available
            {
                await PopupNavigation.Instance.RemovePageAsync(busyPopupPage);
                //show toast told user that server no response or no network.
                //await Navigation.PopAsync();
            } // if no network available
            await Task.CompletedTask;            
        }
                 
        private void LoadFakeData()
        {
            _allNewSiteItems = new List<GraphInfo>();

            _allNewSiteItems.Add(new GraphInfo
            {
                _graphName = "Lab",
                _currentVersion = 2222,
                _displayNames = new Dictionary<string, string>()
                {
                    {"en-US", "Lab" },
                    {"zh-TW", "實驗場域" }
                }
            });

            _allNewSiteItems.Add(new GraphInfo
            {
                //isSelected = false,
                _graphName="aaaa",              
                _currentVersion =11,
                _displayNames= new Dictionary<string, string>()
                {
                    {"en-US", "aaaa" },
                    {"zh-TW", "欸欸欸欸" }
                }
                
            }); ;

            _allNewSiteItems.Add(new GraphInfo
            {
                //isSelected = false,
                _graphName="bbbb",
                _currentVersion=22,
                _displayNames = new Dictionary<string, string>()
                {
                    {"en-US", "BBBB" },
                    {"zh-TW","逼逼逼逼"  }
                }
            }) ;
            _allNewSiteItems.Add(new GraphInfo
            {
                //isSelected = false,
                _graphName = "aaaa",
                _currentVersion = 11,
                _displayNames = new Dictionary<string, string>()
                {
                    {"en-US", "aaaa" },
                    {"zh-TW", "欸欸欸欸" }
                }

            }); 
            _allNewSiteItems.Add(new GraphInfo
            {
                //isSelected = false,
                _graphName = "aaaa",
                _currentVersion = 11,
                _displayNames = new Dictionary<string, string>()
                {
                    {"en-US", "aaaa" },
                    {"zh-TW", "欸欸欸欸" }
                }

            }); 
            _allNewSiteItems.Add(new GraphInfo
            {
                _graphName = "aaaa",
                _currentVersion = 11,
                _displayNames = new Dictionary<string, string>()
                {
                    {"en-US", "aaaa" },
                    {"zh-TW", "欸欸欸欸" }
                }

            }); 
            _allNewSiteItems.Add(new GraphInfo
            {
                //isSelected = false,
                _graphName = "aaaa",
                _currentVersion = 11,
                _displayNames = new Dictionary<string, string>()
                {
                    {"en-US", "aaaa" },
                    {"zh-TW", "欸欸欸欸" }
                }

            }); ;
            AddNewSiteListView.ItemsSource = _allNewSiteItems;
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
            //throw new NotImplementedException();
        }
    }
    #region Classes and Enums    
   
    #endregion
}