//This is the stroage re-structure version.

using IndoorNavigation.Models.NavigaionLayer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Linq;
using Newtonsoft.Json;
/*
note :
     We need to define the stroage path first


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

        internal static readonly string _embeddedResourceReoute =
            "IndoorNavigation.Resources.";

        private static object _fileLock = new object();
        private static PhoneInformation _phoneInformation = new PhoneInformation();

        public static object _fileResources;
        #endregion

        #region Static Constructor
        static NaviGraphStroage_()
        {
            Console.WriteLine(">>NaviGraphStroage Constructor");

            _fileResources = JsonConvert.DeserializeObject<FileResources>(EmbeddedSourceReader("not define yet"));
        }
        #endregion

        //private static void Storing(string sourceRoute, string sinkRoute) 
        //{
        //    using(var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(sourceRoute))
        //    {
        //        StreamReader reader = new StreamReader(stream);
        //        string fileContents = reader.ReadToEnd();
        //        File.WriteAllText(sinkRoute, fileContents.ToString());
        //    }
        //}

        #region Load Graph Files

        private static void CheckDirectoryExist()
        {
            Console.WriteLine(">>CheckDirectoryExist");
            if (!Directory.Exists(_navigraphFolder))
                Directory.CreateDirectory(_navigraphFolder);
            if (!Directory.Exists(_firstDirectionInstuctionFolder))
                Directory.CreateDirectory(_firstDirectionInstuctionFolder);
            if (!Directory.Exists(_informationFolder))
                Directory.CreateDirectory(_informationFolder);
        }
        public static string[] GetAllNavigationGraphName()
        {
            CheckDirectoryExist();
            Console.WriteLine(">>GetAllNavigationGraphName");
            string[] existNaviGraphName =
                Directory.GetFiles(_navigraphFolder)
                .Select(path => Path.GetFileName(path))
                .OrderBy(file => file)
                .ToArray();
            //string[] result = new string[existNaviGraphName.Count()];
            List<string> result = new List<string>();
            XMLInformation information;
            foreach (string fileName in existNaviGraphName)
            {
                Console.WriteLine("fileName : " + fileName);
                information = NaviGraphStroage_.LoadInformationXml(fileName);
                result.Add(information.GiveGraphName());
            }

            return result.ToArray();
        }
        public static NavigationGraph LoadNavigationGraphXml(string fileName)
        {
            Console.WriteLine(">> LoadNavigationGraphXml");
            Console.WriteLine("fileName : " + fileName);

            return new NavigationGraph(new XmlDocument());
        }

        public static XMLInformation LoadInformationXml(string fileName)
        {
            Console.WriteLine(">> LoadInformationXml");
            Console.WriteLine("fileName : " + fileName);

            return new XMLInformation(new XmlDocument());
        }

        public static FirstDirectionInstruction LoadFirstDirectionXml(string fileName)
        {
            Console.WriteLine(">> FirstDirectionInstruction");
            Console.WriteLine("fileName : " + fileName);

            return new FirstDirectionInstruction(new XmlDocument());
        }

        #endregion

        #region Delete Graph Files

        public static void DeleteFile(string path)
        {
            CheckDirectoryExist();
            Console.WriteLine(">>DeleteFile");
            lock (_fileLock)
            {
                File.Delete(path);
            }
        }

        public static void DeleteNavigationGraph(string fileName)
        {
            CheckDirectoryExist();
            string filePath = "";

            DeleteFile(filePath);
        }
        public static void DeleteFirstDirectionXml(string fileName)
        {
            CheckDirectoryExist();
            string filePath_en = "";
            string filePath_zh = "";

            DeleteFile(filePath_en);
            DeleteFile(filePath_zh);
        }
        public static void DeleteInfomationXml(string fileName)
        {
            CheckDirectoryExist();
            Console.WriteLine(">>DeleteInformationXml");
            string filePath_en = "";
            string filePath_zh = "";

            DeleteFile(filePath_en);
            DeleteFile(filePath_zh);
        }

        public static void ClearAllGraphFile()
        {
            CheckDirectoryExist();
            Console.WriteLine(">>ClearAllGraphFile");
            foreach (string buildingName in GetAllNavigationGraphName())
            {
                string map = _phoneInformation.GiveCurrentMapName(buildingName);
                DeleteFirstDirectionXml(buildingName);
                DeleteInfomationXml(buildingName);
                DeleteNavigationGraph(buildingName);
            }
        }

        //private static void DeleteAllFirstDirectionXml() { }
        //private static void DeleteAllInfomationXml() { }
        //private static void DeleteAllNaviGraphXml() { }

        #endregion

        #region Update and Write Graph files
        //public static void GenerateFileRoute(string sourceRoute, string sinkRoute)
        //{
        //    Console.WriteLine(">>GenerateFileRoute");

        //    CheckDirectoryExist();

        //    string sinkNaviGraphData = Path.Combine();
        //    string sinkInformationData_en = Path.Combine();
        //    string sinkInformationData_zh = Path.Combine();
        //    string sinkFirstDirectionData_en = Path.Combine();
        //    string sinkFirstDirectionData_zh = Path.Combine();

        //    try
        //    {

        //    }
        //    catch (Exception exc) 
        //    {
        //        Console.WriteLine("GenerateFileRoute error : " + exc.Message);
        //        throw exc;
        //    }
        //}

        private static void Storing(string sourceContext, string sinkFileName)
        {
            Console.WriteLine(">>Storing");

            string sinkFileRoute = Path.Combine();

            File.WriteAllText(sinkFileRoute, sourceContext);
        }

        private static void Storing(string sourceContext, string sinkFileName, string DocumentType, string Language)
        {
            Console.WriteLine(">>Storing");

            string sinkFileRoute = Path.Combine();

            File.WriteAllText(sinkFileRoute, sourceContext);
        }
        #endregion

        #region Others
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
    }

    public class FileResources
    {
        [JsonProperty("Resources")]
        public List<FileResource> _resources { get; set; }
        public Dictionary<string, FileResource> toDictionary()
        {
            Dictionary<string, FileResource> result = new Dictionary<string, FileResource>();

            foreach (FileResource resource in _resources)
            {
                result.Add(resource._buildingName, resource);
            }
            return result;
        }
    }
    public class FileResource
    {
        [JsonProperty("BuildingName")]
        public string _buildingName { get; set; }
        [JsonProperty("CurrentVersion")]
        public string _version { get; set; }
        [JsonProperty("NaviGraphSinkRoute")]
        public string _naviGraphSinkRoute { get; set; }        
        //need to think how to storage multi-language
        //the first string is language, second one is path
        [JsonProperty("FDDataSinkRoute")]
        public Dictionary<string,string> _fDDataSinkRoute { get; set; }
        //the first string is language, second one is path
        [JsonProperty("InfoDataSinkRoute")]
        public Dictionary<string,string> _infoDataSinkRoute { get; set; }
    }
}

