//This is the stroage re-structure version.

using IndoorNavigation.Models.NavigaionLayer;
using IndoorNavigation.Modules.Utilities;
using IndoorNavigation.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using Plugin.Multilingual;
using System.Xml.Linq;
using System.Text;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using System.Security;
using System.Linq;
using System.Globalization;
using System.Security.Authentication;
using Dijkstra.NET.Model;
/*
note :
1. We need to define the stroage path first

2. A resource object to offer information, like : Languages, MapNames. 
the object will constructor when call static storage class.
to store those data, need to define the storage name, format, class structure.

3. 

then, FD means FirstDirection.

*/

namespace IndoorNavigation.Utilities
{
    public static class Storage
    {
        #region Static Pathes and objects
        internal static readonly string _LocalData =
           Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        internal static readonly string _navigraphFolder =
            Path.Combine(_LocalData, "Navigraph");

        internal static readonly string _firstDirectionInstuctionFolder
             = Path.Combine(_LocalData, "FirstDirection");

        internal static readonly string _informationFolder
             = Path.Combine(_LocalData, "Information");
              

        private static object _fileLock = new object();
        public static GraphResources _resources;
        public static Dictionary<string, GraphInfo> _localResources;
        public static CultureInfo _currentLanguage;
        #endregion

        #region Initial
        static Storage()
        {
            Console.WriteLine(">>NaviGraphStroage Constructor");
            _resources = new GraphResources();
            GetLocalGraphNames();
            CreateDirectory();
            Console.WriteLine("finish Creat Directory");
            GraphResourceParse();
            Console.WriteLine("<<Storage Constructor");                      
        }                
        #endregion

