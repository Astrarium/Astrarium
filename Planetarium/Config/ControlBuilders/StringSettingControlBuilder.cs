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
    public class StringSettingControlBuilder : SettingControlBuilder
    {
        public override FrameworkElement Build(ISettings settings, SettingConfigItem item, IViewManager viewManager)
        {
            var container = new StackPanel() { Orientation = Orientation.Vertical, HorizontalAlignment = HorizontalAlignment.Stretch };
            container.Children.Add(new Label() { Content = item.Name });
            var textbox = new TextBox() { HorizontalAlignment = HorizontalAlignment.Stretch, HorizontalContentAlignment = HorizontalAlignment.Stretch };
            BindingOperations.SetBinding(textbox, TextBox.TextProperty, new Binding(item.Name) { Source = settings });
            container.Children.Add(textbox);
            return container;
        }
    }
}
