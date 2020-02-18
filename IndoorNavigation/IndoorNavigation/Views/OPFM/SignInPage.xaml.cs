using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Reflection;
using Plugin.Multilingual;
using IndoorNavigation.Resources.Helpers;
using System.Resources;

namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SignInPage : ContentPage
    {
        private const string _resourceId = "IndoorNavigation.Resources.AppResources";
        ResourceManager _resourceManager = new ResourceManager(_resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);
        CultureInfo currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
        App app = (App)Application.Current;
        
        public SignInPage()
        {
            InitializeComponent();
            IDnumEntry.Keyboard = Keyboard.Create(KeyboardFlags.CapitalizeCharacter);
            RgDayPicker.Date = app.RgDate;
            IDnumEntry.Text = Preferences.Get("ID_NUMBER_STRING", string.Empty);
        }

        async private void Button_Clicked(object sender, EventArgs e)
        {
            IDnumEntry.Text = IDnumEntry.Text.ToUpper();
            if(IDnumEntry.Text==null || !CheckIDLegal(IDnumEntry.Text))
            {
                await DisplayAlert(_resourceManager.GetString("ERROR_STRING",currentLanguage), _resourceManager.GetString("IDNUM_TYPE_WRONG_STRING", currentLanguage)
                    ,_resourceManager.GetString("OK_STRING",currentLanguage));
                return;
            }            
            Preferences.Set("ID_NUMBER_STRING", IDnumEntry.Text);
            app.IDnumber = IDnumEntry.Text;
            app.RgDate = RgDayPicker.Date;
            app.isRigistered = false;
            await Navigation.PopAsync();
        }                

        //to check the ID number is a legal one or not.
        public bool CheckIDLegal(string IDnum)
        {
            if (IDnum.Length < 10)
                return false;
            int[] priority = { 1, 8, 7, 6, 5, 4, 3, 2, 1, 1 };
            int count = FirstCharacterNumber(IDnum[0]);

            for (int i = 1; i < IDnum.Length; i++)
            {
                int tmp = IDnum[i] - '0';
                if (!(tmp >= 0 && tmp <= 9))
                    return false;
                count += priority[i] * (tmp);
            }
            if(count%10==0)
                return true;
            return false;
        }
        //first char in identity number 
        private int FirstCharacterNumber(char ch)
        {
            switch (ch)
            {
                case 'A': return 1; case 'P': return 29; 
                case 'B': return 10; case 'Q': return 38; 
                case 'C': return 19; case 'R': return 47; 
                case 'D': return 28; case 'S': return 56;
                case 'E': return 37; case 'T': return 65; 
                case 'F': return 46; case 'U': return 74; 
                case 'G': return 55; case 'V': return 83; 
                case 'H': return 64; case 'W': return 21;
                case 'I': return 39; case 'X': return 3; 
                case 'J': return 73; case 'Y': return 12; 
                case 'K': return 82; case 'Z': return 30; 
                case 'L': return 2; 
                case 'M': return 11; 
                case 'N': return 20; 
                case 'O': return 48;
                default: return 0;
            }
        }

       
    }
}