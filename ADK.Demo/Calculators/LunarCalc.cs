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
    public class LunarCalc : BaseSkyCalc
    {
        private Moon moon = new Moon();

        public LunarCalc(Sky sky) : base(sky)
        {
            Sky.AddDataProvider("Moon", () => moon);
        }

        public override void Calculate(SkyContext context)
        {
            // get Earth coordinates
            CrdsHeliocentrical hEarth = PlanetPositions.GetPlanetCoordinates(Planet.EARTH, context.JulianDay, highPrecision: true);

            // transform to ecliptical coordinates of the Sun
            CrdsEcliptical sunEcliptical = new CrdsEcliptical(Angle.To360(hEarth.L + 180), -hEarth.B, hEarth.R);

            // correct solar coordinates to FK5 system
            sunEcliptical += PlanetPositions.CorrectionForFK5(context.JulianDay, sunEcliptical);

            // add nutation effect to ecliptical coordinates of the Sun
            sunEcliptical += Nutation.NutationEffect(context.NutationElements.deltaPsi);

            // add aberration effect, so we have an final ecliptical coordinates of the Sun 
            sunEcliptical += Aberration.AberrationEffect(sunEcliptical.Distance);

            // geocentrical coordinates of the Moon
            moon.Ecliptical0 = LunarMotion.GetCoordinates(context.JulianDay);

            // apparent geocentrical ecliptical coordinates 
            moon.Ecliptical0 += Nutation.NutationEffect(context.NutationElements.deltaPsi);

            // equatorial geocentrical coordinates
            moon.Equatorial0 = moon.Ecliptical0.ToEquatorial(context.Epsilon);

            // Horizontal equatorial parallax
            moon.Parallax = LunarEphem.Parallax(moon.Ecliptical0.Distance);

            // Visible semidiameter
            moon.Semidiameter = LunarEphem.Semidiameter(moon.Ecliptical0.Distance);

            // Topocentric equatorial coordinates
            moon.Equatorial = moon.Equatorial0.ToTopocentric(context.GeoLocation, context.SiderealTime, moon.Parallax);

            // Topocentric ecliptical coordinates
            moon.Ecliptical = moon.Equatorial.ToEcliptical(context.Epsilon);

            // Local horizontal coordinates of the Moon
            moon.Horizontal = moon.Equatorial.ToHorizontal(context.GeoLocation, context.SiderealTime);

            // Elongation of the Moon
            moon.Elongation = Appearance.Elongation(sunEcliptical, moon.Ecliptical0);
            
            // Phase angle
            moon.PhaseAngle = Appearance.PhaseAngle(moon.Elongation, sunEcliptical.Distance * 149597871.0, moon.Ecliptical0.Distance);
            
            // Moon phase
            moon.Phase = Appearance.Phase(moon.PhaseAngle);

            // Topocentrical PA of axis
            moon.PAaxis = LunarEphem.PositionAngleOfAxis(context.JulianDay, moon.Ecliptical, context.Epsilon, context.NutationElements.deltaPsi);

            // Topocentrical libration
            moon.Libration = LunarEphem.Libration(context.JulianDay, moon.Ecliptical, context.NutationElements.deltaPsi);
        }
    }
}
