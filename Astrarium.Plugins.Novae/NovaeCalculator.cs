using Astrarium.Types;
using Astrarium.Algorithms;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using System;

namespace Astrarium.Plugins.Novae
{
    public class NovaeCalculator : BaseCalc, ICelestialObjectCalc<Nova>
    {
        public ICollection<Nova> Novae { get; private set; }

        /// <inheritdoc />
        public IEnumerable<Nova> GetCelestialObjects() => Novae;

        /// <inheritdoc />
        public override void Initialize()
        {
            string file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/novae.json");
            Novae = new NovaeReader().Read(file);
        }

        /// <inheritdoc />
        public override void Calculate(SkyContext ctx)
        {
            foreach (var nova in Novae)
            {
                nova.Horizontal = ctx.Get(Horizontal, nova);
                nova.Mag = ctx.Get(Magnitude, nova);
            }
        }

        /// <summary>
        /// Gets precessional elements to convert equatorial coordinates of stars to current epoch 
        /// </summary>
        private PrecessionalElements GetPrecessionalElements(SkyContext c)
        {
            return Precession.ElementsFK5(Date.EPOCH_J2000, c.JulianDay);
        }

        /// <summary>
        /// Gets equatorial coordinates of a star for current epoch
        /// </summary>
        public CrdsEquatorial Equatorial(SkyContext c, Nova n)
        {
            PrecessionalElements p = c.Get(GetPrecessionalElements);
            double years = c.Get(YearsSince2000);

            // Initial coodinates for J2000 epoch
            CrdsEquatorial eq0 = new CrdsEquatorial(n.Equatorial0);

            // Equatorial coordinates for the mean equinox and epoch of the target date
            CrdsEquatorial eq = Precession.GetEquatorialCoordinates(eq0, p);

            // Nutation effect
            var eq1 = Nutation.NutationEffect(eq, c.NutationElements, c.Epsilon);

            // Aberration effect
            var eq2 = Aberration.AberrationEffect(eq, c.AberrationElements, c.Epsilon);

            // Apparent coordinates of the star
            eq += eq1 + eq2;

            return eq;
        }

        /// <summary>
        /// Gets number of years (with fractions) since J2000.0 epoch
        /// </summary>
        private double YearsSince2000(SkyContext c)
        {
            return (c.JulianDay - Date.EPOCH_J2000) / 365.25;
        }

        /// <summary>
        /// Gets horizontal coordinates of nova star.
        /// </summary>
        private CrdsHorizontal Horizontal(SkyContext ctx, Nova n)
        {
            return ctx.Get(Equatorial, n).ToHorizontal(ctx.GeoLocation, ctx.SiderealTime);
        }

        /// <summary>
        /// Calculates visual magnitude of nova star.
        /// The code is based on Stellarium code, 
        /// see https://github.com/Stellarium/stellarium/blob/1e617e49918ab2e5e5fad318dd1ab4024eb1e41c/plugins/Novae/src/Nova.cpp#L194
        /// </summary>
        /// <param name="ctx">Context instance</param>
        /// <param name="n">Novae</param>
        /// <returns>Visual magnitude value.</returns>
        private float Magnitude(SkyContext ctx, Nova n)
        {
            float mag = n.MinMagnitude;
            double jd = ctx.JulianDay;
            float delta = (float)Math.Abs(n.JulianDayPeak - jd);

            int t2 = n.M2 ?? -1;
            if (n.M2 == null)
            {
                if (n.NovaType.Contains("NA"))
                    t2 = 10;

                if (n.NovaType.Contains("NB"))
                    t2 = 80; 

                if (n.NovaType.Contains("NC"))
                    t2 = 200;
            }

            int t3 = n.M3 ?? -1;
            if (n.M3 == null)
            {
                if (n.NovaType.Contains("NA"))
                    t3 = 30;

                if (n.NovaType.Contains("NB"))
                    t3 = 160;

                if (n.NovaType.Contains("NC"))
                    t3 = 300; 
            }

            int t6 = n.M6 ?? -1;
            if (n.M6 == null)
            {
                if (n.NovaType.Contains("NA"))
                    t6 = 100;

                if (n.NovaType.Contains("NB"))
                    t6 = 300; 

                if (n.NovaType.Contains("NC"))
                    t6 = 1200; 
            }

            int t9 = n.M9 ?? -1;
            if (n.M9 == null)
            {
                if (n.NovaType.Contains("NA"))
                    t9 = 400; 

                if (n.NovaType.Contains("NB"))
                    t9 = 1000; 

                if (n.NovaType.Contains("NC"))
                    t9 = 3000;
            }

            // Fading curve
            if (n.JulianDayPeak <= jd)
            {
                float step;
                float d2 = n.MaxMagnitude + 2f;
                float d3 = n.MaxMagnitude + 3f;
                float d6 = n.MaxMagnitude + 6f;

                if (delta > 0 && delta <= t2)
                {
                    step = 2f / t2;
                    mag = n.MaxMagnitude + step * delta;
                }

                if (delta > t2 && delta <= t3)
                {
                    step = 3f / t3;
                    mag = d2 + step * (delta - t2);
                }

                if (delta > t3 && delta <= t6)
                {
                    step = 6f / t6;
                    mag = d3 + step * (delta - t3);
                }

                if (delta > t6 && delta <= t9)
                {
                    step = 9f / t9;
                    mag = d6 + step * (delta - t6);
                }

                if (delta > t9)
                {
                    mag = n.MinMagnitude;
                }
            }
            // Outburst curve
            else
            {
                int dt = 3;
                if (delta <= dt)
                {
                    float step = (n.MinMagnitude - n.MaxMagnitude) / dt; 
                    mag = n.MaxMagnitude + step * delta;
                }
            }

            if (mag > n.MinMagnitude)
            {
                mag = n.MinMagnitude;
            }

            return mag;
        }

