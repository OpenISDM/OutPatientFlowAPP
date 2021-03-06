﻿using System;
using System.Collections.Generic;
using System.Xml;
using System.Collections.ObjectModel;
using IndoorNavigation.Utilities;
using Xamarin.Forms;

namespace IndoorNavigation.Models
{
    public class HospitalProcessParse
    {
        private ClinicPositionInfo _infos = new ClinicPositionInfo();
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
    
            foreach(XmlNode processNode in processNodeList)
            {
                ProcessOption option = new ProcessOption
                {
                    processID = processNode.Attributes["id"].Value,
                    processName = processNode.Attributes["name"].Value
                };                
                Console.WriteLine("Process Name : {0}, id : {1}", 
                    processNode.Attributes["name"].Value, option.processID);

                SelectList.Add(option);
            }
            return SelectList;
            //throw new NotImplementedException();
        }
        public ObservableCollection<RgRecord> ParseProcess
           (ProcessOption selectedOption)
        {
            XmlDocument doc = Storage.XmlReader("DefineStructureOfProcess.xml");

            ObservableCollection<RgRecord> result =
                new ObservableCollection<RgRecord>();
            string QueryxmlPath =
                string.Format("processes/process[@id='{0}' and @name='{1}']",
                selectedOption.processID, selectedOption.processName);            
            XmlNodeList ProcessNodeList = doc.SelectNodes(QueryxmlPath);

            // This Loop must only run one time theortically. 
            foreach (XmlNode node in ProcessNodeList)
            {
                XmlNodeList RecordNodeList =
                    node.SelectNodes("records/record");

                #region Temperoray code region
                List<OpeningTime> openingTimes = new List<OpeningTime>();
                string AdditionalRequire = "";
                string CareRoom;
                string RecordName;
                string WaypointName;
                int order = 0;
                RecordType type;
                #endregion

                foreach (XmlNode recordNode in RecordNodeList)
                {
                    openingTimes = new List<OpeningTime>();
                    RecordName = recordNode.Attributes["name"].Value;

                    XmlNode CareRoomXmlNode = recordNode.ChildNodes[0];
                    CareRoom = 
                        CareRoomXmlNode.Attributes["name"].Value;
                    WaypointName =
                        CareRoomXmlNode.Attributes["waypointName"].Value;
                    type = 
                        (RecordType)Enum.Parse(typeof(RecordType), 
                        CareRoomXmlNode.Attributes["type"].Value, 
                        false);

                    order = 
                        int.Parse(CareRoomXmlNode.Attributes["order"].Value);

                    #region If the department need additionally require.
                    if (recordNode.ChildNodes.Count >= 2)
                    {
                        XmlNode AdditionXmlNode = recordNode.ChildNodes[1];

                        XmlNode noteXmlNode = AdditionXmlNode.ChildNodes[0];
                        AdditionalRequire =
                            noteXmlNode.Attributes["text"].Value;                        

                        #region If this department has opening time. 
                        if (AdditionXmlNode.ChildNodes.Count >= 2)
                        {
                            Console.WriteLine("Enter OpenTime statement");
                            XmlNodeList openTimeXmlNodeList =
                                AdditionXmlNode.ChildNodes[1]
                                .SelectNodes("dayoftheweek");
                            foreach (XmlNode dayOfTheWeek in openTimeXmlNodeList)
                            {
                                Console.WriteLine("dayOftheWeek hour :"
                                    + dayOfTheWeek.Attributes["day"].Value);

                                openingTimes.Add(ParsingOpenTime(dayOfTheWeek));
                            }
                        }
                        #endregion
                    }
                    #endregion

                    RegionWaypointPoint point = _infos.GetWaypoint(CareRoom);
                    bool isComplete = false;
                    bool isAccept = false; 
                    if( point._waypointID == Guid.Empty ||
                        point._regionID == Guid.Empty)
                    {
                        RecordName = RecordName + "(無效的航點)";
                        isComplete = true;
                        isAccept = true;
                        ((App)Application.Current).FinishCount++;
                    }
                    RgRecord processRecord = new RgRecord
                    {
                        OpeningHours = openingTimes,
                        CareRoom = CareRoom,
                        AdditionalMsg = AdditionalRequire,
                        _subtitleName = WaypointName,
                        type = type,
                        DptName = RecordName,
                        _waypointName = WaypointName,
                        _regionID = point._regionID,
                        _waypointID = point._waypointID,
                        isComplete = isComplete,
                        order = order,
                        isAccept = isAccept,
                        _groupID = int.Parse(selectedOption.processID)
                    };
                    result.Add(processRecord);
                }
            }            
            return result;
        }
        
        private OpeningTime ParsingOpenTime(XmlNode OpenTimeNode)
        {
            TimeSpan startTime =
                TimeSpan.Parse(OpenTimeNode.Attributes["startTime"].Value);

            TimeSpan endTime =
                TimeSpan.Parse(OpenTimeNode.Attributes["endTime"].Value);
            
            Week weekday = 
                (Week)Enum.Parse(typeof(Week), 
                OpenTimeNode.Attributes["day"].Value, 
                false);          

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
        public override string ToString() => processName;
    }    
}
