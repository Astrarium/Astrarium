using Astrarium.Types;
using Astrarium.Algorithms;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using System;
using System.Globalization;

namespace Astrarium.Plugins.Supernovae
{
    public class SupernovaeCalculator : BaseCalc, ICelestialObjectCalc<Supernova>
    {
        public ICollection<Supernova> Supernovae { get; private set; }

        /// <inheritdoc />
        public IEnumerable<Supernova> GetCelestialObjects() => Supernovae;

        /// <inheritdoc />
        public override void Initialize()
        {
            string file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/supernovae.json");
            Supernovae = new SupernovaeReader().Read(file);
        }

        /// <inheritdoc />
        public override void Calculate(SkyContext ctx)
        {
            foreach (var star in Supernovae)
            {
                star.Equatorial = Equatorial(ctx, star);
                star.Magnitude = Magnitude(ctx, star);
            }
        }

        /// <summary>
        /// Gets equatorial coordinates of a star for current epoch
        /// </summary>
        private CrdsEquatorial Equatorial(SkyContext c, Supernova s)
        {
            var eq = Precession.GetEquatorialCoordinates(s.Equatorial0, c.PrecessionElements);
            eq += Nutation.NutationEffect(eq, c.NutationElements, c.Epsilon);
            eq += Aberration.AberrationEffect(eq, c.AberrationElements, c.Epsilon);
            return eq;
        }

        /// <summary>
        /// Gets horizontal coordinates of nova star.
        /// </summary>
        private CrdsHorizontal Horizontal(SkyContext ctx, Supernova s)
        {
            return ctx.Get(Equatorial, s).ToHorizontal(ctx.GeoLocation, ctx.SiderealTime);
        }

        /// <summary>
        /// Calculates visual magnitude of supernova star.
        /// The code is based on Stellarium code, 
        /// see https://github.com/Stellarium/stellarium/blob/4128d5b5fe942001639eed90dd620a37b9afeac6/plugins/Supernovae/src/Supernova.cpp#L188
        /// </summary>
        /// <param name="ctx">Context instance</param>
        /// <param name="s">Supernova</param>
        /// <returns>Visual magnitude value.</returns>
        private float Magnitude(SkyContext ctx, Supernova s)
        {
            double peakJD = s.JulianDayPeak;
            double currentJD = ctx.JulianDay;
            double deltaJD = Math.Abs(peakJD - currentJD);
            double maxMagnitude = s.MaxMagnitude;
            double vmag = 0;

            if (s.Type.Contains("II"))
            {
                // Type II
                if (peakJD <= currentJD)
                {
                    vmag = maxMagnitude;
                    if (deltaJD > 0 && deltaJD <= 30)
                        vmag = maxMagnitude + 0.05 * deltaJD;

                    if (deltaJD > 30 && deltaJD <= 80)
                        vmag = maxMagnitude + 0.013 * (deltaJD - 30) + 1.5;

                    if (deltaJD > 80 && deltaJD <= 100)
                        vmag = maxMagnitude + 0.075 * (deltaJD - 80) + 2.15;

                    if (deltaJD > 100)
                        vmag = maxMagnitude + 0.025 * (deltaJD - 100) + 3.65;
                }
                else
                {
                    if (deltaJD <= 20)
                        vmag = maxMagnitude + 0.75 * deltaJD;
                }
            }
            else
            {
                // Type I
                if (peakJD <= currentJD)
                {
                    vmag = maxMagnitude;
                    if (deltaJD > 0 && deltaJD <= 25)
                        vmag = maxMagnitude + 0.1 * deltaJD;

                    if (deltaJD > 25)
                        vmag = maxMagnitude + 0.016 * (deltaJD - 25) + 2.5;
                }
                else
                {
                    if (deltaJD <= 15)
                        vmag = maxMagnitude + 1.13 * deltaJD;
                }
            }

            if (vmag < maxMagnitude)
                vmag = maxMagnitude;

            return (float)Math.Min(vmag, 30);
        }

