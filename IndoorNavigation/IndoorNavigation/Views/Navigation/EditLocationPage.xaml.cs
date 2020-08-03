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
            INetworkSetting networkSetting = 
                DependencyService.Get<INetworkSetting>();
            CloudDownload _serverDownload = new CloudDownload();

            _allNewSiteItems = new List<GraphInfo>();

            bool isConnectable = await networkSetting.CheckInternetConnect();

            if (isConnectable)
            {
                string SupportList = 
                    _serverDownload.Download
                    (_serverDownload.getSupportListUrl());

                if (!string.IsNullOrEmpty(SupportList))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(SupportList);
                    Dictionary<string, GraphInfo> _serverResource = 
                        GraphInfoReader(doc);

                    foreach(KeyValuePair<string,GraphInfo> pair in _serverResource)
                    {
                        Console.WriteLine("Pair key : " + pair.Key);
                        Console.WriteLine("Pair Value : " + pair.Value._displayName);

                        pair.Value._siteSourceFrom = SiteSourceFrom.Server;
                        _allNewSiteItems.Add(pair.Value);
                    }
                        
                }

                _localResources.All(p =>
                {
                    p.Value._siteSourceFrom = SiteSourceFrom.Local;
                    _allNewSiteItems.Add(p.Value);
                    return true;
                });
            }
            else
            {
                //show please check network.
            }

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
                        Console.WriteLine("Get the msg : Doyouwant to download it");
                        if ((bool)MsgArgs)
                        {
                            //Check version first then download.

                            // start to download.
                            IndicatorPopupPage busyPage = 
                                new IndicatorPopupPage();

                            await PopupNavigation.Instance
                            .PushAsync(busyPage);

                            switch (selectedItem._siteSourceFrom)
                            {
                                case SiteSourceFrom.Local:
                                    Console.WriteLine("Download Local");
                                    EmbeddedGenerateFile
                                    (selectedItem._graphName);
                                    break;
                                case SiteSourceFrom.Server:
                                    try
                                    {
                                        Console.WriteLine("Download Server");
                                        CloudGenerateFile
                                        (selectedItem._graphName);
                                    } catch (Exception exc)
                                    {
                                        Console.WriteLine
                                        ("Error - server download : " 
                                        + exc.Message);
                                        await PopupNavigation.Instance.RemovePageAsync(busyPage);
                                        await PopupNavigation.Instance.PushAsync(new AlertDialogPopupPage("aaaa","OK"));
                                        MessagingCenter.Unsubscribe<AlertDialogPopupPage, bool>
                       (this, "Doyouwant to download it");
                                        return;
                                    }
                                    break;

                            }

                            //CloudGenerateFile(selectedItem._graphName);
                            //EmbeddedGenerateFile(selectedItem._graphName);
                            await PopupNavigation.Instance
                            .RemovePageAsync(busyPage);

                            await PopupNavigation.Instance.PushAsync
                            (new AlertDialogPopupPage("下載成功", "OK"));
                        }
                        MessagingCenter.Unsubscribe<AlertDialogPopupPage, bool>
                        (this, "Doyouwant to download it");
                    });
                RefreshListView();
            }          
        }

        private void RefreshListView()
        {
            AddNewSiteListView.ItemsSource = null;
            AddNewSiteListView.ItemsSource = _allNewSiteItems;
        }
    }
    #region Classes and Enums    
   
    #endregion
}