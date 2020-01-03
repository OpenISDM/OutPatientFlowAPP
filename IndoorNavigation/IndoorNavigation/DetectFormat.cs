using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Xml;

namespace IndoorNavigation
{
    class DetectFormat
    {
        public DetectFormat()
        {

        }

        public bool isJson(string input)
        {
            input = input.Trim();
            Predicate<String> IsWellformed = (str) =>
            {
                try
                {
                    JsonConvert.DeserializeObject(str);
                }
                catch
                {
                    return false;
                }
                return true;
            };

                return (input.StartsWith("{") && input.EndsWith("}")) || (input.StartsWith("[") && input.EndsWith("]")) && IsWellformed(input);
        }

        public bool isXml(string input)
        {
            input = input.Trim();

            Predicate<string> isWellform = (str) =>
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(str);
                }catch(Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                    return false;
                }
                return true;
            };

            return (input.StartsWith("") && input.EndsWith("")) || isWellform(input);
        }

        public bool isCsv(string input)
        {
            return false;
        }
    }
}
