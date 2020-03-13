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
        public string ButtonName { get; set; }
        public string Group { get; protected set; }
    }

    public class ToolbarButton : ToolbarButtonBase
    {
        public ICommand ButtonCommand { get; set; }
    }

    public class ToolbarSeparator : ToolbarItem
    {

    }

    public class ToolbarToggleButton : ToolbarButtonBase
    {
        public string TooltipKey { get; private set; }
        public string ImageKey { get; private set; }
        public string Tooltip => Text.Get(TooltipKey);
        public SimpleBinding IsCheckedBinding { get; private set; }
       
        public ToolbarToggleButton(string tooltipKey, string imageKey, SimpleBinding isCheckedBinding, string group)
        {
            TooltipKey = tooltipKey;
            ImageKey = imageKey;
            Group = group;

            IsCheckedBinding = isCheckedBinding;
            IsCheckedBinding.Source.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == isCheckedBinding.PropertyName)
                {
                    NotifyPropertyChanged(nameof(IsChecked));
                }
            };

            Text.LocaleChanged += () => NotifyPropertyChanged(nameof(Tooltip));
        }

        public bool IsChecked
        {
            get
            {
                if (IsCheckedBinding.Source is ISettings)
                    return (IsCheckedBinding.Source as ISettings).Get<bool>(IsCheckedBinding.PropertyName);
                else
                    return (bool)IsCheckedBinding.Source.GetType().GetProperty(IsCheckedBinding.PropertyName).GetValue(IsCheckedBinding.Source);
            }
            set
            {
                if (IsCheckedBinding.Source is ISettings)
                    (IsCheckedBinding.Source as ISettings).Set(IsCheckedBinding.PropertyName, value);
                else
                    IsCheckedBinding.Source.GetType().GetProperty(IsCheckedBinding.PropertyName).SetValue(IsCheckedBinding.Source, value);

                NotifyPropertyChanged(nameof(IsChecked));
            }
        }
    }
}
