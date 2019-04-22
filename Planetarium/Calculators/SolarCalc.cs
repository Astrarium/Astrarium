using ADK;
using Planetarium.Objects;
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
    public interface ISolarProvider
    {
        Sun Sun { get; }
    }

    public interface ISolarCalc
    {
        double Semidiameter(SkyContext c);
    }

    public class SolarCalc : BaseCalc, ICelestialObjectCalc<Sun>, ISolarProvider, ISolarCalc
    {
        public Sun Sun { get; private set; } = new Sun();

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

        /// <summary>
        /// Gets geocentric equatorial coordinates of the Sun
        /// </summary>
        private CrdsEquatorial Equatorial0(SkyContext c)
        {
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

        public double Semidiameter(SkyContext c)
        {
            return SolarEphem.Semidiameter(c.Get(Ecliptical).Distance);
        }

        private double CarringtonNumber(SkyContext c)
        {
            return SolarEphem.CarringtonNumber(c.JulianDay);
        }

        private double Seasons(SkyContext c, Season s)
        {
            return SolarEphem.Season(c.JulianDay, s);
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

            return Visibility.RiseTransitSet(eq, c.GeoLocation, theta0, c.Get(Parallax), c.Get(Semidiameter) / 3600.0);
        }

        public CelestialObjectInfo GetInfo(SkyContext c, Sun sun)
        {
            var rts = c.Get(RiseTransitSet);
            var jdSpring = c.Get(Seasons, Season.Spring);
            var jdSummer = c.Get(Seasons, Season.Summer);
            var jdAutumn = c.Get(Seasons, Season.Autumn);
            var jdWinter = c.Get(Seasons, Season.Winter);

            var info = new CelestialObjectInfo();
            info.SetTitle("Sun")

                .AddRow("Constellation", Constellations.FindConstellation(c.Get(Equatorial), c.JulianDay))
                .AddHeader("Equatorial coordinates (geocentrical)")
                .AddRow("Equatorial0.Alpha", c.Get(Equatorial0).Alpha)
                .AddRow("Equatorial0.Delta", c.Get(Equatorial0).Delta)

                .AddHeader("Equatorial coordinates (topocentrical)")
                .AddRow("Equatorial.Alpha", c.Get(Equatorial).Alpha)
                .AddRow("Equatorial.Delta", c.Get(Equatorial).Delta)

                .AddHeader("Ecliptical coordinates")
                .AddRow("Ecliptical.Lambda", c.Get(Ecliptical).Lambda)
                .AddRow("Ecliptical.Beta", c.Get(Ecliptical).Beta)

                .AddHeader("Horizontal coordinates")
                .AddRow("Horizontal.Azimuth", c.Get(Horizontal).Azimuth)
                .AddRow("Horizontal.Altitude", c.Get(Horizontal).Altitude)

                .AddHeader("Visibility")
                .AddRow("RTS.Rise", rts.Rise, c.JulianDayMidnight + rts.Rise)
                .AddRow("RTS.Transit", rts.Transit, c.JulianDayMidnight + rts.Transit)
                .AddRow("RTS.Set", rts.Set, c.JulianDayMidnight + rts.Set)
                .AddRow("RTS.Duration", rts.Duration)

                .AddHeader("Appearance")
                .AddRow("Distance", c.Get(Ecliptical).Distance)
                .AddRow("HorizontalParallax", c.Get(Parallax))
                .AddRow("AngularDiameter", c.Get(Semidiameter) * 2 / 3600.0)
                .AddRow("CRN", c.Get(CarringtonNumber))

                .AddHeader("Seasons")
                .AddRow("Seasons.Spring", new Date(jdSpring, c.GeoLocation.UtcOffset), jdSpring)
                .AddRow("Seasons.Summer", new Date(jdSummer, c.GeoLocation.UtcOffset), jdSummer)
                .AddRow("Seasons.Autumn", new Date(jdAutumn, c.GeoLocation.UtcOffset), jdAutumn)
                .AddRow("Seasons.Winter", new Date(jdWinter, c.GeoLocation.UtcOffset), jdWinter);

            return info;
        }

        public void ConfigureEphemeris(EphemerisConfig<Sun> e)
        {
            e.Add("RTS.Rise", (c, s) => RiseTransitSet(c).Rise);
            e.Add("RTS.Transit", (c, s) => RiseTransitSet(c).Transit);
            e.Add("RTS.Set", (c, s) => RiseTransitSet(c).Set);
            e.Add("RTS.Duration", (c, s) => RiseTransitSet(c).Duration);
        }

        public ICollection<SearchResultItem> Search(string searchString, int maxCount = 50)
        {
            if (CultureInfo.InvariantCulture.CompareInfo.IndexOf("Sun", searchString, CompareOptions.IgnoreCase) >= 0)
                return new[] { new SearchResultItem(Sun, "Sun") };
            else
                return new SearchResultItem[0];
        }

        public string GetName(Sun m)
        {
            return "Sun";
        }
    }
}
