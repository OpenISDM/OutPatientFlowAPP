using System;
using IndoorNavigation.Models;
using Xamarin.Forms;
using System.Threading.Tasks;
using IndoorNavigation.Droid;
using System.Net;
using System.IO;

[assembly: Dependency(typeof(DownloadFile))]
namespace IndoorNavigation.Droid
{
    class DownloadFile: IDownloadFile
    {
        async public Task<bool> DownloadImage(string url, string ImageName)
        {
            var webClient = new WebClient();
            webClient.DownloadDataCompleted += (s, e) => {
                var bytes = e.Result; // get the downloaded data
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                string localFilename = "downloaded.png";
                string localPath = Path.Combine(documentsPath, localFilename);
                File.WriteAllBytes(localPath, bytes); // writes to local storage
            };
            //var url = new Uri("https://www.xamarin.com/content/images/pages/branding/assets/xamagon.png");
            webClient.DownloadDataAsync(new Uri(url));
            return await Task.FromResult(true);
        }
    }
}