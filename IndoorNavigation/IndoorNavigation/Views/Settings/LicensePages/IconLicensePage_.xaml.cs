using System;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace IndoorNavigation.Views.Settings.LicensePages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class IconLicensePage_ : ContentPage
    {
        public IconLicensePage_()
        {
            InitializeComponent();
            BindingContext = this;
        }
        
        public ICommand HyperlinkClickCommand => new Command<string>((url) =>
        {
            Device.OpenUri(new Uri(url));
        });
    }
}