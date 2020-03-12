using System;
using System.Linq;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

using System.Resources;

[assembly: ExportRenderer(typeof(ContentPage), typeof(IndoorNavigation.iOS.Renderers.CustomContentPageRenderer))]
namespace IndoorNavigation.iOS.Renderers
{
    public class CustomContentPageRenderer : PageRenderer
    {
        public new ContentPage Element
        {
            get { return (ContentPage)base.Element; }
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            ConfigureToolbarItems();
        }

        private void ConfigureToolbarItems()
        {
            if (NavigationController != null)
            {
                UINavigationItem navigationItem = NavigationController.TopViewController.NavigationItem;
                var orderedItems = Element.ToolbarItems.OrderBy(x => x.Priority);

                // add right side items
                var rightItems = orderedItems.Where(x => x.Priority >= 0).Select(x => x.ToUIBarButtonItem()).ToArray();
                navigationItem.SetRightBarButtonItems(rightItems, false);

                // add left side items, keep any already there
                var leftItems = orderedItems.Where(x => x.Priority < 0).Select(x => x.ToUIBarButtonItem()).ToArray();
                //if (navigationItem.LeftBarButtonItems != null)
                    //leftItems = navigationItem.LeftBarButtonItems.Union(leftItems).ToArray();
                navigationItem.SetLeftBarButtonItems(leftItems, false);
            }
        }

        public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
        {
            base.TraitCollectionDidChange(previousTraitCollection);

            if(this.TraitCollection.UserInterfaceStyle != previousTraitCollection.UserInterfaceStyle)
            {
                SetAppTheme();
            }
        }

        void SetAppTheme() 
        {
            if (this.TraitCollection.UserInterfaceStyle == UIUserInterfaceStyle.Dark) 
            {
                //change to dark theme
            }
            else 
            {
                //change to light theme
            }
        }

        public ResourceDictionary lightTheme 
        {
            get;
            set;
        }
    }
}
