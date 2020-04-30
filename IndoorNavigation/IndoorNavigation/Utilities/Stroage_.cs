using IndoorNavigation.Models.NavigaionLayer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

//note :
//     first step is to define the stroage path
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

        public static string[] GetAllNavigationGraphName() { }
        public static NavigationGraph LoadNavigationGraphXml(string fileName)
        {

        }

        public static XMLInformation LoadInformationXml(string fileName) 
        {

        }

        public static FirstDirectionInstruction LoadFirstDirectionXml(string fileName)
        {

        }

        #endregion

        #region Delete Graph Files
        public static void DeleteNavigationGraph(string fileName) { }
        public static void DeleteFirstDirectionXml(string fileName) { }
        public static void DeleteInfomationXml(string fileName) { }

        public static void DeleteAllGraphFile() { }

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
            var assembly = typeof(Stroage_).GetTypeInfo().Assembly;
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
