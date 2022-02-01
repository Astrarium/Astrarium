using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Astrarium.Types.Controls
{
    [TemplatePart(Name = PartEditor, Type = typeof(TextBox))]
    [TemplatePart(Name = PartUpButton, Type = typeof(RepeatButton))]
    [TemplatePart(Name = PartDownButton, Type = typeof(RepeatButton))]
    public class TimeInput : Control
    {
        public const string PartEditor = "PART_Editor";
        public const string PartUpButton = "PART_UpButton";
        public const string PartDownButton = "PART_DownButton";

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(double), typeof(TimeInput), new FrameworkPropertyMetadata(0d) { AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, PropertyChangedCallback = new PropertyChangedCallback(ValuePropertyChanged), BindsTwoWayByDefault = true });

        public static readonly DependencyProperty ShowSecondsProperty = DependencyProperty.Register(nameof(ShowSeconds), typeof(bool), typeof(TimeInput), new FrameworkPropertyMetadata(false) { AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, PropertyChangedCallback = new PropertyChangedCallback(ShowSecondsPropertyChanged), BindsTwoWayByDefault = true });

        private string oldValue = "";
        private TextBox editor;
        private RepeatButton upButton;
        private RepeatButton downButton;
        private string currentGroup = "";

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public bool ShowSeconds
        {
            get => (bool)GetValue(ShowSecondsProperty);
            set => SetValue(ShowSecondsProperty, value);
        }

        private static void ValuePropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            TimeInput @this = (TimeInput)sender;
            bool showSeconds = @this.ShowSeconds;
            double value = (double)e.NewValue;
            @this.editor.Text = ValueToString(value, showSeconds);
        }

        private static void ShowSecondsPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            TimeInput @this = (TimeInput)sender;
            bool showSeconds = (bool)e.NewValue;
            double value = @this.Value;
            if (@this.editor != null)
            {
                @this.editor.Text = ValueToString(value, showSeconds);
            }
        }

        static TimeInput()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TimeInput), new FrameworkPropertyMetadata(typeof(TimeInput)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            editor = Template.FindName(PartEditor, this) as TextBox;
            upButton = Template.FindName(PartUpButton, this) as RepeatButton;
            downButton = Template.FindName(PartDownButton, this) as RepeatButton;

            editor.IsReadOnly = true;
            editor.IsReadOnlyCaretVisible = false;

            upButton.Click += Increment;
            downButton.Click += Decrement;

            if (editor != null)
            {
                editor.Text = ShowSeconds ? "00:00:00" : "00:00";
                editor.SelectionChanged += EditorSelectionChanged;
                editor.PreviewKeyDown += OnEditorKeyDown;
                editor.LostFocus += Editor_LostFocus;
            }
        }

        private void Editor_LostFocus(object sender, RoutedEventArgs e)
        {
            TryParceValueFromEditorText();
            EndEdit();
        }

        private static string ValueToString(double value, bool showSeconds)
        {
            string format = showSeconds ? "hh\\:mm\\:ss" : "hh\\:mm";
            return TimeSpan.FromHours(value).ToString(format, CultureInfo.InvariantCulture);
        }

        private static double StringToValue(string str, bool showSeconds)
        {
            string format = showSeconds ? "HH:mm:ss" : "HH:mm";
            return DateTime.ParseExact(str, format, CultureInfo.InvariantCulture).TimeOfDay.TotalHours;
        }

        private void TryParceValueFromEditorText()
        {
            try
            {
                if (ValueToString(Value, ShowSeconds) != editor.Text)
                {
                    Value = StringToValue(editor.Text, ShowSeconds);
                    oldValue = "";
                }
            }
            catch { }
        }

        private void EndEdit()
        {
            editor.SelectionChanged -= EditorSelectionChanged;
            editor.Select(0, 0);
            editor.SelectionChanged += EditorSelectionChanged;
        }

        private void EditorSelectionChanged(object sender, RoutedEventArgs e)
        {
            int group = GetGroupIndex();
            int start = group * 3;
            if (start != editor.SelectionStart || editor.SelectionLength != 2)
            {
                editor.SelectionChanged -= EditorSelectionChanged;
                editor.SelectionStart = start;
                editor.SelectionLength = 2;
                editor.SelectionChanged += EditorSelectionChanged;
            }
        }

        private void Increment(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(oldValue))
            {
                oldValue = editor.Text;
            }

            int group = GetGroupIndex();
            ProcessGroups(groups => groups[group]++);
        }

        private void Decrement(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(oldValue))
            {
                oldValue = editor.Text;
            }

            int group = GetGroupIndex();
            ProcessGroups(groups => groups[group]--);
        }

        private void OnEditorKeyDown(object sender, KeyEventArgs e)
        {
            if (string.IsNullOrEmpty(oldValue))
            {
                oldValue = editor.Text;
            }

            int group = GetGroupIndex();

            if (group > 0 && e.Key == Key.Left)
            {
                editor.SelectionStart = GroupToSelectionStart(group - 1);
                editor.SelectionLength = 2;
                currentGroup = "";
            }
            else if (group < 2 && e.Key == Key.Right)
            {
                editor.SelectionStart = GroupToSelectionStart(group + 1);
                editor.SelectionLength = 2;
                currentGroup = "";
            }
            else if (e.Key == Key.Up)
            {
                ProcessGroups(groups => groups[group]++);
            }
            else if (e.Key == Key.Down)
            {
                ProcessGroups(groups => groups[group]--);
            }
            else if (e.Key == Key.Delete)
            {
                ProcessGroups(groups => groups[group] = 0);
                editor.SelectionStart = GroupToSelectionStart(group);
                editor.SelectionLength = 2;
                currentGroup = "";
            }
            else if (e.Key >= Key.D0 && e.Key <= Key.D9)
            {
                int num = e.Key - Key.D0;
                ProcessDigitKey(group, num);
            }
            else if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
            {
                int num = e.Key - Key.NumPad0;
                ProcessDigitKey(group, num);
            }
            else if (e.Key == Key.Back)
            {
                ProcessGroups(groups => groups[group] = 0);

                if (group > 0)
                {
                    editor.SelectionStart = GroupToSelectionStart(group - 1);
                    editor.SelectionLength = 2;
                    currentGroup = "";
                }
            }
            else if (e.Key == Key.Enter)
            {
                TryParceValueFromEditorText();
                EndEdit();
            }
            else if (e.Key == Key.Escape)
            {
                editor.Text = oldValue;
                TryParceValueFromEditorText();
                EndEdit();
            }

            e.Handled = true;
        }

        private void ProcessDigitKey(int group, int num)
        {
            ProcessGroups(groups =>
            {
                if (currentGroup.Length < 2)
                {
                    currentGroup += num.ToString();
                    groups[group] = int.Parse(currentGroup);

                    if (currentGroup.Length == 2)
                    {
                        editor.SelectionStart = GroupToSelectionStart(group + 1);
                        editor.SelectionLength = 2;
                        currentGroup = "";
                    }
                }
            });
        }

        private void ProcessGroups(Action<int[]> action)
        {
            int[] groups = editor.Text.Split(':').Select(g => int.Parse(g)).ToArray();

            action(groups);

            groups[0] = Math.Max(0, Math.Min(groups[0], 23));
            groups[1] = Math.Max(0, Math.Min(groups[1], 59));

            if (ShowSeconds)
            {
                groups[2] = Math.Max(0, Math.Min(groups[2], 59));
            }

            int start = editor.SelectionStart;
            string text = string.Join(":", groups.Select(g => $"{g:D2}"));

            var value = StringToValue(text, ShowSeconds);
            if (editor.Text != text)
            {
                editor.Text = ValueToString(value, ShowSeconds);
            }
            editor.SelectionStart = start;
            editor.SelectionLength = 2;
        }

        private int GetGroupIndex()
        {
            return editor.SelectionStart / 3;
        }

        private int GroupToSelectionStart(int group)
        {
            return group * 3;
        }
    }
}
