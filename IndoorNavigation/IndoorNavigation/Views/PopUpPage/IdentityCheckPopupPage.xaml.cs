using System;
using System.Threading.Tasks;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Extensions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Rg.Plugins.Popup.Services;
using IndoorNavigation.Views.PopUpPage;
using static IndoorNavigation.Utilities.Constants;
using static IndoorNavigation.Utilities.Storage;
namespace IndoorNavigation.Views.PopUpPage
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class IdentityCheckPopupPage : PopupPage
    {
        TaskCompletionSource<bool> _tcs = null;
        private bool isButtonClick = false;

        public IdentityCheckPopupPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        async private void CancelBtn_Clicked(object sender, EventArgs e)
        {
            if (isButtonClick) return;
            isButtonClick = true;

            _tcs?.SetResult(false);
            await PopupNavigation.Instance.RemovePageAsync(this);
        }

        async private void ConfirmBtn_Clicked(object sender, EventArgs e)
        {
            if (isButtonClick) return;
            isButtonClick = true;


            if (EntryText == DEVELOPER_PASSWORD)
            {
                AlertDialogPopupPage checkedPage = 
                    new AlertDialogPopupPage(
                        GetResourceString("LOGIN_SUCCESSFULLY_STRING"), 
                        GetResourceString("OK_STRING")
                        );
                await checkedPage.show();
                _tcs.SetResult(true);
                await PopupNavigation.Instance.RemovePageAsync(this);
                return;
            }

            AlertDialogPopupPage alertPopupPage = 
                new AlertDialogPopupPage(
                    GetResourceString("UNAVAILABLE_PASSWORD_STRING"), 
                    GetResourceString("OK_STRING")
                    );

            await alertPopupPage.show();
            isButtonClick = false;
        }

        public async Task<bool> showPopupPage()
        {
            _tcs = new TaskCompletionSource<bool>();
            await Navigation.PushPopupAsync(this);
            return await _tcs.Task;
        }

        private string _entryText = "";
        public string EntryText
        {
            get => _entryText;
            set
            {
                _entryText = value;
                ConfirmBtn.IsEnabled = _entryText.Length != 0;            }
        }
    }
}