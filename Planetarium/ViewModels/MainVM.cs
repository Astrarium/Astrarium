using ADK;
using ADK.Demo;
using ADK.Demo.Config;
using ADK.Demo.Objects;
using ADK.Demo.UI;
using Planetarium.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
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

        public Command<Key> MapKeyDownCommand { get; private set; }
        public Command<int> ZoomCommand { get; private set; }
        public Command<PointF> MapDoubleClickCommand { get; private set; }
        public Command<PointF> MapRightClickCommand { get; private set; }
        public Command SearchObjectCommand { get; private set; }
        public Command<PointF> CenterOnPointCommand { get; private set; }
        public Command<CelestialObject> GetObjectInfoCommand { get; private set; }
        public Command<CelestialObject> GetObjectEphemerisCommand { get; private set; }
        public Command<CelestialObject> MotionTrackCommand { get; private set; }
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
                MapViewAngleString = map.ViewAngle.ToString();

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
            SearchObjectCommand = new Command(SearchObject);
            CenterOnPointCommand = new Command<PointF>(CenterOnPoint);
            GetObjectInfoCommand = new Command<CelestialObject>(GetObjectInfo);
            GetObjectEphemerisCommand = new Command<CelestialObject>(GetObjectEphemeris);
            MotionTrackCommand = new Command<CelestialObject>(MotionTrack);
            CenterOnObjectCommand = new Command<CelestialObject>(CenterOnObject);
            ClearObjectsHistoryCommand = new Command(ClearObjectsHistory);

            map.SelectedObjectChanged += Map_SelectedObjectChanged;
            map.ViewAngleChanged += Map_ViewAngleChanged;
        }

        private void Map_ViewAngleChanged(double viewAngle)
        {
            MapViewAngleString = map.ViewAngle.ToString();
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
                if (SelectedObjectsMenuItems.Count > 12)
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
            // Add = Zoom In
            if (key == Key.Add)
            {
                Zoom(1);
            }

            // Subtract = Zoom Out
            else if (key == Key.Subtract)
            {
                Zoom(-1);
            }
            else if (key == Key.D)
            {
                var vm = new DateVM(sky.Context.JulianDay, sky.Context.GeoLocation.UtcOffset);
                if (viewManager.ShowDialog(vm) ?? false)
                {
                    sky.Context.JulianDay = vm.JulianDay;
                    sky.Calculate();
                    map.Invalidate();
                }
            }
            else if (key == Key.A)
            {
                sky.Context.JulianDay += 1;
                sky.Calculate();
                map.Invalidate();
            }
            else if (key == Key.S)
            {
                sky.Context.JulianDay -= 1;
                sky.Calculate();
                map.Invalidate();
            }
            else if (key == Key.O)
            {
                using (var frmSettings = new FormSettings(settings))
                {
                    settings.SettingValueChanged += Settings_OnSettingChanged;
                    frmSettings.ShowDialog();
                    settings.SettingValueChanged -= Settings_OnSettingChanged;
                }
            }
            else if (key == Key.I)
            {
                GetObjectInfo(map.SelectedObject);
            }
            else if (key == Key.F12)
            {
                SetFullScreen(true);
            }
            else if (key == Key.Escape)
            {
                SetFullScreen(false);
            }
            else if (key == Key.F)
            {
                SearchObject();
            }
            else if (key == Key.E)
            {
                //var formAlmanacSettings = new FormAlmanacSettings(
                //    sky.Context.JulianDayMidnight,
                //    sky.Context.GeoLocation.UtcOffset,
                //    sky.GetEventsCategories());

                //if (formAlmanacSettings.ShowDialog() == WF.DialogResult.OK)
                //{
                //    var events = await Task.Run(() =>
                //    {
                //        return sky.GetEvents(
                //            formAlmanacSettings.JulianDayFrom,
                //            formAlmanacSettings.JulianDayTo,
                //            formAlmanacSettings.Categories);
                //    });

                //    var formAlmanac = new FormAlmanac(events, sky.Context.GeoLocation.UtcOffset);
                //    if (formAlmanac.ShowDialog() == WF.DialogResult.OK)
                //    {
                //        sky.Context.JulianDay = formAlmanac.JulianDay;
                //        sky.Calculate();
                //        skyView.Invalidate();
                //    }
                //}
            }
            else if (key == Key.P)
            {
                var body = map.SelectedObject;
                if (body != null)
                {
                    GetObjectEphemeris(body);
                }
            }
            else if (key == Key.T)
            {
                MotionTrack(map.SelectedObject);
            }
            else if (key == Key.B)
            {
                viewManager.ShowDialog<SettingsVM>();
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
                Header = "Object info",
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
                Header = "Measure tool"
            });
            ContextMenuItems.Add(null);

            ContextMenuItems.Add(new MenuItemVM()
            {
                Header = "Object ephemeris",
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
                Header = "Lock / Unlock",
                IsEnabled = map.SelectedObject != null
            });
            
            NotifyPropertyChanged(nameof(ContextMenuItems));
        }

        private async void GetObjectEphemeris(CelestialObject body)
        {
            using (var formEphemerisSettings = new FormEphemerisSettings(sky, body))
            {
                if (formEphemerisSettings.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var ephem = await Task.Run(() => sky.GetEphemerides(
                        formEphemerisSettings.SelectedObject,
                        formEphemerisSettings.JulianDayFrom,
                        formEphemerisSettings.JulianDayTo,
                        formEphemerisSettings.Step,
                        formEphemerisSettings.Categories
                    ));

                    var vm = new EphemerisVM(
                        viewManager,
                        sky.GetObjectName(body),
                        ephem, 
                        formEphemerisSettings.JulianDayFrom,
                        formEphemerisSettings.JulianDayTo,
                        formEphemerisSettings.Step,
                        sky.Context.GeoLocation.UtcOffset);

                    viewManager.ShowDialog(vm);
                }
            }
        }

        private void SearchObject()
        {
            SearchVM vm = viewManager.CreateViewModel<SearchVM>();
            bool? dialogResult = viewManager.ShowDialog(vm);

            if (dialogResult != null && dialogResult.Value)
            {
                CenterOnObject(vm.SelectedItem.Body);
            }
        }

        private void CenterOnObject(CelestialObject body)
        {
            bool show = true;
            if (settings.Get<bool>("Ground") && body.Horizontal.Altitude <= 0)
            {
                show = false;
                if (viewManager.ShowMessageBox("Question", "The object is under horizon at the moment. Do you want to switch off displaying the ground?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    show = true;
                    settings.Set("Ground", false);
                }
            }

            if (show)
            {
                map.GoToObject(body, TimeSpan.FromSeconds(1));
            }
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

        private void GetObjectInfo(CelestialObject body)
        {
            if (body != null)
            {
                var info = sky.GetInfo(body);
                if (info != null)
                {
                    var model = new ObjectInfoVM(info);
                    bool? dialogResult = viewManager.ShowDialog(model);
                    if (dialogResult ?? false)
                    {
                        sky.Context.JulianDay = model.JulianDay;
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
                viewManager.ShowDialog<MotionTrackVM>();
               



                /*
                var formTrackSettings = viewManager.GetForm<FormTrackSettings>();
                formTrackSettings.Track = new Track() { Body = body, From = sky.Context.JulianDay, To = sky.Context.JulianDay + 30, LabelsStep = TimeSpan.FromDays(1) };
                if (formTrackSettings.ShowDialog() == DialogResult.OK)
                {
                    skyView.Invalidate();
                }
                */
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
