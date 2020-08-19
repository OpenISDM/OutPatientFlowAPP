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

namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TestPage : ContentPage
    {
        WaypointClient waypointClient;
        public TestPage()
        {
            InitializeComponent();

            waypointClient = new WaypointClient();


            waypointClient
                .SetWaypointList(new List<Models.WaypointBeaconsMapping>());
        }

        bool isStart = true;

        private void Button_Clicked(object sender, EventArgs e)
        {
            if (isStart)
            {
                waypointClient.Stop();
                isStart = false;
            }
            else
            {
                waypointClient.SetWaypointList(new List<WaypointBeaconsMapping>());
                isStart = true;
            }
        }
    }
}