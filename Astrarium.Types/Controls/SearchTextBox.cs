using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Astrarium.Types.Controls
{
    [TemplatePart(Name = PartEditor, Type = typeof(TextBox))]
    public class SearchTextBox : Control
    {
        public const string PartEditor = "PART_Editor";
        public const string PartWatermark = "PART_Watermark";
        public const string PartClear = "PART_Clear";

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(SearchTextBox), new FrameworkPropertyMetadata(string.Empty) { BindsTwoWayByDefault = true, PropertyChangedCallback = OnTextPropertyChanged });
        public static readonly DependencyProperty WatermarkProperty = DependencyProperty.Register("Watermark", typeof(string), typeof(SearchTextBox), new FrameworkPropertyMetadata(string.Empty));

        private TextBox _editor;
        private TextBlock _watermark;
        private Button _clear;

        static SearchTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchTextBox), new FrameworkPropertyMetadata(typeof(SearchTextBox)));
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public string Watermark
        {
            get => (string)GetValue(WatermarkProperty);
            set => SetValue(WatermarkProperty, value);
        }

        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            SearchTextBox search = d as SearchTextBox;
            string value = args.NewValue as string;
            if (search._editor != null)
                search._editor.Text = value;
            if (search._watermark != null)
                search._watermark.Visibility = string.IsNullOrEmpty(value) ? Visibility.Visible : Visibility.Collapsed;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _editor = Template.FindName(PartEditor, this) as TextBox;
            _watermark = Template.FindName(PartWatermark, this) as TextBlock;
            _clear = Template.FindName(PartClear, this) as Button;

            _editor.TextChanged += _editor_TextChanged;
            _clear.Click += _clear_Click;

            GotFocus += AutoCompleteTextBox_GotFocus;
            LostFocus += SearchTextBox_LostFocus;
        }

        private void _clear_Click(object sender, RoutedEventArgs e)
        {
            _editor.Text = null;
            Keyboard.ClearFocus();
            _watermark.Visibility = Visibility.Visible;
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            _watermark.Visibility = string.IsNullOrEmpty(Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void _editor_TextChanged(object sender, TextChangedEventArgs e)
        {
            Text = _editor.Text;
            _clear.Visibility = string.IsNullOrEmpty(Text) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void AutoCompleteTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            _watermark.Visibility = Visibility.Collapsed;
        }
    }
}
