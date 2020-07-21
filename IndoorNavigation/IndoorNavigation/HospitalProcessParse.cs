using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

using IndoorNavigation.Models;
using IndoorNavigation.Utilities;
using static IndoorNavigation.Utilities.Storage;

namespace IndoorNavigation.Models
{
    public class HospitalProcessParse
    {
        public HospitalProcessParse()
        {

        }

        // when user enter the oppa page, this function will be called, and
        // return how many option can be selected by user.
        public List<string> GetProcessOption()
        {
            List<string> SelectList = new List<string>();

            XmlDocument doc = Storage.XmlReader("DefineStructureOfProcess.xml");

            XmlNodeList processNodeList = doc.GetElementsByTagName("process");

            int i = 0;
            foreach(XmlNode processNode in processNodeList)
            {
                i = int.Parse(processNode.Attributes["id"].Value);
                SelectList.Add(processNode.Attributes["name"].Value);
                Console.WriteLine("Process Name : {0}, id : {1}", 
                    processNode.Attributes["name"].Value, i);
            }
            return SelectList;
            //throw new NotImplementedException();
        }

        public ObservableCollection<ProcessRecord> ParseProcess(string selectedOptionName, string selectedid)
        {
            XmlDocument doc = Storage.XmlReader("DefineStructureOfProcess.xml");

            ObservableCollection<ProcessRecord> result = 
                new ObservableCollection<ProcessRecord>();

            XmlNodeList ProcessNodeList = doc.SelectNodes($"processes/process[@id={selectedid}]");

            foreach (XmlNode node in ProcessNodeList)
            {
                Console.WriteLine(node.Attributes["name"].Value+"aaaaaaaa");
            }
            #region  use for loop to find out result
            //XmlNodeList ProcessNodeList = doc.GetElementsByTagName("process");

            //foreach(XmlNode ProcessNode in ProcessNodeList)
            //{
            //    string processName = ProcessNode.Attributes["name"].Value;
            //    string processID = ProcessNode.Attributes["id"].Value;
            //    Console.WriteLine("ProcessNode name :" + processName);
            //    Console.WriteLine("ProcessNode id : " + processID);

            //    if(selectedid == processID && processName == selectedOptionName)
            //    {
            //        Console.WriteLine("There are one process meet the option.");

            //        XmlNodeList recordNodeList = ProcessNode.SelectNodes()
            //    }               
            //}
            #endregion
            return result;
        }
    }

    public struct ProcessRecord
    {
        public string TitleName { get; set; }
        public string AdditionalMsg { get; set; }
        public List<Tuple<DateTime, DateTime>> OpeningHours { get; set; }
        public string CareNoom { get; set; }
        public Guid _waypointID { get; set; }
        public Guid _regionID { get; set; }
        public string _waypointName{ get; set; }
        public RecordType _recordType { get; set; }
    }
}
