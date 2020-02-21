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
using System.Resources;
using Xamarin.Essentials;
using Rg.Plugins.Popup.Services;
using System.Threading.Tasks;
using Plugin.Multilingual;

using IndoorNavigation.Modules.Utilities;
using IndoorNavigation.Resources.Helpers;

namespace IndoorNavigation
{
    class HttpRequest
    {
        private string bodyString = "";
        private string responseString = "";
        private App app;

        private const string _resourceID= "IndoorNavigation.Resources.AppResources";
        private ResourceManager _resourceManager= new ResourceManager(_resourceID, typeof(TranslateExtension).GetTypeInfo().Assembly);
        private CultureInfo _currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;

        public HttpRequest()
        {
            app = (App)Application.Current;     
        }

        public void GetXMLBody()
        {
            Console.WriteLine("Now Excution is::: GetXMLBody");
           
            //to put into xml need taiwan year format
            TaiwanCalendar calendar = new TaiwanCalendar();
            string selectedDay = string.Format("{0}{1}", calendar.GetYear(app.RgDate), app.RgDate.ToString("MMdd"));

            XmlDocument doc = NavigraphStorage.XmlReader("Yuanlin_OPFM.RequestBody.xml");

            XmlNodeList xmlNodeList = doc.GetElementsByTagName("hs:Document");

            XmlNode node_patient = xmlNodeList[0].ChildNodes[0];

            XmlNode node_sdate = xmlNodeList[0].ChildNodes[4];
            XmlNode node_edate = xmlNodeList[0].ChildNodes[5];
                
            node_patient.InnerText = app.IDnumber;
            node_edate.InnerText = selectedDay;
            node_sdate.InnerText = selectedDay;

                //parse xml to string
            StringWriter stringWriter = new StringWriter();
            XmlWriter writer = XmlWriter.Create(stringWriter);
            
            doc.WriteContentTo(writer);
            
            writer.Flush();

            bodyString = stringWriter.ToString();
            
        }

        async public Task RequestData()
        {
            Console.WriteLine("Now Excution is::: RequstData");
            string contentString;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://bc.cch.org.tw:8080/WSRgSRV/Service.asmx");

            //set headers
            //request.Headers.Set(HttpRequestHeader.ContentType, "text/xml");
            request.ContentType = "text/xml";
            request.Headers.Set("SOAPAction", "http://tempuri.org/GetRGdata2");
            request.Timeout = 20000;
            //set POST action
            request.Method = "POST";

            //set POST Body
            byte[] bytes = Encoding.UTF8.GetBytes(bodyString);
            request.ContentLength = bytes.Length;
            try
            {
                //do post
                using (Stream postStream = await request.GetRequestStreamAsync())
                {
                    postStream.Write(bytes, 0, bytes.Length);
                }
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            try
            {
                //get response 
                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string content = reader.ReadToEnd();

                    contentString = content;
                    responseString = content;

                }
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                request.Abort();
            }

            ResponseXmlParse();

            
        }
        public void ResponseXmlParse()
        {
            Console.WriteLine("Now Excution is::: ResponseXmlParse");
            XmlDocument XmlfromRespone = new XmlDocument();
            XmlfromRespone.LoadXml(responseString);
            XmlNodeList ResponeList = XmlfromRespone.GetElementsByTagName("GetRGdata2Response");

            string modifyString = ResponeList[0].InnerText;
          
            StringWriter writer = new StringWriter();
            HttpUtility.HtmlDecode(modifyString, writer);
            responseString = writer.ToString();

            XmlDocument doc = new XmlDocument();
           
            doc.LoadXml(responseString);
          
            XmlNodeList records = doc.GetElementsByTagName("RgRecord");
            Console.WriteLine(responseString);
            DestinationXmlinfo infos = new DestinationXmlinfo();

            int index = (app.getRigistered) ? app.records.Count - 1 : app.records.Count;

            for (int i = 0; i < records.Count; i++)
            {
                RgRecord record = new RgRecord();

                record.OpdDate = records[i].ChildNodes[0].InnerText;
                record.DptName = records[i].ChildNodes[1].InnerText;
                record.Shift = records[i].ChildNodes[2].InnerText;
                record.CareRoom = records[i].ChildNodes[3].InnerText;
                record.DrName = records[i].ChildNodes[4].InnerText;
                record.SeeSeq = records[i].ChildNodes[5].InnerText;
                record.type = RecordType.Queryresult;
                record._waypointName = record.CareRoom;
                record._regionID = infos.GetRegionID(record.CareRoom); 
                record._waypointID = infos.GetDestinationID(record.CareRoom);
                                 
                if (record._regionID.Equals(Guid.Empty) && record._waypointID.Equals(Guid.Empty))
                {
                    record.isAccept = true;
                    record.isComplete = true;
                    record.DptName = record.DptName + getResourceString("INVALID_WAYPOINT_STRING");
                    app.FinishCount++;
                    app.records.Insert(index++,record);
                    continue;
                }
                app.records.Insert(index++,record);
                app._TmpRecords.Add(record);

               
                Console.WriteLine($"HttpRequest region id={record._regionID}, waypoint id={record._waypointID}");
            }
            if (!app.getRigistered)
                app.records.Add(new RgRecord { type=RecordType.NULL });
            Console.WriteLine(app._TmpRecords.Count);
        }

        private string getResourceString(string key)
        {
            return _resourceManager.GetString(key, _currentLanguage);
        }
    }
}
