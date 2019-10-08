using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Resources;
using IndoorNavigation.Resources.Helpers;
using System.Reflection;
using IndoorNavigation.Models.NavigaionLayer;
using Plugin.Multilingual;
using IndoorNavigation.Modules.Utilities;
using Xamarin.Forms;
using System.Linq;
using Plugin.InputKit.Shared.Abstraction;

namespace IndoorNavigation
{   
    /*the class haven't completed, I need some time to try how SelectionView function work/*/
    class AddPopupPageViewModel:INotifyPropertyChanged
    {
        public IList<DestinationItem> AddItems { get; set; }
        //public DestinationItem _selectItem;

        public IList<DestinationItem> _preSelectItems;

        public ICommand ButtonCommand { private set; get; }
        const string _resourceId = "IndoorNavigation.Resources.AppResources";
        ResourceManager _resourceManager =new ResourceManager(_resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);

        private XMLInformation _nameInformation;
        private string navigationGraphName { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string propName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        public AddPopupPageViewModel()
        {
            AddItems = new ObservableCollection<DestinationItem>();
            _preSelectItems = new ObservableCollection<DestinationItem>();
            navigationGraphName = "實驗室";

            if (CrossMultilingual.Current.CurrentCultureInfo.ToString() == "en" || CrossMultilingual.Current.CurrentCultureInfo.ToString() == "en-US")
            {
                _nameInformation = NavigraphStorage.LoadInformationML(navigationGraphName + "_info_en-US.xml");
            }
            else if (CrossMultilingual.Current.CurrentCultureInfo.ToString() == "zh" || CrossMultilingual.Current.CurrentCultureInfo.ToString() == "zh-TW")
            {
                _nameInformation = NavigraphStorage.LoadInformationML(navigationGraphName + "_info_zh.xml");
            }


            ButtonCommand = new Command(ToAddItems);
            LoadData();
        }

        private void LoadData()
        {
            AddItems.Add(new RgRecord
            {
                _regionID = new Guid("11111111-1111-1111-1111-111111111111"),
                _waypointID = new Guid("00000000-0000-0000-0000-000000000002"),
                _waypointName = "內視鏡",
                DptName = "內視鏡",
                Key = "AddItem"
            });
            AddItems.Add(new RgRecord
            {
                _regionID = new Guid("11111111-1111-1111-1111-111111111111"),
                _waypointID = new Guid("00000000-0000-0000-0000-000000000002"),
                _waypointName = "X光",
                DptName = "X光",
                Key = "AddItem"
            });
            AddItems.Add(new RgRecord
            {
                _regionID = new Guid("11111111-1111-1111-1111-111111111111"),
                _waypointID = new Guid("00000000-0000-0000-0000-000000000002"),
                _waypointName = "超音波",
                DptName = "超音波",
                Key = "AddITem"
            });
            AddItems.Add(new RgRecord
            {
                _regionID = new Guid("11111111-1111-1111-1111-111111111111"),
                _waypointID = new Guid("00000000-0000-0000-0000-000000000002"),
                _waypointName = "抽血處",
                DptName = "抽血處",
                Key = "AddItem"
            });
            AddItems.Add(new RgRecord
            {
                _regionID = new Guid("11111111-1111-1111-1111-111111111111"),
                _waypointID = new Guid("00000000-0000-0000-0000-000000000002"),
                DptName = "檢查室",
                _waypointName = "檢察室",
                Key = "AddItem"
            });
        }

       public IList<DestinationItem> PreSelectItems
        {
            get { return AddItems.Where(w => (w is ISelection) && (w as ISelection).IsSelected)?.ToList(); }
            set {
                foreach (var item in AddItems)
                {
                    if(item is ISelection)
                    {
                        var o = item as ISelection;
                        var b = value.Contains(item);
                        o.IsSelected = b;
                    }
                }
            }
        }

        private void ToAddItems()
        {
          

        }

    }
}
