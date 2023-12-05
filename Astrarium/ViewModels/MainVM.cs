using Astrarium.Algorithms;
using Astrarium.Controls;
using Astrarium.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Astrarium.ViewModels
{
    public class MainVM : ViewModelBase, IMainWindow
    {
        private readonly ISky sky;
        private readonly ISkyMap map;
        private readonly ISettings settings;
        private readonly IAppUpdater appUpdater;

        public string MapViewAngleString { get; private set; }
        public string DateString { get; private set; }

        public Command<KeyEventArgs> MapKeyDownCommand { get; private set; }
        public Command<int> ZoomCommand { get; private set; }
        public Command<PointF> MapDoubleClickCommand { get; private set; }
        public Command<PointF> MapRightClickCommand { get; private set; }
        public Command SetDateCommand { get; private set; }
        public Command SelectLocationCommand { get; private set; }
        public Command SearchObjectCommand { get; private set; }
        public Command CenterOnPointCommand { get; private set; }
        public Command<CelestialObject> GetObjectInfoCommand { get; private set; }
        public Command GetObjectEphemerisCommand { get; private set; }
        public Command CalculatePhenomenaCommand { get; private set; }
        public Command<CelestialObject> MotionTrackCommand { get; private set; }
        public Command<CelestialObject> LockOnObjectCommand { get; private set; }
        public Command<PointF> MeasureToolCommand { get; private set; }
        public Command<CelestialObject> GoToHistoryItemCommand { get; private set; }
        public Command ClearObjectsHistoryCommand { get; private set; }
        public Command ChangeSettingsCommand { get; private set; }
        public Command ShowAboutCommand { get; private set; }
        public Command CheckForUpdatesCommand { get; private set; }
        public Command ExitAppCommand { get; private set; }
        public Command<CelestialObject> QuickSearchCommand { get; private set; }

        public ObservableCollection<MenuItem> MainMenuItems { get; private set; } = new ObservableCollection<MenuItem>();
        public ObservableCollection<MenuItem> ContextMenuItems { get; private set; } = new ObservableCollection<MenuItem>();
        public ObservableCollection<MenuItem> SelectedObjectsMenuItems { get; private set; } = new ObservableCollection<MenuItem>();
        public ObservableCollection<ToolbarItem> ToolbarItems { get; private set; } = new ObservableCollection<ToolbarItem>();
        public ISuggestionProvider SearchProvider { get; private set; }
        public CelestialObject SelectedObject { get; private set; }

        public WindowState WindowState
        {
            get => settings.Get<bool>("StartMaximized") ? WindowState.Maximized : WindowState.Normal;
        }

        public bool FullScreen
        {
            get => GetValue<bool>(nameof(FullScreen));
            set => SetValue(nameof(FullScreen), value);
        }

        public bool IsCompactMenu
        {
            get => GetValue<bool>(nameof(IsCompactMenu));
            set => SetValue(nameof(IsCompactMenu), value);
        }

        public bool IsToolbarVisible
        {
            get => GetValue<bool>(nameof(IsToolbarVisible));
            set => SetValue(nameof(IsToolbarVisible), value);
        }

        public bool IsStatusBarVisible
        {
            get => GetValue<bool>(nameof(IsStatusBarVisible));
            set => SetValue(nameof(IsStatusBarVisible), value);
        }

        private System.Drawing.Size windowSize;
        public System.Drawing.Size WindowSize
        {
            get => settings.Get("RememberWindowSize") ? settings.Get<System.Drawing.Size>("WindowSize") : System.Drawing.Size.Empty;
            set 
            {
                if (settings.Get("RememberWindowSize") && value != WindowSize)
                {
                    windowSize = value;
                    settings.SetAndSave("WindowSize", windowSize);
                }
            }
        }

        public class SearchSuggestionProvider : ISuggestionProvider
        {
            private readonly ISky sky;

            public SearchSuggestionProvider(ISky sky)
            {
                this.sky = sky;
            }

            public IEnumerable GetSuggestions(string filter)
            {
                return sky.Search(filter, body => true, 10);
            }
        }

        public string FPS
        {
            set => SetValue(nameof(FPS), value);
            get => GetValue<string>(nameof(FPS));
        }

        public string MouseConstellation
        {
            set => SetValue(nameof(MouseConstellation), value);
            get => GetValue<string>(nameof(MouseConstellation));
        }

        public CrdsEquatorial MouseEquatorialCoordinates
        {
            set => SetValue(nameof(MouseEquatorialCoordinates), value);
            get => GetValue<CrdsEquatorial>(nameof(MouseEquatorialCoordinates));
        }

        public CrdsHorizontal MouseHorizontalCoordinates
        {
            set => SetValue(nameof(MouseHorizontalCoordinates), value);
            get => GetValue<CrdsHorizontal>(nameof(MouseHorizontalCoordinates));
        }

        private bool dateTimeSync = false;

        public bool TimeSync
        {
            get => dateTimeSync;
            set
            {
                dateTimeSync = value;
                sky.TimeSync = value;
                map.TimeSync = value;
            }
        }

        public MainVM(ISky sky, ISkyMap map, IAppUpdater appUpdater, ISettings settings, UIElementsIntegration uiIntegration)
        {
            this.sky = sky;
            this.map = map;
            this.settings = settings;
            this.appUpdater = appUpdater;

            if (settings.Get("CheckUpdatesOnStart"))
            {
                Task.Run(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(3));
                    appUpdater.CheckUpdates(x => OnAppUpdateFound(x));
                });
            }

            sky.Calculate();

            MapKeyDownCommand = new Command<KeyEventArgs>(MapKeyDown);
            ZoomCommand = new Command<int>(Zoom);
            MapDoubleClickCommand = new Command<PointF>(MapDoubleClick);
            MapRightClickCommand = new Command<PointF>(MapRightClick);
            SetDateCommand = new Command(SetDate);
            SelectLocationCommand = new Command(SelectLocation);
            SearchObjectCommand = new Command(SearchObject);
            QuickSearchCommand = new Command<CelestialObject>(GoToObject);
            CenterOnPointCommand = new Command(CenterOnPoint);
            GetObjectInfoCommand = new Command<CelestialObject>(GetObjectInfo);
            GetObjectEphemerisCommand = new Command(GetObjectEphemeris);
            CalculatePhenomenaCommand = new Command(CalculatePhenomena);
            LockOnObjectCommand = new Command<CelestialObject>(LockOnObject);
            GoToHistoryItemCommand = new Command<CelestialObject>(GoToObject);
            ClearObjectsHistoryCommand = new Command(ClearObjectsHistory);
            ChangeSettingsCommand = new Command(ChangeSettings);
            ShowAboutCommand = new Command(ShowAbout);
            CheckForUpdatesCommand = new Command(CheckForUpdates);
            ExitAppCommand = new Command(Application.Current.Shutdown);
            SearchProvider = new SearchSuggestionProvider(sky);

            sky.Context.ContextChanged += Sky_ContextChanged;
            sky.Calculated += map.Invalidate;
            sky.TimeSyncChanged += () => NotifyPropertyChanged(nameof(TimeSync));
            map.SelectedObjectChanged += Map_SelectedObjectChanged;
            map.LockedObjectChanged += Map_LockedObjectChanged;
            map.FovChanged += Map_ViewAngleChanged;
            settings.SettingValueChanged += (s, v) => map.Invalidate();

            AddBinding(new SimpleBinding(settings, "IsToolbarVisible", nameof(IsToolbarVisible)));
            AddBinding(new SimpleBinding(settings, "IsCompactMenu", nameof(IsCompactMenu)));
            AddBinding(new SimpleBinding(settings, "IsStatusBarVisible", nameof(IsStatusBarVisible)));

            Sky_ContextChanged();
            Map_SelectedObjectChanged(map.SelectedObject);

            // Toolbar initialization

            // predefined toolbar groups
            List<string> groups = new List<string> { "Objects", "Constellations", "Grids", "Ground" };

            foreach (var group in uiIntegration.ToolbarButtons.Groups.OrderBy(g => groups.IndexOf(g)))
            {
                foreach (var button in uiIntegration.ToolbarButtons[group])
                {
                    ToolbarItems.Add(button);
                }
                ToolbarItems.Add(null);
            }

            ToolbarItems.Add(new ToolbarButton("IconSettings", "$Menu.Settings", ChangeSettingsCommand));

            // Main window menu

            // MAP
            var menuMap = new MenuItem("$Menu.Map")
            {
                SubItems = new ObservableCollection<MenuItem>()
            };

            var menuMapTransformMirror = new MenuItem("$Menu.MapTransform.Mirror") { IsCheckable = true };
            menuMapTransformMirror.Command = new Command(() => {
                menuMapTransformMirror.IsChecked = !menuMapTransformMirror.IsChecked;
                settings.SetAndSave("IsMirrored", menuMapTransformMirror.IsChecked);
            });
            menuMapTransformMirror.AddBinding(new SimpleBinding(settings, "IsMirrored", nameof(MenuItem.IsChecked)));

            var menuMapTransformInvert = new MenuItem("$Menu.MapTransform.Invert") { IsCheckable = true };
            menuMapTransformInvert.Command = new Command(() => {
                menuMapTransformInvert.IsChecked = !menuMapTransformInvert.IsChecked;
                settings.SetAndSave("IsInverted", menuMapTransformInvert.IsChecked);
            });
            menuMapTransformInvert.AddBinding(new SimpleBinding(settings, "IsInverted", nameof(MenuItem.IsChecked)));
            
            var menuMountModeHorizontal = new MenuItem("$Menu.MountMode.Horizontal") { IsCheckable = true, IsChecked = true };
            var menuMountModeEquatorial = new MenuItem("$Menu.MountMode.Equatorial") { IsCheckable = true };
            
            menuMountModeHorizontal.Command = new Command(() => {
                menuMountModeHorizontal.IsChecked = true;
                menuMountModeEquatorial.IsChecked = false;
                map.Projection.ViewMode = ProjectionViewType.Horizontal;
                map.Invalidate();
            });
            
            menuMountModeEquatorial.Command = new Command(() => {
                menuMountModeEquatorial.IsChecked = true;
                menuMountModeHorizontal.IsChecked = false;
                map.Projection.ViewMode = ProjectionViewType.Equatorial;
                map.Invalidate();
            });

            var menuMapMountMode = new MenuItem("$Menu.MountMode")
            {
                SubItems = new ObservableCollection<MenuItem>(new MenuItem[] {
                    menuMountModeHorizontal,
                    menuMountModeEquatorial
                })
            };

            var projectionTypes = System.Reflection.Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Projection)) && !t.IsAbstract).ToArray();

            var projectionMenuItems = projectionTypes.Select(t => new MenuItem(t.Name, new Command(() => map.SetProjection(t))));

            var menuMapProjection = new MenuItem("Projection")
            {
                SubItems = new ObservableCollection<MenuItem>(projectionMenuItems)
            };

            var menuMapTransform = new MenuItem("$Menu.MapTransform")
            {
                SubItems = new ObservableCollection<MenuItem>(new MenuItem[] {
                    menuMapTransformMirror,
                    menuMapTransformInvert
                })
            };

            var menuColorSchema = new MenuItem("$Menu.ColorSchema")
            {
                SubItems = new ObservableCollection<MenuItem>(Enum.GetValues(typeof(ColorSchema))
                    .Cast<ColorSchema>()
                    .Select(s =>
                    {
                        var menuItem = new MenuItem($"$Settings.Schema.{s}");
                        menuItem.Command = new Command(() => {
                            menuItem.IsChecked = true;
                            settings.SetAndSave("Schema", s);
                        });
                        menuItem.AddBinding(new SimpleBinding(settings, "Schema", "IsChecked")
                        {
                            SourceToTargetConverter = (schema) => (ColorSchema)schema == s,
                            TargetToSourceConverter = (isChecked) => (bool)isChecked ? s : settings.Get<ColorSchema>("Schema"),                            
                        });
                        return menuItem;
                    }))
            };

            menuMap.SubItems.Add(menuMapProjection);
            menuMap.SubItems.Add(menuMapTransform);
            menuMap.SubItems.Add(menuMapMountMode);
            menuMap.SubItems.Add(null);
            menuMap.SubItems.Add(menuColorSchema);
            menuMap.SubItems.Add(null);

            foreach (var group in groups)
            {
                if (uiIntegration.ToolbarButtons.Groups.Any(g => g == group))
                {
                    var menuGroup = new MenuItem($"$Menu.{group}");
                    foreach (var button in uiIntegration.ToolbarButtons[group].OfType<ToolbarToggleButton>())
                    {
                        var binding = button.FindBinding(nameof(ToolbarToggleButton.IsChecked));
                        if (binding != null)
                        {
                            var menuItem = new MenuItem(button.Tooltip);
                            menuItem.Command = new Command(() => {
                                menuItem.IsChecked = !menuItem.IsChecked;
                                if (binding.Source == settings)
                                {
                                    settings.SetAndSave(binding.SourcePropertyName, menuItem.IsChecked);
                                }
                            });
                            menuItem.AddBinding(new SimpleBinding(binding.Source, binding.SourcePropertyName, nameof(MenuItem.IsChecked)));
                            menuGroup.SubItems.Add(menuItem);
                        }

                        button.Command = new Command(() => {
                            if (binding.Source == settings)
                            {
                                settings.SetAndSave(binding.SourcePropertyName, button.IsChecked);
                            }
                        });
                    }
                    menuMap.SubItems.Add(menuGroup);
                }
            }

            menuMap.SubItems.Add(null);
            menuMap.SubItems.Add(new MenuItem("$Menu.Exit") { Command = ExitAppCommand, HotKey = new KeyGesture(Key.None, ModifierKeys.None, "Alt+F4") });

            MainMenuItems.Add(menuMap);
            

            // VIEW

            var menuView = new MenuItem("$Menu.View")
            {
                SubItems = new ObservableCollection<MenuItem>()
            };

            menuView.SubItems.Add(null);

            var menuCompact = new MenuItem("$Menu.CompactMenu");
            menuCompact.Command = new Command(() => {
                menuCompact.IsChecked = !menuCompact.IsChecked;
                settings.SetAndSave("IsCompactMenu", menuCompact.IsChecked);
            });
            menuCompact.AddBinding(new SimpleBinding(settings, "IsCompactMenu", nameof(MenuItem.IsChecked)));
            menuView.SubItems.Add(menuCompact);

            var menuToolbar = new MenuItem("$Menu.Toolbar");
            menuToolbar.Command = new Command(() => {
                menuToolbar.IsChecked = !menuToolbar.IsChecked;
                settings.SetAndSave("IsToolbarVisible", menuToolbar.IsChecked);
            });
            menuToolbar.AddBinding(new SimpleBinding(settings, "IsToolbarVisible", nameof(MenuItem.IsChecked)));
            menuView.SubItems.Add(menuToolbar);

            var menuStatusbar = new MenuItem("$Menu.StatusBar");
            menuStatusbar.Command = new Command(() => {
                menuStatusbar.IsChecked = !menuStatusbar.IsChecked;
                settings.SetAndSave("IsStatusBarVisible", menuStatusbar.IsChecked);
            });
            menuStatusbar.AddBinding(new SimpleBinding(settings, "IsStatusBarVisible", nameof(MenuItem.IsChecked)));
            menuView.SubItems.Add(menuStatusbar);

            menuView.SubItems.Add(null);

            var menuFullScreen = new MenuItem("$Menu.FullScreen");
            menuFullScreen.Command = new Command(() => menuFullScreen.IsChecked = !menuFullScreen.IsChecked);
            menuFullScreen.AddBinding(new SimpleBinding(this, nameof(FullScreen), nameof(MenuItem.IsChecked)));
            menuFullScreen.HotKey = new KeyGesture(Key.F12, ModifierKeys.None, "F12");
            menuView.SubItems.Add(menuFullScreen);

            MainMenuItems.Add(menuView);

            var menuTools = new MenuItem("$Menu.Tools")
            {
                SubItems = new ObservableCollection<MenuItem>()
                {
                    new MenuItem("$Menu.Search", SearchObjectCommand) { HotKey = new KeyGesture(Key.F, ModifierKeys.Control, "Ctrl+F") },
                    new MenuItem("$Menu.Phenomena", CalculatePhenomenaCommand) { HotKey = new KeyGesture(Key.P, ModifierKeys.Control, "Ctrl+P") },
                    new MenuItem("$Menu.Ephemerides", GetObjectEphemerisCommand) { HotKey = new KeyGesture(Key.E, ModifierKeys.Control, "Ctrl+E") }                
                }
            };

            // "Tools" menu items from plugins
            foreach (var menuItem in uiIntegration.MenuItems[MenuItemPosition.MainMenuTools])
            {
                menuTools.SubItems.Add(menuItem);
            }

            MainMenuItems.Add(menuTools);

            // top-level menu items from plugins
            foreach (var menuItem in uiIntegration.MenuItems[MenuItemPosition.MainMenuTop])
            {
                MainMenuItems.Add(menuItem);
            }

            var menuOptions = new MenuItem("$Menu.Options")
            {
                SubItems = new ObservableCollection<MenuItem>()
                {
                    new MenuItem("$Menu.DateTime", SetDateCommand) { HotKey = new KeyGesture(Key.D, ModifierKeys.Control, "Ctrl+D") },
                    new MenuItem("$Menu.ObserverLocation", SelectLocationCommand) { HotKey = new KeyGesture(Key.L, ModifierKeys.Control, "Ctrl+L") },
                    new MenuItem("$Menu.Settings", ChangeSettingsCommand) { HotKey = new KeyGesture(Key.O, ModifierKeys.Control, "Ctrl+O") },
                    null,
                    new MenuItem("$Menu.Language")
                    {
                        SubItems = new ObservableCollection<MenuItem>(Text.GetLocales().Select(loc => {
                            var menuItem = new MenuItem(loc.NativeName);
                            menuItem.Command = new Command(() => {
                                menuItem.IsChecked = true;
                                settings.SetAndSave("Language", loc.Name);
                            });
                            menuItem.AddBinding(new SimpleBinding(settings, "Language", nameof(MenuItem.IsChecked))
                            {
                                SourceToTargetConverter = (lang) => (string)lang == loc.Name,
                                TargetToSourceConverter = (isChecked) => (bool)isChecked ? loc.Name : settings.Get<string>("Language"),
                            });
                            return menuItem;
                        }))
                    },
                    null,
                    new MenuItem("$Menu.CheckForUpdates", CheckForUpdatesCommand),
                    new MenuItem("$Menu.About", ShowAboutCommand)
                }
            };
            MainMenuItems.Add(menuOptions);

            // Context menu initialization

            var menuInfo = new MenuItem("$ContextMenu.Info", GetObjectInfoCommand);
            menuInfo.HotKey = new KeyGesture(Key.I, ModifierKeys.Control);
            menuInfo.AddBinding(new SimpleBinding(map, nameof(map.SelectedObject), nameof(MenuItem.CommandParameter)));
            menuInfo.AddBinding(new SimpleBinding(map, nameof(map.SelectedObject), nameof(MenuItem.IsEnabled))
            {
                SourceToTargetConverter = (o) => map.SelectedObject != null
            });

            ContextMenuItems.Add(menuInfo);

            ContextMenuItems.Add(null);

            ContextMenuItems.Add(new MenuItem("$ContextMenu.Center", CenterOnPointCommand));
            ContextMenuItems.Add(new MenuItem("$ContextMenu.Search", SearchObjectCommand));

            ContextMenuItems.Add(null);

            var menuEphemerides = new MenuItem("$ContextMenu.Ephemerides", GetObjectEphemerisCommand);
            menuEphemerides.HotKey = new KeyGesture(Key.E, ModifierKeys.Control, "Ctrl+E");
            menuEphemerides.AddBinding(new SimpleBinding(map, nameof(map.SelectedObject), nameof(MenuItem.IsEnabled))
            {
                SourceToTargetConverter = (o) => map.SelectedObject != null && sky.GetEphemerisCategories(map.SelectedObject).Any()
            });

            ContextMenuItems.Add(menuEphemerides);

            // dynamic menu items from plugins
            foreach (var configItem in uiIntegration.MenuItems[MenuItemPosition.ContextMenu])
            {
                ContextMenuItems.Add(configItem);
            }

            ContextMenuItems.Add(null);

            var menuLock = new MenuItem("", LockOnObjectCommand);
            menuLock.AddBinding(new SimpleBinding(map, nameof(map.SelectedObject), nameof(MenuItem.Header))
            {
                SourceToTargetConverter = (o) => map.LockedObject != null ? (map.SelectedObject != null && map.SelectedObject != map.LockedObject ? Text.Get("ContextMenu.Lock") : Text.Get("ContextMenu.Unlock")) : Text.Get("ContextMenu.Lock")
            });
            menuLock.AddBinding(new SimpleBinding(map, nameof(map.SelectedObject), nameof(MenuItem.IsEnabled))
            {
                SourceToTargetConverter = (o) => map.LockedObject != null || map.SelectedObject != null
            });
            menuLock.AddBinding(new SimpleBinding(map, nameof(map.SelectedObject), nameof(MenuItem.CommandParameter)));

            ContextMenuItems.Add(menuLock);
        }

        private void OnAppUpdateFound(LastRelease lastRelease)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var vm = ViewManager.CreateViewModel<AppUpdateVM>();
                vm.SetReleaseInfo(lastRelease);
                ViewManager.ShowDialog(vm);
            });
        }

        private void OnAppUpdateError(Exception ex)
        {
            Application.Current.Dispatcher.Invoke(() => ViewManager.ShowMessageBox("$Error", $"Unable to check app updates: {ex.Message}"));
        }

        private void Sky_ContextChanged()
        {
            var months = CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames.Take(12).ToArray();
            var d = new Date(sky.Context.JulianDay, sky.Context.GeoLocation.UtcOffset);
            DateString = $"{(int)d.Day:00} {months[d.Month - 1]} {d.Year} {d.Hour:00}:{d.Minute:00}:{d.Second:00}";
            NotifyPropertyChanged(nameof(DateString));
        }

        private void Map_ViewAngleChanged(double viewAngle)
        {
            MapViewAngleString = Formatters.Angle.Format(viewAngle);
            NotifyPropertyChanged(nameof(MapViewAngleString));
        }

        private void Map_SelectedObjectChanged(CelestialObject body)
        {
            SelectedObject = body;

            if (body != null)
            {
                if (!SelectedObjectsMenuItems.Any())
                {
                    SelectedObjectsMenuItems.Add(new MenuItem("$StatusBar.ClearSelectedObjectsList", ClearObjectsHistoryCommand));                
                    SelectedObjectsMenuItems.Add(null);
                }

                var existingItem = SelectedObjectsMenuItems.FirstOrDefault(i => body.Equals(i?.CommandParameter));
                if (existingItem != null)
                {
                    SelectedObjectsMenuItems.Remove(existingItem);
                }

                SelectedObjectsMenuItems.Insert(2, new MenuItem(body.Names.First(), GoToHistoryItemCommand, body));

                // 10 items of history + "clear all" + separator
                if (SelectedObjectsMenuItems.Count > 13)
                {
                    SelectedObjectsMenuItems.RemoveAt(0);
                }
            }

            NotifyPropertyChanged(nameof(SelectedObject));
        }

        private void Map_LockedObjectChanged(CelestialObject obj)
        {
            if (obj == null)
            {
                // TODO: localize
                Application.Current.Dispatcher.Invoke(() => ViewManager.ShowPopupMessage("Map is unlocked"));
            }
        }

        private void Zoom(int delta)
        {
            map.Projection.Fov *= Math.Pow(1.1, -delta / 120);
            map.Invalidate();
        }
       
        private IEnumerable<MenuItem> GetMenuItems(IEnumerable<MenuItem> items)
        {
            foreach (var childNode in items.Where(i => i != null))
            {
                yield return childNode;
                               
                foreach (var child in GetMenuItems(childNode.SubItems ?? new ObservableCollection<MenuItem>()))
                    yield return child;
            }
        }

        private void MapKeyDown(KeyEventArgs args)
        {
            var item = GetMenuItems(MainMenuItems.Concat(ContextMenuItems)).FirstOrDefault(i => i.HotKey != null && i.HotKey.Matches(null, args));
            if (item != null)
            {
                item.Command.Execute(item.CommandParameter);
            }
            else
            {
                Key key = args.Key;

                // "+" = Zoom In
                if (key == Key.Add)
                {
                    Zoom(120);
                }
                // "-" = Zoom Out
                else if (key == Key.Subtract)
                {
                    Zoom(-120);
                }
                // "A" = [A]dd
                else if (key == Key.A)
                {
                    sky.Context.JulianDay += 5.0 / 24 / 60;
                    sky.Calculate();
                }
                // "S" = [S]ubtract
                else if (key == Key.S)
                {
                    sky.Context.JulianDay -= 5.0 / 24 / 60;
                    sky.Calculate();
                }
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
        }

        private async void GetObjectEphemeris()
        {
            var es = ViewManager.CreateViewModel<EphemerisSettingsVM>();
            es.SelectedBody = map.SelectedObject;
            es.JulianDayFrom = sky.Context.JulianDay;
            es.JulianDayTo = sky.Context.JulianDay + 30;
            if (ViewManager.ShowDialog(es) ?? false)
            {
                var tokenSource = new CancellationTokenSource();
                var progress = new Progress<double>();

                ViewManager.ShowProgress("$CalculateEphemerides.WaitTitle", "$CalculateEphemerides.WaitText", tokenSource, progress);

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
                    var vm = ViewManager.CreateViewModel<EphemerisVM>();
                    vm.SetData(es.SelectedBody, es.JulianDayFrom, es.JulianDayTo, es.Step, ephem);
                    ViewManager.ShowWindow(vm);
                }
            }
        }

        private async void CalculatePhenomena()
        {
            var ps = ViewManager.CreateViewModel<PhenomenaSettingsVM>();

            if (ViewManager.ShowDialog(ps) ?? false)
            {
                var tokenSource = new CancellationTokenSource();
                ViewManager.ShowProgress("$CalculatePhenomena.WaitTitle", "$CalculatePhenomena.WaitText", tokenSource);

                var events = await Task.Run(() => sky.GetEvents(
                        ps.JulianDayFrom,
                        ps.JulianDayTo,
                        ps.Categories,
                        tokenSource.Token), tokenSource.Token);
               
                if (!tokenSource.IsCancellationRequested)
                {
                    tokenSource.Cancel();
                    var vm = ViewManager.CreateViewModel<PhenomenaVM>();
                    vm.SetEvents(events);
                    vm.OnEventSelected += OnPhenomenaSelected;
                    ViewManager.ShowWindow(vm);
                }
            }
        }

        private void OnPhenomenaSelected(AstroEvent ev)
        {
            sky.SetDate(ev.JulianDay);
            if (ev.PrimaryBody != null)
            {
                if (ev.SecondaryBody != null)
                {
                    var eq = Angle.Intermediate(ev.PrimaryBody.Equatorial, ev.SecondaryBody.Equatorial, 0.5);

                    var targetViewAngle = Angle.Separation(ev.PrimaryBody.Equatorial, ev.SecondaryBody.Equatorial) * 3;

                    if (ev.PrimaryBody is SizeableCelestialObject pb && ev.SecondaryBody is SizeableCelestialObject sb)
                    {
                        var minSemidiamter = Math.Min(pb.Semidiameter, sb.Semidiameter) / 3600;
                        if (minSemidiamter > targetViewAngle)
                        {
                            targetViewAngle = minSemidiamter * 3;
                        }
                    }

                    CenterOnPoint(eq, targetViewAngle);
                }
                else
                {
                    GoToObject(ev.PrimaryBody);
                }
            }
        }

        private void SearchObject()
        {
            CelestialObject body = ViewManager.ShowSearchDialog();
            if (body != null)
            {
                GoToObject(body);
            }
        }

        private void ChangeSettings()
        {
            ViewManager.ShowDialog<SettingsVM>();
        }

        private void ShowAbout()
        {
            ViewManager.ShowDialog<AboutVM>();
        }

        private async void CheckForUpdates()
        {
            await Task.Run(() => appUpdater.CheckUpdates(x => OnAppUpdateFound(x), x => OnAppUpdateError(x)));
        }

        private void GoToObject(CelestialObject body)
        {
            if (!CenterOnObject(body))
            {
                // TODO: localize
                ViewManager.ShowMessageBox("Warning", "Selected object can not be found on the sky.");
            }
        }

        public bool CenterOnObject(CelestialObject celestialObject)
        {
            CelestialObject body = sky.Search(celestialObject.Type, celestialObject.CommonName);
            if (body == null)
            {
                return false;
            }

            if (body.DisplaySettingNames.Any(s => !settings.Get(s)))
            {
                if (ViewManager.ShowMessageBox("$ObjectInvisible.Title", "$ObjectInvisible.Text", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    body.DisplaySettingNames.ToList().ForEach(s => settings.Set(s, true));
                }
                else
                {
                    return true;
                }
            }

            if (settings.Get<bool>("Ground"))
            {
                var hor = body.Equatorial.ToHorizontal(sky.Context.GeoLocation, sky.Context.SiderealTime);

                if (hor.Altitude <= 0)
                {
                    if (ViewManager.ShowMessageBox("$ObjectUnderHorizon.Title", "$ObjectUnderHorizon.Text", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        settings.Set("Ground", false);
                    }
                    else
                    {
                        return true;
                    }
                }
            }

            if (map.LockedObject != null && map.LockedObject != body)
            {
                if (ViewManager.ShowMessageBox("$ObjectLocked.Title", "$ObjectLocked.Text", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    map.LockedObject = null;
                }
                else
                {
                    return true;
                }
            }

            map.GoToObject(body, TimeSpan.FromSeconds(1));

            return true;
        }

        private void CenterOnPoint()
        {
            
        }

        public void CenterOnPoint(CrdsEquatorial eq, double targetViewAngle)
        {
            var hor = eq.ToHorizontal(sky.Context.GeoLocation, sky.Context.SiderealTime);

            if (settings.Get<bool>("Ground") && hor.Altitude <= 0)
            {
                if (ViewManager.ShowMessageBox("$PointUnderHorizon.Title", "$PointUnderHorizon.Text", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    settings.Set("Ground", false);
                }
                else
                {
                    return;
                }
            }

            if (map.LockedObject != null)
            {
                if (ViewManager.ShowMessageBox("$ObjectLocked.Title", "$ObjectLocked.Text", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    map.LockedObject = null;
                }
                else
                {
                    return;
                }
            }

            map.GoToPoint(eq, TimeSpan.FromSeconds(1), targetViewAngle);
        }

        public void Focus()
        {
            ViewManager.ShowWindow<MainVM>(true);
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

        private void GetObjectInfo(CelestialObject body)
        {
            if (body != null)
            {
                var info = sky.GetInfo(body);
                if (info != null)
                {
                    var vm = new ObjectInfoVM(info);
                    if (ViewManager.ShowDialog(vm) ?? false)
                    {
                        sky.SetDate(vm.JulianDay);
                        map.GoToObject(body, TimeSpan.Zero, 0);
                    }
                }
            }
        }

        private void SetDate()
        {
            double? jd = ViewManager.ShowDateDialog(sky.Context.JulianDay, sky.Context.GeoLocation.UtcOffset);
            if (jd != null)
            {
                sky.SetDate(jd.Value);
            }
        }

        private void SelectLocation()
        {
            CrdsGeographical location = ViewManager.ShowLocationDialog(sky.Context.GeoLocation);
            if (location != null)
            {
                sky.Context.GeoLocation = new CrdsGeographical(location);
                settings.SetAndSave("ObserverLocation", location);
                sky.Calculate();
            }
        }
    }
}
