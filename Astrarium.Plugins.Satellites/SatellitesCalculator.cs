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
        public ICollection<Satellite> Satellites { get; private set; }

        public CrdsRectangular SunRectangular { get; private set; }

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
            SunRectangular = ecl.ToRectangular(context.Epsilon);

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

        public void ConfigureEphemeris(EphemerisConfig<Satellite> e)
        {
            // Satellites does not provide epheremeris
        }

        public void GetInfo(CelestialObjectInfo<Satellite> info)
        {
            Satellite s = info.Body;
            SkyContext c = info.Context;

            var eq = s.Equatorial;
            var hor = s.Equatorial.ToHorizontal(c.GeoLocation, c.SiderealTime);
            //bool isEclipsed = Norad.IsSatelliteEclipsed(s)

            string haBaseUri = "https://heavens-above.com/";
            string haQuery = "?satid=" + Uri.EscapeDataString(s.Tle.SatelliteNumber) + $"&lat={c.GeoLocation.Latitude.ToString(CultureInfo.InvariantCulture)}&lng={(-c.GeoLocation.Longitude).ToString(CultureInfo.InvariantCulture)}";

            string n2yoBaseUri = "https://www.n2yo.com/";
            string n2yoQuery = $"?s={s.Tle.SatelliteNumber}";

            string openInBrowser = "Show 🡥";

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
                .AddRow("Magnitude", s.Magnitude, Formatters.Magnitude)
                .AddRow("Topocentric position vector", s.Position, Formatters.Simple) // TODO: calculate
                .AddRow("Geocentric position vector", s.Position, Formatters.Simple)
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

                .AddHeader("Heavens Above")
                .AddRow("Satellite info", new Uri(haBaseUri + "satinfo.aspx" + haQuery), openInBrowser)
                .AddRow("Orbit", new Uri(haBaseUri + "orbit.aspx" + haQuery), openInBrowser)
                .AddRow("Passes", new Uri(haBaseUri + "PassSummary.aspx" + haQuery), openInBrowser)
                .AddRow("Close encounters", new Uri(haBaseUri + "CloseEncounters.aspx" + haQuery), openInBrowser)

                .AddHeader("N2YO")
                .AddRow("Satellite info", new Uri(n2yoBaseUri + "satellite/" + n2yoQuery), openInBrowser)
                .AddRow("Live tracking", new Uri(n2yoBaseUri + n2yoQuery + "&live=1"), openInBrowser)
                .AddRow("Passes", new Uri(n2yoBaseUri + "passes/" + n2yoQuery), openInBrowser)
                ;
            /*
                AddText(Program.Language["FormObjectInfo.Magnitude"], s.IsEclipsed ? "В тени" : ((double)(s.Mag)).ToStringMagnitude());
            AddText(Program.Language["FormObjectInfo.Distance"], s.Range.ToStringDistanceKm());
            AddText(Program.Language["FormObjectInfo.SSOAngle"], s.SSOAngle.ToStringAngleShort());
            AddText(Program.Language["FormObjectInfo.RevolutionPeriod"], (s.Tle.Period / 1440.0).ToStringTimeInterval());
            AddText(Program.Language["FormObjectInfo.OrbitInclination"], s.Tle.Inclination.ToStringAngle());
            AddText(Program.Language["FormObjectInfo.Eccentricity"], s.Tle.Eccentricity.ToStringEccentricity());
            AddText(Program.Language["FormObjectInfo.Apsis.Apogee"], Norad.GetSatelliteApogee(s.Tle).ToStringDistanceKm());
            AddText(Program.Language["FormObjectInfo.Apsis.Perigee"], Norad.GetSatellitePerigee(s.Tle).ToStringDistanceKm());
            AddText(Program.Language["FormObjectInfo.DataAge"], (Sky.JulianDay - s.Tle.Epoch).ToStringTimeInterval());
            */

            //info
            //.SetTitle(string.Join(", ", s.Names))
            //.SetSubtitle(Text.Get("Star.Type"))

            //.AddRow("Constellation", Constellations.FindConstellation(c.Get(Equatorial, s.Number), c.JulianDay))

            //.AddHeader(Text.Get("Star.Equatorial"))
            //.AddRow("Equatorial.Alpha", c.Get(Equatorial, s.Number).Alpha)
            //.AddRow("Equatorial.Delta", c.Get(Equatorial, s.Number).Delta)

            //.AddHeader(Text.Get("Star.Equatorial0"))
            //.AddRow("Equatorial0.Alpha", (double)s.Alpha0)
            //.AddRow("Equatorial0.Delta", (double)s.Delta0)

            //.AddHeader(Text.Get("Star.Horizontal"))
            //.AddRow("Horizontal.Azimuth")
            //.AddRow("Horizontal.Altitude")

            //.AddHeader(Text.Get("Star.RTS"))
            //.AddRow("RTS.Rise")
            //.AddRow("RTS.Transit")
            //.AddRow("RTS.Set")
            //.AddRow("RTS.Duration")

            //.AddHeader(Text.Get("Star.Visibility"))
            //.AddRow("Visibility.Begin")
            //.AddRow("Visibility.End")
            //.AddRow("Visibility.Duration")
            //.AddRow("Visibility.Period")

            //.AddHeader(Text.Get("Star.Properties"))
            //.AddRow("Magnitude", s.Magnitude)
            //.AddRow("IsInfraredSource", details.IsInfraredSource)
            //.AddRow("SpectralClass", details.SpectralClass);

            //if (!string.IsNullOrEmpty(details.Pecularity))
            //{
            //    info.AddRow("Pecularity", details.Pecularity);
            //}

            //if (details.RadialVelocity != null)
            //{
            //    info.AddRow("RadialVelocity", details.RadialVelocity + " km/s");
            //}
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
