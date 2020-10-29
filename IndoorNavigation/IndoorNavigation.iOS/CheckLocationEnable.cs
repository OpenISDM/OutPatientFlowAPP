using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

using Xamarin.Forms;
using IndoorNavigation.Models;
using IndoorNavigation.iOS;
using CoreBluetooth;
[assembly: Xamarin.Forms.Dependency(typeof(CheckLocationEnable))]
namespace IndoorNavigation.iOS
{
    public class CheckLocationEnable : ICheckLocationEnable
    {
        
        public bool IsBluetoothEnable()
        {
            var bluetoothManager = new CoreBluetooth.CBCentralManager();

            if (bluetoothManager.State == CBCentralManagerState.PoweredOn)
                return true;
            return false;
        }

        public bool IsGPSEnable()
        {            
            throw new NotImplementedException();
        }

        public void OpenBluetoothSetting()
        {
            NSUrl url = new NSUrl("App-Prefs:root=Bluetooth");
            
            if(UIApplication.SharedApplication.CanOpenUrl(url))
                UIApplication.SharedApplication.OpenUrl(url);           
        }

        public void OpenLocationSetting()
        {
            NSUrl url = new NSUrl("App-Prefs:root=LOCATION_SERVICES");

            if (UIApplication.SharedApplication.CanOpenUrl(url))
                UIApplication.SharedApplication.OpenUrl(url);
            //throw new NotImplementedException();
        }
    }
}