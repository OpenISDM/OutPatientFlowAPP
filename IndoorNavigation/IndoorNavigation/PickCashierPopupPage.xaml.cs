using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rg.Plugins.Popup.Pages;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using IndoorNavigation.Models;
using Rg.Plugins.Popup.Services;
using System.Collections.ObjectModel;
namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PickCashierPopupPage : PopupPage
    {
        ObservableCollection<DestinationItem> items;
        SelectionView sv;
        //PickCahsierPopPageViewModel _viewmodel;
        public PickCashierPopupPage()
        {
            InitializeComponent();
            items = new ObservableCollection<DestinationItem>();

            LoadData();

            sv = new SelectionView
            { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center, ItemsSource=items,
              ColumnNumber=1, SelectionType=SelectionType.RadioButton, RowSpacing=7
            };
            
            SelectionStack.Children.Add(sv);
        }

        public void LoadData()
        {
            items.Add(new DestinationItem
            {
                _waypointName = "前門出口",
                _waypointID = new Guid("00000000-0000-0000-0000-000000000002"),
                _regionID = new Guid("11111111-1111-1111-1111-111111111111"),
                Key = "exit"
            });

            items.Add(new DestinationItem
            {
                _waypointName = "b門出口",
                _waypointID = new Guid("00000000-0000-0000-0000-000000000002"),
                _regionID = new Guid("11111111-1111-1111-1111-111111111111"),
                Key = "exit"
            });

            items.Add(new DestinationItem
            {
                _waypointName = "a門出口",
                _waypointID = new Guid("00000000-0000-0000-0000-000000000002"),
                _regionID = new Guid("11111111-1111-1111-1111-111111111111"),
                Key = "exit"
            });
        }

        async private void CashierOKBtn_Clicked(object sender, EventArgs e)
        {
            var o = sv.SelectedItem as DestinationItem;
            if (o != null)
            {
                ((App)Application.Current).records.Add(new RgRecord
                {
                    _waypointID=o._waypointID,
                    Key="exit",
                    _regionID=o._regionID,
                    _waypointName=o._waypointName,
                    DptName=o._waypointName
                });
                await DisplayAlert("bb", sv.SelectedItem.ToString(), "ok");
                //await PopupNavigation.Instance.PopAsync();

            }
            return;
        }

        private void CashierCancelBtn_Clicked(object sender, EventArgs e)
        {

        }
    }
}