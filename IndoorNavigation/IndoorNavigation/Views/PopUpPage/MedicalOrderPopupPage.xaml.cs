using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Rg.Plugins.Popup.Pages;
namespace IndoorNavigation.Views.PopUpPage
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MedicalOrderPopupPage : PopupPage
    {

        public MedicalOrderPopupPage()
        {
            InitializeComponent();
        }

        private void MedicalOderConfirmBtn_Clicked(object sender, EventArgs e)
        {

        }

        private void LoadData()
        {

        }

        private void GenerateCheckBox()
        {

        }

        private void bindingRevisitBox_Tapped(object sender, EventArgs e)
        {
            RevisitCheckBox.IsChecked = !RevisitCheckBox.IsChecked;
        }
    }
}