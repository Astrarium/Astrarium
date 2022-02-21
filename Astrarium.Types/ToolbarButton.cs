using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace Astrarium.Types
{
    public abstract class ToolbarItem : PropertyChangedBase
    {
        public Type Type => GetType();
    }

    public abstract class ToolbarButtonBase : ToolbarItem
    {
        public string ImageKey
        {
            get => GetValue<string>(nameof(ImageKey), null);
            set => SetValue(nameof(ImageKey), value);
        }

        public string Tooltip
        {
            get => GetValue<string>(nameof(Tooltip), null);
            set => SetValue(nameof(Tooltip), value);
        }
    }

    public class ToolbarButton : ToolbarButtonBase
    {
        public ToolbarButton(string imageKey, string toolTip, Command command)
        {
            ImageKey = imageKey;
            Tooltip = toolTip;
            Command = command;
            Text.LocaleChanged += () => NotifyPropertyChanged(nameof(Tooltip));
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
    }

    public class ToolbarToggleButton : ToolbarButton
    {
        public ToolbarToggleButton(string imageKey, string toolTip, SimpleBinding checkedBinding) : base(imageKey, toolTip, null)
        {
            AddBinding(checkedBinding);
        }

        public bool IsChecked
        {
            get => GetValue<bool>(nameof(IsChecked));
            set => SetValue(nameof(IsChecked), value);
        }
    }
}
