using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static IndoorNavigation.Utilities.Storage;

namespace IndoorNavigation.Views.Navigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditLocationPage : ContentPage
    {
        List<AddNewSiteItem> listviewItemSources;

        public EditLocationPage()
        {
            InitializeComponent();
            LoadData();
        }


        private void LoadData()
        {
            listviewItemSources = new List<AddNewSiteItem>();

            
            listviewItemSources.Add(new AddNewSiteItem
            {
                isSelected = false,
                _graphName="aaaa",
                _currentVersion=11,
                _displayNames= new Dictionary<string, string>()
                {
                    {"en-US", "aaaa" },
                    {"zh-TW", "欸欸欸欸" }
                }
                
            }); ;

            listviewItemSources.Add(new AddNewSiteItem
            {
                isSelected = false,
                _graphName="bbbb",
                _currentVersion=22,
                _displayNames = new Dictionary<string, string>()
                {
                    {"en-US", "BBBB" },
                    {"zh-TW","逼逼逼逼"  }
                }
            }) ;

            AddNewSiteListView.ItemsSource = listviewItemSources;
        }
        private void AddNewSite_TextChanged(object sender, TextChangedEventArgs e)
        {
            //AddNewSiteListView.BeginRefresh();
            if (string.IsNullOrEmpty(e.NewTextValue))
            {
                //AddNewSiteListView.ItemsSource = listviewItemSources;
                RefreshListView();
            }
            else
            {
                var SearchBarSelect = listviewItemSources.Select(p => p._displayNme.Contains(e.NewTextValue));
                AddNewSiteListView.ItemsSource = null;
                AddNewSiteListView.ItemsSource = SearchBarSelect;
            }
            //AddNewSiteListView.EndRefresh();
        }                
        
        public class AddNewSiteItem : GraphInfo
        {
            public bool isSelected { get; set; }
            public string _displayNme
            {
                get { return _displayNames[_currentCulture.Name]; }
                private set { }
            }
        }

        private void AddNewSiteListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            AddNewSiteListView.SelectedItem = null;
        }

        private void AddNewSiteListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var selectedItem = e.Item as AddNewSiteItem;

            selectedItem.isSelected = !selectedItem.isSelected;

            RefreshListView();
        }

        private void RefreshListView()
        {
            AddNewSiteListView.ItemsSource = null;
            AddNewSiteListView.ItemsSource = listviewItemSources;
        }
    }
}