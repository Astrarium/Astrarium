using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
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
           DependencyProperty.Register(nameof(Orientation), typeof(SolarEclipseChartOrientation), typeof(SolarEclipseChartControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = true, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, DefaultValue = SolarEclipseChartOrientation.Zenithal });

        public static readonly DependencyProperty ContactProperty =
           DependencyProperty.Register(nameof(Contact), typeof(SolarEclipseChartContact), typeof(SolarEclipseChartControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = true, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, DefaultValue = SolarEclipseChartContact.Max });

        public static readonly DependencyProperty ZoomLevelProperty =
           DependencyProperty.Register(nameof(ZoomLevel), typeof(float), typeof(SolarEclipseChartControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = true, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, DefaultValue = (float)1 });

        public static readonly DependencyProperty ShowLabelsProperty =
           DependencyProperty.Register(nameof(ShowLabels), typeof(bool), typeof(SolarEclipseChartControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = true, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, DefaultValue = true });

        public static readonly DependencyProperty ShowContactCirclesProperty =
           DependencyProperty.Register(nameof(ShowContactCircles), typeof(bool), typeof(SolarEclipseChartControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = true, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, DefaultValue = true });

        public static readonly DependencyProperty ShowMoonOutlineOnlyProperty =
           DependencyProperty.Register(nameof(ShowMoonOutlineOnly), typeof(bool), typeof(SolarEclipseChartControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = true, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, DefaultValue = false });

        public static readonly DependencyProperty DarkModeProperty =
           DependencyProperty.Register(nameof(DarkMode), typeof(bool), typeof(SolarEclipseChartControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, DefaultValue = false });

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

        /// <summary>
        /// Zoom level 
        /// </summary>
        public float ZoomLevel
        {
            get => (float)GetValue(ZoomLevelProperty);
            set => SetValue(ZoomLevelProperty, value);
        }

        /// <summary>
        /// Display labels on the chart
        /// </summary>
        public bool ShowLabels
        {
            get => (bool)GetValue(ShowLabelsProperty);
            set => SetValue(ShowLabelsProperty, value);
        }

        /// <summary>
        /// If flag is set, contact circles are shown
        /// </summary>
        public bool ShowContactCircles
        {
            get => (bool)GetValue(ShowContactCirclesProperty);
            set => SetValue(ShowContactCirclesProperty, value);
        }

        /// <summary>
        /// If set, Moon displayed as oultine circle without filling
        /// </summary>
        public bool ShowMoonOutlineOnly
        {
            get => (bool)GetValue(ShowMoonOutlineOnlyProperty);
            set => SetValue(ShowMoonOutlineOnlyProperty, value);
        }

        /// <summary>
        /// If set, dark mode is used
        /// </summary>
        public bool DarkMode
        {
            get => (bool)GetValue(DarkModeProperty);
            set => SetValue(DarkModeProperty, value);
        }

        private Typeface font = new Typeface(new FontFamily("#Noto Sans"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

        private Brush[] foregroundBrush = new Brush[] { Brushes.Gray, Brushes.Red };
        private Brush ForegroundBrush => foregroundBrush[DarkMode ? 1 : 0];

        private Brush[] linesBrush = new Brush[] { Brushes.DimGray, Brushes.DarkRed };
        private Brush LinesBrush => linesBrush[DarkMode ? 1 : 0];

        private Brush[] moonBrush = new Brush[] { Brushes.Blue, Brushes.DarkRed };
        private Brush MoonBrush => moonBrush[DarkMode ? 1 : 0];

        private Brush[] sunBrush = new Brush[] { Brushes.Yellow, Brushes.Red };
        private Brush SunBrush => sunBrush[DarkMode ? 1 : 0];

        private Brush[] horizonBrush = new Brush[] { Brushes.Green, Brushes.DarkRed };
        private Brush HorizonBrush => horizonBrush[DarkMode ? 1 : 0];

        protected override void OnRender(DrawingContext ctx)
        {
            var pSun = new Point(ActualWidth / 2, ActualHeight / 2);

            if (Circumstances != null && !Circumstances.IsInvisible)
            {
                double solarRadius = ZoomLevel * Math.Min(ActualWidth, ActualHeight) / 6;
                double lunarRadius = solarRadius * Circumstances.MoonToSunDiameterRatio;

                var bounds = new Rect(0, 0, ActualWidth, ActualHeight);
                ctx.PushClip(new RectangleGeometry(bounds));
                ctx.DrawEllipse(SunBrush, null, pSun, solarRadius, solarRadius);

                Point pC1;
                {
                    double dist = solarRadius + lunarRadius;
                    double angle = GetOrientationAngle(Circumstances.PartialBegin);
                    pC1 = GetMoonCoordinates(pSun, angle, dist);
                }

                Point pC2;
                {
                    double dist = Math.Abs(solarRadius - lunarRadius);
                    double angle = GetOrientationAngle(Circumstances.TotalBegin);
                    pC2 = GetMoonCoordinates(pSun, angle, dist);
                }

                Point pMax;
                {
                    double dist = (solarRadius + lunarRadius) - Circumstances.MaxMagnitude * (2 * solarRadius);
                    double angle = GetOrientationAngle(Circumstances.Maximum);
                    pMax = GetMoonCoordinates(pSun, angle, dist);
                }

                Point pC3;
                {
                    double dist = Math.Abs(solarRadius - lunarRadius);
                    double angle = GetOrientationAngle(Circumstances.TotalEnd);
                    pC3 = GetMoonCoordinates(pSun, angle, dist);
                }

                Point pC4;
                {
                    double dist = solarRadius + lunarRadius;
                    double angle = GetOrientationAngle(Circumstances.PartialEnd);
                    pC4 = GetMoonCoordinates(pSun, angle, dist);
                }

                if (ShowContactCircles)
                {
                    ctx.DrawEllipse(null, new Pen(LinesBrush, 1), pC1, lunarRadius, lunarRadius);
                    //if (Circumstances.TotalBegin != null)
                    //    ctx.DrawEllipse(null, new Pen(LinesBrush, 1), pC2, lunarRadius, lunarRadius);
                    ctx.DrawEllipse(null, new Pen(LinesBrush, 1), pMax, lunarRadius, lunarRadius);
                    //if (Circumstances.TotalEnd != null)
                    //    ctx.DrawEllipse(null, new Pen(LinesBrush, 1), pC3, lunarRadius, lunarRadius);
                    ctx.DrawEllipse(null, new Pen(LinesBrush, 1), pC4, lunarRadius, lunarRadius);
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

                    if (ShowMoonOutlineOnly)
                    {
                        ctx.DrawEllipse(null, new Pen(MoonBrush, 1.5), pMoon, lunarRadius, lunarRadius);
                    }
                    else
                    {
                        ctx.DrawEllipse(MoonBrush, null, pMoon, lunarRadius, lunarRadius);
                    }

                    if (ShowLabels)
                    {
                        if (ShowContactCircles || contact == SolarEclipseChartContact.C1)
                            DrawText(ctx, "C1", pC1, 10);

                        if (ShowContactCircles || contact == SolarEclipseChartContact.Max)
                            DrawText(ctx, "Max", pMax, 10);

                        if (ShowContactCircles || contact == SolarEclipseChartContact.C4)
                            DrawText(ctx, "C4", pC4, 10);
                    }

                    // Draw horizon
                    Point[] p = new Point[4];

                    if (Orientation == SolarEclipseChartOrientation.Zenithal)
                    {
                        q = 0;
                    }

                    // Calculate points needed to draw horizon
                    // i=0,1 is a horizon line itself,
                    // i=2,3 is a line 20 degrees below horizon
                    // 0.25 is a mean solar radius in degrees (more accurate value is not needed)
                    for (int i = 0; i < 2; i++)
                    {
                        double d = (alt + (i == 0 ? 0 : 20)) / 0.25 * solarRadius;
                        double ang = Angle.ToRadians(q);
                        double dx = d * Math.Sin(ang);
                        double dy = d * Math.Cos(ang);
                        Point p0 = new Point(pSun.X + dx, pSun.Y + dy);
                        double r = 2 * Math.Sqrt(ActualHeight * ActualHeight + ActualWidth * ActualWidth);
                        dx = r * Math.Sin(ang);
                        dy = r * Math.Cos(ang);
                        p[2 * i] = new Point(p0.X - dy, p0.Y + dx);
                        p[2 * i + 1] = new Point(p0.X + dy, p0.Y - dx);
                    }

                    StreamGeometry g = new StreamGeometry();
                    using (var gc = g.Open())
                    {
                        gc.BeginFigure(p[0], true, true);
                        gc.LineTo(p[1], true, true);
                        gc.LineTo(p[3], true, true);
                        gc.LineTo(p[2], true, true);
                    }

                    ctx.PushOpacity(0.5);
                    ctx.DrawGeometry(HorizonBrush, null, g);
                }

                ctx.Pop();
            }
            else
            {
                DrawText(ctx, Text.Get("EclipseView.ChartInvisible"), pSun, 12);
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
            double ang = Angle.ToRadians(posAngle + 180);
            double dx = dist * Math.Sin(ang);
            double dy = dist * Math.Cos(ang);
            var pMoon = new Point(pSun.X + dx, pSun.Y + dy);
            return pMoon;
        }

        private void DrawText(DrawingContext ctx, string text, Point point, double size)
        {
            FormattedText formattedText = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, font, size, ForegroundBrush);
            ctx.DrawText(formattedText, new Point(point.X - formattedText.Width / 2, point.Y - formattedText.Height / 2));
        }
    }

    /// <summary>
    /// Defines chart orientation
    /// </summary>
    public enum SolarEclipseChartOrientation
    {
        /// <summary>
        /// Top of the chart points on Zenith
        /// </summary>
        Zenithal,

        /// <summary>
        /// Top of the chart point os North
        /// </summary>
        Equatorial
    }

    /// <summary>
    /// Defines main contacts instants of an eclipse
    /// </summary>
    public enum SolarEclipseChartContact
    {
        /// <summary>
        /// First external contact (Moon disk touches Sun first time)
        /// </summary>
        C1,

        /// <summary>
        /// First internal contact (beginning of total or annular phase)
        /// </summary>
        C2,

        /// <summary>
        /// Instant of eclipse maximum
        /// </summary>
        Max,

        /// <summary>
        /// Last internal contact (end of total or annular phase)
        /// </summary>
        C3,

        /// <summary>
        /// Last external contact (Moon disk goes out from the Sun)
        /// </summary>
        C4
    }
}
