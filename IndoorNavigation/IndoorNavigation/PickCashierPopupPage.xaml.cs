using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rg.Plugins.Popup.Pages;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using IndoorNavigation.Models;
using Rg.Plugins.Popup.Services;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Xml;
using System.IO;
namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PickCashierPopupPage : PopupPage
    {
        ObservableCollection<DestinationItem> Cashieritems;
        ObservableCollection<DestinationItem> Pharmacyitems;
        App app = (App)Application.Current;
        SelectionView sv,Pharmacysv;
        //PickCahsierPopPageViewModel _viewmodel;
        public PickCashierPopupPage()
        {
            BackgroundColor = Color.FromRgba(150, 150, 150, 70);
            InitializeComponent();
            Cashieritems = new ObservableCollection<DestinationItem>();
            Pharmacyitems = new ObservableCollection<DestinationItem>();
            LoadData();

            sv = new SelectionView
            { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center, ItemsSource=Cashieritems,
              ColumnNumber=1, SelectionType=SelectionType.RadioButton, RowSpacing=7
            };
            Pharmacysv = new SelectionView
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                ItemsSource = Pharmacyitems,
                ColumnNumber = 1,
                SelectionType = SelectionType.RadioButton,
                RowSpacing = 7
            };
            SelectionStack.Children.Add(sv);
            SelectionStack.Children.Add(Pharmacysv);
        }

        public void LoadData()
        {
            string filename = "CashierStation.xml";
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
                Pharmacyitems.Add(item);
            }
        }

        async private void CashierOKBtn_Clicked(object sender, EventArgs e)
        {
            var o = sv.SelectedItem as DestinationItem;
            if (o != null )
            {
                app.records.Insert(app.records.Count-1 ,new RgRecord
                {
                    _waypointID=o._waypointID,
                    Key= "Cashier",
                    _regionID=o._regionID,
                    _waypointName=o._waypointName,
                    DptName=o._waypointName
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
        protected override bool OnBackgroundClicked()
        {
            GobackPage(true); 
            return base.OnBackgroundClicked();
        }
    }
}