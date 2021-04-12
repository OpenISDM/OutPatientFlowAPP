using System;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace IndoorNavigation.Views.Settings.LicensePages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class IconLicensePage : ContentPage
    {
        public IconLicensePage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        public ICommand HyperlinkClickCommand => new Command<string>(async (url) =>
        {
            await Launcher.TryOpenAsync(new Uri(url));
        });
    }
}