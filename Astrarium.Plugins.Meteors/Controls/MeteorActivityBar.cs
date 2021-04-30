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

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (Meteor != null)
            {
                double x = (Meteor.Begin - 1) / 365.0 * ActualWidth;
                double w = (Meteor.End - Meteor.Begin) / 365.0 * ActualWidth;

                drawingContext.DrawRectangle(new SolidColorBrush(Colors.Blue), null, new Rect(x, 0, w, ActualHeight));
            }
        }
    }
}
