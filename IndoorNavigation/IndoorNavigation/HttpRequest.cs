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

namespace IndoorNavigation
{
    class HttpRequest
    {
        private string bodyString = "";
        private string responseString = "";
        private ObservableCollection<RgRecord> nowadayRecords;

        public HttpRequest()
        {
            nowadayRecords = new ObservableCollection<RgRecord>();
            GetXMLBody();
            //responseString = RequestData();
            //nowadayRecords = GetListofToday();
        }

        private void GetXMLBody()
        {
            //bodyString = "";

            string filename = "RequestBody.xml";
            var assembly = typeof(HttpRequest).GetTypeInfo().Assembly;

            Stream stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.{filename}");

            using (var reader = new StreamReader(stream))
            {
                bodyString = reader.ReadToEnd();
                //Console.WriteLine(bodyString);

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(bodyString);

                XmlNodeList xmlNodeList = doc.GetElementsByTagName("hs:Document");

                XmlNode node_patient = xmlNodeList[0].ChildNodes[0];

                node_patient.InnerText = "aaaaaa";

                //node.InnerText = "aaaa";
                doc.Save(stream);
            }
        }

        public string RequestData()
        {
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
                Console.WriteLine("Response from Redmine Issue Tracker: " + content);
                StringWriter writer = new StringWriter();

                HttpUtility.HtmlDecode(content, writer);

                Console.WriteLine("this is after decode:  " + writer.ToString());
                contentString = writer.ToString();
                responseString = writer.ToString();
                Console.WriteLine(contentString + "9999999999999999");
            }
            return contentString;
        }

        public ObservableCollection<RgRecord> GetListofToday()
        {
            TaiwanCalendar calendar = new TaiwanCalendar();
            DateTime date = DateTime.Now;

            string nowaday = string.Format("{0}{1}", calendar.GetYear(date), date.ToString("MMdd"));

            XmlDocument doc = new XmlDocument();
            //doc.LoadXml(responseString);

            XmlNodeList queryResult = doc.GetElementsByTagName("RgRecord");

            foreach (XmlNode node in queryResult)
            {
                Console.WriteLine("First Inner Text is:" + node.ChildNodes[0].InnerText + "66666666");
            }

            return nowadayRecords;
        }
    }
}
