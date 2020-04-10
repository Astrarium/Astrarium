using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Astrarium.Controls
{
    /// <remarks>
    /// The control code and logic are based on AutoCompleteTextBox control,
    /// https://github.com/quicoli/WPF-AutoComplete-TextBox
    /// Copyright (c) 2016 Paulo Roberto Quicoli
    /// Under the MIT License
    /// </remarks>
    [TemplatePart(Name = PartEditor, Type = typeof(TextBox))]
    [TemplatePart(Name = PartPopup, Type = typeof(Popup))]
    [TemplatePart(Name = PartEmptySearch, Type = typeof(Popup))]
    [TemplatePart(Name = PartSelector, Type = typeof(Selector))]
    public class AutoCompleteTextBox : Control
    {
        public const string PartEditor = "PART_Editor";
        public const string PartPopup = "PART_Popup";
        public const string PartEmptySearch = "PART_EmptySearch";
        public const string PartSelector = "PART_Selector";
        public static readonly DependencyProperty DelayProperty = DependencyProperty.Register("Delay", typeof(int), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(200));
        public static readonly DependencyProperty DisplayMemberProperty = DependencyProperty.Register("DisplayMember", typeof(string), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(string.Empty));
        public static readonly DependencyProperty IconPlacementProperty = DependencyProperty.Register("IconPlacement", typeof(IconPlacement), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(IconPlacement.Left));
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(object), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty IconVisibilityProperty = DependencyProperty.Register("IconVisibility", typeof(Visibility), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(Visibility.Visible));
        public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register("IsDropDownOpen", typeof(bool), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty IsEmptySearchProperty = DependencyProperty.Register("IsEmptySearch", typeof(bool), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register("IsLoading", typeof(bool), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty ItemTemplateSelectorProperty = DependencyProperty.Register("ItemTemplateSelector", typeof(DataTemplateSelector), typeof(AutoCompleteTextBox));
        public static readonly DependencyProperty LoadingContentProperty = DependencyProperty.Register("LoadingContent", typeof(object), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty ProviderProperty = DependencyProperty.Register("Provider", typeof(ISuggestionProvider), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(null, OnSelectedItemChanged));
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(string.Empty));
        public static readonly DependencyProperty MaxLengthProperty = DependencyProperty.Register("MaxLength", typeof(int), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(0));
        public static readonly DependencyProperty CharacterCasingProperty = DependencyProperty.Register("CharacterCasing", typeof(CharacterCasing), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(CharacterCasing.Normal));
        public static readonly DependencyProperty MaxPopUpHeightProperty = DependencyProperty.Register("MaxPopUpHeight", typeof(int), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(600));
        public static readonly DependencyProperty WatermarkProperty = DependencyProperty.Register("Watermark", typeof(string), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(string.Empty));
        public static readonly DependencyProperty SelectionCommitProperty = DependencyProperty.Register("SelectionCommit", typeof(ICommand), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty SuggestionBackgroundProperty = DependencyProperty.Register("SuggestionBackground", typeof(Brush), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(Brushes.White));
        private bool _isUpdatingText;
        private bool _isSelectionCommit;
        private string _filter;

        private SuggestionsAdapter _suggestionsAdapter;
        private BindingEvaluator _bindingEvaluator;
        private SelectionAdapter _selectionAdapter;
        private DispatcherTimer _fetchTimer;

        private TextBox _editor;
        private Selector _itemsSelector;
        private Popup _popup;
        private Popup _emptySearch;

        static AutoCompleteTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(typeof(AutoCompleteTextBox)));
        }

        public int MaxPopupHeight
        {
            get => (int)GetValue(MaxPopUpHeightProperty);
            set => SetValue(MaxPopUpHeightProperty, value);
        }

        public CharacterCasing CharacterCasing
        {
            get => (CharacterCasing)GetValue(CharacterCasingProperty);
            set => SetValue(CharacterCasingProperty, value);
        }

        public int MaxLength
        {
            get => (int)GetValue(MaxLengthProperty);
            set => SetValue(MaxLengthProperty, value);
        }

        public int Delay
        {
            get => (int)GetValue(DelayProperty);
            set => SetValue(DelayProperty, value);
        }

        public string DisplayMember
        {
            get => (string)GetValue(DisplayMemberProperty);
            set => SetValue(DisplayMemberProperty, value);
        }

        public object Icon
        {
            get => GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public IconPlacement IconPlacement
        {
            get => (IconPlacement)GetValue(IconPlacementProperty);
            set => SetValue(IconPlacementProperty, value);
        }

        public Visibility IconVisibility
        {
            get => (Visibility)GetValue(IconVisibilityProperty);
            set => SetValue(IconVisibilityProperty, value);
        }

        public bool IsDropDownOpen
        {
            get => (bool)GetValue(IsDropDownOpenProperty);
            set => SetValue(IsDropDownOpenProperty, value);
        }

        public bool IsEmptySearch
        {
            get => (bool)GetValue(IsEmptySearchProperty);
            set => SetValue(IsEmptySearchProperty, value);
        }

        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set => SetValue(IsLoadingProperty, value);
        }

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        public DataTemplate ItemTemplate
        {
            get => (DataTemplate)GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        public DataTemplateSelector ItemTemplateSelector
        {
            get => ((DataTemplateSelector)(GetValue(ItemTemplateSelectorProperty)));
            set => SetValue(ItemTemplateSelectorProperty, value);
        }

        public ICommand SelectionCommit
        {
            get => (ICommand)GetValue(SelectionCommitProperty);
            set => SetValue(SelectionCommitProperty, value);
        }

        public object LoadingContent
        {
            get => GetValue(LoadingContentProperty);
            set => SetValue(LoadingContentProperty, value);
        }

        public ISuggestionProvider Provider
        {
            get => (ISuggestionProvider)GetValue(ProviderProperty);
            set => SetValue(ProviderProperty, value);
        }

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
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

        public Brush SuggestionBackground
        {
            get => (Brush)GetValue(SuggestionBackgroundProperty);
            set => SetValue(SuggestionBackgroundProperty, value);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _editor = Template.FindName(PartEditor, this) as TextBox;
            _popup = Template.FindName(PartPopup, this) as Popup;
            _emptySearch = Template.FindName(PartEmptySearch, this) as Popup;
            _itemsSelector = Template.FindName(PartSelector, this) as Selector;
            _bindingEvaluator = new BindingEvaluator(new Binding(DisplayMember));

            if (_editor != null)
            {
                _editor.TextChanged += OnEditorTextChanged;
                _editor.PreviewKeyDown += OnEditorKeyDown;
                _editor.LostFocus += OnEditorLostFocus;
                _editor.GotFocus += OnEditorGotFocus;
                _editor.IsKeyboardFocusWithinChanged += OnEditorIsKeyboardFocusWithinChanged;

                if (SelectedItem != null)
                {
                    _isUpdatingText = true;
                    _editor.Text = _bindingEvaluator.Evaluate(SelectedItem);
                    _isUpdatingText = false;
                }
            }

            GotFocus += AutoCompleteTextBox_GotFocus;

            if (_popup != null)
            {
                _popup.StaysOpen = false;
                _popup.Opened += OnPopupOpened;
            }
            if (_itemsSelector != null)
            {
                _selectionAdapter = new SelectionAdapter(_itemsSelector);
                _selectionAdapter.Commit += OnSelectionAdapterCommit;
                _selectionAdapter.Cancel += OnSelectionAdapterCancel;
                _selectionAdapter.SelectionChanged += OnSelectionAdapterSelectionChanged;
                _itemsSelector.PreviewMouseDown += ItemsSelector_PreviewMouseDown;
            }
        }

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AutoCompleteTextBox act = d as AutoCompleteTextBox;
            if (act != null)
            {
                if (act._editor != null & !act._isUpdatingText)
                {
                    act._isUpdatingText = true;
                    act._editor.Text = act._bindingEvaluator.Evaluate(e.NewValue);
                    act._isUpdatingText = false;
                }
            }
        }

        private void ScrollToSelectedItem()
        {
            if (_itemsSelector is ListBox listBox && listBox.SelectedItem != null)
                listBox.ScrollIntoView(listBox.SelectedItem);
        }

        private void OnEditorIsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _editor?.Clear();
        }

        private void OnEditorGotFocus(object sender, RoutedEventArgs e)
        {
            _editor?.Clear();
        }

        private void ItemsSelector_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((e.OriginalSource as FrameworkElement)?.DataContext == null)
                return;
            if (!_itemsSelector.Items.Contains(((FrameworkElement)e.OriginalSource)?.DataContext))
                return;
            _itemsSelector.SelectedItem = ((FrameworkElement)e.OriginalSource)?.DataContext;
            OnSelectionAdapterCommit();
        }

        private void AutoCompleteTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            _editor?.Clear();
            _editor?.Focus();
        }

        private string GetDisplayText(object dataItem)
        {
            if (_bindingEvaluator == null)
            {
                _bindingEvaluator = new BindingEvaluator(new Binding(DisplayMember));
            }
            if (dataItem == null)
            {
                return string.Empty;
            }
            if (string.IsNullOrEmpty(DisplayMember))
            {
                return dataItem.ToString();
            }
            return _bindingEvaluator.Evaluate(dataItem);
        }

        private void OnEditorKeyDown(object sender, KeyEventArgs e)
        {
            if (_selectionAdapter != null)
            {
                if (IsDropDownOpen)
                    _selectionAdapter.HandleKeyDown(e);
                else
                    IsDropDownOpen = e.Key == Key.Down || e.Key == Key.Up;
            }

            if (e.Key == Key.Escape)
            {
                _editor.Clear();
            }
        }

        private void OnEditorLostFocus(object sender, RoutedEventArgs e)
        {
            if (!IsKeyboardFocusWithin)
            {
                IsDropDownOpen = false;
            }
            IsEmptySearch = false;
            _editor.Clear();
        }

        private void OnEditorTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingText)
                return;
            if (_fetchTimer == null)
            {
                _fetchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(Delay) };
                _fetchTimer.Tick += OnFetchTimerTick;
            }
            _fetchTimer.IsEnabled = false;
            _fetchTimer.Stop();
            SetSelectedItem(null);
            if (_editor.Text.Length > 0)
            {
                _fetchTimer.IsEnabled = true;
                _fetchTimer.Start();
            }
            else
            {
                IsDropDownOpen = false;
                IsEmptySearch = false;
            }
        }

        private void OnFetchTimerTick(object sender, EventArgs e)
        {
            _fetchTimer.IsEnabled = false;
            _fetchTimer.Stop();
            if (Provider != null && _itemsSelector != null)
            {
                _filter = _editor.Text;
                if (_suggestionsAdapter == null)
                {
                    _suggestionsAdapter = new SuggestionsAdapter(this);
                }
                _suggestionsAdapter.GetSuggestions(_filter);
            }
        }

        private void OnPopupOpened(object sender, EventArgs e)
        {
            _itemsSelector.SelectedItem = SelectedItem;
        }

        private void OnSelectionAdapterCancel()
        {
            _isUpdatingText = true;
            _editor.Text = SelectedItem == null ? _filter : GetDisplayText(SelectedItem);
            _isUpdatingText = false;
            IsDropDownOpen = false;
            IsEmptySearch = false;
        }

        private void OnSelectionAdapterCommit()
        {   
            if (!_isSelectionCommit && _itemsSelector.SelectedItem != null)
            {
                _isSelectionCommit = true;

                var item = _itemsSelector.SelectedItem;

                _isUpdatingText = true;
                //_editor.Text = GetDisplayText(item);
                _editor.Clear();

                SetSelectedItem(item);
                
                _isUpdatingText = false;
                IsDropDownOpen = false;
                IsEmptySearch = false;

                var comm = GetValue(SelectionCommitProperty) as ICommand;
                if (comm != null && item != null)
                {
                    comm.Execute(item);
                }

                _isSelectionCommit = false;
            }
        }

        private void OnSelectionAdapterSelectionChanged()
        {
            _isUpdatingText = true;
            _editor.Text = _itemsSelector.SelectedItem == null ? _filter : GetDisplayText(_itemsSelector.SelectedItem);
            _editor.SelectionStart = _editor.Text.Length;
            _editor.SelectionLength = 0;
            ScrollToSelectedItem();
            _isUpdatingText = false;
        }

        private void SetSelectedItem(object item)
        {
            _isUpdatingText = true;
            SelectedItem = item;
            _isUpdatingText = false;
        }

        private class SuggestionsAdapter
        {
            private readonly AutoCompleteTextBox _actb;
            private string _filter;
            public SuggestionsAdapter(AutoCompleteTextBox actb)
            {
                _actb = actb;
            }

            public void GetSuggestions(string searchText)
            {
                _filter = searchText;
                _actb.IsLoading = true;
                _actb._itemsSelector.ItemsSource = null;
                new Thread(GetSuggestionsAsync).Start(new object[] { searchText, _actb.Provider });
            }

            private void DisplaySuggestions(IEnumerable suggestions, string filter)
            {
                if (_filter != filter)
                {
                    return;
                }
                if (_actb.IsLoading)
                {
                    _actb.IsLoading = false;
                    _actb._itemsSelector.ItemsSource = suggestions;
                    _actb.IsDropDownOpen = _actb._itemsSelector.HasItems;

                    if (_actb._itemsSelector.HasItems)
                    {
                        _actb._itemsSelector.SelectedIndex = 0;
                    }

                    _actb.IsEmptySearch = !_actb.IsDropDownOpen;
                }
            }

            private void GetSuggestionsAsync(object param)
            {
                if (param is object[] args)
                {
                    string searchText = Convert.ToString(args[0]);
                    if (args[1] is ISuggestionProvider provider)
                    {
                        IEnumerable list = provider.GetSuggestions(searchText);
                        _actb.Dispatcher.BeginInvoke(new Action<IEnumerable, string>(DisplaySuggestions), DispatcherPriority.Background, list, searchText);
                    }
                }
            }
        }

        private class BindingEvaluator : FrameworkElement
        {
            public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(string), typeof(BindingEvaluator), new FrameworkPropertyMetadata(string.Empty));

            public BindingEvaluator(Binding binding)
            {
                ValueBinding = binding;
            }

            public string Value
            {
                get => (string)GetValue(ValueProperty);
                set => SetValue(ValueProperty, value);
            }

            public Binding ValueBinding { get; set; }

            public string Evaluate(object dataItem)
            {
                DataContext = dataItem;
                SetBinding(ValueProperty, ValueBinding);
                return Value;
            }
        }

        private class SelectionAdapter
        {
            public SelectionAdapter(Selector selector)
            {
                SelectorControl = selector;
                SelectorControl.PreviewMouseUp += OnSelectorMouseDown;
            }

            public delegate void CancelEventHandler();
            public delegate void CommitEventHandler();
            public delegate void SelectionChangedEventHandler();

            public event CancelEventHandler Cancel;
            public event CommitEventHandler Commit;
            public event SelectionChangedEventHandler SelectionChanged;

            public Selector SelectorControl { get; set; }

            public void HandleKeyDown(KeyEventArgs key)
            {
                switch (key.Key)
                {
                    case Key.Down:
                        IncrementSelection();
                        break;
                    case Key.Up:
                        DecrementSelection();
                        break;
                    case Key.Enter:
                        Commit?.Invoke();
                        break;
                    case Key.Escape:
                        Cancel?.Invoke();
                        break;
                    default:
                        break;
                }
            }

            private void DecrementSelection()
            {
                if (SelectorControl.SelectedIndex == -1)
                {
                    SelectorControl.SelectedIndex = SelectorControl.Items.Count - 1;
                }
                else
                {
                    SelectorControl.SelectedIndex -= 1;
                }

                SelectionChanged?.Invoke();
            }

            private void IncrementSelection()
            {
                if (SelectorControl.SelectedIndex == SelectorControl.Items.Count - 1)
                {
                    SelectorControl.SelectedIndex = -1;
                }
                else
                {
                    SelectorControl.SelectedIndex += 1;
                }

                SelectionChanged?.Invoke();
            }

            private void OnSelectorMouseDown(object sender, MouseButtonEventArgs e)
            {
                Commit?.Invoke();
            }
        }
    }

    public interface ISuggestionProvider
    {
        IEnumerable GetSuggestions(string filter);
    }

    public enum IconPlacement
    {
        Left,
        Right
    }
}
