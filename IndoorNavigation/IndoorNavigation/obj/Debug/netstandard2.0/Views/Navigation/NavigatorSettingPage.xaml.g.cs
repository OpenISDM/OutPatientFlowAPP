//------------------------------------------------------------------------------
// <auto-generated>
//     這段程式碼是由工具產生的。
//     執行階段版本:4.0.30319.42000
//
//     對這個檔案所做的變更可能會造成錯誤的行為，而且如果重新產生程式碼，
//     變更將會遺失。
// </auto-generated>
//------------------------------------------------------------------------------

[assembly: global::Xamarin.Forms.Xaml.XamlResourceIdAttribute("IndoorNavigation.Views.Navigation.NavigatorSettingPage.xaml", "Views/Navigation/NavigatorSettingPage.xaml", typeof(global::IndoorNavigation.Views.Navigation.NavigatorSettingPage))]

namespace IndoorNavigation.Views.Navigation {
    
    
    [global::Xamarin.Forms.Xaml.XamlFilePathAttribute("Views\\Navigation\\NavigatorSettingPage.xaml")]
    public partial class NavigatorSettingPage : global::Xamarin.Forms.ContentPage {
        
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Xamarin.Forms.Build.Tasks.XamlG", "0.0.0.0")]
        private global::AiForms.Renderers.SettingsView NavigationSettingsView;
        
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Xamarin.Forms.Build.Tasks.XamlG", "0.0.0.0")]
        private global::AiForms.Renderers.SwitchCell AvoidStair;
        
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Xamarin.Forms.Build.Tasks.XamlG", "0.0.0.0")]
        private global::AiForms.Renderers.SwitchCell AvoidElevator;
        
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Xamarin.Forms.Build.Tasks.XamlG", "0.0.0.0")]
        private global::AiForms.Renderers.SwitchCell AvoidEscalator;
        
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Xamarin.Forms.Build.Tasks.XamlG", "0.0.0.0")]
        private global::AiForms.Renderers.TextPickerCell OptionPicker;
        
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Xamarin.Forms.Build.Tasks.XamlG", "0.0.0.0")]
        private void InitializeComponent() {
            global::Xamarin.Forms.Xaml.Extensions.LoadFromXaml(this, typeof(NavigatorSettingPage));
            NavigationSettingsView = global::Xamarin.Forms.NameScopeExtensions.FindByName<global::AiForms.Renderers.SettingsView>(this, "NavigationSettingsView");
            AvoidStair = global::Xamarin.Forms.NameScopeExtensions.FindByName<global::AiForms.Renderers.SwitchCell>(this, "AvoidStair");
            AvoidElevator = global::Xamarin.Forms.NameScopeExtensions.FindByName<global::AiForms.Renderers.SwitchCell>(this, "AvoidElevator");
            AvoidEscalator = global::Xamarin.Forms.NameScopeExtensions.FindByName<global::AiForms.Renderers.SwitchCell>(this, "AvoidEscalator");
            OptionPicker = global::Xamarin.Forms.NameScopeExtensions.FindByName<global::AiForms.Renderers.TextPickerCell>(this, "OptionPicker");
        }
    }
}
