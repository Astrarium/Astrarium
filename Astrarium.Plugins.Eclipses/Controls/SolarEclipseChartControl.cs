using Astrarium.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Media;

namespace Astrarium.Plugins.Eclipses.Controls
{
    /// <summary>
    /// Visually shows local circumstances of Solar eclipse
    /// </summary>
    public class SolarEclipseChartControl : Control
    {
        public static readonly DependencyProperty CircumstancesProperty =
            DependencyProperty.Register(nameof(Circumstances), typeof(SolarEclipseLocalCircumstances), typeof(SolarEclipseChartControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });

        public static readonly DependencyProperty OrientationProperty =
           DependencyProperty.Register(nameof(Orientation), typeof(SolarEclipseChartOrientation), typeof(SolarEclipseChartControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = true, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, DefaultValue = SolarEclipseChartOrientation.Equatorial });

        public static readonly DependencyProperty ContactProperty =
           DependencyProperty.Register(nameof(Contact), typeof(SolarEclipseChartContact), typeof(SolarEclipseChartControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = true, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, DefaultValue = SolarEclipseChartContact.Max });

        /// <summary>
        /// Local curcumstances of the eclipse
        /// </summary>
        public SolarEclipseLocalCircumstances Circumstances
        {
            get => (SolarEclipseLocalCircumstances)GetValue(CircumstancesProperty);
            set => SetValue(CircumstancesProperty, value);
        }

