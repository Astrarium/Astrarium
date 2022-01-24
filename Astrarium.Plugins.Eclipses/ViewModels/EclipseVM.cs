using Astrarium.Algorithms;
using Astrarium.Plugins.Eclipses.ImportExport;
using Astrarium.Plugins.Eclipses.Types;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;

namespace Astrarium.Plugins.Eclipses.ViewModels
{
    /// <summary>
    /// Base class for Lunar and Solar eclipses view models
    /// </summary>
    public abstract class EclipseVM : ViewModelBase
    {
        #region Fields

        protected static NumberFormatInfo nf;

        protected double julianDay;
        protected int meeusLunationNumber;
        protected int citiesListTableLunationNumber;
        protected int currentSarosSeries;
        protected CrdsGeographical observerLocation;
        protected IGeoLocationsManager locationsManager;
        protected IEclipsesCalculator eclipsesCalculator;
        protected ISettings settings;

        protected readonly MarkerStyle observerLocationMarkerStyle = new MarkerStyle(5, Brushes.Black, null, Brushes.Black, SystemFonts.DefaultFont, StringFormat.GenericDefault);
        protected readonly MarkerStyle citiesListMarkerStyle = new MarkerStyle(5, Brushes.Green, null, Brushes.Black, SystemFonts.DefaultFont, StringFormat.GenericDefault);

        #endregion Fields

        #region Properties

        /// <summary>
        /// Date of the eclipse selected, converted to string
        /// </summary>
        public string EclipseDate 
        {
            get => GetValue<string>(nameof(EclipseDate));
            protected set => SetValue(nameof(EclipseDate), value);
        }

        /// <summary>
        /// Directory to store maps cache
        /// </summary>
        public string CacheFolder 
        {
            get => GetValue<string>(nameof(CacheFolder));
            protected set => SetValue(nameof(CacheFolder), value);
        }

        /// <summary>
        /// Index of currently selected tab page
        /// </summary>
        public int SelectedTabIndex
        {
            get => GetValue<int>(nameof(SelectedTabIndex));
            set
            {
                SetValue(nameof(SelectedTabIndex), value);
                CalculateSarosSeries();
                CalculateCitiesTable();
            }
        }

        /// <summary>
        /// Flag indicating calculation is in progress
        /// </summary>
        public bool IsCalculating
        {
            get => GetValue<bool>(nameof(IsCalculating));
            protected set => SetValue(nameof(IsCalculating), value);
        }

        /// <summary>
        /// Collection of map tile servers to switch between them
        /// </summary>
        public ICollection<ITileServer> TileServers
        {
            get => GetValue<ICollection<ITileServer>>(nameof(TileServers));
            protected set => SetValue(nameof(TileServers), value);
        }

        /// <summary>
        /// Collection of markers (points) on the map
        /// </summary>
        public ObservableCollection<Marker> Markers
        {
            get => GetValue(nameof(Markers), new ObservableCollection<Marker>());
            protected set => SetValue(nameof(Markers), value);
        }

        /// <summary>
        /// Collection of tracks (lines) on the map
        /// </summary>
        public ICollection<Track> Tracks
        {
            get => GetValue<ICollection<Track>>(nameof(Tracks), new ObservableCollection<Track>());
            protected set => SetValue(nameof(Tracks), value);
        }

        /// <summary>
        /// Collection of polygons (areas) on the map
        /// </summary>
        public ICollection<Polygon> Polygons
        {
            get => GetValue<ICollection<Polygon>>(nameof(Polygons), new ObservableCollection<Polygon>());
            protected set => SetValue(nameof(Polygons), value);
        }

        /// <summary>
        /// Flag indicating previous saros button is enabled
        /// </summary>
        public bool PrevSarosEnabled 
        {
            get => GetValue<bool>(nameof(PrevSarosEnabled));
            protected set => SetValue(nameof(PrevSarosEnabled), value); 
        }

        /// <summary>
        /// Flag indicating next saros button is enabled
        /// </summary>
        public bool NextSarosEnabled
        {
            get => GetValue<bool>(nameof(NextSarosEnabled));
            protected set => SetValue(nameof(NextSarosEnabled), value);
        }

        /// <summary>
        /// Name of the observer location from settings
        /// </summary>
        public string SettingsLocationName
        {
            get => GetValue<string>(nameof(SettingsLocationName));
            protected set => SetValue(nameof(SettingsLocationName), value);
        }

        /// <summary>
        /// Table header for Besselian elements table
        /// </summary>
        public string BesselianElementsTableHeader
        {
            get => GetValue<string>(nameof(BesselianElementsTableHeader));
            protected set => SetValue(nameof(BesselianElementsTableHeader), value);
        }

