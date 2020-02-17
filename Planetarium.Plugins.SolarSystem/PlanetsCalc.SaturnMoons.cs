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
        private CrdsRectangular SaturnMoonRectangular(SkyContext c, int m)
        {
            return c.Get(SaturnMoonsPositions)[m - 1];
        }

        private CrdsRectangular[] SaturnMoonsPositions(SkyContext c)
        {
            CrdsHeliocentrical earth = c.Get(EarthHeliocentrial);
            CrdsHeliocentrical saturn = c.Get(Heliocentrical, Planet.SATURN);
            return SaturnianMoons.Positions(c.JulianDay, earth, saturn);
        }

        private CrdsEquatorial SaturnMoonEquatorial(SkyContext c, int m)
        {
            CrdsEquatorial saturnEq = c.Get(Equatorial, Planet.SATURN);
            CrdsRectangular planetocentric = c.Get(SaturnMoonRectangular, m);
            PlanetAppearance appearance = c.Get(Appearance, Planet.SATURN);
            double semidiameter = c.Get(Semidiameter, Planet.SATURN);
            return planetocentric.ToEquatorial(saturnEq, appearance.P, semidiameter);
        }

        private CrdsHorizontal SaturnMoonHorizontal(SkyContext c, int m)
        {
            return c.Get(SaturnMoonEquatorial, m).ToHorizontal(c.GeoLocation, c.SiderealTime);
        }

        private double SaturnMoonSemidiameter(SkyContext c, int m)
        {
            // distance from Earth to  Saturn, in a.u.
            double r = c.Get(DistanceFromEarth, Planet.SATURN);

            // planetocentric z-coordinate of moon
            double z = c.Get(SaturnMoonRectangular, m).Z;

            // visible moon semidiameter
            return SaturnianMoons.MoonSemidiameter(r, z, m - 1);
        }

        private float SaturnMoonMagnitude(SkyContext c, int m)
        {
            double r = c.Get(DistanceFromEarth, Planet.SATURN);
            double R = c.Get(DistanceFromSun, Planet.SATURN);
            double p = c.Get(Phase, Planet.SATURN);
            return SaturnianMoons.Magnitude(r, R, p, m - 1);
        }

        private double SaturnMoonDistanceFromEarth(SkyContext c, int m)
        {
            double r = c.Get(DistanceFromEarth, Planet.SATURN);
            double z = c.Get(SaturnMoonRectangular, m).Z;
            return SaturnianMoons.DistanceFromEarth(r, z);
        }

        public void ConfigureEphemeris(EphemerisConfig<SaturnMoon> e)
        {
            e["Constellation"] = (c, sm) => Constellations.FindConstellation(c.Get(SaturnMoonEquatorial, sm.Number), c.JulianDay);
            e["Equatorial.Alpha"] = (c, sm) => c.Get(SaturnMoonEquatorial, sm.Number).Alpha;
            e["Equatorial.Delta"] = (c, sm) => c.Get(SaturnMoonEquatorial, sm.Number).Delta;

            e["Horizontal.Altitude"] = (c, sm) => c.Get(SaturnMoonHorizontal, sm.Number).Altitude;
            e["Horizontal.Azimuth"] = (c, sm) => c.Get(SaturnMoonHorizontal, sm.Number).Azimuth;

            e["Rectangular.X"] = (c, sm) => c.Get(SaturnMoonRectangular, sm.Number).X;
            e["Rectangular.Y"] = (c, sm) => c.Get(SaturnMoonRectangular, sm.Number).Y;
            e["Rectangular.Z"] = (c, sm) => c.Get(SaturnMoonRectangular, sm.Number).Z;
            e["Magnitude"] = (c, sm) => c.Get(SaturnMoonMagnitude, sm.Number);

            e["Phase"] = (c, sm) => c.Get(Phase, sm.Number);
            e["PhaseAngle"] = (c, sm) => c.Get(PhaseAngle, sm.Number);
            e["AngularDiameter"] = (c, sm) => c.Get(SaturnMoonSemidiameter, sm.Number) * 2 / 3600.0;

            e["RTS.Rise"] = (c, p) => c.GetDateFromTime(c.Get(RiseTransitSet, Planet.SATURN).Rise);
            e["RTS.Transit"] = (c, p) => c.GetDateFromTime(c.Get(RiseTransitSet, Planet.SATURN).Transit);
            e["RTS.Set"] = (c, p) => c.GetDateFromTime(c.Get(RiseTransitSet, Planet.SATURN).Set);
            e["RTS.Duration"] = (c, p) => c.Get(RiseTransitSet, Planet.SATURN).Duration;
        }

        public void GetInfo(CelestialObjectInfo<SaturnMoon> info)
        {
            info.SetSubtitle("Satellite of Saturn").SetTitle(info.Body.Names.First())

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
            .AddRow("Phase")
            .AddRow("PhaseAngle")
            .AddRow("Magnitude")
            .AddRow("AngularDiameter");
        }
    }
}
