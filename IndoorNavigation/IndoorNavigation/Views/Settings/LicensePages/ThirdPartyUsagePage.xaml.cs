using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using static IndoorNavigation.Utilities.Storage;
namespace IndoorNavigation.Views.Settings.LicensePages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ThirdPartyUsagePage : ContentPage
    {

        private List<ThirdParty> _thirdParties;

        public ThirdPartyUsagePage()
        {
            InitializeComponent();

            LoadData();
            ThirdPartyListView.ItemsSource = _thirdParties;
        }

        public void LoadData()
        {
            _thirdParties = new List<ThirdParty>();

            XmlDocument doc = XmlReader("Resources.ThirdPartyList.xml");

            XmlNodeList thirdPartyList = doc.SelectNodes("ThirdPartis/ThirdParty");

            foreach(XmlNode thirdPartyNode in thirdPartyList)
            {
                ThirdParty thirdParty = new ThirdParty();

                thirdParty._license = 
                    thirdPartyNode.Attributes["License"].Value;

                thirdParty._authorName = 
                    thirdPartyNode.Attributes["authorName"].Value;

                thirdParty._packageName =
                    thirdPartyNode.Attributes["packageName"].Value;

                thirdParty._combineString =
                    string.Format("Author: {0}, License: {1}",
                        thirdParty._authorName, thirdParty._license);

                _thirdParties.Add(thirdParty);
            }
        }
    }    

    public class ThirdParty
    {
        public string _license;
        public string _packageName;
        public string _authorName;
        public string _combineString;
    }
}