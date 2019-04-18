using ADK;
using Planetarium.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

namespace Planetarium.Controls
{
    public class TimeSpanPicker : Control
    {
        public TimeSpanPicker()
        {
            
        }

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

        [DependecyInjection]
        public IViewManager ViewManager { get; private set; }

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
            return $"{timeSpan.Days}d {timeSpan.Hours:D2}h {timeSpan.Minutes:D2}m {timeSpan.Seconds:D2}s";
        }
    }
}
