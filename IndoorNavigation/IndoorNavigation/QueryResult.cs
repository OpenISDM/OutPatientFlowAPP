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
        const string _resourceId = "IndoorNavigation.Resources.AppResources";
        ResourceManager _resourceManager =
            new ResourceManager(_resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);

        public string SeqAndRoom
        {
            get
            {
                var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
                return string.Format("{0}:{1}, {2}{3}", _resourceManager.GetString("SEQUENCE_STRING", currentLanguage),
                    SeeSeq, CareRoom, _resourceManager.GetString("CAREROOM_STRING", currentLanguage));
            }
        }
        public string Date { get; set; }
        public string DptName { get; set; }
        public string DptTime { get; set; }
        public string Shift { get; set; }
        public string CareRoom { get; set; }
        public string DrName { get; set; }
        public string SeeSeq { get; set; }
        //date name shift careroom drname seq
        public bool isComplete { get; set; }
        public bool isAccept { get; set; }

    }
    public class DestinationItem
    {
        public Guid _regionID { get; set; }
        public Guid _waypointID { get; set; }
        public string _waypointName { get; set; }
        public string _floor { get; set; }
        public string Key { get; set; }
    }

}
