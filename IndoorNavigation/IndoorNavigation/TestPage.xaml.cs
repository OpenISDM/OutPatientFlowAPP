using IndoorNavigation.Resources.Helpers;
using Plugin.Multilingual;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TestPage : ContentPage
    {
        App app = (App)Application.Current;
        const string _resourceId = "IndoorNavigation.Resources.AppResources";
        ResourceManager _resourceManager =
            new ResourceManager(_resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);
        int i;
        DestinationItem item1;
        public TestPage(DestinationItem item, int index)
        {
            InitializeComponent();
            i = index;
            TestString.Text = item._waypointName;
            item1 = item;
        }
        public TestPage()
        {
            InitializeComponent();
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            if(PopupNavigation.Instance.PopupStack.Count>0)
                PopupNavigation.Instance.PopAllAsync();
        }
        async private void ArrivalBtn_Clicked(object sender, EventArgs e)
        {
            var currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
            TestString.Text = item1._waypointName;
            if (item1.Key.Equals("exit"))
            {
                string s = string.Format("{0}\n{1}", _resourceManager.GetString("THANK_COMING_STRING", currentLanguage),
                    _resourceManager.GetString("HOPE_STRING", currentLanguage));
                await DisplayAlert(_resourceManager.GetString("MESSAGE_STRING"),s, _resourceManager.GetString("OK_STRING",currentLanguage));
                   System.Environment.Exit(0);
            }
            else if (item1.Key.Equals("register"))
            {
                bool isFinished=await DisplayAlert("message", "Have you finished register?", "Yes", "No");

                if (isFinished)
                {
                    await Navigation.PopAsync();
                }
                else
                {
                    //to reset navigator page items, let user re-navigate.
                }
            }
            else
            {
                // app.records[i].isAccept = true;
                RgRecord record = app.records[i];
                if (app.records[i].Key.Equals("QueryResult"))
                {
                    app.roundRecord = record;
                }
                app.records[i].isComplete = true;
                await Navigation.PopAsync();
            }
        }

    }
}