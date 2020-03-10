using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using IndoorNavigation.Views.Controls;
namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TestPage_Listview : ContentPage
    {
        ObservableCollection<RgRecord> records = new ObservableCollection<RgRecord>();
        public TestPage_Listview()
        {
            InitializeComponent();
            LoadData();

            TestListView.ItemsSource = records;
        }

        private void LoadData()
        {
            records.Add(new RgRecord { type = RecordType.Queryresult, DptName = "1" });
            records.Add(new RgRecord { type = RecordType.AddItem, DptName = "2" });
            records.Add(new RgRecord { type = RecordType.Queryresult, DptName = "3" });
            records.Add(new RgRecord { type = RecordType.Queryresult, DptName = "4" });
            records.Add(new RgRecord { type = RecordType.Invalid, DptName = "5" });
        }

        private void TestListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {

        }
    }
}