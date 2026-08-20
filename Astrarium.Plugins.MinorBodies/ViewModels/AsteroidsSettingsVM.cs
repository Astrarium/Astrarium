using Astrarium.Types;
using System;
using System.Runtime;
using System.Windows.Input;

namespace Astrarium.Plugins.MinorBodies.ViewModels
{
    public class AsteroidsSettingsVM : SettingsViewModel
    {
        private readonly ISettings settings;
        private readonly AsteroidsCalc calculator;

        public ICommand UpdateElementsCommand { get; private set; }

        public AsteroidsSettingsVM(ISettings settings, AsteroidsCalc calculator) : base(settings)
        {
            this.settings = settings;
            this.calculator = calculator;
            UpdateElementsCommand = new Command(UpdateElements);
            settings.SettingValueChanged += Settings_SettingValueChanged;
        }

        private void Settings_SettingValueChanged(string name, object value, object oldValue)
        {
            if (name == "AsteroidsDownloadOrbitalElementsTimestamp")
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
                var timestamp = Settings.Get<DateTime>("AsteroidsDownloadOrbitalElementsTimestamp");
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
