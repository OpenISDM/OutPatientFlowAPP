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
using RadioButton = Plugin.InputKit.Shared.Controls.RadioButton;
namespace IndoorNavigation.Views.PopUpPage

{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SelectPurposePopupPage : PopupPage
    {
        string _naviGraphName;
        const string _documentPath = "Yuanlin_OPFM.PurposeOptions.xml";
        private bool isButtonClick = false;
        Dictionary<string, PurposeOption> options;
        public SelectPurposePopupPage(string naviGraphName)
        {
            InitializeComponent();

            _naviGraphName = naviGraphName;
            options = new Dictionary<string, PurposeOption>();

            LoadPurposeOption();
            SetRadioButton();
        }

        //parsing Purpose options to data structure.
        private void LoadPurposeOption()
        {
            XmlDocument doc = XmlReader(_documentPath);

            //XmlNodeList PurposeNodeList = doc.GetElementsByTagName("Purpose");
            XmlNodeList PurposeNodeList = doc.SelectNodes("Purposes/Purpose");
            Console.WriteLine("purpose node list child count : " + PurposeNodeList.Count);
            foreach(XmlNode purposeNode in PurposeNodeList)
            {
                PurposeOption option = new PurposeOption();

                option.OptionName = 
                    purposeNode.Attributes["access_name"].Value;
                option.id =
                    int.Parse(purposeNode.Attributes["id"].Value);
                //this part I would like to consider add it or not.
                if (purposeNode.HasChildNodes)
                {
                    foreach(XmlNode purposeChild in purposeNode.ChildNodes)
                    {

                    }
                }

                options.Add(option.OptionName, option);
            }            
        }

        //generate radio button and add to view.
        private void SetRadioButton()
        {
            foreach (KeyValuePair<string, PurposeOption> pair in options)
            {
                Console.WriteLine("AAAAAAAAAAAAAAAAAAAAAAAAAAA");
                
                RadioButton raido = new RadioButton
                {
                    Text = pair.Key,
                    TextFontSize = 28,
                    CircleColor = Color.FromRgb(63, 81, 181),
                    HorizontalOptions = LayoutOptions.StartAndExpand,
                    VerticalOptions = LayoutOptions.StartAndExpand
                };
                PurposeRadioGroup.Children.Add(raido);
            }
        }

        async private void SelectPurposeBtn_Clicked(object sender, EventArgs e)
        {
            //to check which radio button was selected.

            if (isButtonClick) return;

            isButtonClick = true;

            foreach(RadioButton optionButton in PurposeRadioGroup.Children)
            {
                if (optionButton.IsChecked)
                {
                    //add Content to it.

                    await PopupNavigation.Instance.RemovePageAsync(this);
                    return;
                }
            }                             

            //show please select one.

            isButtonClick = false;
        }

        protected override bool OnBackButtonPressed()
        {
            // return true, if you don't want the page to pop when user click
            // Android back button.
            return true;
        }

        protected override bool OnBackgroundClicked()
        {
            // return false, if you don't want the page to pop when user 
            // click background.
            return false;
        }
    }
    public class PurposeOption
    {
        public string OptionName { get; set; }
        public List<RgRecord> OptionContent { get; set; }
        public int id { get; set; }

    }

}