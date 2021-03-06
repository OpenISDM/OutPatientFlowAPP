﻿using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using System;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;

namespace IndoorNavigation.Droid
{
    [Activity(Theme = "@style/splashscreen", /*MainLauncher = true, */
     ScreenOrientation = ScreenOrientation.Portrait)]
    public class splashscreenActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.splashscreen);

            Intent intent = new Intent(this, typeof(MainActivity));
            StartActivity(intent);
            Finish();
        }     

        public override void OnBackPressed()
        {
        }
       
    }
}