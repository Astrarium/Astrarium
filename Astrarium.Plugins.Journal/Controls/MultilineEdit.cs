using Astrarium.Types;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Astrarium.Plugins.Journal.Controls
{
    [TemplatePart(Name = PartEditor, Type = typeof(TextBox))]
    [TemplatePart(Name = PartApplyButton, Type = typeof(Button))]
    [TemplatePart(Name = PartCancelButton, Type = typeof(Button))]
    [TemplatePart(Name = PartPlaceholder, Type = typeof(TextBlock))]
    public class MultilineEdit : Control
    {
        public const string PartEditor = "PART_Editor";
        public const string PartCancelButton = "PART_CancelButton";
        public const string PartApplyButton = "PART_ApplyButton";
        public const string PartPlaceholder = "PART_Placeholder";

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        public readonly static DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(MultilineEdit),
            new FrameworkPropertyMetadata(null, (o, e) =>
            {
                
            })
            {
                BindsTwoWayByDefault = false, 
                AffectsRender = true
            });

        public readonly static DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(MultilineEdit),
            new FrameworkPropertyMetadata(null, (o, e) =>
            {
                string value = (string)e.NewValue;
                var placeholder = (o as MultilineEdit)._placeholder;
                if (placeholder != null)
                {
                    placeholder.Visibility = !(o as MultilineEdit).IsEditMode && string.IsNullOrWhiteSpace(value) ? Visibility.Visible : Visibility.Collapsed;
                }
            })
            {
                BindsTwoWayByDefault = true,
                AffectsRender = true
            });

        public readonly static DependencyProperty PlaceholderProperty = DependencyProperty.Register(
            nameof(Placeholder),
            typeof(string),
            typeof(MultilineEdit),
            new FrameworkPropertyMetadata(null, (o, e) =>
            {

            })
            {
                BindsTwoWayByDefault = false,
                AffectsRender = true
            });

        public static readonly DependencyProperty IsEditModeProperty = DependencyProperty.Register(nameof(IsEditMode), typeof(bool), typeof(MultilineEdit), new FrameworkPropertyMetadata(false) { BindsTwoWayByDefault = false, AffectsRender = true, PropertyChangedCallback = IsEditModeChanged });

        private TextBox _editor;
        private Button _cancelButton;
        private Button _applyButton;
        private TextBlock _placeholder;

        private string _text;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _editor = Template.FindName(PartEditor, this) as TextBox;
            _editor.LostFocus += _editor_LostFocus;
            _editor.MouseDoubleClick += _editor_MouseDoubleClick;
            _editor.PreviewKeyDown += _editor_PreviewKeyDown;

            _placeholder = Template.FindName(PartPlaceholder, this) as TextBlock;
            _placeholder.MouseDown += _placeholder_MouseDown;

            _cancelButton = Template.FindName(PartCancelButton, this) as Button;
            _cancelButton.Click += _cancelButton_Click;

            _applyButton = Template.FindName(PartApplyButton, this) as Button;
            _applyButton.Click += _applyButton_Click;
        }

        private static void IsEditModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as MultilineEdit;
            if (control != null)
            {
                control._applyButton.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void _placeholder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                SetEditMode();
            }
        }

        private void _applyButton_Click(object sender, RoutedEventArgs e)
        {
            SetReadMode(true);
        }

        private void _cancelButton_Click(object sender, RoutedEventArgs e)
        {
            SetReadMode(false);
        }

        private void _editor_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (IsEditMode && e.Key == Key.Enter && e.KeyboardDevice.Modifiers == ModifierKeys.Shift)
            {
                SetReadMode(true);
            }
            if (IsEditMode && e.Key == Key.Escape)
            {
                SetReadMode(false);
            }
        }

        public bool IsEditMode
        {
            get => (bool)GetValue(IsEditModeProperty);
            private set => SetValue(IsEditModeProperty, value);
        }

        private void _editor_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetEditMode();
        }

        private void _editor_LostFocus(object sender, RoutedEventArgs e)
        {
            if (IsEditMode)
            {
                if (!string.Equals(_text, Text) && ViewManager.ShowMessageBox("SaveChanges?", "Save?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    SetReadMode(true);
                }
                else
                {

                    SetReadMode(false);
                }
            }
        }

        private void SetEditMode()
        {
            _text = Text;
            IsEditMode = true;
            _placeholder.Visibility = Visibility.Collapsed;
            _applyButton.Visibility = Visibility.Visible;
            _editor.Focus();

            //var adornerLayer = AdornerLayer.GetAdornerLayer(_editor);
            //if (adornerLayer != null)
            //{
            //    adornerLayer.Update(_editor);
            //}
            //else
            //{
            //    var adornErrorText = new AdornErrorText(_editor);
            //    adornerLayer.Add(adornErrorText);
            //}
        }

        private void SetReadMode(bool applyChanges)
        {
            if (!applyChanges)
            {
                Text = _text;
                _editor.Text = _text;
            }
            else
            {
               _text = _editor.Text;
               Text = _text;
            }


            IsEditMode = false;
            _applyButton.Visibility = Visibility.Collapsed;
            _placeholder.Visibility = string.IsNullOrWhiteSpace(_editor.Text) ? Visibility.Visible : Visibility.Collapsed;
            Keyboard.ClearFocus();
        }

        internal class AdornErrorText : Adorner
        {

            internal AdornErrorText(UIElement targetElement) : base(targetElement)
            {

            }

            protected override void OnRender(DrawingContext drawingContext)
            {
                TextBox textBox = this.AdornedElement as TextBox;

                drawingContext.PushClip(new RectangleGeometry(new Rect(0, 0, this.AdornedElement.RenderSize.Width, this.AdornedElement.RenderSize.Height)));

                Dictionary<int, string> wordList = new Dictionary<int, string>();
                wordList.Add(11, "test");

                try
                {
                    foreach (KeyValuePair<int, string> keyValuePair in wordList)
                    {
                        Rect startPosition = textBox.GetRectFromCharacterIndex(keyValuePair.Key);
                        Rect endPosition = textBox.GetRectFromCharacterIndex(keyValuePair.Key + keyValuePair.Value.Length - 1, true);
                        Rect rectUnion = Rect.Union(startPosition, endPosition);
                        drawingContext.DrawLine(new Pen(Brushes.Red, 1), rectUnion.BottomLeft, rectUnion.BottomRight);
                    }
                }
                catch { }
                drawingContext.Pop();
            }
        }
    }
}
