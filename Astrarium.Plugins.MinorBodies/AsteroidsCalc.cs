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
    public class AsteroidsCalc : MinorBodyCalc<Asteroid>, ICelestialObjectCalc<Asteroid>
    {
        private readonly Regex asteroidNameRegex = new Regex("\\((\\d+)\\)\\s*(\\w+)");
        private readonly ISky sky;
        private readonly ISettings settings;
        private readonly AsteroidsReader reader;
        private readonly AsteroidsDataUpdater updater;
        private readonly List<Asteroid> asteroids = new List<Asteroid>();
        private object locker = new object();

        public IEnumerable<Asteroid> GetCelestialObjects() => Asteroids;
        public ICollection<Asteroid> Asteroids { get { lock (locker) { return asteroids; } } }

        public AsteroidsCalc(ISky sky, ISettings settings, AsteroidsReader reader, AsteroidsDataUpdater updater)
        {
            this.sky = sky;
            this.settings = settings;
            this.reader = reader;
            this.updater = updater;
        }

        public async void UpdateOrbitalElements(bool silent)
        {
            ICollection<Asteroid> newData = await updater.Update(silent);
            if (newData != null && newData.Any())
            {
                lock (locker)
                {
                    asteroids.Clear();
                    asteroids.AddRange(newData);
                }
                Trace.TraceInformation($"Updated asteroids: {newData.Count}");
                sky.Calculate();
            }
        }

        public async override void Initialize()
        {
            // use app data path to asteroids data (downloaded by user)
            string file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Astrarium", "OrbitalElements", "Asteroids.dat");

            // use default path to asteroids data
            if (!File.Exists(file))
            {
                file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data", "Asteroids.dat");
            }

            if (File.Exists(file))
            {
                asteroids.Clear();
                asteroids.AddRange(reader.Read(file));
            }
            else
            {
                Trace.TraceError("Asteroids orbital elements data file not found.");
            }

            if (settings.Get<bool>("AsteroidsAutoUpdateOrbitalElements") && 
                DateTime.Now.Subtract(settings.Get<DateTime>("AsteroidsDownloadOrbitalElementsTimestamp")).TotalDays >= (int)settings.Get<decimal>("AsteroidsAutoUpdateOrbitalElementsPeriod"))
            {
                Trace.TraceInformation("Obital elements of asteroids needs to be updated, updating...");
                await Task.Run(() => UpdateOrbitalElements(silent: true));
            }
        }

        public override void Calculate(SkyContext c)
        {
            lock (locker)
            {
                foreach (Asteroid a in asteroids)
                {
                    a.Horizontal = c.Get(Horizontal, a);
                    a.Magnitude = c.Get(Magnitude, a);
                    a.Semidiameter = c.Get(Semidiameter, a);
                }
            }
        }

        protected override OrbitalElements OrbitalElements(SkyContext c, Asteroid a)
        {
            return a.Orbit;
        }

        public float Magnitude(SkyContext c, Asteroid a)
        {
            double delta = c.Get(DistanceFromEarth, a);
            double r = c.Get(DistanceFromSun, a);
            double beta = c.Get(PhaseAngle, a);

            return MinorBodyEphem.Magnitude(a.G, a.H, beta, r, delta);
        }

        private double Semidiameter(SkyContext c, Asteroid a)
        {
            double delta = c.Get(DistanceFromEarth, a);
            return MinorBodyEphem.Semidiameter(delta, a.PhysicalDiameter);
        }

        public void ConfigureEphemeris(EphemerisConfig<Asteroid> e)
        {
            e["Constellation"] = (c, a) => Constellations.FindConstellation(c.Get(EquatorialT, a), c.JulianDay);            
            e["Horizontal.Altitude"] = (c, a) => c.Get(Horizontal, a).Altitude;
            e["Horizontal.Azimuth"] = (c, a) => c.Get(Horizontal, a).Azimuth;
            e["Equatorial.Alpha"] = (c, a) => c.Get(EquatorialT, a).Alpha;
            e["Equatorial.Delta"] = (c, a) => c.Get(EquatorialT, a).Delta;
            e["Equatorial0.Alpha"] = (c, a) => c.Get(EquatorialG, a).Alpha;
            e["Equatorial0.Delta"] = (c, a) => c.Get(EquatorialG, a).Delta;
            e["Ecliptical.Lambda"] = (c, a) => c.Get(Ecliptical, a).Lambda;
            e["Ecliptical.Beta"] = (c, a) => c.Get(Ecliptical, a).Beta;
            e["RTS.Rise"] = (c, a) => c.GetDateFromTime(c.Get(RiseTransitSet, a).Rise);
            e["RTS.Transit"] = (c, a) => c.GetDateFromTime(c.Get(RiseTransitSet, a).Transit);
            e["RTS.Set"] = (c, a) => c.GetDateFromTime(c.Get(RiseTransitSet, a).Set);
            e["RTS.Duration"] = (c, a) => c.Get(RiseTransitSet, a).Duration;
            e["RTS.RiseAzimuth"] = (c, a) => c.Get(RiseTransitSet, a).RiseAzimuth;
            e["RTS.TransitAltitude"] = (c, a) => c.Get(RiseTransitSet, a).TransitAltitude;
            e["RTS.SetAzimuth"] = (c, a) => c.Get(RiseTransitSet, a).SetAzimuth;
            e["Visibility.Begin"] = (c, a) => c.GetDateFromTime(c.Get(Visibility, a).Begin);
            e["Visibility.End"] = (c, a) => c.GetDateFromTime(c.Get(Visibility, a).End);
            e["Visibility.Duration"] = (c, a) => c.Get(Visibility, a).Duration;
            e["Visibility.Period"] = (c, a) => c.Get(Visibility, a).Period;
            e["DistanceFromEarth"] = (c, a) => c.Get(DistanceFromEarth, a);
            e["DistanceFromSun"] = (c, a) => c.Get(DistanceFromSun, a);
            e["Phase"] = (c, a) => c.Get(Phase, a);
            e["PhaseAngle"] = (c, a) => c.Get(PhaseAngle, a);
            e["Magnitude"] = (c, a) => c.Get(Magnitude, a);
            e["HorizontalParallax"] = (c, a) => c.Get(Parallax, a);
            e["AngularDiameter", a => a.PhysicalDiameter > 0] = (c, a) => c.Get(Semidiameter, a) * 2 / 3600.0;
        }

        public void GetInfo(CelestialObjectInfo<Asteroid> info)
        {
            info
            .SetTitle(info.Body.Names.First())
            .SetSubtitle(Text.Get("Asteroid.Type"))
            .AddRow("Constellation")

            .AddHeader(Text.Get("Asteroid.Equatorial"))
            .AddRow("Equatorial.Alpha")
            .AddRow("Equatorial.Delta")

            .AddHeader(Text.Get("Asteroid.Equatorial0"))
            .AddRow("Equatorial0.Alpha")
            .AddRow("Equatorial0.Delta")

            .AddHeader(Text.Get("Asteroid.Ecliptical"))
            .AddRow("Ecliptical.Lambda")
            .AddRow("Ecliptical.Beta")

            .AddHeader(Text.Get("Asteroid.Horizontal"))
            .AddRow("Horizontal.Azimuth")
            .AddRow("Horizontal.Altitude")

            .AddHeader(Text.Get("Asteroid.Appearance"))
            .AddRow("Phase")
            .AddRow("PhaseAngle")
            .AddRow("Magnitude")
            .AddRow("DistanceFromEarth")
            .AddRow("DistanceFromSun")
            .AddRow("HorizontalParallax");

            if (info.Body.PhysicalDiameter > 0)
            { 
                info.AddRow("AngularDiameter");
            }

            info
            .AddHeader(Text.Get("Asteroid.RTS"))
            .AddRow("RTS.Rise")
            .AddRow("RTS.Transit")
            .AddRow("RTS.Set")
            .AddRow("RTS.Duration");

            info
            .AddHeader(Text.Get("Asteroid.Visibility"))
            .AddRow("Visibility.Begin")
            .AddRow("Visibility.End")
            .AddRow("Visibility.Duration")
            .AddRow("Visibility.Period");
        }

        public ICollection<CelestialObject> Search(SkyContext context, string searchString, int maxCount = 50)
        {
            return Asteroids
                .Where(a => IsAsteroidNameMatch(a, searchString))
                .ToArray();
        }

        private bool IsAsteroidNameMatch(Asteroid a, string searchString)
        {
            var match = asteroidNameRegex.Match(a.Name);
            if (match.Success)
            {
                if (match.Groups[1].Value.StartsWith(searchString, StringComparison.OrdinalIgnoreCase))
                    return true;

                if (match.Groups[2].Value.StartsWith(searchString, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return a.Name.StartsWith(searchString, StringComparison.OrdinalIgnoreCase);
        }
    }
}
