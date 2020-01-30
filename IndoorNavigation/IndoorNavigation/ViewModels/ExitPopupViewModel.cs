using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using System.Resources;
using IndoorNavigation.Resources.Helpers;
using System.Reflection;
using IndoorNavigation.Views.Navigation;
using Plugin.Multilingual;
using IndoorNavigation.Models.NavigaionLayer;
using IndoorNavigation.Modules.Utilities;
using Rg.Plugins.Popup.Services;
using System.ComponentModel;
using System.Windows.Input;
using System.Runtime.CompilerServices;
using System.IO;
using System.Xml;

namespace IndoorNavigation
{
    class ExitPopupViewModel : INotifyPropertyChanged
    {
        public IList<DestinationItem> exits {get;set;}
        public DestinationItem _selectItem;
        public ICommand ButtonCommand { private set; get; }
        const string _resourceId = "IndoorNavigation.Resources.AppResources";
        ResourceManager _resourceManager =
            new ResourceManager(_resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);

        private bool isButtonPressed = false;
        private XMLInformation _nameInformation;
        private PhoneInformation phoneInformation;
        private string _navigationGraphName { get; set; }
        async void ToNavigatorExit()
        {
            Page nowPage = Application.Current.MainPage;
            if (SelectItem != null)
            {
                if (isButtonPressed) return;

                isButtonPressed = true;

                var o = SelectItem as DestinationItem;

                await nowPage.Navigation.PushAsync(new NavigatorPage(_navigationGraphName, o._floor, o._regionID, o._waypointID, o._waypointName, _nameInformation));
                App app = (App)Application.Current;
                app.records.Insert(app.FinishCount,new RgRecord
                {
                    _waypointName = o._waypointName,
                    Key = "exit",
                    _regionID = o._regionID,
                    _waypointID = o._waypointID,
                    _floor=o._floor,
                    isComplete=true,
                   DptName=o._waypointName
                });
                await PopupNavigation.Instance.PopAllAsync();
            }
            else
            {
                var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
                await nowPage.DisplayAlert(_resourceManager.GetString("MESSAGE_STRING", currentLanguage), _resourceManager.GetString("SELECT_EXIT_STRING", currentLanguage), _resourceManager.GetString("OK_STRING", currentLanguage));
                return;
            }
          //  await nowPage.PopupNavigation.Instance.PopAllAsync();
        }

        public DestinationItem SelectItem
        {
            get => _selectItem;
            set
            {
                _selectItem = value;
                OnPropertyChanged();
            }
        }
        public ExitPopupViewModel(string navigationGraphName)
        {
            exits = new ObservableCollection<DestinationItem>();
            _navigationGraphName = navigationGraphName;
            phoneInformation = new PhoneInformation();

            _nameInformation = NavigraphStorage.LoadInformationML(phoneInformation.GiveCurrentMapName(_navigationGraphName) + "_info_" + phoneInformation.GiveCurrentLanguage() + ".xml");

            ButtonCommand = new Command(ToNavigatorExit);
            LoadData();
        }


        private void LoadData()
        {
            string fileName = "Yuanlin_OPFM.ExitMap.xml";
            var assembly = typeof(ExitPopupViewModel).GetTypeInfo().Assembly;
            string content = "";
            Stream stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.{fileName}");

            using (var reader = new StreamReader(stream))
            {
                content = reader.ReadToEnd();        
            }
            stream.Close();

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(content);

            XmlNodeList exitNodes = doc.GetElementsByTagName("exit");

            foreach (XmlNode node in exitNodes)
            {
                DestinationItem item = new DestinationItem();
                item._regionID = new Guid(node.Attributes["region_id"].Value);
                item._waypointID = new Guid(node.Attributes["waypoint_id"].Value);
                item._waypointName = node.Attributes["name"].Value;
                item._floor = node.Attributes["floor"].Value;
                item.Key = "exit";

                exits.Add(item);
            }
            return;
        }       
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string propName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
