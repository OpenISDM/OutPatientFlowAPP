using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Resources;
using IndoorNavigation.Resources.Helpers;
using System.Reflection;
using IndoorNavigation.Views.Navigation;
using Plugin.Multilingual;
using IndoorNavigation.Models.NavigaionLayer;
using IndoorNavigation.Modules.Utilities;
using Rg.Plugins.Popup;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using IndoorNavigation.ViewModels;
using System.ComponentModel;
using System.Windows.Input;
using System.Runtime.CompilerServices;

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
        private string navigationGraphName { get; set; }
        async void ToNavigatorExit()
        {
            Page nowPage = Application.Current.MainPage;
            if (SelectItem != null)
            {
                if (isButtonPressed) return;

                isButtonPressed = true;
                //XMLInformation _nameInformation = NavigraphStorage.LoadInformationML("Lab" + "_info_zh.xml");
                var o = SelectItem as DestinationItem;
                //  await nowPage.DisplayAlert("test", string.Format("waypoint id={0}\n,regionid={1}\n,name={2}\n",o._waypointID.ToString(),o._regionID.ToString(),o._waypointName), "OK");
                // await nowPage.Navigation.PushAsync(new TestPage(SelectItem, 0));
                await nowPage.Navigation.PushAsync(new NavigatorPage(navigationGraphName, o._regionID, o._waypointID, o._waypointName, _nameInformation));
                App app = (App)Application.Current;
                app.records.Insert(app.FinishCount,new RgRecord
                {
                    _waypointName = o._waypointName,
                    Key = "exit",
                    _regionID = o._regionID,
                    _waypointID = o._waypointID,
                    isComplete=true,
                   DptName=o._waypointName
                });
                await PopupNavigation.Instance.PopAllAsync();
            }
            else
            {
                var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
                await nowPage.DisplayAlert(_resourceManager.GetString("MESSAGE_STRING", currentLanguage), _resourceManager.GetString("PICK_EXIT_STRING", currentLanguage), _resourceManager.GetString("OK_STRING", currentLanguage));
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
        public ExitPopupViewModel()
        {
            exits = new ObservableCollection<DestinationItem>();
            navigationGraphName = "員林基督教醫院";

            if (CrossMultilingual.Current.CurrentCultureInfo.ToString() == "en" || CrossMultilingual.Current.CurrentCultureInfo.ToString() == "en-US")
            {
                _nameInformation = NavigraphStorage.LoadInformationML(navigationGraphName + "_info_en-US.xml");
            }
            else if (CrossMultilingual.Current.CurrentCultureInfo.ToString() == "zh" || CrossMultilingual.Current.CurrentCultureInfo.ToString() == "zh-TW")
            {
                _nameInformation = NavigraphStorage.LoadInformationML(navigationGraphName + "_info_zh.xml");
            }


            ButtonCommand = new Command(ToNavigatorExit);
            LoadData();
        }



        private void LoadData()  //fake data 
        {

            exits.Add(new DestinationItem
            {
                _waypointName = "前門出口",
                _waypointID = new Guid("00000000-0000-0000-0000-000000000002"),
                _regionID = new Guid("11111111-1111-1111-1111-111111111111"),
                Key = "exit"
            });

            exits.Add(new DestinationItem
            {
                _waypointName = "停車場",
                _waypointID = new Guid("00000000-0000-0000-0000-000000000002"),
                _regionID = new Guid("11111111-1111-1111-1111-111111111111"),
                Key = "exit"
            });

           exits.Add(new DestinationItem
            {
                _waypointName = "側門出口",
               _waypointID = new Guid("00000000-0000-0000-0000-000000000002"),
               _regionID = new Guid("11111111-1111-1111-1111-111111111111"),
               Key = "exit"
            });
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string propName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
