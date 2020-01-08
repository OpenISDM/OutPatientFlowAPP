using System;
using System.Collections.Generic;
using System.Text;

namespace IndoorNavigation
{
    public interface IWifiConnect
    {
        void ConnectToWifi(string ssid, string password);
        void DisconnectToWifi();

    }
}
