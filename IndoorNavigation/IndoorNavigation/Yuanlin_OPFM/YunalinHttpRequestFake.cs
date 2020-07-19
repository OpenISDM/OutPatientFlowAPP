using IndoorNavigation.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Xml;

using System.Net.Security;
using Newtonsoft.Json;
using Xamarin.Forms;
using IndoorNavigation.Models;
using System.Resources;
using IndoorNavigation.Resources.Helpers;
using System.Globalization;
using Plugin.Multilingual;
using System.Reflection;
using System.Collections.ObjectModel;

namespace IndoorNavigation.Yuanlin_OPFM
{
    public class YunalinHttpRequestFake
    {
        private const string requestWebUrl =
            "http://wpin.iis.sinica.edu.tw/oppa?building=CCH";
        private App app;

        private const string _resourceID =
            "IndoorNavigation.Resources.AppResources";
        private ResourceManager _resourceManager =
            new ResourceManager(_resourceID, typeof(TranslateExtension)
            .GetTypeInfo().Assembly);
        private CultureInfo _currentLanguage =
            CrossMultilingual.Current.CurrentCultureInfo;

        private const string _noPersonString = "No such person found...";

        public YunalinHttpRequestFake()
        {
            app = (App)Application.Current;
        }

        // for skip ssl check
        private bool CheckSSLValidation(object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors errors)
        {
            return true;
        }

        public string GetJsonBody(string PatientID, DateTime OpdDate)
        {
            Console.WriteLine(">>GexXmlBody");
            string JsonBodyString = 
                Storage.EmbeddedSourceReader("Yuanlin_OPFM.PostBody.json");

            PostObject postObject =
                JsonConvert.DeserializeObject<PostObject>(JsonBodyString);

            Console.WriteLine("Apipassword : " + postObject.ApiPassword);
            Console.WriteLine("ApiAccount : " + postObject.ApiAccount);
            Console.WriteLine("PatientID : " + postObject.PatientID);
            Console.WriteLine("OpdDate :" + postObject.OpdDate);

            postObject.PatientID = PatientID;
            postObject.OpdDate = OpdDate.ToString("yyyy/MM/dd");

            JsonBodyString = JsonConvert.SerializeObject(postObject);
            return JsonBodyString;
        }

        async public Task RequestFakeHIS()
        {
            Console.WriteLine(">>RequestFakeHIS");

            HttpWebRequest request =
                (HttpWebRequest)WebRequest.Create(requestWebUrl);

            if (requestWebUrl.StartsWith("https",
                StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback =
                    new RemoteCertificateValidationCallback(CheckSSLValidation);
            }

            request.ContentType = "Application/json";
            request.Timeout = 20000;
            request.Method = "POST";

            byte[] RequestBytes =
                Encoding.UTF8.GetBytes(GetJsonBody(app.IDnumber, new DateTime()));

            request.ContentLength = RequestBytes.Length;

            try
            {
                using (Stream postStream = await request.GetRequestStreamAsync())
                {
                    postStream.Write(RequestBytes, 0, RequestBytes.Length);
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("Network error - request error : "
                    + exc.StackTrace);
            }

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)
                    await request.GetResponseAsync())
                {
                    using (StreamReader streamReader =
                        new StreamReader(response.GetResponseStream()))
                    {
                        string content = streamReader.ReadToEnd();

                        if (!string.IsNullOrEmpty(content) ||
                            content != _noPersonString)
                        {
                            ContextParse(content);
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("Network error - no response " +
                    exc.StackTrace);

                // there need to implement a error handle to ask user whether he 
                // or she want to try again or not.
                Page page = Application.Current.MainPage;

                var WantRetry =
                    await page.DisplayAlert(GetResourceString("ERROR_STRING"),
                    GetResourceString("HAPPEND_ERROR_STRING"),
                    GetResourceString("RETRY_STRING"),
                    GetResourceString("NO_STRING"));
                if (WantRetry)
                {
                    await RequestFakeHIS();
                }
                else
                {
                    await page.Navigation.PopAsync();
                    ((App)(Application.Current)).isRigistered = false;
                    ((App)(Application.Current)).records = 
                        new ObservableCollection<RgRecord>();
                }
            }
            finally
            {
                request.Abort();
            }

        }

        public void ContextParse(string respondString)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(respondString);

            XmlNodeList XmlRecordList = doc.GetElementsByTagName("Record");
            ClinicPositionInfo infos = new ClinicPositionInfo();

            int index = 
                app.getRigistered ? app.records.Count - 1 : app.records.Count;

            foreach (XmlNode XmlRecordNode in XmlRecordList)
            {
                RgRecord record = new RgRecord();

                record.DptName =
                    XmlRecordNode.ChildNodes[0].Attributes["name"].Value;

                record.Shift =
                    XmlRecordNode.ChildNodes[1].Attributes["name"].Value;

                record.CareRoom =
                    XmlRecordNode.ChildNodes[2].Attributes["name"].Value;

                record._waypointName =
                    XmlRecordNode.ChildNodes[2].Attributes["name"].Value;

                record.DrName =
                    XmlRecordNode.ChildNodes[3].Attributes["name"].Value;

                record.SeeSeq =
                    XmlRecordNode.ChildNodes[4].Attributes["name"].Value;

                string type =
                    XmlRecordNode.ChildNodes[5].Attributes["name"].Value;

                record.type =
                    (RecordType)Enum.Parse(typeof(RecordType), type, true);

                record._regionID = infos.GetRegionID(record.CareRoom);
                record._waypointID = infos.GetDestinationID(record.CareRoom);

                if (record._regionID.Equals(Guid.Empty) &&
                    record._waypointID.Equals(Guid.Empty))
                {
                    record.isAccept = true;
                    record.isComplete = true;
                    record.DptName =
                        record.DptName
                        + GetResourceString("INVALID_WAYPOINT_STRING");
                    app.FinishCount++;
                    app.records.Insert(index++,record);
                    //app.records.Add(record);
                    continue;
                }
                //app.records.Add(record);
                app.records.Insert(index++, record);
                app._TmpRecords.Add(record);


                Console.WriteLine($"region id={record._regionID},"
                    + $" waypoint id={record._waypointID}");
            }
            if (!app.getRigistered)
                app.records.Add(new RgRecord { type = RecordType.NULL });
            Console.WriteLine("Current TmpRecord count : " +
                app._TmpRecords.Count);
        }
        private string GetResourceString(string key)
        {
            return _resourceManager.GetString(key, _currentLanguage);
            //throw new NotImplementedException();
        }

        class PostObject
        {
            public string ApiAccount { get; set; }
            public string ApiPassword { get; set; }
            public string PatientID { get; set; }
            public string OpdDate { get; set; }
        }
    }
}