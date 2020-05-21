using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using Xamarin.Forms;
using Newtonsoft.Json;
//using IndoorNavigation.Resources;
namespace IndoorNavigation.Modules.Utilities
{
    #region Notes
    //this class should be run in a thread instead of ui thread

    //the word "FD" is mean FirstDirection

    //  /buildingName/main                          is for waypoint id
    //  /buildingName/infos/language                is for landmark names
    //  /buildingName/firstdirections/language      is for  firstdirection
    //  /buildingName/beacondata                    is for beacon ids
    //  /                                           is for all support map and languages
    #endregion

    public class CloudDownload
    {
        public const string _localhost = "http://140.109.22.34:80/";
        //private string _context;
        public CurrentMapInfos _currentInfos;
      
        public CloudDownload()
        {
            //string contextString = Download(getSupportListUrl());
            //_currentInfos = JsonConvert.DeserializeObject<CurrentMapInfos>(contextString);
        }            

        public bool CheckMapVersion(string localgraphName, string localVersion)
        {            
            Dictionary<string, string> VersionDict = _currentInfos.ToDictionary();
            return VersionDict[localgraphName].Equals(localVersion);            
        }
        
        public string Download(string url)
        {
            string ContextString = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            //request.ContentType = "text/xml";
            request.Timeout = 10000;
            request.Method = "GET";

            try
            {
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    Console.WriteLine("url : " + url + ", StatusCode : " + response.StatusCode);

                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        ContextString = reader.ReadToEnd();
                    }
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("Download Error : " + exc.Message + ", url : " + url);
                ContextString = "";                
            }
            finally
            {
                request.Abort();
            }
            Console.WriteLine("url :" + url + ", ContextString : " + ContextString);
            return ContextString;
        }

        #region Get url function region
        public string getSupportListUrl()
        {
            return _localhost;
        }
        public string getFDUrl(string graphName, string language)
        {
            return _localhost + graphName + "/firstdirections/" + language;
        }
        public string getInfoUrl(string graphName, string language)
        {
            return _localhost + graphName + "/infos/" + language;
        }     
        public string getMainUrl(string graphName)
        {
            return _localhost + graphName + "/main";
        }       
        
        //the function is not supported now.
        public string getBeaconListUrl(string graphName)
        {
            return _localhost + graphName + "/beacons";
        }
        #endregion
 
    }

    public class CurrentMapInfos
    {
        [JsonProperty("Maps")]
        public List<MapInfo> Maps { get; set;}

        [JsonProperty("Languages")]
        public List<LanguageInfo> Languages { get; set; }

        public Dictionary<string, string> ToDictionary()
        {
            Dictionary<string, string> results = new Dictionary<string, string>();

            foreach(MapInfo info in Maps)
            {
                results.Add(info.Name, info.Version);
            }

            return results;
        }
    }

    public struct MapInfo
    {
        [JsonProperty("Name")]
        public string Name { get; set; }
        [JsonProperty("Version")]
        public string Version { get; set; }
        // the version might be considered as a double or a digit type, 
        // it could be more easier to compare?
    }

    public struct LanguageInfo
    {
        [JsonProperty("Name")]
        public string Name { get; set; }
    }   

}
