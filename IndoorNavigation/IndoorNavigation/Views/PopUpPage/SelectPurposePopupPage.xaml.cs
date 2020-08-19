using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Rg.Plugins.Popup.Services;
using Rg.Plugins.Popup.Pages;
using System.Xml;
using static IndoorNavigation.Utilities.Storage;

namespace IndoorNavigation.Views.PopUpPage
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SelectPurposePopupPage : PopupPage
    {
        string _naviGraphName;
        const string _documentPath = "Yuanlin_OPFM.PurposeOptions";
        List<PurposeOptions> options;
        public SelectPurposePopupPage(string naviGraphName)
        {
            InitializeComponent();

            _naviGraphName = naviGraphName;
            options = new List<PurposeOptions>();
        }

        public void LoadPurposeOption()
        {
            XmlDocument doc = XmlReader(_documentPath);
            
            //parsing Purpose options to data structure.

            //generate radio button and add to view.
        }

        private void SelectPurposeBtn_Clicked(object sender, EventArgs e)
        {
            //to check which radio button was selected.
        }

    }
    public class PurposeOptions 
    {

    }

}