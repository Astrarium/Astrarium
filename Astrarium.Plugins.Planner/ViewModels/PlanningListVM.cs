﻿using Astrarium.Algorithms;
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
                NotifyPropertyChanged(nameof(IsSigleTableItemSelected));
            }
        }

        public Ephemerides SelectedTableItem
        {
            get => GetValue<Ephemerides>(nameof(SelectedTableItem));
            set
            {
                SetValue(nameof(SelectedTableItem), value);
                NotifyPropertyChanged(nameof(IsSigleTableItemSelected));

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

        public bool IsSigleTableItemSelected => SelectedTableItems != null && SelectedTableItems.Count == 1;

        public int TotalItemsCount => ephemerides?.Count ?? 0;

        public int FilteredItemsCount => TableData != null ? TableData.Cast<object>().Count() : 0;

        public PlanningListVM(ISky sky, IMainWindow mainWindow, ObservationPlanner planner)
        {
            this.planner = planner;
            this.sky = sky;
            this.mainWindow = mainWindow;

            SetTimeCommand = new Command<Date>(SetTime);
            ShowObjectCommand = new Command<CelestialObject>(ShowObject);
            RemoveSelectedItemsCommand = new Command(RemoveSelectedItems);
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

            ephemerides = await Task.Run(() => planner.CreatePlan(filter, tokenSource.Token, progress));

            if (!tokenSource.IsCancellationRequested)
            {
                if (ephemerides.Any())
                {
                    TableData = CollectionViewSource.GetDefaultView(ephemerides);
                    NotifyPropertyChanged(nameof(TotalItemsCount), nameof(FilteredItemsCount));
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
            if (SelectedTableItem != null)
            {
                mainWindow.CenterOnObject(SelectedTableItem.CelestialObject);
            }
        }

        private void ShowObject(CelestialObject body)
        {
            mainWindow.CenterOnObject(body);
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
                }
            }
        }
    }
}
