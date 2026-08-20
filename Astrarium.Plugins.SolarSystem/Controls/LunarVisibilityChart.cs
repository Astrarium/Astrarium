using Astrarium.Algorithms;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Astrarium.Plugins.SolarSystem.Controls
{
    public class LunarVisibilityChart : Canvas
    {
        public readonly static DependencyProperty SunCoordinatesProperty = DependencyProperty.Register(nameof(SunCoordinates), typeof(CrdsEquatorial), typeof(LunarVisibilityChart), new FrameworkPropertyMetadata(defaultValue: null) { AffectsRender = true, BindsTwoWayByDefault = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, PropertyChangedCallback = new PropertyChangedCallback(DepPropertyChanged) });
        public readonly static DependencyProperty MoonRiseTransitSetProperty = DependencyProperty.Register(nameof(MoonRiseTransitSet), typeof(RTS), typeof(LunarVisibilityChart), new FrameworkPropertyMetadata(defaultValue: null) { AffectsRender = true, BindsTwoWayByDefault = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, PropertyChangedCallback = new PropertyChangedCallback(DepPropertyChanged) });
        public readonly static DependencyProperty GeoLocationProperty = DependencyProperty.Register(nameof(GeoLocation), typeof(CrdsGeographical), typeof(LunarVisibilityChart), new FrameworkPropertyMetadata(defaultValue: null) { AffectsRender = true, BindsTwoWayByDefault = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, PropertyChangedCallback = new PropertyChangedCallback(DepPropertyChanged) });
        public readonly static DependencyProperty SiderealTimeProperty = DependencyProperty.Register(nameof(SiderealTime), typeof(double), typeof(LunarVisibilityChart), new FrameworkPropertyMetadata(0.0) { AffectsRender = true, BindsTwoWayByDefault = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, PropertyChangedCallback = new PropertyChangedCallback(DepPropertyChanged) });
        public readonly static DependencyProperty PositionProperty = DependencyProperty.Register(nameof(Position), typeof(double), typeof(LunarVisibilityChart), new FrameworkPropertyMetadata(0.0) { AffectsRender = true, BindsTwoWayByDefault = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, PropertyChangedCallback = new PropertyChangedCallback(DepPropertyChanged) });
        public static readonly DependencyProperty IsDarkModeProperty = DependencyProperty.Register(nameof(IsDarkMode), typeof(bool), typeof(LunarVisibilityChart), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });

        protected CrdsHorizontal[] sunCoordinatesInterpolated = null;

        private static void DepPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ((LunarVisibilityChart)sender).Interpolate();
        }

        public CrdsGeographical GeoLocation
        {
            get => (CrdsGeographical)GetValue(GeoLocationProperty);
            set => SetValue(GeoLocationProperty, value);
        }

        public double SiderealTime
        {
            get => (double)GetValue(SiderealTimeProperty);
            set => SetValue(SiderealTimeProperty, value);
        }

        public double Position
        {
            get => (double)GetValue(PositionProperty);
            set => SetValue(PositionProperty, value);
        }

        public CrdsEquatorial SunCoordinates
        {
            get => (CrdsEquatorial)GetValue(SunCoordinatesProperty);
            set => SetValue(SunCoordinatesProperty, value);
        }

        public RTS MoonRiseTransitSet
        {
            get => (RTS)GetValue(MoonRiseTransitSetProperty);
            set => SetValue(MoonRiseTransitSetProperty, value);
        }

        public bool IsDarkMode
        {
            get => (bool)GetValue(IsDarkModeProperty);
            set => SetValue(IsDarkModeProperty, value);
        }

        private void Interpolate()
        {
            if (SunCoordinates != null && GeoLocation != null)
            {
                sunCoordinatesInterpolated = Interpolate(SunCoordinates);
                InvalidateVisual();
            }
            else
            {
                sunCoordinatesInterpolated = null;
                InvalidateVisual();
            }
        }

        private CrdsHorizontal[] Interpolate(CrdsEquatorial eq)
        {
            const int count = 120;
            double theta0 = SiderealTime;
            var location = GeoLocation;

            var hor = new List<CrdsHorizontal>();
            for (int i = 0; i <= count; i++)
            {
                double n = i / (double)count;
                double sidTime = Angle.To360(theta0 + n * 360.98564736629);
                hor.Add(eq.ToHorizontal(location, sidTime));
            }

            return hor.ToArray();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            AdjustPosition(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            AdjustPosition(e);
        }

        private void AdjustPosition(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var pos = e.GetPosition(this);
                Position = pos.X / ActualWidth;
            }
        }

        protected override void OnRender(DrawingContext dc)
        {
            Matrix m = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
            double dpiFactor = 1 / m.M11;

            RenderOptions.SetEdgeMode(this, EdgeMode.Unspecified);

            const double verticalPadding = 20;
            const double horizontalPadding = 0;

            Color colorDaylight = IsDarkMode ? Colors.DarkRed : Color.FromRgb(0, 114, 196);
            Color colorMoonlight = IsDarkMode ? Color.FromArgb(100, 200, 0, 0) : Color.FromArgb(100, 200, 200, 200);

            Brush brushBackground = IsDarkMode ? new SolidColorBrush(Color.FromRgb(20, 0, 0)) : new SolidColorBrush(Color.FromRgb(20, 20, 20));
            Brush brushTextLabel = new SolidColorBrush((Color)FindResource("ColorControlLightBackground"));
            Brush brushHourLine = IsDarkMode ? new SolidColorBrush(Color.FromRgb(255, 0, 0)) : new SolidColorBrush(Color.FromRgb(200, 200, 200));
            Brush brushPosition = IsDarkMode ? Brushes.Red : Brushes.Yellow;

            Pen penHourLine = new Pen(brushHourLine, dpiFactor);
            Pen penPosition = new Pen(brushPosition, dpiFactor * 2);

            var bounds = new Rect(0, 0, ActualWidth, ActualHeight);
            
            dc.PushClip(new RectangleGeometry(bounds));  
            
            dc.DrawRectangle(brushBackground, null, bounds);

            // sun visibility
            // daylight/night background
            if (sunCoordinatesInterpolated != null)
            {
                var boundsSun = new Rect(0, 0, ActualWidth, ActualHeight / 2);

                double sunAltitudesCount = sunCoordinatesInterpolated.Length;
                LinearGradientBrush linearGradientBrush = new LinearGradientBrush();
                linearGradientBrush.StartPoint = new Point(horizontalPadding, ActualHeight / 2);
                linearGradientBrush.EndPoint = new Point(ActualWidth - horizontalPadding, ActualHeight / 2);
                linearGradientBrush.MappingMode = BrushMappingMode.Absolute;
                for (int i = 0; i < sunCoordinatesInterpolated.Length; i++)
                {
                    // -18 degrees is an astronomical night
                    double transp = sunCoordinatesInterpolated[i].Altitude <= -18 ? 0 : (sunCoordinatesInterpolated[i].Altitude < 0 ? (sunCoordinatesInterpolated[i].Altitude + 18) / 18.0 : 1);
                    Color c = Color.FromArgb((byte)(transp * 255), colorDaylight.R, colorDaylight.G, colorDaylight.B);
                    double f = (i / (sunAltitudesCount - 1) + 0.5) % 1;
                    linearGradientBrush.GradientStops.Add(new GradientStop(c, f));
                }
                dc.DrawRectangle(linearGradientBrush, null, boundsSun);
            }

            // lunar visibility
            if (MoonRiseTransitSet != null)
            {
                var boundsMoon = new Rect(0, ActualHeight / 2, ActualWidth, ActualHeight);
                var b = new LinearGradientBrush();
                b.StartPoint = new Point(horizontalPadding, ActualHeight / 2);
                b.EndPoint = new Point(ActualWidth - horizontalPadding, ActualHeight / 2);
                b.MappingMode = BrushMappingMode.Absolute;
                var g = b.GradientStops;

                if (MoonRiseTransitSet.Rise < MoonRiseTransitSet.Set)
                {
                    g.Add(new GradientStop(Colors.Transparent, 0));
                    g.Add(new GradientStop(Colors.Transparent, MoonRiseTransitSet.Rise - 0.001));
                    g.Add(new GradientStop(colorMoonlight, MoonRiseTransitSet.Rise));
                    g.Add(new GradientStop(colorMoonlight, MoonRiseTransitSet.Set));
                    g.Add(new GradientStop(Colors.Transparent, MoonRiseTransitSet.Set + 0.001));
                    g.Add(new GradientStop(Colors.Transparent, 1));
                }
                else if (MoonRiseTransitSet.Set < MoonRiseTransitSet.Rise)
                {
                    g.Add(new GradientStop(colorMoonlight, 0));
                    g.Add(new GradientStop(colorMoonlight, MoonRiseTransitSet.Set));
                    g.Add(new GradientStop(Colors.Transparent, MoonRiseTransitSet.Set + 0.001));
                    g.Add(new GradientStop(Colors.Transparent, MoonRiseTransitSet.Rise - 0.001));
                    g.Add(new GradientStop(colorMoonlight, MoonRiseTransitSet.Rise));
                    g.Add(new GradientStop(colorMoonlight, 1));
                }
                else if (double.IsNaN(MoonRiseTransitSet.Set) && !double.IsNaN(MoonRiseTransitSet.Rise))
                {
                    g.Add(new GradientStop(Colors.Transparent, 0));
                    g.Add(new GradientStop(Colors.Transparent, MoonRiseTransitSet.Rise - 0.001));
                    g.Add(new GradientStop(colorMoonlight, MoonRiseTransitSet.Rise));
                    g.Add(new GradientStop(colorMoonlight, 1));
                }
                else if (double.IsNaN(MoonRiseTransitSet.Rise) && !double.IsNaN(MoonRiseTransitSet.Set))
                {
                    g.Add(new GradientStop(colorMoonlight, 0));
                    g.Add(new GradientStop(colorMoonlight, MoonRiseTransitSet.Set - 0.001));
                    g.Add(new GradientStop(Colors.Transparent, MoonRiseTransitSet.Set));
                    g.Add(new GradientStop(Colors.Transparent, 1));
                }
                else if (MoonRiseTransitSet.Duration == 1)
                {
                    g.Add(new GradientStop(colorMoonlight, 0));
                    g.Add(new GradientStop(colorMoonlight, 1));
                }
            
                dc.DrawRectangle(b, null, boundsMoon);
            }

            // time grid
            for (int i = 0; i <= 24; i++)
            {
                double x = horizontalPadding + i / 24.0 * (ActualWidth - 2 * horizontalPadding);
                GuidelineSet guidelines = new GuidelineSet();
                guidelines.GuidelinesX.Add(x);
                dc.PushGuidelineSet(guidelines);
                dc.DrawLine(penHourLine, new Point(x, 0), new Point(x, ActualHeight));
                dc.Pop();
            }

            dc.DrawLine(penPosition, new Point(Position * ActualWidth, 0), new Point(Position * ActualWidth, ActualHeight));

            // pop the vertical offset margins
            dc.Pop();

            // time grid labels
            for (int i = 0; i <= 24; i++)
            {
                double x = horizontalPadding + i / 24.0 * (ActualWidth - 2 * horizontalPadding);
                var text = new FormattedText($"{i}", System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 10, brushTextLabel);
                dc.DrawText(text, new Point(x - text.WidthIncludingTrailingWhitespace / 2, (- verticalPadding - text.Height) / 2));
            }
        }
    }
}
