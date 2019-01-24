using ADK.Demo.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo.Calculators
{
    public class LunarCalc : BaseSkyCalc, IEphemProvider<Moon>, IInfoProvider<Moon>
    {
        private Moon moon = new Moon();

        public LunarCalc(Sky sky) : base(sky)
        {
            Sky.AddDataProvider("Moon", () => moon);
        }

        public override void Calculate(SkyContext c)
        {
            moon.Equatorial = c.Get(Equatorial);
            moon.Horizontal = c.Get(Horizontal);
            moon.PAaxis = c.Get(PAaxis);
            moon.Phase = c.Get(Phase);
            moon.Ecliptical0 = c.Get(Ecliptical0);
            moon.Semidiameter = c.Get(Semidiameter);
            moon.Elongation = c.Get(Elongation);
            moon.Libration = c.Get(LibrationElements);
        }

        private CrdsHeliocentrical EarthHeliocentrical(SkyContext c)
        {
            return PlanetPositions.GetPlanetCoordinates(Planet.EARTH, c.JulianDay, highPrecision: true);
        }

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
        private double Semidiameter(SkyContext c)
        {
            return LunarEphem.Semidiameter(c.Get(Ecliptical0).Distance);
        }

        /// <summary>
        /// Gets euqatorial topocentric coordinates of the Moon
        /// </summary>
        private CrdsEquatorial Equatorial(SkyContext c)
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
            return Appearance.Elongation(c.Get(SunEcliptical), c.Get(Ecliptical0));
        }

        private double PhaseAngle(SkyContext c)
        {
            return Appearance.PhaseAngle(c.Get(Elongation), c.Get(SunEcliptical).Distance * 149597871.0, c.Get(Ecliptical0).Distance);
        }

        private double Phase(SkyContext c)
        {
            return Appearance.Phase(c.Get(PhaseAngle));
        }

        private double PAaxis(SkyContext c)
        {
            return LunarEphem.PositionAngleOfAxis(c.JulianDay, c.Get(Ecliptical), c.Epsilon, c.NutationElements.deltaPsi);
        }

        private Libration LibrationElements(SkyContext c)
        {
            return LunarEphem.Libration(c.JulianDay, c.Get(Ecliptical), c.NutationElements.deltaPsi);
        }

        private double Magnitude(SkyContext c)
        {
            return LunarEphem.Magnitude(c.Get(PhaseAngle));
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

            return Appearance.RiseTransitSet(eq, c.GeoLocation, theta0, c.Get(Parallax), c.Get(Semidiameter) / 3600.0);
        }

        public void ConfigureEphemeris(EphemerisConfig<Moon> config)
        {
            config.Add("RTS.Rise", (c, m) => c.Get(RiseTransitSet).Rise)
                .WithFormatter(Formatters.Time);

            config.Add("RTS.RiseAzimuth", (c, m) => c.Get(RiseTransitSet).RiseAzimuth)
                .WithFormatter(Formatters.IntAzimuth);

            config.Add("RTS.Transit", (c, m) => c.Get(RiseTransitSet).Transit)
                .WithFormatter(Formatters.Time);

            config.Add("RTS.TransitAltitude", (c, m) => c.Get(RiseTransitSet).TransitAltitude)
                .WithFormatter(Formatters.Altitude1d);

            config.Add("RTS.Set", (c, m) => c.Get(RiseTransitSet).Set)
                .WithFormatter(Formatters.Time);

            config.Add("RTS.SetAzimuth", (c, m) => c.Get(RiseTransitSet).SetAzimuth)
                .WithFormatter(Formatters.IntAzimuth);

            config.Add("Equatorial.Alpha", (c, m) => c.Get(Equatorial).Alpha)
                .WithFormatter(Formatters.RA);

            config.Add("Equatorial.Delta", (c, m) => c.Get(Equatorial).Delta)
                .WithFormatter(Formatters.Dec);
        }

        CelestialObjectInfo IInfoProvider<Moon>.GetInfo(SkyContext c, Moon m)
        {
            var rts = c.Get(RiseTransitSet);

            var info = new CelestialObjectInfo();
            info.SetTitle("Moon")

                .AddRow("Constellation", Constellations.FindConstellation(c.Get(Equatorial)))

                .AddHeader("Equatorial coordinates (geocentrical)")
                .AddRow("RA", c.Get(Equatorial0).Alpha, Formatters.RA)
                .AddRow("Dec", c.Get(Equatorial0).Delta, Formatters.Dec)

                .AddHeader("Equatorial coordinates (topocentrical)")
                .AddRow("RA", c.Get(Equatorial).Alpha, Formatters.RA)
                .AddRow("Dec", c.Get(Equatorial).Delta, Formatters.Dec)

                .AddHeader("Ecliptical coordinates")
                .AddRow("Longitude", c.Get(Ecliptical0).Lambda, Formatters.Longitude)
                .AddRow("Latitude", c.Get(Ecliptical0).Beta, Formatters.Latitude)

                 .AddHeader("Horizontal coordinates")
                .AddRow("Azimuth", c.Get(Horizontal).Azimuth, Formatters.Longitude)
                .AddRow("Altitude", c.Get(Horizontal).Altitude, Formatters.Latitude)

                .AddHeader("Visibility")
                .AddRow("Rise", rts.Rise, Formatters.Time, c.JulianDayMidnight + rts.Rise)
                .AddRow("Transit", rts.Transit, Formatters.Time, c.JulianDayMidnight + rts.Transit)
                .AddRow("Set", rts.Set, Formatters.Time, c.JulianDayMidnight + rts.Set)

                .AddHeader("Appearance")
                .AddRow("Phase", c.Get(Phase), Formatters.Phase)
                .AddRow("Phase angle", c.Get(PhaseAngle))
                .AddRow("Magnitude", Formatters.Magnitude.Format(c.Get(Magnitude)) + "<sup>m</sup>")
                .AddRow("Distance", (int)c.Get(Ecliptical0).Distance + " km")
                .AddRow("Horizontal parallax", c.Get(Parallax))
                .AddRow("Angular diameter", c.Get(Semidiameter) * 2)
                .AddRow("Libration in latitude", c.Get(LibrationElements).b)
                .AddRow("Libration in longitude", c.Get(LibrationElements).l);
            return info;
        }
    }
}
