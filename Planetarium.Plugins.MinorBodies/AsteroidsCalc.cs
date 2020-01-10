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
    public class AsteroidsCalc : MinorBodyCalc<Asteroid>, ICelestialObjectCalc<Asteroid>
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
            foreach (Asteroid a in asteroids)
            {
                a.Horizontal = c.Get(Horizontal, a);
                a.Magnitude = c.Get(Magnitude, a);
                a.Semidiameter = c.Get(Semidiameter, a);
            }
        }

        protected override OrbitalElements OrbitalElements(SkyContext c, Asteroid a)
        {
            return a.Orbit;
        }

        private float Magnitude(SkyContext c, Asteroid a)
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
            .SetSubtitle("Minor planet")
            .AddRow("Constellation")

            .AddHeader("Equatorial coordinates (topocentrical)")
            .AddRow("Equatorial.Alpha")
            .AddRow("Equatorial.Delta")

            .AddHeader("Equatorial coordinates (geocentrical)")
            .AddRow("Equatorial0.Alpha")
            .AddRow("Equatorial0.Delta")

            .AddHeader("Ecliptical coordinates ")
            .AddRow("Ecliptical.Lambda")
            .AddRow("Ecliptical.Beta")

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
