using ADK.Demo;
using Planetarium.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Planetarium.ViewModels
{
    public class SettingsVM : ViewModelBase
    {
        public ICommand CloseCommand { get; private set; }
        public ObservableCollection<SettingsSectionVM> SettingsSections { get; private set; }

        private SettingsSectionVM selectedSection;
        public SettingsSectionVM SelectedSection
        {
            get
            {
                return selectedSection;
            }
            set
            {
                selectedSection = value;
                SectionContent = selectedSection.Panel;
                NotifyPropertyChanged(nameof(SectionContent));
            }
        }
        public UIElement SectionContent { get; private set; } 

        public SettingsVM(ISettings settings, ISettingsConfig settingConfig)
        {
            CloseCommand = new Command(Close);

            SettingsSections = new ObservableCollection<SettingsSectionVM>();

            var sections = settingConfig.GroupBy(c => c.Section);

            foreach (var section in sections)
            {
                StackPanel panel = new StackPanel();
                panel.Orientation = Orientation.Vertical;

                foreach (var item in section)
                {
                    FrameworkElement control = null;

                    if (item.Type == typeof(bool))
                    {
                        control = new CheckBox() { Content = item.Name };
                        BindingOperations.SetBinding(control, CheckBox.IsCheckedProperty, new Binding(item.Name) { Source = settings });
                    }
                    else if (item.Type.IsEnum)
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
                        control = radioGroup;
                    }
                    else if (item.Type == typeof(Color))
                    {
                        var picker = new ColorPicker() { Caption = item.Name };
                        BindingOperations.SetBinding(picker, ColorPicker.SelectedColorProperty, new Binding(item.Name) { Source = settings });
                        control = picker;
                    }



                    if (control != null && item.EnabledWhenCondition != null)
                    {
                        var binding = new Binding(nameof(FuncBinder.Value));
                        binding.Source = new FuncBinder(settings, item.EnabledWhenCondition);
                        BindingOperations.SetBinding(control, UIElement.IsEnabledProperty, binding);
                    }

                    if (control != null && item.VisibleWhenCondition != null)
                    {
                        var binding = new Binding(nameof(FuncBinder.Value));
                        binding.Source = new FuncBinder(settings, item.VisibleWhenCondition);
                        binding.Converter = new BooleanToVisibilityConverter();
                        BindingOperations.SetBinding(control, UIElement.VisibilityProperty, binding);
                    }

                    if (control != null)
                    {
                        control.Margin = new Thickness(0, 0, 0, 4);
                        panel.Children.Add(control);
                    }
                }

                SettingsSections.Add(new SettingsSectionVM() { Title = section.Key, Panel = panel });
            }
            
            SelectedSection = SettingsSections.First();
        }

        public class SettingsSectionVM
        {
            public string Title { get; set; }
            public StackPanel Panel { get; set; }
        }

        public class RadioButtonCheckedConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return value.Equals(parameter);
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return value.Equals(true) ? parameter : Binding.DoNothing;
            }
        }

        

        public class FuncBinder : INotifyPropertyChanged
        {
            private Func<ISettings, bool> func;
            private ISettings settings;

            public event PropertyChangedEventHandler PropertyChanged;

            public FuncBinder(ISettings settings, Func<ISettings, bool> func)
            {
                this.settings = settings;
                this.func = func;
                this.settings.SettingValueChanged += Settings_SettingValueChanged;
            }

            private void Settings_SettingValueChanged(string arg1, object arg2)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
            }

            public bool Value { get { return func(settings); } }
        }
    }

    public interface ISettingsConfig : IEnumerable<SettingConfigItem>
    {

    }

    public class SettingsConfig : ISettingsConfig
    {
        private readonly List<SettingConfigItem> Items = new List<SettingConfigItem>();

        public SettingConfigItem Add<T>(string section, string name, T defaultValue = default(T))
        {
            var item = new SettingConfigItem(section, name, typeof(T), defaultValue);
            Items.Add(item);
            return item;
        }

        public IEnumerator<SettingConfigItem> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }
    }

    public class SettingConfigItem
    {
        public string Name { get; private set; }
        public string Section { get; private set; }
        public Type Type { get; private set; }
        public object DefaultValue { get; private set; }
        public Func<ISettings, bool> EnabledWhenCondition { get; private set; }
        public Func<ISettings, bool> VisibleWhenCondition { get; private set; }

        public SettingConfigItem(string section, string name, Type type, object defaultValue)
        {
            Section = section;
            Name = name;
            Type = type;
            DefaultValue = defaultValue;
        }

        public SettingConfigItem EnabledWhen(Func<ISettings, bool> condition)
        {
            EnabledWhenCondition = condition;
            return this;
        }

        public SettingConfigItem EnabledWhenTrue(string settingName)
        {
            EnabledWhenCondition = (s) => s.Get<bool>(settingName);
            return this;
        }

        public SettingConfigItem EnabledWhenFalse(string settingName)
        {
            EnabledWhenCondition = (s) => !s.Get<bool>(settingName);
            return this;
        }

        public SettingConfigItem VisibleWhen(Func<ISettings, bool> condition)
        {
            VisibleWhenCondition = condition;
            return this;
        }

        public SettingConfigItem VisibleWhenTrue(string settingName)
        {
            VisibleWhenCondition = (s) => s.Get<bool>(settingName);
            return this;
        }

        public SettingConfigItem VisibleWhenFalse(string settingName)
        {
            VisibleWhenCondition = (s) => !s.Get<bool>(settingName);
            return this;
        }
    }
}
