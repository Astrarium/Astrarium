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
    public class SolarCalc : BaseCalc, ICelestialObjectCalc<Sun>
    {
        private const double TWILIGHT_ASTRONOMICAL  = 18;
        private const double TWILIGHT_NAUTICAL      = 12;
        private const double TWILIGHT_CIVIL         = 6;

        public Sun Sun { get; private set; } = new Sun();

        private LunarCalc lunarCalc;

        public SolarCalc(LunarCalc lunarCalc)
        {
            this.lunarCalc = lunarCalc;
        }

        public override void Calculate(SkyContext c)
        {
            Sun.Equatorial = c.Get(Equatorial);
            Sun.Horizontal = c.Get(Horizontal);
            Sun.Ecliptical = c.Get(Ecliptical);
            Sun.Semidiameter = c.Get(Semidiameter);
        }

        public CrdsEcliptical Ecliptical(SkyContext c)
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
        public CrdsEquatorial Equatorial0(SkyContext c)
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

        public CrdsHorizontal Horizontal(SkyContext c)
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

        private Date Seasons(SkyContext c, Season s)
        {
            return c.GetDate(SolarEphem.Season(c.JulianDay, s));
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

        /// <summary>
        /// Gets twilight information
        /// </summary>
        private RTS Twilight(SkyContext c, double altitude)
        {
            double jd = c.JulianDayMidnight;
            double theta0 = Date.ApparentSiderealTime(jd, c.NutationElements.deltaPsi, c.Epsilon);

            CrdsEquatorial[] eq = new CrdsEquatorial[3];
            double[] diff = new double[] { 0, 0.5, 1 };

            for (int i = 0; i < 3; i++)
            {
                eq[i] = new SkyContext(jd + diff[i], c.GeoLocation).Get(Equatorial0);
            }

            return Visibility.RiseTransitSet(eq, c.GeoLocation, theta0, c.Get(Parallax), altitude);
        }

        public void GetInfo(CelestialObjectInfo<Sun> info)
        {
            // TODO: move this to separate Eclipse calculator

            SunMoonPosition[] pos = new SunMoonPosition[5];
            for (int i = 0; i < 5; i++)
            {
                // 5 measurements with 3h step, so interval is -6...+6 hours
                SkyContext c = new SkyContext(info.Context.JulianDay + TimeSpan.FromHours(3).TotalDays * (i - 2), info.Context.GeoLocation);
                pos[i] = new SunMoonPosition()
                {
                    JulianDay = c.JulianDay,
                    Sun = c.Get(Equatorial0),
                    Moon = c.Get(lunarCalc.Equatorial0),
                    DistanceSun = c.Get(Ecliptical).Distance * 149597870 / 6371.0,
                    DistanceMoon = c.Get(lunarCalc.Ecliptical0).Distance / 6371.0
                };
            }

            var el = SolarEclipses.FindPolynomialBesselianElements(pos);

            var curves = SolarEclipses.GetCurves(el);

            var pts =
                //string.Join("\n", curves.TotalPath.Select(p => (p != null ? (p.Latitude).ToString("0.000000", CultureInfo.InvariantCulture) : null) + "," + (p != null ? (-p.Longitude).ToString("0.000000", CultureInfo.InvariantCulture) : null))) + "\n" +
                //string.Join("\n", curves.UmbraNorthernLimit.Select(p => (p != null ? (p.Latitude).ToString("0.000000", CultureInfo.InvariantCulture) : null) + "," + (p != null ? (-p.Longitude).ToString("0.000000", CultureInfo.InvariantCulture) : null))) + "\n" +
                //string.Join("\n", curves.UmbraSouthernLimit.Select(p => (p != null ? (p.Latitude).ToString("0.000000", CultureInfo.InvariantCulture) : null) + "," + (p != null ? (-p.Longitude).ToString("0.000000", CultureInfo.InvariantCulture) : null))) + "\n" +
                //string.Join("\n", curves.PenumbraNorthernLimit.Select(p => (p != null ? (p.Latitude).ToString("0.000000", CultureInfo.InvariantCulture) : null) + "," + (p != null ? (-p.Longitude).ToString("0.000000", CultureInfo.InvariantCulture) : null))) + "\n" +
                //string.Join("\n", curves.PenumbraSouthernLimit.Select(p => (p != null ? (p.Latitude).ToString("0.000000", CultureInfo.InvariantCulture) : null) + "," + (p != null ? (-p.Longitude).ToString("0.000000", CultureInfo.InvariantCulture) : null))) + "\n" +
                string.Join("\n", curves.RiseSetCurve[0].Select(p => (p != null ? (p.Latitude).ToString("0.000000", CultureInfo.InvariantCulture) : null) + "," + (p != null ? (-p.Longitude).ToString("0.000000", CultureInfo.InvariantCulture) : null))) + "\n" +
                string.Join("\n", curves.RiseSetCurve[1].Select(p => (p != null ? (p.Latitude).ToString("0.000000", CultureInfo.InvariantCulture) : null) + "," + (p != null ? (-p.Longitude).ToString("0.000000", CultureInfo.InvariantCulture) : null))) + "\n" +

            "";

            info.SetTitle(Sun.Name)

            .AddRow("Constellation")
            .AddHeader(Text.Get("Sun.Equatorial0"))
            .AddRow("Equatorial0.Alpha")
            .AddRow("Equatorial0.Delta")

            .AddHeader(Text.Get("Sun.Equatorial"))
            .AddRow("Equatorial.Alpha")
            .AddRow("Equatorial.Delta")

            .AddHeader(Text.Get("Sun.Ecliptical"))
            .AddRow("Ecliptical.Lambda")
            .AddRow("Ecliptical.Beta")

            .AddHeader(Text.Get("Sun.Horizontal"))
            .AddRow("Horizontal.Azimuth")
            .AddRow("Horizontal.Altitude")

            .AddHeader(Text.Get("Sun.RTS"))
            .AddRow("RTS.Rise")
            .AddRow("RTS.Transit")
            .AddRow("RTS.Set")
            .AddRow("RTS.Duration")

            .AddHeader(Text.Get("Sun.Twilight"))
            .AddRow("Twilight.Astronomical.Dawn")
            .AddRow("Twilight.Nautical.Dawn")
            .AddRow("Twilight.Civil.Dawn")
            .AddRow("Twilight.Civil.Dust")
            .AddRow("Twilight.Nautical.Dust")
            .AddRow("Twilight.Astronomical.Dust")            

            .AddHeader(Text.Get("Sun.Appearance"))
            .AddRow("Distance")
            .AddRow("HorizontalParallax")
            .AddRow("AngularDiameter")
            .AddRow("CRN")

            .AddHeader(Text.Get("Sun.Seasons"))
            .AddRow("Seasons.Spring")
            .AddRow("Seasons.Summer")
            .AddRow("Seasons.Autumn")
            .AddRow("Seasons.Winter");
        }

        public void ConfigureEphemeris(EphemerisConfig<Sun> e)
        {
            e["Constellation"] = (c, s) => Constellations.FindConstellation(c.Get(Equatorial), c.JulianDay);
            e["Equatorial0.Alpha"] = (c, s) => c.Get(Equatorial0).Alpha;
            e["Equatorial0.Delta"] = (c, s) => c.Get(Equatorial0).Delta;
            e["Equatorial.Alpha"] = (c, s) => c.Get(Equatorial).Alpha;
            e["Equatorial.Delta"] = (c, s) => c.Get(Equatorial).Delta;
            e["Ecliptical.Lambda"] = (c, s) => c.Get(Ecliptical).Lambda;
            e["Ecliptical.Beta"] = (c, s) => c.Get(Ecliptical).Beta;
            e["Horizontal.Altitude"] = (c, s) => c.Get(Horizontal).Altitude;
            e["Horizontal.Azimuth"] = (c, s) => c.Get(Horizontal).Azimuth;
            e["RTS.Rise"] = (c, s) => c.GetDateFromTime(c.Get(RiseTransitSet).Rise);
            e["RTS.Transit"] = (c, s) => c.GetDateFromTime(c.Get(RiseTransitSet).Transit);
            e["RTS.Set"] = (c, s) => c.GetDateFromTime(c.Get(RiseTransitSet).Set);
            e["RTS.Duration"] = (c, s) => c.Get(RiseTransitSet).Duration;
            e["Twilight.Astronomical.Dawn", Formatters.Time] = (c, s) => c.GetDateFromTime(c.Get(Twilight, TWILIGHT_ASTRONOMICAL).Rise);
            e["Twilight.Astronomical.Dust", Formatters.Time] = (c, s) => c.GetDateFromTime(c.Get(Twilight, TWILIGHT_ASTRONOMICAL).Set);
            e["Twilight.Nautical.Dawn", Formatters.Time] = (c, s) => c.GetDateFromTime(c.Get(Twilight, TWILIGHT_NAUTICAL).Rise);
            e["Twilight.Nautical.Dust", Formatters.Time] = (c, s) => c.GetDateFromTime(c.Get(Twilight, TWILIGHT_NAUTICAL).Set);
            e["Twilight.Civil.Dawn", Formatters.Time] = (c, s) => c.GetDateFromTime(c.Get(Twilight, TWILIGHT_CIVIL).Rise);
            e["Twilight.Civil.Dust", Formatters.Time] = (c, s) => c.GetDateFromTime(c.Get(Twilight, TWILIGHT_CIVIL).Set);
            e["Distance"] = (c, s) => c.Get(Ecliptical).Distance;
            e["HorizontalParallax"] = (c, x) => c.Get(Parallax);
            e["AngularDiameter"] = (c, x) => c.Get(Semidiameter) * 2 / 3600.0;
            e["CRN"] = (c, s) => c.Get(CarringtonNumber);
            e["Seasons.Spring", Formatters.DateTime] = (c, x) => c.Get(Seasons, Season.Spring);
            e["Seasons.Summer", Formatters.DateTime] = (c, x) => c.Get(Seasons, Season.Summer);
            e["Seasons.Autumn", Formatters.DateTime] = (c, x) => c.Get(Seasons, Season.Autumn);
            e["Seasons.Winter", Formatters.DateTime] = (c, x) => c.Get(Seasons, Season.Winter);            
        }

        public ICollection<SearchResultItem> Search(SkyContext context, string searchString, int maxCount = 50)
        {
            if (Sun.Name.StartsWith(searchString, StringComparison.OrdinalIgnoreCase))
                return new[] { new SearchResultItem(Sun, Sun.Name) };
            else
                return new SearchResultItem[0];
        }
    }
}
