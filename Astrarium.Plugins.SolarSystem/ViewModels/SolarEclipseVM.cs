using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
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

        public ICollection<ITileServer> TileServers { get; private set; }

        public ICollection<Marker> Markers { get; private set; }
        public ICollection<Track> Tracks { get; private set; }
        public ICollection<Polygon> Polygons { get; private set; }

        private readonly SolarCalc solarCalc;
        private readonly LunarCalc lunarCalc;
        private readonly ISettings settings;
        private readonly CrdsGeographical observerLocation;

        public ITileServer TileServer
        {
            get => GetValue<ITileServer>(nameof(TileServer));
            set => SetValue(nameof(TileServer), value);
        }

        public SolarEclipseVM(SolarCalc solarCalc, LunarCalc lunarCalc, ISettings settings)
        {
            this.solarCalc = solarCalc;
            this.lunarCalc = lunarCalc;
            this.settings = settings;
            this.observerLocation = settings.Get<CrdsGeographical>("ObserverLocation");

            CacheFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Astrarium", "MapsCache");

            TileServers = new List<ITileServer>() 
            {
                new OfflineTileServer(),
                new OpenStreetMapTileServer("Astrarium v1.0 contact astrarium@astrarium.space"),
                new StamenTerrainTileServer(),
                new OpenTopoMapServer()
            };

            TileServer = TileServers.First();

            JulianDay = new Date(DateTime.Now).ToJulianEphemerisDay();
            CalculateEclipse(next: true);
        }

        private void PrevEclipse()
        {
            CalculateEclipse(next: false);
        }

        private void NextEclipse()
        {
            CalculateEclipse(next: true);
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

            var pbe = SolarEclipses.FindPolynomialBesselianElements(pos);

            var map = SolarEclipses.GetCurves(pbe);

            EclipseDate = Formatters.Date.Format(new Date(JulianDay, observerLocation.UtcOffset));

            var tracks = new List<Track>();
            var polygons = new List<Polygon>();
            var markers = new List<Marker>();

            if (map.P1 != null)
            {
                markers.Add(new Marker(ToGeo(map.P1.Coordinates), MarkerStyle.Default, "P1"));
            }
            if (map.P2 != null)
            {
                markers.Add(new Marker(ToGeo(map.P2.Coordinates), MarkerStyle.Default, "P2"));
            }
            if (map.P3 != null)
            {
                markers.Add(new Marker(ToGeo(map.P3.Coordinates), MarkerStyle.Default, "P3"));
            }
            if (map.P4 != null)
            {
                markers.Add(new Marker(ToGeo(map.P4.Coordinates), MarkerStyle.Default, "P4"));
            }

            if (map.C1 != null)
            {
                markers.Add(new Marker(ToGeo(map.C1.Coordinates), MarkerStyle.Default, "C1"));
            }
            if (map.C2 != null)
            {
                markers.Add(new Marker(ToGeo(map.C2.Coordinates), MarkerStyle.Default, "C2"));
            }


            for (int i = 0; i < 2; i++)
            {
                if (map.UmbraNorthernLimit[i].Any())
                {
                    var track = new Track(new TrackStyle(new Pen(Color.Gray, 2)));
                    track.AddRange(map.UmbraNorthernLimit[i].Select(p => ToGeo(p)));
                    tracks.Add(track);
                }

                if (map.UmbraSouthernLimit[i].Any())
                {
                    var track = new Track(new TrackStyle(new Pen(Color.Gray, 2)));
                    track.AddRange(map.UmbraSouthernLimit[i].Select(p => ToGeo(p)));
                    tracks.Add(track);
                }
            }

            for (int i = 0; i < 2; i++)
            {
                if (map.TotalPath[i].Any())
                {
                    var track = new Track(new TrackStyle(new Pen(Color.Black, 2)));
                    track.AddRange(map.TotalPath[i].Select(p => ToGeo(p)));
                    tracks.Add(track);
                }
            }


            //// central line is single => draw shadow path as single polygon
            //if (map.TotalPath[0].Any() && !map.TotalPath[1].Any())
            //{
            //    var polygon = new Polygon(new PolygonStyle(new SolidBrush(Color.FromArgb(100, Color.Gray))));
            //    polygon.Add(ToGeo(map.C1.Coordinates));

            //    polygon.AddRange(map.UmbraNorthernLimit[0].Select(p => ToGeo(p)));
            //    polygon.AddRange(map.UmbraNorthernLimit[1].Select(p => ToGeo(p)));
            //    polygon.Add(ToGeo(map.C2.Coordinates));
            //    polygon.AddRange((map.UmbraSouthernLimit[1] as IEnumerable<CrdsGeographical>).Reverse().Select(p => ToGeo(p)));
            //    polygon.AddRange((map.UmbraSouthernLimit[0] as IEnumerable<CrdsGeographical>).Reverse().Select(p => ToGeo(p)));
            //    polygon.Add(ToGeo(map.C1.Coordinates));
            //    polygons.Add(polygon);
            //}

            // central line is divided into 2 ones => draw shadow path as 2 polygons
            //if (map.TotalPath[0].Any() && map.TotalPath[1].Any())
            {
                

                if ((map.UmbraNorthernLimit[0].Any() && !map.UmbraNorthernLimit[1].Any()) ||
                    (map.UmbraSouthernLimit[0].Any() && !map.UmbraSouthernLimit[1].Any()))
                {
                    var polygon = new Polygon(new PolygonStyle(new SolidBrush(Color.FromArgb(100, Color.Gray))));
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


                        if ((map.UmbraNorthernLimit[i].Any() && map.UmbraSouthernLimit[i].Any()))
                        {
                            var polygon = new Polygon(new PolygonStyle(new SolidBrush(Color.FromArgb(100, Color.Gray))));

                            if (i == 0)
                            {
                                //polygon.Add(ToGeo(map.C1.Coordinates));
                            }


                            polygon.AddRange(map.UmbraNorthernLimit[i].Select(p => ToGeo(p)));

                            if (i == 1)
                            {
                                //polygon.Add(ToGeo(map.C2.Coordinates));
                            }

                            polygon.AddRange((map.UmbraSouthernLimit[i] as IEnumerable<CrdsGeographical>).Reverse().Select(p => ToGeo(p)));
                            polygons.Add(polygon);
                        }
                    }
                }
            }

            





            

            foreach (var curve in map.RiseSetCurve)
            {
                if (curve.Any())
                {
                    var track = new Track(new TrackStyle(new Pen(Color.Red, 2)));
                    track.AddRange(curve.Select(p => ToGeo(p)));
                    track.Add(track.First());
                    tracks.Add(track);
                }
            }

            if (map.PenumbraNorthernLimit.Any())
            {
                var track = new Track(new TrackStyle(new Pen(Color.Orange, 2)));
                track.AddRange(map.PenumbraNorthernLimit.Select(p => ToGeo(p)));
                tracks.Add(track);
            }

            if (map.PenumbraSouthernLimit.Any())
            {
                var track = new Track(new TrackStyle(new Pen(Color.Orange, 2)));
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
