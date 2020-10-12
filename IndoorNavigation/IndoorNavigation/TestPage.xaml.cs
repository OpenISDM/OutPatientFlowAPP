using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using IndoorNavigation.Modules;
using IndoorNavigation.Modules.IPSClients;
using IndoorNavigation.Models;
using Xamarin.Essentials;
using Plugin.Geolocator.Abstractions;
using Plugin.Geolocator;
using System.Diagnostics;
using System.Net.Http;
using System.IO;

namespace IndoorNavigation
{   
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TestPage : ContentPage
    {
        public TestPage()
        {
            InitializeComponent();
        }

        async private void DownloadClick(object sender, EventArgs e)
        {
            if (!ImageService.CheckImageExist("aaa.jpg"))
            {
                Console.WriteLine("don't contains aaa.jpg");
                byte[] imageByte = await ImageService.DownloadImage("https://i.imgur.com/xQfmQBY.jpg");

                ImageService.SaveImageToDisk("aaa.jpg", imageByte);
            }

            TestImage.Source = ImageService.GetImageFromDisk("aaa.jpg");
        }
        #region  to try to use GPS location package.
        //private IGeolocator Locator => CrossGeolocator.Current;
        //private Stopwatch watch; //= new Stopwatch();
        //WaypointClient waypointClient;
        //public TestPage()
        //{
        //    InitializeComponent();
        //    watch = new Stopwatch();

        //    watch.Start();
        //    TestListen();
        //    Locator.PositionChanged += PositionChanged;

        //}

        //private void PositionChanged(object sender, PositionEventArgs e)
        //{
        //    var position = e.Position;
        //    string s = "";
        //    /*Console.WriteLine*/
        //    s += "\nTimer : " + watch.ElapsedMilliseconds;
        //    WatchTimerRestart();
        //    s+=("\nLatitude : " + position.Latitude) ;
        //    /*Console.WriteLine*/
        //    s+=("\nLongitude : " + position.Longitude);
        //    /*Console.WriteLine*/
        //    s+=("\nAltitude : " + position.Altitude);
        //    s += ("\nHeading : " + position.Heading);
        //    s += ("\nAccuracy : " + position.Accuracy);
        //    s += "\n";
        //    TestLab.Text += s;
        //}

        //private void WatchTimerRestart()
        //{
        //    watch.Stop();
        //    watch.Reset();
        //    watch.Start();
        //}

        //async private void TestListen()
        //{
        //    await Locator.StartListeningAsync(TimeSpan.FromSeconds(0.5), 1, true);
        //}        

        //private Task<bool> StartListenAsync(TimeSpan minimum,
        //    double minimumDistance, bool includedHeading = false,
        //    ListenerSettings listenerSettings = null)
        //{

        //}

        //public event EventHandler<PositionChangedEventArgs> positionChanged;
        //public event EventHandler<PositionErrorEventArgs> PostionError;
        #endregion
        #region
        //private void Button_Clicked(object sender, EventArgs e)
        //{
        //    GetLocation();
        //}

        //async private void GetLocation()
        //{
        //    try
        //    {
        //        var location = await Geolocation.GetLocationAsync();

        //        if (location != null)
        //        {
        //            Console.WriteLine("lat ={0}, lon={1}, alt={2}", location.Longitude, location.Latitude, location.Altitude);
        //        }
        //    }
        //    catch(Exception exc)
        //    {
        //        Console.WriteLine("Gps error - " + exc.Message);
        //    }
        //}

        #endregion
    }

    static class ImageService 
    {
        static readonly HttpClient _client = new HttpClient();

        public static bool CheckImageExist(string imageFileName)
        {
            return Preferences.ContainsKey(imageFileName);
        }

        public static Task<byte[]> DownloadImage(string imageUrl)
        {
            if(!imageUrl.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Android and iOS can't use http!");
            }
            return _client.GetByteArrayAsync(imageUrl);
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
            //return new ImageSource();
        }
        
        //public static string GetImageFromDisk(string imageFileName)
        //{
        //    return "";
        //}
    }
}