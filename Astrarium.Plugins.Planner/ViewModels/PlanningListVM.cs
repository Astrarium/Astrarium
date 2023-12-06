using Astrarium.Algorithms;
using Astrarium.Plugins.Planner.ImportExport;
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
        #region Dependencies

        private readonly ISky sky;
        private readonly IMainWindow mainWindow;
        private readonly IRecentPlansManager recentPlansManager;
        private readonly IObservationPlanner planner;
        private readonly IPlanManagerFactory readWriterFactory;
        private readonly ITelescopeManager telescopeManager;

        #endregion Dependencies

        private readonly List<Ephemerides> ephemerides = new List<Ephemerides>();
        private readonly ObservationPlanSmartFilter smartFilter = new ObservationPlanSmartFilter();

        #region Commands

        public ICommand SetTimeCommand { get; private set; }
        public ICommand ShowObjectCommand { get; private set; }
        public ICommand SlewTelescopeCommand { get; private set; }
        public ICommand ClearFilterCommand { get; private set; }
        public ICommand ShowSmartFilterCommand { get; private set; }
        public ICommand RemoveSelectedItemsCommand { get; private set; }
        public ICommand AddObjectCommand { get; private set; }
        public ICommand AddObjectsCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }

        #endregion Commands

        /// <summary>
        /// Flag indicating plan is already created (possible with no objects).
        /// </summary>
        private bool isInitialized = false;

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
                    try
                    {
                        var filterExpression = smartFilter.CreateFromString(value);
                        TableData.Filter = x => x != null && filterExpression((Ephemerides)x);
                    }
                    catch
                    {
                        TableData.Filter = e => (e as Ephemerides).CelestialObject.Names.Any(n => n.IndexOf(value.Trim(), StringComparison.OrdinalIgnoreCase) >= 0);
                    }
                    finally
                    {
                        TableData.Refresh();
                    }
                }
                SetValue(nameof(FilterString), value);

                NotifyTableItemsCountChanged();
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

        public string DurationString
        {
            get 
            { 
                double hours = ToTime.TotalHours - FromTime.TotalHours;
                if (hours < 0) hours += 24;
                return Formatters.TimeSpan.Format(TimeSpan.FromHours(hours));
            }
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

        public string FilePath
        {
            get => GetValue<string>(nameof(FilePath));
            set => SetValue(nameof(FilePath), value);
        }

        public bool IsSaved
        {
            get => GetValue<bool>(nameof(IsSaved));
            private set => SetValue(nameof(IsSaved), value);
        }

        private void NotifySelectedTableItemChanged()
        {
            NotifyPropertyChanged(
                nameof(IsSigleTableItemSelected),
                nameof(IsGoToObservationBeginEnabled),
                nameof(IsGoToObservationBestEnabled),
                nameof(IsGoToObservationEndEnabled));
        }

        private void NotifyTableItemsCountChanged()
        {
            NotifyPropertyChanged(
                nameof(TotalItemsCount),
                nameof(FilteredItemsCount),
                nameof(NoTotalItems),
                nameof(NoFilteredItems),
                nameof(HasItemsToDisplay));
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
        public bool NoTotalItems => isInitialized && TotalItemsCount == 0;
        public bool NoFilteredItems => isInitialized && TotalItemsCount > 0 && FilteredItemsCount == 0;
        public bool HasItemsToDisplay => isInitialized && !NoTotalItems && !NoFilteredItems;
        public bool IsTelescopeAvailable => telescopeManager.IsTelescopeAvailable;
        public bool IsTelescopeConnected => telescopeManager.IsTelescopeConnected;
        
        public DateTime Date 
        {
            get => GetValue<DateTime>(nameof(Date));
            private set => SetValue(nameof(Date), value);
        }

        public bool IsDarkMode
        {
            get => GetValue<bool>(nameof(IsDarkMode));
            private set => SetValue(nameof(IsDarkMode), value);
        }

        public PlanningListVM(ISky sky, ISettings settings, IMainWindow mainWindow, IRecentPlansManager recentPlansManager, IObservationPlanner planner, IPlanManagerFactory readWriterFactory, ITelescopeManager telescopeManager)
        {
            this.planner = planner;
            this.sky = sky;
            this.mainWindow = mainWindow;
            this.recentPlansManager = recentPlansManager;
            this.readWriterFactory = readWriterFactory;
            this.telescopeManager = telescopeManager;
            this.telescopeManager.TelescopeConnectionChanged += () => NotifyPropertyChanged(nameof(IsTelescopeConnected));

            IsDarkMode = settings.Get("NightMode");
            settings.SettingValueChanged += Settings_SettingValueChanged;

            SetTimeCommand = new Command<Date>(SetTime);
            ShowObjectCommand = new Command<CelestialObject>(ShowObject);
            SlewTelescopeCommand = new Command<CelestialObject>(SlewTelescope);
            RemoveSelectedItemsCommand = new Command(RemoveSelectedItems);
            ClearFilterCommand = new Command(ClearFilter);
            ShowSmartFilterCommand = new Command(ShowSmartFilter);
            AddObjectCommand = new Command(AddObject);
            AddObjectsCommand = new Command(AddObjects);
            SaveCommand = new Command(Save);

            TableData = CollectionViewSource.GetDefaultView(ephemerides);
        }

        private double julianDay;
        private PlanningFilter filter;

        private void Settings_SettingValueChanged(string settingName, object value)
        {
            if (settingName == "NightMode")
            {
                IsDarkMode = (bool)value;
            }
        }

        public void CreatePlan(PlanningFilter filter)
        {
            this.filter = filter;
            julianDay = filter.JulianDayMidnight;

            GeoLocation = filter.ObserverLocation;
            FromTime = TimeSpan.FromHours(filter.TimeFrom);
            ToTime = TimeSpan.FromHours(filter.TimeTo);
            NotifyPropertyChanged(nameof(DurationString));
            SkyContext context = new SkyContext(julianDay, GeoLocation, preferFast: true);
            SiderealTime = context.SiderealTime;
            SunCoordinates = context.Get(sky.SunEquatorial);
            Date = new Date(julianDay, GeoLocation.UtcOffset).ToDateTime();

            DoCreatePlan(filter);
        }

        private async void DoCreatePlan(PlanningFilter filter)
        {
            var tokenSource = new CancellationTokenSource();
            var progress = new Progress<double>();
            ViewManager.ShowProgress("$PlanningListWindow.WaitDialog.Title", "$PlanningListWindow.WaitDialog.Text", tokenSource, progress);
            var ephemerides = await Task.Run(() => planner.CreatePlan(filter, tokenSource.Token, progress));
            isInitialized = true;
            if (!tokenSource.IsCancellationRequested)
            {
                this.ephemerides.InsertRange(0, ephemerides);
                TableData.Refresh();
                NotifyTableItemsCountChanged();
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

        private void ClearFilter()
        {
            FilterString = null;
        }

        private void ShowSmartFilter()
        {
            var vm = ViewManager.CreateViewModel<SmartFilterVM>();
            vm.FilterString = FilterString;
            if (ViewManager.ShowDialog(vm) == true)
            {
                FilterString = vm.FilterString;
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
                ShowObjectNotFoundDialog();
            }
        }

        private void SlewTelescope(CelestialObject celestialObject)
        {
            CelestialObject body = sky.Search(celestialObject.Type, celestialObject.CommonName);
            if (body != null)
            {
                mainWindow.CenterOnObject(body);
                telescopeManager.SlewToCoordinates(body.Equatorial);
            }
            else
            {
                ShowObjectNotFoundDialog();
            }
        }

        private void ShowObjectNotFoundDialog()
        {
            if (ViewManager.ShowMessageBox("$Warning", "$PlanningListWindow.ObjectNotFoundDialog.Text", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                ephemerides.Remove(SelectedTableItem);
                IsSaved = false;
                TableData.Refresh();
                NotifyTableItemsCountChanged();
                NotifySelectedTableItemChanged();
            }
        }

        private void RemoveSelectedItems()
        {
            if (SelectedTableItems != null && SelectedTableItems.Count > 0)
            {
                var items = SelectedTableItems.Cast<Ephemerides>().ToArray();
                var result = ViewManager.ShowMessageBox("$Warning", "$PlanningListWindow.DeleteDialog.Text", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    foreach (Ephemerides item in items)
                    {
                        ephemerides.Remove(item);
                    }
                    TableData.Refresh();
                    IsSaved = false;
                    NotifyTableItemsCountChanged();
                    NotifySelectedTableItemChanged();
                }
            }
        }

        private void AddObjects()
        {
            var vm = ViewManager.CreateViewModel<PlanningFilterVM>();
            vm.Title = Text.Get("Planner.PlanningFilter.AddingObjects.Title");
            vm.Filter = filter;
            vm.IsDateTimeControlsVisible = false;
            if (ViewManager.ShowDialog(vm) ?? false)
            {
                DoCreatePlan(vm.Filter);
                IsSaved = false;
            }
        }

        private void AddObject()
        {
            var body = ViewManager.ShowSearchDialog(x => true);
            if (body != null)
            {
                AddObject(body);
            }
        }

        public void AddObject(CelestialObject body)
        {
            var item = ephemerides.FirstOrDefault(x => x.CelestialObject.CommonName == body.CommonName && x.CelestialObject.Type == body.Type);
            if (item == null)
            {
                item = planner.GetObservationDetails(filter, body);
                ephemerides.Insert(0, item);
                TableData.Refresh();
                TableData.MoveCurrentTo(item);
                IsSaved = false;
                NotifyTableItemsCountChanged();
            }
        }

        public override void Close()
        {
            if (IsSaved || !ephemerides.Any())
            {
                base.Close();
            }
            else
            {
                string question = string.IsNullOrEmpty(FilePath) ? Text.Get("PlanningListWindow.PlanNotSavedDialog.Text") : Text.Get("PlanningListWindow.UnsavedChangesDialog.Text");
                var answer = ViewManager.ShowMessageBox("$Warning", question, MessageBoxButton.YesNoCancel);
                switch (answer)
                {
                    case MessageBoxResult.Yes:
                        Save();
                        break;
                    case MessageBoxResult.No:
                        base.Close();
                        break;
                    default:
                        break;
                }
            }
        }

        private async void Save()
        {
            string filePath = ViewManager.ShowSaveFileDialog("$Save", $"Observation-{Date:yyyy-MM-dd}", ".plan", readWriterFactory.FormatsString, out int selectedExtensionIndex);
            if (filePath != null)
            {
                var format = readWriterFactory.GetFormat(selectedExtensionIndex);
                var writer = readWriterFactory.Create(format);
                await Task.Run(() => writer.Write(new PlanExportData() { Date = Date, Begin = FromTime, End = ToTime, Ephemerides = ephemerides }, filePath));
                IsSaved = true;
                FilePath = filePath;
                recentPlansManager.AddToRecentList(new RecentPlan(filePath, format));
            }
        }
    }
}
