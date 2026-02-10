using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Astrarium.Plugins.SolarSystem.Controls
{
    public class MoonDiskImageControl : FrameworkElement
    {
        public static readonly DependencyProperty PhaseProperty = DependencyProperty.Register(nameof(Phase), typeof(double), typeof(MoonDiskImageControl), new PropertyMetadata(0.0, OnPropertyChanged));
        public static readonly DependencyProperty RotateAxisProperty = DependencyProperty.Register(nameof(RotateAxis), typeof(bool), typeof(MoonDiskImageControl), new PropertyMetadata(true, OnPropertyChanged));
        public static readonly DependencyProperty NorthTopProperty = DependencyProperty.Register(nameof(NorthTop), typeof(bool), typeof(MoonDiskImageControl), new PropertyMetadata(true, OnPropertyChanged));
        public static readonly DependencyProperty AxisRotationProperty = DependencyProperty.Register(nameof(AxisRotation), typeof(double), typeof(MoonDiskImageControl), new PropertyMetadata(0.0, OnPropertyChanged));
        public static readonly DependencyProperty NightModeProperty = DependencyProperty.Register(nameof(NightMode), typeof(bool), typeof(MoonDiskImageControl), new PropertyMetadata(false, OnPropertyChanged));

        public double Phase
        {
            get => (double)GetValue(PhaseProperty);
            set => SetValue(PhaseProperty, value);
        }

        public bool RotateAxis
        {
            get => (bool)GetValue(RotateAxisProperty);
            set => SetValue(RotateAxisProperty, value);
        }

        public bool NorthTop
        {
            get => (bool)GetValue(NorthTopProperty);
            set => SetValue(NorthTopProperty, value);
        }

        public double AxisRotation
        {
            get => (double)GetValue(AxisRotationProperty);
            set => SetValue(AxisRotationProperty, value);
        }

        public bool NightMode
        {
            get => (bool)GetValue(NightModeProperty);
            set => SetValue(NightModeProperty, value);
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as MoonDiskImageControl;
            control.InvalidateVisual();
        }

        static MoonDiskImageControl()
        {
            string filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data", "MoonDisk.png");
            if (File.Exists(filePath))
            {
                LoadImage(filePath);
            }
        }

        protected override void OnRender(DrawingContext dc)
        {
            if (originalImage != null)
            {
                double centerX = ActualWidth / 2;
                double centerY = ActualWidth / 2;

                ImageSource image;

                if (NightMode)
                {
                    var dg = new DrawingGroup();
                    dg.Children.Add(new ImageDrawing(originalImage, new Rect(0, 0, originalImage.PixelWidth, originalImage.PixelHeight)));
                    dg.Children.Add(new GeometryDrawing(
                        new SolidColorBrush(Color.FromRgb(100, 0, 0)),
                        null,
                        new EllipseGeometry(new Rect(0, 0, originalImage.PixelWidth, originalImage.PixelHeight))
                    ));

                    image = new DrawingImage(dg);
                }
                else
                {
                    image = originalImage;
                }

                var rotateTransform = new RotateTransform(RotateAxis ? -AxisRotation : (NorthTop ? 0 : 180), centerX, centerY);
                dc.PushTransform(rotateTransform);

                dc.PushTransform(new ScaleTransform(0.999, 0.999, Width / 2, Height / 2));
                dc.DrawImage(image, new Rect(0, 0, ActualWidth, ActualHeight));
                dc.Pop();
                
                Brush shadowBrush = new SolidColorBrush(Color.FromArgb(200, 0, 0, 0));

                if (Phase >= 0 && Phase <= 0.5)
                {
                    double w = -(2 * Phase - 1) * Width / 2;
                    var g1 = new CombinedGeometry();
                    g1.GeometryCombineMode = GeometryCombineMode.Intersect;
                    g1.Geometry1 = new RectangleGeometry(new Rect(0, 0, Width / 2, Height));
                    g1.Geometry2 = new EllipseGeometry(new Rect(0, 0, Width, Height));
                    var g2 = new CombinedGeometry();
                    g2.GeometryCombineMode = GeometryCombineMode.Union;
                    g2.Geometry1 = new EllipseGeometry(new Rect(Width / 2 - w, 0, w * 2, Height));
                    g2.Geometry2 = g1;

                    dc.DrawGeometry(shadowBrush, null, g2);
                }
                else if (Phase > 0.5 && Phase < 1)
                {
                    double w = (2 * Phase - 1) * Width / 2;

                    var g1 = new CombinedGeometry();
                    g1.GeometryCombineMode = GeometryCombineMode.Exclude;
                    g1.Geometry1 = new EllipseGeometry(new Rect(0, 0, Width, Height));
                    g1.Geometry2 = new RectangleGeometry(new Rect(Width / 2, 0, Width, Height));
                    var g2 = new CombinedGeometry();
                    g2.GeometryCombineMode = GeometryCombineMode.Exclude;
                    g2.Geometry1 = g1;
                    g2.Geometry2 = new EllipseGeometry(new Rect(Width / 2 - w, 0, w * 2, Height));

                    dc.DrawGeometry(shadowBrush, null, g2);
                }
                else if (Phase > -1 && Phase < -0.5)
                {
                    double w = -(2 * Phase + 1) * Width / 2;

                    var g1 = new CombinedGeometry();
                    g1.GeometryCombineMode = GeometryCombineMode.Intersect;
                    g1.Geometry1 = new RectangleGeometry(new Rect(Width / 2, 0, Width / 2, Height));
                    g1.Geometry2 = new EllipseGeometry(new Rect(0, 0, Width, Height));
                    var g2 = new CombinedGeometry();
                    g2.GeometryCombineMode = GeometryCombineMode.Exclude;
                    g2.Geometry1 = g1;
                    g2.Geometry2 = new EllipseGeometry(new Rect(Width / 2 - w, 0, w * 2, Height));

                    dc.DrawGeometry(shadowBrush, null, g2);
                }
                else if (Phase >= -0.5 && Phase <= 0)
                {
                    double w = (2 * Phase + 1) * Width / 2;

                    var g1 = new CombinedGeometry();
                    g1.GeometryCombineMode = GeometryCombineMode.Intersect;
                    g1.Geometry1 = new RectangleGeometry(new Rect(Width / 2, 0, Width / 2, Height));
                    g1.Geometry2 = new EllipseGeometry(new Rect(0, 0, Width, Height));
                    var g2 = new CombinedGeometry();
                    g2.GeometryCombineMode = GeometryCombineMode.Union;
                    g2.Geometry1 = new EllipseGeometry(new Rect(Width / 2 - w, 0, w * 2, Height));
                    g2.Geometry2 = g1;

                    dc.DrawGeometry(shadowBrush, null, g2);
                }
                dc.Pop();

            }
            base.OnRender(dc);
        }

        private static void LoadImage(string filePath)
        {
            try
            {
                originalImage = new BitmapImage();
                originalImage.BeginInit();
                originalImage.CacheOption = BitmapCacheOption.OnLoad;
                originalImage.UriSource = new Uri(filePath, UriKind.RelativeOrAbsolute);
                originalImage.EndInit();
                originalImage.Freeze();


                
            }
            catch
            {
                originalImage = null;
            }
        }

        private static BitmapImage originalImage;
    }


}
