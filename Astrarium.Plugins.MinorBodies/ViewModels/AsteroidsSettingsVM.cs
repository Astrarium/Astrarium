using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Astrarium.Plugins.MinorBodies.ViewModels
{
    public class AsteroidsSettingsVM : SettingsViewModel
    {
        private AsteroidsCalc Calculator;
        public ICommand UpdateElementsCommand { get; private set; }

        public AsteroidsSettingsVM(ISettings settings, AsteroidsCalc calculator) : base(settings)
        {
            Calculator = calculator;
            UpdateElementsCommand = new Command(UpdateElements);
            settings.SettingValueChanged += Settings_SettingValueChanged;
        }

        private void Settings_SettingValueChanged(string name, object value)
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
            Calculator.UpdateOrbitalElements(silent: false);
            IsUpdating = false;
        }
    }
}
