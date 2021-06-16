using System;
using MvvmHelpers;
using static IndoorNavigation.Utilities.Storage;
using IndoorNavigation.Views.PopUpPage;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace IndoorNavigation.ViewModels
{
    public class DownloadGraphPopupPageViewModel : BaseViewModel
    {
        public DownloadGraphPopupPageViewModel()
        {
            _downloadEvent._eventHandler += new EventHandler(DownloadEventResult);
        }

        async private void DownloadEventResult(object sender, EventArgs e)
        {
            var args = e as DownloadEventArgs;

            ProgressBarValue = (double)(args._finishCount / args._totalCount);
            ProgressBarText = string.Format("{0}/{1}", args._finishCount, args._totalCount);

            if (args._finishCount == args._totalCount)
            {
                await PopupNavigation.Instance.PopAsync();
                Device.BeginInvokeOnMainThread(async()=>
                    await PopupNavigation.Instance.PushAsync(
                        new AlertDialogPopupPage(
                            GetResourceString("DOWNLOAD_COMPLETELTY_STRING"),
                            GetResourceString("OK_STRING")
                            )
                    ));
                _downloadEvent._eventHandler -= new EventHandler(DownloadEventResult);
            }
        }
        #region Binding Context variables.
        private double _progressBarValue;
        public double ProgressBarValue
        {
            get => _progressBarValue;
            set => SetProperty(ref _progressBarValue, value);
        }

        private string _progressBarText;
        public string ProgressBarText
        {
            get => _progressBarText;
            set => SetProperty(ref _progressBarText, value);
        }
        #endregion
    }
}
