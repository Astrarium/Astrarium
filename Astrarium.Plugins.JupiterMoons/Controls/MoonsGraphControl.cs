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

namespace Astrarium.Plugins.JupiterMoons.Controls
{
    public class MoonsGraphControl : Control
    {
        public static readonly DependencyProperty PositionsProperty =
            DependencyProperty.Register(nameof(Positions), typeof(ICollection<CrdsRectangular[,]>), typeof(MoonsGraphControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });

        public static readonly DependencyProperty DaysOffsetProperty =
            DependencyProperty.Register(nameof(DaysOffset), typeof(double), typeof(MoonsGraphControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });

        private Typeface font = new Typeface(new FontFamily("#Noto Sans"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

        /// <summary>
        /// Positions of moons
        /// </summary>
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

        protected override void OnRender(DrawingContext ctx)
        {
            var pCenter = new Point(ActualWidth / 2, ActualHeight / 2);
            var bounds = new Rect(0, 0, ActualWidth, ActualHeight);
            ctx.PushClip(new RectangleGeometry(bounds));

            if (Positions != null && Positions.Any())
            {
                double daysCount = Positions.Count / 24;

                double dayHeight = 40;// ActualHeight / daysCount;
                double jupRadius = 10;

                double daysOffset = this.DaysOffset;

                // Jupiter bounds
                {
                    ctx.DrawLine(new Pen(Brushes.Wheat, 0.5), new Point(pCenter.X - jupRadius, 0), new Point(pCenter.X - jupRadius, ActualHeight));
                    ctx.DrawLine(new Pen(Brushes.Wheat, 0.5), new Point(pCenter.X + jupRadius, 0), new Point(pCenter.X + jupRadius, ActualHeight));
                }
                // grid
                for (int d = 0; d <= daysCount; d++)
                {
                    double y = -daysOffset * dayHeight + (daysOffset / daysCount) * ActualHeight + d * dayHeight;
                    ctx.DrawLine(new Pen(Brushes.Gray, 0.5), new Point(20, y), new Point(ActualWidth, y));
                    DrawText(ctx, ((d + 1) % (daysCount + 1)).ToString(), new Point(10, y), 10);
                }

                // curves
                for (int m = 0; m < 4; m++) 
                {
                    var points = new List<Point>();
                    var curve = new PathFigure();
                    int h = 0;

                    foreach (var pos in Positions)
                    {
                        h++;
                        double x = pCenter.X + pos[m, 0].X * jupRadius;
                        double y = -daysOffset * dayHeight + (daysOffset / daysCount) * ActualHeight + (h / 24.0) * dayHeight + pos[m, 0].Y * jupRadius;

                        points.Add(new Point(x, y));
                    }

                    curve.StartPoint = points.First();
                    curve.Segments.Add(new PolyLineSegment(points, true));
                    curve.IsFilled = false;
                    curve.IsClosed = false;


                    var path = new PathGeometry();
                    path.Figures.Add(curve);


                    

                    ctx.DrawGeometry(null, new Pen(Brushes.Red, 1), path);
                }
            }

           // ctx.DrawEllipse(Brushes.Red, null, pCenter, 20, 20);

            
        }

        private void DrawText(DrawingContext ctx, string text, Point point, double size)
        {
            FormattedText formattedText = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, font, size, Brushes.White);
            ctx.DrawText(formattedText, new Point(point.X - formattedText.Width / 2, point.Y - formattedText.Height / 2));
        }
    }
}
