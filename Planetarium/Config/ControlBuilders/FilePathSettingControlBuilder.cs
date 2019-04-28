using Planetarium.Controls;
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
    public class FilePathSettingControlBuilder : SettingControlBuilder
    {
        public override FrameworkElement Build(ISettings settings, SettingConfigItem item, IViewManager viewManager)
        {
            var container = new StackPanel() { Orientation = Orientation.Vertical };
            container.Children.Add(new Label() { Content = item.Name });
            var picker = new FilePathPicker() { Caption = item.Name };
            BindingOperations.SetBinding(picker, FilePathPicker.SelectedPathProperty, new Binding(item.Name) { Source = settings });
            container.Children.Add(picker);
            return container;
        }
    }
}
