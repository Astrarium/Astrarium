using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Astrarium.Plugins.Planner.ViewModels
{
    public class PlanningListVM : ViewModelBase
    {
        private readonly ISky sky;
        private readonly ISkyMap map;
        private readonly ObservationPlanner planner;

        public ICommand SetTimeCommand { get; private set; }
        public ICommand ShowObjectCommand { get; private set; }

        public ICollection<Ephemerides> Ephemerides
        {
            get => GetValue<ICollection<Ephemerides>>(nameof(Ephemerides));
            private set => SetValue(nameof(Ephemerides), value);
        }

        public Ephemerides SelectedTableItem
        {
            get => GetValue<Ephemerides>(nameof(SelectedTableItem));
            set => SetValue(nameof(SelectedTableItem), value);
        }

        public PlanningListVM(ISky sky, ISkyMap map, ObservationPlanner planner)
        {
            this.planner = planner;
            this.sky = sky;
            this.map = map;

            SetTimeCommand = new Command<Date>(SetTime);
            ShowObjectCommand = new Command<CelestialObject>(ShowObject);
        }

        public async void CreatePlan(PlanningFilter filter)
        {
            var tokenSource = new CancellationTokenSource();
            var progress = new Progress<double>();

            ViewManager.ShowProgress("Please wait", "Creating observation plan...", tokenSource, progress);

            ICollection<Ephemerides> ephemerides = await Task.Run(() => planner.CreatePlan(filter, tokenSource.Token, progress));

            if (!tokenSource.IsCancellationRequested)
            {
                if (ephemerides.Any())
                {
                    Ephemerides = ephemerides;
                }
                tokenSource.Cancel();
            }
        }

        private void SetTime(Date time)
        {
            sky.SetDate(time.ToJulianEphemerisDay());
        }

        private void ShowObject(CelestialObject body)
        {
            map.GoToObject(body, TimeSpan.FromSeconds(1));
        }
    }
}
