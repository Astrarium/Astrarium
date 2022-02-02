using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Astrarium.Plugins.MinorBodies
{
    public class CometsCalc : MinorBodyCalc<Comet>, ICelestialObjectCalc<Comet>
    {
        private readonly ISky sky;
        private readonly ISettings settings;
        private readonly CometsReader reader;
        private readonly CometsDataUpdater updater;
        private readonly List<Comet> comets = new List<Comet>();
        private object locker = new object();
        public ICollection<Comet> Comets { get { lock (locker) { return comets; } } }
        public IEnumerable<Comet> GetCelestialObjects() => Comets;

        public CometsCalc(ISky sky, ISettings settings, CometsReader reader, CometsDataUpdater updater)
        {
            this.sky = sky;
            this.settings = settings;
            this.reader = reader;
            this.updater = updater;
        }

        public async void UpdateOrbitalElements(bool silent)
        {
            ICollection<Comet> newData = await updater.Update(silent);
            if (newData != null && newData.Any())
            {
                lock (locker)
                {
                    comets.Clear();
                    comets.AddRange(newData);
                    sky.Calculate();
                }
            }
        }

        public async override void Initialize()
        {
            // use app data path to comets data (downloaded by user)
            string file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Astrarium", "OrbitalElements", "Comets.dat");

            // use default path to comets data
            if (!File.Exists(file))
            {
                file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data", "Comets.dat");
            }

            if (File.Exists(file))
            {
                comets.Clear();
                comets.AddRange(reader.Read(file));
            }
            else
            {
                Trace.TraceError("Comets orbital elements data file not found.");
            }

            if (settings.Get<bool>("CometsAutoUpdateOrbitalElements") &&
                DateTime.Now.Subtract(settings.Get<DateTime>("CometsDownloadOrbitalElementsTimestamp")).TotalDays >= (int)settings.Get<decimal>("CometsAutoUpdateOrbitalElementsPeriod"))
            {
                await Task.Run(() => UpdateOrbitalElements(silent: true));
            }
        }

        public override void Calculate(SkyContext ctx)
        {
            lock (locker)
            {
                foreach (Comet c in comets)
                {
                    c.Horizontal = ctx.Get(Horizontal, c);
                    c.Magnitude = ctx.Get(Magnitude, c);
                    c.Semidiameter = ctx.Get(Appearance, c).Coma;
                    c.TailHorizontal = ctx.Get(TailHorizontal, c);
                }
            }
        }

        protected override OrbitalElements OrbitalElements(SkyContext ctx, Comet c)
        {
            return c.Orbit;
        }

        private float Magnitude(SkyContext ctx, Comet c)
        {
            var delta = ctx.Get(DistanceFromEarth, c);
            var r = ctx.Get(DistanceFromSun, c);
            return (float)(c.H + 5 * Math.Log10(delta) + c.G * Math.Log(r));
        } 

        private CometAppearance Appearance(SkyContext ctx, Comet c)
        {
            double r = ctx.Get(DistanceFromSun, c);
            double delta = ctx.Get(DistanceFromEarth, c);
            return MinorBodyEphem.CometAppearance(c.H, c.G, r, delta);
        }

        /// <summary>
        /// Gets equatorial coordinates of comet tail end
        /// </summary>
        private CrdsEquatorial TailEquatorial(SkyContext ctx, Comet c)
        {
            var rBody = ctx.Get(RectangularH, c);
            var rSun = ctx.Get(SunRectangular);

            // distance from Sun
            double r = Math.Sqrt(rBody.X * rBody.X + rBody.Y * rBody.Y + rBody.Z * rBody.Z);

            double k = (r + ctx.Get(Appearance, c).Tail) / r;

            double x = rSun.X + k * rBody.X;
            double y = rSun.Y + k * rBody.Y;
            double z = rSun.Z + k * rBody.Z;

            // distance from Earth of tail end
            double Delta = Math.Sqrt(x * x + y * y + z * z);

            double alpha = Angle.ToDegrees(Math.Atan2(y, x));
            double delta = Angle.ToDegrees(Math.Asin(z / Delta));

            // geocentric equatoral for J2000.0
            var eq0 = new CrdsEquatorial(alpha, delta);

            // Precessinal elements to convert between epochs
            var pe = ctx.Get(GetPrecessionalElements);

            // Equatorial coordinates for the mean equinox and epoch of the target date
            // No nutation an aberration corrections here, because we do not need high precision
            return Precession.GetEquatorialCoordinates(eq0, pe);
        }

        /// <summary>
        /// Calculates horizontal coordinates of comet tail end
        /// </summary>
        private CrdsHorizontal TailHorizontal(SkyContext ctx, Comet c)
        {
            var eq = ctx.Get(TailEquatorial, c);
            return eq.ToHorizontal(ctx.GeoLocation, ctx.SiderealTime);
        }

        /// <summary>
        /// Calculates visible length of comet tail, in degrees of arc
        /// </summary>
        internal double TailVisibleLength(SkyContext ctx, Comet c)
        {
            var eqComet = ctx.Get(EquatorialT, c);
            var eqTail = ctx.Get(TailEquatorial, c);
            return Angle.Separation(eqComet, eqTail);
        }

        public void ConfigureEphemeris(EphemerisConfig<Comet> e)
        {
            e["Constellation"] = (c, p) => Constellations.FindConstellation(c.Get(EquatorialT, p), c.JulianDay);
            e["Magnitude"] = (c, p) => c.Get(Magnitude,p);
            e["AngularDiameter"] = (c, p) => c.Get(Appearance, p).Coma / 3600f;
            e["DistanceFromEarth"] = (c, p) => c.Get(DistanceFromEarth, p);
            e["DistanceFromSun"] = (c, p) => c.Get(DistanceFromSun, p);
            e["HorizontalParallax"] = (c, p) => c.Get(Parallax, p);
            e["Horizontal.Altitude"] = (c, p) => c.Get(Horizontal, p).Altitude;
            e["Horizontal.Azimuth"] = (c, p) => c.Get(Horizontal, p).Azimuth;
            e["Equatorial.Alpha"] = (c, p) => c.Get(EquatorialT, p).Alpha;
            e["Equatorial.Delta"] = (c, p) => c.Get(EquatorialT, p).Delta;
            e["Equatorial0.Alpha"] = (c, p) => c.Get(EquatorialJ2000, p).Alpha;
            e["Equatorial0.Delta"] = (c, p) => c.Get(EquatorialJ2000, p).Delta;
            e["Equatorial0T.Alpha", Formatters.RA] = (c, p) => c.Get(EquatorialJ2000T, p).Alpha;
            e["Equatorial0T.Delta", Formatters.Dec] = (c, p) => c.Get(EquatorialJ2000T, p).Delta;
            e["EquatorialG.Alpha", Formatters.RA] = (c, p) => c.Get(EquatorialG, p).Alpha;
            e["EquatorialG.Delta", Formatters.Dec] = (c, p) => c.Get(EquatorialG, p).Delta;
            e["Ecliptical.Lambda"] = (c, p) => c.Get(Ecliptical, p).Lambda;
            e["Ecliptical.Beta"] = (c, p) => c.Get(Ecliptical, p).Beta;
            e["RTS.Rise"] = (c, p) => c.GetDateFromTime(c.Get(RiseTransitSet, p).Rise);
            e["RTS.Transit"] = (c, p) => c.GetDateFromTime(c.Get(RiseTransitSet, p).Transit);
            e["RTS.Set"] = (c, p) => c.GetDateFromTime(c.Get(RiseTransitSet, p).Set);
            e["RTS.Duration"] = (c, p) => c.Get(RiseTransitSet, p).Duration;
            e["RTS.RiseAzimuth"] = (c, p) => c.Get(RiseTransitSet, p).RiseAzimuth;
            e["RTS.TransitAltitude"] = (c, p) => c.Get(RiseTransitSet, p).TransitAltitude;
            e["RTS.SetAzimuth"] = (c, p) => c.Get(RiseTransitSet, p).SetAzimuth;
            e["Visibility.Begin"] = (c, p) => c.GetDateFromTime(c.Get(Visibility, p).Begin);
            e["Visibility.End"] = (c, p) => c.GetDateFromTime(c.Get(Visibility, p).End);
            e["Visibility.Duration"] = (c, p) => c.Get(Visibility, p).Duration;
            e["Visibility.Period"] = (c, p) => c.Get(Visibility, p).Period;
        }

        public void GetInfo(CelestialObjectInfo<Comet> info)
        {
            info
            .SetTitle(info.Body.Names.First())
            .SetSubtitle(Text.Get("Comet.Type"))

            .AddRow("Constellation")

            .AddHeader(Text.Get("Comet.Horizontal"))
            .AddRow("Horizontal.Azimuth")
            .AddRow("Horizontal.Altitude")

            .AddHeader(Text.Get("Comet.Equatorial"))
            .AddRow("Equatorial.Alpha")
            .AddRow("Equatorial.Delta")

            .AddHeader(Text.Get("Comet.EquatorialG"))
            .AddRow("EquatorialG.Alpha")
            .AddRow("EquatorialG.Delta")

            .AddHeader(Text.Get("Comet.Equatorial0T"))
            .AddRow("Equatorial0T.Alpha")
            .AddRow("Equatorial0T.Delta")

            .AddHeader(Text.Get("Comet.Equatorial0"))
            .AddRow("Equatorial0.Alpha")
            .AddRow("Equatorial0.Delta")

            .AddHeader(Text.Get("Comet.Ecliptical"))
            .AddRow("Ecliptical.Lambda")
            .AddRow("Ecliptical.Beta")

            .AddHeader(Text.Get("Comet.RTS"))
            .AddRow("RTS.Rise")
            .AddRow("RTS.Transit")
            .AddRow("RTS.Set")
            .AddRow("RTS.Duration")

            .AddHeader(Text.Get("Comet.Visibility"))
            .AddRow("Visibility.Begin")
            .AddRow("Visibility.End")
            .AddRow("Visibility.Duration")
            .AddRow("Visibility.Period")

            .AddHeader(Text.Get("Comet.Appearance"))
            .AddRow("AngularDiameter")
            .AddRow("Magnitude")
            .AddRow("DistanceFromEarth")
            .AddRow("DistanceFromSun")
            .AddRow("HorizontalParallax");
        }

        public ICollection<CelestialObject> Search(SkyContext context, string searchString, int maxCount = 50)
        {
            return Comets
                .Where(c => GetNames(c.Name).Any(n => n.StartsWith(searchString, StringComparison.OrdinalIgnoreCase)))
                .ToArray();
        }

        private string[] GetNames(string name)
        {
            var match = Regex.Match(name, "^(.+)\\((.+)\\)$");
            if (match.Success)
            {
                return new string[] { match.Groups[1].Value.Trim(), match.Groups[2].Value.Trim() };
            }
            else
            {
                return name.Split('/').Select(n => n.Trim()).ToArray();
            }
        }
    }
}
