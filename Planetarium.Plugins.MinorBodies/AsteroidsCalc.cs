using ADK;
using Planetarium.Objects;
using Planetarium.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Planetarium.Plugins.MinorBodies
{
    public class AsteroidsCalc : MinorBodyCalc, ICelestialObjectCalc<Asteroid>
    {
        private readonly string ORBITAL_ELEMENTS_FILE = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/Asteroids.dat");
        private readonly string SIZES_FILE = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/AsteroidsSizes.dat");
        private readonly Regex asteroidNameRegex = new Regex("\\((\\d+)\\)\\s*(\\w+)");
        private readonly AsteroidsReader reader = new AsteroidsReader();
        private readonly List<Asteroid> asteroids = new List<Asteroid>();

        public ICollection<Asteroid> Asteroids => asteroids;

        public override void Initialize()
        {
            asteroids.AddRange(reader.Read(ORBITAL_ELEMENTS_FILE, SIZES_FILE));
        }

        public override void Calculate(SkyContext c)
        {
            for (int i = 0; i < asteroids.Count; i++)
            {
                asteroids[i].Horizontal = c.Get(Horizontal, i);
                asteroids[i].Magnitude = c.Get(Magnitude, i);
                asteroids[i].Semidiameter = c.Get(Semidiameter, i);
            }
        }

        protected override OrbitalElements OrbitalElements(SkyContext c, int i)
        {
            return asteroids[i].Orbit;
        }

        private float Magnitude(SkyContext c, int i)
        {
            var a = asteroids[i];

            double delta = c.Get(DistanceFromEarth, i);
            double r = c.Get(DistanceFromSun, i);
            double beta = c.Get(PhaseAngle, i);

            return MinorBodyEphem.Magnitude(a.G, a.H, beta, r, delta);
        }

        private double Semidiameter(SkyContext c, int i)
        {
            var a = asteroids[i];
            double delta = c.Get(DistanceFromEarth, i);
            return MinorBodyEphem.Semidiameter(delta, a.PhysicalDiameter);
        }

        public void ConfigureEphemeris(EphemerisConfig<Asteroid> e)
        {
            e["Constellation"] = (c, a) => Constellations.FindConstellation(c.Get(EquatorialT, asteroids.IndexOf(a)), c.JulianDay);            
            e["Horizontal.Altitude"] = (c, p) => c.Get(Horizontal, asteroids.IndexOf(p)).Altitude;
            e["Horizontal.Azimuth"] = (c, p) => c.Get(Horizontal, asteroids.IndexOf(p)).Azimuth;
            e["Equatorial.Alpha"] = (c, p) => c.Get(EquatorialT, asteroids.IndexOf(p)).Alpha;
            e["Equatorial.Delta"] = (c, p) => c.Get(EquatorialT, asteroids.IndexOf(p)).Delta;
            e["Equatorial0.Alpha"] = (c, p) => c.Get(EquatorialG, asteroids.IndexOf(p)).Alpha;
            e["Equatorial0.Delta"] = (c, p) => c.Get(EquatorialG, asteroids.IndexOf(p)).Delta;
            e["RTS.Rise"] = (c, p) => c.GetDateFromTime(c.Get(RiseTransitSet, asteroids.IndexOf(p)).Rise);
            e["RTS.Transit"] = (c, p) => c.GetDateFromTime(c.Get(RiseTransitSet, asteroids.IndexOf(p)).Transit);
            e["RTS.Set"] = (c, p) => c.GetDateFromTime(c.Get(RiseTransitSet, asteroids.IndexOf(p)).Set);
            e["RTS.Duration"] = (c, p) => c.Get(RiseTransitSet, asteroids.IndexOf(p)).Duration;
            e["DistanceFromEarth"] = (c, a) => c.Get(DistanceFromEarth, asteroids.IndexOf(a));
            e["DistanceFromSun"] = (c, a) => c.Get(DistanceFromSun, asteroids.IndexOf(a));
            e["Phase"] = (c, a) => c.Get(Phase, asteroids.IndexOf(a));
            e["PhaseAngle"] = (c, a) => c.Get(PhaseAngle, asteroids.IndexOf(a));
            e["Magnitude"] = (c, p) => c.Get(Magnitude, asteroids.IndexOf(p));
            e["HorizontalParallax"] = (c, a) => c.Get(Parallax, asteroids.IndexOf(a));
            e["AngularDiameter", a => a.PhysicalDiameter > 0] = (c, a) => c.Get(Semidiameter, asteroids.IndexOf(a)) * 2 / 3600.0;
        }

        public void GetInfo(CelestialObjectInfo<Asteroid> info)
        {
            info
            .SetTitle(info.Body.Names.First())
            .SetSubtitle("Minor planet")
            .AddRow("Constellation")

            .AddHeader("Equatorial coordinates (topocentrical)")
            .AddRow("Equatorial.Alpha")
            .AddRow("Equatorial.Delta")

            .AddHeader("Equatorial coordinates (geocentrical)")
            .AddRow("Equatorial0.Alpha")
            .AddRow("Equatorial0.Delta")

            .AddHeader("Horizontal coordinates")
            .AddRow("Horizontal.Azimuth")
            .AddRow("Horizontal.Altitude")

            .AddHeader("Appearance")
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
            .AddHeader("Visibility")
            .AddRow("RTS.Rise")
            .AddRow("RTS.Transit")
            .AddRow("RTS.Set")
            .AddRow("RTS.Duration");
        }

        public ICollection<SearchResultItem> Search(SkyContext context, string searchString, int maxCount = 50)
        {
            return Asteroids
                .Where(a => IsAsteroidNameMatch(a, searchString))
                .Select(p => new SearchResultItem(p, p.Name)).ToArray();
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
