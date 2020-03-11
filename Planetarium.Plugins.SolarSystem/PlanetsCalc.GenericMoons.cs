using ADK;
using Planetarium.Objects;
using Planetarium.Types;
using Planetarium.Types.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Plugins.SolarSystem
{
    public partial class PlanetsCalc
    {
        public class SatellitePositionData
        {
            public double Jd { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
        }

        private List<SatellitePositionData> ParsePositions(string response)
        {
            List<string> lines = response.Split('\n').ToList();
            string soeMarker = lines.FirstOrDefault(ln => ln == "$$SOE");
            string eoeMarker = lines.FirstOrDefault(ln => ln == "$$EOE");

            int soeMarkerIndex = lines.IndexOf(soeMarker);
            int eoeMarkerIndex = lines.IndexOf(eoeMarker);

            string header = lines[soeMarkerIndex - 2];

            List<string> headerItems = header.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(item => item.Trim()).ToList();

            List<SatellitePositionData> pos = new List<SatellitePositionData>();

            for (int i = soeMarkerIndex + 1; i < eoeMarkerIndex - 1; i++)
            {
                string orbitLine = lines[i];
                List<string> orbitItems = orbitLine.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(item => item.Trim()).ToList();


                pos.Add(new SatellitePositionData()
                {
                    Jd = double.Parse(orbitItems[headerItems.IndexOf("Date_________JDUT")], CultureInfo.InvariantCulture),
                    X = double.Parse(orbitItems[headerItems.IndexOf("X_(sat-prim)")], CultureInfo.InvariantCulture),
                    Y = double.Parse(orbitItems[headerItems.IndexOf("Y_(sat-prim)")], CultureInfo.InvariantCulture)
                });
            }

            return pos;
        }

        private CrdsEcliptical GenericMoon_Ecliptical(SkyContext c, int id)
        {
            var moon = genericMoons.FirstOrDefault(gm => gm.Id == id);
            var eclPlanet = moon.Planet == Planet.PLUTO ? c.Get(Pluto_Ecliptical) : c.Get(Planet_Ecliptical, moon.Planet);
            var orbit = moon.Data;

            if (orbit.jpl)
            {
                var startDate = new Date(c.JulianDay);
                var endDate = new Date(c.JulianDay + 2);

                var startDateStr = $"{startDate.Year:D4}-{startDate.Month:D2}-{(int)(startDate.Day):D2}";
                var endDateStr = $"{endDate.Year:D4}-{endDate.Month:D2}-{(int)(endDate.Day):D2}";

                List<SatellitePositionData> positions = null;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                WebRequest request = WebRequest.Create($"https://ssd.jpl.nasa.gov/horizons_batch.cgi?batch='1'&COMMAND='{(orbit.planet * 100 + orbit.satellite)}'&CENTER='500@399'&MAKE_EPHEM='YES'&TABLE_TYPE='OBSERVER'&START_TIME='{startDateStr}'&STOP_TIME='{endDateStr}'&STEP_SIZE='1 h'&CAL_FORMAT='JD'&APPARENT='AIRLESS'&REF_SYSTEM='J2000'&CSV_FORMAT='YES'&OBJ_DATA='NO'&QUANTITIES='6'");
                using (var response = (HttpWebResponse)request.GetResponse())
                using (var receiveStream = response.GetResponseStream())
                using (var reader = new StreamReader(receiveStream))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        positions = ParsePositions(reader.ReadToEnd());
                    }
                }

                var pos0 = positions.LastOrDefault(np => np.Jd < c.JulianDay);
                var pos1 = positions.FirstOrDefault(np => np.Jd >= c.JulianDay);

                if (pos0 != null && pos1 != null)
                {
                    double dt = pos1.Jd - pos0.Jd;
                    double x = pos0.X + (c.JulianDay - pos0.Jd) / dt * (pos1.X - pos0.X);
                    double y = pos0.Y + (c.JulianDay - pos0.Jd) / dt * (pos1.Y - pos0.Y);

                    CrdsEquatorial eqPlanet = eclPlanet.ToEquatorial(c.Epsilon);

                    // offsets values in degrees           
                    double dAlphaCosDelta = x / 3600;
                    double dDelta = y / 3600;

                    double delta = eqPlanet.Delta + dDelta;
                    double dAlpha = dAlphaCosDelta / Math.Cos(Angle.ToRadians(eqPlanet.Delta));
                    double alpha = eqPlanet.Alpha + dAlpha;

                    var eqSatellite = new CrdsEquatorial(alpha, delta);

                    CrdsEcliptical eclSatellite = eqSatellite.ToEcliptical(c.Epsilon);
                    eclSatellite.Distance = eclPlanet.Distance;

                    return eclSatellite;
                }
                else
                {
                    return eclPlanet;
                }
            }
            else
            {
                return GenericSatellite.Position(c.JulianDay, orbit, eclPlanet);
            }
        }

        private CrdsEquatorial GenericMoon_Equatorial0(SkyContext c, int id)
        {
            return c.Get(GenericMoon_Ecliptical, id).ToEquatorial(c.Epsilon);
        }

        private CrdsEquatorial GenericMoon_Equatorial(SkyContext c, int id)
        {
            var moon = genericMoons.FirstOrDefault(gm => gm.Id == id);
            double parallax = moon.Planet == Planet.PLUTO ? c.Get(Pluto_Parallax) : c.Get(Planet_Parallax, moon.Planet);
            return c.Get(GenericMoon_Equatorial0, id).ToTopocentric(c.GeoLocation, c.SiderealTime, parallax);
        }

        private CrdsHorizontal GenericMoon_Horizontal(SkyContext c, int id)
        {
            return c.Get(GenericMoon_Equatorial, id).ToHorizontal(c.GeoLocation, c.SiderealTime);
        }

        private double GenericMoon_Semidiameter(SkyContext c, int id)
        {
            var ecl = c.Get(GenericMoon_Ecliptical, id);
            var radius = genericMoons.FirstOrDefault(gm => gm.Id == id).Data.radius;
            return GenericSatellite.Semidiameter(ecl.Distance, radius);
        }

        private float GenericMoon_Magnitude(SkyContext c, int id)
        {
            var moon = genericMoons.FirstOrDefault(gm => gm.Id == id);
            var delta = moon.Planet == Planet.PLUTO ? c.Get(Pluto_DistanceFromEarth) : c.Get(Planet_DistanceFromEarth, moon.Planet);
            double r = moon.Planet == Planet.PLUTO ? c.Get(Pluto_DistanceFromSun) : c.Get(Planet_DistanceFromSun, moon.Planet);
            var mag0 = moon.Data.mag;
            return GenericSatellite.Magnitude(mag0, delta, r);
        }

        public void ConfigureEphemeris(EphemerisConfig<GenericMoon> e)
        {
            e["Constellation"] = (c, nm) => Constellations.FindConstellation(c.Get(GenericMoon_Equatorial, nm.Id), c.JulianDay);
            e["Equatorial.Alpha"] = (c, nm) => c.Get(GenericMoon_Equatorial, nm.Id).Alpha;
            e["Equatorial.Delta"] = (c, nm) => c.Get(GenericMoon_Equatorial, nm.Id).Delta;
            e["Horizontal.Altitude"] = (c, nm) => c.Get(GenericMoon_Horizontal, nm.Id).Altitude;
            e["Horizontal.Azimuth"] = (c, nm) => c.Get(GenericMoon_Horizontal, nm.Id).Azimuth;            
            e["AngularDiameter"] = (c, nm) => c.Get(GenericMoon_Semidiameter, nm.Id) * 2 / 3600.0;
            e["Magnitude"] = (c, nm) => c.Get(GenericMoon_Magnitude, nm.Id);
        }

        public void GetInfo(CelestialObjectInfo<GenericMoon> info)
        {
            info
            .SetSubtitle(Text.Get("Satellite.Subtitle", ("planetName", Text.Get($"Planet.{info.Body.Data.planet}.GenitiveName"))))
            .SetTitle(info.Body.Names.First())

            .AddRow("Constellation")

            .AddHeader(Text.Get("GenericMoon.Horizontal"))
            .AddRow("Horizontal.Azimuth")
            .AddRow("Horizontal.Altitude")

            .AddHeader(Text.Get("GenericMoon.Equatorial"))
            .AddRow("Equatorial.Alpha")
            .AddRow("Equatorial.Delta")

            .AddRow("Magnitude")
            .AddRow("AngularDiameter");
            //.AddHeader(Text.Get("GenericMoon.RTS"))
            //.AddRow("RTS.Rise")
            //.AddRow("RTS.Transit")
            //.AddRow("RTS.Set")
            //.AddRow("RTS.Duration");
        }
    }
}
