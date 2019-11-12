using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using System.Xml;
using Xamarin.Forms;
using Xamarin.Essentials;
namespace IndoorNavigation
{
    class HttpRequest
    {
        private string bodyString = "";
        private string responseString = "";
        private ObservableCollection<RgRecord> nowadayRecords;
        private App app;

        public HttpRequest()
        {
            nowadayRecords = new ObservableCollection<RgRecord>();
            app = (App)Application.Current;     
        }

        public void GetXMLBody()
        {
            Console.WriteLine("Now Excution is::: GetXMLBody");
            bodyString = "";
            
            string filename = "RequestBody.xml";
            var assembly = typeof(HttpRequest).GetTypeInfo().Assembly;

            Stream stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.{filename}");
            
            using (var reader = new StreamReader(stream))
            {
                //to put into xml need taiwan year format
                TaiwanCalendar calendar = new TaiwanCalendar();
                DateTime date = DateTime.Now;

                string nowaday = string.Format("{0}{1}", calendar.GetYear(date), date.ToString("MMdd"));


                bodyString = reader.ReadToEnd();

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(bodyString);
                XmlDocument doc2 = doc;
                XmlNodeList xmlNodeList = doc.GetElementsByTagName("hs:Document");

                //get tht node that we have to edit
                XmlNode node_patient = xmlNodeList[0].ChildNodes[0];
                XmlNode node_sdate = xmlNodeList[0].ChildNodes[4];
                XmlNode node_edate = xmlNodeList[0].ChildNodes[5];
                //node_patient.InnerText = "aaaaaa";
                node_edate.InnerText = "1081015";
                node_sdate.InnerText = "1081015";

                //parse xml to string
                StringWriter stringWriter = new StringWriter();
                XmlWriter writer = XmlWriter.Create(stringWriter);

                doc.WriteContentTo(writer);
                writer.Flush();

                bodyString = stringWriter.ToString();
                
            }
            stream.Close();
           
        }

        public void RequestData()
        {
            Console.WriteLine("Now Excution is::: RequstData");
            string contentString;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://bc.cch.org.tw:8080/WSRgSRV/Service.asmx");

            //set headers
            //request.Headers.Set(HttpRequestHeader.ContentType, "text/xml");
            request.ContentType = "text/xml";
            request.Headers.Set("SOAPAction", "http://tempuri.org/GetRGdata2");

            //set POST action
            request.Method = "POST";

            //set POST Body
            byte[] bytes = Encoding.UTF8.GetBytes(bodyString);
            request.ContentLength = bytes.Length;

            //do post
            using (Stream postStream = request.GetRequestStream())
            {
                postStream.Write(bytes, 0, bytes.Length);
            }
            //get response 
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                string content = reader.ReadToEnd();
              
                contentString = content;
                responseString = content;
               
            }   
        }
        public void ResponseXmlParse()
        {
            Console.WriteLine("Now Excution is::: ResponseXmlParse");
            string modifyString = "";
            modifyString = responseString.Remove(0, responseString.IndexOf("&lt;?xml"));
            modifyString = modifyString.Remove(modifyString.IndexOf("</GetRGdata2Result>"), modifyString.Length - modifyString.IndexOf("</GetRGdata2Result>"));
            
            StringWriter writer = new StringWriter();
            HttpUtility.HtmlDecode(modifyString, writer);
            responseString = writer.ToString();

            XmlDocument doc = new XmlDocument();
           
            doc.LoadXml(responseString);
          
            XmlNodeList records = doc.GetElementsByTagName("RgRecord");
            Console.WriteLine(responseString);
            for (int i = 0; i < records.Count; i++)
            {
                RgRecord record = new RgRecord();

                record.OpdDate = records[i].ChildNodes[0].InnerText;
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
            }

            Console.WriteLine(app._TmpRecords.Count);
        }
    }
}
