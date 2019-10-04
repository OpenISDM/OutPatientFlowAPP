using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Xamarin.Forms;
using Plugin.Multilingual;
using System.Resources;
using IndoorNavigation.Resources.Helpers;
using System.Reflection;
using IndoorNavigation.Views;
using IndoorNavigation.Views.Navigation;
using IndoorNavigation.Modules.Utilities;
using IndoorNavigation.Models.NavigaionLayer;

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


        async void ToNavigatorExit()
        {
            Page nowPage = Application.Current.MainPage;
            if (SelectItem != null)
            {
                XMLInformation _nameInformation = NavigraphStorage.LoadInformationML("Lab" + "_info_zh.xml");
                var o = SelectItem as DestinationItem;
                // await nowPage.Navigation.PushAsync(new TestPage(SelectItem, 0));
                await nowPage.Navigation.PushAsync(new NavigatorPage("Lab", o._regionID, o._waypointID, o._waypointName, _nameInformation, "exit", 0));
            }
            else
            {
                var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
                await nowPage.DisplayAlert(_resourceManager.GetString("MESSAGE_STRING", currentLanguage), _resourceManager.GetString("PICK_EXIT_STRING", currentLanguage), _resourceManager.GetString("OK_STRING", currentLanguage));
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
            ButtonCommand = new Command(ToNavigatorExit);
            LoadData();
        }



        private void LoadData()
        {

            exits.Add(new DestinationItem
            {
                _waypointName = "前門出口",
                Key = "exit"
            });

            exits.Add(new DestinationItem
            {
                _waypointName = "停車場",
                Key = "exit"
            });

           exits.Add(new DestinationItem
            {
                _waypointName = "側門出口",
                Key = "exit"
            });
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string propName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
