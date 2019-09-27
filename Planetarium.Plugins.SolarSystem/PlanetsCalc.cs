using ADK;
using Planetarium.Config;
using Planetarium.Objects;
using Planetarium.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Planetarium.Calculators
{
    public interface IPlanetsProvider
    {
        ICollection<JupiterMoon> JupiterMoons { get; }
        ICollection<Planet> Planets { get; }
        RingsAppearance SaturnRings { get; }
        double GreatRedSpotLongitude { get; }
    }

    public interface IPlanetsCalc
    {
        float Magnitude(SkyContext ctx, int number);
        double Elongation(SkyContext ctx, int number);
        double LongitudeDifference(SkyContext ctx, int number);
        double PhaseAngle(SkyContext ctx, int number);
        VisibilityDetails Visibility(SkyContext ctx, int number);
        CrdsEquatorial Equatorial(SkyContext ctx, int number);
        CrdsEcliptical Ecliptical(SkyContext ctx, int number);
        CrdsEcliptical SunEcliptical(SkyContext ctx);
        string GetPlanetName(int number);
    }

    public class PlanetsCalc : BaseCalc, ICelestialObjectCalc<Planet>, ICelestialObjectCalc<JupiterMoon>, IPlanetsCalc, IPlanetsProvider
    {
        private ISettings settings;
        private Planet[] planets = new Planet[8];
        private JupiterMoon[] jupiterMoons = new JupiterMoon[4];

        public ICollection<Planet> Planets => planets;
        public ICollection<JupiterMoon> JupiterMoons => jupiterMoons;
        public RingsAppearance SaturnRings { get; private set; } = new RingsAppearance();
        public double GreatRedSpotLongitude { get; private set; }

        private string[] PlanetNames = new string[]
        {
            "Mercury",
            "Venus",
            "Earth",
            "Mars",
            "Jupiter",
            "Saturn",
            "Uranus",
            "Neptune"
        };

        private string[] JuipterMoonNames = new string[] { "Io", "Europa", "Ganymede", "Callisto" };

        public PlanetsCalc(ISettings settings)
        {
            this.settings = settings;

            for (int i = 0; i < planets.Length; i++)
            {
                planets[i] = new Planet() { Number = i + 1, Name = PlanetNames[i] };
            }

            for (int i = 0; i < JupiterMoons.Count; i++)
            {
                jupiterMoons[i] = new JupiterMoon() { Number = i + 1, Name = JuipterMoonNames[i] };
            }

            planets[Planet.JUPITER - 1].Flattening = 0.064874f;
            planets[Planet.SATURN - 1].Flattening = 0.097962f;
        }

        public string GetPlanetName(int number)
        {
            return PlanetNames[number - 1];
        }

        /// <summary>
        /// Get heliocentrical coordinates of Earth
        /// </summary>
        private CrdsHeliocentrical EarthHeliocentrial(SkyContext c)
        {
            return PlanetPositions.GetPlanetCoordinates(Planet.EARTH, c.JulianDay, !c.PreferFastCalculation);
        }

        /// <summary>
        /// Gets ecliptical coordinates of Sun
        /// </summary>
        public CrdsEcliptical SunEcliptical(SkyContext c)
        {
            CrdsHeliocentrical hEarth = c.Get(EarthHeliocentrial);
            var sunEcliptical = new CrdsEcliptical(Angle.To360(hEarth.L + 180), -hEarth.B, hEarth.R);

            // Corrected solar coordinates to FK5 system
            sunEcliptical += PlanetPositions.CorrectionForFK5(c.JulianDay, sunEcliptical);

            // Add nutation effect to ecliptical coordinates of the Sun
            sunEcliptical += Nutation.NutationEffect(c.NutationElements.deltaPsi);

            // Add aberration effect, so we have an final ecliptical coordinates of the Sun 
            sunEcliptical += Aberration.AberrationEffect(sunEcliptical.Distance);

            return sunEcliptical;
        }

        /// <summary>
        /// Gets equatorial coordinates of the Sun
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private CrdsEquatorial SunEquatorial(SkyContext c)
        {
            return c.Get(SunEcliptical).ToEquatorial(c.Epsilon);
        }

        /// <summary>
        /// Gets heliocentrical coordinates of planet
        /// </summary>
        private CrdsHeliocentrical Heliocentrical(SkyContext c, int p)
        {
            // final difference to stop iteration process, 1 second of time
            double deltaTau = TimeSpan.FromSeconds(1).TotalDays;

            // time taken by the light to reach the Earth
            double tau = 0;

            // previous value of tau to calculate the difference
            double tau0 = 1;

            // Heliocentrical coordinates of planet
            CrdsHeliocentrical planetHeliocentrial = null;

            // Heliocentrical coordinates of Earth
            CrdsHeliocentrical hEarth = c.Get(EarthHeliocentrial);

            // Iterative process to find heliocentrical coordinates of planet
            while (Math.Abs(tau - tau0) > deltaTau)
            {
                // Heliocentrical coordinates of planet
                planetHeliocentrial = PlanetPositions.GetPlanetCoordinates(p, c.JulianDay - tau, !c.PreferFastCalculation);

                // Ecliptical coordinates of planet
                var planetEcliptical = planetHeliocentrial.ToRectangular(hEarth).ToEcliptical();

                tau0 = tau;
                tau = PlanetPositions.LightTimeEffect(planetEcliptical.Distance);
            }

            return planetHeliocentrial;
        }

        /// <summary>
        /// Gets ecliptical coordinates of Earth
        /// </summary>
        public CrdsEcliptical Ecliptical(SkyContext c, int p)
        {
            // Heliocentrical coordinates of planet
            CrdsHeliocentrical heliocentrical = c.Get(Heliocentrical, p);

            // Heliocentrical coordinates of Earth
            CrdsHeliocentrical hEarth = c.Get(EarthHeliocentrial);

            // Ecliptical coordinates of planet
            var ecliptical = heliocentrical.ToRectangular(hEarth).ToEcliptical();

            // Correction for FK5 system
            ecliptical += PlanetPositions.CorrectionForFK5(c.JulianDay, ecliptical);

            // Take nutation into account
            ecliptical += Nutation.NutationEffect(c.NutationElements.deltaPsi);

            return ecliptical;
        }

        /// <summary>
        /// Gets geocentrical equatorial coordinates of planet
        /// </summary>
        private CrdsEquatorial Equatorial0(SkyContext c, int p)
        {
            return c.Get(Ecliptical, p).ToEquatorial(c.Epsilon);
        }

        /// <summary>
        /// Gets distance from Earth to planet
        /// </summary>
        private double DistanceFromEarth(SkyContext c, int p)
        {
            return c.Get(Ecliptical, p).Distance;
        }

        /// <summary>
        /// Gets distance from planet to Sun
        /// </summary>
        private double DistanceFromSun(SkyContext c, int p)
        {
            return c.Get(Heliocentrical, p).R;
        }

        /// <summary>
        /// Gets visible semidianeter of planet
        /// </summary>
        private double Semidiameter(SkyContext c, int p)
        {
            return PlanetEphem.Semidiameter(p, c.Get(DistanceFromEarth, p));
        }

        /// <summary>
        /// Gets horizontal parallax of planet
        /// </summary>
        private double Parallax(SkyContext c, int p)
        {
            return PlanetEphem.Parallax(c.Get(DistanceFromEarth, p));
        }

        /// <summary>
        /// Gets apparent topocentric coordinates of planet
        /// </summary>
        public CrdsEquatorial Equatorial(SkyContext c, int p)
        {
            return c.Get(Equatorial0, p).ToTopocentric(c.GeoLocation, c.SiderealTime, c.Get(Parallax, p));
        }

        /// <summary>
        /// Gets apparent horizontal coordinates of planet
        /// </summary>
        private CrdsHorizontal Horizontal(SkyContext c, int p)
        {
            return c.Get(Equatorial, p).ToHorizontal(c.GeoLocation, c.SiderealTime);
        }

        /// <summary>
        /// Gets elongation angle for the planet
        /// </summary>
        public double Elongation(SkyContext c, int p)
        {
            return BasicEphem.Elongation(c.Get(SunEcliptical), c.Get(Ecliptical, p));
        }

        /// <summary>
        /// Gets difference between planet and Sun's ecliptical longitudes
        /// </summary>
        public double LongitudeDifference(SkyContext c, int p)
        {
            return BasicEphem.LongitudeDifference(c.Get(SunEcliptical).Lambda, c.Get(Ecliptical, p).Lambda);
        }

        /// <summary>
        /// Gets phase angle for the planet
        /// </summary>
        public double PhaseAngle(SkyContext c, int p)
        {
            return BasicEphem.PhaseAngle(c.Get(Elongation, p), c.Get(SunEcliptical).Distance, c.Get(DistanceFromEarth, p));
        }

        /// <summary>
        /// Gets phase for the planet
        /// </summary>
        private double Phase(SkyContext c, int p)
        {
            return BasicEphem.Phase(c.Get(PhaseAngle, p));
        } 

        /// <summary>
        /// Gets visible magnitude of the planet
        /// </summary>
        public float Magnitude(SkyContext c, int p)
        {
            float mag = PlanetEphem.Magnitude(p, c.Get(DistanceFromEarth, p), c.Get(DistanceFromSun, p), c.Get(PhaseAngle, p));
            if (p == Planet.SATURN)
            {
                var saturnRings = PlanetEphem.SaturnRings(c.JulianDay, c.Get(Heliocentrical, p), c.Get(EarthHeliocentrial), c.Epsilon);
                mag += saturnRings.GetRingsMagnitude();
            }

            return mag;
        }

        /// <summary>
        /// Gets visual appearance for the planet
        /// </summary>
        private PlanetAppearance Appearance(SkyContext c, int p)
        {
            return PlanetEphem.PlanetAppearance(c.JulianDay, p, c.Get(Equatorial0, p), c.Get(DistanceFromEarth, p));
        }

        /// <summary>
        /// Gets rise, transit and set info for the planet
        /// </summary>
        private RTS RiseTransitSet(SkyContext c, int p)
        {
            double jd = c.JulianDayMidnight;
            double theta0 = Date.ApparentSiderealTime(jd, c.NutationElements.deltaPsi, c.Epsilon);
            double parallax = c.Get(Parallax, p);

            CrdsEquatorial[] eq = new CrdsEquatorial[3];
            double[] diff = new double[] { 0, 0.5, 1 };

            for (int i = 0; i < 3; i++)
            {
                eq[i] = new SkyContext(jd + diff[i], c.GeoLocation).Get(Equatorial0, p);
            }

            return ADK.Visibility.RiseTransitSet(eq, c.GeoLocation, theta0, parallax);
        }

        public VisibilityDetails Visibility(SkyContext c, int p)
        {
            double jd = c.JulianDayMidnight;
            double theta0 = Date.ApparentSiderealTime(jd, c.NutationElements.deltaPsi, c.Epsilon);
            double parallax = c.Get(Parallax, p);

            var ctx = new SkyContext(jd, c.GeoLocation);
            
            var eq = ctx.Get(Equatorial, p);
            var eqSun = ctx.Get(SunEquatorial);

            return ADK.Visibility.Details(eq, eqSun, c.GeoLocation, theta0, 5);
        }

        public override void Calculate(SkyContext context)
        {
            foreach (var p in planets)
            {
                if (p.Number == Planet.EARTH) continue;

                int n = p.Number;

                p.Equatorial = context.Get(Equatorial, n);
                p.Horizontal = context.Get(Horizontal, n);
                p.Appearance = context.Get(Appearance, n);
                p.Magnitude = context.Get(Magnitude, n);
                p.DistanceFromSun = context.Get(DistanceFromSun, n);
                p.Semidiameter = context.Get(Semidiameter, n);
                p.Phase = context.Get(Phase, n);
                p.Elongation = context.Get(Elongation, n);
                p.Ecliptical = context.Get(Ecliptical, n);

                if (p.Number == Planet.JUPITER)
                {
                    foreach (var j in JupiterMoons)
                    {
                        int m = j.Number;
                        j.Rectangular = context.Get(JupiterMoonRectangular, m);
                        j.RectangularS = context.Get(JupiterMoonRectangularS, m);
                        j.Equatorial = context.Get(JupiterMoonEquatorial, m);
                        j.Horizontal = context.Get(JupiterMoonHorizontal, m);
                        j.Semidiameter = context.Get(JupiterMoonSemidiameter, m);
                        j.CM = context.Get(JupiterMoonCentralMeridian, m);
                        j.Magnitude = context.Get(JupiterMoonMagnitude, m);
                    }

                    GreatRedSpotLongitude = context.Get(JupiterGreatRedSpotLongitude);
                }

                if (p.Number == Planet.SATURN)
                {
                    SaturnRings = context.Get(GetSaturnRings, n);
                }
            }
        }

        private RingsAppearance GetSaturnRings(SkyContext c, int p)
        {
            return PlanetEphem.SaturnRings(c.JulianDay, c.Get(Heliocentrical, p), c.Get(EarthHeliocentrial), c.Epsilon);
        }

        private CrdsRectangular JupiterMoonRectangular(SkyContext c, int m)
        {
            return c.Get(JupiterMoonsPositions)[m - 1, 0];
        }

        private CrdsRectangular JupiterMoonRectangularS(SkyContext c, int m)
        {
            return c.Get(JupiterMoonsPositions)[m - 1, 1];
        }

        private CrdsRectangular[,] JupiterMoonsPositions(SkyContext c)
        {
            CrdsHeliocentrical earth = c.Get(EarthHeliocentrial);
            CrdsHeliocentrical jupiter = c.Get(Heliocentrical, Planet.JUPITER);
            return GalileanMoons.Positions(c.JulianDay, earth, jupiter);
        }

        private CrdsEquatorial JupiterMoonEquatorial(SkyContext c, int m)
        {
            CrdsEquatorial jupiterEq = c.Get(Equatorial, Planet.JUPITER);
            CrdsRectangular planetocentric = c.Get(JupiterMoonRectangular, m);
            PlanetAppearance appearance = c.Get(Appearance, Planet.JUPITER);
            double semidiameter = c.Get(Semidiameter, Planet.JUPITER);
            return planetocentric.ToEquatorial(jupiterEq, appearance.P, semidiameter);            
        }

        private CrdsHorizontal JupiterMoonHorizontal(SkyContext c, int m)
        {
            return c.Get(JupiterMoonEquatorial, m).ToHorizontal(c.GeoLocation, c.SiderealTime);
        }

        private double JupiterMoonSemidiameter(SkyContext c, int m)
        {
            // distance from Earth to Jupiter, in a.u.
            double r = c.Get(DistanceFromEarth, Planet.JUPITER);

            // planetocentric z-coordinate of moon
            double z = c.Get(JupiterMoonRectangular, m).Z;

            // visible moon semidiameter
            return GalileanMoons.MoonSemidiameter(r, z, m - 1);
        }

        private double JupiterMoonCentralMeridian(SkyContext c, int m)
        {
            // planetocentric rectangular coordinates of the moon
            CrdsRectangular r = c.Get(JupiterMoonRectangular, m);

            return GalileanMoons.MoonCentralMeridian(r, m - 1);
        }

        private float JupiterMoonMagnitude(SkyContext c, int m)
        {
            double r = c.Get(DistanceFromEarth, Planet.JUPITER);
            double R = c.Get(DistanceFromSun, Planet.JUPITER);
            double p = c.Get(Phase, Planet.JUPITER);
            return GalileanMoons.Magnitude(r, R, p, m - 1);
        }

        private double JupiterGreatRedSpotLongitude(SkyContext c)
        {
            var grsSettings = settings.Get<GreatRedSpotSettings>("GRSLongitude");
            return PlanetEphem.GreatRedSpotLongitude(c.JulianDay, grsSettings);
        }

        public void ConfigureEphemeris(EphemerisConfig<Planet> e)
        {
            e.Add("Magnitude", (c, p) => c.Get(Magnitude, p.Number));
            e.Add("Horizontal.Altitude", (c, p) => c.Get(Horizontal, p.Number).Altitude);
            e.Add("Horizontal.Azimuth", (c, p) => c.Get(Horizontal, p.Number).Azimuth);
            e.Add("Equatorial.Alpha", (c, p) => c.Get(Equatorial, p.Number).Alpha);
            e.Add("Equatorial.Delta", (c, p) => c.Get(Equatorial, p.Number).Delta);

            e.Add("SaturnRings.a", (c, p) => c.Get(GetSaturnRings, p.Number).a)
                .AvailableIf(p => (p is Planet) && (p as Planet).Number == Planet.SATURN);

            e.Add("SaturnRings.b", (c, p) => c.Get(GetSaturnRings, p.Number).b)
                .AvailableIf(p => (p is Planet) && (p as Planet).Number == Planet.SATURN);

            e.Add("GRSLongitude", (c, p) => c.Get(JupiterGreatRedSpotLongitude))
                .AvailableIf(p => (p is Planet) && (p as Planet).Number == Planet.JUPITER);

            e.Add("RTS.Rise", (c, p) => c.Get(RiseTransitSet, p.Number).Rise);
            e.Add("RTS.Transit", (c, p) => c.Get(RiseTransitSet, p.Number).Transit);
            e.Add("RTS.Set", (c, p) => c.Get(RiseTransitSet, p.Number).Set);

            e.Add("Visibility.Duration", (c, p) => c.Get(Visibility, p.Number).Duration);
            e.Add("Visibility.Period", (c, p) => c.Get(Visibility, p.Number).Period);
        }

        public void ConfigureEphemeris(EphemerisConfig<JupiterMoon> e)
        {
            e.Add("Rectangular.X", (c, j) => c.Get(JupiterMoonRectangular, j.Number).X);
            e.Add("Rectangular.Y", (c, j) => c.Get(JupiterMoonRectangular, j.Number).Y);
            e.Add("Rectangular.Z", (c, j) => c.Get(JupiterMoonRectangular, j.Number).Z);
            e.Add("Magnitude", (c, j) => c.Get(JupiterMoonMagnitude, j.Number));
        }

        public CelestialObjectInfo GetInfo(SkyContext c, Planet planet)
        {
            int p = planet.Number;

            var rts = c.Get(RiseTransitSet, p);
            var vis = c.Get(Visibility, p);

            var info = new CelestialObjectInfo();
            info.SetSubtitle("Planet").SetTitle(planet.Names.First())

            .AddRow("Constellation", Constellations.FindConstellation(c.Get(Equatorial, p), c.JulianDay))

            .AddHeader("Equatorial coordinates (geocentrical)")
            .AddRow("Equatorial0.Alpha", c.Get(Equatorial0, p).Alpha)
            .AddRow("Equatorial0.Delta", c.Get(Equatorial0, p).Delta)

            .AddHeader("Equatorial coordinates (topocentrical)")
            .AddRow("Equatorial.Alpha", c.Get(Equatorial, p).Alpha)
            .AddRow("Equatorial.Delta", c.Get(Equatorial, p).Delta)

            .AddHeader("Ecliptical coordinates")
            .AddRow("Ecliptical.Lambda", c.Get(Ecliptical, p).Lambda)
            .AddRow("Ecliptical.Beta", c.Get(Ecliptical, p).Beta)

            .AddHeader("Horizontal coordinates")
            .AddRow("Horizontal.Azimuth", c.Get(Horizontal, p).Azimuth)
            .AddRow("Horizontal.Altitude", c.Get(Horizontal, p).Altitude)

            .AddHeader("Visibility")
            .AddRow("RTS.Rise", rts.Rise, c.JulianDayMidnight + rts.Rise)
            .AddRow("RTS.Transit", rts.Transit, c.JulianDayMidnight + rts.Transit)
            .AddRow("RTS.Set", rts.Set, c.JulianDayMidnight + rts.Set)

            .AddRow("Visibility.Period", vis.Period)
            .AddRow("Visibility.Begin", vis.Begin, c.JulianDayMidnight + vis.Begin)
            .AddRow("Visibility.End", vis.End, c.JulianDayMidnight + vis.End)
            .AddRow("Visibility.Duration", vis.Duration)

            .AddHeader("Appearance")
            .AddRow("Phase", c.Get(Phase, p))
            .AddRow("PhaseAngle", c.Get(PhaseAngle, p))
            .AddRow("Magnitude", c.Get(Magnitude, p))
            .AddRow("DistanceFromEarth", c.Get(DistanceFromEarth, p))
            .AddRow("DistanceFromSun", c.Get(DistanceFromSun, p))
            .AddRow("HorizontalParallax", c.Get(Parallax, p))
            .AddRow("AngularDiameter", c.Get(Semidiameter, p) * 2 / 3600.0);

            if (p == Planet.SATURN)
            {
                info
                .AddRow("SaturnRings.a", c.Get(GetSaturnRings, p).a)
                .AddRow("SaturnRings.b", c.Get(GetSaturnRings, p).b);
            }
            else if (p == Planet.JUPITER)
            {
                info.AddRow("GRSLongitude", c.Get(JupiterGreatRedSpotLongitude));
            }

            info
            .AddRow("Appearance.CM", c.Get(Appearance, p).CM)
            .AddRow("Appearance.P", c.Get(Appearance, p).P)
            .AddRow("Appearance.D", c.Get(Appearance, p).D);

            return info;
        }

        public CelestialObjectInfo GetInfo(SkyContext c, JupiterMoon moon)
        {
            int p = Planet.JUPITER;
            int m = moon.Number;

            var rts = c.Get(RiseTransitSet, p);

            var info = new CelestialObjectInfo();
            info.SetSubtitle("Satellite of Jupiter").SetTitle(moon.Names.First())

            .AddRow("Constellation", Constellations.FindConstellation(c.Get(JupiterMoonEquatorial, m), c.JulianDay))

            .AddHeader("Equatorial coordinates (topocentrical)")
            .AddRow("Equatorial.Alpha", c.Get(JupiterMoonEquatorial, m).Alpha)
            .AddRow("Equatorial.Delta", c.Get(JupiterMoonEquatorial, m).Delta)

            .AddHeader("Horizontal coordinates")
            .AddRow("Horizontal.Azimuth", c.Get(JupiterMoonHorizontal, m).Azimuth)
            .AddRow("Horizontal.Altitude", c.Get(JupiterMoonHorizontal, m).Altitude)

            .AddHeader("Rectangular planetocentric coordinates")
            .AddRow("Rectangular.X", c.Get(JupiterMoonRectangular, m).X)
            .AddRow("Rectangular.Y", c.Get(JupiterMoonRectangular, m).Y)
            .AddRow("Rectangular.Z", c.Get(JupiterMoonRectangular, m).Z)

            .AddHeader("Visibility")
            .AddRow("RTS.Rise", rts.Rise, c.JulianDayMidnight + rts.Rise)
            .AddRow("RTS.Transit", rts.Transit, c.JulianDayMidnight + rts.Transit)
            .AddRow("RTS.Set", rts.Set, c.JulianDayMidnight + rts.Set)
            .AddRow("RTS.Duration", rts.Duration)

            .AddHeader("Appearance")
            .AddRow("Phase", c.Get(Phase, p))
            .AddRow("PhaseAngle", c.Get(PhaseAngle, p))
            .AddRow("Magnitude", c.Get(JupiterMoonMagnitude, m))
            //.AddRow("DistanceFromEarth", c.Get(DistanceFromEarth, p))
            //.AddRow("DistanceFromSun", c.Get(DistanceFromSun, p))
            //.AddRow("HorizontalParallax", c.Get(Parallax, p))
            .AddRow("AngularDiameter", c.Get(JupiterMoonSemidiameter, m) * 2 / 3600.0)
            .AddRow("Appearance.CM", c.Get(JupiterMoonCentralMeridian, m));

            return info;
        }

        public ICollection<SearchResultItem> Search(SkyContext context, string searchString, int maxCount = 50)
        {
            var s1 = planets.Where(p => p.Number != Planet.EARTH && p.Name.StartsWith(searchString, StringComparison.OrdinalIgnoreCase))
                .Select(p => new SearchResultItem(p, p.Name));

            var s2 = jupiterMoons.Where(m => m.Name.StartsWith(searchString, StringComparison.OrdinalIgnoreCase))
                .Select(p => new SearchResultItem(p, p.Name));

            return s1.Concat(s2).ToArray();
        }
    }
}
