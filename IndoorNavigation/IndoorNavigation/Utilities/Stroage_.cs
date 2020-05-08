//This is the stroage re-structure version.

using IndoorNavigation.Models.NavigaionLayer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Linq;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
/*
note :
1. We need to define the stroage path first

2. A resource object to offer information, like : Languages, MapNames. 
   the object will constructor when call static storage class.
   to store those data, need to define the storage name, format, class structure.

3. 

then, FD is means FirstDirection.

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
                Directory.CreateDirectory(_navigraphFolder);

            if (!Directory.Exists(_informationFolder))
                Directory.CreateDirectory(_informationFolder);

            if (!Directory.Exists(_firstDirectionInstuctionFolder))
                Directory.CreateDirectory(_firstDirectionInstuctionFolder);
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
        static public void DeleteFDXml(string fileName) 
        {
            //use loop to access multi-langugaes
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
        }
        public static void EmbeddedStoring(string sourceRoute,string sinkRoute) 
        {
            string FileContext = EmbeddedSourceReader(sourceRoute);
            File.WriteAllText(sinkRoute, FileContext);            
        }

        public static void CloudGenerateFile(string sinkName) 
        {

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

