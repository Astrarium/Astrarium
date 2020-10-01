using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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
        private readonly ISettings settings;
        private readonly CrdsGeographical observerLocation;
        private PolynomialBesselianElements besselianElements;
        private readonly CelestialObject sun;
        private readonly CelestialObject moon;

        #region Map styles

        private readonly MarkerStyle riseSetMarkerStyle = new MarkerStyle(5, Brushes.Red, null, Brushes.Red, SystemFonts.DefaultFont, StringFormat.GenericDefault);
        private readonly MarkerStyle centralLineMarkerStyle = new MarkerStyle(5, Brushes.Black, null, Brushes.Black, SystemFonts.DefaultFont, StringFormat.GenericDefault);
        private readonly MarkerStyle maxPointMarkerStyle = new MarkerStyle(5, Brushes.Red, null, Brushes.Black, SystemFonts.DefaultFont, StringFormat.GenericDefault);
        private readonly TrackStyle riseSetTrackStyle = new TrackStyle(new Pen(Color.Red, 2));
        private readonly TrackStyle penumbraLimitTrackStyle = new TrackStyle(new Pen(Color.Orange, 2));
        private readonly TrackStyle umbraLimitTrackStyle = new TrackStyle(new Pen(Color.Gray, 2));
        private readonly TrackStyle centralLineTrackStyle = new TrackStyle(new Pen(Color.Black, 2));
        private readonly PolygonStyle umbraPolygonStyle = new PolygonStyle(new SolidBrush(Color.FromArgb(100, Color.Gray)));

        #endregion Map styles

        public ITileServer TileServer
        {
            get => GetValue<ITileServer>(nameof(TileServer));
            set  
            { 
                SetValue(nameof(TileServer), value);
                TileImageAttributes = GetImageAttributes();
            }
        }

        public ImageAttributes TileImageAttributes
        {
            get => GetValue<ImageAttributes>(nameof(TileImageAttributes));
            set => SetValue(nameof(TileImageAttributes), value);
        }

        public SolarEclipseVM(ISky sky, ISettings settings)
        {
            this.sky = sky;
            this.settings = settings;
            this.settings.PropertyChanged += Settings_PropertyChanged;
            observerLocation = settings.Get<CrdsGeographical>("ObserverLocation");
            sun = sky.Search("@sun", b => true).FirstOrDefault();
            moon = sky.Search("@moon", b => true).FirstOrDefault();

            CacheFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Astrarium", "MapsCache");

            TileServers = new List<ITileServer>() 
            {
                new OfflineTileServer(),
                new OpenStreetMapTileServer("Astrarium v1.0 contact astrarium@astrarium.space"),
                new StamenTerrainTileServer(),
                new OpenTopoMapServer()
            };

            TileServer = TileServers.First();

            JulianDay = sky.Context.JulianDay;

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

        public string IsTotal { get; set; }

        public GeoPoint MapMouse
        {
            get => GetValue<GeoPoint>(nameof(MapMouse));
            set
            {
                SetValue(nameof(MapMouse), value);

                var pos = new CrdsGeographical(-value.Longitude, value.Latitude);
                var localMax = SolarEclipses.FindLocalMax(besselianElements, pos);
                var isTotal = SolarEclipses.Obscuration(besselianElements, pos, localMax);

                IsTotal = isTotal.ToString();
                NotifyPropertyChanged(nameof(IsTotal));
            }
        }

        private void ClickOnMap()
        {
            

            ;
            //MessageBox.Show(Formatters.DateTime.Format(date));
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

        private void CalculateEclipse(bool next)
        {
            if (sun == null || moon == null)
            {
                ViewManager.ShowMessageBox("Error", "Failed to calculate eclipse details: unable to determine Sun and/or Moon positions. Probably SolarSystem plugin is missed.");
                return;
            }

            // TODO: move to EclipsesCalculator

            SolarEclipse eclipse = SolarEclipses.NearestEclipse(JulianDay + (next ? 30 : -30), next);
            JulianDay = eclipse.JulianDayMaximum;

            // 5 measurements with 3h step, so interval is -6...+6 hours
            SunMoonPosition[] pos = new SunMoonPosition[5];

            double dt = TimeSpan.FromHours(6).TotalDays;
            double step = TimeSpan.FromHours(3).TotalDays;
            string[] ephemerides = new[] { "Equatorial0.Alpha", "Equatorial0.Delta", "Distance" };

            var sunEphem = sky.GetEphemerides(sun, JulianDay - dt, JulianDay + dt + 1e-6, step, ephemerides);
            var moonEphem = sky.GetEphemerides(moon, JulianDay - dt, JulianDay + dt + 1e-6, step, ephemerides);

            for (int i = 0; i < 5; i++)
            {
                pos[i] = new SunMoonPosition()
                {
                    JulianDay = JulianDay + step * (i - 2),
                    Sun = new CrdsEquatorial(sunEphem[i].GetValue<double>("Equatorial0.Alpha"), sunEphem[i].GetValue<double>("Equatorial0.Delta")),
                    Moon = new CrdsEquatorial(moonEphem[i].GetValue<double>("Equatorial0.Alpha"), moonEphem[i].GetValue<double>("Equatorial0.Delta")),
                    DistanceSun = sunEphem[i].GetValue<double>("Distance") * 149597870 / 6371.0,
                    DistanceMoon = moonEphem[i].GetValue<double>("Distance") / 6371.0
                };
            }

            besselianElements = SolarEclipses.FindPolynomialBesselianElements(pos);

            var map = SolarEclipses.GetEclipseMap(besselianElements);

            EclipseDate = Formatters.Date.Format(new Date(JulianDay, observerLocation.UtcOffset));

            var tracks = new List<Track>();
            var polygons = new List<Polygon>();
            var markers = new List<Marker>();

            if (map.P1 != null)
            {
                markers.Add(new Marker(ToGeo(map.P1.Coordinates), riseSetMarkerStyle, "P1"));
            }
            if (map.P2 != null)
            {
                markers.Add(new Marker(ToGeo(map.P2.Coordinates), riseSetMarkerStyle, "P2"));
            }
            if (map.P3 != null)
            {
                markers.Add(new Marker(ToGeo(map.P3.Coordinates), riseSetMarkerStyle, "P3"));
            }
            if (map.P4 != null)
            {
                markers.Add(new Marker(ToGeo(map.P4.Coordinates), riseSetMarkerStyle, "P4"));
            }

            if (map.C1 != null)
            {
                markers.Add(new Marker(ToGeo(map.C1.Coordinates), centralLineMarkerStyle, "C1"));
            }
            if (map.C2 != null)
            {
                markers.Add(new Marker(ToGeo(map.C2.Coordinates), centralLineMarkerStyle, "C2"));
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

            for (int i = 0; i < 2; i++)
            {
                if (map.TotalPath[i].Any())
                {
                    var track = new Track(centralLineTrackStyle);
                    if (i == 0 && map.C1 != null)
                    {
                        track.Add(ToGeo(map.C1.Coordinates));
                    }
                    track.AddRange(map.TotalPath[i].Select(p => ToGeo(p)));                    
                    if (map.C2 != null && ((i == 0 && !map.TotalPath[1].Any()) || i == 1))
                    {
                        track.Add(ToGeo(map.C2.Coordinates));
                    }                    
                    tracks.Add(track);
                }
            }

            // central line is divided into 2 ones => draw shadow path as 2 polygons
    
            if ((map.UmbraNorthernLimit[0].Any() && !map.UmbraNorthernLimit[1].Any()) ||
                (map.UmbraSouthernLimit[0].Any() && !map.UmbraSouthernLimit[1].Any()))
            {
                var polygon = new Polygon(umbraPolygonStyle);
                polygon.AddRange(map.UmbraNorthernLimit[0].Select(p => ToGeo(p)));
                polygon.AddRange(map.UmbraNorthernLimit[1].Select(p => ToGeo(p)));
                if (map.C2 != null) polygon.Add(ToGeo(map.C2.Coordinates));
                polygon.AddRange((map.UmbraSouthernLimit[1] as IEnumerable<CrdsGeographical>).Reverse().Select(p => ToGeo(p)));
                polygon.AddRange((map.UmbraSouthernLimit[0] as IEnumerable<CrdsGeographical>).Reverse().Select(p => ToGeo(p)));
                if (map.C1 != null) polygon.Add(ToGeo(map.C1.Coordinates));
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
                markers.Add(new Marker(ToGeo(map.Max.Coordinates), maxPointMarkerStyle, "Max"));
            }

            Tracks = tracks;
            Polygons = polygons;
            Markers = markers;



            NotifyPropertyChanged(nameof(EclipseDate), nameof(Tracks), nameof(Polygons), nameof(Markers));
        }

        private GeoPoint ToGeo(CrdsGeographical g)
        {
            return new GeoPoint((float)-g.Longitude, (float)g.Latitude);
        }
    }
}
