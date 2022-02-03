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

        private static void DepPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            VisibilityChart @this = (VisibilityChart)sender;
            @this.Interpolate();
        }

        public VisibilityChart() { }

        public CrdsGeographical GeoLocation
        {
            get { return (CrdsGeographical)GetValue(GeoLocationProperty); }
            set
            {
                SetValue(GeoLocationProperty, value);
            }
        }

        public double SiderealTime
        {
            get { return (double)GetValue(SiderealTimeProperty); }
            set
            {
                SetValue(SiderealTimeProperty, value);
            }
        }

        public CrdsEquatorial[] SunCoordinates
        {
            get { return (CrdsEquatorial[])GetValue(SunCoordinatesProperty); }
            set 
            {
                SetValue(SunCoordinatesProperty, value);
            }
        }

        public CrdsEquatorial[] BodyCoordinates
        {
            get { return (CrdsEquatorial[])GetValue(BodyCoordinatesProperty); }
            set 
            { 
                SetValue(BodyCoordinatesProperty, value);
            }
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
            Brush brushBackground = Brushes.Black;
            Brush brushGround = Brushes.DarkGreen;
            Brush brushHourLine = Brushes.DarkSlateGray;
            Brush brushBodyLine = Brushes.Yellow;
            Color colorDaylight = Color.FromArgb(255, 119, 203, 255);
            Pen penHourLine = new Pen(brushHourLine, 1);
            Pen penBodyLine = new Pen(brushBodyLine, 1);

            var bounds = new Rect(0, 0, ActualWidth, ActualHeight);
            dc.PushClip(new RectangleGeometry(bounds));

            // background
            dc.DrawRectangle(brushBackground, null, bounds);

            if (sunCoordinatesInterpolated != null)
            {
                double sunAltitudesCount = sunCoordinatesInterpolated.Length;
                for (int i = 0; i < sunCoordinatesInterpolated.Length; i++)
                {
                    double x0 = i / sunAltitudesCount * ActualWidth - 0.5;
                    double x1 = (i + 1) / sunAltitudesCount * ActualWidth + 0.5;

                    // -18 degrees is an astronomical night
                    double transp = sunCoordinatesInterpolated[i].Altitude <= -18 ? 0 : (sunCoordinatesInterpolated[i].Altitude < 0 ? (sunCoordinatesInterpolated[i].Altitude + 18) / 18.0 : 1);

                    Color c = Color.FromArgb((byte)(transp * 255), colorDaylight.R, colorDaylight.G, colorDaylight.B);

                    SolidColorBrush brush = new SolidColorBrush(c);

                    for (int j = 0; j < 2; j++)
                    {
                        double _x0 = x0 + (j == 0 ? -1 : 1) * ActualWidth / 2;
                        double _x1 = x1 + (j == 0 ? -1 : 1) * ActualWidth / 2;
                        dc.DrawRectangle(brush, null, new Rect(_x0, 0, _x1 - _x0, ActualHeight));
                    }
                }
            }

            dc.DrawRectangle(brushGround, null, new Rect(0, ActualHeight / 2, ActualWidth, ActualHeight / 2));

            // time grid
            for (int i = 0; i <= 24; i++)
            {
                double x = i / 24.0 * ActualWidth; 
                dc.DrawLine(penHourLine, new Point(x, 0), new Point(x, ActualHeight));
            }

            if (bodyCoordinatesInterpolated != null)
            {
                // body altitude line
                var figure = new PathFigure();

                for (int j = 0; j < 2; j++)
                {
                    for (int i = 0; i <= bodyCoordinatesInterpolated.Length; i++)
                    {
                        double x = i / (double)bodyCoordinatesInterpolated.Length * ActualWidth;
                        double alt = i == bodyCoordinatesInterpolated.Length ? bodyCoordinatesInterpolated[0].Altitude : bodyCoordinatesInterpolated[i].Altitude;
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
                dc.DrawGeometry(null, penBodyLine, geometry);
            }


        }
    }
}
