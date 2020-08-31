using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Rg.Plugins.Popup.Pages;
using IndoorNavigation.Utilities;
using Rg.Plugins.Popup.Services;
using Rg.Plugins.Popup.Extensions;

namespace IndoorNavigation.Views.PopUpPage
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ManualAdjustPopupPage : PopupPage
    {

        TaskCompletionSource<bool> _tcs = null;
        private bool isDoubleClick = false;
        public ManualAdjustPopupPage()
        {
            InitializeComponent();           
            ManualRssiLabel.Text = TmperorayStatus.RssiOption.ToString();
        }

        private void StepperAddBtn_Clicked(object sender, EventArgs e)
        {
            if(TmperorayStatus.RssiOption+1 <= 15)
            {
                TmperorayStatus.RssiOption += 1;

                ManualRssiLabel.Text =
                    //string.Format("靈敏度 : {0}", TmperorayStatus.RssiOption);
                    TmperorayStatus.RssiOption.ToString();
            }
        }

        private void StepperCutBtn_Clicked(object sender, EventArgs e)
        {
            if(TmperorayStatus.RssiOption -1 >= -15)
            {
                TmperorayStatus.RssiOption -= 1;
                ManualRssiLabel.Text =
                    //string.Format("靈敏度 : {0}", TmperorayStatus.RssiOption);
                    TmperorayStatus.RssiOption.ToString();
            }
        }

        async private void ConfirmBtn_Clicked(object sender, EventArgs e)
        {
            if (isDoubleClick) return;
            isDoubleClick = true;

            await PopupNavigation.Instance.RemovePageAsync(this);           
            _tcs?.SetResult(true);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _tcs?.SetResult(true);
        }


        async public Task<bool> show() 
        {
            _tcs = new TaskCompletionSource<bool>();

            await Navigation.PushPopupAsync(this);
            return await _tcs.Task;
        }
        
    }
}