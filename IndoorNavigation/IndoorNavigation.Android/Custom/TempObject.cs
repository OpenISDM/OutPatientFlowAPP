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

namespace IndoorNavigation.Droid.Custom
{
    public class TemporalObject : Java.Lang.Object
    {
        public object Item { get; private set; }
        public Android.Views.View View { get; private set; }

        public TemporalObject(Android.Views.View v, object i)
        {
            View = v;
            Item = i;
        }
    }
}