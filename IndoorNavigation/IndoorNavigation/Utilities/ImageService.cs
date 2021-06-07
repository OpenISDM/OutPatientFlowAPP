using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace IndoorNavigation.Utilities
{
    static class ImageService
    {
        //static readonly HttpClient _client = new HttpClient();
        public static bool CheckImageExistInDisk(string imageFileName)
        {
            if (string.IsNullOrEmpty(imageFileName)) 
                return false;
            if (!imageFileName.EndsWith(".png")) 
                return Preferences.ContainsKey(imageFileName + ".png");
            return Preferences.ContainsKey(imageFileName);
        }

        //public static Task<byte[]> DownloadImage(string imageUrl)
        //{
        //    if (!imageUrl.StartsWith("https", StringComparison.OrdinalIgnoreCase))
        //    {
        //        Console.WriteLine("Android and iOS can't use http!");
        //    }
        //    return _client.GetByteArrayAsync(imageUrl);
        //}

        public static void RemoveImageFromDisk(string imageFileName)
        {
            if (Preferences.ContainsKey(imageFileName))
            { 
                Preferences.Remove(imageFileName);
                Console.WriteLine("Remove picture name : " + imageFileName);
            }
        }

        public static void SaveImageToDisk(string imageFileName,
            byte[] imageBase64)
        {
            Preferences.Set(imageFileName,
                Convert.ToBase64String(imageBase64));
        }

        public static ImageSource GetImageFromDisk(string imageFileName)
        {
            return ImageSource.FromStream(() =>
                new MemoryStream(Convert.FromBase64String
                (Preferences.Get(imageFileName, string.Empty))));
        }
    }
}
