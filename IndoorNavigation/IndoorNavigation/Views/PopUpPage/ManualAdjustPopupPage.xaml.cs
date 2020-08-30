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

namespace IndoorNavigation.Views.PopUpPage
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ManualAdjustPopupPage : PopupPage
    {
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

                ManualRssiLabel.Text = TmperorayStatus.RssiOption.ToString();
            }
        }

        private void StepperCutBtn_Clicked(object sender, EventArgs e)
        {
            if(TmperorayStatus.RssiOption -1 >= -15)
            {
                TmperorayStatus.RssiOption -= 1;
                ManualRssiLabel.Text = TmperorayStatus.RssiOption.ToString();
            }
        }

        async private void ConfirmBtn_Clicked(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.RemovePageAsync(this);
        }

      

        
    }
}