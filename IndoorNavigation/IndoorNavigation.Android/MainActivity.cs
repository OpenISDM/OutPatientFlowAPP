/*
 * 2020 © Copyright (c) BiDaE Technology Inc. 
 * Provided under BiDaE SHAREWARE LICENSE-1.0 in the LICENSE.
 *
 * Project Name:
 *
 *      IndoorNavigation
 *
 * Version:
 *
 *      1.0.0, 20200221
 * 
 * File Name:
 *
 *      MainActivity.cs
 *
 * Abstract:
 *      
 *
 *      
 * Authors:
 * 
 *      Jason Chang, jasonchang@bidae.tech 
 *      
 */
using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Views;
using Plugin.Permissions;
using System;
using System.Threading;
using Plugin.CurrentActivity;
using AndroidX.AppCompat.App;

namespace IndoorNavigation.Droid
{
    [Activity(Label = "帶路機", Icon = "@mipmap/icon", Theme = "@style/splashscreen", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        internal static MainActivity Instance { get; private set; }

        App xamarinApp;

        protected override void OnCreate(Bundle bundle)
        {
            Instance = this;
            //show splash screen
            //base.Window.RequestFeature(WindowFeatures.ActionBar);
            //Thread.Sleep(600);

            base.SetTheme(Resource.Style.MainTheme);
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) != Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new String[] {
                    Manifest.Permission.AccessCoarseLocation,
                    Manifest.Permission.AccessFineLocation,
                    Manifest.Permission.WriteExternalStorage
                }, 0);
            }

            Plugin.InputKit.Platforms.Droid.Config.Init(this, bundle);
            Xamarin.Forms.Forms.SetFlags("FastRenderers_Experimental");
            Xamarin.Essentials.Platform.Init(this, bundle);
            Rg.Plugins.Popup.Popup.Init(this, bundle);

            CrossCurrentActivity.Current.Init(this, bundle);
            var a = new AiForms.Renderers.Droid.PickerCellRenderer();


            global::Xamarin.Forms.Forms.Init(this, bundle);

            //to disable Android Dark mode.
            //AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightNo;

            xamarinApp = new App();
            LoadApplication(xamarinApp);

            //Window.SetStatusBarColor(Android.Graphics.Color.Argb(255, 0, 160, 204));

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

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnDestroy()
        {
            xamarinApp.OnStop();
            base.OnDestroy();
        }
    }
}

