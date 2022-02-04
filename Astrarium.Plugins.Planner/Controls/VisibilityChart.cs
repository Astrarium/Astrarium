using Astrarium.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Astrarium.Plugins.Planner.Controls
{
    public class VisibilityChart : Canvas
    {
        public readonly static DependencyProperty SunCoordinatesProperty = DependencyProperty.Register(nameof(SunCoordinates), typeof(CrdsEquatorial[]), typeof(VisibilityChart), new FrameworkPropertyMetadata(defaultValue: null) { AffectsRender = true, BindsTwoWayByDefault = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, PropertyChangedCallback = new PropertyChangedCallback(DepPropertyChanged) });
        public readonly static DependencyProperty BodyCoordinatesProperty = DependencyProperty.Register(nameof(BodyCoordinates), typeof(CrdsEquatorial[]), typeof(VisibilityChart), new FrameworkPropertyMetadata(defaultValue: null) { AffectsRender = true, BindsTwoWayByDefault = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, PropertyChangedCallback = new PropertyChangedCallback(DepPropertyChanged) });
        public readonly static DependencyProperty GeoLocationProperty = DependencyProperty.Register(nameof(GeoLocation), typeof(CrdsGeographical), typeof(VisibilityChart), new FrameworkPropertyMetadata(defaultValue: null) { AffectsRender = true, BindsTwoWayByDefault = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, PropertyChangedCallback = new PropertyChangedCallback(DepPropertyChanged) });
        public readonly static DependencyProperty SiderealTimeProperty = DependencyProperty.Register(nameof(SiderealTime), typeof(double), typeof(VisibilityChart), new FrameworkPropertyMetadata(0.0) { AffectsRender = true, BindsTwoWayByDefault = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, PropertyChangedCallback = new PropertyChangedCallback(DepPropertyChanged) });
        public readonly static DependencyProperty FromTimeProperty = DependencyProperty.Register(nameof(FromTime), typeof(TimeSpan), typeof(VisibilityChart), new FrameworkPropertyMetadata(TimeSpan.Zero) { AffectsRender = true, BindsTwoWayByDefault = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, PropertyChangedCallback = new PropertyChangedCallback(DepPropertyChanged) });
        public readonly static DependencyProperty ToTimeProperty = DependencyProperty.Register(nameof(ToTime), typeof(TimeSpan), typeof(VisibilityChart), new FrameworkPropertyMetadata(TimeSpan.Zero) { AffectsRender = true, BindsTwoWayByDefault = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, PropertyChangedCallback = new PropertyChangedCallback(DepPropertyChanged) });


        private static void DepPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            VisibilityChart @this = (VisibilityChart)sender;
            @this.Interpolate();
        }

        public VisibilityChart() { }

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

        public CrdsEquatorial[] SunCoordinates
        {
            get => (CrdsEquatorial[])GetValue(SunCoordinatesProperty);
            set => SetValue(SunCoordinatesProperty, value);
        }

        public CrdsEquatorial[] BodyCoordinates
        {
            get => (CrdsEquatorial[])GetValue(BodyCoordinatesProperty);
            set => SetValue(BodyCoordinatesProperty, value);
        }

        public TimeSpan FromTime
        {
            get => (TimeSpan)GetValue(FromTimeProperty);
            set => SetValue(FromTimeProperty, value);
        }

        public TimeSpan ToTime
        {
            get => (TimeSpan)GetValue(ToTimeProperty);
            set => SetValue(ToTimeProperty, value);
        }

        private CrdsHorizontal[] bodyCoordinatesInterpolated = null;
        private CrdsHorizontal[] sunCoordinatesInterpolated = null;

        private void Interpolate()
        {
            if (SunCoordinates != null && SunCoordinates.Length == 3 &&
                BodyCoordinates != null && BodyCoordinates.Length == 3 &&
                GeoLocation != null)
            {
                sunCoordinatesInterpolated = Interpolate(SunCoordinates, GeoLocation, SiderealTime, 120);
                bodyCoordinatesInterpolated = Interpolate(BodyCoordinates, GeoLocation, SiderealTime, 120);

                InvalidateVisual();
            }
        }

        private CrdsHorizontal[] Interpolate(CrdsEquatorial[] eq, CrdsGeographical location, double theta0, int count)
        {
            if (eq.Length != 3)
                throw new ArgumentException("Number of equatorial coordinates in the array should be equal to 3.");

            double[] alpha = new double[3];
            double[] delta = new double[3];
            for (int i = 0; i < 3; i++)
            {
                alpha[i] = eq[i].Alpha;
                delta[i] = eq[i].Delta;
            }

            Angle.Align(alpha);
            Angle.Align(delta);

            List<CrdsHorizontal> hor = new List<CrdsHorizontal>();
            for (int i = 0; i <= count; i++)
            {
                double n = i / (double)count;
                CrdsEquatorial eq0 = InterpolateEq(alpha, delta, n);
                var sidTime = InterpolateSiderialTime(theta0, n);
                hor.Add(eq0.ToHorizontal(location, sidTime));
            }

            return hor.ToArray();
        }

        private static double InterpolateSiderialTime(double theta0, double n)
        {
            return Angle.To360(theta0 + n * 360.98564736629);
        }

        private static CrdsEquatorial InterpolateEq(double[] alpha, double[] delta, double n)
        {
            double[] x = new double[] { 0, 0.5, 1 };
            CrdsEquatorial eq = new CrdsEquatorial();
            eq.Alpha = Interpolation.Lagrange(x, alpha, n);
            eq.Delta = Interpolation.Lagrange(x, delta, n);
            return eq;
        }

        protected override void OnRender(DrawingContext dc)
        {
            Matrix m = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
            double dpiFactor = 1 / m.M11;

            RenderOptions.SetEdgeMode(this, EdgeMode.Unspecified);

            Brush brushBackground = new SolidColorBrush(Color.FromRgb(20, 20, 20));
            Brush brushGround = Brushes.DarkGreen;
            Brush brushHourLine = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255));
            Brush brushBodyLine = Brushes.Yellow;
            Color colorDaylight = Color.FromArgb(255, 119, 203, 255);
            Pen penHourLine = new Pen(brushHourLine, dpiFactor);
            Pen penBodyLine = new Pen(brushBodyLine, dpiFactor);
            Brush brushObservationLimits = new SolidColorBrush(Color.FromArgb(60, 255, 0, 0));

            var bounds = new Rect(0, 0, ActualWidth, ActualHeight);
            dc.PushClip(new RectangleGeometry(bounds));

            // background
            dc.DrawRectangle(brushBackground, null, bounds);

            if (sunCoordinatesInterpolated != null)
            {
                double sunAltitudesCount = sunCoordinatesInterpolated.Length;
                for (int i = 0; i < sunCoordinatesInterpolated.Length; i++)
                {
                    double x0 = i / sunAltitudesCount * ActualWidth;
                    double x1 = (i + 1) / sunAltitudesCount * ActualWidth;

                    // -18 degrees is an astronomical night
                    double transp = sunCoordinatesInterpolated[i].Altitude <= -18 ? 0 : (sunCoordinatesInterpolated[i].Altitude < 0 ? (sunCoordinatesInterpolated[i].Altitude + 18) / 18.0 : 1);

                    Color c = Color.FromArgb((byte)(transp * 255), colorDaylight.R, colorDaylight.G, colorDaylight.B);

                    SolidColorBrush brush = new SolidColorBrush(c);

                    for (int j = 0; j < 2; j++)
                    {
                        double _x0 = x0 + (j == 0 ? -1 : 1) * ActualWidth / 2;
                        double _x1 = x1 + (j == 0 ? -1 : 1) * ActualWidth / 2;
                        var rect = new Rect(_x0, 0, _x1 - _x0, ActualHeight);

                        // Create a guidelines set
                        var guidelines = new GuidelineSet();
                        guidelines.GuidelinesX.Add(rect.Left);
                        guidelines.GuidelinesX.Add(rect.Right);
                        guidelines.GuidelinesY.Add(rect.Top);
                        guidelines.GuidelinesY.Add(rect.Bottom);

                        dc.PushGuidelineSet(guidelines);
                        dc.DrawRectangle(brush, null, rect);
                        dc.Pop();
                    }
                }
            }

            dc.DrawRectangle(brushGround, null, new Rect(0, ActualHeight / 2, ActualWidth, ActualHeight / 2));

            if (bodyCoordinatesInterpolated != null)
            {
                // body altitude line
                var figure = new PathFigure();
                
                for (int j = 0; j < 2; j++)
                {
                    for (int i = 0; i <= bodyCoordinatesInterpolated.Length; i++)
                    {
                        double k = i;
                        double x = k / (double)bodyCoordinatesInterpolated.Length * ActualWidth;
                        double alt = i == bodyCoordinatesInterpolated.Length ? bodyCoordinatesInterpolated[i - 1].Altitude : bodyCoordinatesInterpolated[i].Altitude;
                        double y = ActualHeight / 2 - alt / 90.0 * (ActualHeight / 2);
                        var point = new Point(x + (j == 0 ? -ActualWidth / 2 : ActualWidth / 2), y);
                        if (i == 0 && j == 0)
                        {
                            figure.StartPoint = point;
                        }
                        figure.Segments.Add(new LineSegment(point, true));
                    }
                }

                var geometry = new PathGeometry();
                geometry.Figures.Add(figure);

                RenderOptions.SetEdgeMode(geometry, EdgeMode.Unspecified);

                dc.DrawGeometry(null, penBodyLine, geometry);
            }

            dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(150, 0, 0, 0)), null, new Rect(0, ActualHeight / 2, ActualWidth, ActualHeight / 2));;

            // visibility box
            if (true)
            {
                double from = ((FromTime.TotalDays + 0.5) % 1) * ActualWidth;
                double to = ((ToTime.TotalDays + 0.5) % 1) * ActualWidth;

                if (from < to)
                {
                    dc.DrawRectangle(brushObservationLimits, null, new Rect(0, 0, from, ActualHeight / 2));
                    dc.DrawRectangle(brushObservationLimits, null, new Rect(to, 0, ActualWidth - to, ActualHeight / 2));
                }
                else
                {
                    dc.DrawRectangle(brushObservationLimits, null, new Rect(0, 0, from - to, ActualHeight / 2));
                }
            }

            // time grid
            for (int i = 0; i <= 24; i++)
            {
                double x = i / 24.0 * ActualWidth;
                GuidelineSet guidelines = new GuidelineSet();
                guidelines.GuidelinesX.Add(penHourLine.Thickness * 0.5);
                dc.PushGuidelineSet(guidelines);
                dc.DrawLine(penHourLine, new Point(x, 0), new Point(x, ActualHeight));
                dc.Pop();
            }


        }
    }
}