        /// <summary>
        /// Table footer for Besselian elements table
        /// </summary>
        public string BesselianElementsTableFooter
        {
            get => GetValue<string>(nameof(BesselianElementsTableFooter));
            protected set => SetValue(nameof(BesselianElementsTableFooter), value);
        }

        /// <summary>
        /// Title of Saros series table
        /// </summary>
        public string SarosSeriesTableTitle
        {
            get => GetValue<string>(nameof(SarosSeriesTableTitle));
            protected set => SetValue(nameof(SarosSeriesTableTitle), value);
        }

        /// <summary>
        /// Flag indicating locations list table is not empty
        /// </summary>
        public bool IsCitiesListTableNotEmpty
        {
            get => GetValue<bool>(nameof(IsCitiesListTableNotEmpty));
            protected set => SetValue(nameof(IsCitiesListTableNotEmpty), value);
        }

        /// <summary>
        /// General eclipse info table
        /// </summary>
        public ObservableCollection<NameValueTableItem> EclipseGeneralDetails 
        {
            get => GetValue(nameof(EclipseGeneralDetails), new ObservableCollection<NameValueTableItem>());
            protected set => SetValue(nameof(EclipseGeneralDetails), value);
        }

        /// <summary>
        /// Eclipse contacts info table
        /// </summary>
        public ObservableCollection<ContactsTableItem> EclipseContacts 
        {
            get => GetValue(nameof(EclipseContacts), new ObservableCollection<ContactsTableItem>());
            protected set => SetValue(nameof(EclipseContacts), value);
        } 

        /// <summary>
        /// Saros series table
        /// </summary>
        public ObservableCollection<SarosSeriesTableItem> SarosSeriesTable
        {
            get => GetValue(nameof(SarosSeriesTable), new ObservableCollection<SarosSeriesTableItem>());
            protected set => SetValue(nameof(SarosSeriesTable), value);
        }

        /// <summary>
        /// Eclipse description
        /// </summary>
        public string EclipseDescription
        {
            get => GetValue<string>(nameof(EclipseDescription));
            protected set => SetValue(nameof(EclipseDescription), value);
        }

        /// <summary>
        /// Image attributes used to modify the eclipse map tiles
        /// </summary>
        public ImageAttributes TileImageAttributes
        {
            get => GetValue<ImageAttributes>(nameof(TileImageAttributes));
            set => SetValue(nameof(TileImageAttributes), value);
        }

        /// <summary>
        /// Flag indicating mouse is over eclipse map
        /// </summary>
        public bool IsMouseOverMap
        {
            get => GetValue<bool>(nameof(IsMouseOverMap));
            set
            {
                SetValue(nameof(IsMouseOverMap), value);
                if (!IsMapLocked)
                {
                    CalculateLocalCircumstances(observerLocation);
                }
            }
        }

        /// <summary>
        /// Tile server of the eclipse map
        /// </summary>
        public ITileServer TileServer
        {
            get => GetValue<ITileServer>(nameof(TileServer));
            set
            {
                SetValue(nameof(TileServer), value);
                TileImageAttributes = GetImageAttributes();
                if (settings.Get<string>("EclipseMapTileServer") != value.Name)
                {
                    settings.SetAndSave("EclipseMapTileServer", value.Name);
                }
            }
        }

        /// <summary>
        /// Current geographical coordinates of the mouse over the eclipse map
        /// </summary>
        public GeoPoint MapMouse
        {
            get => GetValue<GeoPoint>(nameof(MapMouse));
            set
            {
                SetValue(nameof(MapMouse), value);
                MapMouseString = Format.Geo.Format(FromGeoPoint(value));
                if (!IsMapLocked)
                {
                    CalculateLocalCircumstances(FromGeoPoint(value));
                }
            }
        }

        /// <summary>
        /// Location nearest to the current mouse position on the map
        /// </summary>
        public CrdsGeographical NearestLocation
        {
            get => GetValue<CrdsGeographical>(nameof(NearestLocation));
            protected set => SetValue(nameof(NearestLocation), value);
        }

        /// <summary>
        /// Flag indicating the map is locked on some point
        /// </summary>
        public bool IsMapLocked
        {
            get => GetValue<bool>(nameof(IsMapLocked));
            set
            {
                SetValue(nameof(IsMapLocked), value);
                if (!value)
                {
                    observerLocation = settings.Get<CrdsGeographical>("ObserverLocation");
                    CalculateLocalCircumstances(observerLocation);
                    AddLocationMarker();
                }
            }
        }

