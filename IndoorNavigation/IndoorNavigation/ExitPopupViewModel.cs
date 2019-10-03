using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using MvvmHelpers;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Xamarin.Forms;
using Rg.Plugins.Popup;
using Rg.Plugins.Popup.Services;
namespace IndoorNavigation
{
    class ExitPopupViewModel : INotifyPropertyChanged
    {
        public IList<DestinationItem> exits {get;set;}
        public DestinationItem _selectItem;
        public ICommand ButtonCommand { private set; get; }

        async void ToNavigatorExit()
        {
            Page nowPage = Application.Current.MainPage;
            await nowPage.Navigation.PushAsync(new TestPage(SelectItem, 0));
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
        /*    exits.Add(new DestinationItem
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
            });*/
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string propName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
