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

        public DestinationXmlinfo()
        {
            var assembly = typeof(DestinationXmlinfo).GetTypeInfo().Assembly;
            var fileName = "CareRoomMapp.xml";
            var DocumentPath = $"{assembly.GetName().Name}.{fileName}";
            string context = "";
            Stream stream = assembly.GetManifestResourceStream(DocumentPath);

            using (StreamReader reader=new StreamReader(stream)){
                context = reader.ReadToEnd();
            }

            Guid Dguid, Rguid;
            RegionGuidDict = new Dictionary<string, Guid>();
            DestinationGuidDict = new Dictionary<string, Guid>();

            XmlDocument doc = new XmlDocument();
            //doc.Load(DocumentPath);
            doc.LoadXml(context);

            XmlNodeList destinationList = doc.SelectNodes("navigation_graph/regions/region/Destination");
            XmlNodeList regionList = doc.SelectNodes("navigation_graph/regions/region");

            foreach(XmlNode regionNode in regionList)
            {
                Rguid = new Guid(regionNode.Attributes["id"].Value);
                RegionGuidDict.Add(regionNode.Attributes["floor"].Value, Rguid);
                Console.WriteLine($"Dict add a new Region, ID={regionNode.Attributes["id"].Value}, Name={regionNode.Attributes["floor"].Value}");
            }

            foreach(XmlNode destinationNode in destinationList)
            {
                Dguid = new Guid(destinationNode.Attributes["id"].Value);
                DestinationGuidDict.Add(destinationNode.Attributes["name"].Value, Dguid);
                Console.WriteLine($"Dict add a new Destinaiton, ID={destinationNode.Attributes["id"].Value}, Name={destinationNode.Attributes["name"].Value}");
            }
        }

        public Guid GetRegionID(string key)
        {
            return RegionGuidDict[key];
        }
        public Guid GetDestinationID(String key)
        {
            return DestinationGuidDict[key];
        }
    }
}
