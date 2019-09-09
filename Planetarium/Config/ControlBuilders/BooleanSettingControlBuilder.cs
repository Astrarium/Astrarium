using Planetarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Planetarium.Config.ControlBuilders
{
    public class BooleanSettingControlBuilder : SettingControlBuilder
    {
        public override FrameworkElement Build(ISettings settings, SettingConfigItem item, IViewManager viewManager)
        {
            var control = new CheckBox() { Content = item.Name };
            BindingOperations.SetBinding(control, CheckBox.IsCheckedProperty, new Binding(item.Name) { Source = settings });
            return control;
        }
    }
}
