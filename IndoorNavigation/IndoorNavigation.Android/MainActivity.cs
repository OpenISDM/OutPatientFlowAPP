using Android;
using Android.App;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Views;
using Plugin.Permissions;
using System;
using System.Threading;

namespace IndoorNavigation.Droid
{
    // Android Dark mode theme need to be test.
    // it require android 10 or up version.

    [Activity(Label = "帶路機_developerVer", 
	 Icon = "@mipmap/icon",
     Theme = "@style/splashscreen",
     MainLauncher = true, 
	 ConfigurationChanges=ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode,
     ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android
								.FormsAppCompatActivity
    {
        internal static MainActivity Instance { get; private set; }

        protected override void OnCreate(Bundle bundle)
        {
            Instance = this;
            //show splash screen
            base.Window.RequestFeature(WindowFeatures.ActionBar);
            Thread.Sleep(600);

            base.SetTheme(Resource.Style.MainTheme);
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);
           
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation)!= Permission.Granted ||  
                ContextCompat.CheckSelfPermission(this,Manifest.Permission.WriteExternalStorage)!= Permission.Granted )
                /*|| ContextCompat.CheckSelfPermission(this, Manifest.Permission.Camera) != Permission.Granted)*/
            {
                ActivityCompat.RequestPermissions
					(this, new String[] 
						  { 
							 Manifest.Permission.AccessCoarseLocation, 
							 Manifest.Permission.AccessFineLocation,
                             Manifest.Permission.WriteExternalStorage,
                             //Manifest.Permission.Camera
						  }, 0);
            }
   
            Plugin.InputKit.Platforms.Droid.Config.Init(this, bundle);
            Xamarin.Forms.Forms.SetFlags("FastRenderers_Experimental");
            Rg.Plugins.Popup.Popup.Init(this, bundle);
            global::Xamarin.Forms.Forms.Init(this, bundle);
            Xamarin.Essentials.Platform.Init(this, bundle);
            var a = new AiForms.Renderers.Droid.PickerCellRenderer();

            //ZXing.Net.Mobile.Forms.Android.Platform.Init();
            //ZXing.Mobile.MobileBarcodeScanner.Initialize(this.Application);
            LoadApplication(new App());
            Window.SetStatusBarColor(Android.Graphics.Color.Argb(255, 
																   0, 
																 160, 
																 204));

            //Finish();
        }
        public override void OnBackPressed()
        {
            if (Rg.Plugins.Popup.Popup.SendBackPressed(base.OnBackPressed))
            {
                // Rg.Plugins.Popup.Popup.SendBackPressed();
            }
            else
            {
                // base.OnBackPressed();
            }
            
        }     
        public override void OnRequestPermissionsResult(int requestCode,
                                                        string[] permissions,
                                                        Permission[] grantResults)
        {
            //global::ZXing.Net.Mobile.Android.PermissionsHandler
            //.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            PermissionsImplementation.Current
            .OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode,
                                            permissions,
                                            grantResults);
        }
        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            if((newConfig.UiMode & UiMode.NightNo) != 0)
            {
                //changed to light theme
            }
            else
            {
                // changed to dark theme
            }
        }
    }
}

