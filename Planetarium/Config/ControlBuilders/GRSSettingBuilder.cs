using ADK;
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
    public class GRSSettingBuilder : SettingControlBuilder
    {
        public override FrameworkElement Build(ISettings settings, SettingConfigItem item)
        {
            var groupBox = new GroupBox() { Header = item.Name };
            var grid = new Grid();

            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0, GridUnitType.Auto) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0, GridUnitType.Star) });

            var datePicker = new Controls.DatePicker();

            BindingOperations.SetBinding(datePicker, Controls.DatePicker.JulianDayProperty, new Binding(item.Name + "." + "Epoch")
            {
                Source = settings
            });

            datePicker.SetValue(Grid.ColumnProperty, 1);

            grid.Children.Add(datePicker);

            groupBox.Content = grid;
            return groupBox;
        }
    }
}
