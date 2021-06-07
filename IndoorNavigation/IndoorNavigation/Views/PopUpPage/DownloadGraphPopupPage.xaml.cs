using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rg.Plugins.Popup.Pages;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static IndoorNavigation.Utilities.Storage;
using IndoorNavigation.ViewModels;
namespace IndoorNavigation.Views.PopUpPage
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DownloadGraphPopupPage : PopupPage
    {
        private DownloadGraphPopupPageViewModel _viewModel;
        public DownloadGraphPopupPage()
        {
            InitializeComponent();
            _viewModel = new DownloadGraphPopupPageViewModel();
            BindingContext = _viewModel;
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }

        protected override bool OnBackgroundClicked()
        {
            return false;
        }
    }
}