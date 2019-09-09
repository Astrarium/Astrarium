using Planetarium.Controls;
using Planetarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Planetarium.Config.ControlBuilders
{
    public class ColorSettingControlBuilder : SettingControlBuilder
    {
        public override FrameworkElement Build(ISettings settings, SettingConfigItem item, IViewManager viewManager)
        {
            var picker = new ColorPicker() { Caption = item.Name };
            BindingOperations.SetBinding(picker, ColorPicker.SelectedColorProperty, new Binding(item.Name) { Source = settings });
            return picker;
        }
    }
}
