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

namespace Planetarium.Controls
{
    public class NumericUpDown : Control
    {
        public NumericUpDown()
        {
            
        }

        public bool HideButtons
        {
            get { return (bool)GetValue(HideButtonsProperty); }
            set { SetValue(HideButtonsProperty, value); }
        }

        public readonly static DependencyProperty HideButtonsProperty = DependencyProperty.Register(
            "HideButtons", typeof(bool), typeof(NumericUpDown), new UIPropertyMetadata(false));

        public decimal Maximum
        {
            get { return (decimal)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }
        public readonly static DependencyProperty MaximumProperty = DependencyProperty.Register(
            "Maximum", typeof(decimal), typeof(NumericUpDown), new UIPropertyMetadata((decimal)100, (o, e) => {
                NumericUpDown tb = (NumericUpDown)o;
                decimal maximum = (decimal)e.NewValue;
                if (tb.Value > maximum)
                {
                    tb.Value = maximum;
                }
            }));

        public decimal Minimum
        {
            get { return (decimal)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }
        public readonly static DependencyProperty MinimumProperty = DependencyProperty.Register(
            "Minimum", typeof(decimal), typeof(NumericUpDown), new UIPropertyMetadata((decimal)0, (o, e) =>
            {
                NumericUpDown tb = (NumericUpDown)o;
                decimal minimum = (decimal)e.NewValue;
                if (tb.Value < minimum)
                {
                    tb.Value = minimum;
                }
            }));

        public decimal Value
        {
            get { return (decimal)GetValue(ValueProperty); }
            set { SetCurrentValue(ValueProperty, value); }
        }
        public readonly static DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(decimal), typeof(NumericUpDown), new FrameworkPropertyMetadata((decimal)0, (o, e) =>
            {
                NumericUpDown tb = (NumericUpDown)o;
                tb.RaiseValueChangedEvent(e);
            })
            {
                BindsTwoWayByDefault = true,
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

        public event EventHandler<DependencyPropertyChangedEventArgs> ValueChanged;
        private void RaiseValueChangedEvent(DependencyPropertyChangedEventArgs e)
        {
            ValueChanged?.Invoke(this, e);
        }

        public decimal Step
        {
            get { return (decimal)GetValue(StepProperty); }
            set { SetValue(StepProperty, value); }
        }
        public readonly static DependencyProperty StepProperty = DependencyProperty.Register(
            "Step", typeof(decimal), typeof(NumericUpDown), new UIPropertyMetadata((decimal)1));

        TextBox _TextBox;
        RepeatButton _UpButton;
        RepeatButton _DownButton;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _TextBox = Template.FindName("PART_TextBox", this) as TextBox;
            _UpButton = Template.FindName("PART_UpButton", this) as RepeatButton;
            _DownButton = Template.FindName("PART_DownButton", this) as RepeatButton;
            _UpButton.Click += Increment;
            _DownButton.Click += Decrement;
            _TextBox.PreviewTextInput += TextBox_PreviewTextInput;
            _TextBox.PreviewKeyDown += TextBox_PreviewKeyDown;
            DataObject.AddPastingHandler(_TextBox, new DataObjectPastingEventHandler(TextBox_PreviewPaste));
        }

        private void TextBox_PreviewPaste(object sender, DataObjectPastingEventArgs e)
        {
            var isText = e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true);
            if (isText)
            {
                var text = e.SourceDataObject.GetData(DataFormats.UnicodeText) as string;             
                e.Handled = !OnValidateText(FullText(text));
                if (e.Handled)
                {
                    e.CancelCommand();
                }
            }
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                Increment(sender, e);
            }
            else if (e.Key == Key.Down)
            {
                Decrement(sender, e);
            }
        }

        protected virtual bool OnValidateText(string text)
        {
            bool isMatch = decimal.TryParse(text, out decimal result);
            if (isMatch)
            {
                isMatch = (result >= Minimum && result <= Maximum);
            }

            return isMatch;
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !OnValidateText(FullText(e.Text));
        }

        private string FullText(string newText)
        {
            string text;
            if (_TextBox.Text.Length < _TextBox.CaretIndex)
                text = _TextBox.Text;
            else
            {
                string remainingTextAfterRemoveSelection;

                text = TreatSelectedText(out remainingTextAfterRemoveSelection)
                    ? remainingTextAfterRemoveSelection.Insert(_TextBox.SelectionStart, newText)
                    : _TextBox.Text.Insert(_TextBox.CaretIndex, newText);
            }

            return text;
        }

        private bool TreatSelectedText(out string text)
        {
            text = null;
            if (_TextBox.SelectionLength <= 0)
                return false;

            var length = _TextBox.Text.Length;
            if (_TextBox.SelectionStart >= length)
                return true;

            if (_TextBox.SelectionStart + _TextBox.SelectionLength >= length)
                _TextBox.SelectionLength = length - _TextBox.SelectionStart;

            text = _TextBox.Text.Remove(_TextBox.SelectionStart, _TextBox.SelectionLength);
            return true;
        }

        private void Increment(object sender, RoutedEventArgs e)
        {
            if (Value < Maximum)
            {
                Value += Step;
                if (Value > Maximum)
                    Value = Maximum;
            }
        }

        private void Decrement(object sender, RoutedEventArgs e)
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
