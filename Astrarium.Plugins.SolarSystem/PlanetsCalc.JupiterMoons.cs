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
        private CrdsRectangular JupiterMoon_Rectangular(SkyContext c, int m)
        {
            return c.Get(JupiterMoons_Positions)[m - 1, 0];
        }

        private CrdsRectangular JupiterMoonShadow_Rectangular(SkyContext c, int m)
        {
            return c.Get(JupiterMoons_Positions)[m - 1, 1];
        }

        private CrdsRectangular[,] JupiterMoons_Positions(SkyContext c)
        {
            CrdsHeliocentrical earth = c.Get(Earth_Heliocentrial);
            CrdsHeliocentrical jupiter = c.Get(Planet_Heliocentrical, Planet.JUPITER);
            double distance = jupiter.ToRectangular(earth).ToEcliptical().Distance;
            return GalileanMoons.Positions(c.JulianDay, earth, jupiter);
        }

        private CrdsEquatorial JupiterMoon_Equatorial(SkyContext c, int m)
        {
            CrdsEquatorial jupiterEq = c.Get(Planet_Equatorial, Planet.JUPITER);
            CrdsRectangular planetocentric = c.Get(JupiterMoon_Rectangular, m);
            PlanetAppearance appearance = c.Get(Planet_Appearance, Planet.JUPITER);
            double semidiameter = c.Get(Planet_Semidiameter, Planet.JUPITER);
            return planetocentric.ToEquatorial(jupiterEq, appearance.P, semidiameter);
        }

        private CrdsHorizontal JupiterMoon_Horizontal(SkyContext c, int m)
        {
            return c.Get(JupiterMoon_Equatorial, m).ToHorizontal(c.GeoLocation, c.SiderealTime);
        }

        private float JupiterMoon_Semidiameter(SkyContext c, int m)
        {
            // distance from Earth to Jupiter, in a.u.
            double r = c.Get(Planet_DistanceFromEarth, Planet.JUPITER);

            // planetocentric z-coordinate of moon
            double z = c.Get(JupiterMoon_Rectangular, m).Z;

            // visible moon semidiameter
            return (float)GalileanMoons.MoonSemidiameter(r, z, m - 1);
        }

        private double JupiterMoon_CentralMeridian(SkyContext c, int m)
        {
            CrdsRectangular r = c.Get(JupiterMoon_Rectangular, m);
            return GalileanMoons.MoonCentralMeridian(r);
        }

        private float JupiterMoon_Magnitude(SkyContext c, int m)
        {
            double r = c.Get(Planet_DistanceFromEarth, Planet.JUPITER);
            double R = c.Get(Planet_DistanceFromSun, Planet.JUPITER);
            double p = c.Get(Planet_Phase, Planet.JUPITER);
            return GalileanMoons.Magnitude(r, R, p, m - 1);
        }

        private double JupiterMoon_DistanceFromEarth(SkyContext c, int m)
        {
            double r = c.Get(Planet_DistanceFromEarth, Planet.JUPITER);
            double z = c.Get(JupiterMoon_Rectangular, m).Z;
            return GalileanMoons.DistanceFromEarth(r, z);
        }

        public void ConfigureEphemeris(EphemerisConfig<JupiterMoon> e)
        {
            e["Constellation"] = (c, jm) => Constellations.FindConstellation(c.Get(JupiterMoon_Equatorial, jm.Number), c.JulianDay);
            e["Equatorial.Alpha"] = (c, jm) => c.Get(JupiterMoon_Equatorial, jm.Number).Alpha;
            e["Equatorial.Delta"] = (c, jm) => c.Get(JupiterMoon_Equatorial, jm.Number).Delta;

            e["Horizontal.Altitude"] = (c, jm) => c.Get(JupiterMoon_Horizontal, jm.Number).Altitude;
            e["Horizontal.Azimuth"] = (c, jm) => c.Get(JupiterMoon_Horizontal, jm.Number).Azimuth;

            e["Rectangular.X"] = (c, jm) => c.Get(JupiterMoon_Rectangular, jm.Number).X;
            e["Rectangular.Y"] = (c, jm) => c.Get(JupiterMoon_Rectangular, jm.Number).Y;
            e["Rectangular.Z"] = (c, jm) => c.Get(JupiterMoon_Rectangular, jm.Number).Z;
            e["Magnitude"] = (c, jm) => c.Get(JupiterMoon_Magnitude, jm.Number);

            e["Phase"] = (c, jm) => c.Get(Planet_Phase, Planet.JUPITER);
            e["PhaseAngle"] = (c, jm) => c.Get(Planet_PhaseAngle, Planet.JUPITER);
            e["AngularDiameter"] = (c, jm) => c.Get(JupiterMoon_Semidiameter, jm.Number) * 2 / 3600.0;
            e["Appearance.CM"] = (c, jm) => c.Get(JupiterMoon_CentralMeridian, jm.Number);

            e["RTS.Rise"] = (c, p) => c.GetDateFromTime(c.Get(Planet_RiseTransitSet, Planet.JUPITER).Rise);
            e["RTS.Transit"] = (c, p) => c.GetDateFromTime(c.Get(Planet_RiseTransitSet, Planet.JUPITER).Transit);
            e["RTS.Set"] = (c, p) => c.GetDateFromTime(c.Get(Planet_RiseTransitSet, Planet.JUPITER).Set);
            e["RTS.Duration"] = (c, p) => c.Get(Planet_RiseTransitSet, Planet.JUPITER).Duration;
        }

        public void GetInfo(CelestialObjectInfo<JupiterMoon> info)
        {
            info
            .SetSubtitle(Text.Get("Satellite.Subtitle", ("planetName", Text.Get($"Planet.5.GenitiveName"))))
            .SetTitle(info.Body.Names.First())

            .AddRow("Constellation")

            .AddHeader(Text.Get("JupiterMoon.Horizontal"))
            .AddRow("Horizontal.Azimuth")
            .AddRow("Horizontal.Altitude")

            .AddHeader(Text.Get("JupiterMoon.Equatorial"))
            .AddRow("Equatorial.Alpha")
            .AddRow("Equatorial.Delta")

            .AddHeader(Text.Get("JupiterMoon.Rectangular"))
            .AddRow("Rectangular.X")
            .AddRow("Rectangular.Y")
            .AddRow("Rectangular.Z")

            .AddHeader(Text.Get("JupiterMoon.RTS"))
            .AddRow("RTS.Rise")
            .AddRow("RTS.Transit")
            .AddRow("RTS.Set")
            .AddRow("RTS.Duration")

            .AddHeader(Text.Get("JupiterMoon.Appearance"))
            .AddRow("Phase")
            .AddRow("PhaseAngle")
            .AddRow("Magnitude")
            .AddRow("AngularDiameter")
            .AddRow("Appearance.CM");
        }
    }
}
