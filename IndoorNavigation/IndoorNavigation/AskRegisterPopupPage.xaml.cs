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
using System.Reflection;

namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AskRegisterPopupPage : PopupPage
    {
        private App app = (App)Application.Current;
        private HttpRequest request;
        private bool ButtonLock;

        const string _resourceId = "IndoorNavigation.Resources.AppResources";
        ResourceManager _resourceManager =
            new ResourceManager(_resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);
        CultureInfo currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;

        NetworkAccess networkState = Connectivity.NetworkAccess;
        public AskRegisterPopupPage()
        {
            InitializeComponent();
            BackgroundColor = Color.FromRgba(150, 150, 150, 70);
            request = new HttpRequest();

        }
        protected override void OnAppearing()
        {
            base.OnAppearing();

            ButtonLock = false;
        }
        async private void RegisterCancelBtn_Clicked(object sender, EventArgs e)
        {
            networkState=Connectivity.NetworkAccess;
            if(networkState==NetworkAccess.Internet)
                CancelorClickBack();
            else
            {
                //await DisplayAlert("info", "沒有網路~", "Ok");
                await PopupNavigation.Instance.PushAsync(new DisplayAlertPopupPage(_resourceManager.GetString("NO_NETWORK_STRING", currentLanguage), true));
                return;
            }
            //ReadXml();
            //if (ButtonLock) return;
            //ButtonLock = true;
            //app.FinishCount = 0;
            //app.records.Clear();
            //app._TmpRecords.Clear();

            //request.GetXMLBody();
            //request.RequestData();
            ////request.ResponseXmlParse();
            //foreach (RgRecord record in app._TmpRecords)
            //    app.records.Add(record);
            //app.records.Add(new RgRecord { Key = "NULL" });
            await PopupNavigation.Instance.PopAllAsync();
        }

        async private void RegisterOKBtn_Clicked(object sender, EventArgs e)
        {
            if (ButtonLock) return;
            app.records.Clear();
            app._TmpRecords.Clear();
            app.HaveCashier = false;
            app.roundRecord = null;
            app.FinishCount = 0;
            ButtonLock = true;
            app.getRigistered = true;
            app.records.Add(new RgRecord
            {
                DptName =_resourceManager.GetString("NAVIGATE_TO_REGISTER_STRING", currentLanguage),
                _regionID = new Guid("11111111-1111-1111-1111-111111111111"),
                _waypointID = new Guid("00000000-0000-0000-0000-000000000002"),
                _waypointName = "掛號台",
                Key = "register"
            });
            app.records.Add(new RgRecord { Key = "NULL" });
            MessagingCenter.Send(this, "isReset", true);
            await PopupNavigation.Instance.PopAllAsync();
        }

        protected override bool OnBackgroundClicked()
        {
            networkState = Connectivity.NetworkAccess;
            if (networkState == NetworkAccess.Internet)
            {
                CancelorClickBack();
            }
            else
            {
                PopupNavigation.Instance.PushAsync(new DisplayAlertPopupPage(_resourceManager.GetString("NO_NETWORK_STRING", currentLanguage), true));
                return false;
            }
            return base.OnBackgroundClicked();
        }
        protected override bool OnBackButtonPressed()
        {
            networkState = Connectivity.NetworkAccess;
            if (networkState == NetworkAccess.Internet)
            {
                CancelorClickBack();
            }
            else
            {
                PopupNavigation.Instance.PushAsync(new DisplayAlertPopupPage(_resourceManager.GetString("NO_NETWORK_STRING", currentLanguage), true));
                return true;
            }
            return base.OnBackButtonPressed();
        }

        private void CancelorClickBack()
        {
            app.getRigistered = false;
            app.records.Clear();
            app._TmpRecords.Clear();
            app.HaveCashier = false;
            app.FinishCount = 0;
            app.roundRecord = null;
            request.GetXMLBody();
            request.RequestData();
            MessagingCenter.Send(this, "isReset", true);
        }
      
        

        //    Stream stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.{filename}");
        //    using (var reader = new StreamReader(stream))
        //    {
        //        var xmlString = reader.ReadToEnd();
        //        Console.WriteLine(xmlString);

        //        XDocument xd = XDocument.Parse(xmlString);
        //        XmlDocument doc = new XmlDocument();
        //        doc.LoadXml(xmlString);
        //        Console.WriteLine(doc.InnerText);
        //        app._TmpRecords.Clear();
        //        //XmlNodeList records = doc.SelectNodes("QueryResult/RgRecords/RgRecord/DptName");
        //        XmlNodeList records = doc.GetElementsByTagName("RgRecord");

        //        for (int i = 0; i < records.Count; i++)
        //        {
        //            //Console.WriteLine(records[i].ChildNodes[0].InnerText);
        //            RgRecord record = new RgRecord();掛號

        //            record.OpdDate = records[i].ChildNodes[0].InnerText;
        //            if (record.OpdDate != app.SelectDate) continue;
        //            record.DptName = records[i].ChildNodes[1].InnerText;
        //            record.Shift = records[i].ChildNodes[2].InnerText;
        //            record.CareRoom = records[i].ChildNodes[3].InnerText;
        //            record.DrName = records[i].ChildNodes[4].InnerText;
        //            record.SeeSeq = records[i].ChildNodes[5].InnerText;
        //            record.Key = "QueryResult";
        //            record._waypointName = record.DptName;
        //            record._regionID = new Guid("11111111-1111-1111-1111-111111111111");
        //            record._waypointID = new Guid("00000000-0000-0000-0000-000000000002");
        //            app._TmpRecords.Add(record);
        //            app.records.Add(record);
        //        }
        //    }
        //}
    }
}