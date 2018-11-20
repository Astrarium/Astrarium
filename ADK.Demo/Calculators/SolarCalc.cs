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
    public class SolarCalc : BaseSkyCalc
    {
        private Sun Sun = new Sun();

        public SolarCalc(Sky sky) : base(sky)
        {
            Sky.AddDataProvider("Sun", () => Sun);
        }

        public override void Calculate()
        {
            // get Earth coordinates
            CrdsHeliocentrical crds = PlanetPositions.GetPlanetCoordinates(3, Sky.JulianDay, highPrecision: true);

            // transform to ecliptical coordinates of the Sun
            Sun.Ecliptical = new CrdsEcliptical(Angle.To360(crds.L + 180), -crds.B, crds.R);

            // get FK5 system correction
            CrdsEcliptical corr = PlanetPositions.CorrectionForFK5(Sky.JulianDay, Sun.Ecliptical);

            // correct solar coordinates to FK5 system
            Sun.Ecliptical += corr;

            // add nutation effect
            Sun.Ecliptical += Nutation.NutationEffect(Sky.NutationElements.deltaPsi);

            // add aberration effect 
            Sun.Ecliptical += Aberration.AberrationEffect(Sun.Ecliptical.Distance);

            // convert ecliptical to equatorial coordinates
            Sun.Equatorial = Sun.Ecliptical.ToEquatorial(Sky.Epsilon);

            // local horizontal coordinates of the Sun
            Sun.Horizontal = Sun.Equatorial.ToHorizontal(Sky.GeoLocation, Sky.SiderealTime);

            // TODO: parallax effect

            // Solar semidiameter
            // TODO: move to separate class
            Sun.Semidiameter = PlanetPositions.Semidiameter(3, Sun.Ecliptical.Distance);
        }
    }
}
