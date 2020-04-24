using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;


using Xamarin.Forms;
using Newtonsoft.Json;
using IndoorNavigation.Modules.Utilities;
//using IndoorNavigation.Resources;
namespace IndoorNavigation.Modules.Utilities
{
    public class CloudDownload
    {
        private const string _localhost = "https://localhost:port/";
        private string _context;
        private CurrentMapInfos _currentInfos;

        //private const string _informationFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Information");
        //private const string _firstDirectionFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"FirstDirection");
        //private const string _naviGraphFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Navigraph");       

        public CloudDownload()
        {

        }

        public void GenerateFilePath(string fileName)
        {
            string sinkInformation_zh = Path.Combine(NavigraphStorage._informationFolder, fileName + "_info_zh.xml");
            string sinkInformation_en = Path.Combine(NavigraphStorage._informationFolder, fileName + "_info_en-US.xml");
            string sinkFDData_zh = Path.Combine(NavigraphStorage._firstDirectionInstuctionFolder, fileName + "_zh.xml");
            string sinkFDData_en = Path.Combine(NavigraphStorage._firstDirectionInstuctionFolder, fileName + "_en-US.xml");
            string sinkNaviGraph = Path.Combine(NavigraphStorage._navigraphFolder, fileName);

            if (!Directory.Exists(NavigraphStorage._navigraphFolder))
                Directory.CreateDirectory(NavigraphStorage._navigraphFolder);
            if (!Directory.Exists(NavigraphStorage._firstDirectionInstuctionFolder))
                Directory.CreateDirectory(NavigraphStorage._firstDirectionInstuctionFolder);
            if (!Directory.Exists(NavigraphStorage._informationFolder))
                Directory.CreateDirectory(NavigraphStorage._informationFolder);


            try
            {
                
            }
            catch(Exception exc)
            {
                Console.WriteLine(exc.Message);
                throw exc;
            }
        }

        private void Storing(string context, string sinkRoute)
        {
            File.WriteAllText(sinkRoute, context);
        }

        async public Task<bool> CheckMapVersion(string localgraphName, string localVersion)
        {
            //url format : {localhost}:{port}/
            string url = _localhost + "version";

            _context = Download(url);

            _currentInfos = JsonConvert.DeserializeObject<CurrentMapInfos>(_context);
            var samaNameMap = _currentInfos.Maps.Where(p => p.Name == localgraphName).First();

            //this statement might happend error.
            if (samaNameMap.Version.Equals(localVersion))
            {
                return true;
            }
            else
            {
                return false;
            }            
        }

        async public Task DownloadFDFile(string graphName, string language)
        {
            //url format : {localhost}:{port}/firstdirections/{language}

            string url = _localhost + "firstdirections/" + language;
            _context = Download(url);

            

            await Task.CompletedTask;
        }

        async public Task DownloadInfoFile(string graphName, string language)
        {
            //url format : {localhost}:{port}/infos/{language}

            string url = _localhost + "infos/" + language;
            _context = Download(url);
            await Task.CompletedTask;
        }

        async public Task DownloadMainFile(string graphName)
        {
            //url format : {localhost}:{port}/main
            string url = _localhost + "main";
            _context = Download(url);
            await Task.CompletedTask;
        }

        //this function might be useless in future?
        //async public Task DownloadBeaconFile(string graphName)
        //{
        ////    url format : { localhost}:{ port}/ beacondata
        //    string url = _localhost + "beacondata";
        //    _context = Download(url);
        //    await Task.CompletedTask;
        //}

        private string Download(string url)
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
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        ContextString = reader.ReadToEnd();
                    }
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("Download Error : " + exc.Message);
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

            return ContextString;
        }
    }

    public class CurrentMapInfos
    {
        public List<MapInfo> Maps { get; set;}
        public List<LanguageInfo> Languages { get; set; }
    }

    public struct MapInfo
    {
        public string Name { get; set; }

        public string Version { get; set; }
        // the version might be considered as a double or a digit type, 
        // it could be more easier to compare?
    }

    public struct LanguageInfo
    {
        public string Name { get; set; }
    }

    //note 
    //  /buildingName/main                          is for waypoint id
    //  /buildingName/infos/language                is for landmark names
    //  /buildingName/firstdirections/language      is for  firstdirection
    //  /buildingName/beacondata                    is for beacon ids
    //  /                                           is for all support map and languages
}
