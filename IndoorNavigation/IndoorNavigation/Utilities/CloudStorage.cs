/*
 * 2020 © Copyright (c) BiDaE Technology Inc. 
 * Provided under BiDaE SHAREWARE LICENSE-1.0 in the LICENSE.
 *
 * Project Name:
 *
 *      IndoorNavigation
 *
 * Version:
 *
 *      1.0.0, 20200221
 * 
 * File Name:
 *
 *      AddPopupPage.cs
 *
 * Abstract:
 *      
 *
 *      
 * Authors:
 * 
 *      Jason Chang, jasonchang@bidae.tech 
 *      
 */
using System;
using System.Net;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace IndoorNavigation.Modules.Utilities
{
    #region Notes
    //this class should be run in a thread instead of ui thread

    //the word "FD" is mean FirstDirection

    //  /buildingName/main is for waypoint id
    //  /buildingName/infos/language is for landmark names
    //  /buildingName/firstdirections/language is for  firstdirection
    //  /buildingName/beacondata is for beacon ids
    //  / is for all support map and languages
    #endregion

    public class CloudDownload
    {
        private const string _localhost =
            "https://ec2-18-183-238-222.ap-northeast-1.compute.amazonaws.com/";
        public CloudDownload()
        {
        }

        private bool CheckSSLValidation(object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors errors)
        {
            return true;
        }

        public string Download(string url)
        {
            string ContextString = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback =
                    new RemoteCertificateValidationCallback
                    (CheckSSLValidation);
            }

            request.Timeout = 5000;
            request.Method = "GET";

            try
            {
                using (HttpWebResponse response = request.GetResponse() as
                    HttpWebResponse)
                {
                    using (StreamReader reader =
                        new StreamReader(response.GetResponseStream()))
                    {
                        ContextString = reader.ReadToEnd();
                    }
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("Download Error : " + exc.Message +
                    ", url : " + url);
                ContextString = "";
                throw exc;
            }
            finally
            {
                request.Abort();
            }
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
        #endregion

    }
}