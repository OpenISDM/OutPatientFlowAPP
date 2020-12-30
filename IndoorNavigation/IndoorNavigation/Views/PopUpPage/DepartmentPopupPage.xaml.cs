using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Rg.Plugins.Popup.Pages;
using CheckBox = Plugin.InputKit.Shared.Controls.CheckBox;
namespace IndoorNavigation.Views.PopUpPage
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DepartmentPopupPage : PopupPage
    {

        public DepartmentPopupPage()
        {
            InitializeComponent();
        }

        private void LoadData()
        {

        }

        private void GenerateCheckBox() 
        {

        }

        async private void DepartmentConfirm_Clicked(object sender, 
            EventArgs args) 
        {
            await Task.CompletedTask;
        }
        
    }
}