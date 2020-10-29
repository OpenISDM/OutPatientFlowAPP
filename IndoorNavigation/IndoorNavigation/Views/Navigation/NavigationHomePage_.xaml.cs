using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IndoorNavigation.Utilities.Storage;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using IndoorNavigation.ViewModels;
using IndoorNavigation.Models.NavigaionLayer;

namespace IndoorNavigation.Views.Navigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NavigationHomePage_ : ContentPage
    {

        private string _navigationGraphName;
        private NavigationGraph _navigationGraph;
        public NavigationHomePage_(Location location, NavigationGraph navigationGraph)
        {
            InitializeComponent();

            _navigationGraphName = location.sourcePath;
            _navigationGraph = navigationGraph;

        }         

        async void InfoButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync
                (new NavigatorSettingPage(_navigationGraphName));
        }       
        #region Button click event
        async private void ExitBtn_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DestinationPickPage(_navigationGraphName, CategoryType.Exit));
        }

        async private void BathroomBtn_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DestinationPickPage(_navigationGraphName, CategoryType.Bathroom));
        }

        async private void ConvenienceStoreBtn_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DestinationPickPage(_navigationGraphName, CategoryType.ConvenienceStore));
        }

        async private void BloodCollectionBtn_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DestinationPickPage(_navigationGraphName, CategoryType.BloodCollectionCounter));
        }

        async private void ExaminationRoomBtn_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DestinationPickPage(_navigationGraphName, CategoryType.ExaminationRoom));
        }

        async private void ClinicsBtn_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DestinationPickPage(_navigationGraphName, CategoryType.Clinics));
        }

        async private void CashierBtn_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DestinationPickPage(_navigationGraphName, CategoryType.Cashier));
        }
        #endregion

        async private void RadiologyBtn_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DestinationPickPage(_navigationGraphName, CategoryType.Radiology));
        }

        async private void CommunityHealthBtn_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DestinationPickPage(_navigationGraphName, CategoryType.CommunityHealth));
        }
    }
}