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
    public class SolarEclipseLocalView : Control
    {
        public static readonly DependencyProperty CircumstancesProperty =
            DependencyProperty.Register(nameof(Circumstances), typeof(SolarEclipseLocalCircumstances), typeof(SolarEclipseLocalView), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });

        public SolarEclipseLocalCircumstances Circumstances
        {
            get => (SolarEclipseLocalCircumstances)GetValue(CircumstancesProperty);
            set => SetValue(CircumstancesProperty, value);
        }

        public SolarEclipseLocalView()
        {
            //Child = mapControl;
        }

        

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (Circumstances != null)
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

                drawingContext.DrawEllipse(Brushes.Yellow, null, pSun, solarRadius, solarRadius);


                // C1
                {
                    double dist = solarRadius + lunarRadius;
                    var pMoon = GetMoonCoordinates(pSun, Circumstances.PAnglePartialBegin, dist);
                    drawingContext.DrawEllipse(null, new Pen(Brushes.Gray, 1), pMoon, lunarRadius, lunarRadius);
                }

                //// C2
                //{
                //    double dist = Math.Abs(solarRadius - lunarRadius);
                //    var pMoon = GetMoonCoordinates(pSun, Circumstances.PosAngleTotalBegin, dist);
                //    drawingContext.DrawEllipse(null, new Pen(Brushes.Gray, 1), pMoon, lunarRadius, lunarRadius);
                //}

                //// C3
                //{
                //    double dist = Math.Abs(solarRadius - lunarRadius);
                //    var pMoon = GetMoonCoordinates(pSun, Circumstances.PosAngleTotalEnd, dist);
                //    drawingContext.DrawEllipse(null, new Pen(Brushes.Gray, 1), pMoon, lunarRadius, lunarRadius);
                //}

                // C4
                {
                    double dist = solarRadius + lunarRadius;
                    var pMoon = GetMoonCoordinates(pSun, Circumstances.PAnglePartialEnd, dist);
                    drawingContext.DrawEllipse(null, new Pen(Brushes.Gray, 1), pMoon, lunarRadius, lunarRadius);
                }

                // max
                {
                    double dist = (solarRadius + lunarRadius) - Circumstances.MaxMagnitude * (2 * solarRadius);
                    var pMoon = GetMoonCoordinates(pSun, Circumstances.PAngleMax, dist);
                    drawingContext.DrawEllipse(Brushes.Blue, null, pMoon, lunarRadius, lunarRadius);
                }


            }

            base.OnRender(drawingContext);
        }

        private Point GetMoonCoordinates(Point pSun, double posAngle, double dist)
        {
            double dx = dist * Math.Sin(Angle.ToRadians(posAngle + 180));
            double dy = dist * Math.Cos(Angle.ToRadians(posAngle + 180));
            var pMoon = new Point(pSun.X + dx, pSun.Y + dy);
            return pMoon;
        }
    }
}
