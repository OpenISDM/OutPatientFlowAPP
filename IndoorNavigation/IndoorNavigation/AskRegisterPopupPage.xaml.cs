using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AskRegisterPopupPage : PopupPage
    {
        private App app = (App)Application.Current;
        private HttpRequest request;
        public AskRegisterPopupPage()
        {
            InitializeComponent();
            request = new HttpRequest();

        }

        async private void RegisterCancelBtn_Clicked(object sender, EventArgs e)
        {
            //ReadXml();
            request.GetXMLBody();
            request.RequestData();
            request.ResponseXmlParse();
            foreach (RgRecord record in app._TmpRecords)
                app.records.Add(record);
            app.records.Add(new RgRecord { Key = "NULL" });
            await PopupNavigation.Instance.PopAllAsync();
        }

        async private void RegisterOKBtn_Clicked(object sender, EventArgs e)
        {
            app.isRegister = true;
            app.records.Add(new RgRecord
            {
                DptName = "導航至掛號台",
                _regionID = new Guid("11111111-1111-1111-1111-111111111111"),
                _waypointID = new Guid("00000000-0000-0000-0000-000000000002"),
                _waypointName = "掛號台",
                Key = "register"
            });
            app.records.Add(new RgRecord { Key = "NULL" });
            await PopupNavigation.Instance.PopAllAsync();
        }

        protected override bool OnBackgroundClicked()
        {
            //ReadXml();
            request.GetXMLBody();
            request.RequestData();
            request.ResponseXmlParse();
            foreach (RgRecord record in app._TmpRecords)
                app.records.Add(record);
            app.records.Add(new RgRecord { Key = "NULL" });
            return base.OnBackgroundClicked();
        }
        protected override bool OnBackButtonPressed()
        {
            //ReadXml();
            request.GetXMLBody();
            request.RequestData();
            request.ResponseXmlParse(); foreach (RgRecord record in app._TmpRecords)
                app.records.Add(record);
            app.records.Add(new RgRecord { Key = "NULL" });
            return base.OnBackButtonPressed();
        }
        //private void ReadXml()
        //{

        //    if (app._TmpRecords.Count != 0)
        //    {
        //        foreach (RgRecord tmprecord in app._TmpRecords)
        //        {
        //            if (app.records.Contains(tmprecord)) app.records.Remove(tmprecord);
        //        }
        //    }
        //    string filename = "PatientData.xml";
        //    var assembly = typeof(RigisterList).GetTypeInfo().Assembly;


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
        //            RgRecord record = new RgRecord();

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