using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Rg.Plugins.Popup.Pages;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AskOrderPopupPage : PopupPage
    {
        public AskOrderPopupPage()
        {
            InitializeComponent();
            BackgroundColor = Color.FromRgba(150, 150, 150, 70);
            ObservableCollection<RgRecord> colleciton = ((App)Application.Current)._TmpRecords;
            QueryResultListview.ItemsSource = colleciton;
        }

        private void AskOrderConfirmBtn_Clicked(object sender, EventArgs e)
        {

        }

        private void AskOrderCancelBtn_Clicked(object sender, EventArgs e)
        {

        }
    }
}