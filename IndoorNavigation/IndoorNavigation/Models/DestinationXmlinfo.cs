using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using Xamarin.Forms;
using IndoorNavigation.Modules.Utilities;
namespace IndoorNavigation
{
    class ClinicPositionInfo
    {
        private Dictionary<String, RoomInfo> RoomInfos;

        public ClinicPositionInfo()
        {
            
            Guid Dguid,Rguid;
            RoomInfos = new Dictionary<string, RoomInfo>();

            XmlDocument doc = NavigraphStorage.XmlReader("Yuanlin_OPFM.CareRoomMapp.xml");           
            //doc.LoadXml(context);
            XmlNodeList destinationList = doc.SelectNodes("navigation_graph/regions/region/Destination");

            foreach (XmlNode destinationNode in destinationList)
            {
                Dguid = new Guid(destinationNode.Attributes["id"].Value);
                Rguid = new Guid(destinationNode.ParentNode.Attributes["id"].Value);

                RoomInfos.Add(destinationNode.Attributes["name"].Value,new RoomInfo(Rguid,Dguid));

                Console.WriteLine($"Dict add a new Destinaiton, ID={destinationNode.Attributes["id"].Value}, Name={destinationNode.Attributes["name"].Value}, Region id={Rguid.ToString()}");               
            }
        }
        public Guid GetRegionID(string key)
        {
            if (!RoomInfos.ContainsKey(key))
                return new Guid();
            return RoomInfos[key]._region;
            //return RegionGuidDict[key];
        }
        public Guid GetDestinationID(string key)
        {
            if (!RoomInfos.ContainsKey(key))
                return new Guid();
            return RoomInfos[key]._clinic;
        }
    }
    class RoomInfo
    {
        public Guid _region;
        public Guid _clinic;

        public RoomInfo(Guid region,Guid clinic)
        {
            _region = region;
            _clinic = clinic;
        }
    }
}
