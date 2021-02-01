using Astrarium.Algorithms;
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace Astrarium.Plugins.Eclipses.ViewModels
{
    public class SolarEclipseVM : ViewModelBase
    {
        /// <summary>
        /// Directory to store maps cache
        /// </summary>
        public string CacheFolder { get; private set; }

        /// <summary>
        /// Selected Julian date
        /// </summary>
        public double JulianDay { get; set; }

        /// <summary>
        /// Name of the observer location from settings
        /// </summary>
        public string SettingsLocationName { get; private set; }

        /// <summary>
        /// Date of the eclipse selected, converted to string
        /// </summary>
        public string EclipseDate { get; private set; }

        /// <summary>
        /// Eclipse description
        /// </summary>
        public string EclipseDescription { get; private set; }

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

        /// <summary>
        /// Location nearest to the current mouse position on the map
        /// </summary>
        public CrdsGeographical NearestLocation
        {
            get => GetValue<CrdsGeographical>(nameof(NearestLocation));
            private set => SetValue(nameof(NearestLocation), value);
        }

        /// <summary>
        /// Table of local contacts instants, displayed to the right of eclipse map
        /// </summary>
        public ObservableCollection<LocalContactsTableItem> LocalContactsTable { get; private set; } = new ObservableCollection<LocalContactsTableItem>();

        /// <summary>
        /// Table of local circumstances, displayed to the right of eclipse map
        /// </summary>
        public ObservableCollection<NameValueTableItem> LocalCircumstancesTable { get; private set; } = new ObservableCollection<NameValueTableItem>();

        /// <summary>
        /// Table of local circumstances for selected cities
        /// </summary>
        public ObservableCollection<CitiesListTableItem> CitiesListTable { get; private set; } = new ObservableCollection<CitiesListTableItem>();

        /// <summary>
        /// Local circumstance of the eclipse
        /// </summary>
        public SolarEclipseLocalCircumstances LocalCircumstances { get; private set; }

        /// <summary>
        /// Title of Saros series table
        /// </summary>
        public string SarosSeriesTableTitle { get; private set; }

        /// <summary>
        /// Saros series table
        /// </summary>
        public ObservableCollection<SarosSeriesTableItem> SarosSeriesTable { get; private set; } = new ObservableCollection<SarosSeriesTableItem>();

        /// <summary>
        /// General eclipse info table
        /// </summary>
        public ObservableCollection<NameValueTableItem> EclipseGeneralDetails { get; private set; } = new ObservableCollection<NameValueTableItem>();

        /// <summary>
        /// Eclipse contacts info table
        /// </summary>
        public ObservableCollection<ContactsTableItem> EclipseContacts { get; private set; } = new ObservableCollection<ContactsTableItem>();

        /// <summary>
        /// Besselian elements table
        /// </summary>
        public ObservableCollection<BesselianElementsTableItem> BesselianElementsTable { get; private set; } = new ObservableCollection<BesselianElementsTableItem>();

        /// <summary>
        /// Table header for Besselian elements table
        /// </summary>
        public string BesselianElementsTableHeader { get; private set; }

        /// <summary>
        /// Table footer for Besselian elements table
        /// </summary>
        public string BesselianElementsTableFooter { get; private set; }

        /// <summary>
        /// Flag indicating calculation is in progress
        /// </summary>
        public bool IsCalculating { get; private set; }

        /// <summary>
        /// Flag indicating previous saros button is enabled
        /// </summary>
        public bool PrevSarosEnabled { get; private set; }

        /// <summary>
        /// Flag indicating next saros button is enabled
        /// </summary>
        public bool NextSarosEnabled { get; private set; }

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

        public CitiesListOption FillCitiesOption
        {
            get => GetValue<CitiesListOption>(nameof(FillCitiesOption));
            set => SetValue(nameof(FillCitiesOption), value);
        }

        /// <summary>
        /// Flag indicating dark mode is used
        /// </summary>
        public bool IsDarkMode
        {
            get => GetValue<bool>(nameof(IsDarkMode));
            private set => SetValue(nameof(IsDarkMode), value);
        }

        /// <summary>
        /// Name of current observer location point
        /// </summary>
        public string ObserverLocationName
        {
            get => GetValue<string>(nameof(ObserverLocationName));
            private set => SetValue(nameof(ObserverLocationName), value);
        }
       
        /// <summary>
        /// String description of local visibility, like "Visible as partial", "Invisible" and etc.
        /// </summary>
        public string LocalVisibilityDescription
        {
            get => GetValue<string>(nameof(LocalVisibilityDescription));
            private set => SetValue(nameof(LocalVisibilityDescription), value); 
        }

        /// <summary>
        /// Flag indicating the eclipse is visible from current place
        /// </summary>
        public bool IsVisibleFromCurrentPlace
        {
            get => GetValue<bool>(nameof(IsVisibleFromCurrentPlace));
            private set => SetValue(nameof(IsVisibleFromCurrentPlace), value);
        }

        public float ChartZoomLevel
        {
            get => GetValue<float>(nameof(ChartZoomLevel));
            set => SetValue(nameof(ChartZoomLevel), value);
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
        public ICommand ChartZoomInCommand => new Command(ChartZoomIn);
        public ICommand ChartZoomOutCommand => new Command(ChartZoomOut);
        public ICommand FillCitiesTableCommand => new Command(FillCitiesTable);
        public ICommand CitiesListTableGoToCoordinatesCommand => new Command<CrdsGeographical>(CitiesListTableGoToCoordinates);
        public ICommand ExportCitiesTableCommand => new Command(ExportCitiesTable);
        
        public int SelectedTabIndex
        {
            get => GetValue<int>(nameof(SelectedTabIndex)); 
            set  
            {
                SetValue(nameof(SelectedTabIndex), value);
                CalculateSarosSeries();
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

        public string MapMouseString
        {
            get => GetValue<string>(nameof(MapMouseString));
            private set => SetValue(nameof(MapMouseString), value);
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

        public ImageAttributes TileImageAttributes
        {
            get => GetValue<ImageAttributes>(nameof(TileImageAttributes));
            set => SetValue(nameof(TileImageAttributes), value);
        }

        /// <summary>
        /// Collection of map tile servers to switch between them
        /// </summary>
        public ICollection<ITileServer> TileServers { get; private set; }

        /// <summary>
        /// Collection of markers (points) on the map
        /// </summary>
        public ICollection<Marker> Markers { get; private set; }

        /// <summary>
        /// Collection of tracks (lines) on the map
        /// </summary>
        public ICollection<Track> Tracks { get; private set; }

        /// <summary>
        /// Collection of polygons (areas) on the map
        /// </summary>
        public ICollection<Polygon> Polygons { get; private set; }

        private readonly CsvLocationsReader locationsReader;
        private readonly CitiesManager citiesManager;
        private readonly IEclipsesCalculator eclipsesCalculator;
        private readonly ISettings settings;
        private CrdsGeographical observerLocation;

        private int currentSarosSeries;
        private SolarEclipseMap map;
        private PolynomialBesselianElements be;
        private SolarEclipse eclipse;
        private static NumberFormatInfo nf;
       
        #region Map styles

        private readonly MarkerStyle riseSetMarkerStyle = new MarkerStyle(5, Brushes.Red, null, Brushes.Red, SystemFonts.DefaultFont, StringFormat.GenericDefault);
        private readonly MarkerStyle centralLineMarkerStyle = new MarkerStyle(5, Brushes.Black, null, Brushes.Black, SystemFonts.DefaultFont, StringFormat.GenericDefault);
        private readonly MarkerStyle maxPointMarkerStyle = new MarkerStyle(5, Brushes.Red, null, Brushes.Black, SystemFonts.DefaultFont, StringFormat.GenericDefault);
        private readonly TrackStyle riseSetTrackStyle = new TrackStyle(new Pen(Color.Red, 2));
        private readonly TrackStyle penumbraLimitTrackStyle = new TrackStyle(new Pen(Color.Orange, 2));
        private readonly TrackStyle umbraLimitTrackStyle = new TrackStyle(new Pen(Color.Gray, 2));
        private readonly TrackStyle centralLineTrackStyle = new TrackStyle(new Pen(Color.Black, 2));
        private readonly PolygonStyle umbraPolygonStyle = new PolygonStyle(new SolidBrush(Color.FromArgb(100, Color.Gray)));
        private readonly MarkerStyle observerLocationMarkerStyle = new MarkerStyle(5, Brushes.Black, null, Brushes.Black, SystemFonts.DefaultFont, StringFormat.GenericDefault);

        #endregion Map styles

        static SolarEclipseVM()
        {
            nf = new NumberFormatInfo();
            nf.NumberDecimalSeparator = ".";
            nf.NumberGroupSeparator = "\u2009";
        }

        public SolarEclipseVM(IEclipsesCalculator eclipsesCalculator, CitiesManager citiesManager, CsvLocationsReader locationsReader, ISky sky, ISettings settings)
        {
            this.eclipsesCalculator = eclipsesCalculator;
            this.citiesManager = citiesManager;
            this.locationsReader = locationsReader;
            this.settings = settings;
            this.settings.PropertyChanged += Settings_PropertyChanged;
            observerLocation = settings.Get<CrdsGeographical>("ObserverLocation");

            SettingsLocationName = $"Local visibility ({observerLocation.LocationName})";
            FillCitiesOption = CitiesListOption.FromFile;
            IsDarkMode = settings.Get<ColorSchema>("Schema") == ColorSchema.Red;
            ChartZoomLevel = 1;
            CacheFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Astrarium", "MapsCache");

            TileServers = new List<ITileServer>() 
            {
                new OfflineTileServer(),
                new OpenStreetMapTileServer("Astrarium v1.0 contact astrarium@astrarium.space"),
                new StamenTerrainTileServer(),
                new OpenTopoMapServer()
            };

            for (int i = 0; i < 5; i++) 
            {
                LocalContactsTable.Add(new LocalContactsTableItem(null, null));
                LocalCircumstancesTable.Add(new NameValueTableItem(null, null));
            }

            string tileServerName = settings.Get<string>("EclipseMapTileServer");
            var tileServer = TileServers.FirstOrDefault(s => s.Name.Equals(tileServerName));            
            TileServer = tileServer ?? TileServers.First();
            JulianDay = sky.Context.JulianDay - LunarEphem.SINODIC_PERIOD;

            CalculateEclipse(next: true, saros: false);
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Schema")
            {
                IsDarkMode = settings.Get<ColorSchema>("Schema") == ColorSchema.Red; 
                TileImageAttributes = GetImageAttributes();
            }
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
            Markers.Remove(Markers.Last());
            AddLocationMarker();
            IsMapLocked = true;
            CalculateLocalCircumstances(observerLocation);
        }

        private void AddCurrentPositionToCitiesList()
        {
            var g = FromGeoPoint(MapMouse);
            g.LocationName = "Locked Point";
            AddToCitiesList(g);
        }

        private void AddNearestLocationToCitiesList()
        {
            AddToCitiesList(NearestLocation);
        }

        private void AddToCitiesList(CrdsGeographical location)
        {
            var local = eclipsesCalculator.FindLocalCircumstancesForCities(be, new[] { location }).First();
            CitiesListTable.Add(new CitiesListTableItem(local, eclipsesCalculator.GetLocalVisibilityString(eclipse, local)));
        }

        private void RightClickOnMap()
        {
            var mouse = FromGeoPoint(MapMouse);
            NearestLocation = citiesManager
                .FindCities(mouse, 30)
                .OrderBy(c => c.DistanceTo(mouse))
                .FirstOrDefault();
        }

        private void CitiesListTableGoToCoordinates(CrdsGeographical location)
        {
            MapZoomLevel = Math.Min(12, TileServer.MaxZoomLevel);
            MapCenter = new GeoPoint((float)(-location.Longitude), (float)location.Latitude);
            SelectedTabIndex = 0;
        }

        private void SarosSeriesTableSetDate(double jd)
        {
            JulianDay = jd - LunarEphem.SINODIC_PERIOD;
            CalculateEclipse(next: true, saros: false);
        }

        private void ChartZoomIn()
        {
            ChartZoomLevel = Math.Min(3, ChartZoomLevel * 1.1f);
        }

        private void ChartZoomOut()
        {
            ChartZoomLevel = Math.Max(1.0f / 3f, ChartZoomLevel / 1.1f);
        }

        private ImageAttributes GetImageAttributes()
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

        private async void CalculateEclipse(bool next, bool saros)
        {
            IsCalculating = true;
            NotifyPropertyChanged(nameof(IsCalculating));

            eclipse =  eclipsesCalculator.GetNearestEclipse(JulianDay, next, saros);
            JulianDay = eclipse.JulianDayMaximum;
            EclipseDate = Formatters.Date.Format(new Date(JulianDay, 0));
            be = eclipsesCalculator.GetBesselianElements(JulianDay);
            string type = eclipse.EclipseType.ToString();
            string subtype = eclipse.IsNonCentral ? " non-central" : "";
            EclipseDescription = $"{type}{subtype} solar eclipse";
            PrevSarosEnabled = eclipsesCalculator.GetNearestEclipse(JulianDay, next: false, saros: true).Saros == eclipse.Saros;
            NextSarosEnabled = eclipsesCalculator.GetNearestEclipse(JulianDay, next: true, saros: true).Saros == eclipse.Saros;

            NotifyPropertyChanged(
                nameof(EclipseDate), 
                nameof(EclipseDescription), 
                nameof(PrevSarosEnabled),
                nameof(NextSarosEnabled),
                nameof(SettingsLocationName));

            await Task.Run(() =>
            {
                map = SolarEclipses.EclipseMap(be);

                var tracks = new List<Track>();
                var polygons = new List<Polygon>();
                var markers = new List<Marker>();

                if (map.P1 != null)
                {
                    markers.Add(new Marker(ToGeo(map.P1), riseSetMarkerStyle, "P1"));
                }
                if (map.P2 != null)
                {
                    markers.Add(new Marker(ToGeo(map.P2), riseSetMarkerStyle, "P2"));
                }
                if (map.P3 != null)
                {
                    markers.Add(new Marker(ToGeo(map.P3), riseSetMarkerStyle, "P3"));
                }
                if (map.P4 != null)
                {
                    markers.Add(new Marker(ToGeo(map.P4), riseSetMarkerStyle, "P4"));
                }
                if (map.U1 != null)
                {
                    markers.Add(new Marker(ToGeo(map.U1), centralLineMarkerStyle, "U1"));
                }
                if (map.U2 != null)
                {
                    markers.Add(new Marker(ToGeo(map.U2), centralLineMarkerStyle, "U2"));
                }

                for (int i = 0; i < 2; i++)
                {
                    if (map.UmbraNorthernLimit[i].Any())
                    {
                        var track = new Track(umbraLimitTrackStyle);
                        track.AddRange(map.UmbraNorthernLimit[i].Select(p => ToGeo(p)));
                        tracks.Add(track);
                    }

                    if (map.UmbraSouthernLimit[i].Any())
                    {
                        var track = new Track(umbraLimitTrackStyle);
                        track.AddRange(map.UmbraSouthernLimit[i].Select(p => ToGeo(p)));
                        tracks.Add(track);
                    }
                }

                if (map.TotalPath.Any())
                {
                    var track = new Track(centralLineTrackStyle);
                    track.AddRange(map.TotalPath.Select(p => ToGeo(p)));
                    tracks.Add(track);
                }

                // central line is divided into 2 ones => draw shadow path as 2 polygons

                if ((map.UmbraNorthernLimit[0].Any() && !map.UmbraNorthernLimit[1].Any()) ||
                    (map.UmbraSouthernLimit[0].Any() && !map.UmbraSouthernLimit[1].Any()))
                {
                    var polygon = new Polygon(umbraPolygonStyle);
                    polygon.AddRange(map.UmbraNorthernLimit[0].Select(p => ToGeo(p)));
                    polygon.AddRange(map.UmbraNorthernLimit[1].Select(p => ToGeo(p)));
                    if (map.U2 != null) polygon.Add(ToGeo(map.U2));
                    polygon.AddRange((map.UmbraSouthernLimit[1] as IEnumerable<CrdsGeographical>).Reverse().Select(p => ToGeo(p)));
                    polygon.AddRange((map.UmbraSouthernLimit[0] as IEnumerable<CrdsGeographical>).Reverse().Select(p => ToGeo(p)));
                    if (map.U1 != null) polygon.Add(ToGeo(map.U1));
                    polygons.Add(polygon);
                }
                else
                {
                    for (int i = 0; i < 2; i++)
                    {
                        if (map.UmbraNorthernLimit[i].Any() && map.UmbraSouthernLimit[i].Any())
                        {
                            var polygon = new Polygon(umbraPolygonStyle);
                            polygon.AddRange(map.UmbraNorthernLimit[i].Select(p => ToGeo(p)));
                            polygon.AddRange((map.UmbraSouthernLimit[i] as IEnumerable<CrdsGeographical>).Reverse().Select(p => ToGeo(p)));
                            polygons.Add(polygon);
                        }
                    }
                }

                foreach (var curve in map.RiseSetCurve)
                {
                    if (curve.Any())
                    {
                        var track = new Track(riseSetTrackStyle);
                        track.AddRange(curve.Select(p => ToGeo(p)));
                        track.Add(track.First());
                        tracks.Add(track);
                    }
                }

                if (map.PenumbraNorthernLimit.Any())
                {
                    var track = new Track(penumbraLimitTrackStyle);
                    track.AddRange(map.PenumbraNorthernLimit.Select(p => ToGeo(p)));
                    tracks.Add(track);
                }

                if (map.PenumbraSouthernLimit.Any())
                {
                    var track = new Track(penumbraLimitTrackStyle);
                    track.AddRange(map.PenumbraSouthernLimit.Select(p => ToGeo(p)));
                    tracks.Add(track);
                }

                if (map.Max != null)
                {
                    markers.Add(new Marker(ToGeo(map.Max), maxPointMarkerStyle, "Max"));
                }

                // Local curcumstances at point of maximum
                var maxCirc = SolarEclipses.LocalCircumstances(be, map.Max);

                // Brown lunation number
                var lunation = LunarEphem.Lunation(JulianDay);

                // TODO:
                // add Sun/Moon info for greatest eclipse:
                // https://eclipse.gsfc.nasa.gov/SEplot/SEplot2001/SE2022Apr30P.GIF

                var eclipseGeneralDetails = new ObservableCollection<NameValueTableItem>()
                {
                    new NameValueTableItem("Type", $"{type}{subtype}"),
                    new NameValueTableItem("Saros", $"{eclipse.Saros}"),
                    new NameValueTableItem("Date", $"{EclipseDate}"),
                    new NameValueTableItem("Magnitude", $"{eclipse.Magnitude.ToString("N5", nf)}"),
                    new NameValueTableItem("Gamma", $"{eclipse.Gamma.ToString("N5", nf)}"),
                    new NameValueTableItem("Maximal Duration", $"{Format.Time.Format(maxCirc.TotalDuration) }"),
                    new NameValueTableItem("Brown Lunation Number", $"{lunation}"),
                    new NameValueTableItem("ΔT", $"{be.DeltaT.ToString("N1", nf) } s")
                };
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    EclipseGeneralDetails = eclipseGeneralDetails;
                });

                var eclipseContacts = new ObservableCollection<ContactsTableItem>();

                if (!double.IsNaN(map.P1.JulianDay))
                {
                    eclipseContacts.Add(new ContactsTableItem("P1: First external contact", map.P1));
                }
                if (map.P2 != null)
                {
                    eclipseContacts.Add(new ContactsTableItem("P2: First internal contact", map.P2));
                }
                if (map.U1 != null && !double.IsNaN(map.U1.JulianDay))
                {
                    eclipseContacts.Add(new ContactsTableItem("U1: First umbra contact", map.U1));
                }
                eclipseContacts.Add(new ContactsTableItem("Max: Greatest Eclipse", map.Max));   
                if (map.U2 != null && !double.IsNaN(map.U2.JulianDay))
                {
                    eclipseContacts.Add(new ContactsTableItem("U2: Last umbra contact", map.U2));
                }
                if (map.P3 != null)
                {
                    eclipseContacts.Add(new ContactsTableItem("P3: Last internal contact", map.P3));
                }

                if (!double.IsNaN(map.P4.JulianDay))
                {
                    eclipseContacts.Add(new ContactsTableItem("P4: Last external contact", map.P4));
                }
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    EclipseContacts = eclipseContacts;
                });

                var besselianElementsTable = new ObservableCollection<BesselianElementsTableItem>();
                for (int i=0; i<4; i++)
                {
                    besselianElementsTable.Add(new BesselianElementsTableItem(i, be));
                }
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    BesselianElementsTable = besselianElementsTable;
                });

                // Besselian elements table header
                var beTableHeader = new StringBuilder();
                beTableHeader.AppendLine($"Elements for t\u2080 = {Formatters.DateTime.Format(new Date(be.JulianDay0))} TDT (JDE = { be.JulianDay0.ToString("N6", nf)})");
                beTableHeader.AppendLine($"The Besselian elements are valid over the period t\u2080 - 6h ≤ t\u2080 ≤ t\u2080 + 6h");
                BesselianElementsTableHeader = beTableHeader.ToString();

                // Besselian elements table footer
                var beTableFooter = new StringBuilder();
                beTableFooter.AppendLine($"Tan ƒ1 = {be.TanF1.ToString("N7", nf)}");
                beTableFooter.AppendLine($"Tan ƒ2 = {be.TanF2.ToString("N7", nf)}");
                BesselianElementsTableFooter = beTableFooter.ToString();

                Tracks = tracks;
                Polygons = polygons;
                Markers = markers;
                IsCalculating = false;

                AddLocationMarker();

                CalculateSarosSeries();

                CalculateLocalCircumstances(observerLocation);

                NotifyPropertyChanged(
                    nameof(IsCalculating),
                    nameof(Tracks),
                    nameof(Polygons),
                    nameof(Markers),
                    nameof(EclipseGeneralDetails),
                    nameof(EclipseContacts),
                    nameof(BesselianElementsTable),
                    nameof(BesselianElementsTableHeader),
                    nameof(BesselianElementsTableFooter)
                );
            });

            if (CitiesListTable.Any())
            {
                var cities = CitiesListTable.Select(l => l.Location).ToArray();
                var locals = await Task.Run(() => eclipsesCalculator.FindLocalCircumstancesForCities(be, cities));
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    CitiesListTable.Clear();
                    CitiesListTable = new ObservableCollection<CitiesListTableItem>(locals.Select(c => new CitiesListTableItem(c, eclipsesCalculator.GetLocalVisibilityString(eclipse, c))));                
                });
                NotifyPropertyChanged(nameof(CitiesListTable));
            }
        }

        private void AddLocationMarker()
        {
            Markers.Add(new Marker(ToGeo(observerLocation), observerLocationMarkerStyle, observerLocation.LocationName));
            Markers = new List<Marker>(Markers);            
            NotifyPropertyChanged(nameof(Markers));
        }

        private void CalculateLocalCircumstances(CrdsGeographical pos)
        {
            var local = SolarEclipses.LocalCircumstances(be, pos);

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var items = new List<LocalContactsTableItem>();
                LocalContactsTable[0] = new LocalContactsTableItem("C1: Beginning of partial phase", local.PartialBegin);
                LocalContactsTable[1] = new LocalContactsTableItem("C2: Beginning of total phase", local.TotalBegin);
                LocalContactsTable[2] = new LocalContactsTableItem("Max: Local maximum", local.Maximum);
                LocalContactsTable[3] = new LocalContactsTableItem("C3: End of total phase", local.TotalEnd);
                LocalContactsTable[4] = new LocalContactsTableItem("C4: End of partial phase", local.PartialEnd);

                LocalCircumstancesTable[0] = new NameValueTableItem("Maximal magnitude", local.MaxMagnitude > 0 ? Format.Mag.Format(local.MaxMagnitude) : "");
                LocalCircumstancesTable[1] = new NameValueTableItem("Moon/Sun diameter ratio", local.MoonToSunDiameterRatio > 0 ? Format.Ratio.Format(local.MoonToSunDiameterRatio) : "");
                LocalCircumstancesTable[2] = new NameValueTableItem("Partial phase duration", !double.IsNaN(local.PartialDuration) && local.PartialDuration > 0 ? Format.Time.Format(local.PartialDuration) : "");
                LocalCircumstancesTable[3] = new NameValueTableItem("Total phase duration", !double.IsNaN(local.TotalDuration) && local.TotalDuration > 0 ? Format.Time.Format(local.TotalDuration) : "");
                LocalCircumstancesTable[4] = new NameValueTableItem("Shadow path width", local.PathWidth > 0 ? Format.PathWidth.Format(local.PathWidth) : "");
            });

            ObserverLocationName = (IsMouseOverMap && !IsMapLocked) ? $"Mouse coordinates ({Format.Geo.Format(FromGeoPoint(MapMouse))})" : $"{observerLocation.LocationName} ({Format.Geo.Format(observerLocation)})";            
            LocalVisibilityDescription = eclipsesCalculator.GetLocalVisibilityString(eclipse, local);
            IsVisibleFromCurrentPlace = !local.IsInvisible;
            LocalCircumstances = local;
            NotifyPropertyChanged(nameof(LocalCircumstances));
        }

        private async void CalculateSarosSeries()
        {
            await Task.Run(() =>
            {
                if (SelectedTabIndex != 2 || currentSarosSeries == eclipse.Saros) return;

                IsCalculating = true;
                NotifyPropertyChanged(nameof(IsCalculating));

                double jd = JulianDay;
                List<SolarEclipse> eclipses = new List<SolarEclipse>();

                // add current eclipse
                eclipses.Add(eclipse);
                currentSarosSeries = eclipse.Saros;
                SarosSeriesTableTitle = $"List of eclipses of saros series {currentSarosSeries}";

                // add previous eclipses
                do
                {
                    var e = eclipsesCalculator.GetNearestEclipse(jd, next: false, saros: true);
                    jd = e.JulianDayMaximum;
                    if (e.Saros == eclipse.Saros)
                    {
                        eclipses.Insert(0, e);
                    }
                    else
                    {
                        break;
                    }
                }
                while (true);

                jd = JulianDay;
                // add next eclipses
                do
                {
                    var e = eclipsesCalculator.GetNearestEclipse(jd, next: true, saros: true);
                    jd = e.JulianDayMaximum;
                    if (e.Saros == eclipse.Saros)
                    {
                        eclipses.Add(e);
                    }
                    else
                    {
                        break;
                    }
                }
                while (true);

                var settingsLocation = settings.Get<CrdsGeographical>("ObserverLocation");
                ObservableCollection<SarosSeriesTableItem> sarosSeriesTable = new ObservableCollection<SarosSeriesTableItem>();

                int sarosMember = 0;
                foreach (var e in eclipses)
                {
                    string type = e.EclipseType.ToString();
                    string subtype = e.IsNonCentral ? " non-central" : "";
                    var pbe = eclipsesCalculator.GetBesselianElements(e.JulianDayMaximum);
                    var local = SolarEclipses.LocalCircumstances(pbe, settingsLocation);
                    sarosSeriesTable.Add(new SarosSeriesTableItem()
                    {
                        Member = $"{++sarosMember}",
                        JulianDay = e.JulianDayMaximum,
                        Date = Formatters.Date.Format(new Date(e.JulianDayMaximum, 0)),
                        Type = $"{type}{subtype}",
                        Gamma = e.Gamma.ToString("N5", nf),
                        Magnitude = e.Magnitude.ToString("N5", nf),
                        LocalVisibility = eclipsesCalculator.GetLocalVisibilityString(eclipse, local)
                    });
                }

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    SarosSeriesTable = sarosSeriesTable;
                });

                IsCalculating = false;
                NotifyPropertyChanged(
                    nameof(SarosSeriesTable), 
                    nameof(IsCalculating), 
                    nameof(SarosSeriesTableTitle)
                );
            });
        }

        private async void FillCitiesTable()
        {
            switch (FillCitiesOption)
            {
                case CitiesListOption.FromFile:
                    break;
                case CitiesListOption.TotalPath:
                    break;
            }

            var tokenSource = new CancellationTokenSource();
            var progress = new Progress<double>();
            ICollection<SolarEclipseLocalCircumstances> locals = null;

            try
            {
                if (FillCitiesOption == CitiesListOption.TotalPath)
                {
                    ViewManager.ShowProgress("Please wait", "Searching cities on central line of the eclipse...", tokenSource, progress);
                    locals = await Task.Run(() => eclipsesCalculator.FindCitiesOnCentralLine(be, map.TotalPath, tokenSource.Token, progress));
                }
                else if (FillCitiesOption == CitiesListOption.FromFile)
                {
                    string file = ViewManager.ShowOpenFileDialog("Open cities list", "Comma-separated files (*.csv)|*.csv");
                    if (file == null)
                        return;

                    ViewManager.ShowProgress("Please wait", "Calculating circumstances for locations...", tokenSource);
                    var cities = locationsReader.ReadFromFile(file);
                    locals = await Task.Run(() => eclipsesCalculator.FindLocalCircumstancesForCities(be, cities, tokenSource.Token, null));
                }
            }
            catch (Exception ex)
            {
                tokenSource.Cancel();
                ViewManager.ShowMessageBox("$Error", ex.Message);
            }

            if (!tokenSource.IsCancellationRequested && locals != null)
            {
                tokenSource.Cancel();
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    CitiesListTable.Clear();
                    CitiesListTable = new ObservableCollection<CitiesListTableItem>(locals.Select(c => new CitiesListTableItem(c, eclipsesCalculator.GetLocalVisibilityString(eclipse, c))));
                });
            }

            NotifyPropertyChanged(nameof(CitiesListTable));
        }

        private void ExportCitiesTable()
        {
            var formats = new Dictionary<string, string>
            {
                ["Comma-separated files (with formatting) (*.csv)"] = "*.csv",
                ["Comma-separated files (raw data) (*.csv)"] = "*.csv",
            };
            string filter = string.Join("|", formats.Select(kv => $"{kv.Key}|{kv.Value}"));
            var file = ViewManager.ShowSaveFileDialog("Export", "CitiesList", ".csv", filter, out int selectedFilterIndex);
            if (file != null)
            {
                CitiesTableCsvWriter writer = null;
                string ext = Path.GetExtension(file);
                switch (ext)
                {
                    case ".csv":
                        writer = new CitiesTableCsvWriter(file, selectedFilterIndex == 2);
                        break;
                    default:
                        break;
                }

                writer?.Write(CitiesListTable);

                //ViewManager.ShowMessageBox("$SolarEclipseWindow.ExportDoneTitle", "$SolarEclipseWindow.ExportDoneText", MessageBoxButton.OK);
                var answer = ViewManager.ShowMessageBox("Информация", "Экспорт в файл успешно завершён. Окрыть файл?", System.Windows.MessageBoxButton.YesNo);
                if (answer == System.Windows.MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start(file);
                }
            }
        }

        private GeoPoint ToGeo(CrdsGeographical g)
        {
            return new GeoPoint((float)-g.Longitude, (float)g.Latitude);
        }

        private CrdsGeographical FromGeoPoint(GeoPoint p)
        {
            return new CrdsGeographical(-p.Longitude, p.Latitude);
        }
    }

    public enum CitiesListOption
    {
        FromFile = 0,
        TotalPath = 1
    }
}
