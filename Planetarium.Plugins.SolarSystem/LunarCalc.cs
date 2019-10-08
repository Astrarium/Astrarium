using ADK;
using Planetarium.Objects;
using Planetarium.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Calculators
{
    public class LunarCalc : BaseCalc, ICelestialObjectCalc<Moon>
    {
        public Moon Moon { get; private set; } = new Moon();

        public override void Calculate(SkyContext c)
        {
            Moon.Equatorial = c.Get(Equatorial);
            Moon.Horizontal = c.Get(Horizontal);
            Moon.PAaxis = c.Get(PAaxis);
            Moon.Phase = c.Get(Phase);
            Moon.Ecliptical0 = c.Get(Ecliptical0);
            Moon.Semidiameter = c.Get(Semidiameter);
            Moon.Elongation = c.Get(Elongation);
            Moon.Libration = c.Get(LibrationElements);
            Moon.AscendingNode = c.Get(AscendingNode);
            Moon.EarthShadow = c.Get(EarthShadow);
            Moon.EarthShadowCoordinates = c.Get(EarthShadowCoordinates);
        }

        /// <summary>
        /// Gets helipcentrical coordinates of Earth
        /// </summary>
        private CrdsHeliocentrical EarthHeliocentrical(SkyContext c)
        {
            return PlanetPositions.GetPlanetCoordinates(Planet.EARTH, c.JulianDay, highPrecision: true);
        }

        /// <summary>
        /// Gets apparent geocentrical ecliptical coordinates of the Sun
        /// </summary>
        private CrdsEcliptical SunEcliptical(SkyContext c)
        {
            // get Earth coordinates
            CrdsHeliocentrical hEarth = c.Get(EarthHeliocentrical);

            // transform to ecliptical coordinates of the Sun
            CrdsEcliptical sunEcliptical = new CrdsEcliptical(Angle.To360(hEarth.L + 180), -hEarth.B, hEarth.R);

            // correct solar coordinates to FK5 system
            sunEcliptical += PlanetPositions.CorrectionForFK5(c.JulianDay, sunEcliptical);

            // add nutation effect to ecliptical coordinates of the Sun
            sunEcliptical += Nutation.NutationEffect(c.NutationElements.deltaPsi);

            // add aberration effect, so we have an final ecliptical coordinates of the Sun 
            sunEcliptical += Aberration.AberrationEffect(sunEcliptical.Distance);

            return sunEcliptical;
        }

        /// <summary>
        /// Gets apparent geocentrical ecliptical coordinates of the Moon
        /// </summary>
        private CrdsEcliptical Ecliptical0(SkyContext c)
        {
            // geocentrical coordinates of the Moon
            CrdsEcliptical ecliptical0 = LunarMotion.GetCoordinates(c.JulianDay);

            // apparent geocentrical ecliptical coordinates 
            ecliptical0 += Nutation.NutationEffect(c.NutationElements.deltaPsi);

            return ecliptical0;
        }

        /// <summary>
        /// Gets equatorial geocentrical coordinates of the Moon
        /// </summary>
        private CrdsEquatorial Equatorial0(SkyContext c)
        {
            return c.Get(Ecliptical0).ToEquatorial(c.Epsilon);
        }

        /// <summary>
        /// Gets Moon horizontal equatorial parallax
        /// </summary>
        private double Parallax(SkyContext c)
        {
            return LunarEphem.Parallax(c.Get(Ecliptical0).Distance);
        }

        /// <summary>
        /// Gets visible semidiameter of the Moon, in seconds of arc 
        /// </summary>
        public double Semidiameter(SkyContext c)
        {
            return LunarEphem.Semidiameter(c.Get(Ecliptical0).Distance);
        }

        /// <summary>
        /// Gets euqatorial topocentric coordinates of the Moon
        /// </summary>
        public CrdsEquatorial Equatorial(SkyContext c)
        {
            return c.Get(Equatorial0).ToTopocentric(c.GeoLocation, c.SiderealTime, c.Get(Parallax));
        }

        /// <summary>
        /// Gets ecliptical coordinates of the Moon
        /// </summary>
        private CrdsEcliptical Ecliptical(SkyContext c)
        {
            return c.Get(Equatorial).ToEcliptical(c.Epsilon);
        }

        /// <summary>
        /// Gets local horizontal coordinates of the Moon
        /// </summary>
        private CrdsHorizontal Horizontal(SkyContext c)
        {
            return c.Get(Equatorial).ToHorizontal(c.GeoLocation, c.SiderealTime);
        }

        /// <summary>
        /// Gets geocentric elongation angle of the Moon
        /// </summary>
        private double Elongation(SkyContext c)
        {
            return BasicEphem.Elongation(c.Get(SunEcliptical), c.Get(Ecliptical0));
        }

        /// <summary>
        /// Gets phase angle for the Moon
        /// </summary>
        private double PhaseAngle(SkyContext c)
        {
            return BasicEphem.PhaseAngle(c.Get(Elongation), c.Get(SunEcliptical).Distance * 149597871.0, c.Get(Ecliptical0).Distance);
        }

        /// <summary>
        /// Gets phase of the Moon
        /// </summary>
        public double Phase(SkyContext c)
        {
            return BasicEphem.Phase(c.Get(PhaseAngle));
        }

        /// <summary>
        /// Get position angle of axis for the Moon
        /// </summary>
        private double PAaxis(SkyContext c)
        {
            return LunarEphem.PositionAngleOfAxis(c.JulianDay, c.Get(Ecliptical), c.Epsilon, c.NutationElements.deltaPsi);
        }

        /// <summary>
        /// Gets libration info for the Moon
        /// </summary>
        private Libration LibrationElements(SkyContext c)
        {
            return LunarEphem.Libration(c.JulianDay, c.Get(Ecliptical), c.NutationElements.deltaPsi);
        }

        /// <summary>
        /// Gets visual magnitude of the Moon
        /// </summary>
        public double Magnitude(SkyContext c)
        {
            return LunarEphem.Magnitude(c.Get(PhaseAngle));
        }

        /// <summary>
        /// Gets nearest phase date
        /// </summary>
        private double NearestPhase(SkyContext c, MoonPhase p)
        {
            return LunarEphem.NearestPhase(c.JulianDay, p);
        }

        /// <summary>
        /// Gets Moon age in days
        /// </summary>
        private double Age(SkyContext c)
        {
            return LunarEphem.Age(c.JulianDay);
        }

        /// <summary>
        /// Gets nearest apsis date
        /// </summary>
        private double NearestApsis(SkyContext c, MoonApsis a)
        {
            return LunarEphem.NearestApsis(c.JulianDay, a, out double diameter);
        }

        /// <summary>
        /// Gets longitude of true ascending node of lunar orbit
        /// </summary>
        private double AscendingNode(SkyContext c)
        {
            return LunarEphem.TrueAscendingNode(c.JulianDay);
        }

        /// <summary>
        /// Gets details of Earth shadow 
        /// </summary>
        private ShadowAppearance EarthShadow(SkyContext c)
        {
            return LunarEphem.Shadow(c.JulianDay);
        }

        /// <summary>
        /// Gets horizontal topocentrical coordinates of Earth Shadow
        /// </summary>
        private CrdsHorizontal EarthShadowCoordinates(SkyContext c)
        {
            // Ecliptical coordinates of Sun
            var eclSun = c.Get(SunEcliptical);

            // Coordinates of Earth shadow is an opposite point on the celestial sphere
            var eclShadow = new CrdsEcliptical(eclSun.Lambda + 180, -eclSun.Beta);

            // Equatorial geocentrical coordinates of the shadow center
            var eq0 = eclShadow.ToEquatorial(c.Epsilon);

            // Topocentrical equatorial coordinates
            var eq = eq0.ToTopocentric(c.GeoLocation, c.SiderealTime, c.Get(Parallax));

            // finally get the horizontal coordinates
            return eq.ToHorizontal(c.GeoLocation, c.SiderealTime);
        }

        /// <summary>
        /// Gets rise, transit and set info for the Moon
        /// </summary>
        private RTS RiseTransitSet(SkyContext c)
        {
            double jd = c.JulianDayMidnight;
            double theta0 = Date.ApparentSiderealTime(jd, c.NutationElements.deltaPsi, c.Epsilon);

            CrdsEquatorial[] eq = new CrdsEquatorial[3];
            double[] diff = new double[] { 0, 0.5, 1 };

            for (int i = 0; i < 3; i++)
            {
                eq[i] = new SkyContext(jd + diff[i], c.GeoLocation).Get(Equatorial0);
            }

            return Visibility.RiseTransitSet(eq, c.GeoLocation, theta0, c.Get(Parallax), c.Get(Semidiameter) / 3600);
        }

        public void ConfigureEphemeris(EphemerisConfig<Moon> e)
        {
            e["RTS.Rise"] = (c, m) => c.Get(RiseTransitSet).Rise;
            e["RTS.RiseAzimuth"] = (c, m) => c.Get(RiseTransitSet).RiseAzimuth;
            e["RTS.Transit"] = (c, m) => c.Get(RiseTransitSet).Transit;
            e["RTS.TransitAltitude"] = (c, m) => c.Get(RiseTransitSet).TransitAltitude;
            e["RTS.Set"] = (c, m) => c.Get(RiseTransitSet).Set;
            e["RTS.SetAzimuth"] = (c, m) => c.Get(RiseTransitSet).SetAzimuth;
            e["Equatorial.Alpha"] = (c, m) => c.Get(Equatorial).Alpha;
            e["Equatorial.Delta"] = (c, m) => c.Get(Equatorial).Delta;
            e["Horizontal.Altitude"] = (c, m) => c.Get(Horizontal).Altitude;
            e["Horizontal.Azimuth"] = (c, m) => c.Get(Horizontal).Azimuth;
            e["Libration.Longitude"] = (c, m) => c.Get(LibrationElements).l;
            e["Libration.Latitude"] = (c, m) => c.Get(LibrationElements).b;
            e["Phase"] = (c, m) => c.Get(Phase);
            e["PhaseAngle"] = (c, m) => c.Get(PhaseAngle);
            e["Age"] = (c, m) => c.Get(Age);
            e["Magnitude"] = (c, m) => c.Get(Magnitude);
        }

        public CelestialObjectInfo GetInfo(SkyContext c, Moon m)
        {
            var rts = c.Get(RiseTransitSet);
            var jdNM = c.Get(NearestPhase, MoonPhase.NewMoon);
            var jdFQ = c.Get(NearestPhase, MoonPhase.FirstQuarter);
            var jdFM = c.Get(NearestPhase, MoonPhase.FullMoon);
            var jdLQ = c.Get(NearestPhase, MoonPhase.LastQuarter);
            var jdApogee = c.Get(NearestApsis, MoonApsis.Apogee);
            var jdPerigee = c.Get(NearestApsis, MoonApsis.Perigee);

            var info = new CelestialObjectInfo();
            info.SetTitle(Moon.Name)

                .AddRow("Constellation", Constellations.FindConstellation(c.Get(Equatorial), c.JulianDay))

                .AddHeader("Equatorial coordinates (geocentrical)")
                .AddRow("Equatorial0.Alpha", c.Get(Equatorial0).Alpha)
                .AddRow("Equatorial0.Delta", c.Get(Equatorial0).Delta)

                .AddHeader("Equatorial coordinates (topocentrical)")
                .AddRow("Equatorial.Alpha", c.Get(Equatorial).Alpha)
                .AddRow("Equatorial.Delta", c.Get(Equatorial).Delta)

                .AddHeader("Ecliptical coordinates")
                .AddRow("Ecliptical.Lambda", c.Get(Ecliptical0).Lambda)
                .AddRow("Ecliptical.Beta", c.Get(Ecliptical0).Beta)

                .AddHeader("Horizontal coordinates")
                .AddRow("Horizontal.Azimuth", c.Get(Horizontal).Azimuth)
                .AddRow("Horizontal.Altitude", c.Get(Horizontal).Altitude)

                .AddHeader("Visibility")
                .AddRow("RTS.Rise", rts.Rise, c.JulianDayMidnight + rts.Rise)
                .AddRow("RTS.Transit", rts.Transit, c.JulianDayMidnight + rts.Transit)
                .AddRow("RTS.Set", rts.Set, c.JulianDayMidnight + rts.Set)
                .AddRow("RTS.Duration", rts.Duration)

                .AddHeader("Appearance")
                .AddRow("Phase", c.Get(Phase))
                .AddRow("PhaseAngle", c.Get(PhaseAngle))
                .AddRow("Age", c.Get(Age))
                .AddRow("Magnitude", c.Get(Magnitude))
                .AddRow("Distance", (int)c.Get(Ecliptical0).Distance + " km", Formatters.Simple)
                .AddRow("HorizontalParallax", c.Get(Parallax))
                .AddRow("AngularDiameter", c.Get(Semidiameter) * 2 / 3600.0)
                .AddRow("Libration.Latitude", c.Get(LibrationElements).b)
                .AddRow("Libration.Longitude", c.Get(LibrationElements).l)

                .AddHeader("Nearest phases")
                .AddRow("MoonPhases.NewMoon", c.GetDate(jdNM), jdNM)
                .AddRow("MoonPhases.FirstQuarter", c.GetDate(jdFQ), jdFQ)
                .AddRow("MoonPhases.FullMoon", c.GetDate(jdFM), jdFM)
                .AddRow("MoonPhases.LastQuarter", c.GetDate(jdLQ), jdLQ)

                .AddHeader("Nearest apsides")
                .AddRow("MoonApsides.Apogee", c.GetDate(jdApogee), jdApogee)
                .AddRow("MoonApsides.Perigee", c.GetDate(jdPerigee), jdPerigee);

            return info;
        }

        public ICollection<SearchResultItem> Search(SkyContext context, string searchString, int maxCount = 50)
        {
            if (Moon.Name.StartsWith(searchString, StringComparison.OrdinalIgnoreCase))
                return new[] { new SearchResultItem(Moon, Moon.Name) };
            else
                return new SearchResultItem[0];
        }
    }
}
