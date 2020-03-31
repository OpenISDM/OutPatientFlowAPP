using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Resources;
using System.Globalization;
using Plugin.Multilingual;
using IndoorNavigation.Resources.Helpers;
namespace IndoorNavigation.Models
{
    public static class PhoneInformation_
    {
        //private const string _resourceId= "IndoorNavigation.Resources.AppResources";
        #region Propertys
        private static string NTUH_Yunlin= GetResourceString("HOSPITAL_NAME_STRING");
        private static string TCH = GetResourceString("TAIPEI_CITY_HALL_STRING");
        private static string Lab = GetResourceString("LAB_STRING");
        private static string YCH = GetResourceString("YUANLIN_CHRISTIAN_HOSPITAL_STRING");

        private static CultureInfo _currentLanguage;
        #endregion

        //public PhoneInformation_()
        //{
        //    NTUH_Yunlin = GetResourceString("HOSPITAL_NAME_STRING");
        //    TCH = GetResourceString("TAIPEI_CITY_HALL_STRING");
        //    Lab = GetResourceString("LAB_STRING");
        //    YCH = GetResourceString("YUANLIN_CHRISTIAN_HOSPITAL_STRING");

        //    _currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
        //}

        public static string GiveCurrentLanguage()
        {
            if (_currentLanguage.ToString() == "en-US" || _currentLanguage.ToString() == "en")
                return "en-US";
            if (_currentLanguage.ToString() == "zh-TW" || _currentLanguage.ToString() == "zh")
                return "zh";
            
            return string.Empty;
        }

        

        public static string GetCurrentMapName(string userNaming)
        {
            if (userNaming == NTUH_Yunlin)
                return "NTUH Yunlin Branch";
            if (userNaming == TCH)
                return "Taipei City Hall";
            if (userNaming == Lab)
                return "Lab";
            if (userNaming == TCH)
                return "Yuanlin Christian Hospital";
            return string.Empty;
        }


        public static MapName GiveGenerateMapName(string userNaming)
        {

            if (userNaming == NTUH_Yunlin)
                return new MapName("NTUH Yunlin Branch", "NTUH_YunLin");
            if (userNaming == TCH)
                return new MapName("Taipei City Hall", "Taipei_City_Hall");
            if (userNaming == Lab)
                return new MapName("Lab", "Lab");
            if (userNaming == YCH)
                return new MapName("Yuanlin Christian Hospital", "Yuanlin_Christian_Hospital");
            return null;
        }       

        private static string GetResourceString(string key)
        {
            string _resourceId = "IndoorNavigation.Resources.AppResources";
            ResourceManager resourceManager = 
                new ResourceManager(_resourceId, typeof(TranslateExtension).GetTypeInfo().Assembly);

            return resourceManager.GetString(key, _currentLanguage);
        }
    }
    public class MapName
    {
        public string _fileName;
        public string _readingPath;

        public MapName(string fileName, string readingPath)
        {
            this._fileName = fileName;
            this._readingPath = readingPath;
        }
    }
}
