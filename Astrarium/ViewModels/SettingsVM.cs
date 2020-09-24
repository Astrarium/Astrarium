using Astrarium.Types;
using Astrarium.Config.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections;
using System.ComponentModel;

namespace Astrarium.ViewModels
{
    internal class SettingsVM : ViewModelBase
    {
        public ICommand CloseCommand { get; private set; }
        public ICommand ResetCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }

        public ObservableCollection<SettingsSectionVM> SettingsSections { get; private set; }

        public string WindowTitle => Text.Get("SettingsWindow.Title");
        public string ResetToDefaultsText => Text.Get("SettingsWindow.ResetToDefaults");
        public string SaveText => Text.Get("Save");
        public string CloseText => Text.Get("Close");

        private readonly ISettings settings;

        private SettingsSectionVM selectedSection;
        public SettingsSectionVM SelectedSection
        {
            get
            {
                return selectedSection;
            }
            set
            {                
                if (value != null)
                {
                    selectedSection = value;
                    NotifyPropertyChanged(nameof(SelectedSection));
                }
            }
        }

        public ConfigControlTemplateSelector ControlSelector { get; private set; }

        public SettingsVM(ISettings settings, UIElementsIntegration uiIntegration)
        {
            this.settings = settings;

            CloseCommand = new Command(Close);
            ResetCommand = new Command(Reset);
            SaveCommand = new Command(Save);

            SettingsSections = new ObservableCollection<SettingsSectionVM>();
            ControlSelector = new ConfigControlTemplateSelector();

            foreach (var section in uiIntegration.SettingItems.Groups.Where(g => g != null))
            {
                var sectionVM = new SettingsSectionVM(section);

                foreach (SettingItem item in uiIntegration.SettingItems[section])
                {
                    Type controlType = item.ControlType ?? GetDefaultControlType(item.DefaultValue.GetType()); 

                    if (controlType != null)
                    {
                        var vm = new SettingVM(settings, item.Name, item.EnabledCondition, controlType);

                        if (section == "Colors")
                        {
                            vm.AddBinding(new SimpleBinding(settings, "Schema", nameof(SettingVM.IsVisible))
                            {
                                SourceToTargetConverter = s => ((ColorSchema)s) == ColorSchema.Night,
                                TargetToSourceConverter = s => settings.Get<ColorSchema>("Schema")
                            });
                        }

                        sectionVM.Add(vm);
                    }
                }

                SettingsSections.Add(sectionVM);
            }

            SelectedSection = SettingsSections.FirstOrDefault();

            Text.LocaleChanged += () => NotifyPropertyChanged(
                nameof(WindowTitle), 
                nameof(SaveText), 
                nameof(CloseText), 
                nameof(ResetToDefaultsText)
            );

            this.settings.Save("Current");
        }

        public override void Close()
        {
            if (settings.IsChanged)
            {
                var result = ViewManager.ShowMessageBox("$SettingsWindow.WarningTitle", "$SettingsWindow.UnsavedValuesWarningText", MessageBoxButton.YesNoCancel);
                if (MessageBoxResult.Yes == result)
                {
                    base.Close();
                    settings.Save();
                }
                else if (MessageBoxResult.No == result)
                {
                    base.Close();
                    settings.Load("Current");
                }
            }
            else
            {
                base.Close();
            }
        }

        public override void Dispose()
        {
            // need to utilizate settings controls
            SettingsSections.Clear();
            base.Dispose();
        }

        private void Save()
        {
            settings.Save();
            base.Close();
        }

        private void Reset()
        {
            if (MessageBoxResult.Yes == ViewManager.ShowMessageBox("$SettingsWindow.WarningTitle", "$SettingsWindow.ResetToDefaultsWarningText", MessageBoxButton.YesNo))
            {
                settings.Load("Defaults");
                Save();
            }
        }

        private Type GetDefaultControlType(Type settingType)
        {
            if (settingType == typeof(bool))
                return typeof(CheckboxSettingControl);

            if (settingType == typeof(string))
                return typeof(TextboxSettingControl);

            if (settingType.IsEnum)
                return typeof(EnumSettingControl);

            if (settingType == typeof(Font))
                return typeof(FontPickerSettingControl);

            if (settingType == typeof(Color))
                return typeof(ColorPickerSettingControl);

            return null;
        }

        internal class SettingsSectionVM : List<SettingVM>, INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            private string section;
            public string Title => Text.Get($"Section.{section}");

            internal SettingsSectionVM(string section)
            {
                this.section = section;
                Text.LocaleChanged += () => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
            }
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

            public bool IsVisible
            {
                get => GetValue<bool>(nameof(IsVisible));
                set => SetValue(nameof(IsVisible), value);
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

            public Type ControlType { get; private set; }

            public SettingVM(ISettings settings, string name, Func<ISettings, bool> isEnabledCondition, Type controlType)
            {
                this.settings = settings;
                this.settings.SettingValueChanged += Settings_SettingValueChanged;
                SettingName = name;
                ControlType = controlType;
                IsEnabledCondition = isEnabledCondition;
                Text.LocaleChanged += () => NotifyPropertyChanged(nameof(SettingTitle));
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

        internal class ConfigControlTemplateSelector : DataTemplateSelector
        {
            public override DataTemplate SelectTemplate(object item, DependencyObject container)
            {
                SettingVM setting = (SettingVM)item;
                var controlFactory = new FrameworkElementFactory(setting.ControlType);
                controlFactory.SetValue(FrameworkElement.DataContextProperty, item);
                controlFactory.SetValue(FrameworkElement.MarginProperty, new Thickness(0, 0, 0, 4));
                return new DataTemplate() { VisualTree = controlFactory };
            }
        }
    }
}