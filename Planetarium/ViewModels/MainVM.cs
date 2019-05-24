using ADK;
using Planetarium.Config;
using Planetarium.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Planetarium.ViewModels
{
    public class MainVM : ViewModelBase
    { 
        private readonly Sky sky;
        private readonly ISkyMap map;
        private readonly IViewManager viewManager;
        private readonly ISettings settings;

        public bool FullScreen { get; private set; }
        public string MapEquatorialCoordinatesString { get; private set; }
        public string MapHorizontalCoordinatesString { get; private set; }
        public string MapConstellationNameString { get; private set; }
        public string MapViewAngleString { get; private set; }
        public string DateString { get; private set; }

        public Command<Key> MapKeyDownCommand { get; private set; }
        public Command<int> ZoomCommand { get; private set; }
        public Command<PointF> MapDoubleClickCommand { get; private set; }
        public Command<PointF> MapRightClickCommand { get; private set; }
        public Command SetDateCommand { get; private set; }
        public Command SearchObjectCommand { get; private set; }
        public Command<PointF> CenterOnPointCommand { get; private set; }
        public Command<CelestialObject> GetObjectInfoCommand { get; private set; }
        public Command<CelestialObject> GetObjectEphemerisCommand { get; private set; }
        public Command<CelestialObject> MotionTrackCommand { get; private set; }
        public Command<CelestialObject> LockOnObjectCommand { get; private set; }
        public Command<PointF> MeasureToolCommand { get; private set; }
        public Command<CelestialObject> CenterOnObjectCommand { get; private set; }
        public Command ClearObjectsHistoryCommand { get; private set; }

        public ObservableCollection<MenuItemVM> ContextMenuItems { get; private set; } = new ObservableCollection<MenuItemVM>();
        public ObservableCollection<MenuItemVM> SelectedObjectsMenuItems { get; private set; } = new ObservableCollection<MenuItemVM>();
        public string SelectedObjectName { get; private set; }

        public class MenuItemVM
        {
            public bool IsChecked { get; set; }
            public bool IsEnabled { get; set; } = true;
            public string Header { get; set; }
            public ICommand Command { get; set; }
            public object CommandParameter { get; set; }
            public ObservableCollection<MenuItemVM> SubItems { get; set; }
        }

        public class ObservableUniqueItemsCollection<T> : ObservableCollection<T>
        {
            public new void Add(T item)
            {
                base.Add(item);
            }
        }

        public PointF SkyMousePosition
        {
            set
            {
                var hor = map.Projection.Invert(value);
                var eq = hor.ToEquatorial(sky.Context.GeoLocation, sky.Context.SiderealTime);

                MapEquatorialCoordinatesString = eq.ToString();
                MapHorizontalCoordinatesString = hor.ToString();
                MapConstellationNameString = Constellations.FindConstellation(eq, sky.Context.JulianDay);
                MapViewAngleString = Formatters.AngularDiameter.Format(map.ViewAngle);

                NotifyPropertyChanged(
                    nameof(MapEquatorialCoordinatesString), 
                    nameof(MapHorizontalCoordinatesString), 
                    nameof(MapConstellationNameString), 
                    nameof(MapViewAngleString));
            }
        }

        public MainVM(Sky sky, ISkyMap map, ISettings settings, IViewManager viewManager)
        {
            this.sky = sky;
            this.map = map;
            this.settings = settings;
            this.viewManager = viewManager;

            sky.Initialize();
            map.Initialize();

            sky.Calculate();

            MapKeyDownCommand = new Command<Key>(MapKeyDown);
            ZoomCommand = new Command<int>(Zoom);
            MapDoubleClickCommand = new Command<PointF>(MapDoubleClick);
            MapRightClickCommand = new Command<PointF>(MapRightClick);
            SetDateCommand = new Command(SetDate);
            SearchObjectCommand = new Command(SearchObject);
            CenterOnPointCommand = new Command<PointF>(CenterOnPoint);
            GetObjectInfoCommand = new Command<CelestialObject>(GetObjectInfo);
            GetObjectEphemerisCommand = new Command<CelestialObject>(GetObjectEphemeris);
            MotionTrackCommand = new Command<CelestialObject>(MotionTrack);
            LockOnObjectCommand = new Command<CelestialObject>(LockOnObject);
            MeasureToolCommand = new Command<PointF>(MeasureTool);
            CenterOnObjectCommand = new Command<CelestialObject>(CenterOnObject);
            ClearObjectsHistoryCommand = new Command(ClearObjectsHistory);

            sky.Context.ContextChanged += Sky_ContextChanged;
            map.SelectedObjectChanged += Map_SelectedObjectChanged;
            map.ViewAngleChanged += Map_ViewAngleChanged;

            Sky_ContextChanged();
            Map_SelectedObjectChanged(map.SelectedObject);
            Map_ViewAngleChanged(map.ViewAngle);
        }

        private void Sky_ContextChanged()
        {
            DateString = Formatters.DateTime.Format(new Date(sky.Context.JulianDay, sky.Context.GeoLocation.UtcOffset));
            NotifyPropertyChanged(nameof(DateString));
        }

        private void Map_ViewAngleChanged(double viewAngle)
        {
            MapViewAngleString = Formatters.AngularDiameter.Format(map.ViewAngle);
            NotifyPropertyChanged(nameof(MapViewAngleString));
        }

        private void Map_SelectedObjectChanged(CelestialObject body)
        {
            if (body != null)
            {
                SelectedObjectName = sky.GetObjectName(body);

                if (!SelectedObjectsMenuItems.Any())
                {          
                    SelectedObjectsMenuItems.Add(new MenuItemVM()
                    {
                        Header = "Clear all",
                        Command = ClearObjectsHistoryCommand
                    });                    
                    SelectedObjectsMenuItems.Add(null);
                }

                var existingItem = SelectedObjectsMenuItems.FirstOrDefault(i => i?.CommandParameter == body);
                if (existingItem != null)
                {
                    SelectedObjectsMenuItems.Remove(existingItem);
                }

                SelectedObjectsMenuItems.Insert(2, new MenuItemVM()
                {
                    Command = CenterOnObjectCommand,
                    CommandParameter = body,
                    Header = SelectedObjectName
                });

                // 10 items of history + "clear all" + separator
                if (SelectedObjectsMenuItems.Count > 13)
                {
                    SelectedObjectsMenuItems.RemoveAt(0);
                }
            }
            else
            {
                SelectedObjectName = "<No object>";
            }

            NotifyPropertyChanged(nameof(SelectedObjectName));
        }

        private void Zoom(int delta)
        {
            double v = map.ViewAngle;

            if (delta < 0)
            {
                v *= 1.1;
            }
            else
            {
                v /= 1.1;
            }

            if (v >= 90)
            {
                v = 90;
            }
            if (v < 1.0 / 1024.0)
            {
                v = 1.0 / 1024.0;
            }

            map.ViewAngle = v;
            map.Invalidate();
        }

        private void MapKeyDown(Key key)
        {
            // "+" = Zoom In
            if (key == Key.Add)
            {
                Zoom(1);
            }
            // "-" = Zoom Out
            else if (key == Key.Subtract)
            {
                Zoom(-1);
            }
            // "D" = [D]ate
            else if (key == Key.D)
            {
                SetDate();
            }
            // "A" = [A]dd
            else if (key == Key.A)
            {
                sky.Context.JulianDay += 1;
                sky.Calculate();
                map.Invalidate();
            }
            // "S" = [S]ubtract
            else if (key == Key.S)
            {
                sky.Context.JulianDay -= 1;
                sky.Calculate();
                map.Invalidate();
            }
            // "O" = [O]ptions
            else if (key == Key.O)
            {
                settings.SettingValueChanged += Settings_OnSettingChanged;
                viewManager.ShowDialog<SettingsVM>();
                settings.SettingValueChanged -= Settings_OnSettingChanged;
            }
            // "I" = [I]nfo
            else if (key == Key.I)
            {
                GetObjectInfo(map.SelectedObject);
            }
            // "F12" = Full Screen On
            else if (key == Key.F12)
            {
                SetFullScreen(true);
            }
            // "Esc" = Full Screen Off
            else if (key == Key.Escape)
            {
                SetFullScreen(false);
            }
            // "F" = [F]ind
            else if (key == Key.F)
            {
                SearchObject();
            }
            // "E" = [E]phemerides
            else if (key == Key.E)
            {
                GetObjectEphemeris(map.SelectedObject);
            }
            // "P" = [P]henomena
            else if (key == Key.P)
            {
                CalculatePhenomena();
            }
            // "L" = [L]ocation
            else if (key == Key.L)
            {
                var vm = viewManager.CreateViewModel<LocationVM>();
                if (viewManager.ShowDialog(vm) ?? false)
                {
                    sky.Context.GeoLocation = new CrdsGeographical(vm.ObserverLocation);
                    settings.Set("ObserverLocation", vm.ObserverLocation);
                    settings.Save();
                    sky.Calculate();
                    map.Invalidate();
                }
            }
            // "T" = [T]rack
            else if (key == Key.T)
            {
                MotionTrack(map.SelectedObject);
            }
        }

        private void MapDoubleClick(PointF point)
        {
            map.SelectedObject = map.FindObject(point);
            map.Invalidate();
            GetObjectInfo(map.SelectedObject);
        }

        private void MapRightClick(PointF point)
        {
            map.SelectedObject = map.FindObject(point);
            map.Invalidate();

            ContextMenuItems.Clear();
            
            ContextMenuItems.Add(new MenuItemVM()
            {
                Header = "Info",
                Command = MapDoubleClickCommand,
                CommandParameter = point,
                IsEnabled = map.SelectedObject != null
            });

            ContextMenuItems.Add(null);

            ContextMenuItems.Add(new MenuItemVM()
            {
                Header = "Center",
                Command = CenterOnPointCommand,
                CommandParameter = point
            });
            ContextMenuItems.Add(new MenuItemVM()
            {
                Header = "Search object...",
                Command = SearchObjectCommand
            });
            ContextMenuItems.Add(new MenuItemVM()
            {
                Header = "Go to point..."
            });
            ContextMenuItems.Add(new MenuItemVM()
            {
                Header = "Measure tool",
                Command = MeasureToolCommand,
                CommandParameter = point
            });
            ContextMenuItems.Add(null);

            ContextMenuItems.Add(new MenuItemVM()
            {
                Header = "Ephemerides",
                IsEnabled = map.SelectedObject != null && sky.GetEphemerisCategories(map.SelectedObject).Any(),
                Command = GetObjectEphemerisCommand,
                CommandParameter = map.SelectedObject
            });

            ContextMenuItems.Add(new MenuItemVM()
            {
                Header = "Motion track",
                IsEnabled = map.SelectedObject != null && map.SelectedObject is IMovingObject,
                Command = MotionTrackCommand,
                CommandParameter = map.SelectedObject
            });
            
            ContextMenuItems.Add(null);
            ContextMenuItems.Add(new MenuItemVM()
            {
                Header = map.LockedObject != null ? (map.SelectedObject != null && map.SelectedObject != map.LockedObject ? "Lock" : "Unlock") : "Lock",
                IsEnabled = map.LockedObject != null || map.SelectedObject != null,
                Command = LockOnObjectCommand,
                CommandParameter = map.SelectedObject
            });
            
            NotifyPropertyChanged(nameof(ContextMenuItems));
        }

        private async void GetObjectEphemeris(CelestialObject body)
        {
            var es = viewManager.CreateViewModel<EphemerisSettingsVM>();
            es.SelectedBody = body;
            es.JulianDayFrom = sky.Context.JulianDay;
            es.JulianDayTo = sky.Context.JulianDay + 30;
            if (viewManager.ShowDialog(es) ?? false)
            {
                var tokenSource = new CancellationTokenSource();
                var progress = new Progress<double>();

                viewManager.ShowProgress("Please wait", "Calculating ephemerides...", tokenSource, progress);

                var ephem = await Task.Run(() => sky.GetEphemerides(
                    es.SelectedBody,
                    es.JulianDayFrom,
                    es.JulianDayTo,
                    es.Step.TotalDays,
                    es.Categories,
                    tokenSource.Token,
                    progress
                ));

                if (!tokenSource.IsCancellationRequested)
                {
                    tokenSource.Cancel();
                    var vm = viewManager.CreateViewModel<EphemerisVM>();
                    vm.SetData(es.SelectedBody, es.JulianDayFrom, es.JulianDayTo, es.Step, ephem);
                    viewManager.ShowWindow(vm);
                }
            }
        }

        private async void CalculatePhenomena()
        {
            var ps = viewManager.CreateViewModel<PhenomenaSettingsVM>();
            ps.JulianDayFrom = sky.Context.JulianDay;
            ps.JulianDayTo = sky.Context.JulianDay + 30;
            if (viewManager.ShowDialog(ps) ?? false)
            {
                var tokenSource = new CancellationTokenSource();

                viewManager.ShowProgress("Please wait", "Calculating phenomena...", tokenSource);

                var events = await Task.Run(() => sky.GetEvents(
                        ps.JulianDayFrom,
                        ps.JulianDayTo,
                        ps.Categories,
                        tokenSource.Token));
               
                if (!tokenSource.IsCancellationRequested)
                {
                    tokenSource.Cancel();
                    var vm = viewManager.CreateViewModel<PhenomenaVM>();
                    vm.SetEvents(events);
                    if (viewManager.ShowDialog(vm) ?? false)
                    {
                        sky.Context.JulianDay = vm.JulianDay;
                        sky.Calculate();
                        map.Invalidate();
                    }
                }
            }    
        }

        private void SearchObject()
        {
            var vm = viewManager.CreateViewModel<SearchVM>();
            if (viewManager.ShowDialog(vm) ?? false)
            {
                CenterOnObject(vm.SelectedItem.Body);
            }
        }

        private void CenterOnObject(CelestialObject body)
        {
            if (settings.Get<bool>("Ground") && body.Horizontal.Altitude <= 0)
            {
                if (viewManager.ShowMessageBox("Question", "The object is under horizon at the moment. Do you want to switch off displaying the ground?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    settings.Set("Ground", false);
                }
                else
                {
                    return;
                }
            }

            if (map.LockedObject != null && map.LockedObject != body)
            {
                if (viewManager.ShowMessageBox("Question", "The map is locked on different celestial body. Do you want to unlock the map?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    map.LockedObject = null;
                }
                else
                {
                    return;
                }
            }
            
            map.GoToObject(body, TimeSpan.FromSeconds(1));
        }

        private void LockOnObject(CelestialObject body)
        {
            if (map.LockedObject != body)
            {
                map.LockedObject = body;
            }
            else
            {
                map.LockedObject = null;
            }
            map.Invalidate();
        }

        private void ClearObjectsHistory()
        {
            SelectedObjectsMenuItems.Clear();
        }

        private void CenterOnPoint(PointF point)
        {
            map.Center.Set(map.Projection.Invert(point));
            map.Invalidate();
        }

        private void MeasureTool(PointF point)
        {
            if (map.MeasureOrigin == null)
            {
                if (map.SelectedObject != null)
                {
                    map.MeasureOrigin = map.SelectedObject.Horizontal;
                }
                else
                {
                    map.MeasureOrigin = map.Projection.Invert(point);
                }
            }
            else
            {
                map.MeasureOrigin = null;
            }

            map.Invalidate();
        }

        private void GetObjectInfo(CelestialObject body)
        {
            if (body != null)
            {
                var info = sky.GetInfo(body);
                if (info != null)
                {
                    var vm = new ObjectInfoVM(info);
                    if (viewManager.ShowDialog(vm) ?? false)
                    {
                        sky.Context.JulianDay = vm.JulianDay;
                        sky.Calculate();
                        map.GoToObject(body, TimeSpan.Zero);
                    }
                }
            }
        }

        private void MotionTrack(CelestialObject body)
        {
            if (body != null && body is IMovingObject)
            {
                var vm = viewManager.CreateViewModel<MotionTrackVM>();
                vm.SelectedBody = body;
                vm.JulianDayFrom = sky.Context.JulianDay;
                vm.JulianDayTo = sky.Context.JulianDay + 30;
                vm.UtcOffset = sky.Context.GeoLocation.UtcOffset;

                if (viewManager.ShowDialog(vm) ?? false)
                {
                    sky.Calculate();
                    map.Invalidate();
                }
            }
        }

        private void SetDate()
        {
            var vm = new DateVM(sky.Context.JulianDay, sky.Context.GeoLocation.UtcOffset);
            if (viewManager.ShowDialog(vm) ?? false)
            {
                sky.Context.JulianDay = vm.JulianDay;
                sky.Calculate();
                map.Invalidate();
            }
        }

        private void SetFullScreen(bool isFullScreen)
        {
            FullScreen = isFullScreen;
            NotifyPropertyChanged(nameof(FullScreen));
        }

        private void Settings_OnSettingChanged(string settingName, object settingValue)
        {
            map.Invalidate();
        }
    }
}
