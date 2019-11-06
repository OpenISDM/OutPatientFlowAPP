using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Xamarin.Forms;
using System.Collections.ObjectModel;
using System.Reflection;

namespace IndoorNavigation.Models
{
    public class PatientXMLParse
    {
        private ObservableCollection<RgRecord> _records;
        private String _PatientID;
        public PatientXMLParse(XmlDocument fileName)
        {
            //var assembly = IntrospectionExtensions.GetTypeInfo(typeof(LoadResourceText)).Assembly;
            //XmlDocument doc = new XmlDocument();
            // doc.Load(fileName);
            XmlNodeList xmlRecords = fileName.SelectNodes("QueryResult/RgRecords/RgRecord");
            XmlNodeList patientID = fileName.SelectNodes("QueryResult/PatientID");
            _records = new ObservableCollection<RgRecord>();
           
            foreach(XmlNode node in patientID)
            {
                XmlElement patient = (XmlElement)node;
                _PatientID = patient.GetAttribute("PatientID").ToString();
            }

            foreach (XmlNode node in xmlRecords)
            {
                XmlElement element = (XmlElement)node;
                RgRecord record = new RgRecord();

                record.OpdDate = element.GetAttribute("OpdDate").ToString();
                record.DptName = element.GetAttribute("DptName").ToString();
                record.Shift = element.GetAttribute("Shift").ToString();
                record.CareRoom = element.GetAttribute("CareRoom").ToString();
                record.DrName = element.GetAttribute("DrName").ToString();
                record.SeeSeq = element.GetAttribute("SeeSeq").ToString();

                _records.Add(record);
            }
            Console.WriteLine("records count is :" + _records.Count+"11111111111111111111111");
        }
        public ObservableCollection<RgRecord> GetRecords()
        {
            return _records;
        }
        public String GetID()
        {
            return _PatientID;
        }



    }


}