        /// <summary>
        /// Current geographical coordinates of eclipse map center
        /// </summary>
        public GeoPoint MapCenter
        {
            get => GetValue(nameof(MapCenter), new GeoPoint());
            set => SetValue(nameof(MapCenter), value);
        }

        /// <summary>
        /// Current zoom level of the eclipse map
        /// </summary>
        public int MapZoomLevel
        {
            get => GetValue(nameof(MapZoomLevel), 1);
            set => SetValue(nameof(MapZoomLevel), value);
        }

        /// <summary>
        /// Formatted string representing current mouse coordinates on the map
        /// </summary>
        public string MapMouseString
        {
            get => GetValue<string>(nameof(MapMouseString));
            protected set => SetValue(nameof(MapMouseString), value);
        }

        /// <summary>
        /// Flag indicating dark mode is used
        /// </summary>
        public bool IsDarkMode
        {
            get => GetValue<bool>(nameof(IsDarkMode));
            protected set => SetValue(nameof(IsDarkMode), value);
        }

        /// <summary>
        /// Background color for eclipse map thumbnails
        /// </summary>
        public Color MapThumbnailBackColor
        {
            get => GetValue<Color>(nameof(MapThumbnailBackColor));
            protected set => SetValue(nameof(MapThumbnailBackColor), value);
        }

        /// <summary>
        /// Foreground color for eclipse map thumbnails text
        /// </summary>
        public Color MapThumbnailForeColor
        {
            get => GetValue<Color>(nameof(MapThumbnailForeColor));
            protected set => SetValue(nameof(MapThumbnailForeColor), value);
        }

        /// <summary>
        /// Name of current observer location point
        /// </summary>
        public string ObserverLocationName
        {
            get => GetValue<string>(nameof(ObserverLocationName));
            protected set => SetValue(nameof(ObserverLocationName), value);
        }

        /// <summary>
        /// String description of local visibility, like "Visible as partial", "Invisible" and etc.
        /// </summary>
        public string LocalVisibilityDescription
        {
            get => GetValue<string>(nameof(LocalVisibilityDescription));
            protected set => SetValue(nameof(LocalVisibilityDescription), value);
        }

        /// <summary>
        /// Flag indicating the eclipse is visible from current place
        /// </summary>
        public bool IsVisibleFromCurrentPlace
        {
            get => GetValue<bool>(nameof(IsVisibleFromCurrentPlace));
            protected set => SetValue(nameof(IsVisibleFromCurrentPlace), value);
        }

        /// <summary>
        /// Current zoom level of local view chart
        /// </summary>
        public float ChartZoomLevel
        {
            get => GetValue<float>(nameof(ChartZoomLevel));
            set => SetValue(nameof(ChartZoomLevel), value);
        }

        #endregion Properties

        #region Commands

        public ICommand ChangeDateCommand => new Command(ChangeDate);
        public ICommand PrevEclipseCommand => new Command(PrevEclipse);
        public ICommand NextEclipseCommand => new Command(NextEclipse);
        public ICommand PrevSarosCommand => new Command(PrevSaros);
        public ICommand NextSarosCommand => new Command(NextSaros);
        public ICommand LockOnCurrentPositionCommand => new Command(LockOnCurrentPosition);
        public ICommand LockOnNearestLocationCommand => new Command(LockOnNearestLocation);
        public ICommand AddCurrentPositionToCitiesListCommand => new Command(AddCurrentPositionToCitiesList);
        public ICommand AddNearestLocationToCitiesListCommand => new Command(AddNearestLocationToCitiesList);
        public ICommand RightClickOnMapCommand => new Command(RightClickOnMap);
        public ICommand SarosSeriesTableSetDateCommand => new Command<int>(SarosSeriesTableSetDate);
        public ICommand ExportSarosSeriesTableCommand => new Command(ExportSarosSeriesTable);
        public ICommand CitiesListTableGoToCoordinatesCommand => new Command<CrdsGeographical>(CitiesListTableGoToCoordinates);
        public ICommand ShowLocationsFileFormatCommand => new Command(ShowLocationsFileFormat);
        public ICommand ChartZoomInCommand => new Command(ChartZoomIn);
        public ICommand ChartZoomOutCommand => new Command(ChartZoomOut);

        #endregion Commands

        /// <summary>
        /// Static initialization
        /// </summary>
        static EclipseVM()
        {
            nf = new NumberFormatInfo();
            nf.NumberDecimalSeparator = ".";
            nf.NumberGroupSeparator = "\u2009";
        }

