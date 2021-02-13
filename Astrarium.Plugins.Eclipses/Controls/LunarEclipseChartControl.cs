using Astrarium.Algorithms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        //public static readonly DependencyProperty ShowContactCirclesProperty =
        //   DependencyProperty.Register(nameof(ShowContactCircles), typeof(bool), typeof(SolarEclipseChartControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = true, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, DefaultValue = true });

        //public static readonly DependencyProperty ShowMoonOutlineOnlyProperty =
        //   DependencyProperty.Register(nameof(ShowMoonOutlineOnly), typeof(bool), typeof(SolarEclipseChartControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = true, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, DefaultValue = false });

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

        ///// <summary>
        ///// If flag is set, contact circles are shown
        ///// </summary>
        //public bool ShowContactCircles
        //{
        //    get => (bool)GetValue(ShowContactCirclesProperty);
        //    set => SetValue(ShowContactCirclesProperty, value);
        //}

        ///// <summary>
        ///// If set, Moon displayed as oultine circle without filling
        ///// </summary>
        //public bool ShowMoonOutlineOnly
        //{
        //    get => (bool)GetValue(ShowMoonOutlineOnlyProperty);
        //    set => SetValue(ShowMoonOutlineOnlyProperty, value);
        //}

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

            if (Circumstances != null)
            {
                var contact = Contact;
                LunarEclipseLocalCircumstancesContactPoint contactPoint = null;

                switch (contact)
                {
                    case LunarEclipseChartContact.P1:
                        contactPoint = Circumstances.PenumbralBegin;
                        break;
                    case LunarEclipseChartContact.U1:
                        contactPoint = Circumstances.PartialBegin;
                        break;
                    case LunarEclipseChartContact.U2:
                        contactPoint = Circumstances.TotalBegin;
                        break;
                    case LunarEclipseChartContact.Max:
                        contactPoint = Circumstances.Maximum;
                        break;
                    case LunarEclipseChartContact.U3:
                        contactPoint = Circumstances.TotalEnd;
                        break;
                    case LunarEclipseChartContact.U4:
                        contactPoint = Circumstances.PartialEnd;
                        break;
                    case LunarEclipseChartContact.P4:
                        contactPoint = Circumstances.PenumbralEnd;
                        break;
                }

                if (Orientation == SolarEclipseChartOrientation.Zenithal)
                {
                    ctx.PushTransform(new RotateTransform(contactPoint.QAngle, pCenter.X, pCenter.Y));                  
                }

                double penumbraRadius = ZoomLevel * Math.Min(ActualWidth, ActualHeight) / 3;
                double umbraRadius = contactPoint.F2 / contactPoint.F1 * penumbraRadius;
                double moonRadius = contactPoint.F3 / contactPoint.F1 * penumbraRadius;

                // Moon track
                Point pP1 = new Point(pCenter.X - Circumstances.PenumbralBegin.X / Circumstances.PenumbralBegin.F1 * penumbraRadius, pCenter.Y - Circumstances.PenumbralBegin.Y / Circumstances.PenumbralBegin.F1 * penumbraRadius);
                Point pP4 = new Point(pCenter.X - Circumstances.PenumbralEnd.X / Circumstances.PenumbralEnd.F1 * penumbraRadius, pCenter.Y - Circumstances.PenumbralEnd.Y / Circumstances.PenumbralEnd.F1 * penumbraRadius);
                ctx.DrawLine(new Pen(Brushes.DimGray, 1) { DashStyle = new DashStyle(new double[] { 2 }, 2) }, pP1, pP4);

                // Penumbra
                ctx.DrawEllipse(null, new Pen(Brushes.DimGray, 1), pCenter, penumbraRadius, penumbraRadius);

                // Umbra
                var umbraEllipse = new EllipseGeometry(pCenter, umbraRadius, umbraRadius);                
                ctx.DrawGeometry(null, new Pen(Brushes.DimGray, 1), umbraEllipse);

                // Moon
                var pMoon = new Point(pCenter.X - contactPoint.X / contactPoint.F1 * penumbraRadius, pCenter.Y - contactPoint.Y / contactPoint.F1 * penumbraRadius);
                var moonEllipse = new EllipseGeometry(pMoon, moonRadius, moonRadius);
                ctx.DrawGeometry(Brushes.Gray, null, moonEllipse);

                // Totally eclipsed part of the Moon
                var totalityRegion = new CombinedGeometry(GeometryCombineMode.Intersect, umbraEllipse, moonEllipse);
                if (!totalityRegion.IsEmpty()) 
                {
                    ctx.DrawGeometry(Brushes.Brown, null, totalityRegion);
                }

                DrawText(ctx, "P1", pP1, 10);
                DrawText(ctx, "Max", pMoon, 10);
                DrawText(ctx, "P4", pP4, 10);


                // Draw horizon
                Point[] p = new Point[4];

                double alt = contactPoint.LunarAltitude;
                double q = contactPoint.QAngle;

                // Calculate points needed to draw horizon
                // i=0,1 is a horizon line itself,
                // i=2,3 is a line 20 degrees below horizon
                // 0.25 is a mean solar radius in degrees (more accurate value is not needed)
                for (int i = 0; i < 2; i++)
                {
                    double d = (alt + (i == 0 ? 0 : 90)) / 0.25 * moonRadius;
                    double ang = Angle.ToRadians(q);
                    double dx = d * Math.Sin(ang);
                    double dy = d * Math.Cos(ang);
                    Point p0 = new Point(pMoon.X + dx, pMoon.Y + dy);
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
                ctx.DrawGeometry(Brushes.Green, null, g);
            }


            ctx.Pop();
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
        /// Top of the chart point os North
        /// </summary>
        Equatorial
    }

    /// <summary>
    /// Defines main contacts instants of an eclipse
    /// </summary>
    // TODO: docs
    public enum LunarEclipseChartContact
    {
        P1,
        U1,
        U2,
        Max,
        U3,
        U4,
        P4
    }
}
