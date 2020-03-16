using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using IndoorNavigation.Views.Controls;
using Plugin.InputKit.Shared.Controls;
using Rg.Plugins.Popup.Services;
using IndoorNavigation.Views;

namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TestPage_Listview : ContentPage
    {
        public TestPage_Listview()
        {
            InitializeComponent();
            Random rand = new Random();
            int i, j;
            //for(i=0;i<TestGrid.RowDefinitions.Count-1;i++)
            //{
            //    for(j=0;j<TestGrid.ColumnDefinitions.Count-1;j++)
            //    {
            //        int r = rand.Next(0, 255);
            //        int g = rand.Next(0, 255);
            //        int blue = rand.Next(0, 255);
            //        BoxView view = new BoxView { Color = Color.FromRgb(r, g, blue) };
            //        TestGrid.Children.Add(view, j , i);
            //    }
            //}
        }

        private void FinishButton_Clicked(object sender, EventArgs e)
        {

        }
        //ObservableCollection<RgRecord> records = new ObservableCollection<RgRecord>();
        //public TestPage_Listview()
        //{
        //    InitializeComponent();
        //    LoadData();

        //    //  TestListView.ItemsSource = records;
        //    SetRadioButton();
        //    //Group.Children.Add(new RadioButton("1", false));
        //    Group.Children.Add(new RadioButton { Text = "3" });
        //}
        //RadioButtonGroupView group;
        //private void SetRadioButton()
        //{
        //    group = new RadioButtonGroupView
        //    {
        //        Children =
        //        {
        //            new RadioButton{Text="1" , TextFontSize=26, Color= Color.Cyan, CircleColor=Color.Aqua, TextColor=Color.Green},
        //            new RadioButton{Text="2"},
        //            new RadioButton{Text="3"}
        //        }
        //    };
        //    DisplayTest.Children.Add(group);


        //    group.Children.Add(new RadioButton { Text = "4" });
        //    //group.Children.Add(new RadioButton("5"));
        //    //group.SelectedItemChanged += clicked;
        //}

        //private void clicked(object sender, EventArgs args)
        //{
        //    var o = sender as RadioButton;
        //   // Console.WriteLine("o 's text" + o.Text);
        //}

        //private void LoadData()
        //{
        //    records.Add(new RgRecord { type = RecordType.Queryresult, DptName = "1" });
        //    records.Add(new RgRecord { type = RecordType.AddItem, DptName = "2" });
        //    records.Add(new RgRecord { type = RecordType.Queryresult, DptName = "3" });
        //    records.Add(new RgRecord { type = RecordType.Queryresult, DptName = "4" });
        //    records.Add(new RgRecord { type = RecordType.Invalid, DptName = "5" });
        //}

        //private void TestListView_ItemTapped(object sender, ItemTappedEventArgs e)
        //{

        //}

        //async private void event_Clicked(object sender, EventArgs e)
        //{
        //    Console.WriteLine(">>event_clicked");
        //    //foreach(RadioButton radioButton in group.Children)
        //    //{
        //    //    Console.WriteLine("radio button name" + radioButton.Text + "  isChecked: " + radioButton.IsChecked);
        //    //}
        //    await PopupNavigation.Instance.PushAsync(new ExitPopupPage("員林基督教醫院"));
        //}
    }
}