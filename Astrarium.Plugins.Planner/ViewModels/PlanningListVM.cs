using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Astrarium.Plugins.Planner.ViewModels
{
    public class PlanningListVM : ViewModelBase
    {
        private readonly ISky sky;
        private readonly IMainWindow mainWindow;
        private readonly ObservationPlanner planner;

        private ICollection<Ephemerides> ephemerides;

        public ICommand SetTimeCommand { get; private set; }
        public ICommand ShowObjectCommand { get; private set; }
        public ICommand RemoveSelectedItemsCommand { get; private set; }
        public ICommand AddObjectCommand { get; private set; }
        public ICommand AddObjectsCommand { get; private set; }

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

                NotifyPropertyChanged(nameof(FilteredItemsCount));
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

        public IList SelectedTableItems
        {
            get => GetValue<IList>(nameof(SelectedTableItems));
            set
            {
                SetValue(nameof(SelectedTableItems), value);
                NotifySelectedTableItemChanged();
            }
        }

        private void NotifySelectedTableItemChanged()
        {
            NotifyPropertyChanged(
                nameof(IsSigleTableItemSelected),
                nameof(IsGoToObservationBeginEnabled),
                nameof(IsGoToObservationBestEnabled),
                nameof(IsGoToObservationEndEnabled));
        }

        public Ephemerides SelectedTableItem
        {
            get => GetValue<Ephemerides>(nameof(SelectedTableItem));
            set
            {
                SetValue(nameof(SelectedTableItem), value);
                NotifySelectedTableItemChanged();

                if (SelectedTableItem != null)
                {
                    double alpha = SelectedTableItem.GetValue<double>("Equatorial.Alpha");
                    double delta = SelectedTableItem.GetValue<double>("Equatorial.Delta");
                    BodyCoordinates = new CrdsEquatorial(alpha, delta);
                }
                else
                {
                    BodyCoordinates = null;
                }
            }
        }

        public bool IsGoToObservationBeginEnabled => IsSigleTableItemSelected && SelectedTableItem != null && !double.IsNaN(SelectedTableItem.GetValue<Date>("Observation.Begin").Day);
        public bool IsGoToObservationBestEnabled => IsSigleTableItemSelected && SelectedTableItem != null &&  !double.IsNaN(SelectedTableItem.GetValue<Date>("Observation.Best").Day);
        public bool IsGoToObservationEndEnabled => IsSigleTableItemSelected && SelectedTableItem != null && !double.IsNaN(SelectedTableItem.GetValue<Date>("Observation.End").Day);

        public bool IsSigleTableItemSelected => SelectedTableItems != null && SelectedTableItems.Count == 1;

        public int TotalItemsCount => ephemerides?.Count ?? 0;

        public int FilteredItemsCount => TableData != null ? TableData.Cast<object>().Count() : 0;
        
        public string Name { get; set; }

        public PlanningListVM(ISky sky, IMainWindow mainWindow, ObservationPlanner planner)
        {
            this.planner = planner;
            this.sky = sky;
            this.mainWindow = mainWindow;

            SetTimeCommand = new Command<Date>(SetTime);
            ShowObjectCommand = new Command<CelestialObject>(ShowObject);
            RemoveSelectedItemsCommand = new Command(RemoveSelectedItems);
            AddObjectCommand = new Command(AddObject);
            AddObjectsCommand = new Command(AddObjects);
        }
        
        private double julianDay;
        private PlanningFilter filter;
        
        public async void CreatePlan(PlanningFilter filter)
        {
            this.filter = filter;
            julianDay = filter.JulianDayMidnight;

            Name = new Date(filter.JulianDayMidnight, filter.ObserverLocation.UtcOffset).ToString();
            GeoLocation = filter.ObserverLocation;
            FromTime = TimeSpan.FromHours(filter.TimeFrom);
            ToTime = TimeSpan.FromHours(filter.TimeTo);
            SkyContext context = new SkyContext(julianDay, GeoLocation, preferFast: true);
            SunCoordinates = context.Get(sky.SunEquatorial);
            SiderealTime = context.SiderealTime;

            var tokenSource = new CancellationTokenSource();
            var progress = new Progress<double>();
            ViewManager.ShowProgress("Please wait", "Creating observation plan...", tokenSource, progress);
            ephemerides = await Task.Run(() => planner.CreatePlan(filter, tokenSource.Token, progress));

            if (!tokenSource.IsCancellationRequested)
            {
                TableData = CollectionViewSource.GetDefaultView(ephemerides);
                NotifyPropertyChanged(nameof(TotalItemsCount), nameof(FilteredItemsCount));
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
            if (SelectedTableItem != null)
            {
                ShowObject(SelectedTableItem.CelestialObject);
            }
        }

        private void ShowObject(CelestialObject body)
        {
            if (!mainWindow.CenterOnObject(body))
            {
                // TODO: localize
                if (ViewManager.ShowMessageBox("Warning", "Selected object can not be found on the sky. Remove it from the observation plan?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    ephemerides.Remove(SelectedTableItem);
                    TableData.Refresh();
                    NotifyPropertyChanged(nameof(TotalItemsCount), nameof(FilteredItemsCount));
                    NotifySelectedTableItemChanged();
                }
            }
        }

        private void RemoveSelectedItems()
        {
            if (SelectedTableItems != null && SelectedTableItems.Count > 0)
            {
                var items = SelectedTableItems.Cast<Ephemerides>().ToArray();
                var result = ViewManager.ShowMessageBox("Warning", "Do you really want to delete selected items?", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    foreach (Ephemerides item in items)
                    {
                        ephemerides.Remove(item);
                    }
                    TableData.Refresh();
                    NotifyPropertyChanged(nameof(TotalItemsCount), nameof(FilteredItemsCount));
                    NotifySelectedTableItemChanged();
                }
            }
        }

        private async void AddObjects()
        {
            var vm = ViewManager.CreateViewModel<PlanningFilterVM>();
            vm.CelestialObjectsTypes = sky.CelestialObjects.Select(c => c.Type).Where(t => t != null).Distinct().ToArray();
            vm.Filter = filter;
            if (ViewManager.ShowDialog(vm) ?? false)
            {
                var tokenSource = new CancellationTokenSource();
                var progress = new Progress<double>();
                ViewManager.ShowProgress("Please wait", "Creating observation plan...", tokenSource, progress);
                var addedItems = await Task.Run(() => planner.CreatePlan(vm.Filter, tokenSource.Token, progress));
                
                // TODO: rewrite
                foreach (var item in addedItems)
                {
                    ephemerides.Add(item);
                }
                TableData.Refresh();
                NotifyPropertyChanged(nameof(TotalItemsCount), nameof(FilteredItemsCount));
            }
        }

        private void AddObject()
        {
            var body = ViewManager.ShowSearchDialog(x => true);
            if (body != null)
            {
                var item = ephemerides.FirstOrDefault(x => x.CelestialObject.CommonName == body.CommonName && x.CelestialObject.Type == body.Type);
                if (item == null)
                {
                    AddObject(body);
                }
                TableData.MoveCurrentTo(item);
            }
        }

        public void AddObject(CelestialObject body)
        {
            Ephemerides item = planner.GetObservationDetails(filter, body);
            ephemerides.Add(item);
            TableData.Refresh();
            NotifyPropertyChanged(nameof(TotalItemsCount), nameof(FilteredItemsCount));
        }
    }
}
