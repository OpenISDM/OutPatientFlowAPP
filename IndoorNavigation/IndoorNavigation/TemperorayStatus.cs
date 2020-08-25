using System;
using System.Collections.Generic;
using System.Text;
using Plugin.Settings;
using Plugin.Settings.Abstractions;
using Xamarin.Forms;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace IndoorNavigation.Utilities
{    
    public static class TmperorayStatus
    {
        private static ISettings AppSettings => CrossSettings.Current;
        #region Attributes Get-Set define
        public static bool FirstTimeUse
        {
            get => AppSettings.GetValueOrDefault(nameof(FirstTimeUse), false);
            //get => false;
            set => AppSettings.AddOrUpdateValue(nameof(FirstTimeUse), value);
        }
        public static int FinishCount
        {
            get => AppSettings.GetValueOrDefault(nameof(FinishCount), 0);
            set => AppSettings.AddOrUpdateValue(nameof(FinishCount), value);
        }
        public static int RssiOption
        {
            get => AppSettings.GetValueOrDefault(nameof(RssiOption), 0);
            set => AppSettings.AddOrUpdateValue(nameof(RssiOption), value);
        }       
        public static bool isRigistered
        {
            get => AppSettings.GetValueOrDefault(nameof(isRigistered), false);
            set => AppSettings.AddOrUpdateValue(nameof(isRigistered), value);
        }
        public static bool getRigistered
        {
            get => AppSettings.GetValueOrDefault(nameof(getRigistered), false);
            set => AppSettings.AddOrUpdateValue(nameof(getRigistered), value);
        }
        public static bool HaveCashier 
        { 
            get => AppSettings.GetValueOrDefault(nameof(HaveCashier), false);
            set => AppSettings.AddOrUpdateValue(nameof(HaveCashier), value);
        }
        public static string records
        {
            get => AppSettings.GetValueOrDefault(nameof(records), string.Empty);
            set => AppSettings.AddOrUpdateValue(nameof(records), value);
        }
        public static string lastFinished
        {
            get => 
              AppSettings.GetValueOrDefault(nameof(lastFinished), string.Empty);
            set => 
              AppSettings.AddOrUpdateValue(nameof(lastFinished), value);
        }

        public static string OrderDistrict
        {
            get => 
             AppSettings.GetValueOrDefault(nameof(OrderDistrict), string.Empty);
            set => 
             AppSettings.AddOrUpdateValue(nameof(OrderDistrict), value);
        }
        #endregion
        static public void StoreAllState()
        {
            Console.WriteLine(">>StoreAllState");
            App app = (App)Application.Current;

            #region if finish all item, app will clear all state.
            if((app.FinishCount+1==app.records.Count && 
                app.lastFinished.type == RecordType.Exit) || 
                app.records.Count == 0)
            {
                ClearAllState();
                return;
            }
            #endregion

            FinishCount = app.FinishCount;
            isRigistered = app.isRigistered;
            getRigistered = app.getRigistered;
            HaveCashier = app.HaveCashier;

            #region Object to String for storage
            string recordsJsonString =
                JsonConvert.SerializeObject(app.records);

            Console.WriteLine("recordJsonString parsing = " + recordsJsonString);

            string lastFinishedJsonString =
                JsonConvert.SerializeObject(app.lastFinished);

            string OrderDistrictJsonString =
                JsonConvert.SerializeObject(app.OrderDistrict);

            records = recordsJsonString;
            lastFinished = lastFinishedJsonString;
            OrderDistrict = OrderDistrictJsonString;
            #endregion

            Console.WriteLine("<<StoreAllState");
        }

        static public void RestoreAllState()
        {
            Console.WriteLine(">>RestoreAllState");

            App app = (App)Application.Current;
          
            app.FinishCount = FinishCount;
            app.isRigistered = isRigistered;
            app.getRigistered = getRigistered;
            app.HaveCashier = HaveCashier;            

            app.lastFinished = 
                GetJsonObject<RgRecord>(lastFinished);
            app.records = 
                GetJsonObject<ObservableCollection<RgRecord>>(records);
            app.OrderDistrict =
                GetJsonObject<Dictionary<int, int>>(OrderDistrict);

            Console.WriteLine("<<RestoreAllState");
        }
        static private T GetJsonObject<T>(string jsonString) 
            where T : class, new()
        {
            Console.WriteLine("Json String = " + jsonString);
            if (string.IsNullOrEmpty(jsonString))
                return new T();
            return JsonConvert.DeserializeObject<T>(jsonString);   
        }
        static public void ClearAllState()
        {
            Console.WriteLine(">>ClearAllState");
            FinishCount = 0;
            HaveCashier = false;
            isRigistered = false;
            getRigistered = false;
            records = string.Empty;
            OrderDistrict = string.Empty;
            lastFinished = string.Empty;
            Console.WriteLine("<<ClearAllState");
        }
    }    
}
