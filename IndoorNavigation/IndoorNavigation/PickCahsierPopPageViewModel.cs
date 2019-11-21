//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Collections.ObjectModel;
//using System.ComponentModel;
//using System.Runtime.CompilerServices;
//using Plugin.Multilingual;
//using IndoorNavigation.Modules.Utilities;
//using IndoorNavigation.Models.NavigaionLayer;
//using System.Windows.Input;
//using Xamarin.Forms;
//using Rg.Plugins.Popup.Services;
//using System.Resources;
//using IndoorNavigation.Resources.Helpers;
//using System.Reflection;

//namespace IndoorNavigation
//{
//    class PickCahsierPopPageViewModel: INotifyPropertyChanged
//    {
//        public event PropertyChangedEventHandler PropertyChanged;
//        public void onPropertyChanged([CallerMemberName]string callerName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(callerName));
//        public IList<DestinationItem> CashierItem;
//        private string navigationGraphName;
//        public Command ButtonCommand;
//        private App app = (App)Application.Current;

//        const string _resourceId = "IndoorNavigation.Resources.AppResources";
//        ResourceManager _resourceManager =
//            new ResourceManager(_resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);

//        private bool ButtonLock=false;
//        public PickCahsierPopPageViewModel()
//        {
//            CashierItem = new ObservableCollection<DestinationItem>();
//            navigationGraphName = "員林基督教醫院";
//            if (CrossMultilingual.Current.CurrentCultureInfo.ToString() == "en" || CrossMultilingual.Current.CurrentCultureInfo.ToString() == "en-US")
//            {
//                _nameInformation = NavigraphStorage.LoadInformationML(navigationGraphName + "_info_en-US.xml");
//            }
//            else if (CrossMultilingual.Current.CurrentCultureInfo.ToString() == "zh" || CrossMultilingual.Current.CurrentCultureInfo.ToString() == "zh-TW")
//            {
//                _nameInformation = NavigraphStorage.LoadInformationML(navigationGraphName + "_info_zh.xml");
//            }
//            LoadBoxed();
//            ButtonCommand = new Command(AddCashierCommand);
            
//        }

//        async public void AddCashierCommand()
//        {
//            if (ButtonLock) return;
//            ButtonLock = true;

//            Page page = Application.Current.MainPage;
//            if (SelectItem != null)
//            {
//                await page.Navigation.PushAsync(new NavigationPage());
//                app.records.Insert(app.FinishCount, new RgRecord
//                {
//                    _waypointName=SelectItem._waypointName,
//                    Key= "Cashier",
//                    _regionID=SelectItem._regionID,
//                    _waypointID=SelectItem._waypointID,
//                    isComplete=true,
//                    DptName=SelectItem._waypointName
//                });
//                await PopupNavigation.Instance.PopAsync();
//            }
//            else { return; }

//        }

//        private void LoadBoxed()
//        {
//            CashierItem.Add(new DestinationItem
//            {
//                _waypointName = "前門出口",
//                _waypointID = new Guid("00000000-0000-0000-0000-000000000002"),
//                _regionID = new Guid("11111111-1111-1111-1111-111111111111"),
//                Key = "exit"
//            });

//            CashierItem.Add(new DestinationItem
//            {
//                _waypointName = "停車場",
//                _waypointID = new Guid("00000000-0000-0000-0000-000000000002"),
//                _regionID = new Guid("11111111-1111-1111-1111-111111111111"),
//                Key = "exit"
//            });

//            CashierItem.Add(new DestinationItem
//            {
//                _waypointName = "側門出口",
//                _waypointID = new Guid("00000000-0000-0000-0000-000000000002"),
//                _regionID = new Guid("11111111-1111-1111-1111-111111111111"),
//                Key = ""
//            });
//        }

//        private DestinationItem _selected;
//        private XMLInformation _nameInformation;

//        public DestinationItem SelectItem
//        {
//            get => _selected;
//            set
//            {
//                _selected = value;
//                onPropertyChanged();
//            }
//        }


//    }
//}
