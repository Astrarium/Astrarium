using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Planetarium.Views
{
    public class IntegerTextBox : Control
    {
        public IntegerTextBox()
        {
            
        }

        public int Maximum
        {
            get { return (int)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }
        public readonly static DependencyProperty MaximumProperty = DependencyProperty.Register(
            "Maximum", typeof(int), typeof(IntegerTextBox), new UIPropertyMetadata(int.MaxValue));



        public int Minimum
        {
            get { return (int)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }
        public readonly static DependencyProperty MinimumProperty = DependencyProperty.Register(
            "Minimum", typeof(int), typeof(IntegerTextBox), new UIPropertyMetadata(int.MinValue));


        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetCurrentValue(ValueProperty, value); }
        }
        public readonly static DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(int), typeof(IntegerTextBox), new UIPropertyMetadata(0, (o, e) =>
            {
                IntegerTextBox tb = (IntegerTextBox)o;
                tb.RaiseValueChangedEvent(e);
            }));

        public event EventHandler<DependencyPropertyChangedEventArgs> ValueChanged;
        private void RaiseValueChangedEvent(DependencyPropertyChangedEventArgs e)
        {
            ValueChanged?.Invoke(this, e);
        }


        public int Step
        {
            get { return (int)GetValue(StepProperty); }
            set { SetValue(StepProperty, value); }
        }
        public readonly static DependencyProperty StepProperty = DependencyProperty.Register(
            "Step", typeof(int), typeof(IntegerTextBox), new UIPropertyMetadata(1));

        RepeatButton _UpButton;
        RepeatButton _DownButton;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _UpButton = Template.FindName("PART_UpButton", this) as RepeatButton;
            _DownButton = Template.FindName("PART_DownButton", this) as RepeatButton;
            _UpButton.Click += btup_Click;
            _DownButton.Click += btdown_Click;
        }


        private void btup_Click(object sender, RoutedEventArgs e)
        {
            if (Value < Maximum)
            {
                Value += Step;
                if (Value > Maximum)
                    Value = Maximum;
            }
        }

        private void btdown_Click(object sender, RoutedEventArgs e)
        {
            if (Value > Minimum)
            {
                Value -= Step;
                if (Value < Minimum)
                    Value = Minimum;
            }
        }
    }
}
