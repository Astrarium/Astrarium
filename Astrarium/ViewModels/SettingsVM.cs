using Astrarium.Types;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Astrarium.Types.Controls;

namespace Astrarium.ViewModels
{
    internal class SettingsVM : ViewModelBase
    {
        private readonly ISettings settings;

        public ICommand CloseCommand { get; private set; }
        public ICommand ResetCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }

        public ObservableCollection<SettingsSection> SettingsSections { get; private set; }

        public SettingsSection SelectedSection
        {
            get => GetValue<SettingsSection>(nameof(SelectedSection));
            set => SetValue(nameof(SelectedSection), value);
        }

        public SettingsVM(ISettings settings, UIElementsIntegration uiIntegration)
        {
            this.settings = settings;

            CloseCommand = new Command(Close);
            ResetCommand = new Command(Reset);
            SaveCommand = new Command(Save);

            SettingsSections = new ObservableCollection<SettingsSection>();

            foreach (var section in uiIntegration.SettingSections)
            {
                var model = ViewManager.CreateViewModel(section.ViewModelType);
                var control = Activator.CreateInstance(section.ViewType) as SettingsSection;
                control.SetValue(FrameworkElement.DataContextProperty, model);
                SettingsSections.Add(control);
            }

            SelectedSection = SettingsSections.FirstOrDefault();
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
    }
}