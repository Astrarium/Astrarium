using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Astrarium.Plugins.MinorBodies.ViewModels
{
    public class CometsSettingsVM : SettingsViewModel
    {
        private CometsCalc Calculator;
        public ICommand UpdateElementsCommand { get; private set; }

        public CometsSettingsVM(ISettings settings, CometsCalc calculator) : base(settings)
        {
            Calculator = calculator;
            UpdateElementsCommand = new Command(UpdateElements);
            settings.SettingValueChanged += Settings_SettingValueChanged;
        }

        private void Settings_SettingValueChanged(string name, object value)
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
                return timestamp < new DateTime(2000, 1, 1) ? "unknown" : Formatters.DateTime.Format(timestamp);
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
