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
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(2, GridUnitType.Auto) });

            var labelEpoch = new Label() { Content = "Epoch:" };
            var datePicker = viewManager.CreateControl<Controls.DatePicker>();
            datePicker.Options = ViewModels.DateOptions.MonthYear;
            BindingOperations.SetBinding(datePicker, Controls.DatePicker.JulianDayProperty, new Binding(item.Name + "." + "Epoch")
            {
                Source = settings
            });
            
            var labelLongitude = new Label() { Content = "Longitude at Epoch:" };
            labelLongitude.SetValue(Grid.ColumnProperty, 0);
            labelLongitude.SetValue(Grid.RowProperty, 1);

            labelEpoch.SetValue(Grid.ColumnProperty, 0);
            labelEpoch.SetValue(Grid.RowProperty, 0);
            datePicker.SetValue(Grid.ColumnProperty, 1);
            datePicker.SetValue(Grid.RowProperty, 0);

            var updLongitude = new NumericUpDown() { Minimum = 0, Maximum = 359, DecimalPlaces = 0 };
            BindingOperations.SetBinding(updLongitude, NumericUpDown.ValueProperty, new Binding(item.Name + "." + "Longitude")
            {
                Source = settings
            });

            updLongitude.SetValue(Grid.ColumnProperty, 1);
            updLongitude.SetValue(Grid.RowProperty, 1);


            var labelDrift = new Label() { Content = "Monthly Drift:" };
            labelDrift.SetValue(Grid.ColumnProperty, 0);
            labelDrift.SetValue(Grid.RowProperty, 2);



            var updDrift= new NumericUpDown() { Minimum = 0, Maximum = 359, Step = 0.1m, DecimalPlaces = 2 };
            BindingOperations.SetBinding(updDrift, NumericUpDown.ValueProperty, new Binding(item.Name + "." + "MonthlyDrift")
            {
                Source = settings
            });

            updDrift.SetValue(Grid.ColumnProperty, 1);
            updDrift.SetValue(Grid.RowProperty, 2);


            grid.Children.Add(labelEpoch);
            grid.Children.Add(datePicker);
            grid.Children.Add(labelLongitude);
            grid.Children.Add(updLongitude);
            grid.Children.Add(labelDrift);
            grid.Children.Add(updDrift);

            groupBox.Content = grid;
            return groupBox;
        }
    }
}
