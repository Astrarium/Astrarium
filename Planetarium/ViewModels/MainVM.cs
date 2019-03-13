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
        private ISkyMap map;
        private Sky sky;
        private IViewManager viewManager;
        private Settings settings;

        public bool FullScreen { get; private set; }
        public string MapEquatorialCoordinatesString { get; private set; }
        public string MapHorizontalCoordinatesString { get; private set; }
        public string MapConstellationNameString { get; private set; }
        public string MapViewAngleString { get; private set; }

        public Command<Key> MapKeyDownCommand { get; private set; }
        public Command<int> MapZoomCommand { get; private set; }
        public Command<PointF> MapDoubleClickCommand { get; private set; }
        public Command<PointF> MapRightClickCommand { get; private set; }

        public bool ContextMenuIsOpened { get; set; }
        public ObservableCollection<ContextMenuItemVM> ContextMenuItems { get; private set; } = new ObservableCollection<ContextMenuItemVM>();

        public class ContextMenuItemVM
        {
            public bool IsEnabled { get; set; } = true;
            public string Header { get; set; }
            public ICommand Command { get; set; }
            public object CommandParameter { get; set; }
            public ObservableCollection<ContextMenuItemVM> SubItems { get; set; }
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

                NotifyPropertyChanged(nameof(MapEquatorialCoordinatesString));
                NotifyPropertyChanged(nameof(MapHorizontalCoordinatesString));
                NotifyPropertyChanged(nameof(MapConstellationNameString));
                NotifyPropertyChanged(nameof(MapViewAngleString));
            }
        }

        public MainVM(ISkyMap map, Sky sky, IViewManager viewManager, Settings settings)
        {
            this.map = map;
            this.sky = sky;
            this.viewManager = viewManager;
            this.settings = settings;

            MapKeyDownCommand = new Command<Key>(MapKeyDown);
            MapZoomCommand = new Command<int>(MapZoom);
            MapDoubleClickCommand = new Command<PointF>(MapDoubleClick);
            MapRightClickCommand = new Command<PointF>(MapRightClick);
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
            else if (key == Key.F12)
            {
                FullScreen = true;
                NotifyPropertyChanged(nameof(FullScreen));
            }
            else if (key == Key.Escape)
            {
                FullScreen = false;
                NotifyPropertyChanged(nameof(FullScreen));
            }

            else if (key == Key.F)
            {
                SearchVM model = viewManager.CreateViewModel<SearchVM>();
                bool? dialogResult = viewManager.ShowDialog(model);

                if (dialogResult != null && dialogResult.Value)
                {
                    bool show = true;
                    var body = model.SelectedItem.Body;
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
                //var body = map.SelectedObject;
                //if (body != null)
                //{
                //    using (var formEphemerisSettings = new FormEphemerisSettings(sky, body))
                //    {
                //        if (formEphemerisSettings.ShowDialog() == WF.DialogResult.OK)
                //        {
                //            var ephem = await Task.Run(() => sky.GetEphemerides(
                //                formEphemerisSettings.SelectedObject,
                //                formEphemerisSettings.JulianDayFrom,
                //                formEphemerisSettings.JulianDayTo,
                //                formEphemerisSettings.Step,
                //                formEphemerisSettings.Categories
                //            ));

                //            var formEphemeris = new FormEphemeris(ephem,
                //                formEphemerisSettings.JulianDayFrom,
                //                formEphemerisSettings.JulianDayTo,
                //                formEphemerisSettings.Step,
                //                sky.Context.GeoLocation.UtcOffset);

                //            formEphemeris.Show();
                //        }
                //    }
                //}
            }
            else if (key == Key.T)
            {
                var body = map.SelectedObject;
                if (body != null && body is IMovingObject)
                {
                    //var wTrackProperties = new TrackPropertiesWindow() { Owner = this };
                    //wTrackProperties.ShowDialog();



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
        }

        private void MapZoom(int delta)
        {
            Zoom(delta);
        }

        private void MapDoubleClick(PointF point)
        {
            var body = map.FindObject(point);
            map.SelectedObject = body;
            map.Invalidate();
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

        private void MapRightClick(PointF point)
        {
            var body = map.FindObject(point);
            map.SelectedObject = body;
            map.Invalidate();

            ContextMenuItems.Clear();
            
            ContextMenuItems.Add(new ContextMenuItemVM()
            {
                Header = "Object info...",
                Command = MapDoubleClickCommand,
                CommandParameter = point,
                IsEnabled = body != null
            });

            ContextMenuItems.Add(null);

            ContextMenuItems.Add(new ContextMenuItemVM()
            {
                Header = "Center"
            });
            ContextMenuItems.Add(new ContextMenuItemVM()
            {
                Header = "Search object..."
            });
            ContextMenuItems.Add(new ContextMenuItemVM()
            {
                Header = "Go to point..."
            });
            ContextMenuItems.Add(new ContextMenuItemVM()
            {
                Header = "Measure tool"
            });
            ContextMenuItems.Add(null);

            ContextMenuItems.Add(new ContextMenuItemVM()
            {
                Header = "Object ephemerides...",
                IsEnabled = body != null && sky.GetEphemerisCategories(body).Any(),

            });

            ContextMenuItems.Add(new ContextMenuItemVM()
            {
                Header = "Motion track...",
                IsEnabled = body != null && body is IMovingObject
            });
            ContextMenuItems.Add(null);

            ContextMenuItems.Add(new ContextMenuItemVM()
            {
                Header = "Lock / Unlock",
                IsEnabled = body != null
            });
            
            NotifyPropertyChanged(nameof(ContextMenuItems));
            NotifyPropertyChanged(nameof(ContextMenuIsOpened));
        }

        private void Settings_OnSettingChanged(string settingName, object settingValue)
        {
            map.Invalidate();
        }
    }
}
