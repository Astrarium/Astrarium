using Astrarium.Types;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Astrarium.Types.Controls
{
    public class TimeSpanPicker : Control
    {
        public TimeSpan TimeSpan
        {
            get { return (TimeSpan)GetValue(TimeSpanProperty); }
            set { SetValue(TimeSpanProperty, value); }
        }

        public string Caption
        {
            get { return (string)GetValue(CaptionProperty); }
            private set { SetValue(CaptionProperty, value); }
        }

        public readonly static DependencyProperty TimeSpanProperty = DependencyProperty.Register(
            nameof(TimeSpan), 
            typeof(TimeSpan), 
            typeof(TimeSpanPicker), 
            new FrameworkPropertyMetadata(TimeSpan.FromDays(1), (o, e) => {
                var picker = (TimeSpanPicker)o;
                picker.Caption = TimeSpanToString((TimeSpan)e.NewValue);
            })
            {
                BindsTwoWayByDefault = true,
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                AffectsRender = true
            });

        public readonly static DependencyProperty CaptionProperty = DependencyProperty.Register(
            nameof(Caption),
            typeof(string),
            typeof(TimeSpanPicker),
            new FrameworkPropertyMetadata(TimeSpanToString(TimeSpan.FromDays(1))));

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            var timeSpan = ViewManager.ShowTimeSpanDialog(TimeSpan);
            if (timeSpan != null)
            {
                TimeSpan = timeSpan.Value;
            }
        }

        private static string TimeSpanToString(TimeSpan timeSpan)
        {
            return Formatters.TimeSpan.Format(timeSpan);
        }
    }
}
