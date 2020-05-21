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
        public CloudDownload()
        {
            //string contextString = Download(getSupportListUrl());
            //_currentInfos = JsonConvert.DeserializeObject<CurrentMapInfos>(contextString);
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
}