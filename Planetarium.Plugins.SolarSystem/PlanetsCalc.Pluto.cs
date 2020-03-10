using ADK;
using Planetarium.Objects;
using Planetarium.Types;
using Planetarium.Types.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Plugins.SolarSystem
{
    public partial class PlanetsCalc
    {
        // Mean obliquity of the ecliptic for J2000.0 epoch
        private const double epsilonJ2000 = 23.4392912510;

        private CrdsHeliocentrical Earth_HeliocentricalJ1000(SkyContext c)
        {
            return PlanetPositions.GetPlanetCoordinates(3, c.JulianDay, highPrecision: !c.PreferFastCalculation, epochOfDate: false);
        }

        private CrdsRectangular Sun_RectangularJ2000(SkyContext c)
        {
            // Heliocentrical coordinates of Earth
            CrdsHeliocentrical hEarth = c.Get(Earth_HeliocentricalJ1000);

            // transform to ecliptical coordinates of the Sun
            CrdsEcliptical eclSun = new CrdsEcliptical(Angle.To360(hEarth.L + 180), -hEarth.B, hEarth.R);

            // Sun rectangular coordinates, J2000.0 epoch
            CrdsRectangular rSun = eclSun.ToRectangular(epsilonJ2000);

            return rSun;
        }

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
            CrdsHeliocentrical earthHeliocentrical = c.Get(Earth_HeliocentricalJ1000);
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

        private CrdsEcliptical Pluto_Ecliptical(SkyContext c)
        {
            return c.Get(Pluto_Equatorial0).ToEcliptical(c.Epsilon);
        }

        /// <summary>
        /// Calculate parallax of Pluto
        /// </summary>
        private double Pluto_Parallax(SkyContext c)
        {
            double distance = c.Get(Pluto_EclipticalJ2000).Distance;
            return PlanetEphem.Parallax(distance);
        }

        /// <summary>
        /// Calculates equatorial topocentric coordinates of Pluto for current epoch
        /// </summary>
        private CrdsEquatorial Pluto_Equatorial(SkyContext c)
        {
            return c.Get(Pluto_Equatorial0).ToTopocentric(c.GeoLocation, c.SiderealTime, c.Get(Pluto_Parallax));
        }

        private CrdsHorizontal Pluto_Horizontal(SkyContext c)
        {
            return c.Get(Pluto_Equatorial).ToHorizontal(c.GeoLocation, c.SiderealTime);
        }

        private double Pluto_DistanceFromEarth(SkyContext c)
        {
            return c.Get(Pluto_EclipticalJ2000).Distance;
        }

        private double Pluto_DistanceFromSun(SkyContext c)
        {
            return c.Get(Pluto_HeliocentricalJ2000).R;
        }

        private double Pluto_Semidiameter(SkyContext c)
        {
            return PlutoPosition.Semidiameter(c.Get(Pluto_DistanceFromEarth));
        }

        private float Pluto_Magnitude(SkyContext c)
        {
            double distanceFromEarth = c.Get(Pluto_DistanceFromEarth);
            double distanceFromSun = c.Get(Pluto_DistanceFromSun);
            return PlutoPosition.Magnitude(distanceFromEarth, distanceFromSun);
        }

        private PlanetAppearance Pluto_Appearance(SkyContext c)
        {
            return PlanetEphem.PlanetAppearance(c.JulianDay, 9, c.Get(Pluto_Equatorial0), c.Get(Pluto_DistanceFromEarth));
        }

        public void ConfigureEphemeris(EphemerisConfig<Pluto> e)
        {
            e["Constellation"] = (c, nm) => Constellations.FindConstellation(c.Get(Pluto_Equatorial), c.JulianDay);
            e["Equatorial.Alpha"] = (c, nm) => c.Get(Pluto_Equatorial).Alpha;
            e["Equatorial.Delta"] = (c, nm) => c.Get(Pluto_Equatorial).Delta;
            e["Horizontal.Altitude"] = (c, nm) => c.Get(Pluto_Horizontal).Altitude;
            e["Horizontal.Azimuth"] = (c, nm) => c.Get(Pluto_Horizontal).Azimuth;            
            e["AngularDiameter"] = (c, nm) => c.Get(Pluto_Semidiameter) * 2 / 3600.0;
            e["Magnitude"] = (c, nm) => c.Get(Pluto_Magnitude);
        }

        public void GetInfo(CelestialObjectInfo<Pluto> info)
        {
            info
            .SetSubtitle($"Dwarf planet")
            .SetTitle(info.Body.Names.First())

            .AddRow("Constellation")

            .AddHeader(Text.Get("Pluto.Horizontal"))
            .AddRow("Horizontal.Azimuth")
            .AddRow("Horizontal.Altitude")

            .AddHeader(Text.Get("Pluto.Equatorial"))
            .AddRow("Equatorial.Alpha")
            .AddRow("Equatorial.Delta")

            .AddRow("Magnitude")
            .AddRow("AngularDiameter");
            //.AddHeader(Text.Get("Pluto.RTS"))
            //.AddRow("RTS.Rise")
            //.AddRow("RTS.Transit")
            //.AddRow("RTS.Set")
            //.AddRow("RTS.Duration");
        }
    }
}
