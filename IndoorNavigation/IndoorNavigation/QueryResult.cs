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

namespace IndoorNavigation
{
    public class QueryResult
    {
        //public ObservableCollection<RgRecord> RgRecords=null;
    }

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
        public override string ToString() => _waypointName;
       
    }
    public class DestinationItem
    {
        public Guid _regionID { get; set; }
        public Guid _waypointID { get; set; }
        public string _waypointName { get; set; }
        public string _floor { get; set; }
        public string Key { get; set; }
        public override string ToString() => _waypointName;
        
    }

}
