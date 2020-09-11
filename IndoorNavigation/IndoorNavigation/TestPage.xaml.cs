using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using IndoorNavigation.Modules;
using IndoorNavigation.Modules.IPSClients;
using IndoorNavigation.Models;
using Xamarin.Essentials;

namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TestPage : ContentPage
    {
        WaypointClient waypointClient;
        public TestPage()
        {
            InitializeComponent();

          
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            GetLocation();
        }

        async private void GetLocation()
        {
            try
            {
                var location = await Geolocation.GetLocationAsync();

                if (location != null)
                {
                    Console.WriteLine("lat ={0}, lon={1}, alt={2}", location.Longitude, location.Latitude, location.Altitude);
                }
            }
            catch(Exception exc)
            {
                Console.WriteLine("Gps error - " + exc.Message);
            }
        }
    }
}