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

            // convert ecliptical to geocentric equatorial coordinates
            Sun.Equatorial0 = Sun.Ecliptical.ToEquatorial(Sky.Epsilon);

            // solar parallax
            // TODO: move to separate class SolarEphem
            Sun.Parallax = PlanetEphem.Parallax(Sun.Ecliptical.Distance);
           
            // Topocentric equatorial coordinates (parallax effect)
            Sun.Equatorial = Sun.Equatorial0.ToTopocentric(Sky.GeoLocation, Sky.SiderealTime, Sun.Parallax);

            // local horizontal coordinates of the Sun
            Sun.Horizontal = Sun.Equatorial.ToHorizontal(Sky.GeoLocation, Sky.SiderealTime);

            // Solar semidiameter
            // TODO: move to separate class SolarEphem
            Sun.Semidiameter = PlanetEphem.Semidiameter(3, Sun.Ecliptical.Distance);
        }
    }
}
