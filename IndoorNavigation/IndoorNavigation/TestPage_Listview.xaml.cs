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
using Rg.Plugins.Popup.Services;
using Newtonsoft.Json;
using IndoorNavigation.Modules.Utilities;
using IndoorNavigation.Views.PopUpPage;
using IndoorNavigation.Utilities;
using System.Collections.Generic;

namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TestPage_Listview : ContentPage
    {
        public TestPage_Listview()
        {
            InitializeComponent();

           // string context = NaviGraphStroage_.EmbeddedSourceReader("Utilities.a.json");

           //// var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, FileResource>>(context);
           // Console.WriteLine(">>parse well done");
           // foreach(KeyValuePair<string, FileResource> resource in jsonObject)
           // {
           //     Console.WriteLine("Key = " + resource.Key);
           //     //Console.WriteLine("Value = " + resource.Value);
           //     Console.WriteLine("Value version = " + resource.Value._version);
           //     Console.WriteLine("Value naviGarphSinkRoute = " + resource.Value._naviGraphSinkRoute);
           //     Console.WriteLine("Value FD SinkRoute = " + resource.Value._infoDataSinkRoute.Values);
           //     Console.WriteLine("Value Info SinkRoute = " + resource.Value._infoDataSinkRoute.Keys);
           // }
        }

        #region Cloud Test
        //CloudDownload _cloudDownload;
        //Thread ResourceThread;
        //bool isDownloading = true;

        //public TestPage_Listview()
        //{
        //    InitializeComponent();

        //    ResourceThread = new Thread(() => ConstructObject());
        //}       

        //private void ConstructObject()
        //{
        //    _cloudDownload = new CloudDownload();

        //    PopupNavigation.Instance.PopAllAsync();
        //}

        //private void DownloadFiles()
        //{
        //    _cloudDownload.GenerateFilePath("NTUH Yunlin Branch", "NTUH_Yunlin");
        //    PopupNavigation.Instance.PopAllAsync();
        //}

        //private void Button_Clicked(object sender, System.EventArgs e)
        //{
        //    PopupNavigation.Instance.PushAsync(new IndicatorPopupPage());
        //    ResourceThread = new Thread(() => ConstructObject());
        //    ResourceThread.Start();

        //}

        //private void Button_Clicked_1(object sender, EventArgs e)
        //{
        //    PopupNavigation.Instance.PushAsync(new IndicatorPopupPage());
        //    ResourceThread = new Thread(() => DownloadFiles());
        //    ResourceThread.Start();
        //}

        //private void Button_Clicked_2(object sender, EventArgs e)
        //{
        //    _cloudDownload.DeleteFile("NTUH Yunlin Branch");
        //}

        //private void Button_Clicked_3(object sender, EventArgs e)
        //{

        //    _cloudDownload.GenerateFilePath("Yuanlin Christian Hospital", "Yuanlin_Christian_Hospital");
        //}

        //private void Button_Clicked_4(object sender, EventArgs e)
        //{
        //    _cloudDownload.GenerateFilePath("Lab","Lab");
        //}
        #endregion
    }
}