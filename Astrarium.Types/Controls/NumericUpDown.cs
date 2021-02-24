using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Astrarium.Types.Controls
{
    public class NumericUpDown : Control
    {
        public bool Loop
        {
            get { return (bool)GetValue(LoopProperty); }
            set { SetValue(LoopProperty, value); }
        }

        public readonly static DependencyProperty LoopProperty = DependencyProperty.Register(
            "Loop", typeof(bool), typeof(NumericUpDown), new UIPropertyMetadata(false));

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

        public uint DecimalPlaces
        {
            get { return (uint)GetValue(DecimalPlacesProperty); }
            set { SetValue(DecimalPlacesProperty, value); }
        }
        public readonly static DependencyProperty DecimalPlacesProperty = DependencyProperty.Register(
            "DecimalPlaces", typeof(uint), typeof(NumericUpDown), new UIPropertyMetadata((uint)2));

        public decimal Value
        {
            get 
            {
                decimal value = (decimal)GetValue(ValueProperty);
                if (value > Maximum)
                    value = Maximum;
                if (value < Minimum)
                    value = Minimum;
                return value; 
            }
            set 
            {
                if (value > Maximum)
                    value = Maximum;
                if (value < Minimum)
                    value = Minimum;
                SetCurrentValue(ValueProperty, value); 
            }
        }
        public readonly static DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(decimal), typeof(NumericUpDown), new FrameworkPropertyMetadata((decimal)0, (o, e) =>
            {
                decimal value = (decimal)e.NewValue;
                NumericUpDown tb = (NumericUpDown)o;

                if (value > tb.Maximum)
                    tb.Value = tb.Maximum;
                if (value < tb.Minimum)
                    tb.Value = tb.Minimum;

                tb.RaiseValueChangedEvent(e);
            })
            {
                BindsTwoWayByDefault = true,
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

        public readonly static DependencyProperty LoopIncrementCommandProperty = DependencyProperty.Register(
            "LoopIncrementCommand", typeof(ICommand), typeof(NumericUpDown));

        public ICommand LoopIncrementCommand
        {
            get { return (ICommand)GetValue(LoopIncrementCommandProperty); }
            set { SetValue(LoopIncrementCommandProperty, value); }
        }

        public readonly static DependencyProperty LoopDecrementCommandProperty = DependencyProperty.Register(
            "LoopDecrementCommand", typeof(ICommand), typeof(NumericUpDown));

        public ICommand LoopDecrementCommand
        {
            get { return (ICommand)GetValue(LoopDecrementCommandProperty); }
            set { SetValue(LoopDecrementCommandProperty, value); }
        }

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
            _TextBox.LostFocus += TextBox_LostFocus;
            _TextBox.PreviewTextInput += TextBox_PreviewTextInput;
            _TextBox.PreviewKeyDown += TextBox_PreviewKeyDown;
            DataObject.AddPastingHandler(_TextBox, new DataObjectPastingEventHandler(TextBox_PreviewPaste));
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            _TextBox.Text = Value.ToString(CultureInfo.InvariantCulture);
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
            var numberStyles = (DecimalPlaces == 0) ? NumberStyles.Integer : NumberStyles.Float;
            bool isMatch = decimal.TryParse(text, numberStyles, CultureInfo.InvariantCulture, out decimal result);
            return isMatch;
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (DecimalPlaces == 0 && !e.Text.All(c => char.IsNumber(c)))
                e.Handled = true;
            else 
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
            else if (Loop)
            {
                LoopIncrementCommand?.Execute(null);
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
            else if (Loop)
            {
                LoopDecrementCommand?.Execute(null);
            }
        }
    }
}
