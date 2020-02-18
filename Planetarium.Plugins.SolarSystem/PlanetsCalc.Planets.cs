using ADK;
using Planetarium.Objects;
using Planetarium.Types;
using Planetarium.Types.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Plugins.SolarSystem
{
    public partial class PlanetsCalc
    {
        /// <summary>
        /// Get heliocentrical coordinates of Earth
        /// </summary>
        private CrdsHeliocentrical Earth_Heliocentrial(SkyContext c)
        {
            return PlanetPositions.GetPlanetCoordinates(Planet.EARTH, c.JulianDay, !c.PreferFastCalculation);
        }

        /// <summary>
        /// Gets ecliptical coordinates of Sun
        /// </summary>
        public CrdsEcliptical Sun_Ecliptical(SkyContext c)
        {
            CrdsHeliocentrical hEarth = c.Get(Earth_Heliocentrial);
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
        private CrdsEquatorial Sun_Equatorial(SkyContext c)
        {
            return c.Get(Sun_Ecliptical).ToEquatorial(c.Epsilon);
        }

        /// <summary>
        /// Gets heliocentrical coordinates of planet
        /// </summary>
        private CrdsHeliocentrical Planet_Heliocentrical(SkyContext c, int p)
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
            CrdsHeliocentrical hEarth = c.Get(Earth_Heliocentrial);

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
        /// Gets ecliptical coordinates of planet
        /// </summary>
        public CrdsEcliptical Planet_Ecliptical(SkyContext c, int p)
        {
            // Heliocentrical coordinates of planet
            CrdsHeliocentrical heliocentrical = c.Get(Planet_Heliocentrical, p);

            // Heliocentrical coordinates of Earth
            CrdsHeliocentrical hEarth = c.Get(Earth_Heliocentrial);

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
        private CrdsEquatorial Planet_Equatorial0(SkyContext c, int p)
        {
            return c.Get(Planet_Ecliptical, p).ToEquatorial(c.Epsilon);
        }

        /// <summary>
        /// Gets distance from Earth to planet
        /// </summary>
        private double Planet_DistanceFromEarth(SkyContext c, int p)
        {
            return c.Get(Planet_Ecliptical, p).Distance;
        }

        /// <summary>
        /// Gets distance from planet to Sun
        /// </summary>
        private double Planet_DistanceFromSun(SkyContext c, int p)
        {
            return c.Get(Planet_Heliocentrical, p).R;
        }

        /// <summary>
        /// Gets visible semidianeter of planet
        /// </summary>
        private double Planet_Semidiameter(SkyContext c, int p)
        {
            return PlanetEphem.Semidiameter(p, c.Get(Planet_DistanceFromEarth, p));
        }

        /// <summary>
        /// Gets horizontal parallax of planet
        /// </summary>
        private double Planet_Parallax(SkyContext c, int p)
        {
            return PlanetEphem.Parallax(c.Get(Planet_DistanceFromEarth, p));
        }

        /// <summary>
        /// Gets apparent topocentric coordinates of planet
        /// </summary>
        public CrdsEquatorial Planet_Equatorial(SkyContext c, int p)
        {
            return c.Get(Planet_Equatorial0, p).ToTopocentric(c.GeoLocation, c.SiderealTime, c.Get(Planet_Parallax, p));
        }

        /// <summary>
        /// Gets apparent horizontal coordinates of planet
        /// </summary>
        private CrdsHorizontal Planet_Horizontal(SkyContext c, int p)
        {
            return c.Get(Planet_Equatorial, p).ToHorizontal(c.GeoLocation, c.SiderealTime);
        }

        /// <summary>
        /// Gets elongation angle for the planet
        /// </summary>
        public double Planet_Elongation(SkyContext c, int p)
        {
            return BasicEphem.Elongation(c.Get(Sun_Ecliptical), c.Get(Planet_Ecliptical, p));
        }

        /// <summary>
        /// Gets difference between planet and Sun's ecliptical longitudes
        /// </summary>
        public double Planet_LongitudeDifference(SkyContext c, int p)
        {
            return BasicEphem.LongitudeDifference(c.Get(Sun_Ecliptical).Lambda, c.Get(Planet_Ecliptical, p).Lambda);
        }

        /// <summary>
        /// Gets phase angle for the planet
        /// </summary>
        public double Planet_PhaseAngle(SkyContext c, int p)
        {
            return BasicEphem.PhaseAngle(c.Get(Planet_Elongation, p), c.Get(Sun_Ecliptical).Distance, c.Get(Planet_DistanceFromEarth, p));
        }

        /// <summary>
        /// Gets phase for the planet
        /// </summary>
        private double Planet_Phase(SkyContext c, int p)
        {
            return BasicEphem.Phase(c.Get(Planet_PhaseAngle, p));
        }

        /// <summary>
        /// Gets visible magnitude of the planet
        /// </summary>
        public float Planet_Magnitude(SkyContext c, int p)
        {
            float mag = PlanetEphem.Magnitude(p, c.Get(Planet_DistanceFromEarth, p), c.Get(Planet_DistanceFromSun, p), c.Get(Planet_PhaseAngle, p));
            if (p == Planet.SATURN)
            {
                var saturnRings = PlanetEphem.SaturnRings(c.JulianDay, c.Get(Planet_Heliocentrical, p), c.Get(Earth_Heliocentrial), c.Epsilon);
                mag += saturnRings.GetRingsMagnitude();
            }

            return mag;
        }

        /// <summary>
        /// Gets visual appearance for the planet
        /// </summary>
        private PlanetAppearance Planet_Appearance(SkyContext c, int p)
        {
            return PlanetEphem.PlanetAppearance(c.JulianDay, p, c.Get(Planet_Equatorial0, p), c.Get(Planet_DistanceFromEarth, p));
        }

        /// <summary>
        /// Gets rise, transit and set info for the planet
        /// </summary>
        private RTS Planet_RiseTransitSet(SkyContext c, int p)
        {
            double jd = c.JulianDayMidnight;
            double theta0 = Date.ApparentSiderealTime(jd, c.NutationElements.deltaPsi, c.Epsilon);
            double parallax = c.Get(Planet_Parallax, p);

            CrdsEquatorial[] eq = new CrdsEquatorial[3];
            double[] diff = new double[] { 0, 0.5, 1 };

            for (int i = 0; i < 3; i++)
            {
                eq[i] = new SkyContext(jd + diff[i], c.GeoLocation).Get(Planet_Equatorial0, p);
            }

            return Visibility.RiseTransitSet(eq, c.GeoLocation, theta0, parallax);
        }

        public VisibilityDetails Planet_Visibility(SkyContext c, int p)
        {
            double jd = c.JulianDayMidnight;
            double theta0 = Date.ApparentSiderealTime(jd, c.NutationElements.deltaPsi, c.Epsilon);
            double parallax = c.Get(Planet_Parallax, p);

            var ctx = new SkyContext(jd, c.GeoLocation);

            var eq = ctx.Get(Planet_Equatorial, p);
            var eqSun = ctx.Get(Sun_Equatorial);

            return Visibility.Details(eq, eqSun, c.GeoLocation, theta0, 5);
        }

        private double Jupiter_GreatRedSpotLongitude(SkyContext c)
        {
            var grsSettings = settings.Get<GreatRedSpotSettings>("GRSLongitude");
            return PlanetEphem.GreatRedSpotLongitude(c.JulianDay, grsSettings);
        }

        private RingsAppearance Saturn_RingsAppearance(SkyContext c, int p)
        {
            return PlanetEphem.SaturnRings(c.JulianDay, c.Get(Planet_Heliocentrical, p), c.Get(Earth_Heliocentrial), c.Epsilon);
        }

        public void ConfigureEphemeris(EphemerisConfig<Planet> e)
        {
            e["Constellation"] = (c, p) => Constellations.FindConstellation(c.Get(Planet_Equatorial, p.Number), c.JulianDay);
            e["Equatorial.Alpha"] = (c, p) => c.Get(Planet_Equatorial, p.Number).Alpha;
            e["Equatorial.Delta"] = (c, p) => c.Get(Planet_Equatorial, p.Number).Delta;
            e["Equatorial0.Alpha"] = (c, p) => c.Get(Planet_Equatorial0, p.Number).Alpha;
            e["Equatorial0.Delta"] = (c, p) => c.Get(Planet_Equatorial0, p.Number).Delta;
            e["Ecliptical.Lambda"] = (c, p) => c.Get(Planet_Ecliptical, p.Number).Lambda;
            e["Ecliptical.Beta"] = (c, p) => c.Get(Planet_Ecliptical, p.Number).Beta;
            e["Horizontal.Altitude"] = (c, p) => c.Get(Planet_Horizontal, p.Number).Altitude;
            e["Horizontal.Azimuth"] = (c, p) => c.Get(Planet_Horizontal, p.Number).Azimuth;
            e["Magnitude"] = (c, p) => c.Get(Planet_Magnitude, p.Number);
            e["Phase"] = (c, p) => c.Get(Planet_Phase, p.Number);
            e["PhaseAngle"] = (c, p) => c.Get(Planet_PhaseAngle, p.Number);
            e["DistanceFromEarth"] = (c, p) => c.Get(Planet_DistanceFromEarth, p.Number);
            e["DistanceFromSun"] = (c, p) => c.Get(Planet_DistanceFromSun, p.Number);
            e["HorizontalParallax"] = (c, p) => c.Get(Planet_Parallax, p.Number);
            e["AngularDiameter"] = (c, p) => c.Get(Planet_Semidiameter, p.Number) * 2 / 3600.0;
            e["Appearance.CM"] = (c, p) => c.Get(Planet_Appearance, p.Number).CM;
            e["Appearance.D"] = (c, p) => c.Get(Planet_Appearance, p.Number).D;
            e["Appearance.P"] = (c, p) => c.Get(Planet_Appearance, p.Number).P;
            e["SaturnRings.a", IsSaturn] = (c, p) => c.Get(Saturn_RingsAppearance, p.Number).a;
            e["SaturnRings.b", IsSaturn] = (c, p) => c.Get(Saturn_RingsAppearance, p.Number).b;
            e["GRSLongitude", IsJupiter] = (c, p) => c.Get(Jupiter_GreatRedSpotLongitude);
            e["RTS.Rise"] = (c, p) => c.GetDateFromTime(c.Get(Planet_RiseTransitSet, p.Number).Rise);
            e["RTS.Transit"] = (c, p) => c.GetDateFromTime(c.Get(Planet_RiseTransitSet, p.Number).Transit);
            e["RTS.Set"] = (c, p) => c.GetDateFromTime(c.Get(Planet_RiseTransitSet, p.Number).Set);
            e["RTS.Duration"] = (c, p) => c.Get(Planet_RiseTransitSet, p.Number).Duration;
            e["Visibility.Begin"] = (c, p) => c.GetDateFromTime(c.Get(Planet_Visibility, p.Number).Begin);
            e["Visibility.End"] = (c, p) => c.GetDateFromTime(c.Get(Planet_Visibility, p.Number).End);
            e["Visibility.Duration"] = (c, p) => c.Get(Planet_Visibility, p.Number).Duration;
            e["Visibility.Period"] = (c, p) => c.Get(Planet_Visibility, p.Number).Period;
        }

        public void GetInfo(CelestialObjectInfo<Planet> info)
        {
            info
                .SetSubtitle("Planet")
                .SetTitle(info.Body.Names.First())

                .AddRow("Constellation")

                .AddHeader(Text.Get("Planet.Horizontal"))
                .AddRow("Horizontal.Azimuth")
                .AddRow("Horizontal.Altitude")

                .AddHeader(Text.Get("Planet.Equatorial"))
                .AddRow("Equatorial.Alpha")
                .AddRow("Equatorial.Delta")

                .AddHeader(Text.Get("Planet.Equatorial0"))
                .AddRow("Equatorial0.Alpha")
                .AddRow("Equatorial0.Delta")

                .AddHeader(Text.Get("Planet.Ecliptical"))
                .AddRow("Ecliptical.Lambda")
                .AddRow("Ecliptical.Beta")

                .AddHeader(Text.Get("Planet.RTS"))
                .AddRow("RTS.Rise")
                .AddRow("RTS.Transit")
                .AddRow("RTS.Set")
                .AddRow("RTS.Duration")

                .AddHeader(Text.Get("Planet.Visibility"))
                .AddRow("Visibility.Period")
                .AddRow("Visibility.Begin")
                .AddRow("Visibility.End")
                .AddRow("Visibility.Duration")

                .AddHeader(Text.Get("Planet.Appearance"))
                .AddRow("Phase")
                .AddRow("PhaseAngle")
                .AddRow("Magnitude")
                .AddRow("DistanceFromEarth")
                .AddRow("DistanceFromSun")
                .AddRow("HorizontalParallax")
                .AddRow("AngularDiameter")
                .AddRow("Appearance.CM")
                .AddRow("Appearance.P")
                .AddRow("Appearance.D");

            if (info.Body.Number == Planet.SATURN)
            {
                info
                    .AddHeader(Text.Get("SaturnRings"))
                    .AddRow("SaturnRings.a")
                    .AddRow("SaturnRings.b");
            }
            else if (info.Body.Number == Planet.JUPITER)
            {
                info
                    .AddRow("GRSLongitude");
            }
        }
    }
}
