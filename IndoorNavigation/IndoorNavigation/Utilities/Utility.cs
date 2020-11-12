/*
 * 2020 © Copyright (c) BiDaE Technology Inc. 
 * Provided under BiDaE SHAREWARE LICENSE-1.0 in the LICENSE.
 *
 * Project Name:
 *
 *      IndoorNavigation
 *
 * File Description:
 * 
 *      This class contains all static functions that are not part of 
 *      Convert.cs and Storage.cs.
 *      
 * Version:
 *
 *      1.0.0-beta.1, 20190319
 * 
 * File Name:
 *
 *      Utility.cs
 *
 * Abstract:
 *
 *      Waypoint-based navigator is a mobile Bluetooth navigation application
 *      that runs on smart phones. It is structed to support anywhere 
 *      navigation. Indoors in areas covered by different indoor positioning 
 *      system (IPS) and outdoors covered by GPS. In particilar, it can rely on 
 *      BeDIS (Building/environment Data and Information System) for indoor 
 *      positioning. Using this IPS, the navigator does not need to 
 *      continuously monitor its own position, since the IPS broadcast to the 
 *      navigator the location of each waypoint. 
 *      This version makes use of Xamarin.Forms, which is a complete 
 *      cross-platform UI tookit that runs on both iOS and Android.
 *
 * Authors:
 *
 *      Kenneth Tang, kenneth@gm.nssh.ntpc.edu.tw
 *      
 */

using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using GeoCoordinatePortable;
using IndoorNavigation.Models;
using IndoorNavigation.Modules.Utilities;

namespace IndoorNavigation.Modules
{
    public static class Utility
    {
        public static IBeaconScan _ibeaconScan;
        public static LBeaconScan _lbeaconScan;
        public static ITextToSpeech _textToSpeech;

        ///// <summary>
        ///// Download navigation graph from specified server
        ///// </summary>
        ///// <param name="URL"></param>
        ///// <param name="navigraphName"></param>
        ///// <returns></returns>
        //public static bool DownloadNavigraph(string URL, 
        //string navigraphName)
        //{
        //    string filePath = Path.Combine(NavigraphStorage._navigraphFolder,
        //                                    navigraphName);
        //    try
        //    {
        //        if (!Directory.Exists(NavigraphStorage._navigraphFolder))
        //            Directory.CreateDirectory(
        //                NavigraphStorage._navigraphFolder);

        //        using (WebClient webClient = new WebClient())
        //            webClient.DownloadFileAsync(new Uri(URL), filePath);
        //        return true;
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e);
        //        return false;
        //    }
        //}
        //public static bool DownloadFirstDirectionFile
        //    (string URL, string fileName)
        //{
        //    string filePath = 
        //        Path.Combine(NavigraphStorage._firstDirectionInstuctionFolder, 
        //        fileName);
        //    try
        //    {
        //        if (!Directory.Exists(NavigraphStorage.
        //            _firstDirectionInstuctionFolder))
        //            Directory.CreateDirectory(
        //                NavigraphStorage._firstDirectionInstuctionFolder);

        //        using (WebClient webClient = new WebClient())
        //            webClient.DownloadFileAsync(new Uri(URL), filePath);
        //        return true;
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e);
        //        return false;
        //    }

        //}

        //public static bool DownloadInformationFile(string URL,
        //string fileName)
        //{
        //    string filePath = 
        //        Path.Combine(NavigraphStorage._informationFolder, fileName);
        //    try
        //    {
        //        if (!Directory.Exists(NavigraphStorage._informationFolder))
        //            Directory.CreateDirectory(
        //                NavigraphStorage._informationFolder);

        //        using (WebClient webClient = new WebClient())
        //            webClient.DownloadFileAsync(new Uri(URL), filePath);
        //        return true;
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e);
        //        return false;
        //    }

        //}

    }
}
