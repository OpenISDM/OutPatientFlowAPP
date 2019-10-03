using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SignInPage : ContentPage
    {
        
        
        public SignInPage()
        {
            InitializeComponent();

            IDNumEntry.Text = Preferences.Get("ID_NUMBER_STRING",string.Empty);
            PatientIDEntry.Text = Preferences.Get("PATIENT_ID_STRING",string.Empty);
            BirthDayPicker.Date = Preferences.Get("BIRTHDAY_DATETIME", DateTime.Now);
        }

        async private void Button_Clicked(object sender, EventArgs e)
        {
            //QueryResult result = new QueryResult();
            Preferences.Set("ID_NUMBER_STRING",IDNumEntry.Text);
            Preferences.Set("PATIENT_ID_STRING",PatientIDEntry.Text);
            Preferences.Set("BIRTHDAY_DATETIME",BirthDayPicker.Date);
            await Navigation.PopAsync();
        }

        async private void ConnectServer()
        {
            //await DisplayAlert("訊息","")
        }
    }
}