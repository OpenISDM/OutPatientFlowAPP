using System.IO;
using System;
using Android.App;
using IndoorNavigation.Droid;
using IndoorNavigation.Models;
using Xamarin.Forms;

[assembly:Dependency(typeof(ImageChecker))]
namespace IndoorNavigation.Droid
{
    public class ImageChecker : IImageChecker
    {
        public bool DoesImageExist(string image)
        {
            if (string.IsNullOrEmpty(image)) return false;

            var context = Android.App.Application.Context;
            var resources = context.Resources;
            var name = Path.GetFileNameWithoutExtension(image);
           
            int resourceId = 
                resources.GetIdentifier(name.ToLower(), "drawable", 
                context.PackageName);

            return resourceId != 0;
        }
    }
}