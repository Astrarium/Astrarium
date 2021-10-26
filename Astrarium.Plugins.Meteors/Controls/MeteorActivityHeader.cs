using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Astrarium.Plugins.Meteors.Controls
{
    public class MeteorActivityHeader : Control
    {
        private Typeface font = new Typeface(new FontFamily("#Noto Sans"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

        public static readonly DependencyProperty YearProperty =
            DependencyProperty.Register(nameof(Year), typeof(int), typeof(MeteorActivityHeader), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });

        public int Year
        {
            get => (int)GetValue(YearProperty);
            set => SetValue(YearProperty, value);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var jd0 = new Date(Year, 1, 1).ToJulianDay();

            double daysCount = Date.IsLeapYear(Year) ? 366 : 365;

            var monthNames = CultureInfo.CurrentCulture.DateTimeFormat.MonthNames.Take(12).ToArray();

            var pen = new Pen(new SolidColorBrush(Color.FromArgb(255, 80, 80, 80)), 1);

            for (int i = 0; i <= daysCount; i++)
            {
                double x = i / daysCount * ActualWidth;
                double y = ActualHeight / 2;

                var d = new Date(jd0 + i);

                if (d.Day == 1)
                {
                    double dayWidth = ActualWidth / daysCount;
                    var p = new Point(x + Date.DaysInMonth(d.Year, d.Month) / 2.0 * dayWidth, y);
                    DrawText(drawingContext, $"{monthNames[d.Month - 1]}", p, 12);
                }

                y = ActualHeight - ((d.Day == 1) ? ActualHeight : ActualHeight / 5);

                drawingContext.DrawLine(pen, new Point(x, y), new Point(x, ActualHeight));
            }
        }

        private void DrawText(DrawingContext ctx, string text, Point point, double size)
        {
            FormattedText formattedText = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, font, size, Brushes.Gray);
            ctx.DrawText(formattedText, new Point(point.X - formattedText.Width / 2, point.Y - formattedText.Height / 2));
        }
    }
}
