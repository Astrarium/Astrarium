using Astrarium.Types.Localization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace Astrarium.Types
{
    public class MenuItem : ViewModelBase
    {
        public MenuItem(string title)
        {
            this.Header = title;
        }

        public MenuItem(string title, ICommand command)
        {
            this.Header = title;
            this.Command = command;
        }

        public MenuItem(string title, ICommand command, object commandParameter)
        {
            this.Header = title;
            this.Command = command;
            this.CommandParameter = commandParameter;
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

        public string Header { get; private set; }
        public string InputGestureText { get; set; }
        public ICommand Command { get; set; }
        public object CommandParameter { get; set; }
        public ObservableCollection<MenuItem> SubItems { get; set; }
    }
}
