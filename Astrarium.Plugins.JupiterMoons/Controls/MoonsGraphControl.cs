using Astrarium.Algorithms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Astrarium.Plugins.JupiterMoons.Controls
{
    public class MoonsGraphControl : Control
    {
        public static readonly DependencyProperty IsLockedProperty =
            DependencyProperty.Register(nameof(IsLocked), typeof(bool), typeof(MoonsGraphControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });

        public static readonly DependencyProperty PositionsProperty =
            DependencyProperty.Register(nameof(Positions), typeof(ICollection<CrdsRectangular[,]>), typeof(MoonsGraphControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });

        public static readonly DependencyProperty CurrentPositionsProperty =
            DependencyProperty.Register(nameof(CurrentPositions), typeof(CrdsRectangular[,]), typeof(MoonsGraphControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = false, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });

        public static readonly DependencyProperty DaysOffsetProperty =
            DependencyProperty.Register(nameof(DaysOffset), typeof(double), typeof(MoonsGraphControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });

        public static readonly DependencyProperty VerticalScaleProperty =
            DependencyProperty.Register(nameof(VerticalScale), typeof(int), typeof(MoonsGraphControl), new FrameworkPropertyMetadata(null) { DefaultValue = 1, BindsTwoWayByDefault = true, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });

        public static readonly DependencyProperty HorizontalScaleProperty =
            DependencyProperty.Register(nameof(HorizontalScale), typeof(int), typeof(MoonsGraphControl), new FrameworkPropertyMetadata(null) { DefaultValue = 3, BindsTwoWayByDefault = true, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(nameof(Orientation), typeof(ChartOrientation), typeof(MoonsGraphControl), new FrameworkPropertyMetadata(null) { DefaultValue = ChartOrientation.Direct, BindsTwoWayByDefault = true, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });

        private Typeface font = new Typeface(new FontFamily("#Noto Sans"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

        public bool IsLocked
        {
            get => (bool)GetValue(IsLockedProperty);
            set => SetValue(IsLockedProperty, value);
        }

        public ICollection<CrdsRectangular[,]> Positions
        {
            get => (ICollection<CrdsRectangular[,]>)GetValue(PositionsProperty);
            set => SetValue(PositionsProperty, value);
        }

        public double DaysOffset
        {
            get => (double)GetValue(DaysOffsetProperty);
            set => SetValue(DaysOffsetProperty, value);
        }

        public int HorizontalScale
        {
            get => (int)GetValue(HorizontalScaleProperty);
            set => SetValue(HorizontalScaleProperty, value);
        }

        public int VerticalScale
        {
            get => (int)GetValue(VerticalScaleProperty);
            set => SetValue(VerticalScaleProperty, value);
        }

        public CrdsRectangular[,] CurrentPositions
        {
            get => (CrdsRectangular[,])GetValue(CurrentPositionsProperty);
            set => SetValue(CurrentPositionsProperty, value);
        }

        public ChartOrientation Orientation
        {
            get => (ChartOrientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        protected override void OnRender(DrawingContext ctx)
        {
            var pCenter = new Point(ActualWidth / 2, ActualHeight / 2);
            var bounds = new Rect(0, 0, ActualWidth, ActualHeight);
            ctx.PushClip(new RectangleGeometry(bounds));
            ctx.DrawRectangle(Brushes.Transparent, null, bounds);

            // border
            {
                ctx.DrawLine(new Pen(BorderBrush, BorderThickness.Left), new Point(0, 0), new Point(0, ActualHeight));
                ctx.DrawLine(new Pen(BorderBrush, BorderThickness.Right), new Point(ActualWidth, 0), new Point(ActualWidth, ActualHeight));
                ctx.DrawLine(new Pen(BorderBrush, BorderThickness.Top), new Point(0, 0), new Point(ActualWidth, 0));
                ctx.DrawLine(new Pen(BorderBrush, BorderThickness.Bottom), new Point(0, ActualHeight), new Point(ActualWidth, ActualHeight));
            }

            if (Positions != null && Positions.Any())
            {
                int daysCount = Positions.Count / 24;
                double dayHeight = (ActualHeight / daysCount * 5.0) * (VerticalScale / 5.0);
                double jupRadius = (ActualWidth / 110.0) * (HorizontalScale / 5.0);
                double daysOffset = DaysOffset;

                Color[] colors = new Color[4]
                {
                    Colors.Orange,
                    Colors.Cyan,
                    Colors.Red,
                    Colors.LightGreen
                };

                // Jupiter bounds
                {
                    ctx.DrawLine(new Pen(Brushes.Wheat, 0.25) { DashStyle = new DashStyle(new double[] { 5, 5 }, 5) }, new Point(pCenter.X - jupRadius, 0), new Point(pCenter.X - jupRadius, ActualHeight));
                    ctx.DrawLine(new Pen(Brushes.Wheat, 0.25) { DashStyle = new DashStyle(new double[] { 5, 5 }, 5) }, new Point(pCenter.X + jupRadius, 0), new Point(pCenter.X + jupRadius, ActualHeight));
                }

                // grid
                for (int d = 0; d <= daysCount; d++)
                {
                    double y = -daysOffset * dayHeight + (daysOffset / daysCount) * ActualHeight + d * dayHeight;
                    ctx.DrawLine(new Pen(Brushes.Gray, 0.5), new Point(20, y), new Point(ActualWidth, y));
                    int day = (d + 1) % (daysCount + 1);
                    if (day > 1)
                    {
                        DrawText(ctx, day.ToString(), new Point(10, y), 10);
                    }
                }

                // curves
                for (int m = 0; m < 4; m++) 
                {
                    var points = new List<Point>();
                    var curve = new PathFigure();
                    int h = 0;

                    bool mirrored = Orientation != ChartOrientation.Direct;
                    bool southTop = Orientation == ChartOrientation.Inverted;

                    foreach (var pos in Positions)
                    {
                        h++;
                        double x = pCenter.X + pos[m, 0].X * jupRadius * (mirrored ? -1 : 1);
                        double y = -daysOffset * dayHeight + (daysOffset / daysCount) * ActualHeight + (h / 24.0) * dayHeight; // + pos[m, 0].Y * jupRadius * (southTop ? -1 : 1);
                        points.Add(new Point(x, y));

                        if (pos == CurrentPositions)
                        {
                            ctx.DrawLine(new Pen(Brushes.Gray, 1), new Point(0, y), new Point(ActualWidth, y));
                        }
                    }

                    curve.StartPoint = points.First();
                    curve.Segments.Add(new PolyLineSegment(points, true));
                    curve.IsFilled = false;
                    curve.IsClosed = false;

                    var path = new PathGeometry();
                    path.Figures.Add(curve);

                    ctx.DrawGeometry(null, new Pen(new SolidColorBrush(colors[m]), 1), path);
                }
            }
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                IsLocked = !IsLocked;
                InvalidateVisual();
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            if (!IsLocked)
            {
                CurrentPositions = null;
                InvalidateVisual();
            }
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if (Positions != null && Positions.Any() && !IsLocked)
            {
                int daysCount = Positions.Count / 24;
                double dayHeight = (ActualHeight / daysCount * 5.0) * (VerticalScale / 5.0);
                double daysOffset = DaysOffset;

                var p = e.GetPosition(this);

                int h = (int)(Math.Floor(p.Y - (-daysOffset * dayHeight + (daysOffset / daysCount) * ActualHeight)) / dayHeight * 24);
                CurrentPositions = Positions.ElementAt(h);

                InvalidateVisual();
            }
        }

        private void DrawText(DrawingContext ctx, string text, Point point, double size)
        {
            FormattedText formattedText = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, font, size, Brushes.White);
            ctx.DrawText(formattedText, new Point(point.X - formattedText.Width / 2, point.Y - formattedText.Height / 2));
        }

        
    }
}
