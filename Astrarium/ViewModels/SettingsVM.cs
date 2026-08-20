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

        public ObservableCollection<SettingsSection> Sections { get; private set; }

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

            Sections = new ObservableCollection<SettingsSection>();

            foreach (var section in uiIntegration.SettingSections)
            {
                var control = (SettingsSection)Activator.CreateInstance(section.ViewType);
                var vm = ViewManager.CreateViewModel(section.ViewModelType) as SettingsViewModel;
                if (vm == null)
                {
                    throw new Exception($"{section.ViewModelType.FullName} should inherit SettingsViewModel");
                }
                control.DataContext = vm;
                Sections.Add(control);
            }

            SelectedSection = Sections.FirstOrDefault();
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
            Sections.Clear();
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