        /// <summary>
        /// Chart orientation: zenithal or equatorial
        /// </summary>
        public SolarEclipseChartOrientation Orientation
        {
            get => (SolarEclipseChartOrientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        /// <summary>
        /// Contact instant to be displayed on the chart
        /// </summary>
        public SolarEclipseChartContact Contact
        {
            get => (SolarEclipseChartContact)GetValue(ContactProperty);
            set => SetValue(ContactProperty, value);
        }

        protected override void OnRender(DrawingContext ctx)
        {
            if (Circumstances != null && !Circumstances.IsInvisible)
            {
                var pSun = new Point(ActualWidth / 2, ActualHeight / 2);

                double solarRadius = Math.Min(ActualWidth, ActualHeight) / 6;
                double lunarRadius = solarRadius * Circumstances.MoonToSunDiameterRatio;



                /*
                FontFamily courier = new FontFamily("Courier New");

                Typeface courierTypeface = new Typeface(courier, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

                FormattedText ft2 = new FormattedText(dist.ToString("F5"),
                                                     System.Globalization.CultureInfo.CurrentCulture,
                                                     FlowDirection.LeftToRight,
                                                     courierTypeface,
                                                     14.0,
                                                     Brushes.White);

                drawingContext.DrawText(ft2, new Point(0, 0));
*/

                ctx.DrawEllipse(Brushes.Yellow, null, pSun, solarRadius, solarRadius);


                // C1
                {
                    double dist = solarRadius + lunarRadius;
                    double angle = GetOrientationAngle(Circumstances.PartialBegin);
                    var pMoon = GetMoonCoordinates(pSun, angle, dist);
                    ctx.DrawEllipse(null, new Pen(Brushes.Gray, 1), pMoon, lunarRadius, lunarRadius);
                }

                // Max
                {
                    double dist = (solarRadius + lunarRadius) - Circumstances.MaxMagnitude * (2 * solarRadius);
                    double angle = GetOrientationAngle(Circumstances.Maximum);
                    var pMoon = GetMoonCoordinates(pSun, angle, dist);
                    ctx.DrawEllipse(null, new Pen(Brushes.Gray, 1), pMoon, lunarRadius, lunarRadius);
                }

                // C4
                {
                    double dist = solarRadius + lunarRadius;
                    double angle = GetOrientationAngle(Circumstances.PartialEnd);
                    var pMoon = GetMoonCoordinates(pSun, angle, dist);
                    ctx.DrawEllipse(null, new Pen(Brushes.Gray, 1), pMoon, lunarRadius, lunarRadius);
                }

                {
                    double dist = 0, angle = 0, q = 0, alt = 0;
                    var contact = Contact;
                    if (contact == SolarEclipseChartContact.C2 && Circumstances.TotalBegin == null)
                    {
                        contact = SolarEclipseChartContact.Max;
                    }
                    if (contact == SolarEclipseChartContact.C3 && Circumstances.TotalEnd == null)
                    {
                        contact = SolarEclipseChartContact.Max;
                    }

                    Contact = contact;

                    switch (contact)
                    {
                        case SolarEclipseChartContact.C1:
                            dist = solarRadius + lunarRadius;
                            angle = GetOrientationAngle(Circumstances.PartialBegin);
                            alt = Circumstances.PartialBegin.SolarAltitude;
                            q = Circumstances.PartialBegin.QAngle;
                            break;
                        case SolarEclipseChartContact.C2:
                            dist = Math.Abs(solarRadius - lunarRadius);
                            angle = GetOrientationAngle(Circumstances.TotalBegin);
                            alt = Circumstances.TotalBegin.SolarAltitude;
                            q = Circumstances.TotalBegin.QAngle;
                            break;
                        case SolarEclipseChartContact.Max:
                            dist = (solarRadius + lunarRadius) - Circumstances.MaxMagnitude * (2 * solarRadius);
                            angle = GetOrientationAngle(Circumstances.Maximum);
                            alt = Circumstances.Maximum.SolarAltitude;
                            q = Circumstances.Maximum.QAngle;
                            break;
                        case SolarEclipseChartContact.C3:
                            dist = Math.Abs(solarRadius - lunarRadius);
                            angle = GetOrientationAngle(Circumstances.TotalEnd);
                            alt = Circumstances.TotalEnd.SolarAltitude;
                            q = Circumstances.TotalEnd.QAngle;
                            break;
                        case SolarEclipseChartContact.C4:
                            dist = solarRadius + lunarRadius;
                            angle = GetOrientationAngle(Circumstances.PartialEnd);
                            alt = Circumstances.PartialEnd.SolarAltitude;
                            q = Circumstances.PartialEnd.QAngle;
                            break;
                    }
                    var pMoon = GetMoonCoordinates(pSun, angle, dist);
                    ctx.DrawEllipse(Brushes.Blue, null, pMoon, lunarRadius, lunarRadius);

                    // Draw horizon
                    dist = (alt / 0.25) * solarRadius;
                    if (Orientation == SolarEclipseChartOrientation.Zenithal)
                    {
                        double yHorizon = pSun.Y + dist;
                        ctx.PushOpacity(0.5);
                        ctx.PushClip(new RectangleGeometry(new Rect(0, 0, ActualWidth, ActualHeight)));
                        ctx.DrawRectangle(Brushes.Green, null, new Rect(new Point(0, yHorizon), new Point(ActualWidth, ActualHeight)));
                        ctx.Pop();
                    }
                    else
                    {
                        Point p1, p2, p3, p4;

                        {
                            dist = (alt / 0.25) * solarRadius;
                            double dx = dist * Math.Sin(Angle.ToRadians(q));
                            double dy = dist * Math.Cos(Angle.ToRadians(q));

                            Point p0 = new Point(pSun.X + dx, pSun.Y + dy);

                            double r = 2 * Math.Sqrt(ActualHeight * ActualHeight + ActualWidth * ActualWidth);

                            dx = r * Math.Sin(Angle.ToRadians(q));
                            dy = r * Math.Cos(Angle.ToRadians(q));

                            p1 = new Point(p0.X - dy, p0.Y + dx);
                            p2 = new Point(p0.X + dy, p0.Y - dx);
                        }

                        {
                            dist = ((alt + 20) / 0.25) * solarRadius;
                            double dx = dist * Math.Sin(Angle.ToRadians(q));
                            double dy = dist * Math.Cos(Angle.ToRadians(q));

                            Point p0 = new Point(pSun.X + dx, pSun.Y + dy);

                            double r = 2 * Math.Sqrt(ActualHeight * ActualHeight + ActualWidth * ActualWidth);

                            dx = r * Math.Sin(Angle.ToRadians(q));
                            dy = r * Math.Cos(Angle.ToRadians(q));

                            p3 = new Point(p0.X + dy, p0.Y - dx);
                            p4 = new Point(p0.X - dy, p0.Y + dx);                            
                        }


                        

                        //var pts = LineScreenIntersection(p1, p2);
                        //if (pts.Length == 2)
                        //{
                        //    ctx.DrawLine(new Pen(Brushes.Green, 2), pts[0], pts[1]);


                        
                        StreamGeometry g = new StreamGeometry();
                        using (var gc = g.Open())
                        {
                            gc.BeginFigure(p1, true, true);
                            gc.LineTo(p2, true, true);
                            gc.LineTo(p3, true, true);
                            gc.LineTo(p4, true, true);


                        }

                        ctx.PushOpacity(0.5);
                        ctx.PushClip(new RectangleGeometry(new Rect(0, 0, ActualWidth, ActualHeight)));
                        //ctx.DrawLine(new Pen(Brushes.Green, 2), p1, p2);
                        ctx.DrawGeometry(Brushes.Green, null, g);
                        ctx.Pop();
                        
                        //}


                    }
                }

                

            }

            base.OnRender(ctx);
        }

        private double GetOrientationAngle(SolarEclipseLocalCircumstancesContactPoint point)
        {
            return point != null ? 
                (Orientation == SolarEclipseChartOrientation.Equatorial ? point.PAngle : point.ZAngle) : 0;
        }

        private Point GetMoonCoordinates(Point pSun, double posAngle, double dist)
        {
            double dx = dist * Math.Sin(Angle.ToRadians(posAngle + 180));
            double dy = dist * Math.Cos(Angle.ToRadians(posAngle + 180));
            var pMoon = new Point(pSun.X + dx, pSun.Y + dy);
            return pMoon;
        }

        public Point[] LineScreenIntersection(Point p1, Point p2)
        {
            double width = ActualWidth;
            double height = ActualHeight;

            Point p00 = new Point(0, 0);
            Point pW0 = new Point(width, 0);
            Point pWH = new Point(width, height);
            Point p0H = new Point(0, height);

            List<Point> crosses = new List<Point>();

            Point c1 = LinesIntersection(p1, p2, p00, pW0);
            if (Math.Abs(c1.Y) < 1 && c1.X >= 0 && c1.X <= width)
            {
                crosses.Add(c1);
            }

            Point c2 = LinesIntersection(p1, p2, pW0, pWH);
            if (Math.Abs(c2.X - width) < 1 && c2.Y >= 0 && c2.Y <= height)
            {
                crosses.Add(c2);
            }

            Point c3 = LinesIntersection(p1, p2, p0H, pWH);
            if (Math.Abs(c3.Y - height) < 1 && c3.X >= 0 && c3.X <= width)
            {
                crosses.Add(c3);
            }

            Point c4 = LinesIntersection(p1, p2, p00, p0H);
            if (Math.Abs(c4.X) < 1 && c4.Y >= 0 && c4.Y <= height)
            {
                crosses.Add(c4);
            }

            return crosses.ToArray();
        }

        private Point LinesIntersection(Point p1, Point p2, Point p3, Point p4)
        {
            double x1 = p1.X;
            double x2 = p2.X;
            double x3 = p3.X;
            double x4 = p4.X;

            double y1 = p1.Y;
            double y2 = p2.Y;
            double y3 = p3.Y;
            double y4 = p4.Y;

            double x = ((x1 * y2 - y1 * x2) * (x3 - x4) - (x1 - x2) * (x3 * y4 - y3 * x4)) / ((x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4));
            double y = ((x1 * y2 - y1 * x2) * (y3 - y4) - (y1 - y2) * (x3 * y4 - y3 * x4)) / ((x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4));

            return new Point() { X = x, Y = y };
        }
    }

    public enum SolarEclipseChartOrientation
    {
        Zenithal,
        Equatorial
    }

    public enum SolarEclipseChartContact
    {
        C1,
        C2,
        Max,
        C3,
        C4
    }
}
