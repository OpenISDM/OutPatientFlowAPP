using IndoorNavigation.Models;
using System;
using System.Globalization;
using System.Threading;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly : Xamarin.Forms.Dependency(typeof(IMultilingual))]
namespace IndoorNavigation.Droid.Custom
{
    
    public class MultilingualImplemetation : IMultilingual
    {
        CultureInfo _currentCultureInfo = CultureInfo.InstalledUICulture;

        public CultureInfo CurrentCultureInfo
        {
            get { return _currentCultureInfo; }
            set
            {
                _currentCultureInfo = value;
                Thread.CurrentThread.CurrentCulture = value;
                Thread.CurrentThread.CurrentUICulture = value;
            }
        }

        public CultureInfo DeviceCultureInfo { get { return CultureInfo.InstalledUICulture; } }               
        public CultureInfo GetCulture(string Name) { return CultureInfo.GetCultureInfo(Name); }
        
    }
}