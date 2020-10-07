using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Foundation;
using UIKit;
using IndoorNavigation.Models;
using Xamarin.Forms;
using System.Threading.Tasks;
using IndoorNavigation.iOS;
using System.IO;

[assembly: Dependency(typeof(DownloadFile))]
namespace IndoorNavigation.iOS
{
    class DownloadFile : IDownloadFile
    {
        async public Task<bool> DownloadImage(string url, string ImageName)
        {
            var webClient = new WebClient();
            webClient.DownloadDataCompleted += (s, e) => {
                var bytes = e.Result; // get the downloaded data
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                //string localFilename = "downloaded.png";
                string localPath = Path.Combine(documentsPath, ImageName);
                File.WriteAllBytes(localPath, bytes); // writes to local storage
            };
            //var url = new Uri("https://www.xamarin.com/content/images/pages/branding/assets/xamagon.png");
            webClient.DownloadDataAsync(new Uri(url));

            return await Task.FromResult(true);
        }
    }
}