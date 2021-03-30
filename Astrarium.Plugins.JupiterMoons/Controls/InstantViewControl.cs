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
    public class InstantViewControl : Control
    {

        public static readonly DependencyProperty OrientationProperty =
           DependencyProperty.Register(nameof(Orientation), typeof(ChartOrientation), typeof(InstantViewControl), new FrameworkPropertyMetadata(null) { DefaultValue = ChartOrientation.Direct, BindsTwoWayByDefault = true, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });

        public static readonly DependencyProperty PositionsProperty =
            DependencyProperty.Register(nameof(Positions), typeof(CrdsRectangular[,]), typeof(InstantViewControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });

        public static readonly DependencyProperty HorizontalScaleProperty =
            DependencyProperty.Register(nameof(HorizontalScale), typeof(int), typeof(InstantViewControl), new FrameworkPropertyMetadata(null) { DefaultValue = 3, BindsTwoWayByDefault = true, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });

        private Typeface font = new Typeface(new FontFamily("#Noto Sans"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

        private string[] names = new[]
        {
            "Io", "Europa", "Ganymede", "Callisto"
        };

        public ChartOrientation Orientation
        {
            get => (ChartOrientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public int HorizontalScale
        {
            get => (int)GetValue(HorizontalScaleProperty);
            set => SetValue(HorizontalScaleProperty, value);
        }

        public CrdsRectangular[,] Positions
        {
            get => (CrdsRectangular[,])GetValue(PositionsProperty);
            set => SetValue(PositionsProperty, value);
        }

        private bool IsOcculted(CrdsRectangular p) 
        {
            // Y-scale stretching, squared (to avoid Jupiter flattening)
            const double STRETCH = 1.14784224788;
            return p.Z > 0 && Math.Sqrt(p.X * p.X + p.Y * p.Y * STRETCH) < 1;
        }

        protected override void OnRender(DrawingContext ctx)
        {
            var pCenter = new Point(ActualWidth / 2, ActualHeight / 2);
            var bounds = new Rect(0, 0, ActualWidth, ActualHeight);
            ctx.PushClip(new RectangleGeometry(bounds));

            // border
            {
                ctx.DrawLine(new Pen(BorderBrush, BorderThickness.Left), new Point(0, 0), new Point(0, ActualHeight));
                ctx.DrawLine(new Pen(BorderBrush, BorderThickness.Right), new Point(ActualWidth, 0), new Point(ActualWidth, ActualHeight));
                ctx.DrawLine(new Pen(BorderBrush, BorderThickness.Top), new Point(0, 0), new Point(ActualWidth, 0));
                ctx.DrawLine(new Pen(BorderBrush, BorderThickness.Bottom), new Point(0, ActualHeight), new Point(ActualWidth, ActualHeight));
            }

            if (Positions != null)
            {                
                double jupRadius = (ActualWidth / 110.0) * (HorizontalScale / 5.0);

                // Jupiter
                {
                    ctx.DrawEllipse(Brushes.Wheat, null, pCenter, jupRadius, jupRadius * (1 - 0.06487));
                }

                // moons
                for (int m = 0; m < 4; m++)
                {
                    var pos = Positions[m, 0];

                    if (!IsOcculted(pos))
                    {
                        bool mirrored = Orientation != ChartOrientation.Direct;
                        bool southTop = Orientation == ChartOrientation.Inverted;
                        double x = pCenter.X + pos.X * jupRadius * (mirrored ? -1 : 1);
                        double y = pCenter.Y - pos.Y * jupRadius * (southTop ? -1 : 1);
                        ctx.DrawEllipse(Brushes.White, null, new Point(x, y), 1, 1);

                        DrawText(ctx, names[m], new Point(x, y + 10), 10);
                    }
                }
            }
        }

        private void DrawText(DrawingContext ctx, string text, Point point, double size)
        {
            FormattedText formattedText = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, font, size, Brushes.Gray);
            ctx.DrawText(formattedText, new Point(point.X - formattedText.Width / 2, point.Y - formattedText.Height / 2));
        }
    }
}
