using Astrarium.Types.Localization;
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
    public abstract class ToolbarItem : ViewModelBase
    {
        public Type Type => GetType();
    }

    public abstract class ToolbarButtonBase : ToolbarItem
    {
        public string Group
        {
            get => GetValue<string>(nameof(Group), null);
            set => SetValue(nameof(Group), value);
        }
    }

    public class ToolbarButton : ToolbarButtonBase
    {
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

    public class ToolbarToggleButton : ToolbarButtonBase
    {
        public string ImageKey
        {
            get => GetValue<string>(nameof(ImageKey), null);
            set => SetValue(nameof(ImageKey), value);
        }
       
        public ToolbarToggleButton(string imageKey, string toolTip, SimpleBinding binding, string group)
        {
            Tooltip = toolTip;
            ImageKey = imageKey;
            Group = group;          
            Text.LocaleChanged += () => NotifyPropertyChanged(nameof(Tooltip));

            AddBinding(binding);
        }

        public bool IsChecked
        {
            get => GetValue<bool>(nameof(IsChecked));
            set => SetValue(nameof(IsChecked), value);
        }

        public string Tooltip
        {
            get => GetValue<string>(nameof(Tooltip), null);
            set => SetValue(nameof(Tooltip), value);
        }
    }
}
