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
using System.Resources;
using System.Globalization;
using Plugin.Multilingual;

namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PickCashierPopupPage : PopupPage
    {
        ObservableCollection<DestinationItem> Cashieritems;
        ObservableCollection<DestinationItem> Pharmacyitems;

        CultureInfo currentLanguage= CrossMultilingual.Current.CurrentCultureInfo;
        const string _resourceId = "IndoorNavigation.Resources.AppResources";
        ResourceManager _resourceManager =
            new ResourceManager(_resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);
        App app = (App)Application.Current;
 
        //PickCahsierPopPageViewModel _viewmodel;
        public PickCashierPopupPage()
        {
            BackgroundColor = Color.FromRgba(150, 150, 150, 70);
            InitializeComponent();
            Cashieritems = new ObservableCollection<DestinationItem>();
            Pharmacyitems = new ObservableCollection<DestinationItem>();
            LoadData();

            CashierSelectionView.ItemsSource = Cashieritems;
            PharmacySelectionView.ItemsSource = Pharmacyitems;
            //SetViewOfPage();

            if (Pharmacyitems.Count <= 1)
                PharmacySelectionView.SelectedItem = Pharmacyitems[0];
            if (Cashieritems.Count <= 1)
                CashierSelectionView.SelectedItem = Cashieritems[0];
        }        
        
        public void LoadData()
        {
            string filename = "Yuanlin_OPFM.CashierStation.xml";
            var assembly = typeof(PickCashierPopupPage).GetTypeInfo().Assembly;
            String ContentString = "";
            Stream stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.{filename}");

            using (var reader = new StreamReader(stream))
            {
                ContentString = reader.ReadToEnd();
            }
            stream.Close();

            XmlDocument doc = new XmlDocument();

            doc.LoadXml(ContentString);
            XmlNodeList nodeList = doc.GetElementsByTagName("Cashierstation");

            foreach(XmlNode node in nodeList)
            {
                DestinationItem item = new DestinationItem();
                item._regionID = new Guid(node.Attributes["region_id"].Value);
                item._waypointID = new Guid(node.Attributes["waypoint_id"].Value);
                item._floor = node.Attributes["floor"].Value;
                item._waypointName = node.Attributes["name"].Value;
                Cashieritems.Add(item); 
            }
            XmlNodeList pharmacyNodeList = doc.GetElementsByTagName("Pharmacystation");
            foreach(XmlNode node in pharmacyNodeList)
            {
                DestinationItem item = new DestinationItem();
                item._regionID = new Guid(node.Attributes["region_id"].Value);
                item._waypointID = new Guid(node.Attributes["waypoint_id"].Value);
                item._floor = node.Attributes["floor"].Value;
                item._waypointName = node.Attributes["name"].Value;
                Console.WriteLine($"region id is{item._waypointID}, {item._regionID}");
                Pharmacyitems.Add(item);
            }
        }

        async private void CashierOKBtn_Clicked(object sender, EventArgs e)
        {            
            var cashier_item = CashierSelectionView.SelectedItem as DestinationItem;
            var pharmacy_item = PharmacySelectionView.SelectedItem as DestinationItem;
            if(cashier_item==null || pharmacy_item == null)
            {
                string alertmeg ="批價與領藥都要選擇一個";
                await PopupNavigation.Instance.PushAsync(new DisplayAlertPopupPage(alertmeg));
                return;
            }
            if (cashier_item != null )
            {
                app.records.Insert(app.records.Count-1 ,new RgRecord
                {
                    _waypointID=cashier_item._waypointID,
                    Key= "Cashier",
                    _regionID=cashier_item._regionID,
                    _waypointName=cashier_item._waypointName,
                    DptName=cashier_item._waypointName
                });
                app.records.Insert(app.records.Count - 1, new RgRecord
                {
                    _waypointID=pharmacy_item._waypointID,
                    Key="Pharmacy",
                    _regionID=pharmacy_item._regionID,
                    _waypointName=pharmacy_item._waypointName,
                    DptName=pharmacy_item._waypointName
                });
                //await DisplayAlert("bb",$"waypoint={o._waypointID.ToString()}, region={o._regionID.ToString()}", "ok");
                await PopupNavigation.Instance.PopAsync();
                MessagingCenter.Send(this, "isBack", true);
            }
            return;
        }

        private void CashierCancelBtn_Clicked(object sender, EventArgs e)
        {
            GobackPage(true);
            PopupNavigation.Instance.PopAllAsync();
        }
        private void GobackPage(bool ConfirmOrCancel)
        {
            MessagingCenter.Send(this, "GetCashierorNot", ConfirmOrCancel);
            MessagingCenter.Send(this, "isBack", true);
            PopupNavigation.Instance.PopAsync();
        }
        protected override bool OnBackButtonPressed()
        {
            GobackPage(true);
            return base.OnBackButtonPressed();
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            GobackPage(true);
            PopupNavigation.Instance.PopAsync();
        }

        protected override bool OnBackgroundClicked()
        {
            GobackPage(true); 
            return base.OnBackgroundClicked();
        }
    }
}