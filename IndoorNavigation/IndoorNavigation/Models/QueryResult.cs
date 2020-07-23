using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using Plugin.Multilingual;
using System.Resources;
using IndoorNavigation.Resources.Helpers;
using System.Reflection;
using IndoorNavigation.Views.Navigation;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using IndoorNavigation.Models;

namespace IndoorNavigation
{
    public class RgRecord : DestinationItem
    {
        public string OpdDate { get; set; }
        public string DptName { get; set; }
        public string Shift { get; set; }
        public string CareRoom { get; set; }
        public string DrName { get; set; }
        public string SeeSeq { get; set; }
        //date name shift careroom drname seq
        public bool isComplete { get; set; }
        public bool isAccept { get; set; }
        #region For Process Suit
        public int order { get; set; }
        public string TitleName { get; set; }
        public string AdditionalMsg { get; set; }
        public List<OpeningTime> OpeningHours { get; set; }
        #endregion

        public override string ToString() => _waypointName;
       
    }
    public class DestinationItem
    {
        public Guid _regionID { get; set; }
        public Guid _waypointID { get; set; }
        public string _waypointName { get; set; }
        public string _floor { get; set; }
        public RecordType type { get; set; }
        public override string ToString() => _waypointName;
    }

    public enum RecordType
    {
        Register = 0,
        Queryresult,
        Examination,
        AddItem,
        Pharmacy,
        Cashier,
        Exit,
        NULL,
        Invalid
    }
}
