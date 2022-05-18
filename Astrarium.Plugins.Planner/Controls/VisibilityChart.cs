using System;
using System.Windows;
using System.Windows.Media;

namespace Astrarium.Plugins.Planner.Controls
{
    public class VisibilityChart : BaseChart
    {
        protected override void OnRender(DrawingContext dc)
        {
            Matrix m = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
            double dpiFactor = 1 / m.M11;

            RenderOptions.SetEdgeMode(this, EdgeMode.Unspecified);

            const double verticalPadding = 20;
            const double horizontalPadding = 10;

            Brush brushBackground = new SolidColorBrush(Color.FromRgb(20, 20, 20));
            Brush brushGround = IsDarkMode ? Brushes.DarkRed : Brushes.DarkGreen;

            Brush brushTextLabel = new SolidColorBrush((Color)FindResource("ColorControlLightBackground"));
            Brush brushHourLine = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255));
            Brush brushBodyLine = IsDarkMode ? Brushes.Red : Brushes.Yellow;
            Color colorDaylight = IsDarkMode ? Colors.DarkRed : Color.FromRgb(0, 114, 196);
            Pen penHourLine = new Pen(brushHourLine, dpiFactor);
            Pen penBodyLine = new Pen(brushBodyLine, dpiFactor);
            Pen penObservationLimits = new Pen(new SolidColorBrush(Color.FromRgb(155, 0, 0)), dpiFactor * 2);

            var bounds = new Rect(0, 0, ActualWidth, ActualHeight);
            dc.PushClip(new RectangleGeometry(bounds));

            dc.PushClip(new RectangleGeometry(new Rect(horizontalPadding, verticalPadding, Math.Max(0, ActualWidth - 2 * horizontalPadding), ActualHeight - 2 * verticalPadding)));

            // background
            dc.DrawRectangle(brushBackground, null, bounds);

            // daylight/night background
            if (sunCoordinatesInterpolated != null)
            {
                double sunAltitudesCount = sunCoordinatesInterpolated.Length;
                LinearGradientBrush linearGradientBrush = new LinearGradientBrush();
                linearGradientBrush.StartPoint = new Point(horizontalPadding, ActualHeight / 2);
                linearGradientBrush.EndPoint = new Point(ActualWidth - horizontalPadding, ActualHeight / 2);
                linearGradientBrush.MappingMode = BrushMappingMode.Absolute;
                for (int i = 0; i < sunCoordinatesInterpolated.Length; i++)
                {
                    // -18 degrees is an astronomical night
                    double transp = sunCoordinatesInterpolated[i].Altitude <= -18 ? 0 : (sunCoordinatesInterpolated[i].Altitude < 0 ? (sunCoordinatesInterpolated[i].Altitude + 18) / 18.0 : 1);
                    Color c = Color.FromArgb((byte)(transp * 255), colorDaylight.R, colorDaylight.G, colorDaylight.B);
                    double f = (i / (sunAltitudesCount - 1) + 0.5) % 1;
                    linearGradientBrush.GradientStops.Add(new GradientStop(c, f));
                }
                dc.DrawRectangle(linearGradientBrush, null, bounds);
            }

            // ground overlay
            dc.DrawRectangle(brushGround, null, new Rect(0, ActualHeight / 2, ActualWidth, ActualHeight / 2));

            if (ShowChart && bodyCoordinatesInterpolated != null)
            {
                // body altitude line
                for (int j = 0; j < 2; j++)
                {
                    var figure = new PathFigure();

                    for (int i = 0; i <= bodyCoordinatesInterpolated.Length; i++)
                    {
                        double k = i;
                        double x = horizontalPadding + k / (double)bodyCoordinatesInterpolated.Length * (ActualWidth - 2 * horizontalPadding);
                        double alt = i == bodyCoordinatesInterpolated.Length ? bodyCoordinatesInterpolated[i - 1].Altitude : bodyCoordinatesInterpolated[i].Altitude;
                        double y = (ActualHeight / 2) - alt / 90.0 * (ActualHeight / 2 - verticalPadding);
                        double offset = -Math.Sign(j - 0.5) * (ActualWidth - 2 * horizontalPadding) / 2;

                        var point = new Point(x + offset, y);
                        if (i == 0)
                        {
                            figure.StartPoint = point;
                        }
                        figure.Segments.Add(new LineSegment(point, true));
                    }

                    var geometry = new PathGeometry();
                    geometry.Figures.Add(figure);
                    dc.DrawGeometry(null, penBodyLine, geometry);
                }
            }

            // dim the body path below horizon
            dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(150, 0, 0, 0)), null, new Rect(0, ActualHeight / 2, ActualWidth, ActualHeight / 2));;

            // time grid
            for (int i = 0; i <= 24; i++)
            {
                double x = horizontalPadding + i / 24.0 * (ActualWidth - 2 * horizontalPadding);
                GuidelineSet guidelines = new GuidelineSet();
                guidelines.GuidelinesX.Add(x);
                dc.PushGuidelineSet(guidelines);
                dc.DrawLine(penHourLine, new Point(x, 0), new Point(x, ActualHeight));
                dc.Pop();
            }

            // pop the vertical offset margins
            dc.Pop();

            // visibility box
            {
                double from = horizontalPadding + (FromTime.TotalDays + 0.5) % 1 * (ActualWidth - 2 * horizontalPadding);
                double to = horizontalPadding + (ToTime.TotalDays + 0.5) % 1 * (ActualWidth - 2 * horizontalPadding);

                var guidelines = new GuidelineSet();
                guidelines.GuidelinesX.Add(to);
                guidelines.GuidelinesX.Add(ActualWidth - 2 * horizontalPadding - from + to);
                dc.PushGuidelineSet(guidelines);
                dc.DrawLine(penObservationLimits, new Point(from, verticalPadding), new Point(from, ActualHeight - verticalPadding));
                dc.DrawLine(penObservationLimits, new Point(to, verticalPadding), new Point(to, ActualHeight - verticalPadding));
                dc.Pop();

                double[] x = new double[] { from, to };
                string[] label = new string[] { "BEGIN", "END" };
                for (int i = 0; i < 2; i++)
                {
                    var text = new FormattedText(label[i], System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 10, brushTextLabel);
                    double offset = -text.WidthIncludingTrailingWhitespace / 2;
                    dc.DrawText(text, new Point(x[i] + offset, ActualHeight - verticalPadding / 2 - text.Height / 2));
                }
            }

            // time grid labels
            for (int i = 0; i <= 24; i++)
            {
                double x = horizontalPadding +  i / 24.0 * (ActualWidth - 2 * horizontalPadding);
                var text = new FormattedText($"{(i + 12) % 24}", System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 10, brushTextLabel);
                dc.DrawText(text, new Point(x - text.WidthIncludingTrailingWhitespace / 2, (verticalPadding - text.Height) / 2));
            }
        }
    }
}
