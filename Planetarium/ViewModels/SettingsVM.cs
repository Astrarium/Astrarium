using ADK.Demo;
using Planetarium.Themes;
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
using static Planetarium.ViewModels.SettingsVM;

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
                    FrameworkElement control = item.GetBuilder().Build(settings, item);

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

    public abstract class SettingControlBuilder
    {
        public abstract FrameworkElement Build(ISettings settings, SettingConfigItem item);
    }

    public class BooleanSettingControlBuilder : SettingControlBuilder
    {
        public override FrameworkElement Build(ISettings settings, SettingConfigItem item)
        {
            var control = new CheckBox() { Content = item.Name };
            BindingOperations.SetBinding(control, CheckBox.IsCheckedProperty, new Binding(item.Name) { Source = settings });
            return control;
        }
    }

    public class StringSettingControlBuilder : SettingControlBuilder
    {
        public override FrameworkElement Build(ISettings settings, SettingConfigItem item)
        {
            var container = new StackPanel() { Orientation = Orientation.Vertical };
            container.Children.Add(new Label() { Content = item.Name });
            var textbox = new TextBox();
            BindingOperations.SetBinding(textbox, TextBox.TextProperty, new Binding(item.Name) { Source = settings });
            container.Children.Add(textbox);
            return container;
        }
    }

    public class ColorSettingControlBuilder : SettingControlBuilder
    {
        public override FrameworkElement Build(ISettings settings, SettingConfigItem item)
        {
            var picker = new ColorPicker() { Caption = item.Name };
            BindingOperations.SetBinding(picker, ColorPicker.SelectedColorProperty, new Binding(item.Name) { Source = settings });
            return picker;
        }
    }

    public class FilePathSettingControlBuilder : SettingControlBuilder
    {
        public override FrameworkElement Build(ISettings settings, SettingConfigItem item)
        {
            var container = new StackPanel() { Orientation = Orientation.Vertical };
            container.Children.Add(new Label() { Content = item.Name });
            var picker = new FilePathPicker() { Caption = item.Name };
            BindingOperations.SetBinding(picker, FilePathPicker.SelectedPathProperty, new Binding(item.Name) { Source = settings });
            container.Children.Add(picker);
            return container;
        }
    }

    public class EnumSettingControlBuilder : SettingControlBuilder
    {
        public override FrameworkElement Build(ISettings settings, SettingConfigItem item)
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

    public class DropdownSettingControlBuilder : SettingControlBuilder
    {
        public override FrameworkElement Build(ISettings settings, SettingConfigItem item)
        {
            var panel = new StackPanel() { Orientation = Orientation.Vertical };
            var comboBox = new ComboBox() { IsReadOnly = true };
            comboBox.ItemsSource = Enum.GetValues(item.Type);
            BindingOperations.SetBinding(comboBox, ComboBox.SelectedItemProperty, new Binding(item.Name) { Source = settings });
            panel.Children.Add(new Label() { Content = item.Name });
            panel.Children.Add(comboBox);

            return panel;
        }
    }

    public class FontSettingControlBuilder : SettingControlBuilder
    {
        public override FrameworkElement Build(ISettings settings, SettingConfigItem item)
        {
            var container = new StackPanel() { Orientation = Orientation.Vertical };
            container.Children.Add(new Label() { Content = item.Name });
            var picker = new FontPicker();
            BindingOperations.SetBinding(picker, FontPicker.SelectedFontProperty, new Binding(item.Name) { Source = settings });
            container.Children.Add(picker);
            return container;
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
        public SettingControlBuilder Builder { get; private set; }

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

        public SettingConfigItem WithBuilder(Type builderType)
        {
            if (!typeof(SettingControlBuilder).IsAssignableFrom(builderType))
                throw new ArgumentException($"Builder type should be derived from {nameof(SettingControlBuilder)} base type.");

            if (builderType.IsAbstract)
                throw new ArgumentException($"Builder type should not be an abstract class type.");

            if (builderType.GetConstructor(Type.EmptyTypes) == null)
                throw new ArgumentException($"Builder type should have public parameterless constructor.");

            Builder = (SettingControlBuilder)Activator.CreateInstance(builderType);

            return this;
        }

        public SettingControlBuilder GetBuilder()
        {
            if (Builder == null)
            {
                if (Type == typeof(bool))
                    Builder = new BooleanSettingControlBuilder();
                else if (Type == typeof(string))
                    Builder = new StringSettingControlBuilder();
                else if (Type.IsEnum)
                    Builder = new EnumSettingControlBuilder();
                else if (Type == typeof(Color))
                    Builder = new ColorSettingControlBuilder();
                else if (Type == typeof(Font))
                    Builder = new FontSettingControlBuilder();
                else
                    throw new Exception($"There are no control builder defined for setting with type {Type.Name}");
            }

            return Builder;
        }
    }
}
