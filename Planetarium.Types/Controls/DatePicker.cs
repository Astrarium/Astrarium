using ADK;
using Planetarium.Types;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Planetarium.Controls
{
    public class DatePicker : Control
    {
        public DatePicker() { }

        public double JulianDay
        {
            get { return (double)GetValue(JulianDayProperty); }
            set { SetValue(JulianDayProperty, value); }
        }

        public readonly static DependencyProperty JulianDayProperty = DependencyProperty.Register(
            nameof(JulianDay), 
            typeof(double), 
            typeof(DatePicker), 
            new FrameworkPropertyMetadata(Date.Now.ToJulianEphemerisDay(), (o, e) =>
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
            var date = new Date(JulianDay, UtcOffset);
            switch (Options)
            {
                default:
                case DateOptions.DateTime:
                    DateString = Formatters.DateTime.Format(date);
                    break;
                case DateOptions.DateOnly:
                    DateString = Formatters.Date.Format(date);
                    break;
                case DateOptions.MonthYear:
                    DateString = Formatters.MonthYear.Format(date);
                    break;
            }
        }

        public DateOptions Options
        {
            get { return (DateOptions)GetValue(OptionsProperty); }
            set { SetValue(OptionsProperty, value); }
        }
        public readonly static DependencyProperty OptionsProperty = DependencyProperty.Register(
            nameof(Options), 
            typeof(DateOptions), 
            typeof(DatePicker), 
            new FrameworkPropertyMetadata(DateOptions.DateTime, (o, e) => {
                DatePicker picker = (DatePicker)o;
                picker.RaiseJulianDayChangedEvent(e);
            }));

        public double UtcOffset
        {
            get { return (double)GetValue(UtcOffsetProperty); }
            set { SetValue(UtcOffsetProperty, value); }
        }
        public readonly static DependencyProperty UtcOffsetProperty = DependencyProperty.Register(
            nameof(UtcOffset), 
            typeof(double), 
            typeof(DatePicker), 
            new FrameworkPropertyMetadata(0.0, (o, e) => {
                DatePicker picker = (DatePicker)o;
                picker.RaiseJulianDayChangedEvent(e);
            }));

        public string DateString
        {
            get { return (string)GetValue(DateStringProperty); }
            private set { SetValue(DateStringProperty, value); }
        }
        public readonly static DependencyProperty DateStringProperty = DependencyProperty.Register(
            nameof(DateString), 
            typeof(string), 
            typeof(DatePicker), new PropertyMetadata("01 Jan 2000"));

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            double? jd = ViewManager.ShowDateDialog(JulianDay, UtcOffset, Options);
            if (jd != null)
            {
                JulianDay = jd.Value;
            }
        }
    }
}
