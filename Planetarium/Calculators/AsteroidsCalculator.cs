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

    public class AsteroidsCalculator : BaseCalc, ICelestialObjectCalc<Asteroid>, IAsteroidsProvider
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

        private float Magnitude(SkyContext c, int i)
        {
            var a = asteroids[i];

            double delta = c.Get(DistanceFromEarth, i);
            double r = c.Get(DistanceFromSun, i);
            double beta = c.Get(PhaseAngle, i);

            return MinorBodyEphem.Magnitude(a.G, a.H, beta, r, delta);
        }

        private double Phase(SkyContext c, int i)
        {
            return BasicEphem.Phase(c.Get(PhaseAngle, i));
        }

        private double PhaseAngle(SkyContext c, int i)
        {
            double delta = c.Get(DistanceFromEarth, i);
            double r = c.Get(DistanceFromSun, i);
            double R = c.Get(EarthDistanceFromSun);

            return MinorBodyEphem.PhaseAngle(r, delta, R);
        }

        private double Semidiameter(SkyContext c, int i)
        {
            var a = asteroids[i];
            double delta = c.Get(DistanceFromEarth, i);
            return MinorBodyEphem.Semidiameter(delta, a.PhysicalDiameter);
        }

        private CrdsRectangular Rectangular(SkyContext c, int i)
        {
            // final difference to stop iteration process, 1 second of time
            double deltaTau = TimeSpan.FromSeconds(1).TotalDays;

            // time taken by the light to reach the Earth
            double tau = 0;

            // previous value of tau to calculate the difference
            double tau0 = 1;

            // Rectangular coordinates of asteroid
            CrdsRectangular rect = null;

            // Rectangular coordinates of the Sun
            var sun = c.Get(SunRectangular);

            double ksi = 0, eta = 0, zeta = 0, Delta = 0;

            // Iterative process to find rectangular coordinates of asteroid
            while (Math.Abs(tau - tau0) > deltaTau)
            {
                // Rectangular coordinates of asteroid
                rect = MinorBodyPositions.GetRectangularCoordinates(asteroids[i].Orbit, c.JulianDay - tau, c.Epsilon);

                ksi = sun.X + rect.X;
                eta = sun.Y + rect.Y;
                zeta = sun.Z + rect.Z;

                // Distance to the Earth
                Delta = Math.Sqrt(ksi * ksi + eta * eta + zeta * zeta);

                tau0 = tau;
                tau = PlanetPositions.LightTimeEffect(Delta);
            }

            return rect;
        }

        private double DistanceFromEarth(SkyContext c, int i)
        {
            var rAsteroid = c.Get(Rectangular, i);
            var rSun = c.Get(SunRectangular);

            double x = rSun.X + rAsteroid.X;
            double y = rSun.Y + rAsteroid.Y;
            double z = rSun.Z + rAsteroid.Z;

            return Math.Sqrt(x * x + y * y + z * z);
        }

        private double DistanceFromSun(SkyContext c, int i)
        {
            var r = c.Get(Rectangular, i);
            return Math.Sqrt(r.X * r.X + r.Y * r.Y + r.Z * r.Z);
        }

        private CrdsEquatorial Equatorial0(SkyContext c, int i)
        {
            var Delta = c.Get(DistanceFromEarth, i);
            var rAsteroid = c.Get(Rectangular, i);
            var rSun = c.Get(SunRectangular);
            
            double x = rSun.X + rAsteroid.X;
            double y = rSun.Y + rAsteroid.Y;
            double z = rSun.Z + rAsteroid.Z;

            double alpha = Angle.ToDegrees(Math.Atan2(y, x));
            double delta = Angle.ToDegrees(Math.Asin(z / Delta));

            var eq = new CrdsEquatorial(alpha, delta);
           
            // Nutation effect
            var eq1 = Nutation.NutationEffect(eq, c.NutationElements, c.Epsilon);

            // Aberration effect
            var eq2 = Aberration.AberrationEffect(eq, c.AberrationElements, c.Epsilon);

            // Apparent coordinates of the object
            eq += eq1 + eq2;

            return eq;
        }

        /// <summary>
        /// Gets horizontal parallax of asteroid
        /// </summary>
        private double Parallax(SkyContext c, int i)
        {
            return PlanetEphem.Parallax(c.Get(DistanceFromEarth, i));
        }

        private CrdsEquatorial Equatorial(SkyContext c, int i)
        {
            var eq0 = c.Get(Equatorial0, i);
            var parallax = c.Get(Parallax, i);
            return eq0.ToTopocentric(c.GeoLocation, c.SiderealTime, parallax);
        }

        private CrdsHorizontal Horizontal(SkyContext c, int i)
        {
            var eq = c.Get(Equatorial, i);
            return eq.ToHorizontal(c.GeoLocation, c.SiderealTime);
        }

        /// <summary>
        /// Gets rectangular coordinates of Sun
        /// </summary>
        private CrdsRectangular SunRectangular(SkyContext c)
        {
            CrdsHeliocentrical hEarth = PlanetPositions.GetPlanetCoordinates(Planet.EARTH, c.JulianDay, !c.PreferFastCalculation);
            
            var eSun = new CrdsEcliptical(Angle.To360(hEarth.L + 180), -hEarth.B, hEarth.R);

            // Corrected solar coordinates to FK5 system
            // NO correction for nutation and aberration should be performed here (ch. 26, p. 171)
            eSun += PlanetPositions.CorrectionForFK5(c.JulianDay, eSun);

            return eSun.ToRectangular(c.Epsilon);
        }

        private double EarthDistanceFromSun(SkyContext c)
        {
            var r = c.Get(SunRectangular);
            return Math.Sqrt(r.X * r.X + r.Y * r.Y + r.Z * r.Z);
        }

        /// <summary>
        /// Gets rise, transit and set info for the planet
        /// </summary>
        private RTS RiseTransitSet(SkyContext c, int a)
        {
            double jd = c.JulianDayMidnight;
            double theta0 = Date.ApparentSiderealTime(jd, c.NutationElements.deltaPsi, c.Epsilon);
            double parallax = c.Get(Parallax, a);

            CrdsEquatorial[] eq = new CrdsEquatorial[3];
            double[] diff = new double[] { 0, 0.5, 1 };

            for (int i = 0; i < 3; i++)
            {
                eq[i] = new SkyContext(jd + diff[i], c.GeoLocation).Get(Equatorial0, a);
            }

            return Visibility.RiseTransitSet(eq, c.GeoLocation, theta0, parallax);
        }

        public void ConfigureEphemeris(EphemerisConfig<Asteroid> e)
        {
            e.Add("Magnitude", (c, p) => c.Get(Magnitude, asteroids.IndexOf(p)));
            e.Add("Horizontal.Altitude", (c, p) => c.Get(Horizontal, asteroids.IndexOf(p)).Altitude);
            e.Add("Horizontal.Azimuth", (c, p) => c.Get(Horizontal, asteroids.IndexOf(p)).Azimuth);
            e.Add("Equatorial.Alpha", (c, p) => c.Get(Equatorial, asteroids.IndexOf(p)).Alpha);
            e.Add("Equatorial.Delta", (c, p) => c.Get(Equatorial, asteroids.IndexOf(p)).Delta);
            e.Add("Equatorial0.Alpha", (c, p) => c.Get(Equatorial0, asteroids.IndexOf(p)).Alpha);
            e.Add("Equatorial0.Delta", (c, p) => c.Get(Equatorial0, asteroids.IndexOf(p)).Delta);

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
            .AddRow("Constellation", Constellations.FindConstellation(c.Get(Equatorial, i), c.JulianDay))

            .AddHeader("Equatorial coordinates (topocentrical)")
            .AddRow("Equatorial.Alpha", c.Get(Equatorial, i).Alpha)
            .AddRow("Equatorial.Delta", c.Get(Equatorial, i).Delta)

            .AddHeader("Equatorial coordinates (geocentrical)")
            .AddRow("Equatorial0.Alpha", c.Get(Equatorial0, i).Alpha)
            .AddRow("Equatorial0.Delta", c.Get(Equatorial0, i).Delta)

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
