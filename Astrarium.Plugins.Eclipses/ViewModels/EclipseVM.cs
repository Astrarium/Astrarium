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
        public double JulianDay { get; protected set; }

        /// <summary>
        /// Date of the eclipse selected, converted to string
        /// </summary>
        public string EclipseDate { get; protected set; }

        /// <summary>
        /// Directory to store maps cache
        /// </summary>
        public string CacheFolder { get; protected set; }

        /// <summary>
        /// Flag indicating calculation is in progress
        /// </summary>
        public bool IsCalculating { get; protected set; }

        /// <summary>
        /// Collection of map tile servers to switch between them
        /// </summary>
        public ICollection<ITileServer> TileServers { get; protected set; }

        /// <summary>
        /// Collection of markers (points) on the map
        /// </summary>
        public ICollection<Marker> Markers { get; protected set; }

        /// <summary>
        /// Collection of tracks (lines) on the map
        /// </summary>
        public ICollection<Track> Tracks { get; protected set; }

        /// <summary>
        /// Collection of polygons (areas) on the map
        /// </summary>
        public ICollection<Polygon> Polygons { get; protected set; }

        public ICommand ChangeDateCommand => new Command(ChangeDate);
        public ICommand PrevEclipseCommand => new Command(PrevEclipse);
        public ICommand NextEclipseCommand => new Command(NextEclipse);
        public ICommand PrevSarosCommand => new Command(PrevSaros);
        public ICommand NextSarosCommand => new Command(NextSaros);
        //public ICommand LockOnCurrentPositionCommand => new Command(LockOnCurrentPosition);
        //public ICommand LockOnNearestLocationCommand => new Command(LockOnNearestLocation);
        //public ICommand AddCurrentPositionToCitiesListCommand => new Command(AddCurrentPositionToCitiesList);
        //public ICommand AddNearestLocationToCitiesListCommand => new Command(AddNearestLocationToCitiesList);
        //public ICommand RightClickOnMapCommand => new Command(RightClickOnMap);
        //public ICommand SarosSeriesTableSetDateCommand => new Command<double>(SarosSeriesTableSetDate);
        //public ICommand ExportSarosSeriesTableCommand => new Command(ExportSarosSeriesTable);
        //public ICommand ChartZoomInCommand => new Command(ChartZoomIn);
        //public ICommand ChartZoomOutCommand => new Command(ChartZoomOut);
        //public ICommand CitiesListTableGoToCoordinatesCommand => new Command<CrdsGeographical>(CitiesListTableGoToCoordinates);
        //public ICommand ShowLocationsFileFormatCommand => new Command(ShowLocationsFileFormat);
        //public ICommand ClearLocationsTableCommand => new Command(ClearLocationsTable);
        //public ICommand LoadLocationsFromFileCommand => new Command(LoadLocationsFromFile);
        //public ICommand FindLocationsOnTotalPathCommand => new Command(FindLocationsOnTotalPath);
        //public ICommand ExportLocationsTableCommand => new Command(ExportLocationsTable);

        protected EclipseVM(ISettings settings)
        {
            this.settings = settings;

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

        protected void AddLocationMarker()
        {
            Markers.Add(new Marker(ToGeo(observerLocation), observerLocationMarkerStyle, observerLocation.LocationName));
            Markers = new List<Marker>(Markers);
            NotifyPropertyChanged(nameof(Markers));
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

        protected abstract void CalculateLocalCircumstances(CrdsGeographical g);
    }
}
