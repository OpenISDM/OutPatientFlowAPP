using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

using IndoorNavigation.Models;
using IndoorNavigation.Utilities;
using static IndoorNavigation.Utilities.Storage;
using System.Linq;
using Newtonsoft.Json.Converters;
using System.Reflection;
using System.Diagnostics;

namespace IndoorNavigation.Models
{
    public class HospitalProcessParse
    {
        public HospitalProcessParse()
        {

        }

        // when user enter the oppa page, this function will be called, and
        // return how many option can be selected by user.
        public List<ProcessOption> GetProcessOption()
        {
            List<ProcessOption> SelectList = new List<ProcessOption>();

            XmlDocument doc = Storage.XmlReader("DefineStructureOfProcess.xml");

            XmlNodeList processNodeList = doc.GetElementsByTagName("process");

            int i = 0;
            foreach(XmlNode processNode in processNodeList)
            {
                ProcessOption option = new ProcessOption
                {
                    processID = processNode.Attributes["id"].Value,
                    processName = processNode.Attributes["name"].Value
                };
                //i = int.Parse(processNode.Attributes["id"].Value);
                //SelectList.Add(processNode.Attributes["name"].Value);
                Console.WriteLine("Process Name : {0}, id : {1}", 
                    processNode.Attributes["name"].Value, i++);
            }
            return SelectList;
            //throw new NotImplementedException();
        }

        public ObservableCollection<ProcessRecord> ParseProcess
            (ProcessOption selectedOption)
        {           
            XmlDocument doc = Storage.XmlReader("DefineStructureOfProcess.xml");

            ObservableCollection<ProcessRecord> result = 
                new ObservableCollection<ProcessRecord>();
            string xmlPath =
                string.Format("processes/process[@id='{0}' and @name='{1}']",
                selectedOption.processID, selectedOption.processName);
            XmlNodeList ProcessNodeList = 
                doc.SelectNodes(xmlPath);

            // This Loop must only run one time theortically. 
            foreach (XmlNode node in ProcessNodeList)
            {              
                XmlNodeList RecordNodeList = 
                    node.SelectNodes("records/record");

                #region Temperoray code region
                List<OpeningTime> openingTimes = new List<OpeningTime>();
                string AdditionalRequire="";
                string CareRoom;
                string RecordName;
                #endregion

                foreach (XmlNode recordNode in RecordNodeList)
                {
                    RecordName = recordNode.Attributes["name"].Value;                   
                    XmlNode CareRoomXmlNode = recordNode.ChildNodes[0];
                    CareRoom = CareRoomXmlNode.Attributes["name"].Value;
                    
                    #region If the department need additionally require.
                    if (recordNode.ChildNodes.Count >= 2)
                    {
                        XmlNode AdditionXmlNode = recordNode.ChildNodes[1];

                        XmlNode noteXmlNode = AdditionXmlNode.ChildNodes[0];
                        AdditionalRequire = 
                            noteXmlNode.Attributes["text"].Value;
                        Console.WriteLine("noteXml note text : " +
                            AdditionalRequire);

                        #region If this department has opening time. 
                        if (AdditionXmlNode.ChildNodes.Count>=2)
                        {
                            Console.WriteLine("Enter OpenTime statement");
                            XmlNodeList openTimeXmlNodeList =
                                AdditionXmlNode.ChildNodes[1]
                                .SelectNodes("dayoftheweek");                          
                            foreach(XmlNode dayOfTheWeek in openTimeXmlNodeList)
                            {
                                Console.WriteLine("dayOftheWeek hour :"
                                    + dayOfTheWeek.Attributes["day"].Value);

                                openingTimes.Add(ParsingOpenTime(dayOfTheWeek));
                            }
                        }
                        #endregion
                    }
                    #endregion

                    ProcessRecord processRecord = new ProcessRecord
                    {
                        OpeningHours = openingTimes,
                        TitleName = RecordName,
                        CareNoom = CareRoom,
                        AdditionalMsg = AdditionalRequire,
                    };
                    result.Add(processRecord);
                }                
            }
            Console.WriteLine("Result count : " + result.Count);
            return result;
        }
        private OpeningTime ParsingOpenTime(XmlNode OpenTimeNode)
        {
            
            TimeSpan startTime =
                TimeSpan.Parse(OpenTimeNode.Attributes["startTime"].Value);
            //DateTime startTime = DateTime.Today.Add(start);
            TimeSpan endTime =
                TimeSpan.Parse(OpenTimeNode.Attributes["endTime"].Value);
            //DateTime endTime=
                //DateTime.Parse(OpenTimeNode.Attributes["endTime"].Value);
                
            Week weekday = 
                (Week)Enum.Parse(typeof(Week), 
                OpenTimeNode.Attributes["day"].Value, 
                false);
            Console.WriteLine("start Time value : " + startTime);
            Console.WriteLine("end Time value : " + endTime);
            Console.WriteLine("weekday value : " + weekday);

            return new OpeningTime(startTime, endTime, weekday);
        }
    }

    public enum Week 
    {
        Sunday = 0,
        Saturday,
        Workingday,
        Friday,
        Thursday,
        Wednesday,
        Tuesday,
        Monday
    }
    public struct OpeningTime
    {
        public OpeningTime(TimeSpan start,TimeSpan end, Week week)
        {
            startTime = start;
            endTime = end;
            dayOfWeek = week;
        }
        public TimeSpan startTime { get; set; }
        public TimeSpan endTime { get; set; }
        public Week dayOfWeek { get; set; }
    }
    public struct ProcessOption
    {
        public string processID;
        public string processName;
    }
    public struct ProcessRecord 
    {
        public string TitleName { get; set; }
        public string AdditionalMsg { get; set; }
        public List<OpeningTime> OpeningHours { get; set; }
        public string CareNoom { get; set; }
        public Guid _waypointID { get; set; }
        public Guid _regionID { get; set; }
        public string _waypointName{ get; set; }
        public RecordType _recordType { get; set; }
    }
}
