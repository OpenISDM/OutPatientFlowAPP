using IndoorNavigation.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Xml;

using System.Net.Security;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace IndoorNavigation.Yuanlin_OPFM
{
    public class YunalinHttpRequestFake
    {
        private const string requestWebUrl = 
            "wpin.iis.sinica.edu.tw/oppa?building=CCH" ;
        private App app;
        public YunalinHttpRequestFake()
        {
            app = (App)Application.Current;
        }

        // for skip ssl check
        private bool CheckSSLValidation(object sender, 
            X509Certificate certificate, 
            X509Chain chain, 
            SslPolicyErrors errors)
        {
            return true;
        }

        public string GetJsonBody(string PatientID, DateTime OpdDate)
        {
            Console.WriteLine(">>GexXmlBody");
            string JsonBodyString = Storage.EmbeddedSourceReader("");

            PostObject postObject = 
                JsonConvert.DeserializeObject<PostObject>(JsonBodyString);

            Console.WriteLine("Apipassword : " + postObject.ApiPassword);
            Console.WriteLine("ApiAccount : " + postObject.ApiAccount);
            Console.WriteLine("PatientID : " + postObject.PatientID);
            Console.WriteLine("OpdDate :" + postObject.OpdDate);

            return JsonBodyString;
        }

        async public Task RequestFakeHIS()
        {
            Console.WriteLine(">>RequestFakeHIS");

            HttpWebRequest request =
                (HttpWebRequest)WebRequest.Create(requestWebUrl);

            if(requestWebUrl.StartsWith("https", 
                StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback =
                    new RemoteCertificateValidationCallback(CheckSSLValidation);
            }

            request.ContentType = "Application/json";
            request.Timeout = 20000;
            request.Method = "POST";

            byte[] RequestBytes = 
                Encoding.UTF8.GetBytes(GetJsonBody("", new DateTime()));

            request.ContentLength = RequestBytes.Length;

            try
            {
                using(Stream postStream = await request.GetRequestStreamAsync())
                {
                    postStream.Write(RequestBytes, 0, RequestBytes.Length);
                }
            }
            catch(Exception exc)
            {
                Console.WriteLine("Network error - request error : " 
                    + exc.StackTrace);
            }

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)
                    await request.GetResponseAsync())
                {
                    using(StreamReader streamReader = 
                        new StreamReader(response.GetResponseStream()))
                    {
                        string content = streamReader.ReadToEnd();

                        ContextParse();
                    }
                }
            }
            catch(Exception exc)
            {
                Console.WriteLine("Network error - no response " +
                    exc.StackTrace);

                // there need to implement a error handle to ask user whether he 
                // or she want to try again or not.
            }
            finally
            {
                request.Abort();                
            }
            
        }

        public object ContextParse() 
        {
            throw new NotImplementedException();
        }
        private string GetResourceString(string key) 
        { 
            throw new NotImplementedException(); 
        }

        class PostObject
        {
            public string ApiAccount { get; set; }
            public string ApiPassword { get; set; }
            public string PatientID { get; set; }
            public string OpdDate { get; set; }
        }
    }
}
