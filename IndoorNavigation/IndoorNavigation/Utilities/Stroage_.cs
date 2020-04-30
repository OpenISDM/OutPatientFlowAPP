using IndoorNavigation.Models.NavigaionLayer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

namespace IndoorNavigation.Utilities
{
    public static class Stroage_
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
        private static void Storing(string sourceContext, string sinkRoute)
        {
            File.WriteAllText(sinkRoute, sourceContext);
        }
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
