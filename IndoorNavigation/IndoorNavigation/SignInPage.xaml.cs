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
            
            //IDNumEntry.Text = Preferences.Get("ID_NUMBER_STRING", string.Empty);
            //PatientIDEntry.Text = Preferences.Get("PATIENT_ID_STRING", string.Empty);
            //BirthDayPicker.Date = Preferences.Get("BIRTHDAY_DATETIME", DateTime.Now);
            //RgDayPicker.MaximumDate = DateTime.Now;
            //if ((app.isRegister && app.FinishCount >= 2) || (!app.isRegister && app.FinishCount >= 1))
            //{
            //    RgDayPicker.IsEnabled = false;
            ////    return;
            //}
            //if (RgDayPicker.Date!=app.time)
            //    RgDayPicker.Date = Preferences.Get("RGDAY_DATETIME",app.time);


            Console.WriteLine(CheckIDLegal("Q123456789")?"Correct":"Errorrrrrrrrrrrr");
        }

        async private void Button_Clicked(object sender, EventArgs e)
        {
            ////QueryResult result = new QueryResult();
            //Preferences.Set("ID_NUMBER_STRING", IDNumEntry.Text);
            //Preferences.Set("PATIENT_ID_STRING", PatientIDEntry.Text);
            //Preferences.Set("BIRTHDAY_DATETIME", BirthDayPicker.Date);
            //Preferences.Set("RGDAY_DATETIME",RgDayPicker.Date);
            await Navigation.PopAsync();
        }
        
        private void RgDayPicker_DateSelected(object sender, DateChangedEventArgs e)
        {
            TaiwanCalendar calender = new TaiwanCalendar();
            var o = (DatePicker)sender;
            DateTime pickDate;
            if (o != null && app.time!=o.Date)
            {
                pickDate = o.Date;
                app.time = o.Date;
                app.SelectDate = string.Format("{0}{1}", calender.GetYear(pickDate), pickDate.ToString("MMdd"));
                Preferences.Set("RGDAY_DATETIME", RgDayPicker.Date);
                Preferences.Set("PICKDate_String", app.SelectDate);
                //ReadXML();
            }
        }

        public bool CheckIDLegal(string IDnum)
        {
            int[] priority = { 1, 8, 7, 6, 5, 4, 3, 2, 1, 1 };
            int count = IDCharSw(IDnum[0]);

            for (int i = 1; i < IDnum.Length; i++)
                count += priority[i] * (IDnum[i] - '0');
            Console.WriteLine("Count is::::" + count.ToString());
            if(count%10==0)
                return true;

            return false;
        }

        private int IDCharSw(char ch)
        {
            switch (ch)
            {
                case 'A': return 1; case 'P': return 29; 
                case 'B': return 10; case 'Q': return 38; 
                case 'C': return 19; case 'R': return 47; 
                case 'D': return 28; case 'S': return 56;
                case 'E': return 37; case 'T': return 65; 
                case 'F': return 46; case 'U': return 74; 
                case 'G': return 55; case 'V': return 83; 
                case 'H': return 64; case 'W': return 21;
                case 'I': return 39; case 'X': return 3; 
                case 'J': return 73; case 'Y': return 12; 
                case 'K': return 82; case 'Z': return 30; 
                case 'L': return 2; 
                case 'M': return 11; 
                case 'N': return 20; 
                case 'O': return 48;
                default: return 0;
            }
        }

        //private void ReadXML()
        //{
        //    if ((!app.checkRegister && app.isRegister) || (!app.isRegister && app.checkRegister) ) return;
        //    if (app._TmpRecords.Count != 0)
        //    {
        //        foreach (RgRecord tmprecord in app._TmpRecords)
        //        {
        //            //if (!app.records[app.records.IndexOf(tmprecord)].isAccept) return;
        //            if (app.records.Contains(tmprecord) && !app.records[app.records.IndexOf(tmprecord)].isAccept) app.records.Remove(tmprecord);
        //        }
        //    }

        //    string filename = "PatientData.xml";
        //    var assembly = typeof(RigisterList).GetTypeInfo().Assembly;
        //    bool isEmpty = (app.records.Count == 0);

        //    Stream stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.{filename}");
        //    using (var reader = new StreamReader(stream))
        //    {
        //        var xmlString = reader.ReadToEnd();
        //        Console.WriteLine(xmlString);


        //        XmlDocument doc = new XmlDocument();
        //        doc.LoadXml(xmlString);

        //        XmlNodeList records = doc.GetElementsByTagName("RgRecord");
        //        app._TmpRecords.Clear();
        //        for (int i = 0; i < records.Count; i++)
        //        {
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
        //            if (isEmpty)
        //                app.records.Add(record);
        //            else
        //                app.records.Insert(app.records.Count - 1, record);
        //        }
        //    }
        //}
    }
}