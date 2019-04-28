using ADK;
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
    public class GRSSettingBuilder : SettingControlBuilder
    {
        public override FrameworkElement Build(ISettings settings, SettingConfigItem item, IViewManager viewManager)
        {
            var groupBox = new GroupBox() { Header = item.Name };
            var grid = new Grid();

            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0, GridUnitType.Auto) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0, GridUnitType.Star) });

            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });

            var labelEpoch = new Label() { Content = "Epoch:" };
            var datePicker = viewManager.CreateControl<Controls.DatePicker>();
            datePicker.Options = ViewModels.DateOptions.MonthYear;
            BindingOperations.SetBinding(datePicker, Controls.DatePicker.JulianDayProperty, new Binding(item.Name + "." + "Epoch")
            {
                Source = settings
            });
            
            var labelLongitude = new Label() { Content = "Longitude at Epoch:" };

            labelEpoch.SetValue(Grid.ColumnProperty, 0);
            labelEpoch.SetValue(Grid.RowProperty, 0);
            datePicker.SetValue(Grid.ColumnProperty, 1);
            datePicker.SetValue(Grid.RowProperty, 0);

            labelLongitude.SetValue(Grid.ColumnProperty, 0);
            labelLongitude.SetValue(Grid.RowProperty, 1);

            var updLongitude = new IntegerUpDown() { Minimum = 0, Maximum = 359 };
            BindingOperations.SetBinding(updLongitude, IntegerUpDown.ValueProperty, new Binding(item.Name + "." + "Longitude")
            {
                Source = settings
            });

            updLongitude.SetValue(Grid.ColumnProperty, 1);
            updLongitude.SetValue(Grid.RowProperty, 1);

            grid.Children.Add(labelEpoch);
            grid.Children.Add(datePicker);
            grid.Children.Add(labelLongitude);
            grid.Children.Add(updLongitude);

            groupBox.Content = grid;
            return groupBox;
        }
    }
}
