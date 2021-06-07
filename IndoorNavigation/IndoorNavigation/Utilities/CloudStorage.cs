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
using System.Collections.Specialized;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;

namespace IndoorNavigation.Utilities
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
        private const string _serverSite =
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
            Console.WriteLine(ContextString);
            return ContextString;
        }

        #region Get url function region
        public string getSupportListUrl()
        {
            return _serverSite;
        }
        public string getFDUrl(string graphName, string language)
        {
            return _serverSite + graphName + "/firstdirections/" + language;
        }
        public string getInfoUrl(string graphName, string language)
        {
            return _serverSite + graphName + "/infos/" + language;
        }
        public string getMainUrl(string graphName)
        {
            return _serverSite + graphName + "/main";
        }
        #endregion

        #region new functions for beta and download pictures.

        async public Task<byte[]> DownloadPicture(string buildingName, string fileName, bool isBeta)
        {
            string url = _serverSite + "pictures";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback =
                    new RemoteCertificateValidationCallback
                    (CheckSSLValidation);
            }

            request.Timeout = 5000;
            request.Method = "POST";
            request.ContentType = "application/json";

            var postdata = new { buildingName, fileName, isBeta };
            string postBody = JsonConvert.SerializeObject(postdata);
            byte[] byteArray = Encoding.UTF8.GetBytes(postBody);

            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(byteArray, 0, byteArray.Length);
            }
            byte[] responseBytes;
            using (WebResponse response = request.GetResponse())
            {
                using(Stream stream = response.GetResponseStream())
                {
                    responseBytes = new byte[response.ContentLength];
                    
                    int offset = 0;
                    int actuallyRead = 0;
                    do
                    {
                        actuallyRead = 
                            stream.Read(responseBytes, offset, 
                            responseBytes.Length - offset);
                        offset += actuallyRead;
                    } while (actuallyRead > 0);
                    return await Task.FromResult(responseBytes);
                }
            }
        }

        public string Download(object postData, string mode)
        {
            string url = _serverSite + mode;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback =
                    new RemoteCertificateValidationCallback
                    (CheckSSLValidation);
            }

            request.Timeout = 5000;
            request.Method = "POST";
            request.ContentType = "application/json";

            string postBody = JsonConvert.SerializeObject(postData);
            byte[] byteArray = Encoding.UTF8.GetBytes(postBody);
            Console.WriteLine("post body is : " + postBody);
            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(byteArray, 0, byteArray.Length);
            }

            string responseStr = "";
            using (WebResponse response = request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    responseStr = reader.ReadToEnd();
                }
            }
            return responseStr;
        }
        #endregion
    }
}