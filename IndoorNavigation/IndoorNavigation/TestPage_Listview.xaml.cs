using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using System;
using System.Xml;

using static IndoorNavigation.Utilities.Storage;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http.Headers;
using IndoorNavigation.Utilities;
using Dijkstra.NET.Model;
using IndoorNavigation.Modules.Utilities;

namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TestPage_Listview : ContentPage
    {
        CloudDownload _download;
        public TestPage_Listview()
        {
            InitializeComponent();

            //_download = new CloudDownload();
            //Storage.CloudGenerateFile("Lab");
            //Storage.CloudGenerateFile("Taipei_City_Hall");
        }

        #region Object to Xml implemenet
        //public TestPage_Listview()
        //{
        //    InitializeComponent();

        //    XElement root = new XElement("GraphResource");
        //    XElement GraphsElement = new XElement("Graphs");

        //    foreach(KeyValuePair<string, GraphInfo> pair in _resources._graphResources)
        //    {                
        //        XElement displayNameElement= new XElement("DisplayNames");
        //        foreach(KeyValuePair<string,string> displaynamePair in pair.Value._displayNames)
        //        {
        //            //displayNameElement.Add("DisplayName",new XAttribute("name", displayname.Value), new XAttribute("language", displayname.Key));                     
        //            displayNameElement.Add(new XElement("DisplayName", new XAttribute("name", displaynamePair.Value), new XAttribute("language", displaynamePair.Key)));
        //        }
        //        GraphsElement.Add(new XElement("Graph",new XAttribute("name",pair.Key),new XAttribute("version",pair.Value._localVersion),displayNameElement));
        //    }

        //    XElement LanguageElement = new XElement("Languages");

        //    foreach(string language in _resources._languages)
        //    {
        //        LanguageElement.Add(new XElement("Langugae", new XAttribute("name", language)));
        //    }
        //    root.Add(GraphsElement, LanguageElement);
        //    Console.WriteLine(root.ToString());

        //} 
        #endregion
        #region Xml to Resource implement
        //public TestPage_Listview()
        //{
        //    InitializeComponent();
        //    _resources = new GraphResources();

        //    XmlDocument doc =  XmlReader("Resources.GraphResource.xml");

        //    XmlNodeList GraphsList = doc.SelectNodes("GraphResource/Graphs/Graph");
        //    XmlNodeList LanguageList = doc.SelectNodes("GraphResource/Languages/Language");

        //    foreach (XmlNode GraphNode in GraphsList)
        //    {
        //        Console.WriteLine(GraphNode.OuterXml);
        //        Console.WriteLine(">>GraphNode : " + GraphNode.Attributes["name"].Value);
        //        string GraphName = GraphNode.Attributes["name"].Value;
        //        GraphInfo info = new GraphInfo();

        //        XmlNodeList DisplayNameList = GraphNode.SelectNodes("DisplayNames/DisplayName");
        //        Console.WriteLine(">>GraphNode : " + GraphNode.Attributes["name"].Value);
        //        foreach (XmlNode displayName in DisplayNameList)
        //        {
        //            Console.WriteLine(">>displayName, name : " + displayName.Attributes["name"].Value);
        //            Console.WriteLine(">>displayName, version : " + displayName.Attributes["language"].Value);

        //            info._displayNames.Add(displayName.Attributes["language"].Value, displayName.Attributes["name"].Value);
        //            //Console.WriteLine("dddddddd");
        //        }
        //        Console.WriteLine("ddddddd");
        //        //Console.WriteLine(GraphNode.OuterXml);
        //        _resources._graphResources.Add(GraphName, info);
        //        Console.WriteLine("<<Next GraphNode");
        //    }
        //    Console.WriteLine("Next to parse Language");
        //    foreach (XmlNode languageNode in LanguageList)
        //    {
        //        Console.WriteLine(">> languagesNode name : " + languageNode.OuterXml);
        //        _resources._languages.Add(languageNode.Attributes["name"].Value);
        //    }

        //}
        #endregion
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