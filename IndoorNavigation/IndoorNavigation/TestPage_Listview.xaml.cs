using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;

using System;
using System.Resources;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

using Newtonsoft.Json;
using IndoorNavigation.Modules.Utilities;
namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TestPage_Listview : ContentPage
    {
        ResourceManager manager;

        CloudDownload _cloudDownload;
        Thread ResourceThread;

        public TestPage_Listview()
        {
            InitializeComponent();                      
        }

        //private void Thread_Running()
        //{
        //    _cloudDownload = new CloudDownload();
        //}

        private void Button_Clicked(object sender, System.EventArgs e)
        {
            _cloudDownload = new CloudDownload();

            _cloudDownload.Download(_cloudDownload.getMainUrl("Lab"));
        }
    }
}