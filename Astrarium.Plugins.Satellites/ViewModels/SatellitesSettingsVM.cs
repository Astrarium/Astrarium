using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Astrarium.Plugins.Satellites.ViewModels
{
    public class SatellitesSettingsVM : SettingsViewModel
    {
        private readonly ISatellitesCalculator calc;
        private readonly ISkyMap map;
        private readonly IOrbitalElementsUpdater updater;
        private readonly ISettings settings;

        public ICommand EditSourceCommand { get; private set; }
        public ICommand UpdateSourceCommand { get; private set; }
        public ICommand DeleteSourceCommand { get; private set; }
        public ICommand AddSourceCommand { get; private set; }
        public ICommand EditSelectedSourceCommand { get; private set; }

        public ObservableCollection<TLESource> Sources { get; private set; }

        public TLESource SelectedSource { get; set; }

        public SatellitesSettingsVM(ISatellitesCalculator calc, ISkyMap map, IOrbitalElementsUpdater updater, ISettings settings) : base(settings)
        {
            this.calc = calc;
            this.map = map;
            this.updater = updater;
            this.settings = settings;

            var sources = settings.Get<List<TLESource>>("SatellitesOrbitalElements");
            Sources = new ObservableCollection<TLESource>(sources);
            foreach (var source in sources)
            {
                source.PropertyChanged += Source_PropertyChanged;
            }

            DeleteSourceCommand = new Command<TLESource>(DeleteSource);
            EditSourceCommand = new Command<TLESource>(EditSource);
            UpdateSourceCommand = new Command<TLESource>(UpdateSource);
            AddSourceCommand = new Command(() => EditSource(new TLESource()));
            EditSelectedSourceCommand = new Command(() => EditSource(SelectedSource));
        }

        public override void Dispose()
        {
            foreach (var source in Sources)
            {
                source.PropertyChanged -= Source_PropertyChanged;
            }
        }

        private void Source_PropertyChanged(object sender, EventArgs args)
        {
            calc.Calculate();
            map.Invalidate();
        }

        private void DeleteSource(TLESource tleSource)
        {
            if (ViewManager.ShowMessageBox("$Warning", "$Settings.SatellitesOrbitalElementsSources.ButtonDelete.Confirm", System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes)
            {
                tleSource.PropertyChanged -= Source_PropertyChanged;
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
                if (index >= 0)
                {
                    var source = vm.GetTleSource();
                    source.PropertyChanged += Source_PropertyChanged;
                    tleSource.PropertyChanged -= Source_PropertyChanged;
                    Sources.Insert(index, source);
                    Sources.Remove(tleSource);
                }
                else
                {
                    var source = vm.GetTleSource();
                    source.PropertyChanged += Source_PropertyChanged;
                    Sources.Add(source);
                }
                settings.Set("SatellitesOrbitalElements", Sources.ToList());
            }
        }

        private async void UpdateSource(TLESource tleSource)
        {
            await updater.UpdateOrbitalElements(tleSource, silent: false);
        }
    }
}
