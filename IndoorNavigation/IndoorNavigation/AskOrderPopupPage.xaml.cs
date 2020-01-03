using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Rg.Plugins.Popup.Pages;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using IndoorNavigation.Views.Navigation;
using Rg.Plugins.Popup.Services;
namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AskOrderPopupPage : PopupPage
    {
        string _locationName;
        public AskOrderPopupPage(string locationName)
        {
            InitializeComponent();
            BackgroundColor = Color.FromRgba(150, 150, 150, 70);
            ObservableCollection<RgRecord> colleciton = ((App)Application.Current)._TmpRecords;
            QueryResultListview.ItemsSource = colleciton;
            _locationName = locationName;
            SuggestLabel.Text= $"建議先看{colleciton[0].DptName} {colleciton[0].DrName}醫師";
            TodayYouHaveRgLabel.Text = $"您今日掛號以下{colleciton.Count.ToString()}個門診";
        }

        private void AskOrderConfirmBtn_Clicked(object sender, EventArgs e)
        {
            //await Navigation.PushAsync(new NavigatorPage());
            PopupNavigation.Instance.PopAllAsync();
        }

        private void AskOrderCancelBtn_Clicked(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PopAllAsync();
        }
    }
}