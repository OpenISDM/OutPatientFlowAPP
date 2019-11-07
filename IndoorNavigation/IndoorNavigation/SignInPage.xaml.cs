using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Reflection;

namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SignInPage : ContentPage
    {

        App app = (App)Application.Current;
        public SignInPage()
        {
            InitializeComponent();

            IDNumEntry.Text = Preferences.Get("ID_NUMBER_STRING", string.Empty);
            PatientIDEntry.Text = Preferences.Get("PATIENT_ID_STRING", string.Empty);
            BirthDayPicker.Date = Preferences.Get("BIRTHDAY_DATETIME", DateTime.Now);

            RgDayPicker.MaximumDate = DateTime.Now;
        }

        async private void Button_Clicked(object sender, EventArgs e)
        {
            //QueryResult result = new QueryResult();
            Preferences.Set("ID_NUMBER_STRING", IDNumEntry.Text);
            Preferences.Set("PATIENT_ID_STRING", PatientIDEntry.Text);
            Preferences.Set("BIRTHDAY_DATETIME", BirthDayPicker.Date);
            await Navigation.PopAsync();
        }

        private void RgDayPicker_DateSelected(object sender, DateChangedEventArgs e)
        {
            TaiwanCalendar calender = new TaiwanCalendar();
            var o = (DatePicker)sender;
            DateTime pickDate;
            if (o != null)
            {
                pickDate = o.Date;

                app.SelectDate = string.Format("{0}{1}", calender.GetYear(pickDate), pickDate.ToString("MMdd"));

                Console.WriteLine("Pick Date is :" + app.SelectDate + "  99999999999999999999");

                ReadXML();
            }
        }

        private void ReadXML()
        {

            if (app._TmpRecords.Count != 0)
            {
                foreach (RgRecord tmprecord in app._TmpRecords)
                {
                    if (app.records.Contains(tmprecord)) app.records.Remove(tmprecord);
                }
            }

            string filename = "PatientData.xml";
            var assembly = typeof(RigisterList).GetTypeInfo().Assembly;
            bool isEmpty = (app.records.Count == 0);

            Stream stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.{filename}");
            using (var reader = new StreamReader(stream))
            {
                var xmlString = reader.ReadToEnd();
                Console.WriteLine(xmlString);


                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlString);

                XmlNodeList records = doc.GetElementsByTagName("RgRecord");
                app._TmpRecords.Clear();
                for (int i = 0; i < records.Count; i++)
                {
                    RgRecord record = new RgRecord();

                    record.OpdDate = records[i].ChildNodes[0].InnerText;

                    if (record.OpdDate != app.SelectDate) continue;
                    record.DptName = records[i].ChildNodes[1].InnerText;
                    record.Shift = records[i].ChildNodes[2].InnerText;
                    record.CareRoom = records[i].ChildNodes[3].InnerText;
                    record.DrName = records[i].ChildNodes[4].InnerText;
                    record.SeeSeq = records[i].ChildNodes[5].InnerText;
                    record.Key = "QueryResult";
                    record._waypointName = record.DptName;
                    record._regionID = new Guid("11111111-1111-1111-1111-111111111111");
                    record._waypointID = new Guid("00000000-0000-0000-0000-000000000002");

                    app._TmpRecords.Add(record);
                    if (isEmpty)
                        app.records.Add(record);
                    else
                        app.records.Insert(app.records.Count - 1, record);
                }
            }
        }
    }
}