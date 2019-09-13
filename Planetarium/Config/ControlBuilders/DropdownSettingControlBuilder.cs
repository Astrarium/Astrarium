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
    public class DropdownSettingControlBuilder : SettingControlBuilder
    {
        public override FrameworkElement Build(ISettings settings, SettingItem item, IViewManager viewManager)
        {
            var panel = new StackPanel() { Orientation = Orientation.Vertical };
            var comboBox = new ComboBox() { IsReadOnly = true };
            comboBox.ItemsSource = Enum.GetValues(item.DefaultValue.GetType());
            BindingOperations.SetBinding(comboBox, ComboBox.SelectedItemProperty, new Binding(item.Name) { Source = settings });
            panel.Children.Add(new Label() { Content = item.Name });
            panel.Children.Add(comboBox);

            return panel;
        }
    }
}
