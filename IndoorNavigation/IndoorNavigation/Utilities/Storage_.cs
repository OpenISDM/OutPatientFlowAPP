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
using IndoorNavigation.Views.Navigation;
using System.Net;
using System.Resources;
using IndoorNavigation.Resources.Helpers;
using Prism.Navigation.Xaml;
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


        private static readonly string _resourceID = 
            "IndoorNavigation.Resources.AppResources";
        private static ResourceManager _resourceManager =
            new ResourceManager(_resourceID, 
                typeof(TranslateExtension).GetTypeInfo().Assembly);

        private static object _fileLock = new object();
        public static GraphResources _resources;
        public static Dictionary<string, GraphInfo> _localResources;
        public static CultureInfo _currentCulture = CrossMultilingual.Current.CurrentCultureInfo;
        public static Dictionary<string, GraphInfo> _serverResources;
        #endregion

        #region Initial
        static Storage()
        {
            Console.WriteLine(">>NaviGraphStroage Constructor");
            _resources = new GraphResources();
            GetLocalGraphNames();
            Console.WriteLine("finish get Local GraphNames");
            CreateDirectory();
            Console.WriteLine("finish Creat Directory");
            GraphResourceParse();
            Console.WriteLine("<<Storage Constructor");
        }
        #endregion
        #region Load File
        static public string GetDisplayName(string accessName)
        {
            return _resources._graphResources[accessName]
                ._displayNames[_currentCulture.Name];
        }
        static public List<Location> GetAllNaviGraphName()
        {
            Console.WriteLine(">>GetAllNaviGraphName");
            Console.WriteLine("Graph count : " + _resources._graphResources.Count);
            List<Location> names = new List<Location>();
            names =
                _resources._graphResources.Select
                (o => new Location
                {
                    sourcePath = o.Key,
                    UserNaming = o.Value._displayNames[_currentCulture.Name]
                })
                .ToList();
            Console.WriteLine("<<GetAllNaviGraphName");
            return names;
        }
        static public List<Location> GetLocalGraphNames()
        {
            List<Location> LocalNames = new List<Location>();
            _localResources = GraphInfoReader(XmlReader("Resources.GraphResource.xml"));
            LocalNames =
                    _localResources.Select(o =>
                        new Location
                        {
                            sourcePath = o.Key,
                            UserNaming = o.Value._displayNames[_currentCulture.Name]
                        })
                        .ToList();
            return LocalNames;
        }
        static public NavigationGraph LoadNavigationGraphXml(string fileName)
        {
            Console.WriteLine(">>LoadNavigationGraphXml");
            string filePath = Path.Combine(_navigraphFolder, fileName + ".xml");
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(filePath);
            }
            catch (Exception exc)
            {
                Console.WriteLine("LoadNavigationGraphXml error : " + exc.Message);
                throw exc;
            }
            return new NavigationGraph(doc);
        }
        static public FirstDirectionInstruction LoadFDXml(string fileName)
        {
            Console.WriteLine(">>NaviStorage : LoadFDXml");


            string filePath = Path.Combine(_firstDirectionInstuctionFolder, $"{fileName}_FD_{_currentCulture}.xml");
            Console.WriteLine("First Direction File path : " + filePath);
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(filePath);
            }
            catch (Exception exc)
            {
                Console.WriteLine("LoadFDXml error : " + exc.Message);
                throw exc;
            }

            return new FirstDirectionInstruction(doc);
        }
        static public XMLInformation LoadXmlInformation(string fileName)
        {
            Console.WriteLine(">>LoadXmlInformation");
            string filePath = Path.Combine(_informationFolder, $"{fileName}_info_{_currentCulture.Name}.xml");
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

            if (!File.Exists(Path.Combine(_LocalData, "ResourceStatus.xml")))
            {
                UpdateGraphList("zh-TW", AccessGraphOperate.AddLanguage);
                UpdateGraphList("en-US", AccessGraphOperate.AddLanguage);
                EmbeddedGenerateFile("NTUH_Yunlin");
                EmbeddedGenerateFile("Yuanlin_Christian_Hospital");
                //EmbeddedGenerateFile("Lab");
                EmbeddedGenerateFile("Taipei_City_Hall");
                Console.WriteLine("first Use Generate Success");
            }
        }
        static public Dictionary<string, GraphInfo> GraphInfoReader(XmlDocument xmlDocument)
        {
            Dictionary<string, GraphInfo> result = new Dictionary<string, GraphInfo>();

            XmlNodeList GraphsList = xmlDocument.SelectNodes("GraphResource/Graphs/Graph");

            foreach (XmlNode GraphNode in GraphsList)
            {
                GraphInfo info = new GraphInfo();

                string GraphName = GraphNode.Attributes["name"].Value;
                info._currentVersion =double.Parse(GraphNode.Attributes["version"].Value);
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
            var assembly = typeof(Storage).GetTypeInfo().Assembly;

            string sourceContext = "";

            Stream stream = 
                assembly.GetManifestResourceStream($"{assembly.GetName().Name}.{FileName}");
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

        public static string GetResourceString(string key)
        {
            return _resourceManager.GetString(key, _currentCulture);
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
        static public void DeleteBuildingGraph(string fileName)
        {
            DeleteFDXml(fileName);
            DeleteNaviGraph(fileName);
            DeleteXmlInformation(fileName);
            UpdateGraphList(fileName, AccessGraphOperate.Delete);
        }
        static private void DeleteFDXml(string fileName)
        {
            //use loop to access multi-langugaes

            foreach (string language in _resources._languages)
            {
                string filePath = Path.Combine(_firstDirectionInstuctionFolder, $"{fileName}_FD_{language}.xml");
                lock (_fileLock)
                    File.Delete(filePath);
            }
        }
        static private void DeleteNaviGraph(string fileName)
        {
            string filePath = Path.Combine(_navigraphFolder, fileName);

            lock (_fileLock)
                File.Delete(filePath);
        }
        static private void DeleteXmlInformation(string fileName)
        {
            //use loop to access multi-language
            foreach (string language in _resources._languages)
            {
                string filePath = Path.Combine(_informationFolder, $"{fileName}_info_{language}.xml");

                lock (_fileLock)
                    File.Delete(filePath);
            }
        }
        #endregion
        #region Write File              
        static public void EmbeddedGenerateFile(string sourceName)
        {
            Console.WriteLine(">>EmbeddedGenerateFile");

            var assembly = typeof(Storage).GetTypeInfo().Assembly;

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
                    Console.WriteLine("sourceFD : " + sourceFDFile);

                    string sinkInformationPath = Path.Combine(_informationFolder, $"{sourceName}_info_{language}.xml");
  
                    string sinkFDPath = Path.Combine(_firstDirectionInstuctionFolder, $"{sourceName}_FD_{language}.xml");
                    EmbeddedStoring(sourceInfomation, sinkInformationPath);
                    EmbeddedStoring(sourceFDFile, sinkFDPath);
                }
                Console.WriteLine("<<EmbeddedGenerateFile");
                UpdateGraphList(sourceName, AccessGraphOperate.AddLocal);

            }
            catch (Exception exc)
            {
                Console.WriteLine("EmbeddedGenerateFile error : " + exc.Message);
                throw exc;
            }
        }
        static private void EmbeddedStoring(string sourceRoute, string sinkRoute)
        {
            Console.WriteLine(">>EmbeddedStoring, sourceRoute : " +sourceRoute);
            Console.WriteLine("sinkRoute : " + sinkRoute);
            string FileContext = EmbeddedSourceReader(sourceRoute);

            if (string.IsNullOrEmpty(FileContext))
                throw new Exception("the value can't be null attributeName: " + sourceRoute);
            File.WriteAllText(sinkRoute, FileContext);
        }
        static public void CloudGenerateFile(string sourceName)
        {
            CloudDownload _clouddownload = new CloudDownload();
            string sourceNaviGraph = _clouddownload.Download(_clouddownload.getMainUrl(sourceName));            
            ValidateDownloadString(sourceNaviGraph);

            string sinkNaviGraph = Path.Combine(_navigraphFolder, sourceName + ".xml");
            Console.WriteLine("SinkNaviGraph path : " + sinkNaviGraph);
            CloudStoring(sourceNaviGraph, sinkNaviGraph);

            foreach (string language in _resources._languages)
            {
                //lock (_downloadLock)
                {
                    string sourceFD = _clouddownload.Download(_clouddownload.getFDUrl(sourceName, language));
                    string sourceInfo = _clouddownload.Download(_clouddownload.getInfoUrl(sourceName, language));

                    string sinkFD = Path.Combine(_firstDirectionInstuctionFolder, $"{sourceName}_FD_{language}.xml");
                    string sinkInfo = Path.Combine(_informationFolder, $"{sourceName}_info_{language}.xml");

                    Console.WriteLine("Sink FirstDirection path : " + sinkFD);
                    Console.WriteLine("Sink InformationXml path : " + sinkInfo);

                    ValidateDownloadString(sourceFD);
                    ValidateDownloadString(sourceInfo);

                    CloudStoring(sourceFD, sinkFD);
                    CloudStoring(sourceInfo, sinkInfo);
                }
            }
            UpdateGraphList(sourceName, AccessGraphOperate.AddServer);
        }

        static private void ValidateDownloadString(string downloadString)
        {
            XmlDocument doc = new XmlDocument();

            try
            {
                doc.LoadXml(downloadString);                
            }
            catch(Exception exc)
            {
                Console.WriteLine("validateDownloadString error - " +
                    exc.Message);

                throw exc;
            }
        }
        static private void CloudStoring(string Context, string sinkRoute)
        {
            if (string.IsNullOrEmpty(Context))
                throw new Exception("Download context can't be null or empty. Please check url or network");
            File.WriteAllText(sinkRoute, Context);
        }
        #endregion
        #region Update
        static private void UpdateGraphList(string fileName, 
            AccessGraphOperate operate)
        {
            Console.WriteLine(">>UpdateGraphList");
            switch (operate)
            {
                case AccessGraphOperate.AddLocal:
                    {
                        Console.WriteLine(">>AddLocal");
                        if (!_resources._graphResources.ContainsKey(fileName))
                        {
                            _resources._graphResources
                                .Add(fileName, _localResources[fileName]);
                        }
                        else
                        {
                            _resources._graphResources[fileName] = 
                                _localResources[fileName];
                        }
                        break;
                    }
                case AccessGraphOperate.AddServer:
                    {
                        Console.WriteLine(">>AddServer");
                        if (!_resources._graphResources.ContainsKey(fileName))
                        {                            
                            _resources._graphResources
                                .Add(fileName, _serverResources[fileName]);
                        }
                        else
                        {
                            _resources._graphResources[fileName] =
                                _serverResources[fileName];
                        }
                        break;
                    }
                case AccessGraphOperate.AddLanguage:
                    {
                        Console.WriteLine(">>AddLanguage");
                        if (!_resources._languages.Contains(fileName))
                        {
                            _resources._languages.Add(fileName);
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
            Console.WriteLine(">>GraphResourceParse");
            _resources = new GraphResources();

            string filePath = Path.Combine(_LocalData, "ResourceStatus.xml");
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);

            Dictionary<string, GraphInfo> Graphinfos = GraphInfoReader(doc);

            Console.WriteLine("Next to parse Language");

            XmlNodeList LanguageList = doc.SelectNodes("GraphResource/Languages/Language");
            foreach (XmlNode languageNode in LanguageList)
            {
                _resources._languages.Add(languageNode.Attributes["name"].Value);
            }
            _resources._graphResources = Graphinfos;
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
                    new XAttribute("version", pair.Value._currentVersion),
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

        static public bool CheckVersionNumber(string fileName, 
            double currentVersion, 
            AccessGraphOperate operate)
        {
            Console.WriteLine(">>CheckVersionNumber, Operate : " + operate);
            switch (operate)
            {
                case AccessGraphOperate.CheckLocalVersion:
                    {
                        Console.WriteLine(">>CheckVersionNumber -> local");
                        Console.WriteLine("installed version : " + currentVersion);
                        
                        if (_localResources.ContainsKey(fileName) && 
                            _localResources[fileName]._currentVersion > 
                            currentVersion)
                        {
                            Console.WriteLine("local resource version : " + 
                                _localResources[fileName]._currentVersion);
                            return true;
                        }


                        return false;
                    }
                case AccessGraphOperate.CheckCloudVersion:
                    {
                        Console.WriteLine(">>CheckVersionNumber -> Cloud");
                        if (_serverResources != null && 
                            _serverResources.ContainsKey(fileName) && 
                            _serverResources[fileName]._currentVersion > 
                            currentVersion)
                            return true;
                        return false;
                    }
                default:
                    return false;
            }


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
            public string _displayName { 
                get
                {
                    try
                    {
                        return _displayNames[_currentCulture.Name];
                    }
                    catch
                    {
                        return _displayNames.First().Value;
                    }
                }
                private set { }
            }
            public SiteSourceFrom _siteSourceFrom { get; set; }
            public string _graphName { get; set; }
            public double _currentVersion { get; set; }
            public override string ToString() => _displayNames[_currentCulture.Name];

        }

        public class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding { get { return Encoding.UTF8; } }
        }
        public enum SiteSourceFrom
        {
            Local = 0,
            Server
        }

        public enum AccessGraphOperate
        {
            AddLocal,
            AddServer,
            AddLanguage,
            Delete,
            Update,
            CheckLocalVersion,
            CheckCloudVersion
        }
        #endregion
    }
}