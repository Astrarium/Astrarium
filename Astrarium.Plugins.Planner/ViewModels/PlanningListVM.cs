using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
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

        public string FilterString
        {
            get => GetValue<string>(nameof(FilterString));
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    TableData.Filter = null;
                    TableData.Refresh();
                }
                else
                {
                    // TODO: complex filter
                    //TableData.GroupDescriptions.Add(new CustomGroupDescription());
                    TableData.Filter = e => (e as Ephemerides).CelestialObject.Names.Any(n => n.IndexOf(value.Trim(), StringComparison.OrdinalIgnoreCase) >= 0);
                    TableData.Refresh();
                }
                SetValue(nameof(FilterString), value);
            }
        }

        public double SiderealTime
        {
            get => GetValue<double>(nameof(SiderealTime));
            set => SetValue(nameof(SiderealTime), value);
        }

        public CrdsEquatorial SunCoordinates
        {
            get => GetValue<CrdsEquatorial>(nameof(SunCoordinates));
            set => SetValue(nameof(SunCoordinates), value);
        }

        public CrdsEquatorial BodyCoordinates
        {
            get => GetValue<CrdsEquatorial>(nameof(BodyCoordinates));
            set => SetValue(nameof(BodyCoordinates), value);
        }

        public ICollectionView TableData
        {
            get => GetValue<ICollectionView>(nameof(TableData));
            set => SetValue(nameof(TableData), value);
        }
        
        public CrdsGeographical GeoLocation
        {
            get => GetValue<CrdsGeographical>(nameof(GeoLocation));
            set => SetValue(nameof(GeoLocation), value);
        }

        public TimeSpan FromTime
        {
            get => GetValue<TimeSpan>(nameof(FromTime));
            private set => SetValue(nameof(FromTime), value);
        }

        public TimeSpan ToTime
        {
            get => GetValue<TimeSpan>(nameof(ToTime));
            private set => SetValue(nameof(ToTime), value);
        }

        public Ephemerides SelectedTableItem
        {
            get => GetValue<Ephemerides>(nameof(SelectedTableItem));
            set
            {
                SetValue(nameof(SelectedTableItem), value);

                if (SelectedTableItem != null)
                {
                    var body = SelectedTableItem.CelestialObject;
                    double alpha = SelectedTableItem.GetValue<double>("Equatorial.Alpha");
                    double delta = SelectedTableItem.GetValue<double>("Equatorial.Delta");
                    CrdsEquatorial bodyCoordinates = new CrdsEquatorial(alpha, delta);

                    //CrdsEquatorial[] bodyCoordinates = new CrdsEquatorial[3];

                    //for (int i = 0; i < 3; i++)
                    //{
                    //    SkyContext context = new SkyContext(julianDay + i / 2.0, GeoLocation, preferFast: true);
                    //    var e = sky.GetEphemerides(body, context, new string[] { "Equatorial.Alpha", "Equatorial.Delta" });

                    //    double alpha = e.GetValue<double>("Equatorial.Alpha");
                    //    double delta = e.GetValue<double>("Equatorial.Delta");

                    //    bodyCoordinates[i] = new CrdsEquatorial(alpha, delta);
                    //}

                    BodyCoordinates = bodyCoordinates;
                }
                else
                {
                    BodyCoordinates = null;
                }
            }
        }

        public PlanningListVM(ISky sky, ISkyMap map, ObservationPlanner planner)
        {
            this.planner = planner;
            this.sky = sky;
            this.map = map;

            SetTimeCommand = new Command<Date>(SetTime);
            ShowObjectCommand = new Command<CelestialObject>(ShowObject);
        }

        
        private double julianDay;
        
        public async void CreatePlan(PlanningFilter filter)
        {
            julianDay = filter.JulianDayMidnight;
            GeoLocation = filter.ObserverLocation;
            FromTime = TimeSpan.FromHours(filter.TimeFrom);
            ToTime = TimeSpan.FromHours(filter.TimeTo);
            SkyContext context = new SkyContext(julianDay, GeoLocation, preferFast: true);
            SunCoordinates = context.Get(sky.SunEquatorial);
            SiderealTime = context.SiderealTime;

            var tokenSource = new CancellationTokenSource();
            var progress = new Progress<double>();

            ViewManager.ShowProgress("Please wait", "Creating observation plan...", tokenSource, progress);

            ICollection<Ephemerides> ephemerides = await Task.Run(() => planner.CreatePlan(filter, tokenSource.Token, progress));

            if (!tokenSource.IsCancellationRequested)
            {
                if (ephemerides.Any())
                {
                    TableData = CollectionViewSource.GetDefaultView(ephemerides);
                }
                tokenSource.Cancel();
            }
        }

        public class CustomGroupDescription : GroupDescription
        {
            public override object GroupNameFromItem(object item, int level, CultureInfo culture)
            {
                return Text.Get($"{(item as Ephemerides).CelestialObject.Type}.Type");
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
