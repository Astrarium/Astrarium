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

namespace Astrarium.Plugins.SolarSystem
{
    public class SolarEclipseVM : ViewModelBase
    {
        public string CacheFolder { get; private set; }

        public double JulianDay { get; set; }

        public string EclipseDate { get; private set; }

        public ICommand PrevEclipseCommand => new Command(PrevEclipse);
        public ICommand NextEclipseCommand => new Command(NextEclipse);
        public ICommand ClickOnMapCommand => new Command(ClickOnMap);

        public ICollection<ITileServer> TileServers { get; private set; }

        public ICollection<Marker> Markers { get; private set; }
        public ICollection<Track> Tracks { get; private set; }
        public ICollection<Polygon> Polygons { get; private set; }

        private readonly SolarCalc solarCalc;
        private readonly LunarCalc lunarCalc;
        private readonly CrdsGeographical observerLocation;
        private readonly ISettings settings;

        private PolynomialBesselianElements besselianElements;

        private readonly MarkerStyle riseSetMarkerStyle = new MarkerStyle(5, Brushes.Red, null, Brushes.Red, SystemFonts.DefaultFont, StringFormat.GenericDefault);
        private readonly MarkerStyle centralLineMarkerStyle = new MarkerStyle(5, Brushes.Black, null, Brushes.Black, SystemFonts.DefaultFont, StringFormat.GenericDefault);

        private readonly TrackStyle riseSetTrackStyle = new TrackStyle(new Pen(Color.Red, 2));
        private readonly TrackStyle penumbraLimitTrackStyle = new TrackStyle(new Pen(Color.Orange, 2));
        private readonly TrackStyle umbraLimitTrackStyle = new TrackStyle(new Pen(Color.Gray, 2));
        private readonly TrackStyle centralLineTrackStyle = new TrackStyle(new Pen(Color.Black, 2));
        private readonly PolygonStyle umbraPolygonStyle = new PolygonStyle(new SolidBrush(Color.FromArgb(100, Color.Gray)));

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

        public SolarEclipseVM(ISky sky, SolarCalc solarCalc, LunarCalc lunarCalc, ISettings settings)
        {
            this.solarCalc = solarCalc;
            this.lunarCalc = lunarCalc;            
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
            // TODO: move to EclipsesCalculator

            SolarEclipse eclipse = SolarEclipses.NearestEclipse(JulianDay + (next ? 30 : -30), next);
            JulianDay = eclipse.JulianDayMaximum;

            SunMoonPosition[] pos = new SunMoonPosition[5];
            for (int i = 0; i < 5; i++)
            {
                // 5 measurements with 3h step, so interval is -6...+6 hours
                SkyContext c = new SkyContext(JulianDay + TimeSpan.FromHours(3).TotalDays * (i - 2), observerLocation);
                pos[i] = new SunMoonPosition()
                {
                    JulianDay = c.JulianDay,
                    Sun = c.Get(solarCalc.Equatorial0),
                    Moon = c.Get(lunarCalc.Equatorial0),
                    DistanceSun = c.Get(solarCalc.Ecliptical).Distance * 149597870 / 6371.0,
                    DistanceMoon = c.Get(lunarCalc.Ecliptical0).Distance / 6371.0
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
            //if (map.TotalPath[0].Any() && map.TotalPath[1].Any())
                            

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
            else {

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
