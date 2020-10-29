using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Xamarin.Forms;
using IndoorNavigation.Models;
using IndoorNavigation.Droid;

using Android.Bluetooth;
using Android.Locations;

[assembly: Xamarin.Forms.Dependency(typeof(CheckLocationEnable))]
namespace IndoorNavigation.Droid
{
    public class CheckLocationEnable : ICheckLocationEnable
    {
        public bool IsBluetoothEnable()
        {
            BluetoothManager btMananger = (BluetoothManager)
                Android.App.Application.Context.GetSystemService("bluetooth");
            return btMananger.Adapter.IsEnabled;
            //throw new NotImplementedException();
        }

        public bool IsGPSEnable()
        {
            throw new NotImplementedException();
        }

        public void OpenBluetoothSetting()
        {
            Intent BTIntent = new Intent(Android.Provider.Settings.ActionBluetoothSettings);

            var chooseIntent = Intent.CreateChooser(BTIntent, "Go to Bluetooth setting");
            chooseIntent.SetFlags(ActivityFlags.ClearWhenTaskReset | ActivityFlags.NewTask);

            Android.App.Application.Context.StartActivity(chooseIntent);
            //throw new NotImplementedException();
        }

        public void OpenLocationSetting()
        {
            Intent locationIntent = new Intent(Android.Provider.Settings.ActionLocationSourceSettings);
            var chooseIntent = Intent.CreateChooser(locationIntent, "Go to location setting");

            chooseIntent.SetFlags(ActivityFlags.ClearWhenTaskReset | ActivityFlags.NewTask);

            Android.App.Application.Context.StartActivity(chooseIntent);
            //throw new NotImplementedException();
        }
    }
}