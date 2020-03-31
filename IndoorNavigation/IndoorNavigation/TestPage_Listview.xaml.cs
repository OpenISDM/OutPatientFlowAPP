using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Xamarin.Essentials;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;

namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TestPage_Listview : ContentPage
    {       
        public TestPage_Listview()
        {
            InitializeComponent();
            
        }

        async private void Button_Clicked(object sender, System.EventArgs e)
        {
            //var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Camera);

            Console.WriteLine("status is : " + status.ToString() + "   " );
        }
    }
}