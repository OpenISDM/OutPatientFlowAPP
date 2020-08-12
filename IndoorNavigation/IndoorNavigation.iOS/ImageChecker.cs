using IndoorNavigation.iOS;
using IndoorNavigation.Models;
using UIKit;
using Xamarin.Forms;
[assembly:Dependency(typeof(ImageChecker))]
namespace IndoorNavigation.iOS
{
    public class ImageChecker : IImageChecker
    {
        public bool DoesImageExist(string image)
        {
            var x = UIImage.FromBundle(image);
            return x != null;
        }
    }
}