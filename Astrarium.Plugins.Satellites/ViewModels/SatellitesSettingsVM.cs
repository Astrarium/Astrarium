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
        private readonly IOrbitalElementsUpdater updater;
        private readonly ISettings settings;

        public ICommand EditSourceCommand { get; private set; }
        public ICommand UpdateSourceCommand { get; private set; }
        public ICommand DeleteSourceCommand { get; private set; }
        public ICommand AddSourceCommand { get; private set; }
        public ICommand EditSelectedSourceCommand { get; private set; }

        public ObservableCollection<TLESource> Sources { get; private set; }

        public TLESource SelectedSource { get; set; }

        public SatellitesSettingsVM(IOrbitalElementsUpdater updater, ISettings settings) : base(settings)
        {
            this.updater = updater;
            this.settings = settings;

            Sources = new ObservableCollection<TLESource>(settings.Get<List<TLESource>>("SatellitesOrbitalElements"));

            DeleteSourceCommand = new Command<TLESource>(DeleteSource);
            EditSourceCommand = new Command<TLESource>(EditSource);
            UpdateSourceCommand = new Command<TLESource>(UpdateSource);
            AddSourceCommand = new Command(() => EditSource(new TLESource()));
            EditSelectedSourceCommand = new Command(() => EditSource(SelectedSource));
        }

        private void DeleteSource(TLESource tleSource)
        {
            if (ViewManager.ShowMessageBox("$Warning", "$Settings.SatellitesOrbitalElementsSources.ButtonDelete.Confirm", System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes)
            {
                Sources.Remove(tleSource);
            }
        }

        private void EditSource(TLESource tleSource)
        {
            var vm = ViewManager.CreateViewModel<EditTleSourceVM>();
            vm.SetTleSource(Sources, tleSource);
            if (ViewManager.ShowDialog(vm) == true)
            {
                int index = Sources.IndexOf(tleSource);
                Sources.Insert(index, vm.GetTleSource());
                Sources.Remove(tleSource);

                settings.Set("SatellitesOrbitalElements", Sources.ToList());
            }
        }

        private async void UpdateSource(TLESource tleSource)
        {
            await updater.UpdateOrbitalElements(tleSource, silent: false);
        }
    }
}
