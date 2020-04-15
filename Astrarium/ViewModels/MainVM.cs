using Astrarium.Algorithms;
using Astrarium.Controls;
using Astrarium.Objects;
using Astrarium.Types;
using Astrarium.Types.Localization;
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Astrarium.ViewModels
{
    public class MainVM : ViewModelBase
    {
        private readonly ISky sky;
        private readonly ISkyMap map;
        private readonly ISettings settings;

        public string MapEquatorialCoordinatesString { get; private set; }
        public string MapHorizontalCoordinatesString { get; private set; }
        public string MapConstellationNameString { get; private set; }
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
        public Command<CelestialObject> CenterOnObjectCommand { get; private set; }
        public Command ClearObjectsHistoryCommand { get; private set; }
        public Command ChangeSettingsCommand { get; private set; }
        public Command SaveAsImageCommand { get; private set; }
        public Command PrintCommand { get; private set; }
        public Command PrintPreviewCommand { get; private set; }
        public Command ExitAppCommand { get; private set; }
        public Command<SearchResultItem> QuickSearchCommand { get; private set; }

        public ObservableCollection<MenuItem> MainMenuItems { get; private set; } = new ObservableCollection<MenuItem>();
        public ObservableCollection<MenuItem> ContextMenuItems { get; private set; } = new ObservableCollection<MenuItem>();
        public ObservableCollection<MenuItem> SelectedObjectsMenuItems { get; private set; } = new ObservableCollection<MenuItem>();
        public ObservableCollection<ToolbarItem> ToolbarItems { get; private set; } = new ObservableCollection<ToolbarItem>();
        public ISuggestionProvider SearchProvider { get; private set; }
        public string SelectedObjectName { get; private set; }

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

        public bool DateTimeSync
        {
            get { return sky.DateTimeSync; }
            set { sky.DateTimeSync = value; }
        }

        public MainVM(ISky sky, ISkyMap map, ISettings settings, UIElementsIntegration uiIntegration)
        {
            this.sky = sky;
            this.map = map;
            this.settings = settings;

            sky.Calculate();

            MapKeyDownCommand = new Command<KeyEventArgs>(MapKeyDown);
            ZoomCommand = new Command<int>(Zoom);
            MapDoubleClickCommand = new Command<PointF>(MapDoubleClick);
            MapRightClickCommand = new Command<PointF>(MapRightClick);
            SetDateCommand = new Command(SetDate);
            SelectLocationCommand = new Command(SelectLocation);
            SearchObjectCommand = new Command(SearchObject);
            QuickSearchCommand = new Command<SearchResultItem>(sr => CenterOnObject(sr.Body));
            CenterOnPointCommand = new Command(CenterOnPoint);
            GetObjectInfoCommand = new Command<CelestialObject>(GetObjectInfo);
            GetObjectEphemerisCommand = new Command(GetObjectEphemeris);
            CalculatePhenomenaCommand = new Command(CalculatePhenomena);
            LockOnObjectCommand = new Command<CelestialObject>(LockOnObject);
            CenterOnObjectCommand = new Command<CelestialObject>(CenterOnObject);
            ClearObjectsHistoryCommand = new Command(ClearObjectsHistory);
            ChangeSettingsCommand = new Command(ChangeSettings);
            SaveAsImageCommand = new Command(SaveAsImage);
            PrintCommand = new Command(Print);
            PrintPreviewCommand = new Command(PrintPreview);
            ExitAppCommand = new Command(Application.Current.Shutdown);
            SearchProvider = new SearchSuggestionProvider(sky);

            sky.Context.ContextChanged += Sky_ContextChanged;
            sky.Calculated += () => map.Invalidate();
            sky.DateTimeSyncChanged += () => NotifyPropertyChanged(nameof(DateTimeSync));
            map.SelectedObjectChanged += Map_SelectedObjectChanged;
            map.ViewAngleChanged += Map_ViewAngleChanged;
            settings.SettingValueChanged += (s, v) => map.Invalidate();

            AddBinding(new SimpleBinding(settings, "IsToolbarVisible", nameof(IsToolbarVisible)));
            AddBinding(new SimpleBinding(settings, "IsCompactMenu", nameof(IsCompactMenu)));
            AddBinding(new SimpleBinding(settings, "IsStatusBarVisible", nameof(IsStatusBarVisible)));

            Sky_ContextChanged();
            Map_SelectedObjectChanged(map.SelectedObject);
            Map_ViewAngleChanged(map.ViewAngle);

            foreach (var group in uiIntegration.ToolbarButtons.Groups)
            {
                foreach (var button in uiIntegration.ToolbarButtons[group])
                {
                    ToolbarItems.Add(button);
                }
                ToolbarItems.Add(null);
            }

            ToolbarItems.Add(new ToolbarButton("IconSettings", "$Settings", ChangeSettingsCommand));

            // Main window menu

            MainMenuItems.Add(new MenuItem("Map")
            {
                SubItems = new ObservableCollection<MenuItem>()
                {
                    new MenuItem("Save as image...", SaveAsImageCommand),
                    null,
                    new MenuItem("Print...", PrintCommand),
                    new MenuItem("Print preview...", PrintPreviewCommand),
                    null,
                    new MenuItem("Exit", ExitAppCommand)
                }
            });

            var menuView = new MenuItem("View");
            menuView.SubItems = new ObservableCollection<MenuItem>();

            var menuColorSchema = new MenuItem("Color Schema")
            {
                SubItems = new ObservableCollection<MenuItem>(Enum.GetValues(typeof(ColorSchema))
                    .Cast<ColorSchema>()
                    .Select(s =>
                    {
                        var menuItem = new MenuItem($"$Settings.Schema.{s}");
                        menuItem.Command = new Command(() => menuItem.IsChecked = true);
                        menuItem.AddBinding(new SimpleBinding(settings, "Schema", "IsChecked")
                        {
                            SourceToTargetConverter = (schema) => (ColorSchema)schema == s,
                            TargetToSourceConverter = (isChecked) => (bool)isChecked ? s : settings.Get<ColorSchema>("Schema"),                            
                        });
                        return menuItem;
                    }))
            };
            menuView.SubItems.Add(menuColorSchema);

            menuView.SubItems.Add(null);

            foreach (var group in uiIntegration.ToolbarButtons.Groups)
            {
                var menuGroup = new MenuItem(group);
                foreach (var button in uiIntegration.ToolbarButtons[group].OfType<ToolbarToggleButton>())
                {
                    var binding = button.Bindings.FirstOrDefault(b => b.TargetPropertyName == nameof(ToolbarToggleButton.IsChecked));
                    if (binding != null)
                    {
                        var menuItem = new MenuItem(button.Tooltip);
                        menuItem.Command = new Command(() => menuItem.IsChecked = !menuItem.IsChecked);
                        menuItem.AddBinding(new SimpleBinding(binding.Source, binding.SourcePropertyName, nameof(MenuItem.IsChecked)));
                        menuGroup.SubItems.Add(menuItem);
                    }
                }

                menuView.SubItems.Add(menuGroup);
            }

            menuView.SubItems.Add(null);

            var menuCompact = new MenuItem("Compact menu");
            menuCompact.Command = new Command(() => menuCompact.IsChecked = !menuCompact.IsChecked);
            menuCompact.AddBinding(new SimpleBinding(settings, "IsCompactMenu", nameof(MenuItem.IsChecked)));
            menuView.SubItems.Add(menuCompact);

            var menuToolbar = new MenuItem("Toolbar");
            menuToolbar.Command = new Command(() => menuToolbar.IsChecked = !menuToolbar.IsChecked);
            menuToolbar.AddBinding(new SimpleBinding(settings, "IsToolbarVisible", nameof(MenuItem.IsChecked)));
            menuView.SubItems.Add(menuToolbar);

            var menuStatusbar = new MenuItem("Statusbar");
            menuStatusbar.Command = new Command(() => menuStatusbar.IsChecked = !menuStatusbar.IsChecked);
            menuStatusbar.AddBinding(new SimpleBinding(settings, "IsStatusBarVisible", nameof(MenuItem.IsChecked)));
            menuView.SubItems.Add(menuStatusbar);

            menuView.SubItems.Add(null);

            var menuFullScreen = new MenuItem("Full Screen");
            menuFullScreen.Command = new Command(() => menuFullScreen.IsChecked = !menuFullScreen.IsChecked);
            menuFullScreen.AddBinding(new SimpleBinding(this, nameof(FullScreen), nameof(MenuItem.IsChecked)));
            menuFullScreen.HotKey = new KeyGesture(Key.F12, ModifierKeys.None, "F12");
            menuView.SubItems.Add(menuFullScreen);

            MainMenuItems.Add(menuView);

            var menuTools = new MenuItem("Tools")
            {
                SubItems = new ObservableCollection<MenuItem>()
                {
                    new MenuItem("Search object", SearchObjectCommand) { HotKey = new KeyGesture(Key.F, ModifierKeys.Control, "Ctrl+F") },
                    new MenuItem("Astronomical phenomena", CalculatePhenomenaCommand) { HotKey = new KeyGesture(Key.P, ModifierKeys.Control, "Ctrl+P") },
                    new MenuItem("Ephemerides", GetObjectEphemerisCommand) { HotKey = new KeyGesture(Key.E, ModifierKeys.Control, "Ctrl+E") }                
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

            var menuOptions = new MenuItem("Options")
            {
                SubItems = new ObservableCollection<MenuItem>()
                {
                    new MenuItem("Date & Time", SetDateCommand) { HotKey = new KeyGesture(Key.D, ModifierKeys.Control, "Ctrl+D") },
                    new MenuItem("Observer location", SelectLocationCommand) { HotKey = new KeyGesture(Key.L, ModifierKeys.Control, "Ctrl+L") },
                    null,
                    new MenuItem("Language")
                    {
                        SubItems = new ObservableCollection<MenuItem>(Text.GetLocales().Select(loc => {
                            var menuItem = new MenuItem(loc.NativeName);
                            menuItem.Command = new Command(() => menuItem.IsChecked = true);
                            menuItem.AddBinding(new SimpleBinding(settings, "Language", nameof(MenuItem.IsChecked)) 
                            {
                                SourceToTargetConverter = (lang) => (string)lang == loc.Name,
                                TargetToSourceConverter = (isChecked) => (bool)isChecked ? loc.Name : settings.Get<string>("Language"),
                            });
                            return menuItem;
                        }))
                    },
                    null,
                    new MenuItem("$Menu.Settings", ChangeSettingsCommand) { HotKey = new KeyGesture(Key.O, ModifierKeys.Control, "Ctrl+O") }
                }
            };
            MainMenuItems.Add(menuOptions);

            // Context menu

            var menuInfo = new MenuItem("Info", GetObjectInfoCommand);
            menuInfo.HotKey = new KeyGesture(Key.I, ModifierKeys.Control);
            menuInfo.AddBinding(new SimpleBinding(map, nameof(map.SelectedObject), nameof(MenuItem.CommandParameter)));
            menuInfo.AddBinding(new SimpleBinding(map, nameof(map.SelectedObject), nameof(MenuItem.IsEnabled))
            {
                SourceToTargetConverter = (o) => map.SelectedObject != null
            });

            ContextMenuItems.Add(menuInfo);

            ContextMenuItems.Add(null);

            ContextMenuItems.Add(new MenuItem("Center", CenterOnPointCommand));
            ContextMenuItems.Add(new MenuItem("Search object...", SearchObjectCommand));
            ContextMenuItems.Add(new MenuItem("Go to point..."));

            ContextMenuItems.Add(null);

            var menuEphemerides = new MenuItem("Ephemerides", GetObjectEphemerisCommand);
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
                SourceToTargetConverter = (o) => map.LockedObject != null ? (map.SelectedObject != null && map.SelectedObject != map.LockedObject ? "Lock" : "Unlock") : "Lock"
            });
            menuLock.AddBinding(new SimpleBinding(map, nameof(map.SelectedObject), nameof(MenuItem.IsEnabled))
            {
                SourceToTargetConverter = (o) => map.LockedObject != null || map.SelectedObject != null
            });
            menuLock.AddBinding(new SimpleBinding(map, nameof(map.SelectedObject), nameof(MenuItem.CommandParameter)));

            ContextMenuItems.Add(menuLock);
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
            MapViewAngleString = Formatters.AngularDiameter.Format(map.ViewAngle);
            NotifyPropertyChanged(nameof(MapViewAngleString));
        }

        private void Map_SelectedObjectChanged(CelestialObject body)
        {
            if (body != null)
            {
                SelectedObjectName = body.Names.First();

                if (!SelectedObjectsMenuItems.Any())
                {
                    SelectedObjectsMenuItems.Add(new MenuItem("Clear all", ClearObjectsHistoryCommand));                
                    SelectedObjectsMenuItems.Add(null);
                }

                var existingItem = SelectedObjectsMenuItems.FirstOrDefault(i => body.Equals(i?.CommandParameter));
                if (existingItem != null)
                {
                    SelectedObjectsMenuItems.Remove(existingItem);
                }

                SelectedObjectsMenuItems.Insert(2, new MenuItem(SelectedObjectName, CenterOnObjectCommand, body));

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
            map.ViewAngle *= Math.Pow(1.1, -delta / 120);
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
                    sky.Context.JulianDay += 1;
                    sky.Calculate();
                }
                // "S" = [S]ubtract
                else if (key == Key.S)
                {
                    sky.Context.JulianDay -= 1;
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

                ViewManager.ShowProgress("Please wait", "Calculating ephemerides...", tokenSource, progress);

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
            ps.JulianDayFrom = sky.Context.JulianDay;
            ps.JulianDayTo = sky.Context.JulianDay + 30;
            if (ViewManager.ShowDialog(ps) ?? false)
            {
                var tokenSource = new CancellationTokenSource();

                ViewManager.ShowProgress("Please wait", "Calculating phenomena...", tokenSource);

                var events = await Task.Run(() => sky.GetEvents(
                        ps.JulianDayFrom,
                        ps.JulianDayTo,
                        ps.Categories,
                        tokenSource.Token));
               
                if (!tokenSource.IsCancellationRequested)
                {
                    tokenSource.Cancel();
                    var vm = ViewManager.CreateViewModel<PhenomenaVM>();
                    vm.SetEvents(events);
                    if (ViewManager.ShowDialog(vm) ?? false)
                    {
                        sky.SetDate(vm.JulianDay);                        
                        if (vm.Body != null) 
                        {
                            map.GoToObject(vm.Body, TimeSpan.Zero);
                        }
                    }
                }
            }    
        }

        private void SearchObject()
        {
            CelestialObject body = ViewManager.ShowSearchDialog();
            if (body != null)
            {
                CenterOnObject(body);
            }
        }

        private void ChangeSettings()
        {
            ViewManager.ShowDialog<SettingsVM>();
        }

        private void SaveAsImage()
        {
            var formats = new Dictionary<string, ImageFormat>()
            {
                ["Bitmap (*.bmp)|*.bmp"] = ImageFormat.Bmp,
                ["Portable Network Graphics (*.png)|*.png"] = ImageFormat.Png,
                ["Graphics Interchange Format (*.gif)|*.gif"] = ImageFormat.Gif,
                ["Joint Photographic Experts Group (*.jpg)|*.jpg"] = ImageFormat.Jpeg
            };

            string fileName = ViewManager.ShowSaveFileDialog("Save as image", "Map", formats.Keys.First(), string.Join("|", formats.Keys));
            if (fileName != null)
            {
                using (Image img = new Bitmap(map.Width, map.Height))
                using (Graphics g = Graphics.FromImage(img))
                {
                    map.Render(g);
                    string key = formats.Keys.FirstOrDefault(k => k.Split('|')[1].Substring(1).Equals(Path.GetExtension(fileName))) ?? formats.Keys.First();
                    img.Save(fileName, formats[key]);
                }
            }
        }

        private PrintDocument CreatePrintDocument()
        {
            var document = new PrintDocument()
            {
                DocumentName = "Map",
                DefaultPageSettings = new PageSettings() 
                { 
                    Landscape = true 
                }
            };
            document.PrintPage += new PrintPageEventHandler(PrintHandler);
            return document;
        }

        private void PrintHandler(object sender, PrintPageEventArgs e)
        {
            int oldWidth = map.Width;
            int oldHeight = map.Height;
            map.Width = e.PageSettings.Bounds.Width - 1;
            map.Height = e.PageSettings.Bounds.Height - 1;           
            map.Render(e.Graphics);
            map.Width = oldWidth;
            map.Height = oldHeight;
        }

        private void Print()
        {
            var document = CreatePrintDocument();
            if (ViewManager.ShowPrintDialog(document))
            {
                document.Print();
            }
        }

        private void PrintPreview()
        {
            var document = CreatePrintDocument();
            ViewManager.ShowPrintPreviewDialog(document);
        }

        private void CenterOnObject(CelestialObject body)
        {
            if (settings.Get<bool>("Ground") && body.Horizontal.Altitude <= 0)
            {
                if (ViewManager.ShowMessageBox("Question", "The object is under horizon at the moment. Do you want to switch off displaying the ground?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
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
                if (ViewManager.ShowMessageBox("Question", "The map is locked on different celestial body. Do you want to unlock the map?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
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

        private void CenterOnPoint()
        {
            map.Center.Set(map.MousePosition);
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
                    if (ViewManager.ShowDialog(vm) ?? false)
                    {
                        sky.SetDate(vm.JulianDay);
                        map.GoToObject(body, TimeSpan.Zero);
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
            var vm = ViewManager.CreateViewModel<LocationVM>();       
            if (ViewManager.ShowDialog(vm) ?? false)
            {
                sky.Context.GeoLocation = new CrdsGeographical(vm.ObserverLocation);
                settings.Set("ObserverLocation", vm.ObserverLocation);
                settings.Save();
                sky.Calculate();
            }            
        }
    }
}
