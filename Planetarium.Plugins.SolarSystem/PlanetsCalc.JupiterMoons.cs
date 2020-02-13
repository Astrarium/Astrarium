using ADK;
using Planetarium.Objects;
using Planetarium.Types;
using Planetarium.Types.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Plugins.SolarSystem
{
    public partial class PlanetsCalc
    {
        private CrdsRectangular JupiterMoonRectangular(SkyContext c, int m)
        {
            return c.Get(JupiterMoonsPositions)[m - 1, 0];
        }

        private CrdsRectangular JupiterMoonRectangularS(SkyContext c, int m)
        {
            return c.Get(JupiterMoonsPositions)[m - 1, 1];
        }

        private CrdsRectangular[,] JupiterMoonsPositions(SkyContext c)
        {
            CrdsHeliocentrical earth = c.Get(EarthHeliocentrial);
            CrdsHeliocentrical jupiter = c.Get(Heliocentrical, Planet.JUPITER);
            return GalileanMoons.Positions(c.JulianDay, earth, jupiter);
        }

        private CrdsEquatorial JupiterMoonEquatorial(SkyContext c, int m)
        {
            CrdsEquatorial jupiterEq = c.Get(Equatorial, Planet.JUPITER);
            CrdsRectangular planetocentric = c.Get(JupiterMoonRectangular, m);
            PlanetAppearance appearance = c.Get(Appearance, Planet.JUPITER);
            double semidiameter = c.Get(Semidiameter, Planet.JUPITER);
            return planetocentric.ToEquatorial(jupiterEq, appearance.P, semidiameter);
        }

        private CrdsHorizontal JupiterMoonHorizontal(SkyContext c, int m)
        {
            return c.Get(JupiterMoonEquatorial, m).ToHorizontal(c.GeoLocation, c.SiderealTime);
        }

        private double JupiterMoonSemidiameter(SkyContext c, int m)
        {
            // distance from Earth to Jupiter, in a.u.
            double r = c.Get(DistanceFromEarth, Planet.JUPITER);

            // planetocentric z-coordinate of moon
            double z = c.Get(JupiterMoonRectangular, m).Z;

            // visible moon semidiameter
            return GalileanMoons.MoonSemidiameter(r, z, m - 1);
        }

        private double JupiterMoonCentralMeridian(SkyContext c, int m)
        {
            CrdsRectangular r = c.Get(JupiterMoonRectangular, m);
            return GalileanMoons.MoonCentralMeridian(r);
        }

        private float JupiterMoonMagnitude(SkyContext c, int m)
        {
            double r = c.Get(DistanceFromEarth, Planet.JUPITER);
            double R = c.Get(DistanceFromSun, Planet.JUPITER);
            double p = c.Get(Phase, Planet.JUPITER);
            return GalileanMoons.Magnitude(r, R, p, m - 1);
        }

        private double JupiterMoonDistanceFromEarth(SkyContext c, int m)
        {
            double r = c.Get(DistanceFromEarth, Planet.JUPITER);
            double z = c.Get(JupiterMoonRectangular, m).Z;
            return GalileanMoons.DistanceFromEarth(r, z);
        }

        public void ConfigureEphemeris(EphemerisConfig<JupiterMoon> e)
        {
            e["Constellation"] = (c, jm) => Constellations.FindConstellation(c.Get(JupiterMoonEquatorial, jm.Number), c.JulianDay);
            e["Equatorial.Alpha"] = (c, jm) => c.Get(JupiterMoonEquatorial, jm.Number).Alpha;
            e["Equatorial.Delta"] = (c, jm) => c.Get(JupiterMoonEquatorial, jm.Number).Delta;

            e["Horizontal.Altitude"] = (c, jm) => c.Get(JupiterMoonHorizontal, jm.Number).Altitude;
            e["Horizontal.Azimuth"] = (c, jm) => c.Get(JupiterMoonHorizontal, jm.Number).Azimuth;

            e["Rectangular.X"] = (c, jm) => c.Get(JupiterMoonRectangular, jm.Number).X;
            e["Rectangular.Y"] = (c, jm) => c.Get(JupiterMoonRectangular, jm.Number).Y;
            e["Rectangular.Z"] = (c, jm) => c.Get(JupiterMoonRectangular, jm.Number).Z;
            e["Magnitude"] = (c, jm) => c.Get(JupiterMoonMagnitude, jm.Number);

            e["Phase"] = (c, jm) => c.Get(Phase, jm.Number);
            e["PhaseAngle"] = (c, jm) => c.Get(PhaseAngle, jm.Number);
            e["AngularDiameter"] = (c, jm) => c.Get(JupiterMoonSemidiameter, jm.Number) * 2 / 3600.0;
            e["Appearance.CM"] = (c, jm) => c.Get(JupiterMoonCentralMeridian, jm.Number);

            e["RTS.Rise"] = (c, p) => c.GetDateFromTime(c.Get(RiseTransitSet, Planet.JUPITER).Rise);
            e["RTS.Transit"] = (c, p) => c.GetDateFromTime(c.Get(RiseTransitSet, Planet.JUPITER).Transit);
            e["RTS.Set"] = (c, p) => c.GetDateFromTime(c.Get(RiseTransitSet, Planet.JUPITER).Set);
            e["RTS.Duration"] = (c, p) => c.Get(RiseTransitSet, Planet.JUPITER).Duration;
        }

        public void GetInfo(CelestialObjectInfo<JupiterMoon> info)
        {
            info.SetSubtitle("Satellite of Jupiter").SetTitle(info.Body.Names.First())

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
