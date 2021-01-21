using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace Astrarium.Plugins.Eclipses
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
        /// Date of the eclipse selected, converted to string
        /// </summary>
        public string EclipseDate { get; private set; }

        /// <summary>
        /// Eclipse description
        /// </summary>
        public string EclipseDescription { get; private set; }

        /// <summary>
        /// Table of local contacts instants, displayed to the right of eclipse map
        /// </summary>
        public ObservableCollection<LocalContactsTableItem> LocalContactsTable { get; private set; } = new ObservableCollection<LocalContactsTableItem>();

        /// <summary>
        /// Table of local circumstances, displayed to the right of eclipse map
        /// </summary>
        public ObservableCollection<NameValueTableItem> LocalCircumstancesTable { get; private set; } = new ObservableCollection<NameValueTableItem>();

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
                if (!IsMapDragging)
                {
                    CalculateLocalCircumstances(observerLocation);
                }
            } 
        }

        /// <summary>
        /// Flag indicating map is dragging
        /// </summary>
        public bool IsMapDragging
        {
            get => GetValue<bool>(nameof(IsMapDragging));
            set => SetValue(nameof(IsMapDragging), value);
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
        /// String representation of current geographical coordinates
        /// </summary>
        public string ObserverLocationCoordinates
        {
            get => GetValue<string>(nameof(ObserverLocationCoordinates));
            private set => SetValue(nameof(ObserverLocationCoordinates), value);
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

        public ICommand PrevEclipseCommand => new Command(PrevEclipse);
        public ICommand NextEclipseCommand => new Command(NextEclipse);
        public ICommand PrevSarosCommand => new Command(PrevSaros);
        public ICommand NextSarosCommand => new Command(NextSaros);
        public ICommand ClickOnMapCommand => new Command(ClickOnMap);
        public ICommand ClickOnLinkCommand => new Command<double>(ClickOnLink);

        public ICommand ChartZoomInCommand => new Command(ChartZoomIn);
        public ICommand ChartZoomOutCommand => new Command(ChartZoomOut);

        private int currentSarosSeries = 0;
        private int selectedTabIndex = 0;
        public int SelectedTabIndex
        {
            get => selectedTabIndex; 
            set
            {
                selectedTabIndex = value;
                CalculateSarosSeries();
            }
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

        private readonly ISky sky;
        private readonly IEclipsesCalculator eclipsesCalculator;
        private readonly ISettings settings;
        private CrdsGeographical observerLocation;
        private PolynomialBesselianElements be;
        private SolarEclipse eclipse;
        private static NumberFormatInfo nf;

        private static readonly IEphemFormatter fmtGeo = new Formatters.GeoCoordinatesFormatter();
        private static readonly IEphemFormatter fmtTime = new Formatters.TimeFormatter(withSeconds: true);
        private static readonly IEphemFormatter fmtAlt = new Formatters.SignedDoubleFormatter(1, "\u00B0");
        private static readonly IEphemFormatter fmtMag = new Formatters.UnsignedDoubleFormatter(3, "");
        private static readonly IEphemFormatter fmtRatio = new Formatters.UnsignedDoubleFormatter(4, "");
        private static readonly IEphemFormatter fmtPathWidth = new Formatters.UnsignedDoubleFormatter(0, "km");

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

        static SolarEclipseVM()
        {
            nf = new NumberFormatInfo();
            nf.NumberDecimalSeparator = ".";
            nf.NumberGroupSeparator = "\u2009";
        }

        public SolarEclipseVM(IEclipsesCalculator eclipsesCalculator, ISky sky, ISettings settings)
        {
            this.sky = sky;
            this.eclipsesCalculator = eclipsesCalculator;
            this.settings = settings;
            this.settings.PropertyChanged += Settings_PropertyChanged;
            observerLocation = settings.Get<CrdsGeographical>("ObserverLocation");

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

            string tileServerName = settings.Get<string>("EclipseMapTileServer");
            var tileServer = TileServers.FirstOrDefault(s => s.Name.Equals(tileServerName));            
            TileServer = tileServer ?? TileServers.First();

            JulianDay = sky.Context.JulianDay - LunarEphem.SINODIC_PERIOD;

            CalculateEclipse(next: true, saros: false);
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Schema")
            {
                IsDarkMode = settings.Get<ColorSchema>("Schema") == ColorSchema.Red; 
                TileImageAttributes = GetImageAttributes();
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

        public GeoPoint MapMouse
        {
            get => GetValue<GeoPoint>(nameof(MapMouse));
            set
            {
                SetValue(nameof(MapMouse), value);
                if (!IsMapDragging)
                {
                    CalculateLocalCircumstances(new CrdsGeographical(-value.Longitude, value.Latitude));
                }
            }
        }

        private void ClickOnMap()
        {
            var location = MapMouse;

            observerLocation = new CrdsGeographical(-location.Longitude, location.Latitude, 0, 0, "UTC+0", "Selected location");
            Markers.Remove(Markers.Last());
            AddLocationMarker();
        }

        private void ClickOnLink(double jd)
        {
            JulianDay = jd - LunarEphem.SINODIC_PERIOD / 2;
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

            eclipse = SolarEclipses.NearestEclipse(JulianDay + (next ? 1 : -1) * (saros ? LunarEphem.SAROS : LunarEphem.SINODIC_PERIOD), next);           
            JulianDay = eclipse.JulianDayMaximum;
            EclipseDate = Formatters.Date.Format(new Date(JulianDay, observerLocation.UtcOffset));
            be = eclipsesCalculator.GetBesselianElements(JulianDay);
            string type = eclipse.EclipseType.ToString();
            string subtype = eclipse.IsNonCentral ? " non-central" : "";
            EclipseDescription = $"{type}{subtype} solar eclipse";
            PrevSarosEnabled = SolarEclipses.NearestEclipse(JulianDay - LunarEphem.SAROS, next: false).Saros == eclipse.Saros;
            NextSarosEnabled = SolarEclipses.NearestEclipse(JulianDay + LunarEphem.SAROS, next: true).Saros == eclipse.Saros;

            NotifyPropertyChanged(
                nameof(EclipseDate), 
                nameof(EclipseDescription), 
                nameof(PrevSarosEnabled),
                nameof(NextSarosEnabled));

            await Task.Run(() =>
            {
                var map = SolarEclipses.EclipseMap(be);

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

                var maxCirc = SolarEclipses.LocalCircumstances(be, map.Max);

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
                    new NameValueTableItem("Maximal Duration", $"{fmtTime.Format(maxCirc.TotalDuration) }"),
                    new NameValueTableItem("ΔT", $"{be.DeltaT.ToString("N1", nf) } s")
                };
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    EclipseGeneralDetails = eclipseGeneralDetails;
                });

                var eclipseContacts = new ObservableCollection<ContactsTableItem>();
                eclipseContacts.Add(new ContactsTableItem("P1: First external contact", map.P1));
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
                eclipseContacts.Add(new ContactsTableItem("P4: Last external contact", map.P4));    
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
        }

        private void AddLocationMarker()
        {
            Markers.Add(new Marker(ToGeo(observerLocation), observerLocationMarkerStyle, observerLocation.LocationName));
            Markers = new List<Marker>(Markers);            
            NotifyPropertyChanged(nameof(Markers));
        }

        private void CalculateLocalCircumstances(CrdsGeographical pos)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                LocalContactsTable.Clear();
                LocalCircumstancesTable.Clear();
            });

            var local = SolarEclipses.LocalCircumstances(be, pos);
            //if (!local.IsInvisible)
            {
                var contacts = new List<LocalContactsTableItem>();
                contacts.Add(new LocalContactsTableItem("C1: Beginning of partial phase", local.PartialBegin));
                contacts.Add(new LocalContactsTableItem("C2: Beginning of total phase", local.TotalBegin));
                contacts.Add(new LocalContactsTableItem("Max: Local maximum", local.Maximum));
                contacts.Add(new LocalContactsTableItem("C3: End of total phase", local.TotalEnd));
                contacts.Add(new LocalContactsTableItem("C4: End of partial phase", local.PartialEnd));

                var details = new List<NameValueTableItem>();
                details.Add(new NameValueTableItem("Maximal magnitude", local.MaxMagnitude > 0 ? fmtMag.Format(local.MaxMagnitude) : ""));
                details.Add(new NameValueTableItem("Moon/Sun diameter ratio", local.MoonToSunDiameterRatio > 0 ? fmtRatio.Format(local.MoonToSunDiameterRatio) : ""));
                details.Add(new NameValueTableItem("Partial phase duration", !double.IsNaN(local.PartialDuration) && local.PartialDuration > 0 ? fmtTime.Format(local.PartialDuration) : ""));
                details.Add(new NameValueTableItem("Total phase duration", !double.IsNaN(local.TotalDuration) && local.TotalDuration > 0 ? fmtTime.Format(local.TotalDuration) : ""));

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    contacts.ToList().ForEach(i => LocalContactsTable.Add(i));
                    details.ToList().ForEach(i => LocalCircumstancesTable.Add(i));
                });
            }

            ObserverLocationName = IsMouseOverMap ? "Mouse coordinates" : observerLocation.LocationName;
            ObserverLocationCoordinates = IsMouseOverMap ? fmtGeo.Format(new CrdsGeographical(-MapMouse.Longitude, MapMouse.Latitude)) : fmtGeo.Format(observerLocation);
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
                    jd -= LunarEphem.SAROS;
                    var e = SolarEclipses.NearestEclipse(jd, false);
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
                    jd += LunarEphem.SAROS;
                    var e = SolarEclipses.NearestEclipse(jd, true);
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

                ObservableCollection<SarosSeriesTableItem> sarosSeriesTable = new ObservableCollection<SarosSeriesTableItem>();

                foreach (var e in eclipses)
                {
                    string type = e.EclipseType.ToString();
                    string subtype = e.IsNonCentral ? " non-central" : "";
                    var pbe = eclipsesCalculator.GetBesselianElements(e.JulianDayMaximum);
                    var local = SolarEclipses.LocalCircumstances(pbe, observerLocation);
                    sarosSeriesTable.Add(new SarosSeriesTableItem()
                    {
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

        private GeoPoint ToGeo(CrdsGeographical g)
        {
            return new GeoPoint((float)-g.Longitude, (float)g.Latitude);
        }

        public class SarosSeriesTableItem
        {
            public double JulianDay { get; set; }
            public string Date { get; set; }
            public string Type { get; set; }
            public string Gamma { get; set; }
            public string Magnitude { get; set; }
            public string LocalVisibility { get; set; }
        }

        public class NameValueTableItem
        {
            public string Name { get; set; }
            public string Value { get; set; }

            public NameValueTableItem(string name, string value)
            {
                Name = name;
                Value = value;
            }
        }

        public class ContactsTableItem
        {
            public string Point { get; set; }
            public string Coordinates { get; set; }
            public string Time { get; set; }

            public ContactsTableItem(string text, SolarEclipseMapPoint p)
            {
                Point = text;
                Coordinates = fmtGeo.Format(p);
                Time = $"{fmtTime.Format(new Date(p.JulianDay, 0))} UT";
            }
        }

        public class LocalContactsTableItem
        {
            public string Point { get; private set; }
            public string Time { get; private set; }
            public string Altitude { get; private set; }
            public string PAngle { get; private set; }
            public string ZAngle { get; private set; }

            public LocalContactsTableItem(string text, SolarEclipseLocalCircumstancesContactPoint contact)
            {
                Point = text;
                if (contact != null)
                {
                    Time = !double.IsNaN(contact.JulianDay) ? $"{fmtTime.Format(new Date(contact.JulianDay, 0))} UT" : "-";
                    Altitude = fmtAlt.Format(contact.SolarAltitude);
                    PAngle = fmtAlt.Format(contact.PAngle);
                    ZAngle = fmtAlt.Format(contact.ZAngle);
                }
            }
        }

        public class BesselianElementsTableItem
        {
            public string Index { get; set; }
            public string X { get; set; }
            public string Y { get; set; }
            public string D { get; set; }
            public string L1 { get; set; }
            public string L2 { get; set; }
            public string Mu { get; set; }

            public BesselianElementsTableItem(int index, PolynomialBesselianElements pbe)
            {
                Index = index.ToString();
                X = pbe.X[index].ToString("N6", nf);
                Y = pbe.Y[index].ToString("N6", nf);

                if (index <= 2)
                {
                    D = pbe.D[index].ToString("N6", nf);
                    L1 = pbe.L1[index].ToString("N6", nf);
                    L2 = pbe.L2[index].ToString("N6", nf);
                }

                if (index <= 1)
                    Mu = Angle.To360(pbe.Mu[index]).ToString("N6", nf);
            }
        }
    }
}
