/*
 * Copyright (c) 2019 Academia Sinica, Institude of Information Science
 *
 * License:
 *      GPL 3.0 : The content of this file is subject to the terms and
 *      conditions defined in file 'COPYING.txt', which is part of this source
 *      code package.
 *
 * Project Name:
 *
 *      IndoorNavigation
 *
 * 
 *     
 *      
 * Version:
 *
 *      1.0.0, 20200221
 * 
 * File Name:
 *
 *      Yaunlin_HttpRequest.cs
 *
 * Abstract:
 *      
 *
 *      
 * Authors:
 *
 *      Jason Chang, jasonchang@iis.sinica.edu.tw
 *     
 */

using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using System.Xml;
using Xamarin.Forms;
using System.Resources;
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
        private ResourceManager _resourceManager= 
			new ResourceManager(_resourceID, typeof(TranslateExtension)
			.GetTypeInfo().Assembly);
        private CultureInfo _currentLanguage = 
			CrossMultilingual.Current.CurrentCultureInfo;

        public HttpRequest()
        {
            app = (App)Application.Current;     
        }

        public void GetXMLBody()
        {
            Console.WriteLine("Now Excution is::: GetXMLBody");
           
            
            TaiwanCalendar taiwanCalendar = new TaiwanCalendar();
            //in request body, it require to use Taiwan calender to check date
            string selectedDay = 
				taiwanCalendar.GetYear(app.RgDate) 
				+ app.RgDate.ToString("MMdd");

            XmlDocument doc = 
				NavigraphStorage.XmlReader("Yuanlin_OPFM.RequestBody.xml");

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
            HttpWebRequest request = 
				(HttpWebRequest)WebRequest
				.Create("http://bc.cch.org.tw:8080/WSRgSRV/Service.asmx");

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
                using (Stream postStream 
					= await request.GetRequestStreamAsync())
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
                using (HttpWebResponse response = 
					(HttpWebResponse)await request.GetResponseAsync())
                using (StreamReader reader = 
					new StreamReader(response.GetResponseStream()))
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
            XmlNodeList ResponeList = 
				XmlfromRespone.GetElementsByTagName("GetRGdata2Response");

            string modifyString = ResponeList[0].InnerText;
          
            StringWriter writer = new StringWriter();
            HttpUtility.HtmlDecode(modifyString, writer);
            responseString = writer.ToString();

            XmlDocument doc = new XmlDocument();
           
            doc.LoadXml(responseString);
          
            XmlNodeList records = doc.GetElementsByTagName("RgRecord");
            Console.WriteLine(responseString);
            ClinicPositionInfo infos = new ClinicPositionInfo();

    //        int index = 
				//(app.getRigistered) ? app.records.Count - 1 : app.records.Count;

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
                record._waypointID = infos.GetWaypointID(record.CareRoom);
                                 
                //it may appear the reiong or floor that we doesn't support,
                //so I ban it and show it's invalid.
                if (record._regionID.Equals(Guid.Empty) && 
					record._waypointID.Equals(Guid.Empty))
                {
                    record.isAccept = true;
                    record.isComplete = true;
                    record.DptName = 
						record.DptName 
						+ getResourceString("INVALID_WAYPOINT_STRING");
                    app.FinishCount++;
                    //app.records.Insert(index++,record);
                    app.records.Add(record);
                    continue;
                }
                //app.records.Insert(index++,record);
                app.records.Add(record);
                app._TmpRecords.Add(record);

               
                Console.WriteLine($"region id={record._regionID},"
					+$" waypoint id={record._waypointID}");
            }
            //if (!app.getRigistered)
            //    app.records.Add(new RgRecord { type=RecordType.NULL });
            Console.WriteLine(app._TmpRecords.Count);
        }

        //private void AddRecord(RgRecord record)
        //{
        //    if (app.getRigistered)
        //        app.records.Add(record);
        //    else
        //        app.records.in
        //}

        private string getResourceString(string key)
        {
            return _resourceManager.GetString(key, _currentLanguage);
        }
    }
}