        /// <summary>
        /// Creates new instance of <see cref="EclipseVM"/>.
        /// </summary>
        protected EclipseVM(ISky sky, IEclipsesCalculator eclipsesCalculator, IGeoLocationsManager locationsManager, ISettings settings)
        {
            this.settings = settings;
            this.settings.PropertyChanged += Settings_PropertyChanged;
            this.observerLocation = settings.Get<CrdsGeographical>("ObserverLocation");
            this.locationsManager = locationsManager;
            this.eclipsesCalculator = eclipsesCalculator;
            this.locationsManager.Load();

            SetMapColors();

            ChartZoomLevel = 1;
            SettingsLocationName = $"{Text.Get("EclipseView.SettingsLocationName")} ({observerLocation.LocationName})";
            CacheFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Astrarium", "MapsCache");

            TileServers = new List<ITileServer>()
            {
                new OfflineTileServer(),
                new OpenStreetMapTileServer("Astrarium v1.0 contact astrarium@astrarium.space"),
                new StamenTerrainTileServer(),
                new OpenTopoMapServer()
            };

            string tileServerName = settings.Get<string>("EclipseMapTileServer");
            var tileServer = TileServers.FirstOrDefault(s => s.Name.Equals(tileServerName));
            TileServer = tileServer ?? TileServers.First();

            IsDarkMode = settings.Get<ColorSchema>("Schema") == ColorSchema.Red;

            meeusLunationNumber = LunarEphem.Lunation(sky.Context.JulianDay, LunationSystem.Meeus);

            CalculateEclipse(next: true, saros: false);
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Schema")
            {
                IsDarkMode = settings.Get<ColorSchema>("Schema") == ColorSchema.Red;
                TileImageAttributes = GetImageAttributes();
                SetMapColors();
            }
        }

        protected void AddLocationMarker()
        {
            Markers.Remove(Markers.FirstOrDefault(m => m.Data as string == "CurrentLocation"));
            Markers.Add(new Marker(ToGeo(observerLocation), observerLocationMarkerStyle, observerLocation.LocationName) { Data = "CurrentLocation" });
        }

        protected void AddCitiesListMarker(CrdsGeographical g)
        {
            Markers.Add(new Marker(ToGeo(g), citiesListMarkerStyle, g.LocationName) { MinZoomToDisplayLabel = 10, Data = "CitiesList" });
        }

        private void ChartZoomIn()
        {
            ChartZoomLevel = Math.Min(3, ChartZoomLevel * 1.1f);
        }

        private void ChartZoomOut()
        {
            ChartZoomLevel = Math.Max(1.0f / 3f, ChartZoomLevel / 1.1f);
        }

        private void ChangeDate()
        {
            var d = new Date(julianDay, 0);
            var jd0 = new Date(d.Year, d.Month, 1).ToJulianEphemerisDay();
            var jd = ViewManager.ShowDateDialog(jd0, 0, DateOptions.MonthYear);
            if (jd != null && Math.Abs(jd.Value - jd0) > 0.5)
            {
                meeusLunationNumber = LunarEphem.Lunation(jd.Value, LunationSystem.Meeus) - 1;
                CalculateEclipse(next: true, saros: false);
            }
        }

        private void PrevEclipse()
        {
            CalculateEclipse(next: false, saros: false);
        }

        private void NextEclipse()
        {
            CalculateEclipse(next: true, saros: false);
        }

        private void PrevSaros()
        {
            CalculateEclipse(next: false, saros: true);
        }

        private void NextSaros()
        {
            CalculateEclipse(next: true, saros: true);
        }

        private void LockOnCurrentPosition()
        {
            var g = FromGeoPoint(MapMouse);
            g.LocationName = Text.Get("EclipseView.LockedPoint");
            LockOn(g);
        }

        private void LockOnNearestLocation()
        {
            LockOn(NearestLocation);
        }

        private void LockOn(CrdsGeographical location)
        {
            observerLocation = location;
            AddLocationMarker();
            IsMapLocked = true;
            CalculateLocalCircumstances(observerLocation);
        }

        private void AddCurrentPositionToCitiesList()
        {
            var g = FromGeoPoint(MapMouse);
            g.LocationName = Text.Get("EclipseView.NoName");
            AddToCitiesList(g);
        }

        private void AddNearestLocationToCitiesList()
        {
            AddToCitiesList(NearestLocation);
        }

        private void RightClickOnMap()
        {
            var mouse = FromGeoPoint(MapMouse);

            NearestLocation = locationsManager
                .Search(mouse, 30)
                .OrderBy(c => c.DistanceTo(mouse))
                .FirstOrDefault();
        }

