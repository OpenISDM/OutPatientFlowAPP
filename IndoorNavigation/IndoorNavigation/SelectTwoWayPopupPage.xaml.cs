using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using IndoorNavigation.Views.Navigation;
using Xamarin.Essentials;
namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SelectTwoWayPopupPage : PopupPage
    {
        String _locationName;
        bool isButtonPressed = false;
        bool msg = false;
        bool getMsg = false;
        public SelectTwoWayPopupPage(string BuildingName)
        {
            InitializeComponent();
            BackgroundColor = Color.FromRgba(150, 150, 150, 70);
            //frame.BackgroundColor = Color.FromRgba(240, 240, 240, 200);
            _locationName = BuildingName;
        }

        async private void ToNavigationBtn_Clicked(object sender, EventArgs e)
        {
            if (isButtonPressed) return;
            isButtonPressed = true;
            //await Navigation.PushAsync(new RigisterList(_locationName));
            await Navigation.PushAsync(new NavigationHomePage(_locationName));
            await PopupNavigation.Instance.PopAllAsync();
        }

        async private void ToOPFM_Clicked(object sender, EventArgs e)
        {
            if (isButtonPressed) return;
            isButtonPressed = true;

            bool getNotShowAgain = Preferences.Get("NotShowAgain_ToOPFM", false);

            PopupNavigation.Instance.PopAsync();
            if (!getNotShowAgain)
            {
                PopupNavigation.Instance.PushAsync(new ShiftAlertPopupPage("這項服務將會需要使用網路服務，若您沒有行動網路或是WiFi連線，是否需要改用導航?", "是", "否", "NotShowAgain_ToOPFM"));
                MessagingCenter.Subscribe<ShiftAlertPopupPage, bool>(this, "NotShowAgain_ToOPFM",async (msgsender, msgargs) =>
                  {
                      msg = (bool)msgargs;
                      Console.WriteLine("get the msg!");
                      MessagingCenter.Unsubscribe<ShiftAlertPopupPage,bool>(this, "NotShowAgain_ToOPFM");
                      Page page = Application.Current.MainPage;
                      if (!msg) await page.Navigation.PushAsync(new RigisterList(_locationName));
                      else await page.Navigation.PushAsync(new NavigationHomePage(_locationName));
                  });
                //if (getMsg)
                //{
                //    if (msg) await Navigation.PushAsync(new RigisterList(_locationName));
                //    else await Navigation.PushAsync(new NavigationHomePage(_locationName));
                //}
            }
            else
            {
                await Navigation.PushAsync(new RigisterList(_locationName));
            }
            //PopupNavigation.Instance.PopAsync();
            //await Navigation.PushAsync(new RigisterList(_locationName));
            //await PopupNavigation.Instance.PopAllAsync();
        }
    }
}