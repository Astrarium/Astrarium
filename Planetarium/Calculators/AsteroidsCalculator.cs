using ADK;
using Planetarium.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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

        private AsteroidsReader reader = new AsteroidsReader();

        private List<Asteroid> asteroids = new List<Asteroid>();
        public ICollection<Asteroid> Asteroids => asteroids;

        public override void Initialize()
        {
            asteroids.AddRange(reader.Read(ORBITAL_ELEMENTS_FILE));
        }

        public override void Calculate(SkyContext c)
        {
            for (int i = 0; i < Asteroids.Count; i++)
            {
                var eq = c.Get(Equatorial, i);

                // Apparent horizontal coordinates
                Asteroids.ElementAt(i).Horizontal = eq.ToHorizontal(c.GeoLocation, c.SiderealTime);

                Asteroids.ElementAt(i).Magnitude = 1;
            }
        }

        private CrdsEquatorial Equatorial(SkyContext c, int i)
        {
            // final difference to stop iteration process, 1 second of time
            double deltaTau = TimeSpan.FromSeconds(1).TotalDays;

            // time taken by the light to reach the Earth
            double tau = 0;

            // previous value of tau to calculate the difference
            double tau0 = 1;

            // Rectangular coordinates of asteroid
            CrdsRectangular rect = null;

            // Rectangular coordinates of sun
            var sun = c.Get(SunRectangular);

            double ksi = 0, eta = 0, zeta = 0, Delta = 0;

            // Iterative process to find heliocentrical coordinates of planet
            while (Math.Abs(tau - tau0) > deltaTau)
            {
                // Rectangular coordinates of asteroid
                rect = MinorBodyPositions.GetRectangularCoordinates(Asteroids.ElementAt(i).Orbit, c.JulianDay - tau, c.Epsilon);

                ksi = sun.X + rect.X;
                eta = sun.Y + rect.Y;
                zeta = sun.Z + rect.Z;

                // Distance to the Earth
                Delta = Math.Sqrt(ksi * ksi + eta * eta + zeta * zeta);

                tau0 = tau;
                tau = PlanetPositions.LightTimeEffect(Delta);
            }

            double alpha = Angle.ToDegrees(Math.Atan2(eta, ksi));
            double delta = Angle.ToDegrees(Math.Asin(zeta / Delta));

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
        /// Gets rectangular coordinates of Sun
        /// </summary>
        private CrdsRectangular SunRectangular(SkyContext c)
        {
            CrdsHeliocentrical hEarth = PlanetPositions.GetPlanetCoordinates(Planet.EARTH, c.JulianDay, !c.PreferFastCalculation);
            
            var eSun = new CrdsEcliptical(Angle.To360(hEarth.L + 180), -hEarth.B, hEarth.R);

            // Corrected solar coordinates to FK5 system
            eSun += PlanetPositions.CorrectionForFK5(c.JulianDay, eSun);

            // NO correction for nutation and aberration should be performed here (ch. 26, p. 171)

            // Add nutation effect to ecliptical coordinates of the Sun
            //sunEcliptical += Nutation.NutationEffect(c.NutationElements.deltaPsi);

            // Add aberration effect, so we have an final ecliptical coordinates of the Sun 
            //sunEcliptical += Aberration.AberrationEffect(sunEcliptical.Distance);

            return eSun.ToRectangular(c.Epsilon);
        }

        public void ConfigureEphemeris(EphemerisConfig<Asteroid> config)
        {
            
        }

        public CelestialObjectInfo GetInfo(SkyContext c, Asteroid body)
        {
            int i = asteroids.IndexOf(body);

            var info = new CelestialObjectInfo();

            info.SetSubtitle("Minor planet").SetTitle(GetName(body))

            //.AddRow("Constellation", Constellations.FindConstellation(c.Get(JupiterMoonEquatorial, m), c.JulianDay))

            .AddHeader("Equatorial coordinates (geocentrical)")
            .AddRow("Equatorial.Alpha", c.Get(Equatorial, i).Alpha)
            .AddRow("Equatorial.Delta", c.Get(Equatorial, i).Delta);

            return info;
        }

        public ICollection<SearchResultItem> Search(string searchString, int maxCount = 50)
        {
            return Asteroids
                .Where(a => a.Name.StartsWith(searchString, StringComparison.OrdinalIgnoreCase))
                .Select(p => new SearchResultItem(p, p.Name)).ToArray();
        }

        public string GetName(Asteroid body)
        {
            return body.Name;
        }
    }
}
