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
    public class SolarCalc : BaseSkyCalc, IEphemProvider<Sun>, IInfoProvider<Sun>
    {
        private Sun Sun = new Sun();

        public SolarCalc(Sky sky) : base(sky)
        {
            Sky.AddDataProvider("Sun", () => Sun);
        }

        public override void Calculate(SkyContext c)
        {
            Sun.Equatorial = c.Get(Equatorial);
            Sun.Horizontal = c.Get(Horizontal);
            Sun.Ecliptical = c.Get(Ecliptical);
            Sun.Semidiameter = c.Get(Semidiameter);
        }

        private CrdsEcliptical Ecliptical(SkyContext c)
        {
            // get Earth coordinates
            CrdsHeliocentrical crds = PlanetPositions.GetPlanetCoordinates(Planet.EARTH, c.JulianDay, highPrecision: true);

            // transform to ecliptical coordinates of the Sun
            var ecl = new CrdsEcliptical(Angle.To360(crds.L + 180), -crds.B, crds.R);

            // get FK5 system correction
            CrdsEcliptical corr = PlanetPositions.CorrectionForFK5(c.JulianDay, ecl);

            // correct solar coordinates to FK5 system
            ecl += corr;

            // add nutation effect
            ecl += Nutation.NutationEffect(c.NutationElements.deltaPsi);

            // add aberration effect 
            ecl += Aberration.AberrationEffect(ecl.Distance);

            return ecl;
        }

        private CrdsEquatorial Equatorial0(SkyContext c)
        {
            // convert ecliptical to geocentric equatorial coordinates
            return c.Get(Ecliptical).ToEquatorial(c.Epsilon);
        }

        private double Parallax(SkyContext c)
        {
            return SolarEphem.Parallax(c.Get(Ecliptical).Distance);
        }

        private CrdsEquatorial Equatorial(SkyContext c)
        {
            return c.Get(Equatorial0).ToTopocentric(c.GeoLocation, c.SiderealTime, c.Get(Parallax));
        }

        private CrdsHorizontal Horizontal(SkyContext c)
        {
            return c.Get(Equatorial).ToHorizontal(c.GeoLocation, c.SiderealTime);
        }

        private double Semidiameter(SkyContext c)
        {
            return SolarEphem.Semidiameter(c.Get(Ecliptical).Distance);
        }

        /// <summary>
        /// Gets rise, transit and set info for the Sun
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

        CelestialObjectInfo IInfoProvider<Sun>.GetInfo(SkyContext c, Sun sun)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Rise: ").Append(Formatters.Time.Format(c.Get(RiseTransitSet).Rise)).AppendLine();
            sb.Append("Transit: ").Append(Formatters.Time.Format(c.Get(RiseTransitSet).Transit)).AppendLine();
            sb.Append("Set: ").Append(Formatters.Time.Format(c.Get(RiseTransitSet).Set)).AppendLine();

            return null;
        }

        public void ConfigureEphemeris(EphemerisConfig<Sun> e)
        {
            e.Add("RTS.Rise", (c, s) => RiseTransitSet(c).Rise)
                .WithFormatter(Formatters.Time);

            e.Add("RTS.Transit", (c, s) => RiseTransitSet(c).Transit)
                .WithFormatter(Formatters.Time);

            e.Add("RTS.Set", (c, s) => RiseTransitSet(c).Set)
                .WithFormatter(Formatters.Time);
        }
    }
}
