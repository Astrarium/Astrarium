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
        /// Flag indicating calculation is in progress
        /// </summary>
        public bool IsCalculating { get; private set; }

        public ICommand PrevEclipseCommand => new Command(PrevEclipse);
        public ICommand NextEclipseCommand => new Command(NextEclipse);
        public ICommand ClickOnMapCommand => new Command(ClickOnMap);

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
        private PolynomialBesselianElements besselianElements;

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

            CalculateEclipse(next: true);
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
            CalculateEclipse(next: false);
        }

        private void NextEclipse()
        {
            CalculateEclipse(next: true);
        }

        public string Details { get; set; }

        public GeoPoint MapMouse
        {
            get => GetValue<GeoPoint>(nameof(MapMouse));
            set
            {
                SetValue(nameof(MapMouse), value);

                var pos = new CrdsGeographical(-value.Longitude, value.Latitude);
                var isTotal = SolarEclipses.LocalCircumstances(besselianElements, pos);

                Details = isTotal.ToString();

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

        private async void CalculateEclipse(bool next)
        {
            NumberFormatInfo nf = new NumberFormatInfo();
            nf.NumberDecimalSeparator = ".";
            nf.NumberGroupSeparator = "\u2009";

            SolarEclipse eclipse = SolarEclipses.NearestEclipse(JulianDay + (next ? 1 : -1) * LunarEphem.SINODIC_PERIOD, next);
            JulianDay = eclipse.JulianDayMaximum;
            EclipseDate = Formatters.Date.Format(new Date(JulianDay, observerLocation.UtcOffset));
            besselianElements = eclipsesCalculator.GetBesselianElements(JulianDay);
            string type = eclipse.EclipseType.ToString();
            string subtype = eclipse.IsNonCentral ? " non-central" : "";
            EclipseDescription = $"{type}{subtype} solar eclipse";
            EclipseDetails = 
$@"# Eclipse Details
||
|----------|
| Type | Total |
| Date | 12 Jun 2020 |
| Magnitude | 1.02353 |
| Gamma | -0.77878 |
| Maximal duration | 03m 58s |
| Delta T | 85.2 s |
| [link](astrarium://test.link) | link |
# Contacts
| Point | Coordinates | Time |
|-----|------|------|
| P1 (First external contact) | 44*12'54''N 22*34'12''W | 00:16:24 UT |
| P2 (First internal contact) | 44*12'54''N 22*34'12''W | 00:23:24 UT |
| C1 (First umbra contact) | 44*12'54''N 22*34'12''W | 00:23:24 UT |
| Max (Greatest Eclipse) | 44*12'54''N 22*34'12''W | 00:23:24 UT |
| C2 (Last umbra contact) | 44*12'54''N 22*34'12''W | 00:23:24 UT |
| P3 (Last internal contact) | 44*12'54''N 22*34'12''W | 00:23:24 UT |
| P4 (Last external contact) | 44*12'54''N 22*34'12''W | 00:23:24 UT |
# Besselian Elements
| n | x | y | d | l1 | l2 | μ |
|-----|-----|-----|-----|-----|-----|-----|
| 0 | {besselianElements.X[0].ToString("N6", nf)} |{besselianElements.Y[0].ToString("N6", nf)} | {besselianElements.D[0].ToString("N6", nf)} | {besselianElements.L1[0].ToString("N6", nf)} | {besselianElements.L2[0].ToString("N6", nf)} | {besselianElements.Mu[0].ToString("N6", nf)} |
| 1 | {besselianElements.X[1].ToString("N6", nf)} |{besselianElements.Y[1].ToString("N6", nf)} | {besselianElements.D[1].ToString("N6", nf)} | {besselianElements.L1[1].ToString("N6", nf)} | {besselianElements.L2[1].ToString("N6", nf)} | {besselianElements.Mu[1].ToString("N6", nf)} |
| 2 | {besselianElements.X[2].ToString("N6", nf)} |{besselianElements.Y[2].ToString("N6", nf)} | {besselianElements.D[2].ToString("N6", nf)} | {besselianElements.L1[2].ToString("N6", nf)} | {besselianElements.L2[2].ToString("N6", nf)} | |
| 3 | {besselianElements.X[3].ToString("N6", nf)} |{besselianElements.Y[3].ToString("N6", nf)} | | | | |
Tan ƒ1 = {besselianElements.TanF1.ToString("N7", nf)}  
Tan ƒ2 = {besselianElements.TanF2.ToString("N7", nf)}   
t0 = {Formatters.DateTime.Format(new Date(besselianElements.JulianDay0, 0))} UT (JDE = {besselianElements.JulianDay0.ToString("N6", nf)})
";

            IsCalculating = true;

            NotifyPropertyChanged(
                nameof(EclipseDate), 
                nameof(EclipseDescription), 
                nameof(EclipseDetails), 
                nameof(IsCalculating));

            await Task.Run(() =>
            {
                
                var map = SolarEclipses.EclipseMap(besselianElements, eclipse.EclipseType);


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
                
                Tracks = tracks;
                Polygons = polygons;
                Markers = markers;
                IsCalculating = false;

                AddLocationMarker();

                NotifyPropertyChanged(
                    nameof(IsCalculating),
                    nameof(Tracks),
                    nameof(Polygons),
                    nameof(Markers)
                );
            });
        }

        private void AddLocationMarker()
        {
            Markers.Add(new Marker(ToGeo(observerLocation), observerLocationMarkerStyle, observerLocation.LocationName));
            Markers = new List<Marker>(Markers);            
            NotifyPropertyChanged(nameof(Markers));
        }

        private GeoPoint ToGeo(CrdsGeographical g)
        {
            return new GeoPoint((float)-g.Longitude, (float)g.Latitude);
        }
    }
}
