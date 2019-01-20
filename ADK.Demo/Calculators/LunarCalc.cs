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
    public class LunarCalc : BaseSkyCalc, IEphemProvider<Moon>
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

            //// get Earth coordinates
            //CrdsHeliocentrical hEarth = PlanetPositions.GetPlanetCoordinates(Planet.EARTH, context.JulianDay, highPrecision: true);

            //// transform to ecliptical coordinates of the Sun
            //CrdsEcliptical sunEcliptical = new CrdsEcliptical(Angle.To360(hEarth.L + 180), -hEarth.B, hEarth.R);

            //// correct solar coordinates to FK5 system
            //sunEcliptical += PlanetPositions.CorrectionForFK5(context.JulianDay, sunEcliptical);

            //// add nutation effect to ecliptical coordinates of the Sun
            //sunEcliptical += Nutation.NutationEffect(context.NutationElements.deltaPsi);

            //// add aberration effect, so we have an final ecliptical coordinates of the Sun 
            //sunEcliptical += Aberration.AberrationEffect(sunEcliptical.Distance);

            //// geocentrical coordinates of the Moon
            //moon.Ecliptical0 = LunarMotion.GetCoordinates(context.JulianDay);

            //// apparent geocentrical ecliptical coordinates 
            //moon.Ecliptical0 += Nutation.NutationEffect(context.NutationElements.deltaPsi);

            //// equatorial geocentrical coordinates
            //moon.Equatorial0 = moon.Ecliptical0.ToEquatorial(context.Epsilon);

            //// Horizontal equatorial parallax
            //moon.Parallax = LunarEphem.Parallax(moon.Ecliptical0.Distance);

            //// Visible semidiameter
            //moon.Semidiameter = LunarEphem.Semidiameter(moon.Ecliptical0.Distance);

            //// Topocentric equatorial coordinates
            //moon.Equatorial = moon.Equatorial0.ToTopocentric(context.GeoLocation, context.SiderealTime, moon.Parallax);

            //// Topocentric ecliptical coordinates
            //moon.Ecliptical = moon.Equatorial.ToEcliptical(context.Epsilon);

            //// Local horizontal coordinates of the Moon
            //moon.Horizontal = moon.Equatorial.ToHorizontal(context.GeoLocation, context.SiderealTime);

            //// Elongation of the Moon
            //moon.Elongation = Appearance.Elongation(sunEcliptical, moon.Ecliptical0);
            
            //// Phase angle
            //moon.PhaseAngle = Appearance.PhaseAngle(moon.Elongation, sunEcliptical.Distance * 149597871.0, moon.Ecliptical0.Distance);
            
            //// Moon phase
            //moon.Phase = Appearance.Phase(moon.PhaseAngle);

            //// Topocentrical PA of axis
            //moon.PAaxis = LunarEphem.PositionAngleOfAxis(context.JulianDay, moon.Ecliptical, context.Epsilon, context.NutationElements.deltaPsi);

            //// Topocentrical libration
            //moon.Libration = LunarEphem.Libration(context.JulianDay, moon.Ecliptical, context.NutationElements.deltaPsi);
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
        /// Gets equatorial geocentrical coordinates  of the Moon
        /// </summary>
        private CrdsEquatorial Equatorial0(SkyContext c)
        {            
            return c.Get(Ecliptical0).ToEquatorial(c.Epsilon);
        }

        private double Parallax(SkyContext c)
        {
            return LunarEphem.Parallax(c.Get(Ecliptical0).Distance);
        }

        private double Semidiameter(SkyContext c)
        {
            return LunarEphem.Semidiameter(c.Get(Ecliptical0).Distance);
        }

        private CrdsEquatorial Equatorial(SkyContext c)
        {
            return c.Get(Equatorial0).ToTopocentric(c.GeoLocation, c.SiderealTime, c.Get(Parallax));
        }

        private CrdsEcliptical Ecliptical(SkyContext c)
        {
            return c.Get(Equatorial).ToEcliptical(c.Epsilon);
        }

        private CrdsHorizontal Horizontal(SkyContext c)
        {
            return c.Get(Equatorial).ToHorizontal(c.GeoLocation, c.SiderealTime);
        }

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


        /// <summary>
        /// Gets rise, transit and set info for the Moon
        /// </summary>
        private RTS RiseTransitSet(SkyContext c)
        {
            Date d = new Date(c.JulianDay);
            double jd = new Date(d.Year, d.Month, (int)d.Day).ToJulianEphemerisDay() - 3 / 24.0;

            double theta0 = Date.ApparentSiderealTime(jd, c.NutationElements.deltaPsi, c.Epsilon);

            CrdsEquatorial[] eq = new CrdsEquatorial[3];            
            double[] diff = new double[] { 0, 0.5, 1 };

            for (int i = 0; i < 3; i++)
            {
                eq[i] = new SkyContext(jd + diff[i], c.GeoLocation).Get(Equatorial);
            }

            return Appearance.RiseTransitSet2(jd, c.NutationElements.deltaPsi, c.Epsilon, eq, c.GeoLocation, c.DeltaT, 0.7275 * c.Get(Parallax) /*- 0.566666*/);
        }

        public void ConfigureEphemeris(EphemerisConfig<Moon> config)
        {
            config.Add("RTS.Rise", (c, m) => c.Get(RiseTransitSet).Rise)
                .WithFormatter(Formatters.RTS);

            config.Add("RTS.Transit", (c, m) => c.Get(RiseTransitSet).Transit)
                .WithFormatter(Formatters.RTS);

            config.Add("RTS.Set", (c, m) => c.Get(RiseTransitSet).Set)
                .WithFormatter(Formatters.RTS);

            config.Add("Equatorial.Alpha", (c, m) => c.Get(Equatorial).Alpha)
                .WithFormatter(Formatters.RA);

            config.Add("Equatorial.Delta", (c, m) => c.Get(Equatorial).Delta)
                .WithFormatter(Formatters.Dec);
        }

        
    }
}