        #region Load File
        static public List<Location> GetAllNaviGraphName() 
        {
            Console.WriteLine(">>GetAllNaviGraphName");
            List<Location> names = new List<Location>();
            //names = 
            //    _resources._graphResources.Select(o => 
            //        new Location { 
            //        sourcePath = o.Key, 
            //        UserNaming = o.Value._displayNames[_currentLanguage.Name] 
            //        })
            //    .ToList();
            //Console.WriteLine("name :" + names[1]);
            foreach (KeyValuePair<string, GraphInfo> pair in _resources._graphResources)
            {
                Console.WriteLine("pair value : " + pair.Key, "pair key : " + pair.Value._displayNames[CrossMultilingual.Current.CurrentCultureInfo.Name]);
                names.Add(new Location
                {
                    sourcePath = pair.Key,
                    UserNaming = pair.Value._displayNames[CrossMultilingual.Current.CurrentCultureInfo.Name]
                }
                );
            }
            Console.WriteLine("<<GetAllNaviGraphName");
            return names;
        }       
        static public List<Location> GetLocalGraphNames()
        {
            List<Location> LocalNames = new List<Location>();
            _localResources = GraphInfoReader(XmlReader("Resources.GraphResource.xml"));
            foreach (KeyValuePair<string, GraphInfo> pair in _localResources)
            {
                Console.WriteLine("pair value : " + pair.Key, "pair key : " + pair.Value._displayNames[CrossMultilingual.Current.CurrentCultureInfo.Name]);
                LocalNames.Add(new Location
                {
                    sourcePath = pair.Key,
                    UserNaming = pair.Value._displayNames[CrossMultilingual.Current.CurrentCultureInfo.Name]
                }
                );
            }
            //LocalNames = 
            //        _localResources.Select(o => 
            //            new Location { sourcePath = o.Key, 
            //                           UserNaming = o.Value._displayNames[CrossMultilingual.Current.CurrentCultureInfo.Name] })
            //            .ToList();
            return LocalNames;
        }
        static public  NavigationGraph LoadNavigationGraphXml(string fileName) 
        {
            Console.WriteLine(">>LoadNavigationGraphXml");
            string filePath = Path.Combine(_navigraphFolder, fileName + ".xml");
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(filePath);
            }
            catch(Exception exc)
            {
                Console.WriteLine("LoadNavigationGraphXml error : " + exc.Message);
                throw exc;
            }
            return new NavigationGraph(doc);
        }
        static public FirstDirectionInstruction LoadFDXml(string fileName) 
        {
            Console.WriteLine(">>NaviStorage : LoadFDXml");

            
            string filePath = Path.Combine(_firstDirectionInstuctionFolder, $"{fileName}_FD_{CrossMultilingual.Current.CurrentCultureInfo.Name}.xml");
            Console.WriteLine("First Direction File path : " + filePath);
            XmlDocument doc = new XmlDocument();
            try
            {                
                doc.Load(filePath);
            }
            catch(Exception exc)
            {
                Console.WriteLine("LoadFDXml error : " + exc.Message);
                throw exc;
            }

            return new FirstDirectionInstruction(doc);
        }
        static public XMLInformation LoadXmlInformation(string fileName)
        {
            Console.WriteLine(">>LoadXmlInformation");
            string filePath = Path.Combine(_informationFolder, $"{fileName}_info_{CrossMultilingual.Current.CurrentCultureInfo.Name}.xml");
            Console.WriteLine("Information file path : " + filePath);
            XmlDocument doc = new XmlDocument();

            try
            {
                doc.Load(filePath);
            }
            catch (Exception exc)
            {
                Console.WriteLine("LoadXmlInformation error : " + exc.Message);
                throw exc;
            }
            return new XMLInformation(doc);
        }
        #region Others
        static private void CreateDirectory()
        {
            if (!Directory.Exists(_LocalData))
            {
                Directory.CreateDirectory(_LocalData);
                Console.WriteLine("Create LocalData Folder Success!");
            }
            if (!Directory.Exists(_navigraphFolder))
            {                
                Directory.CreateDirectory(_navigraphFolder);
                Console.WriteLine("Create naviGraphFolder success!");
            }
            if (!Directory.Exists(_informationFolder))
            {
                Directory.CreateDirectory(_informationFolder);
                Console.WriteLine("Create informationFolder success!");
            }
            if (!Directory.Exists(_firstDirectionInstuctionFolder))
            {
                Directory.CreateDirectory(_firstDirectionInstuctionFolder);
                Console.WriteLine("Create FD Folder success!");
            }

            if(!File.Exists(Path.Combine(_LocalData, "ResourceStatus.xml")))
            {
                EmbeddedGenerateFile("NTUH_Yunlin");
                EmbeddedGenerateFile("Lab");
                EmbeddedGenerateFile("Taipei_City_Hall");
                Console.WriteLine("first Use Generate Success");
            }
        }
        static private Dictionary<string, GraphInfo> GraphInfoReader(XmlDocument xmlDocument)
        {
            Dictionary<string, GraphInfo> result = new Dictionary<string, GraphInfo>();

            XmlNodeList GraphsList = xmlDocument.SelectNodes("GraphResource/Graphs/Graph");

            foreach (XmlNode GraphNode in GraphsList)
            {
                GraphInfo info = new GraphInfo();

                string GraphName = GraphNode.Attributes["name"].Value;
                info._localVersion = GraphNode.Attributes["version"].Value;
                info._graphName = GraphName;

                XmlNodeList DisplayNameList = GraphNode.SelectNodes("DisplayNames/DisplayName");

                foreach (XmlNode displayName in DisplayNameList)
                {
                    Console.WriteLine("DisplayName context : " + displayName.Attributes["name"].Value);
                    info._displayNames.Add(displayName.Attributes["language"].Value, displayName.Attributes["name"].Value);
                }
                result.Add(GraphName, info);
            }
            return result;
        }
        public static string EmbeddedSourceReader(string FileName)
        {
            Console.WriteLine(">>EmbeddedSourceReader");
            Console.WriteLine("FileName : " + FileName);
            var assembly = typeof(NavigraphStorage).GetTypeInfo().Assembly;

            string sourceContext = "";
            Stream stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.{FileName}");
            using (StreamReader reader = new StreamReader(stream))
            {
                sourceContext = reader.ReadToEnd();
            }
            stream.Close();
            Console.WriteLine("<<EmbeddedSourecReader");
            return sourceContext;
        }
        public static XmlDocument XmlReader(string FileName)
        {
            Console.WriteLine(">>XmlReader");
            string xmlContent = EmbeddedSourceReader(FileName);

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlContent);

