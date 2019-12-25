using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using Xamarin.Forms;

namespace IndoorNavigation
{
    class DestinationXmlinfo
    {
        private Dictionary<String, Guid> RegionGuidDict;
        private Dictionary<String, Guid> DestinationGuidDict;
        private Dictionary<String, RoomInfo> RoomInfos;

        public DestinationXmlinfo()
        {
            var assembly = typeof(DestinationXmlinfo).GetTypeInfo().Assembly;
            var fileName = "Yuanlin_OPFM.CareRoomMapp.xml";
            var DocumentPath = $"{assembly.GetName().Name}.{fileName}";
            string context = "";
            Stream stream = assembly.GetManifestResourceStream(DocumentPath);

            using (StreamReader reader=new StreamReader(stream)){
                context = reader.ReadToEnd();
            }

            Guid Dguid,Rguid;
            RegionGuidDict = new Dictionary<string, Guid>();
            DestinationGuidDict = new Dictionary<string, Guid>();
            RoomInfos = new Dictionary<string, RoomInfo>();
            XmlDocument doc = new XmlDocument();
           
            doc.LoadXml(context);

            XmlNodeList destinationList = doc.SelectNodes("navigation_graph/regions/region/Destination");

            foreach (XmlNode destinationNode in destinationList)
            {
                Dguid = new Guid(destinationNode.Attributes["id"].Value);
                Rguid = new Guid(destinationNode.ParentNode.Attributes["id"].Value);

                RoomInfos.Add(destinationNode.Attributes["name"].Value,new RoomInfo(Rguid,Dguid));

                Console.WriteLine($"Dict add a new Destinaiton, ID={destinationNode.Attributes["id"].Value}, Name={destinationNode.Attributes["name"].Value}, Region id={Rguid.ToString()}");
                //Console.WriteLine(destinationNode.ParentNode.Attributes["id"].Value+"   AAAAAAAAAAAAAAAAAA");
                //DestinationGuidDict.Add(destinationNode.Attributes["name"].Value, Dguid);
                //Console.WriteLine($"Dict add a new Destinaiton, ID={destinationNode.Attributes["id"].Value}, Name={destinationNode.Attributes["name"].Value}");
            }
        }

        public Guid GetRegionID(string key)
        {
            if (!RoomInfos.ContainsKey(key))
                return new Guid("22222222-2222-2222-2222-222222222222");
            return RoomInfos[key]._region;
            //return RegionGuidDict[key];
        }
        public Guid GetDestinationID(String key)
        {
            if (!RoomInfos.ContainsKey(key))
                return new Guid("00000000-0000-0000-0000-000000000002");
            return RoomInfos[key]._clinic;
        }

        
    }
    class RoomInfo
    {
        public Guid _region;
        public Guid _clinic;


        public RoomInfo()
        {
            _region = new Guid();
            _clinic = new Guid();
        }
        public RoomInfo(Guid region,Guid clinic)
        {
            _region = region;
            _clinic = clinic;
        }
    }
}
