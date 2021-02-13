using Astrarium.Algorithms;
using Astrarium.Plugins.Eclipses.Types;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        protected CsvLocationsReader locationsReader;
        protected IGeoLocationsManager locationsManager;
        protected IEclipsesCalculator eclipsesCalculator;
        protected ISettings settings;
        protected CrdsGeographical observerLocation;

        protected readonly MarkerStyle observerLocationMarkerStyle = new MarkerStyle(5, Brushes.Black, null, Brushes.Black, SystemFonts.DefaultFont, StringFormat.GenericDefault);

        #endregion Fields

        /// <summary>
        /// Selected Julian date
        /// </summary>
        public double JulianDay
        {
            get => GetValue<double>(nameof(JulianDay));
            protected set => SetValue(nameof(JulianDay), value);
        }

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

        public int SelectedTabIndex
        {
            get => GetValue<int>(nameof(SelectedTabIndex));
            set
            {
                SetValue(nameof(SelectedTabIndex), value);
                CalculateSarosSeries();
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
        public ICollection<Marker> Markers
        {
            get => GetValue<ICollection<Marker>>(nameof(Markers));
            protected set => SetValue(nameof(Markers), value);
        }

        /// <summary>
        /// Collection of tracks (lines) on the map
        /// </summary>
        public ICollection<Track> Tracks
        {
            get => GetValue<ICollection<Track>>(nameof(Tracks));
            protected set => SetValue(nameof(Tracks), value);
        }

        /// <summary>
        /// Collection of polygons (areas) on the map
        /// </summary>
        public ICollection<Polygon> Polygons
        {
            get => GetValue<ICollection<Polygon>>(nameof(Polygons));
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
        public ICommand SarosSeriesTableSetDateCommand => new Command<double>(SarosSeriesTableSetDate);
        public ICommand ExportSarosSeriesTableCommand => new Command(ExportSarosSeriesTable);
        public ICommand CitiesListTableGoToCoordinatesCommand => new Command<CrdsGeographical>(CitiesListTableGoToCoordinates);
        public ICommand ShowLocationsFileFormatCommand => new Command(ShowLocationsFileFormat);
        public ICommand ChartZoomInCommand => new Command(ChartZoomIn);
        public ICommand ChartZoomOutCommand => new Command(ChartZoomOut);

        protected EclipseVM(IGeoLocationsManager locationsManager, ISettings settings)
        {
            this.settings = settings;
            this.observerLocation = settings.Get<CrdsGeographical>("ObserverLocation");
            this.locationsManager = locationsManager;

            this.locationsManager.Load();

            ChartZoomLevel = 1;
            SettingsLocationName = $"Local visibility ({observerLocation.LocationName})";
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
        }

        /// <summary>
        /// Eclipse description
        /// </summary>
        public string EclipseDescription
        {
            get => GetValue<string>(nameof(EclipseDescription));
            protected set => SetValue(nameof(EclipseDescription), value);
        }

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

        public ITileServer TileServer
        {
            get => GetValue<ITileServer>(nameof(TileServer));
            set
            {
                SetValue(nameof(TileServer), value);
                TileImageAttributes = GetImageAttributes();
                if (settings.Get<string>("EclipseMapTileServer") != value.Name)
                {
                    settings.Set("EclipseMapTileServer", value.Name);
                    settings.Save();
                }
            }
        }

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
                    Markers.Remove(Markers.Last());
                    AddLocationMarker();
                }
            }
        }

        public GeoPoint MapCenter
        {
            get => GetValue(nameof(MapCenter), new GeoPoint());
            set => SetValue(nameof(MapCenter), value);
        }

        public int MapZoomLevel
        {
            get => GetValue(nameof(MapZoomLevel), 1);
            set => SetValue(nameof(MapZoomLevel), value);
        }

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

        public Color MapThumbnailBackColor
        {
            get => GetValue<Color>(nameof(MapThumbnailBackColor));
            protected set => SetValue(nameof(MapThumbnailBackColor), value);
        }

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

        public float ChartZoomLevel
        {
            get => GetValue<float>(nameof(ChartZoomLevel));
            set => SetValue(nameof(ChartZoomLevel), value);
        }

        protected void AddLocationMarker()
        {
            Markers.Add(new Marker(ToGeo(observerLocation), observerLocationMarkerStyle, observerLocation.LocationName));
            Markers = new List<Marker>(Markers);
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
            var jd = ViewManager.ShowDateDialog(JulianDay, 0, DateOptions.MonthYear);
            if (jd != null)
            {
                JulianDay = jd.Value - LunarEphem.SINODIC_PERIOD;
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
            g.LocationName = "Locked Point";
            LockOn(g);
        }

        private void LockOnNearestLocation()
        {
            LockOn(NearestLocation);
        }

        private void LockOn(CrdsGeographical location)
        {
            observerLocation = location;
            Markers.Remove(Markers.LastOrDefault());
            AddLocationMarker();
            IsMapLocked = true;
            CalculateLocalCircumstances(observerLocation);
        }

        private void AddCurrentPositionToCitiesList()
        {
            var g = FromGeoPoint(MapMouse);
            g.LocationName = "<No name>";
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
            ViewManager.ShowMessageBox("Information", "CSV file should contain following columns:\n\n- Location name (string)\n- Latitude in decimal degrees (float, in range -90...90, positive north, negative south)\n- longitude in decimal degrees (float, in range -180...180, positive east, negative west)\n- UTC offset in hours (float, optional)\n");
        }        

        private void SarosSeriesTableSetDate(double jd)
        {
            JulianDay = jd - LunarEphem.SINODIC_PERIOD;
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

        protected abstract void CalculateEclipse(bool next, bool saros);
        protected abstract void CalculateSarosSeries();
        protected abstract void CalculateLocalCircumstances(CrdsGeographical g);
        protected abstract void AddToCitiesList(CrdsGeographical location);
        protected abstract void ExportSarosSeriesTable();
    }
}
