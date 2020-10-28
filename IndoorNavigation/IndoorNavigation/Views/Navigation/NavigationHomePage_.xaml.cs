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

        private Grid GetGridLayout()
        {
            Grid grid = new Grid();

            grid.RowDefinitions.Add(new RowDefinition 
            { 
                Height = new GridLength(0.6, GridUnitType.Star) 
            });

            grid.RowDefinitions.Add(new RowDefinition 
            { 
                Height = new GridLength(0.4, GridUnitType.Star) 
            });

            for (int i = 0; i < 3; i++) 
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width= new GridLength(35, GridUnitType.Star)
                });
            }
            return grid;
        }
    }
}