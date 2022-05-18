using Astrarium.Algorithms;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Astrarium.Plugins.Planner.Controls
{
    public class DirectionChart : BaseChart
    {
        protected override void OnRender(DrawingContext dc)
        {
            Matrix m = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
            double dpiFactor = 1 / m.M11;

            RenderOptions.SetEdgeMode(this, EdgeMode.Unspecified);

            Brush brushTextLabel = new SolidColorBrush((Color)FindResource("ColorControlLightBackground"));
            Brush brushBodyLine = IsDarkMode ? Brushes.Red : Brushes.Yellow;
            Brush brushBackground = IsDarkMode ? new SolidColorBrush(Color.FromRgb(20, 0, 0)) : new SolidColorBrush(Color.FromRgb(20, 20, 20));
            Pen penBodyLine = new Pen(brushBodyLine, dpiFactor);

            double padding = 20;
            double radius = Math.Min(ActualWidth, ActualHeight) / 2 - padding;
            dc.DrawEllipse(brushBackground, null, new Point(ActualWidth / 2, ActualHeight / 2), radius, radius);

            if (ShowChart && bodyCoordinatesInterpolated != null)
            {
                Geometry clipGeometry = new EllipseGeometry(new Point(ActualWidth / 2, ActualHeight / 2), radius, radius);
                dc.PushClip(clipGeometry);

                var figure = new PathFigure();

                double timeFrom = FromTime.TotalHours;
                double timeTo = ToTime.TotalHours;
                double obsDuration = timeTo < timeFrom ? timeTo - timeFrom + 24 : timeTo - timeFrom;

                int fromIndex = (int)(bodyCoordinatesInterpolated.Length / 24 * timeFrom);
                int count = Math.Max(1, (int)(bodyCoordinatesInterpolated.Length / 24 * obsDuration));

                for (int i = fromIndex; i <= fromIndex + count; i++)
                {
                    var c = bodyCoordinatesInterpolated[i > bodyCoordinatesInterpolated.Length - 1 ? i - bodyCoordinatesInterpolated.Length : i];
                    double angle = Angle.ToRadians(c.Azimuth - 90);
                    double r = radius * (90 - c.Altitude) / 90;
                    double x = ActualWidth / 2 + r * Math.Cos(angle);
                    double y = ActualHeight / 2 - r * Math.Sin(angle);

                    if (!figure.Segments.Any())
                    {
                        figure.StartPoint = new Point(x, y);
                    }
                    figure.Segments.Add(new LineSegment(new Point(x, y), true));
                }

                var geometry = new PathGeometry();
                geometry.Figures.Add(figure);
                dc.DrawGeometry(null, penBodyLine, geometry);

                dc.Pop();
            }

            string[] labels = new string[] { "S", "W", "N", "E" };
            for (int i = 0; i < 4; i++)
            {
                var text = new FormattedText(labels[i], System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 10, brushTextLabel);
                double angle = Angle.ToRadians(i * 90 - 90);
                double r = radius + 5 + Math.Max(text.WidthIncludingTrailingWhitespace / 2, text.Height / 2);
                double x = ActualWidth / 2 + r * Math.Cos(angle);
                double y = ActualHeight / 2 - r * Math.Sin(angle);
                dc.DrawText(text, new Point(x - text.WidthIncludingTrailingWhitespace / 2, y - text.Height / 2));
            }
        }
    }
}
