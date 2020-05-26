using IndoorNavigation.Models;
using System;
using System.Globalization;
using System.Threading;
using Xamarin.Forms;
[assembly : Xamarin.Forms.Dependency(typeof(IMultilingual))]
namespace IndoorNavigation.iOS
{
    public class MutilingualImplemtation : IMultilingual
    {
        private CultureInfo _currentCultureInfo = CultureInfo.InstalledUICulture;
        public CultureInfo CurrentCultureInfo 
        { 
            get 
            {
                return _currentCultureInfo;
            } 
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