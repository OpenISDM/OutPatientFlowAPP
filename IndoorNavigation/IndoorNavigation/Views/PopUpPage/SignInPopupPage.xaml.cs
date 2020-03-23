using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Resources;
using System.Reflection;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;

using IndoorNavigation.Resources.Helpers;

using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Plugin.Multilingual;
namespace IndoorNavigation.Views.PopUpPage
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SignInPopupPage : PopupPage
    {
        #region Propertys
        private App app;
        private Page currentPage;
        private string _naviGraphName;
        #endregion
        public SignInPopupPage(string naviGraphName)
        {
            InitializeComponent();

            IDnumberEntry.Text = Preferences.Get("ID_NUMBER_STRING", string.Empty);
            app = (App)Application.Current;
            currentPage = Application.Current.MainPage;
            _naviGraphName = naviGraphName;
        }

        private int FirstCharacterNumber(char ch)
        {
            switch (ch)
            {
                case 'A': return 1;
                case 'P': return 29;
                case 'B': return 10;
                case 'Q': return 38;
                case 'C': return 19;
                case 'R': return 47;
                case 'D': return 28;
                case 'S': return 56;
                case 'E': return 37;
                case 'T': return 65;
                case 'F': return 46;
                case 'U': return 74;
                case 'G': return 55;
                case 'V': return 83;
                case 'H': return 64;
                case 'W': return 21;
                case 'I': return 39;
                case 'X': return 3;
                case 'J': return 73;
                case 'Y': return 12;
                case 'K': return 82;
                case 'Z': return 30;
                case 'L': return 2;
                case 'M': return 11;
                case 'N': return 20;
                case 'O': return 48;
                default: return 0;
            }
        }

        //it's for identity number check that the number is legal or not.
        private bool CheckIDLegal(string IDnum) 
        {
            if (IDnum.Length < 10)
                return false;

            IDnumberEntry.Text = IDnumberEntry.Text.ToUpper();

            int[] priority = { 1, 8, 7, 6, 5, 4, 3, 2, 1, 1 };
            int count = FirstCharacterNumber(IDnum[0]);

            for (int i = 1; i < IDnum.Length; i++)
            {
                int tmp = IDnum[i] - '0';
                if (!(tmp >= 0 && tmp <= 9))
                    return false;
                count += priority[i] * (tmp);
            }
            if (count % 10 == 0)
                return true;
            return false;
        }

        async private void CancelBtn_Clicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(app.IDnumber) || string.IsNullOrEmpty(IDnumberEntry.Text))
            {
                await PopupNavigation.Instance.PopAsync();
                await currentPage.Navigation.PopToRootAsync();
            }
            else
            {
                await PopupNavigation.Instance.PopAsync();
            }
        }

        async private void ConfirmBtn_Clicked(object sender, EventArgs e)
        {
            
            if(string.IsNullOrEmpty(IDnumberEntry.Text) || !CheckIDLegal(IDnumberEntry.Text))
            {
                await currentPage.DisplayAlert(
                    GetResourceString("ERROR_STRING"), 
                    GetResourceString("IDNUM_TYPE_WRONG_STRING"), 
                    GetResourceString("OK_STRING")
               );                  
                return; 
            }
            Preferences.Set("ID_NUMBER_STRING", IDnumberEntry.Text);
            app.IDnumber =
                IDnumberEntry.Text;

            app.RgDate =
                RegisterDatePicker.Date;

            app.isRigistered =
                false;

            await PopupNavigation.Instance.PopAsync();

            await PopupNavigation.Instance.PushAsync(new AskRegisterPopupPage(_naviGraphName));
            app.isRigistered = true;
        }

        protected override bool OnBackgroundClicked()
        {
            Console.WriteLine(">>SignInPopupPage : Back Button Click");
            Device.BeginInvokeOnMainThread(async () =>
            {
                if (string.IsNullOrEmpty(app.IDnumber) || string.IsNullOrEmpty(IDnumberEntry.Text))
                {
                    await PopupNavigation.Instance.PopAsync();
                    await currentPage.Navigation.PopToRootAsync();
                }
                else
                {
                    await PopupNavigation.Instance.PopAsync();
                }
            });
            //return false ;
            return base.OnBackgroundClicked();
        }

        private string GetResourceString(string key)
        {
            CultureInfo currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
            ResourceManager manager = new ResourceManager("IndoorNavigation.Resources.AppResources", typeof(TranslateExtension).GetTypeInfo().Assembly);

            return manager.GetString(key, currentLanguage);
        }

        protected override bool OnBackButtonPressed()
        {
            Console.WriteLine(">>SignInPopupPage : Back Button Click");
            Device.BeginInvokeOnMainThread(async () =>
            {
                if (string.IsNullOrEmpty(app.IDnumber) || string.IsNullOrEmpty(IDnumberEntry.Text))
                {
                    await PopupNavigation.Instance.PopAsync();
                    await currentPage.Navigation.PopToRootAsync();
                }
                else
                {
                    await PopupNavigation.Instance.PopAsync();
                }
            });

            return true;
        }
    }
}