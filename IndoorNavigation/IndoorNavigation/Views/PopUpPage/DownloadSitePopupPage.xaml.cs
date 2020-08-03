using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Rg.Plugins.Popup.Pages;
using MvvmHelpers;
using System.Net;
using Rg.Plugins.Popup.Services;

namespace IndoorNavigation.Views.PopUpPage
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DownloadSitePopupPage : PopupPage
    {
        private double _downloadProgress;
        public DownloadSitePageViewModel _viewmodel;
        public DownloadSitePopupPage()
        {
            InitializeComponent();

            _viewmodel = new DownloadSitePageViewModel();

            BindingContext = _viewmodel;
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }

        async private void DownloadCancelButton_Clicked(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.RemovePageAsync(this);
        }
    }

    public class DownloadSitePageViewModel : BaseViewModel, IDisposable
    {
        private double _downloadProgress=0;
        public double DownloadProgress 
        {
            get { return _downloadProgress; }
            set
            {
                SetProperty(ref _downloadProgress, value);
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}