            return xmlDocument;
        }
        #endregion

        #endregion

        #region Delete File
        static public void DeleteAllGraphFiles()
        {
            foreach (Location location in GetAllNaviGraphName())
            {
                DeleteNaviGraph(location.sourcePath);
                DeleteXmlInformation(location.sourcePath);
                DeleteFDXml(location.sourcePath);
                UpdateGraphList(location.sourcePath, AccessGraphOperate.Delete);                
            }
        }
        static public void DeleteFDXml(string fileName) 
        {
            //use loop to access multi-langugaes

            foreach(string language in _resources._languages)
            {
                string filePath = Path.Combine(_firstDirectionInstuctionFolder, $"{fileName}_FD_{language}.xml");
                lock (_fileLock)
                    File.Delete(filePath);
            }
        }
        static public void DeleteNaviGraph(string fileName) 
        {
            string filePath = Path.Combine(_navigraphFolder, fileName);

            lock (_fileLock)
                File.Delete(filePath);
        }
        static public void DeleteXmlInformation(string fileName) 
        {
            //use loop to access multi-language
            foreach(string language in _resources._languages)
            {
                string filePath = Path.Combine(_informationFolder,$"{fileName}_info_{language}.xml");

                lock (_fileLock)
                    File.Delete(filePath);
            }
        }
        #endregion
        #region Write File              
        static public void EmbeddedGenerateFile(string sourceName) 
        {
            Console.WriteLine(">>EmbeddedGenerateFile");

            var assembly = typeof(NavigraphStorage).GetTypeInfo().Assembly;

            try
            {
                string sourceNaviGraph = $"Resources.{sourceName}.{sourceName}.xml";
                string sinkNaviGraph = Path.Combine(_navigraphFolder, sourceName + ".xml");

                EmbeddedStoring(sourceNaviGraph, sinkNaviGraph);

                foreach (string language in _resources._languages)
                {
                    string sourceInfomation = $"Resources.{sourceName}.{sourceName}_info_{language}.xml";
                    Console.WriteLine("sourceInfo : " + sourceInfomation);
                    string sourceFDFile = $"Resources.{sourceName}.{sourceName}_FD_{language}.xml";
                    Console.WriteLine("sourceFD : " +sourceFDFile);

                    string sinkInformationPath = Path.Combine(_informationFolder, $"{sourceName}_info_{language}.xml");
                    string sinkFDPath = Path.Combine(_firstDirectionInstuctionFolder, $"{sourceName}_FD_{language}.xml");

                    EmbeddedStoring(sourceInfomation, sinkInformationPath);
                    EmbeddedStoring(sourceFDFile, sinkFDPath);
                }
                Console.WriteLine("<<EmbeddedGenerateFile");
                UpdateGraphList(sourceName, AccessGraphOperate.AddLocal);
                
            }
            catch(Exception exc)
            {
                Console.WriteLine("EmbeddedGenerateFile error : " + exc.Message);
                throw exc;
            }
        }
        static private void EmbeddedStoring(string sourceRoute,string sinkRoute) 
        {
            string FileContext = EmbeddedSourceReader(sourceRoute);
            File.WriteAllText(sinkRoute, FileContext);            
        }
        static public void CloudGenerateFile(string sourceName) 
        {
            CloudDownload _clouddownload = new CloudDownload();
            string sourceNaviGraph = _clouddownload.Download(_clouddownload.getMainUrl(sourceName));
            string sinkNaviGraph = Path.Combine(_navigraphFolder, sourceName);

            CloudStoring(sourceNaviGraph, sinkNaviGraph);

            foreach(string language in _resources._languages)
            {
                //lock (_downloadLock)
                {
                    string sourceFD = _clouddownload.Download(_clouddownload.getFDUrl(sourceName, language));
                    string sourceInfo = _clouddownload.Download(_clouddownload.getInfoUrl(sourceName, language));

                    string sinkFD = Path.Combine(_firstDirectionInstuctionFolder, $"{sourceName}_FD_{language}.xml");
                    string sinkInfo = Path.Combine(_informationFolder, $"{sourceName}_info_{language}.xml");

                    CloudStoring(sourceFD, sinkFD);
                    CloudStoring(sourceInfo, sinkInfo);
                }
            }
            UpdateGraphList(sourceName, AccessGraphOperate.AddServer);
        }
        static private void CloudStoring(string Context,string sinkRoute) 
        {
            File.WriteAllText(sinkRoute, Context);
        }
        #endregion

        #region Update
        static private void UpdateGraphList(string fileName, AccessGraphOperate operate)
        {
            Console.WriteLine(">>UpdateGraphList");            
            switch (operate)
            {
                case AccessGraphOperate.AddLocal:
                    {
                        Console.WriteLine(">>AddLocal");
                        if (!_resources._graphResources.ContainsKey(fileName))
                        {
                            _resources._graphResources.Add(fileName, _localResources[fileName]);
                        }
                        break;
                    }
                case AccessGraphOperate.AddServer:
                    {
                        Console.WriteLine(">>AddServer");
                        if (!_resources._graphResources.ContainsKey(fileName))
                        {

                        }
                        break;
                    }
                case AccessGraphOperate.Delete:
                    {
                        Console.WriteLine(">>Delete");
                        if (_resources._graphResources.ContainsKey(fileName))
                        {
                            _resources._graphResources.Remove(fileName);
                        }
                        break;
                    }                
                default:
                    break;
            }
            StoreGraphStatus();
        }
        static private void GraphResourceParse()
        {
            _resources = new GraphResources();

            string filePath = Path.Combine(_LocalData, "ResourceStatus.xml");
            Console.WriteLine("file context : " + File.ReadAllText(filePath));
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);

            Dictionary<string, GraphInfo> Graphinfos = GraphInfoReader(doc);
            Console.WriteLine("Graph context is :" + Graphinfos.Count);
            Console.WriteLine("Next to parse Language");

            XmlNodeList LanguageList = doc.SelectNodes("GraphResource/Languages/Language");
            foreach (XmlNode languageNode in LanguageList)
            {
                Console.WriteLine(">> languagesNode name : " + languageNode.OuterXml);
                _resources._languages.Add(languageNode.Attributes["name"].Value);
            }
            Console.WriteLine("Language context is : " + _resources._languages.Count);

        }
        static private void StoreGraphStatus()
        {
            Console.WriteLine(">>StoreGraphStatus");
            XElement root = new XElement("GraphResource");
            XElement GraphsElement = new XElement("Graphs");

            foreach (KeyValuePair<string, GraphInfo> pair in _resources._graphResources)
            {
                XElement displayNameElement = new XElement("DisplayNames");
                foreach (KeyValuePair<string, string> displaynamePair in pair.Value._displayNames)
                {
                    displayNameElement.Add(new XElement("DisplayName", new XAttribute("name", displaynamePair.Value), new XAttribute("language", displaynamePair.Key)));
                }
                GraphsElement.Add(new XElement("Graph",
                    new XAttribute("name", pair.Key),
                    new XAttribute("version", pair.Value._localVersion),
                    displayNameElement));
            }

            XElement LanguageElement = new XElement("Languages");

            foreach (string language in _resources._languages)
            {
                LanguageElement.Add(new XElement("Language", new XAttribute("name", language)));
            }
            root.Add(GraphsElement, LanguageElement);

            XDocument doc = new XDocument(new XDeclaration("1.0", "utf-8", null), root);

            using (StringWriter writer = new Utf8StringWriter())
            {
                doc.Save(writer);
                string filePath = Path.Combine(_LocalData, "ResourceStatus.xml");
                File.WriteAllText(filePath, writer.ToString());

                Console.WriteLine(">>StoreGraphStatus writer : " + writer.ToString());                        
            }
            Console.WriteLine("<<StoreGraphStatus");
        }
        #endregion
        #region Enums and Classes

        public class GraphResources
        {

            public Dictionary<string, GraphInfo> _graphResources { get; set; }
            public List<string> _languages { get; set; }

            public GraphResources()
            {
                _graphResources = new Dictionary<string, GraphInfo>();
                _languages = new List<string>();
            }
        }

        public class GraphInfo
        {
            public GraphInfo()
            {
                _displayNames = new Dictionary<string, string>();
            }
            public Dictionary<string, string> _displayNames { get; set; }
            public string _graphName { get; set; }
            public string _localVersion { get; set; }
        }       

        public class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding { get { return Encoding.UTF8; } }
        }

        public enum AccessGraphOperate
        {
            AddLocal,
            AddServer,
            Delete,
            Update
        }
        #endregion
    }   
}