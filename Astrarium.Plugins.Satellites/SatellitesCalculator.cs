using Astrarium.Types;
using Astrarium.Algorithms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
            /*
            e["Constellation"] = (c, s) => Constellations.FindConstellation(c.Get(Equatorial, s.Number), c.JulianDay);
            e["Equatorial.Alpha"] = (c, s) => c.Get(Equatorial, s.Number).Alpha;
            e["Equatorial.Delta"] = (c, s) => c.Get(Equatorial, s.Number).Delta;
            e["Horizontal.Azimuth"] = (c, s) => c.Get(Horizontal, s.Number).Azimuth;
            e["Horizontal.Altitude"] = (c, s) => c.Get(Horizontal, s.Number).Altitude;
            e["Magnitude"] = (c, s) => s.Magnitude;
            e["RTS.Rise"] = (c, s) => c.GetDateFromTime(c.Get(RiseTransitSet, s.Number).Rise);
            e["RTS.Transit"] = (c, s) => c.GetDateFromTime(c.Get(RiseTransitSet, s.Number).Transit);
            e["RTS.Set"] = (c, s) => c.GetDateFromTime(c.Get(RiseTransitSet, s.Number).Set);
            e["RTS.Duration"] = (c, s) => c.Get(RiseTransitSet, s.Number).Duration;
            e["RTS.RiseAzimuth"] = (c, s) => c.Get(RiseTransitSet, s.Number).RiseAzimuth;
            e["RTS.TransitAltitude"] = (c, s) => c.Get(RiseTransitSet, s.Number).TransitAltitude;
            e["RTS.SetAzimuth"] = (c, s) => c.Get(RiseTransitSet, s.Number).SetAzimuth;
            e["Visibility.Begin"] = (c, s) => c.GetDateFromTime(c.Get(VisibilityDetails, s.Number).Begin);
            e["Visibility.End"] = (c, s) => c.GetDateFromTime(c.Get(VisibilityDetails, s.Number).End);
            e["Visibility.Duration"] = (c, s) => c.Get(VisibilityDetails, s.Number).Duration;
            e["Visibility.Period"] = (c, s) => c.Get(VisibilityDetails, s.Number).Period;
            */
        }

        public void GetInfo(CelestialObjectInfo<Satellite> info)
        {
            Satellite s = info.Body;
            SkyContext c = info.Context;

            info
                .SetTitle(string.Join(", ", s.Names))
                .SetSubtitle(Text.Get("Satellite.Type"));

            //.AddRow("Constellation", Constellations.FindConstellation(c.Get(Equatorial, s.Tle.SatelliteNumber), c.JulianDay))

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
            return new CelestialObject[0];
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
