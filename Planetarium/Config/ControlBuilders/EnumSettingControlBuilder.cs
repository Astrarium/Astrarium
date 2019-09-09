using Planetarium.Themes;
using Planetarium.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Planetarium.Config.ControlBuilders
{
    public class EnumSettingControlBuilder : SettingControlBuilder
    {
        public override FrameworkElement Build(ISettings settings, SettingConfigItem item, IViewManager viewManager)
        {
            var radioGroup = new GroupBox() { Header = item.Name };
            var radioPanel = new StackPanel() { Orientation = Orientation.Vertical };

            Array values = Enum.GetValues(item.Type);
            foreach (var value in values)
            {
                RadioButton radio = new RadioButton()
                {
                    Content = value.GetType()
                        .GetMember(value.ToString())
                        .FirstOrDefault()
                        ?.GetCustomAttribute<DescriptionAttribute>()
                        ?.Description,
                    Margin = new Thickness(12, 0, 0, 4)
                };

                BindingOperations.SetBinding(radio, RadioButton.IsCheckedProperty, new Binding(item.Name)
                {
                    Source = settings,
                    Converter = new RadioButtonCheckedConverter(),
                    ConverterParameter = value
                });

                radioPanel.Children.Add(radio);
            }

            radioGroup.Content = radioPanel;
            return radioGroup;
        }
    }
}
