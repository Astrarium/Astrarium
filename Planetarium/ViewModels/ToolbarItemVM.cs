using Planetarium.Config;
using Planetarium.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Planetarium.ViewModels
{
    public abstract class ToolbarItemVM : ViewModelBase
    {
        public Type Type => GetType();
    }

    public class ToolbarButtonVM : ToolbarItemVM
    {
        public string ButtonName { get; set; }
        public ICommand ButtonCommand { get; set; }
        public string ImageKey { get; set; }
    }

    public class ToolbarSeparatorVM : ToolbarItemVM
    {

    }

    public class ToolbarToggleButtonVM : ToolbarItemVM
    {
        public ToolbarToggleButtonVM(string buttonName, string imageKey, object bindingObject, string bindingProperty)
        {
            ButtonName = buttonName;
            ImageKey = imageKey;
            BindableObject = bindingObject;
            BindableProperty = bindingProperty;

            if (bindingObject is INotifyPropertyChanged)
            {
                var pc = bindingObject as INotifyPropertyChanged;
                pc.PropertyChanged += (o, e) =>
                {
                    if (e.PropertyName == bindingProperty)
                    {
                        NotifyPropertyChanged(nameof(IsChecked));
                    }
                };
            }
        }

        public string ButtonName { get; private set; }
        public string ImageKey { get; private set; }
        public object BindableObject { get; private set; }
        public string BindableProperty { get; private set; }

        public bool IsChecked
        {
            get
            {
                if (BindableObject is ISettings)
                    return (BindableObject as ISettings).Get<bool>(BindableProperty);
                else 
                    return (bool)BindableObject.GetType().GetProperty(BindableProperty).GetValue(BindableObject);
            }
            set
            {
                if (BindableObject is ISettings)
                    (BindableObject as ISettings).Set(BindableProperty, value);
                else
                    BindableObject.GetType().GetProperty(BindableProperty).SetValue(BindableObject, value);

                NotifyPropertyChanged(nameof(IsChecked));
            }
        }
    }
}
