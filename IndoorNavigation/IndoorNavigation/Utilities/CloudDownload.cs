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
        public const string _localhost = "http://140.109.22.175:3000/";
        //private string _context;
        public CurrentMapInfos _currentInfos;

        //private const string _informationFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Information");
        //private const string _firstDirectionFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"FirstDirection");
        //private const string _naviGraphFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Navigraph");       

        public CloudDownload()
        {
            string contextString = Download(getSupportListUrl());
            _currentInfos = JsonConvert.DeserializeObject<CurrentMapInfos>(contextString);
        }

        //public void DeleteFile(string fileName)
        //{
        //    string sinkInformation_zh = Path.Combine(NavigraphStorage._informationFolder, fileName + "_info_zh.xml");
        //    string sinkInformation_en = Path.Combine(NavigraphStorage._informationFolder, fileName + "_info_en-US.xml");
        //    string sinkFDData_zh = Path.Combine(NavigraphStorage._firstDirectionInstuctionFolder, fileName + "_zh.xml");
        //    string sinkFDData_en = Path.Combine(NavigraphStorage._firstDirectionInstuctionFolder, fileName + "_en-US.xml");
        //    string sinkNaviGraph = Path.Combine(NavigraphStorage._navigraphFolder, fileName);

        //    File.Delete(sinkFDData_en);
        //    File.Delete(sinkFDData_zh);
        //    File.Delete(sinkInformation_en);
        //    File.Delete(sinkInformation_zh);
        //    File.Delete(sinkNaviGraph);
        //}

        //public void GenerateFilePath(string fileName, string downloadPath)
        //{            
        //    string sourceInformation_zh = Download(getInfoUrl(downloadPath, "zh"));           
        //    string sourceInformation_en = Download(getInfoUrl(downloadPath, "en-US"));
        //    string sourceFD_zh = Download(getFDUrl(downloadPath,"zh"));
        //    string sourceFD_en = Download(getFDUrl(downloadPath,"en-US"));
        //    string sourceNaviGraph = Download(getMainUrl(downloadPath));

        //    string sinkInformation_zh = Path.Combine(NavigraphStorage._informationFolder, fileName + "_info_zh.xml");
        //    string sinkInformation_en = Path.Combine(NavigraphStorage._informationFolder, fileName + "_info_en-US.xml");
        //    string sinkFDData_zh = Path.Combine(NavigraphStorage._firstDirectionInstuctionFolder, fileName + "_zh.xml");
        //    string sinkFDData_en = Path.Combine(NavigraphStorage._firstDirectionInstuctionFolder, fileName + "_en-US.xml");
        //    string sinkNaviGraph = Path.Combine(NavigraphStorage._navigraphFolder, fileName);
        //    try 
        //    { 
        //        if (!Directory.Exists(NavigraphStorage._navigraphFolder))
        //            Directory.CreateDirectory(NavigraphStorage._navigraphFolder);
        //        if (!Directory.Exists(NavigraphStorage._firstDirectionInstuctionFolder))
        //            Directory.CreateDirectory(NavigraphStorage._firstDirectionInstuctionFolder);
        //        if (!Directory.Exists(NavigraphStorage._informationFolder))
        //            Directory.CreateDirectory(NavigraphStorage._informationFolder);

        //        Storing(sourceInformation_en, sinkInformation_en);
        //        Storing(sourceInformation_zh, sinkInformation_zh);
        //        Storing(sourceFD_en, sinkFDData_en);
        //        Storing(sourceFD_zh, sinkFDData_zh);
        //        Storing(sourceNaviGraph, sinkNaviGraph);
        //    }
        //    catch(Exception exc)
        //    {
        //        Console.WriteLine(exc.Message);
        //        throw exc;
        //    }
        //}

        //public string GetContextString()
        //{
        //    return Download("NTUH_Yunlin");
        //}

        private void Storing(string context, string sinkRoute)
        {
            File.WriteAllText(sinkRoute, context);
        }

        public bool CheckMapVersion(string localgraphName, string localVersion)
        {            
            Dictionary<string, string> VersionDict = _currentInfos.ToDictionary();
            return VersionDict[localgraphName].Equals(localVersion);            
        }

        public string Download(string url)
        {
            string ContextString = "";
            //bool Error=true;
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
                //Page CurrentPage = Application.Current.MainPage;

                //var WantRetry = await CurrentPage.DisplayAlert("Error", exc.Message, "Ok", "Cancel");

                //if (WantRetry)
                //    ContextString = await Download(url);
                //else
                //{
                //    //do something if user cancel.
                //}
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