        private void CitiesListTableGoToCoordinates(CrdsGeographical location)
        {
            MapZoomLevel = Math.Min(12, TileServer.MaxZoomLevel);
            MapCenter = new GeoPoint((float)(-location.Longitude), (float)location.Latitude);
            SelectedTabIndex = 0;
        }

        private void ShowLocationsFileFormat()
        {
            ViewManager.ShowMessageBox("$EclipseView.InfoMessageBox.Title", "$EclipseView.LocalCircumstances.FileFormat.Info");
        }

        private void ExportSarosSeriesTable()
        {
            var formats = new Dictionary<string, string>
            {
                [Text.Get("EclipseView.LocalCircumstances.OutputFormat.CsvWithFormatting")] = "*.csv",
                [Text.Get("EclipseView.LocalCircumstances.OutputFormat.CsvRawData")] = "*.csv",
            };
            string filter = string.Join("|", formats.Select(kv => $"{kv.Key}|{kv.Value}"));
            var file = ViewManager.ShowSaveFileDialog("$EclipseView.Export", $"SarosSeries{currentSarosSeries}", ".csv", filter, out int selectedFilterIndex);
            if (file != null)
            {
                var writer = new SarosSeriesTableCsvWriter(isRawData: selectedFilterIndex == 2);
                writer.Write(file, SarosSeriesTable);

                var answer = ViewManager.ShowMessageBox("$EclipseView.InfoMessageBox.Title", "$EclipseView.ExportDoneMessage", System.Windows.MessageBoxButton.YesNo);
                if (answer == System.Windows.MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start(file);
                }
            }
        }

        private void SarosSeriesTableSetDate(int lunationNumber)
        {
            meeusLunationNumber = lunationNumber - 1;
            CalculateEclipse(next: true, saros: false);
        }

        protected ImageAttributes GetImageAttributes()
        {
            // make image "red"
            if (settings.Get<ColorSchema>("Schema") == ColorSchema.Red)
            {
                float[][] matrix = {
                    new float[] {0.3f, 0, 0, 0, 0},
                    new float[] {0.3f, 0, 0, 0, 0},
                    new float[] {0.3f, 0, 0, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 0}
                };
                var colorMatrix = new ColorMatrix(matrix);
                var attr = new ImageAttributes();
                attr.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                return attr;
            }

            // make image lighten
            if (TileServer is OfflineTileServer)
            {
                float gamma = 1;
                float brightness = 1.2f;
                float contrast = 1;
                float alpha = 1;

                float adjustedBrightness = brightness - 1.0f;

                float[][] matrix ={
                    new float[] {contrast, 0, 0, 0, 0}, // scale red
                    new float[] {0, contrast, 0, 0, 0}, // scale green
                    new float[] {0, 0, contrast, 0, 0}, // scale blue
                    new float[] {0, 0, 0, alpha, 0},
                    new float[] {adjustedBrightness, adjustedBrightness, adjustedBrightness, 0, 1}};

                var attr = new ImageAttributes();
                attr.ClearColorMatrix();
                attr.SetColorMatrix(new ColorMatrix(matrix), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                attr.SetGamma(gamma, ColorAdjustType.Bitmap);

                return attr;
            }
            else
            {
                return null;
            }
        }

        protected GeoPoint ToGeo(CrdsGeographical g)
        {
            return new GeoPoint((float)-g.Longitude, (float)g.Latitude);
        }

        protected CrdsGeographical FromGeoPoint(GeoPoint p)
        {
            return new CrdsGeographical(-p.Longitude, p.Latitude);
        }

        protected virtual void SetMapColors()
        {
            citiesListMarkerStyle.MarkerBrush = IsDarkMode ? Brushes.Red : Brushes.Green;
            MapThumbnailBackColor = IsDarkMode ? Color.FromArgb(0xFF, 0x33, 0, 0) : Color.FromArgb(0xFF, 0x33, 0x33, 0x33);
            MapThumbnailForeColor = IsDarkMode ? Color.FromArgb(0xFF, 0x59, 0, 0) : Color.FromArgb(0xFF, 0x59, 0x59, 0x59);
        }

        protected abstract void CalculateEclipse(bool next, bool saros);
        protected abstract void CalculateSarosSeries();
        protected abstract void CalculateCitiesTable();
        protected abstract void CalculateLocalCircumstances(CrdsGeographical g);
        protected abstract void AddToCitiesList(CrdsGeographical location);
    }
}
