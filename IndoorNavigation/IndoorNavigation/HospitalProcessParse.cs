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

        public ObservableCollection<RgRecord> ParseProcess()
        {
            XmlDocument doc = Storage.XmlReader("DefineStructureOfProcess.xml");

            ObservableCollection<RgRecord> result = 
                new ObservableCollection<RgRecord>();

            XmlNodeList recordsNodeList = doc.GetElementsByTagName("record");


            foreach(XmlNode recordNode in recordsNodeList)
            {
                Console.WriteLine("Recode Node Name : " + recordNode.Attributes["name"].Value);
               
                if(recordNode.Attributes["note"] != null)
                    Console.WriteLine("Record Node note : " + recordNode.Attributes["note"].Value);
            }
            return result;
        }
    }
}
