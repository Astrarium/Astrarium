using ADK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
//using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Planetarium.Controls
{
    public class EarthMapCanvas : Canvas
    {
        public readonly static DependencyProperty SunHourAngleProperty = DependencyProperty.Register(nameof(SunHourAngle), typeof(double), typeof(EarthMapCanvas), new FrameworkPropertyMetadata(null) { AffectsRender = true });
        public readonly static DependencyProperty ObserverLocationProperty = DependencyProperty.Register(nameof(ObserverLocation), typeof(CrdsGeographical), typeof(EarthMapCanvas), new FrameworkPropertyMetadata(new CrdsGeographical(0, 0), null) { AffectsRender = true });
        public readonly static DependencyProperty SunEquatorialProperty = DependencyProperty.Register(nameof(SunDeclination), typeof(double), typeof(EarthMapCanvas), new FrameworkPropertyMetadata(null) { AffectsRender = true });

        private double MaxScale = 20;
        private double Zoom = 1;
        private double OriginX = 0;
        private double OriginY = 0;

        private BitmapSource earthMap;
        private Point lastMousePosition;
        private Pen scrollerPen = new Pen(new SolidColorBrush(Color.FromArgb(100, 0, 0, 0)), 6);
        private Pen transparentPen = new Pen(Brushes.Transparent, 0);

        private Pen locationPen = new Pen(Brushes.Red, 1);
        private Pen crossingPen = new Pen(Brushes.Brown, 1);

        private SolidColorBrush sunBrush = new SolidColorBrush(Color.FromRgb(250, 210, 10));
        private SolidColorBrush nightBrush = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0));

        private double ScaledWidth { get => ActualWidth * Zoom; }
        private double ScaledHeight { get => ActualHeight * Zoom; }

        public double SunHourAngle
        {
            get { return (double)GetValue(SunHourAngleProperty); }
            set { SetValue(SunHourAngleProperty, value); }
        }

        public CrdsGeographical ObserverLocation
        {
            get { return (CrdsGeographical)GetValue(ObserverLocationProperty); }
            set { SetValue(ObserverLocationProperty, value); }
        }

        public double SunDeclination
        {
            get { return (double)GetValue(SunEquatorialProperty); }
            set { SetValue(SunEquatorialProperty, value); }
        }

        public EarthMapCanvas()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                Uri uri = new Uri(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data", "Earth.jpg"));
                BitmapDecoder dec = BitmapDecoder.Create(uri, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);                
                earthMap = dec.Frames[0];
                earthMap.Freeze();
                dec = null;

                Loaded += (s, e) => { // only at this point the control is ready
                    Window.GetWindow(this) // get the parent window
                          .Closing += (s1, e1) => Dispose(); //disposing logic here
                };
            }
          
            ClipToBounds = true;
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);
            FocusVisualStyle = null;
            Focus();
        }

        private void Dispose()
        {
            earthMap = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        protected override void OnRender(DrawingContext dc)
        {
            if (earthMap != null)
            {
                dc.DrawImage(earthMap, new Rect(OriginX, OriginY, ScaledWidth, ScaledHeight));

                if (Zoom > 1)
                {
                    double t = scrollerPen.Thickness;

                    double x = (OriginX + ScaledWidth - ActualWidth) / Zoom;
                    double w = ActualWidth / Zoom;
                    dc.DrawLine(scrollerPen, new Point(ActualWidth - x, ActualHeight - t / 2), new Point(ActualWidth - (x + w), ActualHeight - t / 2));

                    double y = (OriginY + ScaledHeight - ActualHeight) / Zoom;
                    double h = ActualHeight / Zoom;
                    dc.DrawLine(scrollerPen, new Point(ActualWidth - t / 2, ActualHeight - y), new Point(ActualWidth - t / 2, ActualHeight - (y + h)));
                }

                dc.PushTransform(new TranslateTransform(OriginX, OriginY));

                DrawDayNight(dc);
                DrawSubSolarPoint(dc);
                DrawLocation(dc);

                dc.Pop();
            }
        }

        /// <summary>
        /// Draws Day & Night terminator and night part of the Earth map
        /// </summary>
        private void DrawDayNight(DrawingContext dc)
        {
            double K = Math.PI / 180.0;
            double tanLat, arctanLat;

            double y0 = 90;
            double x0 = 180;

            double y1, y2;
            double longitude;

            PointCollection points = new PointCollection();
            if (SunDeclination >= 0)
            {
                points.Add(new Point(0, ScaledHeight));
                points.Add(new Point(0, 0));
            }
            else
            {
                points.Add(new Point(0, 0));
                points.Add(new Point(0, ScaledHeight));
            }

            for (int i = -180; i <= 180; i++)
            {
                longitude = i + SunHourAngle;
                tanLat = -Math.Cos(longitude * K) / Math.Tan(SunDeclination * K);
                arctanLat = Math.Atan(tanLat) / K;
                y1 = y0 - arctanLat;

                longitude = longitude + 1;
                tanLat = -Math.Cos(longitude * K) / Math.Tan(SunDeclination * K);
                arctanLat = Math.Atan(tanLat) / K;
                y2 = y0 - arctanLat;

                double _x1 = (x0 + i) / 360.0 * ScaledWidth;
                double _y1 = y1 / 180.0 * ScaledHeight;
                double _x2 = (x0 + i + 1) / 360.0 * ScaledWidth;
                double _y2 = y2 / 180.0 * ScaledHeight;

                points.Add(new Point(_x1, _y1));
                points.Add(new Point(_x2, _y2));
            }

            if (SunDeclination >= 0)
            {
                points.Add(new Point(ScaledWidth, 0));
                points.Add(new Point(ScaledWidth, ScaledHeight));
            }
            else
            {
                points.Add(new Point(ScaledWidth, ScaledHeight));
                points.Add(new Point(ScaledWidth, 0));
            }

            var g = new StreamGeometry();
            using (StreamGeometryContext gc = g.Open())
            {
                gc.BeginFigure(points[0], true, true);
                gc.PolyBezierTo(points, true, true);
            }

            dc.DrawGeometry(nightBrush, transparentPen, g);
        }

        /// <summary>
        /// Draws sub-solar point on the map
        /// </summary>
        private void DrawSubSolarPoint(DrawingContext dc)
        {
            float y0 = 90;
            float x0 = 180;
            float radius = 5;

            var p = new Point();
            p.X = (x0 - SunHourAngle) / 360.0 * ScaledWidth;
            p.Y = (y0 - SunDeclination) / 180.0 * ScaledHeight;

            if (p.X > ScaledWidth) p.X -= ScaledWidth;
            if (p.X < 0) p.X += ScaledWidth;

            dc.DrawEllipse(sunBrush, transparentPen, p, radius, radius);
        }

        /// <summary>
        /// Draws current observer location point and crossing lines on the map
        /// </summary>
        private void DrawLocation(DrawingContext dc)
        {
            double x, y;
            x = (180 - ObserverLocation.Longitude) / 360.0 * ScaledWidth;
            y = (90 - ObserverLocation.Latitude) / 180.0 * ScaledHeight;
            dc.DrawEllipse(Brushes.Transparent, locationPen, new Point(x, y), 5, 5);
            dc.DrawLine(crossingPen, new Point(x, 0), new Point(x, ScaledHeight));
            dc.DrawLine(crossingPen, new Point(0, y), new Point(ScaledWidth, y));
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            double v = Zoom;
            int delta = e.Delta;

            if (delta > 0)
            {
                v *= 1.1;
            }
            else
            {
                v /= 1.1;
            }

            if (v >= MaxScale)
            {
                v = MaxScale;
            }
            if (v < 1)
            {
                v = 1;
            }

            var position = e.GetPosition(this);

            double k = v / Zoom;

            Zoom = v;

            OriginX = position.X - k * (position.X - OriginX);
            OriginY = position.Y - k * (position.Y - OriginY);

            AdjustPosition();
            InvalidateVisual();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            lastMousePosition = e.GetPosition(this);

            if (e.ClickCount == 2)
            {
                ChangeObserverLocation(lastMousePosition);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var position = e.GetPosition(this);

                double dx = position.X - lastMousePosition.X;
                double dy = position.Y - lastMousePosition.Y;

                OriginX += dx;
                OriginY += dy;

                AdjustPosition();

                lastMousePosition = position;

                InvalidateVisual();
            }
        }

        private void AdjustPosition()
        {
            if (OriginX > 0) OriginX = 0;
            if (OriginY > 0) OriginY = 0;

            if (OriginX < -ActualWidth * Zoom + ActualWidth) OriginX = -ActualWidth * Zoom + ActualWidth;
            if (OriginY < -ActualHeight * Zoom + ActualHeight) OriginY = -ActualHeight * Zoom + ActualHeight;
        }

        private void ChangeObserverLocation(Point p)
        {
            double Lon, Lat;
            double x = p.X - OriginX;
            double y = p.Y - OriginY;

            if (x < 0) x = 0;
            if (x > ScaledWidth) x = ScaledWidth;
            if (y < 0) y = 0;
            if (y > ScaledHeight) y = ScaledHeight;

            Lon = 180 - 360.0 / ScaledWidth * x;
            Lat = 90 - 180.0 / ScaledHeight * y;

            if (Lon == -180) Lon += 1.0 / 3600.0;
            if (Lon == 180) Lon -= 1.0 / 3600.0;
            if (Lat == -90) Lat += 1.0 / 3600.0;
            if (Lat == 90) Lat -= 1.0 / 3600.0;

            ObserverLocation = new CrdsGeographical(Lat, Lon, ObserverLocation.UtcOffset, ObserverLocation.Elevation);

            InvalidateVisual();
        }
    }
}