        /// <summary>
        /// Gets rise, transit and set info for the star
        /// </summary>
        private RTS RiseTransitSet(SkyContext c, Supernova s)
        {
            double theta0 = Date.ApparentSiderealTime(c.JulianDayMidnight, c.NutationElements.deltaPsi, c.Epsilon);
            var eq = c.Get(Equatorial, s);
            return Visibility.RiseTransitSet(eq, c.GeoLocation, theta0);
        }

        public void ConfigureEphemeris(EphemerisConfig<Supernova> e)
        {
            e["Constellation"] = (c, m) => Constellations.FindConstellation(c.Get(Equatorial, m), c.JulianDay);
            e["Equatorial.Alpha"] = (c, m) => c.Get(Equatorial, m).Alpha;
            e["Equatorial.Delta"] = (c, m) => c.Get(Equatorial, m).Delta;
            e["Horizontal.Altitude"] = (c, m) => c.Get(Horizontal, m).Altitude;
            e["Horizontal.Azimuth"] = (c, m) => c.Get(Horizontal, m).Azimuth;
            e["VarStarType"] = (c, m) => m.SupernovaType;
            e["Magnitude"] = (c, m) => c.Get(Magnitude, m);
            e["MaxMagnitude", Formatters.Magnitude] = (c, m) => m.MaxMagnitude;
            e["RTS.Rise"] = (c, m) => c.GetDateFromTime(c.Get(RiseTransitSet, m).Rise);
            e["RTS.Transit"] = (c, m) => c.GetDateFromTime(c.Get(RiseTransitSet, m).Transit);
            e["RTS.Set"] = (c, m) => c.GetDateFromTime(c.Get(RiseTransitSet, m).Set);
            e["RTS.Duration"] = (c, m) => c.Get(RiseTransitSet, m).Duration;
        }

        public void GetInfo(CelestialObjectInfo<Supernova> info)
        {
            Supernova s = info.Body;
            SkyContext c = info.Context;
            string constellation = Constellations.FindConstellation(c.Get(Equatorial, s), c.JulianDay);
            int year = c.GetDate(c.JulianDay).Year;
            var offset = c.GeoLocation.UtcOffset;
            var jd0 = Date.DeltaT(c.JulianDay) / 86400.0 + Date.JulianDay0(year) - offset / 24;

            info
                .SetTitle(string.Join(", ", info.Body.Names))
                .SetSubtitle(Text.Get("Supernova.Type"))
                .AddRow("Constellation", constellation)

                .AddHeader(Text.Get("Supernova.Equatorial"))
                .AddRow("Equatorial.Alpha")
                .AddRow("Equatorial.Delta")

                .AddHeader(Text.Get("Supernova.Equatorial0"))
                .AddRow("Equatorial0.Alpha", s.Equatorial0.Alpha, Formatters.RA)
                .AddRow("Equatorial0.Delta", s.Equatorial0.Delta, Formatters.Dec)

                .AddHeader(Text.Get("Supernova.Horizontal"))
                .AddRow("Horizontal.Azimuth")
                .AddRow("Horizontal.Altitude")

                .AddHeader(Text.Get("Supernova.Properties"))
                .AddRow("Magnitude")
                .AddRow("PeakDate", new Date(s.JulianDayPeak), Formatters.Date)
                .AddRow("MaxMagnitude", s.MaxMagnitude, Formatters.Magnitude)
                .AddRow("VarStarType", s.SupernovaType)

                .AddHeader(Text.Get("Supernova.RTS"))
                .AddRow("RTS.Rise")
                .AddRow("RTS.Transit")
                .AddRow("RTS.Set")
                .AddRow("RTS.Duration");
        }

        public ICollection<CelestialObject> Search(SkyContext context, string searchString, Func<CelestialObject, bool> filterFunc, int maxCount = 50)
        {
            searchString = searchString.Replace(" ", "");
            return Supernovae
                .Where(m => m.Names.Select(name => name.Replace(" ", "")).Any(name =>
                    name.StartsWith(searchString, StringComparison.OrdinalIgnoreCase) ||
                    (name.StartsWith("SN") && name.Substring(2).Trim().StartsWith(searchString, StringComparison.OrdinalIgnoreCase))))
                .Where(filterFunc)
                .Take(maxCount)
                .ToArray();
        }
    }
}
