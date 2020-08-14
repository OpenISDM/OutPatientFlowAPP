using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IndoorNavigation.Droid
{
    [Activity(Theme = "@android:style/Theme.NoTitleBar", MainLauncher = true, 
        Immersive = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class splashscreenActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.splashscreen);

        }


        protected override void OnResume()
        {
            base.OnResume();
            Task startupWork = new Task(() => { SimulateStartUp(); });
            startupWork.Start();
        }

        public override void OnBackPressed()
        {            
        }

        async void SimulateStartUp()
        {
            await Task.Delay(3000);
            StartActivity(new Intent(Application.Context, 
                typeof(MainActivity)));
        }
    }
}