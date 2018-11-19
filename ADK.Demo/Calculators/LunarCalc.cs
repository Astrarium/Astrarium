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

        public override void Calculate()
        {
            // geocentrical coordinates
            moon.Ecliptical = LunarMotion.GetCoordinates(Sky.JulianDay);

            // apparent geocentrical ecliptical coordinates 
            moon.Ecliptical += Nutation.NutationEffect(Sky.NutationElements.deltaPsi);

            // equatorial geocentrical coordinates
            moon.Equatorial0 = moon.Ecliptical.ToEquatorial(Sky.Epsilon);

            // Horizontal equatorial parallax
            moon.Parallax = LunarEphem.Parallax(moon.Ecliptical.Distance);

            // Visible semidiameter
            moon.Semidiameter = LunarEphem.Semidiameter(moon.Ecliptical.Distance);

            // Topocentric equatorial coordinates
            moon.Equatorial = moon.Equatorial0.ToTopocentric(Sky.GeoLocation, Sky.SiderealTime, moon.Parallax);

            // Local horizontal coordinates of the Moon
            moon.Horizontal = moon.Equatorial.ToHorizontal(Sky.GeoLocation, Sky.SiderealTime);
           
            Sun sun = Sky.Get<Sun>("Sun");

            // Elongation of the Moon
            moon.Elongation = LunarEphem.Elongation(sun.Ecliptical, moon.Ecliptical);
            
            moon.PhaseAngle = LunarEphem.PhaseAngle(moon.Elongation, sun.Ecliptical.Distance * 149597871.0, moon.Ecliptical.Distance);
            
            moon.Phase = LunarEphem.Phase(moon.PhaseAngle);

            // TODO: should use sun.Equatorial0 here?
            moon.PAlimb = LunarEphem.PositionAngleOfBrightLimb(sun.Equatorial, moon.Equatorial0);

            moon.PAcusp = LunarEphem.PositionAngleOfNorthCusp(moon.PAlimb);
        }
    }
}
