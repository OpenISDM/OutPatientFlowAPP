using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Rg.Plugins.Popup.Pages;
using IndoorNavigation.Utilities;

namespace IndoorNavigation.Views.PopUpPage
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ManualAdjustPopupPage : PopupPage
    {
        public ManualAdjustPopupPage()
        {
            InitializeComponent();

            //ManualSlider.Value = TmperorayStatus.RssiOption;
            ManualSlider.Value = TmperorayStatus.RssiOption;
        }

        private void StepperAddBtn_Clicked(object sender, EventArgs e)
        {
            if(TmperorayStatus.RssiOption+1 <= 15)
            {
                TmperorayStatus.RssiOption += 1;

                ManualSlider.Value = TmperorayStatus.RssiOption;
            }
        }

        private void StepperCutBtn_Clicked(object sender, EventArgs e)
        {
            if(TmperorayStatus.RssiOption -1 >= -15)
            {
                TmperorayStatus.RssiOption -= 1;
                ManualSlider.Value = TmperorayStatus.RssiOption;
            }
        }

        private void ConfirmBtn_Clicked(object sender, EventArgs e)
        {

        }

        private void ManualSlider_DragStarted(object sender, EventArgs e)
        {
            ManualSlider.Value = TmperorayStatus.RssiOption;
        }

        private void ManualSlider_DragCompleted(object sender, EventArgs e)
        {
            ManualSlider.Value = TmperorayStatus.RssiOption;
        }
    }
}