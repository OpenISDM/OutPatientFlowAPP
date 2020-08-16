using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Rg.Plugins.Popup.Services;
using Rg.Plugins.Popup.Pages;

namespace IndoorNavigation.Views.PopUpPage
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SelectPurposePopupPage : PopupPage
    {
        public SelectPurposePopupPage()
        {
            InitializeComponent();
        }

        private void SelectPurposeBtn_Clicked(object sender, EventArgs e)
        {

        }
    }
}