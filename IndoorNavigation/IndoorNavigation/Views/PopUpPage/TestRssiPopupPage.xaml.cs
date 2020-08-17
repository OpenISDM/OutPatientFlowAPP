using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace IndoorNavigation.Views.PopUpPage
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TestRssiPopupPage : PopupPage
    {
        public TestRssiPopupPage()
        {
            InitializeComponent();

            if (Application.Current.Properties
                .ContainsKey("RSSI_Test_Adjustment"))
            {
                Console.WriteLine("current value = " +
                    Application.Current.Properties["RSSI_Test_Adjustment"]);

                RSSIAdjustmentSlider.Value =
                  Convert.ToDouble(Application.Current.Properties
                  ["RSSI_Test_Adjustment"]);
            }
        }

        async private void ConfirmBtn_Clicked(object sender, EventArgs e)
        {
            Console.WriteLine("current slider value : " +
                RSSIAdjustmentSlider.Value);

            Application.Current.Properties["RSSI_Test_Adjustment"]
                = (int)(RSSIAdjustmentSlider.Value);

            await PopupNavigation.Instance.PopAllAsync();
        }

        async private void CancelBtn_Clicked(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PopAllAsync();
        }
    }
} 