        /// <summary>
        /// Gets rise, transit and set info for the star
        /// </summary>
        private RTS RiseTransitSet(SkyContext c, Nova n)
        {
            double theta0 = Date.ApparentSiderealTime(c.JulianDayMidnight, c.NutationElements.deltaPsi, c.Epsilon);
            var eq = c.Get(Equatorial, n);
            return Visibility.RiseTransitSet(eq, c.GeoLocation, theta0);
        }

        private readonly string[] NOVA_TYPES = new string[] { "NA", "NB", "NC" };
        private string NovaTypeDescription(Nova n)
        {
            var type = NOVA_TYPES.FirstOrDefault(t => n.NovaType.Contains(t));
            return type != null ? $" ({Text.Get($"Nova.Type.{type}")})" : "";
        }

        public void ConfigureEphemeris(EphemerisConfig<Nova> e)
        {
            e["Constellation"] = (c, m) => Constellations.FindConstellation(c.Get(Equatorial, m), c.JulianDay);
            e["Equatorial.Alpha"] = (c, m) => c.Get(Equatorial, m).Alpha;
            e["Equatorial.Delta"] = (c, m) => c.Get(Equatorial, m).Delta;
            e["Horizontal.Altitude"] = (c, m) => c.Get(Horizontal, m).Altitude;
            e["Horizontal.Azimuth"] = (c, m) => c.Get(Horizontal, m).Azimuth;
            e["Magnitude"] = (c, m) => c.Get(Magnitude, m);
            e["RTS.Rise"] = (c, m) => c.GetDateFromTime(c.Get(RiseTransitSet, m).Rise);
            e["RTS.Transit"] = (c, m) => c.GetDateFromTime(c.Get(RiseTransitSet, m).Transit);
            e["RTS.Set"] = (c, m) => c.GetDateFromTime(c.Get(RiseTransitSet, m).Set);
            e["RTS.Duration"] = (c, m) => c.Get(RiseTransitSet, m).Duration;
        }

        public void GetInfo(CelestialObjectInfo<Nova> info)
        {
            Nova n = info.Body;
            SkyContext c = info.Context;
            string constellation = Constellations.FindConstellation(c.Get(Equatorial, n), c.JulianDay);
            int year = c.GetDate(c.JulianDay).Year;
            var offset = c.GeoLocation.UtcOffset;
            var jd0 = Date.DeltaT(c.JulianDay) / 86400.0 + Date.JulianDay0(year) - offset / 24;

            info
                .SetTitle(string.Join(", ", info.Body.Names))
                .SetSubtitle(Text.Get("Nova.Type"))
                .AddRow("Constellation", constellation)

                .AddHeader(Text.Get("Nova.Equatorial"))
                .AddRow("Equatorial.Alpha")
                .AddRow("Equatorial.Delta")

                .AddHeader(Text.Get("Nova.Equatorial0"))
                .AddRow("Equatorial0.Alpha", n.Equatorial0.Alpha, Formatters.RA)
                .AddRow("Equatorial0.Delta", n.Equatorial0.Delta, Formatters.Dec)

                .AddHeader(Text.Get("Nova.Horizontal"))
                .AddRow("Horizontal.Azimuth")
                .AddRow("Horizontal.Altitude")

                .AddHeader(Text.Get("Nova.Properties"))
                .AddRow("Magnitude")
                .AddRow("PeakDate", new Date(n.JulianDayPeak), Formatters.Date)
                .AddRow("MaxMagnitude", n.MaxMagnitude, Formatters.Magnitude)
                .AddRow("MinMagnitude", n.MinMagnitude, Formatters.Magnitude)
                .AddRow("Type", n.NovaType + NovaTypeDescription(n))

                .AddHeader(Text.Get("Nova.RTS"))
                .AddRow("RTS.Rise")
                .AddRow("RTS.Transit")
                .AddRow("RTS.Set")
                .AddRow("RTS.Duration");
        }

        public ICollection<CelestialObject> Search(SkyContext context, string searchString, Func<CelestialObject, bool> filterFunc, int maxCount = 50)
        {
            return Novae
                .Where(m => m.Names.Any(n => n.StartsWith(searchString, StringComparison.OrdinalIgnoreCase)))
                .Where(filterFunc)
                .Take(maxCount)
                .ToArray();
        }
    }
}
