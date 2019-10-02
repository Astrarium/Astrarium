using Planetarium.Config;
using Planetarium.Types.Themes;
using Planetarium.Types;
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
using Planetarium.Types.Config.Controls;
using Planetarium.Types.Controls;
using Planetarium.Types.Localization;

namespace Planetarium.ViewModels
{
    internal class SettingsVM : ViewModelBase
    {
        public ICommand CloseCommand { get; private set; }
        public ICommand ResetCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }

        public ObservableCollection<SettingsSectionVM> SettingsSections { get; private set; }

        private readonly ISettings settings;
        private readonly IViewManager viewManager;

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

        public SettingsVM(ISettings settings, SettingsConfig settingConfig, IViewManager viewManager)
        {
            this.settings = settings;
            this.viewManager = viewManager;

            CloseCommand = new Command(Close);
            ResetCommand = new Command(Reset);
            SaveCommand = new Command(Save);

            SettingsSections = new ObservableCollection<SettingsSectionVM>();

            var sections = settingConfig.Where(c => !string.IsNullOrEmpty(c.Section)).GroupBy(c => c.Section);

            foreach (var section in sections)
            {
                StackPanel panel = new StackPanel();
                panel.Orientation = Orientation.Vertical;

                foreach (SettingItem item in section)
                {
                    Type controlType = item.Section != null ? 
                        (item.ControlType ?? GetDefaultControlType(item.DefaultValue.GetType())) : 
                        null;

                    if (controlType != null)
                    {
                        FrameworkElement control = viewManager.CreateControl(controlType);
                        control.DataContext = new SettingVM(settings, item.Name, item.EnabledCondition);
                        control.Margin = new Thickness(0, 0, 0, 4);
                        panel.Children.Add(control);
                    }
                }
               
                SettingsSections.Add(new SettingsSectionVM() { Title = section.Key, Panel = panel });
            }

            SelectedSection = SettingsSections.First();
        }

        public override void Close()
        {
            if (settings.IsChanged)
            {
                var result = viewManager.ShowMessageBox("Warning", "You have unsaved changes in program options. Do you want to apply them?", MessageBoxButton.YesNoCancel);
                if (MessageBoxResult.Yes == result)
                {
                    base.Close();
                    settings.Save();
                }
                else if (MessageBoxResult.No == result)
                {
                    base.Close();
                    settings.Load();
                }
            }
            else
            {
                base.Close();
            }
        }

        private void Save()
        {
            settings.Save();
            base.Close();
        }

        private void Reset()
        {
            if (MessageBoxResult.Yes == viewManager.ShowMessageBox("Warning", "Do you really want to reset settings to default values?", MessageBoxButton.YesNo))
            {
                settings.Reset();
            }
        }

        private Type GetDefaultControlType(Type settingType)
        {
            if (settingType == typeof(bool))
                return typeof(CheckboxSettingControl);

            if (settingType.IsEnum)
                return typeof(EnumSettingControl);

            if (settingType == typeof(Font))
                return typeof(FontPickerSettingControl);

            if (settingType == typeof(Color))
                return typeof(ColorPickerSettingControl);

            return null;
        }

        internal class SettingsSectionVM
        {
            public string Title { get; set; }
            public StackPanel Panel { get; set; }
        }

        internal class SettingVM : ViewModelBase
        {
            /// <summary>
            /// ISettings object to bind to 
            /// </summary>
            private ISettings settings;

            /// <summary>
            /// Name of the setting
            /// </summary>
            public string SettingName { get; set; }

            /// <summary>
            /// Title of the setting (i.e. displayable name)
            /// </summary>
            public string SettingTitle
            {
                get
                {
                    return Text.Get($"Settings.{SettingName}");
                }
            }

            /// <summary>
            /// Setting value
            /// </summary>
            public object SettingValue
            {
                get
                {
                    return settings.Get<object>(SettingName);
                }
                set
                {
                    settings.Set(SettingName, value);
                }
            }

            private Func<ISettings, bool> isEnabledCondition;
            internal Func<ISettings, bool> IsEnabledCondition
            {
                get { return isEnabledCondition; }
                set
                {
                    isEnabledCondition = value;
                    Settings_SettingValueChanged(null, null);
                }
            }

            public bool IsEnabled { get; private set; } = true;

            public SettingVM(ISettings settings, string name, Func<ISettings, bool> isEnabledCondition)
            {
                this.settings = settings;
                this.settings.SettingValueChanged += Settings_SettingValueChanged;
                SettingName = name;
                IsEnabledCondition = isEnabledCondition;
            }

            private void Settings_SettingValueChanged(string propertyName, object propertyValue)
            {
                if (isEnabledCondition != null)
                {
                    IsEnabled = isEnabledCondition.Invoke(settings);
                    NotifyPropertyChanged(nameof(IsEnabled));
                }
            }
        }

        private class FuncBinder : INotifyPropertyChanged
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
}
