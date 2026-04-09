using Astrarium.Types;
using System;
using System.Windows.Input;

namespace Astrarium.Plugins.MinorBodies.ViewModels
{
    public class CometsSettingsVM : SettingsViewModel
    {
        private readonly CometsCalc calculator;
        private readonly ISettings settings;

        public ICommand UpdateElementsCommand { get; private set; }

        public CometsSettingsVM(ISettings settings, CometsCalc calculator) : base(settings)
        {
            this.settings = settings;
            this.calculator = calculator;
            UpdateElementsCommand = new Command(UpdateElements);
            settings.SettingValueChanged += Settings_SettingValueChanged;
        }

        private void Settings_SettingValueChanged(string name, object value, object oldValue)
        {
            if (name == "CometsDownloadOrbitalElementsTimestamp")
            {
                NotifyPropertyChanged(nameof(LastUpdated));
            }
        }

        public bool IsUpdating
        {
            get => GetValue<bool>(nameof(IsUpdating));
            set => SetValue(nameof(IsUpdating), value);
        }

        public string LastUpdated
        {
            get
            {
                var timestamp = Settings.Get<DateTime>("CometsDownloadOrbitalElementsTimestamp");
                return timestamp < new DateTime(2000, 1, 1) ? Text.Get("OrbitalElements.LastUpdatedUnknown") : Formatters.DateTime.Format(timestamp);
            }
        }

        private void UpdateElements()
        {
            IsUpdating = true;
            calculator.UpdateOrbitalElements(silent: false);
            IsUpdating = false;
        }

        public override void Dispose()
        {
            settings.SettingValueChanged -= Settings_SettingValueChanged;
            base.Dispose();
        }
    }
}
