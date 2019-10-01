using System;
using System.Collections.Generic;
using System.Text;

namespace IndoorNavigation
{
    public class PatientData
    {
        public PatientData() { }
        public PatientData(string id,string patient,string birthday,DateTime date)
        {
            IDNumber = id;
            PatientID = patient;
            BirthDay = birthday;
            Date = date;
        }
        public string IDNumber { get; set; }
        public string PatientID { get; set; }
        public string BirthDay { get; set; }
        public DateTime Date { get; set; }

    }
}
