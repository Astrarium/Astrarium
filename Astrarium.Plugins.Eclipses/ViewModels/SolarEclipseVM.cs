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
        /// Eclipse details (general information) in markdown format.
        /// </summary>
        public string EclipseDetails { get; private set; }

        /// <summary>
        /// Saros series table in markdown format
        /// </summary>
        public string SarosSeries { get; private set; }

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

        public ICommand PrevEclipseCommand => new Command(PrevEclipse);
        public ICommand NextEclipseCommand => new Command(NextEclipse);
        public ICommand PrevSarosCommand => new Command(PrevSaros);
        public ICommand NextSarosCommand => new Command(NextSaros);
        public ICommand ClickOnMapCommand => new Command(ClickOnMap);

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
        private NumberFormatInfo nf;

        private readonly IEphemFormatter fmtGeo = new Formatters.GeoCoordinatesFormatter();
        private readonly IEphemFormatter fmtTime = new Formatters.TimeFormatter(withSeconds: true);

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

        public SolarEclipseVM(IEclipsesCalculator eclipsesCalculator, ISky sky, ISettings settings)
        {
            this.sky = sky;
            this.eclipsesCalculator = eclipsesCalculator;
            this.settings = settings;
            this.settings.PropertyChanged += Settings_PropertyChanged;
            observerLocation = settings.Get<CrdsGeographical>("ObserverLocation");
            nf = new NumberFormatInfo();
            nf.NumberDecimalSeparator = ".";
            nf.NumberGroupSeparator = "\u2009";
            
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

        public string Details { get; set; }

        public GeoPoint MapMouse
        {
            get => GetValue<GeoPoint>(nameof(MapMouse));
            set
            {
                SetValue(nameof(MapMouse), value);

                var pos = new CrdsGeographical(-value.Longitude, value.Latitude);
                var local = SolarEclipses.LocalCircumstances(be, pos);

                Details = local.ToString();

                NotifyPropertyChanged(nameof(Details));
            }
        }

        private void ClickOnMap()
        {
            var location = MapMouse;

            observerLocation = new CrdsGeographical(-location.Longitude, location.Latitude, 0, 0, "UTC+0", "Selected location");
            Markers.Remove(Markers.Last());
            AddLocationMarker();
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

            SolarEclipse eclipse = SolarEclipses.NearestEclipse(JulianDay + (next ? 1 : -1) * (saros ? LunarEphem.SAROS : LunarEphem.SINODIC_PERIOD), next);
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
                if (map.C1 != null)
                {
                    markers.Add(new Marker(ToGeo(map.C1), centralLineMarkerStyle, "C1"));
                }
                if (map.C2 != null)
                {
                    markers.Add(new Marker(ToGeo(map.C2), centralLineMarkerStyle, "C2"));
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
                    if (map.C2 != null) polygon.Add(ToGeo(map.C2));
                    polygon.AddRange((map.UmbraSouthernLimit[1] as IEnumerable<CrdsGeographical>).Reverse().Select(p => ToGeo(p)));
                    polygon.AddRange((map.UmbraSouthernLimit[0] as IEnumerable<CrdsGeographical>).Reverse().Select(p => ToGeo(p)));
                    if (map.C1 != null) polygon.Add(ToGeo(map.C1));
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

                var eclipseDetails = new StringBuilder();
                eclipseDetails
                    .AppendLine($"# {"Eclipse Details"}")
                    .AppendLine($"||")
                    .AppendLine($"|-----|")
                    .AppendLine($"| {"Type"} | {type}{subtype} |")
                    .AppendLine($"| {"Saros"} | {eclipse.Saros} |")
                    .AppendLine($"| {"Date"} | {EclipseDate} |")
                    .AppendLine($"| {"Magnitude"} | { eclipse.Magnitude.ToString("N5", nf)} |")
                    .AppendLine($"| {"Gamma"} | { eclipse.Gamma.ToString("N5", nf)} |")
                    .AppendLine($"| {"Maximal Duration"} | { fmtTime.Format(maxCirc.TotalDuration) } |")
                    .AppendLine($"| {"ΔT"} | { be.DeltaT.ToString("N1", nf) } s |")
                    .AppendLine($"# {"Contacts"}")
                    .AppendLine($"| {"Point"} | {"Coordinates"} | {"Time"} |")
                    .AppendLine("|-----|-----|-----|")
                    .AppendLine($"| {"P1 (First external contact)"} | {fmtGeo.Format(map.P1)} | {fmtTime.Format(new Date(map.P1.JulianDay, 0))} UT |");

                if (map.P2 != null)
                {
                    eclipseDetails.AppendLine($"| {"P2 (First internal contact)"} | {fmtGeo.Format(map.P2)} | {fmtTime.Format(new Date(map.P2.JulianDay, 0))} UT |");
                }
                if (map.C1 != null && !double.IsNaN(map.C1.JulianDay))
                {
                    eclipseDetails.AppendLine($"| {"C1 (First umbra contact)"} | {fmtGeo.Format(map.C1)} | {fmtTime.Format(new Date(map.C1.JulianDay, 0))} UT |");
                }

                eclipseDetails.AppendLine($"| {"Max (Greatest Eclipse)"} | {fmtGeo.Format(map.Max)} | {fmtTime.Format(new Date(map.Max.JulianDay, 0))} UT |");

                if (map.C2 != null && !double.IsNaN(map.C2.JulianDay))
                {
                    eclipseDetails.AppendLine($"| {"C2 (Last umbra contact)"} | {fmtGeo.Format(map.C2)} | {fmtTime.Format(new Date(map.C2.JulianDay, 0))} UT |");
                }
                if (map.P3 != null)
                {
                    eclipseDetails.AppendLine($"| {"P3 (Last internal contact)"} | {fmtGeo.Format(map.P3)} | {fmtTime.Format(new Date(map.P3.JulianDay, 0))} UT |");
                }
                eclipseDetails.AppendLine($"| {"P4 (Last external contact)"} | {fmtGeo.Format(map.P4)} | {fmtTime.Format(new Date(map.P4.JulianDay, 0))} UT |");

                eclipseDetails
                    .AppendLine($"# {"Besselian Elements"}")
                    .AppendLine($"| n | x | y | d | l1 | l2 | μ |")
                    .AppendLine("|-----|-----|-----|-----|-----|-----|-----|")
                    .AppendLine($"| 0 | {be.X[0].ToString("N6", nf)} |{be.Y[0].ToString("N6", nf)} | {be.D[0].ToString("N6", nf)} | {be.L1[0].ToString("N6", nf)} | {be.L2[0].ToString("N6", nf)} | {be.Mu[0].ToString("N6", nf)} |")
                    .AppendLine($"| 1 | {be.X[1].ToString("N6", nf)} |{be.Y[1].ToString("N6", nf)} | {be.D[1].ToString("N6", nf)} | {be.L1[1].ToString("N6", nf)} | {be.L2[1].ToString("N6", nf)} | {be.Mu[1].ToString("N6", nf)} |")
                    .AppendLine($"| 2 | {be.X[2].ToString("N6", nf)} |{be.Y[2].ToString("N6", nf)} | {be.D[2].ToString("N6", nf)} | {be.L1[2].ToString("N6", nf)} | {be.L2[2].ToString("N6", nf)} | |")
                    .AppendLine($"| 3 | {be.X[3].ToString("N6", nf)} |{be.Y[3].ToString("N6", nf)} | | | | |")
                    .AppendLine()
                    .AppendLine($"Tan ƒ1 = {be.TanF1.ToString("N7", nf)}  ")
                    .AppendLine($"Tan ƒ2 = {be.TanF2.ToString("N7", nf)}  ")
                    .AppendLine($"t\u2080 = {Formatters.DateTime.Format(new Date(be.JulianDay0, 0))} UT (JDE = { be.JulianDay0.ToString("N6", nf)}");

                Tracks = tracks;
                Polygons = polygons;
                Markers = markers;
                IsCalculating = false;
                EclipseDetails = eclipseDetails.ToString();

                AddLocationMarker();

                CalculateSarosSeries();

                NotifyPropertyChanged(
                    nameof(IsCalculating),
                    nameof(Tracks),
                    nameof(Polygons),
                    nameof(Markers),
                    nameof(EclipseDetails)
                );
            });
        }

        private void AddLocationMarker()
        {
            Markers.Add(new Marker(ToGeo(observerLocation), observerLocationMarkerStyle, observerLocation.LocationName));
            Markers = new List<Marker>(Markers);            
            NotifyPropertyChanged(nameof(Markers));
        }

        private void CalculateSarosSeries()
        {
            if (SelectedTabIndex != 2) return;

            double jd = JulianDay;
            List<SolarEclipse> eclipses = new List<SolarEclipse>();
            
            // add current eclipse
            var eclipse = SolarEclipses.NearestEclipse(jd, true);
            eclipses.Add(eclipse);
            int saros = eclipse.Saros;
            
            // add previous eclipses
            do
            {
                jd -= LunarEphem.SAROS;
                eclipse = SolarEclipses.NearestEclipse(jd, false);
                if (eclipse.Saros == saros)
                {
                    eclipses.Insert(0, eclipse);
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
                eclipse = SolarEclipses.NearestEclipse(jd, true);
                if (eclipse.Saros == saros)
                {
                    eclipses.Add(eclipse);
                }
                else
                {
                    break;
                }
            }
            while (true);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"# List of solar eclipses of saros {saros}");
            sb.AppendLine($"| Date | Type | Gamma | Mag |");
            sb.AppendLine($"|-----|-----|-----|-----|");
            foreach (var e in eclipses)
            {
                string type = e.EclipseType.ToString();
                string subtype = e.IsNonCentral ? " non-central" : "";
                sb.Append("| ");
                sb.Append($" {Formatters.Date.Format(new Date(e.JulianDayMaximum, 0))} |");
                sb.Append($" {type}{subtype} |");
                sb.Append($" {e.Gamma.ToString("N5", nf)} |");
                sb.Append($" {e.Magnitude.ToString("N5", nf)} |");
                sb.AppendLine();
            }

            SarosSeries = sb.ToString();
            NotifyPropertyChanged(nameof(SarosSeries));
        }

        private GeoPoint ToGeo(CrdsGeographical g)
        {
            return new GeoPoint((float)-g.Longitude, (float)g.Latitude);
        }
    }
}
