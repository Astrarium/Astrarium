using Astrarium.Algorithms;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Astrarium.Plugins.Eclipses.Controls
{
    /// <summary>
    /// Visually shows local circumstances of Lunar eclipse
    /// </summary>
    public class LunarEclipseChartControl : Control
    {
        public static readonly DependencyProperty CircumstancesProperty =
            DependencyProperty.Register(nameof(Circumstances), typeof(LunarEclipseLocalCircumstances), typeof(LunarEclipseChartControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });

        public static readonly DependencyProperty OrientationProperty =
           DependencyProperty.Register(nameof(Orientation), typeof(LunarEclipseChartOrientation), typeof(LunarEclipseChartControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = true, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, DefaultValue = LunarEclipseChartOrientation.Zenithal });

        public static readonly DependencyProperty ContactProperty =
           DependencyProperty.Register(nameof(Contact), typeof(LunarEclipseChartContact), typeof(LunarEclipseChartControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = true, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, DefaultValue = LunarEclipseChartContact.Max });

        public static readonly DependencyProperty ZoomLevelProperty =
           DependencyProperty.Register(nameof(ZoomLevel), typeof(float), typeof(LunarEclipseChartControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = true, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, DefaultValue = (float)1 });

        public static readonly DependencyProperty ShowLabelsProperty =
           DependencyProperty.Register(nameof(ShowLabels), typeof(bool), typeof(LunarEclipseChartControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = true, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, DefaultValue = true });

        public static readonly DependencyProperty ShowContactCirclesProperty =
           DependencyProperty.Register(nameof(ShowContactCircles), typeof(bool), typeof(LunarEclipseChartControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = true, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, DefaultValue = true });

        public static readonly DependencyProperty DarkModeProperty =
           DependencyProperty.Register(nameof(DarkMode), typeof(bool), typeof(LunarEclipseChartControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, DefaultValue = false });

        /// <summary>
        /// Chart orientation: zenithal or equatorial
        /// </summary>
        public SolarEclipseChartOrientation Orientation
        {
            get => (SolarEclipseChartOrientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        /// <summary>
        /// Local curcumstances of the eclipse
        /// </summary>
        public LunarEclipseLocalCircumstances Circumstances
        {
            get => (LunarEclipseLocalCircumstances)GetValue(CircumstancesProperty);
            set => SetValue(CircumstancesProperty, value);
        }

        /// <summary>
        /// Contact instant to be displayed on the chart
        /// </summary>
        public LunarEclipseChartContact Contact
        {
            get => (LunarEclipseChartContact)GetValue(ContactProperty);
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
        /// If set, dark mode is used
        /// </summary>
        public bool DarkMode
        {
            get => (bool)GetValue(DarkModeProperty);
            set => SetValue(DarkModeProperty, value);
        }

        private Typeface font = new Typeface(new FontFamily("#Noto Sans"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

        private Brush[] foregroundBrush = new Brush[] { Brushes.Gray, Brushes.DarkRed };
        private Brush ForegroundBrush => foregroundBrush[DarkMode ? 1 : 0];

        protected override void OnRender(DrawingContext ctx)
        {
            var pCenter = new Point(ActualWidth / 2, ActualHeight / 2);
            var bounds = new Rect(0, 0, ActualWidth, ActualHeight);
            ctx.PushClip(new RectangleGeometry(bounds));

            if (Circumstances != null && !Circumstances.IsInvisible)
            {
                var contact = Contact;
                if (contact == LunarEclipseChartContact.U1 && Circumstances.PartialBegin == null)
                {
                    contact = LunarEclipseChartContact.Max;
                }
                if (contact == LunarEclipseChartContact.U2 && Circumstances.TotalBegin == null)
                {
                    contact = LunarEclipseChartContact.Max;
                }
                if (contact == LunarEclipseChartContact.U3 && Circumstances.TotalEnd == null)
                {
                    contact = LunarEclipseChartContact.Max;
                }
                if (contact == LunarEclipseChartContact.U4 && Circumstances.PartialEnd == null)
                {
                    contact = LunarEclipseChartContact.Max;
                }
                Contact = contact;

                LunarEclipseLocalCircumstancesContactPoint contactPoint = null;
                int L = 0;

                switch (contact)
                {
                    case LunarEclipseChartContact.P1:
                        L = 3;
                        contactPoint = Circumstances.PenumbralBegin;
                        break;
                    case LunarEclipseChartContact.U1:
                        L = 2;
                        contactPoint = Circumstances.PartialBegin;
                        break;
                    case LunarEclipseChartContact.U2:
                        L = 1;
                        contactPoint = Circumstances.TotalBegin;
                        break;
                    case LunarEclipseChartContact.Max:
                        L = 0;
                        contactPoint = Circumstances.Maximum;
                        break;
                    case LunarEclipseChartContact.U3:
                        L = 1;
                        contactPoint = Circumstances.TotalEnd;
                        break;
                    case LunarEclipseChartContact.U4:
                        L = 2;
                        contactPoint = Circumstances.PartialEnd;
                        break;
                    case LunarEclipseChartContact.P4:
                        L = 3;
                        contactPoint = Circumstances.PenumbralEnd;
                        break;
                }

                double penumbraRadius = ZoomLevel * Math.Min(ActualWidth, ActualHeight) / 3;
                double umbraRadius = contactPoint.F2 / contactPoint.F1 * penumbraRadius;
                double moonRadius = contactPoint.F3 / contactPoint.F1 * penumbraRadius;

                // Central points of contacts
                Point pP1 = ContactPoint(Circumstances.PenumbralBegin, 3);
                Point pU1 = ContactPoint(Circumstances.PartialBegin, 2);
                Point pU2 = ContactPoint(Circumstances.TotalBegin, 1);
                Point pMax = ContactPoint(Circumstances.Maximum, 0);
                Point pU3 = ContactPoint(Circumstances.TotalEnd, 1);
                Point pU4 = ContactPoint(Circumstances.PartialEnd, 2);
                Point pP4 = ContactPoint(Circumstances.PenumbralEnd, 3);

                // Penumbra
                ctx.DrawEllipse(null, new Pen(Brushes.DimGray, 1), pCenter, penumbraRadius, penumbraRadius);

                // Umbra
                var umbraEllipse = new EllipseGeometry(pCenter, umbraRadius, umbraRadius);
                ctx.DrawGeometry(null, new Pen(Brushes.DimGray, 1), umbraEllipse);

                // Contact circles
                if (ShowContactCircles)
                {
                    ctx.DrawEllipse(null, new Pen(Brushes.DimGray, 1), pP1, moonRadius, moonRadius);

                    if (Circumstances.PartialBegin != null)
                        ctx.DrawEllipse(null, new Pen(Brushes.DimGray, 1), pU1, moonRadius, moonRadius);

                    if (Circumstances.TotalBegin != null)
                        ctx.DrawEllipse(null, new Pen(Brushes.DimGray, 1), pU2, moonRadius, moonRadius);

                    ctx.DrawEllipse(null, new Pen(Brushes.DimGray, 1), pMax, moonRadius, moonRadius);

                    if (Circumstances.TotalEnd != null)
                        ctx.DrawEllipse(null, new Pen(Brushes.DimGray, 1), pU3, moonRadius, moonRadius);

                    if (Circumstances.PartialEnd != null)
                        ctx.DrawEllipse(null, new Pen(Brushes.DimGray, 1), pU4, moonRadius, moonRadius);

                    ctx.DrawEllipse(null, new Pen(Brushes.DimGray, 1), pP4, moonRadius, moonRadius);
                }

                // Moon
                var pMoon = ContactPoint(contactPoint, L);// new Point(pCenter.X - contactPoint.X / contactPoint.F1 * penumbraRadius, pCenter.Y - contactPoint.Y / contactPoint.F1 * penumbraRadius);
                var moonEllipse = new EllipseGeometry(pMoon, moonRadius, moonRadius);
                ctx.DrawGeometry(Brushes.Gray, null, moonEllipse);

                // Totally eclipsed part of the Moon
                var totalityRegion = new CombinedGeometry(GeometryCombineMode.Intersect, umbraEllipse, moonEllipse);
                if (!totalityRegion.IsEmpty())
                {
                    ctx.DrawGeometry(Brushes.Brown, null, totalityRegion);
                }

                // Labels
                if (ShowContactCircles && ShowLabels)
                {
                    DrawText(ctx, "P1", pP1, 10);
                    if (Circumstances.PartialBegin != null)
                    {
                        DrawText(ctx, "U1", pU1, 10);
                    }
                    if (Circumstances.TotalBegin != null && Dist(pU2, pMax) > 20)
                    {
                        DrawText(ctx, "U2", pU2, 10);
                    }
                    DrawText(ctx, "Max", pMax, 10);
                    if (Circumstances.TotalEnd != null && Dist(pU3, pMax) > 20)
                    {
                        DrawText(ctx, "U3", pU3, 10);
                    }
                    if (Circumstances.PartialEnd != null)
                    {
                        DrawText(ctx, "U4", pU4, 10);
                    }
                    DrawText(ctx, "P4", pP4, 10);
                }

                // Set chart rotation
                ctx.PushTransform(new RotateTransform(Orientation == SolarEclipseChartOrientation.Equatorial ? -contactPoint.QAngle : 0, pMoon.X, pMoon.Y));

                // Draw horizon
                Point[] p = new Point[4];

                double alt = contactPoint.LunarAltitude;

                // Calculate points needed to draw horizon
                // i=0,1 is a horizon line itself,
                // i=2,3 is a line 90 degrees below horizon
                // 0.25 is a mean moon radius in degrees (more accurate value is not needed)
                for (int i = 0; i < 2; i++)
                {
                    double d = (alt + (i == 0 ? 0 : 90)) / 0.25 * moonRadius;

                    double r = 2 * Math.Sqrt(ActualHeight * ActualHeight + ActualWidth * ActualWidth);

                    p[2 * i] = new Point(pMoon.X - r, pMoon.Y + d);
                    p[2 * i + 1] = new Point(pMoon.X + r, pMoon.Y + d);
                }

                var g = new StreamGeometry();
                using (var gc = g.Open())
                {
                    gc.BeginFigure(p[0], true, true);
                    gc.LineTo(p[1], true, true);
                    gc.LineTo(p[3], true, true);
                    gc.LineTo(p[2], true, true);
                }

                ctx.PushOpacity(0.5);
                ctx.DrawGeometry(Brushes.Green, null, g);
            }
            else
            {
                DrawText(ctx, "Eclipse is invisible", pCenter, 12);
            }

            ctx.Pop();
        }

        private double Dist(Point p1, Point p2)
        {
            double dx = p1.X - p2.X;
            double dy = p1.Y - p2.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private Point ContactPoint(LunarEclipseLocalCircumstancesContactPoint c, int L)
        {
            if (c != null)
            {
                var pCenter = new Point(ActualWidth / 2, ActualHeight / 2);
                double penumbraRadius = ZoomLevel * Math.Min(ActualWidth, ActualHeight) / 3;
                double umbraRadius = c.F2 / c.F1 * penumbraRadius;
                double moonRadius = c.F3 / c.F1 * penumbraRadius;

                double r = Math.Sqrt(c.X * c.X + c.Y * c.Y) / c.F1 * penumbraRadius; ;
                switch (L)
                {
                    case 3:
                        r = penumbraRadius + moonRadius;
                        break;
                    case 2:
                        r = umbraRadius + moonRadius;
                        break;
                    case 1:
                        r = umbraRadius - moonRadius;
                        break;
                }

                double rot = Angle.ToRadians(Orientation == SolarEclipseChartOrientation.Zenithal ? c.ZAngle : c.PAngle);
                return new Point(pCenter.X - r * Math.Sin(rot), pCenter.Y - r * Math.Cos(rot));
            }

            return default(Point);
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
    public enum LunarEclipseChartOrientation
    {
        /// <summary>
        /// Top of the chart points on Zenith
        /// </summary>
        Zenithal,

        /// <summary>
        /// Top of the chart point on North
        /// </summary>
        Equatorial
    }

    /// <summary>
    /// Defines main contacts instants of a lunar eclipse
    /// </summary>
    public enum LunarEclipseChartContact
    {
        /// <summary>
        /// Instant of beginning of penumbral phase (first external contact of Moon with penumbra)
        /// </summary>
        P1,

        /// <summary>
        /// Instant of beginning of umbral (partial) phase (first external contact of Moon with umbra)
        /// </summary>
        U1,

        /// <summary>
        /// Instant of beginning of total phase (first internal contact of Moon with umbra)
        /// </summary>
        U2,

        /// <summary>
        /// Instant of greatest phase
        /// </summary>
        Max,

        /// <summary>
        /// Instant of end of total phase (last internal contact of Moon with umbra)
        /// </summary>
        U3,

        /// <summary>
        /// Instant of end of umbral (partial) phase (last external contact of Moon with umbra)
        /// </summary>
        U4,

        /// <summary>
        /// Instant of end of penumbral phase (last external contact of Moon with penumbra)
        /// </summary>
        P4
    }
}
