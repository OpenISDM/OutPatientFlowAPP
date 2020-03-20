using System;
using Rg.Plugins.Popup.Pages;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Rg.Plugins.Popup.Services;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Xml;
using System.IO;
using IndoorNavigation.Resources.Helpers;
using IndoorNavigation.Modules.Utilities;
using System.Resources;
using System.Globalization;
using Plugin.Multilingual;
using Plugin.InputKit.Shared.Configuration;
using IndoorNavigation.Views.PopUpPage;

namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PickCashierPopupPage : PopupPage
    {
        ObservableCollection<DestinationItem> Cashieritems;
        ObservableCollection<DestinationItem> Pharmacyitems;

        CultureInfo currentLanguage= 
			CrossMultilingual.Current.CurrentCultureInfo;
			
        const string _resourceId = "IndoorNavigation.Resources.AppResources";
        ResourceManager _resourceManager =
            new ResourceManager(_resourceId, 
								typeof(TranslateExtension).GetTypeInfo()
								.Assembly);
        App app = (App)Application.Current;

        bool _buttonLock = false;

        public PickCashierPopupPage()
        {
            BackgroundColor = Color.FromRgba(150, 150, 150, 70);
            InitializeComponent();
            Cashieritems = new ObservableCollection<DestinationItem>();
            Pharmacyitems = new ObservableCollection<DestinationItem>();
            LoadData();

            CashierSelectionView.ItemsSource = Cashieritems;
            PharmacySelectionView.ItemsSource = Pharmacyitems;

            

            if (Pharmacyitems.Count <= 1)
                PharmacySelectionView.SelectedItem = Pharmacyitems[0];
            if (Cashieritems.Count <= 1)
                CashierSelectionView.SelectedItem = Cashieritems[0];
        }        
        
        public void LoadData()
        {          
            XmlDocument doc = 
				NavigraphStorage.XmlReader("Yuanlin_OPFM.CashierStation.xml");
				
            XmlNodeList nodeList = doc.GetElementsByTagName("Cashierstation");

            foreach(XmlNode node in nodeList)
            {
                DestinationItem item = new DestinationItem();
				
                item._regionID = 
					new Guid(node.Attributes["region_id"].Value);
					
                item._waypointID = 
					new Guid(node.Attributes["waypoint_id"].Value);
					
                item._floor = node.Attributes["floor"].Value;
				
                item._waypointName = node.Attributes["name"].Value;
                Cashieritems.Add(item); 
            }
            XmlNodeList pharmacyNodeList = 
				doc.GetElementsByTagName("Pharmacystation");
				
            foreach(XmlNode node in pharmacyNodeList)
            {
                DestinationItem item = new DestinationItem();
                item._regionID = new Guid(node.Attributes["region_id"].Value);
				
                item._waypointID = 
					new Guid(node.Attributes["waypoint_id"].Value);
					
                item._floor = node.Attributes["floor"].Value;
				
                item._waypointName = 
					node.Attributes["name"].Value;
					
                Console.WriteLine($"region id is{item._waypointID},"+
								  $" {item._regionID}");
                Pharmacyitems.Add(item);
            }
        }

        async private void CashierOKBtn_Clicked(object sender, EventArgs e)
        {
            if (_buttonLock) return;
            _buttonLock = true;

            var cashier_item = 
				CashierSelectionView.SelectedItem as DestinationItem;
				
            var pharmacy_item = 
				PharmacySelectionView.SelectedItem as DestinationItem;
				
            if(cashier_item==null || pharmacy_item == null)
            {                
                await PopupNavigation.Instance.PushAsync
					(new AlertDialogPopupPage(
						_resourceManager.GetString
							("CASHIER_SHOULD_SELECT_ONE_STRING", 
							 currentLanguage), 
						_resourceManager.GetString
							("OK_STRING",
							 currentLanguage)
						)
					);
                return;
            }
            if (cashier_item != null && pharmacy_item!=null )
            {
                app.records.Insert(app.records.Count-1 ,new RgRecord
                {
                    _waypointID=cashier_item._waypointID,
                    type=RecordType.Cashier,
                    _regionID=cashier_item._regionID,
                    _waypointName=cashier_item._waypointName,
                    DptName=cashier_item._waypointName
                });
                app.records.Insert(app.records.Count - 1, new RgRecord
                {
                    _waypointID=pharmacy_item._waypointID,
                    type=RecordType.Pharmacy,
                    _regionID=pharmacy_item._regionID,
                    _waypointName=pharmacy_item._waypointName,
                    DptName=pharmacy_item._waypointName
                });
                await PopupNavigation.Instance.PopAsync();
                MessagingCenter.Send(this, "GetCashierorNot", true);
            }
            return;
        }

        private void CashierCancelBtn_Clicked(object sender, EventArgs e)
        {            
            MessagingCenter.Send(this, "GetCashierorNot", false);
            PopupNavigation.Instance.PopAllAsync();
        }
        protected override bool OnBackButtonPressed()
        {
            MessagingCenter.Send(this, "GetCashierorNot", false);
            return base.OnBackButtonPressed();
        }
        
    }
}