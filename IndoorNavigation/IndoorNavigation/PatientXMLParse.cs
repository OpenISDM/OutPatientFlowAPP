using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Xamarin.Forms;
using System.Collections.ObjectModel;
using System.Reflection;
using System.IO;

namespace IndoorNavigation.Models
{
    public class PatientXMLParse
    {
        private string content = "";


        public void ReadXml()
        {
            var assembly = typeof(RigisterList).GetTypeInfo().Assembly;

            Stream stream = assembly.GetManifestResourceStream(string.Format("{0}.{1}", assembly.GetName().Name, "PatientData.xml"));

            using (StreamReader reader = new StreamReader(stream))
            {
                Console.WriteLine("Test~test:" + reader.ReadToEnd());
            }

        }



    }


}
