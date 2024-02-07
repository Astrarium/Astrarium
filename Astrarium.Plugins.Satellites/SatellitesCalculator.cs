using Astrarium.Types;
using Astrarium.Algorithms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace Astrarium.Plugins.Satellites
{
    public class SatellitesCalculator : BaseCalc, ICelestialObjectCalc<Satellite>
    {
        private const double AU = 149597870.691;

        public ICollection<Satellite> Satellites { get; private set; }

        public Vec3 SunVector{ get; private set; }

        /// <inheritdoc />
        public IEnumerable<Satellite> GetCelestialObjects() => Satellites;

        public double JulianDay { get; private set; }

        private Func<SkyContext, CrdsEquatorial> SunEquatorial;

        private readonly ISky sky;

        public SatellitesCalculator (ISky sky)
        {
            this.sky = sky;
        }

        /// <inheritdoc />
        public override void Initialize()
        {
            SunEquatorial = sky.SunEquatorial;
            string file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/Brightest.tle");
            Satellites = LoadSatellites(file);
        }

        public override void Calculate(SkyContext context)
        {
            // Calculate rectangular coordinates of the Sun
            var eq = SunEquatorial.Invoke(context);
            var ecl = eq.ToEcliptical(context.Epsilon);
            ecl.Distance = 1;
            var r = ecl.ToRectangular(context.Epsilon);
            SunVector = AU * new Vec3(r.X, r.Y, r.Z);

            // To reduce CPU load, it's enough to calculate
            // satellites positions once in 5 minutes
            //if (Math.Abs(JulianDay - context.JulianDay) > TimeSpan.FromMinutes(5).TotalDays)
            {
                double deltaT = Date.DeltaT(context.JulianDay);
                var jd = context.JulianDay - deltaT / 86400;
                foreach (var s in Satellites)
                {
                    Norad.SGP4(s.Tle, jd, s.Position, s.Velocity);
                }

                JulianDay = context.JulianDay;
            }
        }

        public Vec3 GetTopocentricLocationVector(SkyContext context)
        {
            return Norad.TopocentricLocationVector(context.GeoLocation, context.SiderealTime);
        }

        public void ConfigureEphemeris(EphemerisConfig<Satellite> e)
        {
            // Satellites does not provide epheremeris
        }

        private class SatelliteMagnitudeFormatter : Formatters.SignedDoubleFormatter
        {
            public SatelliteMagnitudeFormatter() : base(1, "ᵐ") { }
            public override string Format(object value)
            {
                if (value == null)
                    return "[В тени]";
                else
                    return base.Format(value);
            }
        }

        public void GetInfo(CelestialObjectInfo<Satellite> info)
        {
            Satellite s = info.Body;
            SkyContext c = info.Context;

            double deltaTime = (c.JulianDay - JulianDay) * 24;

            // current satellite position vector
            Vec3 vecGeocentric = s.Position + deltaTime * s.Velocity;

            Vec3 topocentricLocationVector = GetTopocentricLocationVector(c);
            Vec3 vecTopocentric = Norad.TopocentricSatelliteVector(topocentricLocationVector, vecGeocentric);

            // coordinates of the satellite
            var hor = Norad.HorizontalCoordinates(c.GeoLocation, vecTopocentric, c.SiderealTime);
            var eq = hor.ToEquatorial(c.GeoLocation, c.SiderealTime);

            bool isEclipsed = Norad.IsSatelliteEclipsed(vecGeocentric, SunVector);
            double ssoAngle = Angle.ToDegrees((-1 * vecTopocentric).Angle(SunVector - vecTopocentric));

            string haBaseUri = "https://heavens-above.com/";
            string haQuery = "?satid=" + Uri.EscapeDataString(s.Tle.SatelliteNumber) + $"&lat={c.GeoLocation.Latitude.ToString(CultureInfo.InvariantCulture)}&lng={(-c.GeoLocation.Longitude).ToString(CultureInfo.InvariantCulture)}";

            string n2yoBaseUri = "https://www.n2yo.com/";
            string n2yoQuery = $"?s={s.Tle.SatelliteNumber}";

            string openInBrowser = "Open";

            info
                .SetTitle(string.Join(", ", s.Names))
                .SetSubtitle(Text.Get("Satellite.Type"))

                .AddRow("Constellation", Constellations.FindConstellation(s.Equatorial, c.JulianDay))

                .AddHeader(Text.Get("Satellite.Equatorial"))
                .AddRow("Equatorial.Alpha", eq.Alpha)
                .AddRow("Equatorial.Delta", eq.Delta)

                .AddHeader(Text.Get("Satellite.Horizontal"))
                .AddRow("Horizontal.Azimuth", hor.Azimuth)
                .AddRow("Horizontal.Altitude", hor.Altitude)

                .AddHeader(Text.Get("Satellite.Characteristics"))
                .AddRow("Magnitude", isEclipsed ? (float?)null : s.Magnitude, new SatelliteMagnitudeFormatter())
                .AddRow("Distance", 0) // TODO, distance from Earth in km
                .AddRow("S-S-O angle", ssoAngle, new Formatters.UnsignedDoubleFormatter(1, "\u00B0"))
                .AddRow("Topocentric position vector", vecTopocentric, Formatters.Simple)
                .AddRow("Geocentric position vector", vecGeocentric, Formatters.Simple)
                .AddRow("Geocentric velocity vector", s.Velocity, Formatters.Simple)

                .AddHeader("Alternate names")
                .AddRow("Satellite number (SATCAT ID)", new Uri("https://celestrak.org/satcat/table-satcat.php?CATNR=" + Uri.EscapeDataString(s.Tle.SatelliteNumber)), s.Tle.SatelliteNumber)
                .AddRow("Int. designator (COSPAR ID)", new Uri("https://nssdc.gsfc.nasa.gov/nmc/spacecraft/display.action?id=" + Uri.EscapeDataString(s.Tle.InternationalDesignator)), s.Tle.InternationalDesignator)

                .AddHeader("Orbital data")
                .AddRow("Epoch", s.Tle.Epoch, Formatters.JulianDay)
                .AddRow("Inclination", s.Tle.Inclination, Formatters.Inclination)
                .AddRow("Eccentricity", s.Tle.Eccentricity, Formatters.Simple)
                .AddRow("Argument of perigee", s.Tle.ArgumentOfPerigee, new Formatters.UnsignedDoubleFormatter(4, "\u00B0"))
                .AddRow("Longitude of ascending node", s.Tle.LongitudeAscNode, new Formatters.UnsignedDoubleFormatter(4, "\u00B0"))
                .AddRow("Mean anomaly", s.Tle.MeanAnomaly, new Formatters.UnsignedDoubleFormatter(4, "\u00B0"))
                .AddRow("Period", TimeSpan.FromMinutes(s.Tle.Period), Formatters.TimeSpan)
                .AddRow("Apogee", Norad.GetSatelliteApogee(s.Tle))
                .AddRow("Perigee", Norad.GetSatellitePerigee(s.Tle))
                .AddRow("Orbital data age", TimeSpan.FromDays(c.JulianDay - s.Tle.Epoch), Formatters.TimeSpan)

                .AddHeader("Heavens Above")
                .AddRow("Satellite info", new Uri(haBaseUri + "satinfo.aspx" + haQuery), openInBrowser)
                .AddRow("Orbit", new Uri(haBaseUri + "orbit.aspx" + haQuery), openInBrowser)
                .AddRow("Passes", new Uri(haBaseUri + "PassSummary.aspx" + haQuery), openInBrowser)
                .AddRow("Close encounters", new Uri(haBaseUri + "CloseEncounters.aspx" + haQuery), openInBrowser)

                .AddHeader("N2YO")
                .AddRow("Satellite info", new Uri(n2yoBaseUri + "satellite/" + n2yoQuery), openInBrowser)
                .AddRow("Live tracking", new Uri(n2yoBaseUri + n2yoQuery + "&live=1"), openInBrowser)
                .AddRow("Passes", new Uri(n2yoBaseUri + "passes/" + n2yoQuery), openInBrowser);
        }

        public ICollection<CelestialObject> Search(SkyContext context, string searchString, Func<CelestialObject, bool> filterFunc, int maxCount = 50)
        {
            return Satellites
                .Where(s => s.Names.Any(n => n.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >=0 ))
                .Where(filterFunc)
                .Take(maxCount)
                .ToArray();
        }

        private ICollection<Satellite> LoadSatellites(string file)
        {
            var satellites = new List<Satellite>();

            using (var sr = new StreamReader(file, Encoding.UTF8))
            {
                while (!sr.EndOfStream)
                {
                    string name = sr.ReadLine();
                    string line1 = sr.ReadLine();
                    string line2 = sr.ReadLine();
                    satellites.Add(new Satellite(name.Trim(), new TLE(line1, line2)));
                }
                sr.Close();
            }

            return satellites;
        }
    }
}
