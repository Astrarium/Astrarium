using Astrarium.Algorithms;
using Astrarium.Plugins.SolarSystem.Objects;
using Astrarium.Types;
using Astrarium.Types.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.SolarSystem
{
    public partial class PlanetsCalc
    {
        // Mean obliquity of the ecliptic for J2000.0 epoch
        private const double epsilonJ2000 = 23.4392912510;

        /// <summary>
        /// Calculates heliocentrical coordinates of Earth for J2000 epoch
        /// </summary>
        private CrdsHeliocentrical Earth_HeliocentricalJ2000(SkyContext c)
        {
            return PlanetPositions.GetPlanetCoordinates(Planet.EARTH, c.JulianDay, highPrecision: !c.PreferFastCalculation, epochOfDate: false);
        }

        /// <summary>
        /// Calculates Sun rectangular coordinates for J2000 epoch
        /// </summary>
        private CrdsRectangular Sun_RectangularJ2000(SkyContext c)
        {
            // Heliocentrical coordinates of Earth
            CrdsHeliocentrical hEarth = c.Get(Earth_HeliocentricalJ2000);

            // transform to ecliptical coordinates of the Sun
            CrdsEcliptical eclSun = new CrdsEcliptical(Angle.To360(hEarth.L + 180), -hEarth.B, hEarth.R);

            // Sun rectangular coordinates, J2000.0 epoch
            CrdsRectangular rSun = eclSun.ToRectangular(epsilonJ2000);

            return rSun;
        }

        /// <summary>
        /// Calculates heliocentrical coordinates of Pluto for J2000 epoch
        /// </summary>
        private CrdsHeliocentrical Pluto_HeliocentricalJ2000(SkyContext c)
        {
            // final difference to stop iteration process, 1 second of time
            double deltaTau = TimeSpan.FromSeconds(1).TotalDays;

            // time taken by the light to reach the Earth
            double tau = 0;

            // previous value of tau to calculate the difference
            double tau0 = 1;

            // Sun rectangular coordinates, J2000.0 epoch
            CrdsRectangular rSun = c.Get(Sun_RectangularJ2000);

            // Heliocentrical coordinates of Pluto
            CrdsHeliocentrical plutoHeliocentrial = null;

            // Iterative process to find heliocentrical coordinates of planet
            while (Math.Abs(tau - tau0) > deltaTau)
            {
                // Heliocentrical coordinates of Pluto
                plutoHeliocentrial = PlutoPosition.Position(c.JulianDay - tau);

                // Rectangular heliocentrical coordinates of Pluto
                CrdsRectangular rPluto = new CrdsEcliptical(plutoHeliocentrial.L, plutoHeliocentrial.B, plutoHeliocentrial.R).ToRectangular(epsilonJ2000);

                double x = rPluto.X + rSun.X;
                double y = rPluto.Y + rSun.Y;
                double z = rPluto.Z + rSun.Z;
                double dist = Math.Sqrt(x * x + y * y + z * z);

                tau0 = tau;
                tau = PlanetPositions.LightTimeEffect(dist);
            }

            return plutoHeliocentrial;
        }

        /// <summary>
        /// Calculates ecliptical coordinates of Pluto for J2000.0 epoch
        /// </summary>
        private CrdsEcliptical Pluto_EclipticalJ2000(SkyContext c)
        {
            CrdsHeliocentrical plutoHeliocentrial = c.Get(Pluto_HeliocentricalJ2000);
            CrdsHeliocentrical earthHeliocentrical = c.Get(Earth_HeliocentricalJ2000);
            CrdsEcliptical plutoEcliptical = plutoHeliocentrial.ToRectangular(earthHeliocentrical).ToEcliptical();
            return plutoEcliptical;
        }

        /// <summary>
        /// Calculates equatorial geocentric coordinates of Pluto for current epoch
        /// </summary>
        private CrdsEquatorial Pluto_Equatorial0(SkyContext c)
        {
            PrecessionalElements pe = Precession.ElementsFK5(Date.EPOCH_J2000, c.JulianDay);
            CrdsEquatorial eq2000 = c.Get(Pluto_EclipticalJ2000).ToEquatorial(epsilonJ2000);
            return Precession.GetEquatorialCoordinates(eq2000, pe);
        }

        /// <summary>
        /// Calculates ecliptical geocentric coordinates of Pluto for current epoch
        /// </summary>
        private CrdsEcliptical Pluto_Ecliptical(SkyContext c)
        {
            CrdsEcliptical plutoEcliptical = c.Get(Pluto_Equatorial0).ToEcliptical(c.Epsilon);
            plutoEcliptical.Distance = c.Get(Pluto_EclipticalJ2000).Distance;
            return plutoEcliptical;
        }

        /// <summary>
        /// Calculate horizontal parallax of Pluto
        /// </summary>
        private double Pluto_Parallax(SkyContext c)
        {
            double distance = c.Get(Pluto_Ecliptical).Distance;
            return PlanetEphem.Parallax(distance);
        }

        /// <summary>
        /// Calculates equatorial topocentric coordinates of Pluto for current epoch
        /// </summary>
        private CrdsEquatorial Pluto_Equatorial(SkyContext c)
        {
            return c.Get(Pluto_Equatorial0).ToTopocentric(c.GeoLocation, c.SiderealTime, c.Get(Pluto_Parallax));
        }

        /// <summary>
        /// Calculates topocentric horizontal coordinates of Pluto
        /// </summary>
        private CrdsHorizontal Pluto_Horizontal(SkyContext c)
        {
            return c.Get(Pluto_Equatorial).ToHorizontal(c.GeoLocation, c.SiderealTime);
        }

        /// <summary>
        /// Calculates distance from Earth for Pluto
        /// </summary>
        private double Pluto_DistanceFromEarth(SkyContext c)
        {
            return c.Get(Pluto_EclipticalJ2000).Distance;
        }

        /// <summary>
        /// Calculates distance from Sun for Pluto
        /// </summary>
        private double Pluto_DistanceFromSun(SkyContext c)
        {
            return c.Get(Pluto_HeliocentricalJ2000).R;
        }

        /// <summary>
        /// Calculates apparent semidiameter of Pluto
        /// </summary>
        private double Pluto_Semidiameter(SkyContext c)
        {
            return PlutoPosition.Semidiameter(c.Get(Pluto_DistanceFromEarth));
        }

        /// <summary>
        /// Calculates visual magnitude of Pluto
        /// </summary>
        private float Pluto_Magnitude(SkyContext c)
        {
            double distanceFromEarth = c.Get(Pluto_DistanceFromEarth);
            double distanceFromSun = c.Get(Pluto_DistanceFromSun);
            return PlutoPosition.Magnitude(distanceFromEarth, distanceFromSun);
        }

        /// <summary>
        /// Calculates appearance details for Pluto
        /// </summary>
        private PlanetAppearance Pluto_Appearance(SkyContext c)
        {
            return PlanetEphem.PlanetAppearance(c.JulianDay, 9, c.Get(Pluto_Equatorial0), c.Get(Pluto_DistanceFromEarth));
        }

        /// <summary>
        /// Gets rise, transit and set info for Pluto
        /// </summary>
        private RTS Pluto_RiseTransitSet(SkyContext c)
        {
            double jd = c.JulianDayMidnight;
            double theta0 = Date.ApparentSiderealTime(jd, c.NutationElements.deltaPsi, c.Epsilon);
            double parallax = c.Get(Pluto_Parallax);

            CrdsEquatorial[] eq = new CrdsEquatorial[3];
            double[] diff = new double[] { 0, 0.5, 1 };

            for (int i = 0; i < 3; i++)
            {
                eq[i] = new SkyContext(jd + diff[i], c.GeoLocation).Get(Pluto_Equatorial0);
            }

            return Visibility.RiseTransitSet(eq, c.GeoLocation, theta0, parallax);
        }

        public VisibilityDetails Pluto_Visibility(SkyContext c)
        {
            double jd = c.JulianDayMidnight;
            double theta0 = Date.ApparentSiderealTime(jd, c.NutationElements.deltaPsi, c.Epsilon);
            double parallax = c.Get(Pluto_Parallax);

            var ctx = new SkyContext(jd, c.GeoLocation);
            var eq = ctx.Get(Pluto_Equatorial);
            var eqSun = ctx.Get(Sun_Equatorial);

            return Visibility.Details(eq, eqSun, c.GeoLocation, theta0, 5);
        }

        public void ConfigureEphemeris(EphemerisConfig<Pluto> e)
        {
            e["Constellation"] = (c, nm) => Constellations.FindConstellation(c.Get(Pluto_Equatorial), c.JulianDay);
            e["Equatorial.Alpha"] = (c, nm) => c.Get(Pluto_Equatorial).Alpha;
            e["Equatorial.Delta"] = (c, nm) => c.Get(Pluto_Equatorial).Delta;
            e["Equatorial0.Alpha"] = (c, nm) => c.Get(Pluto_Equatorial0).Alpha;
            e["Equatorial0.Delta"] = (c, nm) => c.Get(Pluto_Equatorial0).Delta;
            e["Ecliptical.Lambda"] = (c, p) => c.Get(Pluto_Ecliptical).Lambda;
            e["Ecliptical.Beta"] = (c, p) => c.Get(Pluto_Ecliptical).Beta;
            e["Horizontal.Altitude"] = (c, nm) => c.Get(Pluto_Horizontal).Altitude;
            e["Horizontal.Azimuth"] = (c, nm) => c.Get(Pluto_Horizontal).Azimuth;
            e["Magnitude"] = (c, nm) => c.Get(Pluto_Magnitude);
            e["DistanceFromEarth"] = (c, p) => c.Get(Pluto_DistanceFromEarth);
            e["DistanceFromSun"] = (c, p) => c.Get(Pluto_DistanceFromSun);
            e["HorizontalParallax"] = (c, p) => c.Get(Pluto_Parallax);
            e["AngularDiameter"] = (c, p) => c.Get(Pluto_Semidiameter) * 2 / 3600.0;
            e["Appearance.CM"] = (c, p) => c.Get(Pluto_Appearance).CM;
            e["Appearance.D"] = (c, p) => c.Get(Pluto_Appearance).D;
            e["Appearance.P"] = (c, p) => c.Get(Pluto_Appearance).P;
            e["RTS.Rise"] = (c, p) => c.GetDateFromTime(c.Get(Pluto_RiseTransitSet).Rise);
            e["RTS.Transit"] = (c, p) => c.GetDateFromTime(c.Get(Pluto_RiseTransitSet).Transit);
            e["RTS.Set"] = (c, p) => c.GetDateFromTime(c.Get(Pluto_RiseTransitSet).Set);
            e["RTS.Duration"] = (c, p) => c.Get(Pluto_RiseTransitSet).Duration;
            e["Visibility.Begin"] = (c, p) => c.GetDateFromTime(c.Get(Pluto_Visibility).Begin);
            e["Visibility.End"] = (c, p) => c.GetDateFromTime(c.Get(Pluto_Visibility).End);
            e["Visibility.Duration"] = (c, p) => c.Get(Pluto_Visibility).Duration;
            e["Visibility.Period"] = (c, p) => c.Get(Pluto_Visibility).Period;
        }

        public void GetInfo(CelestialObjectInfo<Pluto> info)
        {
            info
            .SetSubtitle(Text.Get("Pluto.Subtitle"))
            .SetTitle(info.Body.Names.First())

            .AddRow("Constellation")

            .AddHeader(Text.Get("Pluto.Horizontal"))
            .AddRow("Horizontal.Azimuth")
            .AddRow("Horizontal.Altitude")

            .AddHeader(Text.Get("Pluto.Equatorial"))
            .AddRow("Equatorial.Alpha")
            .AddRow("Equatorial.Delta")

            .AddHeader(Text.Get("Pluto.Equatorial0"))
            .AddRow("Equatorial0.Alpha")
            .AddRow("Equatorial0.Delta")

            .AddHeader(Text.Get("Pluto.Ecliptical"))
            .AddRow("Ecliptical.Lambda")
            .AddRow("Ecliptical.Beta")

            .AddHeader(Text.Get("Pluto.RTS"))
            .AddRow("RTS.Rise")
            .AddRow("RTS.Transit")
            .AddRow("RTS.Set")
            .AddRow("RTS.Duration")

            .AddHeader(Text.Get("Pluto.Visibility"))
            .AddRow("Visibility.Period")
            .AddRow("Visibility.Begin")
            .AddRow("Visibility.End")
            .AddRow("Visibility.Duration")

            .AddHeader(Text.Get("Pluto.Appearance"))
            .AddRow("Magnitude")
            .AddRow("DistanceFromEarth")
            .AddRow("DistanceFromSun")
            .AddRow("HorizontalParallax")
            .AddRow("AngularDiameter")
            .AddRow("Appearance.CM")
            .AddRow("Appearance.P")
            .AddRow("Appearance.D");
        }
    }
}
