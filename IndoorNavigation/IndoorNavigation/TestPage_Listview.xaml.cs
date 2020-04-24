using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;

using System;
using System.Resources;
using System.IO;

using Newtonsoft.Json;
using IndoorNavigation.Modules.Utilities;
namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TestPage_Listview : ContentPage
    {
        ResourceManager manager;
        
        public TestPage_Listview()
        {
            InitializeComponent();           
           
        }

        async private void Button_Clicked(object sender, System.EventArgs e)
        {
            string TestJsonData = "{'Maps':[{'Name':'Lab' , 'Version':'1.000'}], 'Languages':[{'Name':'en-US'}] }";

            Console.WriteLine(TestJsonData);

            CurrentMapInfos infos = JsonConvert.DeserializeObject<CurrentMapInfos>(TestJsonData);

            Console.WriteLine("Maps : " + infos.Maps[0].Name + ", " + infos.Maps[0].Version);
            Console.WriteLine("Languages : " + infos.Languages[0].Name);
        }
    }
}