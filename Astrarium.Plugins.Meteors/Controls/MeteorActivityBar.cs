using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Astrarium.Plugins.Meteors.Controls
{
    public class MeteorActivityBar : Control
    {
        public static readonly DependencyProperty MeteorProperty =
            DependencyProperty.Register(nameof(Meteor), typeof(Meteor), typeof(MeteorActivityBar), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });

        public Meteor Meteor
        {
            get => (Meteor)GetValue(MeteorProperty);
            set => SetValue(MeteorProperty, value);
        }

        /// <summary>
        /// Array index is a meteor shower activity class, zero-based
        /// </summary>
        private readonly byte[] TransparencyCodes = new byte[] { 250, 200, 150, 100 };

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (Meteor != null)
            {
                for (int i = 0; i < 2; i++)
                {
                    double x = (Meteor.Begin - 1) / 365.0 * ActualWidth;
                    double w = (Meteor.End - Meteor.Begin) / 365.0 * ActualWidth;
                    double x0 = (Meteor.Max - 1) / 365.0 * ActualWidth;

                    x += i * ActualWidth;
                    x0 += i * ActualWidth;

                    var g = new StreamGeometry();

                    var points = new Point[3]
                    {
                        new Point(x, ActualHeight),
                        new Point(x0, -ActualHeight),
                        new Point(x + w, ActualHeight)
                    };

                    using (StreamGeometryContext gc = g.Open())
                    {
                        gc.BeginFigure(points[0], true, true);
                        gc.PolyBezierTo(points, true, true);
                    }

                    var color = Colors.Cyan;
                    drawingContext.DrawGeometry(new SolidColorBrush(Color.FromArgb(TransparencyCodes[Meteor.ActivityClass - 1], color.R, color.G, color.B)), null, g);
                }
            }
        }
    }
}
