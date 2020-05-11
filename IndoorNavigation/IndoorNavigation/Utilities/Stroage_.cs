//This is the stroage re-structure version.

using IndoorNavigation.Models.NavigaionLayer;
using IndoorNavigation.Modules.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;

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
    public static class NaviGraphStroage_
    {
        #region Static Pathes and objects
        internal static readonly string _navigraphFolder =
            Path.Combine(Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                    "Navigraph");
        internal static readonly string _firstDirectionInstuctionFolder
             = Path.Combine(Environment
                            .GetFolderPath(Environment
                            .SpecialFolder.LocalApplicationData),
                            "FirstDirection");

        internal static readonly string _informationFolder
             = Path.Combine(Environment
                            .GetFolderPath(Environment
                            .SpecialFolder.LocalApplicationData),
                            "Information");

        //internal static readonly string _embeddedResourceReoute =
        //    "IndoorNavigation.Resources.";

        private static object _fileLock = new object();
        //private static PhoneInformation _phoneInformation = new PhoneInformation();

        //public static object _fileResources;
        public static GraphResource _resources;
        #endregion

        #region Static Constructor
        static NaviGraphStroage_()
        {
            Console.WriteLine(">>NaviGraphStroage Constructor");
            _resources = new GraphResource();

            _resources.AddLanguage(new CultureInfo("zh-TW"));            
            _resources.AddLanguage(new CultureInfo("en-US"));

            CreateDirectory();            
        }
        #endregion

        #region Load File
        static public List<string> GetAllNaviGraphName() 
        {
            //CheckDirectory();
            //List<string> result = new List<string>();
            return _resources.GraphNames;
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
        static public FirstDirectionInstruction LoadFDXml(string fileName, string language) 
        {
            Console.WriteLine(">>NaviStorage : LoadFDXml");
            string filePath = Path.Combine(_firstDirectionInstuctionFolder, $"{fileName}_FD_{language}.xml");
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
        static public XMLInformation LoadXmlInformation(string fileName, string language)
        {
            Console.WriteLine(">>LoadXmlInformation");
            string filePath = Path.Combine(_informationFolder, $"{fileName}_info_{language}.xml");
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
        }

        public static string EmbeddedSourceReader(string FileName)
        {
            Console.WriteLine(">>EmbeddedSourceReader");
            var assembly = typeof(NaviGraphStroage_).GetTypeInfo().Assembly;

            string sourceContext = "";
            Stream stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.{FileName}");

            using (StreamReader reader = new StreamReader(stream))
            {
                sourceContext = reader.ReadToEnd();
            }
            stream.Close();
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
            foreach (string GraphName in GetAllNaviGraphName())
            {
                DeleteNaviGraph(GraphName);
                DeleteXmlInformation(GraphName);
                DeleteFDXml(GraphName);

                UpdateGraphList(GraphName, AccessGraphOperate.Delete);
            }
        }

        static public void DeleteGraphFile(string fileName)
        {
            if (_resources.GraphNames.Contains(fileName))
            {
                try
                {
                    DeleteNaviGraph(fileName);
                    DeleteXmlInformation(fileName);
                    DeleteFDXml(fileName);
                }
                catch(Exception exc)
                {
                    Console.WriteLine("DeleteGraphFile error : ", exc.Message);
                    throw exc;
                }
                _resources.GraphNames.Remove(fileName);
            }
        }

        static public void DeleteFDXml(string fileName) 
        {
            //use loop to access multi-langugaes

            foreach(string language in _resources.Languages)
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
            foreach(string language in _resources.Languages)
            {
                string filePath = Path.Combine(_informationFolder,$"{fileName}_info_{language}.xml");

                lock (_fileLock)
                    File.Delete(filePath);
            }
        }
        #endregion

        #region Update or Write File

        // The GraphList will constructor and store in storage when Storage constructor.
        // The list need to think which structure to use.
        public static void UpdateGraphList(string fileName, AccessGraphOperate operate)
        {
            switch (operate)
            {
                case AccessGraphOperate.Add:
                {
                    break;
                }
                case AccessGraphOperate.Delete:
                {
                    
                    break;
                }
                default:
                    break;
            }
        }

        public static void EmbeddedGenerateFile(string sourceName, string sinkName) 
        {
            Console.WriteLine(">>EmbeddedGenerateFile");

            var assembly = typeof(NaviGraphStroage_).GetTypeInfo().Assembly;

            try
            {
                string sourceNaviGraph = $"{assembly.GetName().Name}.{sourceName}.{sourceName}.xml";
                string sinkNaviGraph = Path.Combine(_navigraphFolder,sinkName);

                EmbeddedStoring(sourceNaviGraph, sinkNaviGraph);

                foreach (string language in _resources.Languages)
                {
                    string sourceInfomation = $"{assembly.GetName().Name}";
                    string sourceFDFile = $"{assembly.GetName().Name}";

                    string sinkInformationPath = Path.Combine(_informationFolder, $"{sinkName}_info_{language}.xml");
                    string sinkFDPath = Path.Combine(_firstDirectionInstuctionFolder, $"{sinkName}_FD_{language}.xml");

                    EmbeddedStoring(sourceInfomation, sinkInformationPath);
                    EmbeddedStoring(sourceFDFile, sinkFDPath);
                }
                _resources.GraphNames.Add(sourceName);
                Console.WriteLine("<<EmbeddedGenerateFile");
            }
            catch(Exception exc)
            {
                Console.WriteLine("EmbeddedGenerateFile error : " + exc.Message);
                throw exc;
            }
        }
        public static void EmbeddedStoring(string sourceRoute,string sinkRoute) 
        {
            string FileContext = EmbeddedSourceReader(sourceRoute);
            File.WriteAllText(sinkRoute, FileContext);            
        }

        private static object _downloadLock =new object();

        public static void CloudGenerateFile(string sourceName, string sinkName) 
        {
            CloudDownload _clouddownload = new CloudDownload();
            string sourceNaviGraph = _clouddownload.Download(_clouddownload.getMainUrl(sourceName));
            string sinkNaviGraph = Path.Combine(_navigraphFolder, sinkName);

            CloudStoring(sourceNaviGraph, sinkNaviGraph);

            foreach(string language in _resources.Languages)
            {
                //lock (_downloadLock)
                {
                    string sourceFD = _clouddownload.Download(_clouddownload.getFDUrl(sourceName, language));
                    string sourceInfo = _clouddownload.Download(_clouddownload.getInfoUrl(sourceName, language));

                    string sinkFD = Path.Combine(_firstDirectionInstuctionFolder, $"{sinkName}_FD_{language}.xml");
                    string sinkInfo = Path.Combine(_informationFolder, $"{sinkName}_info_{language}.xml");

                    CloudStoring(sourceFD, sinkFD);
                    CloudStoring(sourceInfo, sinkInfo);
                }
            }
        }
        public static void CloudStoring(string Context,string sinkRoute) 
        {
            File.WriteAllText(sinkRoute, Context);
        }
        #endregion

        #region Enums and Classes

        //the function name and structure need to be considered..haha...I have no idea now Orz
        public class GraphResource
        {
            public List<string> GraphNames { get; set; }
            public List<string> Languages { get; set; }

            public void AddLanguage(CultureInfo language)
            {
                Console.WriteLine("AddLanguage : " + language.Name);
                Languages.Add(language.Name);
            }
        }

        public enum AccessGraphOperate
        {
            Add,
            Delete,
            Update
        }

        #endregion
    }
    
}

