using ADK;
using Planetarium.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Planetarium.Calculators
{
    public interface IAsteroidsProvider
    {
        ICollection<Asteroid> Asteroids { get; }
    }

    public class AsteroidsCalc : MinorBodyCalc, ICelestialObjectCalc<Asteroid>, IAsteroidsProvider
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
            e.Add("Magnitude", (c, p) => c.Get(Magnitude, asteroids.IndexOf(p)));
            e.Add("Horizontal.Altitude", (c, p) => c.Get(Horizontal, asteroids.IndexOf(p)).Altitude);
            e.Add("Horizontal.Azimuth", (c, p) => c.Get(Horizontal, asteroids.IndexOf(p)).Azimuth);
            e.Add("Equatorial.Alpha", (c, p) => c.Get(EquatorialT, asteroids.IndexOf(p)).Alpha);
            e.Add("Equatorial.Delta", (c, p) => c.Get(EquatorialT, asteroids.IndexOf(p)).Delta);
            e.Add("Equatorial0.Alpha", (c, p) => c.Get(EquatorialG, asteroids.IndexOf(p)).Alpha);
            e.Add("Equatorial0.Delta", (c, p) => c.Get(EquatorialG, asteroids.IndexOf(p)).Delta);

            e.Add("RTS.Rise", (c, p) => c.Get(RiseTransitSet, asteroids.IndexOf(p)).Rise);
            e.Add("RTS.Transit", (c, p) => c.Get(RiseTransitSet, asteroids.IndexOf(p)).Transit);
            e.Add("RTS.Set", (c, p) => c.Get(RiseTransitSet, asteroids.IndexOf(p)).Set);
        }

        public CelestialObjectInfo GetInfo(SkyContext c, Asteroid body)
        {
            int i = asteroids.IndexOf(body);

            var rts = c.Get(RiseTransitSet, i);

            var info = new CelestialObjectInfo();

            info.SetSubtitle("Minor planet").SetTitle(GetName(body))
            .AddRow("Constellation", Constellations.FindConstellation(c.Get(EquatorialT, i), c.JulianDay))

            .AddHeader("Equatorial coordinates (topocentrical)")
            .AddRow("Equatorial.Alpha", c.Get(EquatorialT, i).Alpha)
            .AddRow("Equatorial.Delta", c.Get(EquatorialT, i).Delta)

            .AddHeader("Equatorial coordinates (geocentrical)")
            .AddRow("Equatorial0.Alpha", c.Get(EquatorialG, i).Alpha)
            .AddRow("Equatorial0.Delta", c.Get(EquatorialG, i).Delta)

            .AddHeader("Horizontal coordinates")
            .AddRow("Horizontal.Azimuth", c.Get(Horizontal, i).Azimuth)
            .AddRow("Horizontal.Altitude", c.Get(Horizontal, i).Altitude)

            .AddHeader("Appearance")
            .AddRow("Phase", c.Get(Phase, i))
            .AddRow("PhaseAngle", c.Get(PhaseAngle, i))
            .AddRow("Magnitude", c.Get(Magnitude, i))
            .AddRow("DistanceFromEarth", c.Get(DistanceFromEarth, i))
            .AddRow("DistanceFromSun", c.Get(DistanceFromSun, i))
            .AddRow("HorizontalParallax", c.Get(Parallax, i));

            if (body.PhysicalDiameter > 0)
            { 
                info.AddRow("AngularDiameter", c.Get(Semidiameter, i) * 2 / 3600.0);
            }

            info
            .AddHeader("Visibility")
            .AddRow("RTS.Rise", rts.Rise, c.JulianDayMidnight + rts.Rise)
            .AddRow("RTS.Transit", rts.Transit, c.JulianDayMidnight + rts.Transit)
            .AddRow("RTS.Set", rts.Set, c.JulianDayMidnight + rts.Set);

            return info;
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

        public string GetName(Asteroid body)
        {
            return body.Name;
        }
    }
}
