using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.CustomControls;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace IndoorNavigation.Views.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProgressBar : ContentView
    {
        public ProgressBar()
        {
            InitializeComponent();

            Container.BackgroundColor = backgroundColor;
           

            WidthOfContainer = Application.Current.MainPage.Width;//Container.Width;
            //Console.WriteLine(WidthOfContainer + " 11111111");
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if(ProgressProperty.PropertyName == propertyName)
            {
                SetProgressBar();
            }
        }

        private void SetProgressBar()
        {
            Container.Children.Clear();

            double progress = NormalizeValue(Progress);

            if (progress < 100)
            {
                Container.Children.Add(new BoxView
                {
                    Color = ProgressColor,
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    HorizontalOptions = LayoutOptions.StartAndExpand,
                    WidthRequest = (double)(/*Container.Width*/WidthOfContainer* (progress/100))
                }) ;
            }
            else
            {
                Container.Children.Add(new BoxView
                {
                    Color=ProgressColor,
                    VerticalOptions=LayoutOptions.FillAndExpand,
                    HorizontalOptions=LayoutOptions.FillAndExpand
                });
            }

            //Console.WriteLine(Container.Width + "bbbbbbb");
        }

        public double NormalizeValue(double value)
        {
            if (value <= 0) value = 0.0000000001; 
                //this to avoid "NaN is not a Number" Exception on GridLength = 0* 
            if (value >= 100) value = 100;
            return value;
        }

        #region Propertys
        private BoxView _pregressColor;
        private double WidthOfContainer;
        public Color ProgressColor
        {
            get { return (Color)GetValue(ProgressColorProperty); }
            set { SetValue(ProgressColorProperty, value); }
        }
        public double Progress
        {
            get { return (double)GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }

        public Color backgroundColor
        {
            get { return (Color)GetValue(backgroundColorProperty); }
            set { SetValue(backgroundColorProperty, value); }
        } 
        //public double Width
        //{
        //    get { return (double)GetValue(WidthProperty); }
        //    set { SetValue(WidthProperty, value); }
        //}
        //public static readonly BindableProperty WidthProperty = BindableProperty.Create(nameof(Width), typeof(double), typeof(ProgressBarView), default(double));
        public static readonly BindableProperty ProgressColorProperty = BindableProperty.Create(nameof(ProgressColor), typeof(Color), typeof(ProgressBarView), Color.Blue);
        public static readonly BindableProperty backgroundColorProperty = BindableProperty.Create(nameof(backgroundColor), typeof(Color), typeof(ProgressBarView), Color.Transparent);
        public static readonly BindableProperty ProgressProperty = BindableProperty.Create(nameof(Progress), typeof(double), typeof(ProgressBarView), default(double));
        #endregion
    }
}