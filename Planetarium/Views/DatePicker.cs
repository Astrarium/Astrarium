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
using System.Windows.Input;

namespace Planetarium.Views
{
    public class DatePicker : Control
    {
        public DatePicker()
        {

        }

        public double JulianDay
        {
            get {
                return (double)GetValue(JulianDayProperty);
            }
            set {
                SetValue(JulianDayProperty, value);
            }
        }
        public readonly static DependencyProperty JulianDayProperty = DependencyProperty.Register(
            "JulianDay", typeof(double), typeof(DatePicker), new FrameworkPropertyMetadata(Date.Now.ToJulianEphemerisDay(), (o, e) =>
            {
                DatePicker picker = (DatePicker)o;
                picker.RaiseJulianDayChangedEvent(e);
            })
            {
                BindsTwoWayByDefault = true,
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

        public event EventHandler<DependencyPropertyChangedEventArgs> JulianDayChanged;
        private void RaiseJulianDayChangedEvent(DependencyPropertyChangedEventArgs e)
        {
            JulianDayChanged?.Invoke(this, e);
            switch (Options)
            {
                default:
                case DateOptions.DateTime:
                    DateString = Formatters.DateTime.Format(new Date(JulianDay, UtcOffset));
                    break;
                case DateOptions.DateOnly:
                    DateString = Formatters.DateOnly.Format(new Date(JulianDay, UtcOffset));
                    break;
                case DateOptions.MonthYear:
                    DateString = Formatters.MonthYear.Format(new Date(JulianDay, UtcOffset));
                    break;
            }
        }

        public DateOptions Options
        {
            get { return (DateOptions)GetValue(OptionsProperty); }
            set { SetValue(OptionsProperty, value); }
        }
        public readonly static DependencyProperty OptionsProperty = DependencyProperty.Register(
            "Options", typeof(DateOptions), typeof(DatePicker), new UIPropertyMetadata(DateOptions.DateTime));

        public double UtcOffset
        {
            get { return (double)GetValue(UtcOffsetProperty); }
            set { SetValue(UtcOffsetProperty, value); }
        }
        public readonly static DependencyProperty UtcOffsetProperty = DependencyProperty.Register(
            "UtcOffset", typeof(double), typeof(DatePicker), new UIPropertyMetadata(0.0));


        public string DateString
        {
            get { return (string)GetValue(DateStringProperty); }
            private set { SetValue(DateStringProperty, value); }
        }
        public readonly static DependencyProperty DateStringProperty = DependencyProperty.Register(
            "DateString", typeof(string), typeof(DatePicker));

        public IViewManager ViewManager
        {
            get { return (IViewManager)GetValue(ViewManagerProperty); }
            set { SetValue(ViewManagerProperty, value); }
        }
        public readonly static DependencyProperty ViewManagerProperty = DependencyProperty.Register(
            "ViewManager", typeof(IViewManager), typeof(DatePicker), new UIPropertyMetadata(null));

        TextBox _TextBox;
        Button _Button;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _TextBox = Template.FindName("TextBox", this) as TextBox;
            _Button = Template.FindName("Button", this) as Button;
            _Button.Click += ShowDateWindow;
        }

        private void ShowDateWindow(object sender, RoutedEventArgs e)
        {
            var vm = new DateVM(JulianDay, UtcOffset, Options);
            if (ViewManager.ShowDialog(vm) ?? false)
            {
                JulianDay = vm.JulianDay;
            }
        }
    }
}
