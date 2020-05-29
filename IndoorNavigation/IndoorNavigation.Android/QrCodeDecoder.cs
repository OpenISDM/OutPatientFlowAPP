///*
// * Copyright (c) 2019 Academia Sinica, Institude of Information Science
// *
// * License:
// *      GPL 3.0 : The content of this file is subject to the terms and
// *      conditions defined in file 'COPYING.txt', which is part of this source
// *      code package.
// *
// * Project Name:
// *
// *      IndoorNavigation
// *
// * File Description:
// *
// *      This file contains all the interfaces required by the application,
// *      such as the interface of IPSClient and the interface for the 
// *      Android project to allow the Xamarin.Forms app to access the APIs. 
// *      
// * Version:
// *
// *      1.0.0, 20190822
// * 
// * File Name:
// *
// *      QrCodeDecoder.cs
// *
// * Abstract:
// *
// *      Waypoint-based navigator is a mobile Bluetooth navigation application
// *      that runs on smart phones. It is structed to support anywhere 
// *      navigation indoors in areas covered by different indoor positioning 
// *      system (IPS) and outdoors covered by GPS.In particilar, it can rely on
// *      BeDIS (Building/environment Data and Information System) for indoor 
// *      positioning. This IPS provides a location beacon at every waypoint. The 
// *      beacon brocasts its own coordinates; Consequesntly, the navigator does 
// *      not need to continuously monitor its own position.
// *      This version makes use of Xamarin.Forms, which is a cross-platform UI 
// *      tookit that runs on both iOS and Android.
// *
// * Authors:
// *
// *      Wang-Chi Ho, h.wangchi.0970@gmail.com
// *
// */
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.OS;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using IndoorNavigation.Droid;
//using IndoorNavigation.Models;
//using ZXing.Mobile;

//[assembly: Xamarin.Forms.Dependency(typeof(QrCodeDecoder))]
//namespace IndoorNavigation.Droid
//{
//    public class QrCodeDecoder : IQrCodeDecoder
//    {
//        public async Task<string> ScanAsync()
//        {
//            MobileBarcodeScanner scanner = new MobileBarcodeScanner();

//            MobileBarcodeScanningOptions scanOptions =
//                new MobileBarcodeScanningOptions
//                {
//                    PossibleFormats = new List<ZXing.BarcodeFormat>()
//                    {
//                        ZXing.BarcodeFormat.QR_CODE,
//                        ZXing.BarcodeFormat.CODE_39
//                    }
//                };

//            ZXing.Result scanResults = await scanner.Scan(scanOptions);

//            if (scanResults != null)
//                return scanResults.Text;
//            else
//                return string.Empty;
//        }
//    }
//}