using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Astrarium.Types
{
    /// <summary>
    /// Represents ViewModel for single menu item
    /// </summary>
    public class MenuItem : PropertyChangedBase
    {
        public MenuItem(string title) : this(title, null) { }

        public MenuItem(string title, ICommand command) : this(title, command, null) { }

        public MenuItem(string title, ICommand command, object commandParameter)
        {
            this.Header = title;
            this.Command = command;
            this.CommandParameter = commandParameter;
            this.SubItems = new ObservableCollection<MenuItem>();
            Text.LocaleChanged += HandleLocaleChanged;
        }

        private void HandleLocaleChanged()
        {
            NotifyPropertyChanged(nameof(Header), nameof(Tooltip));
        }

        public bool IsCheckable
        {
            get => GetValue<bool>(nameof(IsCheckable));
            set => SetValue(nameof(IsCheckable), value);
        }

        public bool IsChecked
        {
            get => GetValue<bool>(nameof(IsChecked));
            set => SetValue(nameof(IsChecked), value);
        }

        public bool IsEnabled
        {
            get => GetValue<bool>(nameof(IsEnabled), true);
            set => SetValue(nameof(IsEnabled), value);
        }

        public bool IsVisible
        {
            get => GetValue<bool>(nameof(IsVisible), true);
            set => SetValue(nameof(IsVisible), value);
        }

        public string IconKey
        {
            get => GetValue<string>(nameof(IconKey), null);
            set => SetValue(nameof(IconKey), value);
        }

        public string Header
        {
            get => GetValue<string>(nameof(Header), null);
            set => SetValue(nameof(Header), value);
        }

        public string Tooltip
        {
            get => GetValue<string>(nameof(Tooltip), null);
            set => SetValue(nameof(Tooltip), value);
        }

        public string InputGestureText
        {
            get
            {
                var gesture = GetValue<KeyGesture>(nameof(HotKey), null);
                return gesture?.DisplayString;
            }
        }

        public KeyGesture HotKey
        {
            get => GetValue<KeyGesture>(nameof(HotKey), null);
            set
            {
                SetValue(nameof(HotKey), value);
                NotifyPropertyChanged(nameof(InputGestureText));
            }
        }

        public ICommand Command
        {
            get => GetValue<ICommand>(nameof(Command), null);
            set => SetValue(nameof(Command), value);
        }

        public object CommandParameter
        {
            get => GetValue<object>(nameof(CommandParameter), null);
            set => SetValue(nameof(CommandParameter), value);
        }

        public ObservableCollection<MenuItem> SubItems
        {
            get => GetValue(nameof(SubItems), new ObservableCollection<MenuItem>());
            set => SetValue(nameof(SubItems), value);
        }
    }

    public enum MenuItemPosition
    {
        ContextMenu = 0,
        MainMenuTools = 1,
        MainMenuTop = 2
    }
}
