using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace IndoorNavigation.Views.Controls
{
    public class CostumeFrame : Frame
    {
        public static readonly BindableProperty ShadowColorProperty
            = BindableProperty.Create(
                nameof(ShadowColor),
                typeof(Color),
                typeof(CostumeFrame),
                Color.Brown
                );

        public static readonly BindableProperty BorderWidthProperty
            = BindableProperty.Create(
                nameof(BorderWidth), 
                typeof(float), 
                typeof(CostumeFrame)
                );

        public Color ShadowColor
        {
            get { return (Color)GetValue(ShadowColorProperty); }
            set { SetValue(ShadowColorProperty, value); }
        }

        public float BorderWidth
        {
            get { return (float)GetValue(BorderWidthProperty); }
            set { SetValue(BorderWidthProperty, value); }
        }
    }
}
