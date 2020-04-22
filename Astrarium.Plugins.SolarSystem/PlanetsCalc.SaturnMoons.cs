using Astrarium.Algorithms;
using Astrarium.Plugins.SolarSystem.Objects;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.SolarSystem
{
    public partial class PlanetsCalc
    {
        private CrdsRectangular SaturnMoon_Rectangular(SkyContext c, int m)
        {
            return c.Get(SaturnMoons_Positions)[m - 1];
        }

        private CrdsRectangular[] SaturnMoons_Positions(SkyContext c)
        {
            CrdsHeliocentrical earth = c.Get(Earth_Heliocentrial);
            CrdsHeliocentrical saturn = c.Get(Planet_Heliocentrical, Planet.SATURN);
            return SaturnianMoons.Positions(c.JulianDay, earth, saturn);
        }

        private CrdsEquatorial SaturnMoon_Equatorial(SkyContext c, int m)
        {
            CrdsEquatorial saturnEq = c.Get(Planet_Equatorial, Planet.SATURN);
            CrdsRectangular planetocentric = c.Get(SaturnMoon_Rectangular, m);
            PlanetAppearance appearance = c.Get(Planet_Appearance, Planet.SATURN);
            double semidiameter = c.Get(Planet_Semidiameter, Planet.SATURN);
            return planetocentric.ToEquatorial(saturnEq, appearance.P, semidiameter);
        }

        private CrdsHorizontal SaturnMoon_Horizontal(SkyContext c, int m)
        {
            return c.Get(SaturnMoon_Equatorial, m).ToHorizontal(c.GeoLocation, c.SiderealTime);
        }

        private double SaturnMoon_Semidiameter(SkyContext c, int m)
        {
            // distance from Earth to  Saturn, in a.u.
            double r = c.Get(Planet_DistanceFromEarth, Planet.SATURN);

            // planetocentric z-coordinate of moon
            double z = c.Get(SaturnMoon_Rectangular, m).Z;

            // visible moon semidiameter
            return SaturnianMoons.MoonSemidiameter(r, z, m - 1);
        }

        private float SaturnMoon_Magnitude(SkyContext c, int m)
        {
            double r = c.Get(Planet_DistanceFromEarth, Planet.SATURN);
            double R = c.Get(Planet_DistanceFromSun, Planet.SATURN);
            return SaturnianMoons.Magnitude(r, R, m - 1);
        }

        private double SaturnMoon_DistanceFromEarth(SkyContext c, int m)
        {
            double r = c.Get(Planet_DistanceFromEarth, Planet.SATURN);
            double z = c.Get(SaturnMoon_Rectangular, m).Z;
            return SaturnianMoons.DistanceFromEarth(r, z);
        }

        public void ConfigureEphemeris(EphemerisConfig<SaturnMoon> e)
        {
            e["Constellation"] = (c, sm) => Constellations.FindConstellation(c.Get(SaturnMoon_Equatorial, sm.Number), c.JulianDay);
            e["Equatorial.Alpha"] = (c, sm) => c.Get(SaturnMoon_Equatorial, sm.Number).Alpha;
            e["Equatorial.Delta"] = (c, sm) => c.Get(SaturnMoon_Equatorial, sm.Number).Delta;

            e["Horizontal.Altitude"] = (c, sm) => c.Get(SaturnMoon_Horizontal, sm.Number).Altitude;
            e["Horizontal.Azimuth"] = (c, sm) => c.Get(SaturnMoon_Horizontal, sm.Number).Azimuth;

            e["Rectangular.X"] = (c, sm) => c.Get(SaturnMoon_Rectangular, sm.Number).X;
            e["Rectangular.Y"] = (c, sm) => c.Get(SaturnMoon_Rectangular, sm.Number).Y;
            e["Rectangular.Z"] = (c, sm) => c.Get(SaturnMoon_Rectangular, sm.Number).Z;
            e["Magnitude"] = (c, sm) => c.Get(SaturnMoon_Magnitude, sm.Number);
            e["AngularDiameter"] = (c, sm) => c.Get(SaturnMoon_Semidiameter, sm.Number) * 2 / 3600.0;

            e["RTS.Rise"] = (c, p) => c.GetDateFromTime(c.Get(Planet_RiseTransitSet, Planet.SATURN).Rise);
            e["RTS.Transit"] = (c, p) => c.GetDateFromTime(c.Get(Planet_RiseTransitSet, Planet.SATURN).Transit);
            e["RTS.Set"] = (c, p) => c.GetDateFromTime(c.Get(Planet_RiseTransitSet, Planet.SATURN).Set);
            e["RTS.Duration"] = (c, p) => c.Get(Planet_RiseTransitSet, Planet.SATURN).Duration;
        }

        public void GetInfo(CelestialObjectInfo<SaturnMoon> info)
        {
            info
            .SetSubtitle(Text.Get("Satellite.Subtitle", ("planetName", Text.Get($"Planet.6.GenitiveName"))))
            .SetTitle(info.Body.Names.First())

            .AddRow("Constellation")

            .AddHeader(Text.Get("SaturnMoon.Horizontal"))
            .AddRow("Horizontal.Azimuth")
            .AddRow("Horizontal.Altitude")

            .AddHeader(Text.Get("SaturnMoon.Equatorial"))
            .AddRow("Equatorial.Alpha")
            .AddRow("Equatorial.Delta")

            .AddHeader(Text.Get("SaturnMoon.Rectangular"))
            .AddRow("Rectangular.X")
            .AddRow("Rectangular.Y")
            .AddRow("Rectangular.Z")

            .AddHeader(Text.Get("SaturnMoon.RTS"))
            .AddRow("RTS.Rise")
            .AddRow("RTS.Transit")
            .AddRow("RTS.Set")
            .AddRow("RTS.Duration")

            .AddHeader(Text.Get("SaturnMoon.Appearance"))
            .AddRow("Magnitude")
            .AddRow("AngularDiameter");
        }
    }
}
