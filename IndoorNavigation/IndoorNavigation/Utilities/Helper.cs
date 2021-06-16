using System;
using static IndoorNavigation.Utilities.Constants;
using Xamarin.Essentials;
namespace IndoorNavigation.Utilities
{
    public static class Helper
    {
        public static bool isEmptyGuid(Guid uuid)
        {
            return Guid.Empty.Equals(uuid);
        }

        public static bool isSameGuid(Guid uuid1, Guid uuid2)
        {
            return uuid1.Equals(uuid2);
        }

        public static double GetPercentage(int current, int total)
        {
            return (double)Math.Round(100 * ((decimal)current /
                               (total)), 3);
        }

        public static bool StrToBool(string str)
        {
            if (str.Trim().ToUpper() == "TRUE")
                return true;
            return false;
        }
        public static string GetListTextColor(string userNaming)
        {
            if (userNaming.Trim().ToUpper().Contains(BETA_STRING)) return "#808A87";
            switch (AppInfo.RequestedTheme){
                case AppTheme.Dark:
                    return "#f5f5f5";                    
                case AppTheme.Light:
                default:
                    return "#3f51b5";
            }
        }
    }
}
