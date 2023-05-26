using Astrarium.Algorithms;
using Astrarium.Plugins.SolarSystem.Objects;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.SolarSystem
{
    public class LunarCalc : BaseCalc, ICelestialObjectCalc<Moon>
    {
        public Moon Moon { get; private set; } = new Moon();

        public IEnumerable<Moon> GetCelestialObjects() => new Moon[] { Moon };

        public LunarCalc(ISky sky)
        {
            sky.MoonEquatorial = Equatorial;
        }

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
        public CrdsEcliptical Ecliptical0(SkyContext c)
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
        public CrdsEquatorial Equatorial0(SkyContext c)
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
        public CrdsHorizontal Horizontal(SkyContext c)
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
        public float Magnitude(SkyContext c)
        {
            return LunarEphem.Magnitude(c.Get(PhaseAngle));
        }

        /// <summary>
        /// Gets nearest phase date
        /// </summary>
        private Date NearestPhase(SkyContext c, MoonPhase p)
        {
            return c.GetDate(LunarEphem.NearestPhase(c.JulianDay, p));
        }

        /// <summary>
        /// Gets Moon age in days
        /// </summary>
        private double Age(SkyContext c)
        {
            return LunarEphem.Age(c.JulianDay);
        }

        /// <summary>
        /// Gets Brown lunation number
        /// </summary>
        private int Lunation(SkyContext c)
        {
            double age = c.Get(Age);
            return LunarEphem.Lunation(c.JulianDay - age);
        }

        /// <summary>
        /// Gets nearest apsis date
        /// </summary>
        private Date NearestApsis(SkyContext c, MoonApsis a)
        {
            return c.GetDate(LunarEphem.NearestApsis(c.JulianDay, a, out double diameter));
        }

        /// <summary>
        /// Gets nearest greatest lunar declination date
        /// </summary>
        private Date NearestMaxDeclination(SkyContext c, MoonDeclination d)
        {
            return c.GetDate(LunarEphem.NearestMaxDeclination(c.JulianDay, d, out double delta));
        }

        /// <summary>
        /// Gets nearest max libration date
        /// </summary>
        private Date NearestMaxLibration(SkyContext c, LibrationEdge e)
        {
            return c.GetDate(LunarEphem.NearestMaxLibration(c.JulianDay, e, out double angle));
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
        private CrdsEquatorial EarthShadowCoordinates(SkyContext c)
        {
            // Ecliptical coordinates of Sun
            var eclSun = c.Get(SunEcliptical);

            // Coordinates of Earth shadow is an opposite point on the celestial sphere
            var eclShadow = new CrdsEcliptical(eclSun.Lambda + 180, -eclSun.Beta);

            // Equatorial geocentrical coordinates of the shadow center
            var eq0 = eclShadow.ToEquatorial(c.Epsilon);

            // Topocentrical equatorial coordinates
            return eq0.ToTopocentric(c.GeoLocation, c.SiderealTime, c.Get(Parallax));
        }

        /// <summary>
        /// Calculates parameter Q descibing the visibility of lunar crescent.
        /// Used for calculating noumenia and epimenia events.
        /// </summary>
        public double CrescentQ(SkyContext c)
        {
            var eclSun = c.Get(SunEcliptical);
            var eclMoon = c.Get(Ecliptical0);
            var sd = c.Get(Semidiameter);
            return LunarEphem.CrescentQ(eclMoon, eclSun, sd, c.Epsilon, c.SiderealTime, c.GeoLocation);
        }

        /// <summary>
        /// Gets rise, transit and set info for the Moon
        /// </summary>
        public RTS RiseTransitSet(SkyContext c)
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
            e["Constellation"] = (c, m) => Constellations.FindConstellation(c.Get(Equatorial), c.JulianDay);
            e["RTS.Rise"] = (c, m) => c.GetDateFromTime(c.Get(RiseTransitSet).Rise);
            e["RTS.RiseAzimuth"] = (c, m) => c.Get(RiseTransitSet).RiseAzimuth;
            e["RTS.Transit"] = (c, m) => c.GetDateFromTime(c.Get(RiseTransitSet).Transit);
            e["RTS.TransitAltitude"] = (c, m) => c.Get(RiseTransitSet).TransitAltitude;
            e["RTS.Set"] = (c, m) => c.GetDateFromTime(c.Get(RiseTransitSet).Set);
            e["RTS.SetAzimuth"] = (c, m) => c.Get(RiseTransitSet).SetAzimuth;
            e["RTS.Duration"] = (c, m) => c.Get(RiseTransitSet).Duration;
            e["Equatorial.Alpha"] = (c, m) => c.Get(Equatorial).Alpha;
            e["Equatorial.Delta"] = (c, m) => c.Get(Equatorial).Delta;
            e["Equatorial0.Alpha"] = (c, m) => c.Get(Equatorial0).Alpha;
            e["Equatorial0.Delta"] = (c, m) => c.Get(Equatorial0).Delta;
            e["Horizontal.Altitude"] = (c, m) => c.Get(Horizontal).Altitude;
            e["Horizontal.Azimuth"] = (c, m) => c.Get(Horizontal).Azimuth;
            e["Ecliptical.Lambda"] = (c, m) => c.Get(Ecliptical0).Lambda;
            e["Ecliptical.Beta"] = (c, m) => c.Get(Ecliptical0).Beta;
            e["Phase"] = (c, m) => c.Get(Phase);
            e["PhaseAngle"] = (c, m) => c.Get(PhaseAngle);
            e["Age", new Formatters.UnsignedDoubleFormatter(2, " d")] = (c, m) => c.Get(Age);
            e["Lunation"] = (c, m) => c.Get(Lunation);
            e["Magnitude"] = (c, m) => c.Get(Magnitude);
            e["Distance", new LunarDistanceFormatter()] = (c, m) => c.Get(Ecliptical0).Distance;
            e["HorizontalParallax"] = (c, m) => c.Get(Parallax);
            e["AngularDiameter"] = (c, m) => c.Get(Semidiameter) * 2 / 3600.0;
            e["Libration.Latitude", new LibrationLatitudeFormatter()] = (c, m) => c.Get(LibrationElements).b;
            e["Libration.Longitude", new LibrationLongitudeFormatter()] = (c, m) => c.Get(LibrationElements).l;
        }

        public void GetInfo(CelestialObjectInfo<Moon> info)
        {
            var c = info.Context;

            info.SetTitle(info.Body.Name)

            .AddRow("Constellation")

            .AddHeader(Text.Get("Moon.Horizontal"))
            .AddRow("Horizontal.Azimuth")
            .AddRow("Horizontal.Altitude")

            .AddHeader(Text.Get("Moon.Equatorial"))
            .AddRow("Equatorial.Alpha")
            .AddRow("Equatorial.Delta")

            .AddHeader(Text.Get("Moon.Equatorial0"))
            .AddRow("Equatorial0.Alpha")
            .AddRow("Equatorial0.Delta")

            .AddHeader(Text.Get("Moon.Ecliptical"))
            .AddRow("Ecliptical.Lambda")
            .AddRow("Ecliptical.Beta")

            .AddHeader(Text.Get("Moon.RTS"))
            .AddRow("RTS.Rise")
            .AddRow("RTS.Transit")
            .AddRow("RTS.Set")
            .AddRow("RTS.Duration")
            .AddRow("RTS.RiseAzimuth")
            .AddRow("RTS.TransitAltitude")
            .AddRow("RTS.SetAzimuth")

            .AddHeader(Text.Get("Moon.Appearance"))
            .AddRow("Phase")
            .AddRow("PhaseAngle")
            .AddRow("Age")
            .AddRow("Lunation")
            .AddRow("Magnitude")
            .AddRow("Distance")
            .AddRow("HorizontalParallax")
            .AddRow("AngularDiameter")
            .AddRow("Libration.Latitude")
            .AddRow("Libration.Longitude");

            info.AddHeader(Text.Get("Moon.Phases"));
            var phases = new Tuple<string, Date>[] {
                 new Tuple<string, Date>("Phases.NewMoon", c.Get(NearestPhase, MoonPhase.NewMoon)),
                 new Tuple<string, Date>("Phases.FirstQuarter", c.Get(NearestPhase, MoonPhase.FirstQuarter)),
                 new Tuple<string, Date>("Phases.FullMoon", c.Get(NearestPhase, MoonPhase.FullMoon)),
                 new Tuple<string, Date>("Phases.LastQuarter", c.Get(NearestPhase, MoonPhase.LastQuarter))
            };
            foreach (var p in phases.OrderBy(i => i.Item2))
            {
                info.AddRow(p.Item1, p.Item2, Formatters.DateTime);
            }

            info.AddHeader(Text.Get("Moon.Apsis"));
            var apsis = new Tuple<string, Date>[] {
                 new Tuple<string, Date>("Apsis.Apogee", c.Get(NearestApsis, MoonApsis.Apogee)),
                 new Tuple<string, Date>("Apsis.Perigee", c.Get(NearestApsis, MoonApsis.Perigee))
            };
            foreach (var p in apsis.OrderBy(i => i.Item2))
            {
                info.AddRow(p.Item1, p.Item2, Formatters.DateTime);
            }

            info.AddHeader(Text.Get("Moon.MaxDeclinations"));
            var maxDeclinations = new Tuple<string, Date>[] {
                 new Tuple<string, Date>("MaxDeclinations.North", c.Get(NearestMaxDeclination, MoonDeclination.North)),
                 new Tuple<string, Date>("MaxDeclinations.South", c.Get(NearestMaxDeclination, MoonDeclination.South))
            };
            foreach (var p in maxDeclinations.OrderBy(i => i.Item2))
            {
                info.AddRow(p.Item1, p.Item2, Formatters.DateTime);
            }

            info.AddHeader(Text.Get("Moon.MaxLibrations"));
            var maxLibrations = new Tuple<string, Date>[] {
                 new Tuple<string, Date>("MaxLibrations.North", c.Get(NearestMaxLibration, LibrationEdge.North)),
                 new Tuple<string, Date>("MaxLibrations.South", c.Get(NearestMaxLibration, LibrationEdge.South)),
                 new Tuple<string, Date>("MaxLibrations.East", c.Get(NearestMaxLibration, LibrationEdge.East)),
                 new Tuple<string, Date>("MaxLibrations.West", c.Get(NearestMaxLibration, LibrationEdge.West))
            };
            foreach (var p in maxLibrations.OrderBy(i => i.Item2))
            {
                info.AddRow(p.Item1, p.Item2, Formatters.DateTime);
            }
        }

        public ICollection<CelestialObject> Search(SkyContext context, string searchString, Func<CelestialObject, bool> filterFunc, int maxCount = 50)
        {
            if (Moon.Name.StartsWith(searchString, StringComparison.OrdinalIgnoreCase) && filterFunc(Moon))
                return new[] { Moon };
            else if (Moon.CommonName.Equals(searchString, StringComparison.OrdinalIgnoreCase) && filterFunc(Moon))
                return new[] { Moon };
            else
                return new CelestialObject[0];
        }
    }
}
