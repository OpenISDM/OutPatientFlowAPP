using System.IO;
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
            var context = Android.App.Application.Context;
            var resources = context.Resources;
            var name = Path.GetFileNameWithoutExtension(image);
            int resourceID = resources.GetIdentifier(name.ToLower(), "drawable",
                context.PackageName);

            return resourceID != 0;
        }
    }
}