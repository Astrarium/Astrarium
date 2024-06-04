using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Astrarium.Plugins.Satellites.ViewModels
{
    public class SatellitesSettingsVM : SettingsViewModel
    {
        private ISettings settings;

        public ICommand DeleteCommand { get; private set; }

        public ObservableCollection<TLESource> TLESources { get; private set; }

        public SatellitesSettingsVM(ISettings settings) : base(settings)
        {
            this.settings = settings;

            TLESources = new ObservableCollection<TLESource>(settings.Get<List<TLESource>>("SatellitesOrbitalElements"));

            DeleteCommand = new Command<object>(Delete);
        }

        private void Delete(object parameter)
        {
            if (parameter is TLESource tleSource)
            {
                if (ViewManager.ShowMessageBox("$Warning", "Do you really want to delete TLE source?", System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes)
                {
                    TLESources.Remove(tleSource);
                }
            }
        }
    }
}
