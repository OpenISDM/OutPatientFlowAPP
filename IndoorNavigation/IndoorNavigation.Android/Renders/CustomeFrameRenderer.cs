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


using IndoorNavigation.Views.Controls;
using IndoorNavigation.Droid.Renders;
using Xamarin.Forms.Platform.Android;
using Android.Graphics;
using Xamarin.Forms;

[assembly : ExportRenderer(typeof(CostumeFrame), typeof(CustomeFrameRenderer))]
namespace IndoorNavigation.Droid.Renders
{
    public class CustomeFrameRenderer : FrameRenderer
    {
        public CustomeFrameRenderer(Context context) : base(context) { }

        protected override void OnElementChanged(ElementChangedEventArgs<Frame> e)
        {
            base.OnElementChanged(e);

            var frame = e.NewElement as CostumeFrame;

            if (frame == null) return;

            if (frame.HasShadow)
            {
                //   ViewGroup.SetBackgroundColor(element.ShadowColor.ToAndroid());
                ViewGroup.SetOutlineSpotShadowColor(frame.ShadowColor.ToAndroid());
            }
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);

            var frame = Element as CostumeFrame;            
            var my1stPaint = new Paint();
            var my2ndPaint = new Paint();
            var backgroundPaint = new Paint();

            my1stPaint.AntiAlias = true;
            my1stPaint.SetStyle(Paint.Style.Stroke);
            my1stPaint.StrokeWidth = frame.BorderWidth + 2;
            my1stPaint.Color = frame.BorderColor.ToAndroid();

            my2ndPaint.AntiAlias = true;
            my2ndPaint.SetStyle(Paint.Style.Stroke);
            my2ndPaint.StrokeWidth = frame.BorderWidth;
            my2ndPaint.Color = frame.BackgroundColor.ToAndroid();

            backgroundPaint.SetStyle(Paint.Style.Stroke);
            backgroundPaint.StrokeWidth = 4;
            backgroundPaint.Color = frame.BackgroundColor.ToAndroid();

            Rect oldBounds = new Rect();
            canvas.GetClipBounds(oldBounds);

            RectF oldOutlineBounds = new RectF();
            oldOutlineBounds.Set(oldBounds);

            RectF myOutlineBounds = new RectF();
            myOutlineBounds.Set(oldBounds);
            myOutlineBounds.Top += (int)my2ndPaint.StrokeWidth + 3;
            myOutlineBounds.Bottom -= (int)my2ndPaint.StrokeWidth + 3;
            myOutlineBounds.Left += (int)my2ndPaint.StrokeWidth + 3;
            myOutlineBounds.Right -= (int)my2ndPaint.StrokeWidth + 3;

            canvas.DrawRoundRect(oldOutlineBounds, 10, 10, backgroundPaint); //to "hide" old outline
            canvas.DrawRoundRect(myOutlineBounds, frame.CornerRadius, frame.CornerRadius, my1stPaint);
            canvas.DrawRoundRect(myOutlineBounds, frame.CornerRadius, frame.CornerRadius, my2ndPaint);

            base.OnDraw(canvas);
        }
    }
}