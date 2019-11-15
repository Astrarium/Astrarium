using Planetarium.Types;
using Planetarium.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Planetarium.Controls
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
            var vm = new TimeSpanVM();
            vm.TimeSpan = TimeSpan;
            if (ViewManager.ShowDialog(vm) ?? false)
            {
                TimeSpan = vm.TimeSpan;
            }
        }

        private static string TimeSpanToString(TimeSpan timeSpan)
        {
            return Formatters.TimeSpan.Format(timeSpan);
        }
    }
}
