//This is the stroage re-structure version.

using IndoorNavigation.Models.NavigaionLayer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Linq;

//note :
//     We need to define the stroage path first
//      
//

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

            string[] existNaviGraphName = 
                Directory.GetFiles(_navigraphFolder)
                .Select(path => Path.GetFileName(path))
                .OrderBy(file => file)
                .ToArray();
            //string[] result = new string[existNaviGraphName.Count()];
            List<string> result = new List<string>();
            XMLInformation information;
            foreach(string fileName in existNaviGraphName)
            {
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
            string filePath_en = "";
            string filePath_zh = "";

            DeleteFile(filePath_en);
            DeleteFile(filePath_zh);
        }

        public static void ClearAllGraphFile() 
        {
            CheckDirectoryExist();
            foreach(string buildingName in GetAllNavigationGraphName())
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

        #region Update Graph files
        public static void GenerateFileRoute(string sourceRoute, string sinkRoute)
        {

        }

        private static void Storing(string sourceContext, string sinkRoute)
        {
            File.WriteAllText(sinkRoute, sourceContext);
        }
        #endregion
        public static XmlDocument XmlReader(string FileName)
        {
            var assembly = typeof(NaviGraphStroage_).GetTypeInfo().Assembly;
            string xmlContent = "";

            Stream stream =
                assembly.GetManifestResourceStream($"{assembly.GetName().Name}"
                                                  + $".{FileName}");

            using (StreamReader reader = new StreamReader(stream))
            {
                xmlContent = reader.ReadToEnd();
            }
            stream.Close();

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlContent);

            return xmlDocument;
        }
    }
}

