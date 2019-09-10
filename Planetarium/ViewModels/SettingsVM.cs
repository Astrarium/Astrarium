using Planetarium.Config;
using Planetarium.Config.ControlBuilders;
using Planetarium.Themes;
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

namespace Planetarium.ViewModels
{
    public class SettingsVM : ViewModelBase
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

        public SettingsVM(ISettings settings, ISettingsConfig settingConfig, IViewManager viewManager)
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

                foreach (var item in section)
                {
                    SettingControlBuilder builder = item.Builder ?? settingConfig.GetBuilder(item.Type);                    
                    if (builder == null)
                    {
                        throw new Exception($"There are no control builder defined for setting with type {item.Type.Name}");
                    }

                    FrameworkElement control = builder.Build(settings, item, viewManager);

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

        public override void Close()
        {
            if (settings.IsChanged)
            {
                var result = viewManager.ShowMessageBox("Warning", "You have unsaved changes in program options. Do you want to apply them?", MessageBoxButton.YesNoCancel);
                if (MessageBoxResult.Yes == result)
                {
                    settings.Save();                    
                }
                else if (MessageBoxResult.No == result)
                {
                    settings.Load();
                }
                else if (MessageBoxResult.Cancel == result)
                {
                    return;
                }
            }
            base.Close();
        }

        private void Save()
        {
            settings.Save();
            Close();
        }

        private void Reset()
        {
            if (MessageBoxResult.Yes == viewManager.ShowMessageBox("Warning", "Do you really want to reset settings to default values?", MessageBoxButton.YesNo))
            {
                settings.Reset();
            }
        }

        public class SettingsSectionVM
        {
            public string Title { get; set; }
            public StackPanel Panel { get; set; }
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
