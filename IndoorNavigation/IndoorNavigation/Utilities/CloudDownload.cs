using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using Xamarin.Forms;
using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Net.Security;

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

    // The SSL certification will expire at Aug 23, 2020
    // remeber to add Https ssl certification in C# code
    // and add a autheication mechanism to check user identity
    //
    #endregion

    public class CloudDownload
    {
        public const string _localhost = "https://wpin.iis.sinica.edu.tw/";      
        public CloudDownload()
        {
            //string contextString = Download(getSupportListUrl());
            //_currentInfos = JsonConvert.DeserializeObject<CurrentMapInfos>(contextString);
        }

        #region Send Request
        public string Download(string url)
        {
            string ContextString = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            //request.ContentType = "text/xml";

            if(url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckSSLValidation);
            }

            request.Timeout = 7000;
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
        private bool CheckSSLValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }
        #endregion
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
