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
        private CrdsRectangular MarsMoon_Rectangular(SkyContext c, int m)
        {
            CrdsEquatorial moonEq = c.Get(MarsMoon_Equatorial, m);
            CrdsEcliptical moonEcl = c.Get(MarsMoon_Ecliptical, m);
            CrdsEquatorial marsEq = c.Get(Planet_Equatorial, Planet.MARS);
            CrdsEcliptical marsEcl = c.Get(Planet_Ecliptical, Planet.MARS);

            double sd = c.Get(Planet_Semidiameter, Planet.MARS) / 3600;
            double P = c.Get(Planet_Appearance, Planet.MARS).P;

            double[] alpha = new double[] { moonEq.Alpha, marsEq.Alpha };
            Angle.Align(alpha);

            double[] delta = new double[] { moonEq.Delta, marsEq.Delta };

            double x = (alpha[0] - alpha[1]) * Math.Cos(Angle.ToRadians(marsEq.Delta)) / sd;
            double y = (delta[0] - delta[1]) / sd;

            // radius-vector of moon, in planet's equatorial radii
            double r = Math.Sqrt(x * x + y * y);

            // rotation angle
            double theta = Angle.ToDegrees(Math.Atan2(x, y));

            // rotate with position angle of the planet
            theta -= P;

            // convert back to rectangular coordinates, but rotated with P angle:
            y = r * Math.Cos(Angle.ToRadians(theta));
            x = -r * Math.Sin(Angle.ToRadians(theta));

            const double MARS_RADIUS = 3390;
            const double AU = 149597870;

            // z is expressed in Mars equatorial radii
            double z = (moonEcl.Distance - marsEcl.Distance) / (2 * MARS_RADIUS / AU);

            return new CrdsRectangular(x, y, z);
        }

        private CrdsRectangular[] MarsMoons_Positions(SkyContext c)
        {
            CrdsHeliocentrical earth = c.Get(Earth_Heliocentrial);
            CrdsHeliocentrical mars = c.Get(Planet_Heliocentrical, Planet.MARS);
            return MartianMoons.Positions(c.JulianDay, earth, mars);
        }

        private CrdsEcliptical MarsMoon_Ecliptical(SkyContext c, int m)
        {
            var ecliptical = c.Get(MarsMoons_Positions)[m - 1].ToEcliptical();

            // Correction for FK5 system
            ecliptical += PlanetPositions.CorrectionForFK5(c.JulianDay, ecliptical);

            // Take nutation into account
            ecliptical += Nutation.NutationEffect(c.NutationElements.deltaPsi);

            return ecliptical;
        }

        private CrdsEquatorial MarsMoon_Equatorial0(SkyContext c, int m)
        {
            return c.Get(MarsMoon_Ecliptical, m).ToEquatorial(c.Epsilon);
        }

        private CrdsEquatorial MarsMoon_Equatorial(SkyContext c, int m)
        {
            return c.Get(MarsMoon_Equatorial0, m).ToTopocentric(c.GeoLocation, c.SiderealTime, c.Get(Planet_Parallax, Planet.MARS));
        }

        private CrdsHorizontal MarsMoon_Horizontal(SkyContext c, int m)
        {
            return c.Get(MarsMoon_Equatorial, m).ToHorizontal(c.GeoLocation, c.SiderealTime);
        }

        private double MarsMoon_Semidiameter(SkyContext c, int m)
        {
            var distance = c.Get(MarsMoon_Ecliptical, m).Distance;
            return MartianMoons.Semidiameter(m, distance);
        }

        public void ConfigureEphemeris(EphemerisConfig<MarsMoon> e)
        {
            e["Constellation"] = (c, um) => Constellations.FindConstellation(c.Get(MarsMoon_Equatorial, um.Number), c.JulianDay);
            e["Equatorial.Alpha"] = (c, um) => c.Get(MarsMoon_Equatorial, um.Number).Alpha;
            e["Equatorial.Delta"] = (c, um) => c.Get(MarsMoon_Equatorial, um.Number).Delta;

            e["Horizontal.Altitude"] = (c, um) => c.Get(MarsMoon_Horizontal, um.Number).Altitude;
            e["Horizontal.Azimuth"] = (c, um) => c.Get(MarsMoon_Horizontal, um.Number).Azimuth;

            e["Rectangular.X"] = (c, um) => c.Get(MarsMoon_Rectangular, um.Number).X;
            e["Rectangular.Y"] = (c, um) => c.Get(MarsMoon_Rectangular, um.Number).Y;
            e["Rectangular.Z"] = (c, um) => c.Get(MarsMoon_Rectangular, um.Number).Z;
            //e["Magnitude"] = (c, um) => c.Get(MarsMoon_Magnitude, um.Number);

            e["RTS.Rise"] = (c, p) => c.GetDateFromTime(c.Get(Planet_RiseTransitSet, Planet.MARS).Rise);
            e["RTS.Transit"] = (c, p) => c.GetDateFromTime(c.Get(Planet_RiseTransitSet, Planet.MARS).Transit);
            e["RTS.Set"] = (c, p) => c.GetDateFromTime(c.Get(Planet_RiseTransitSet, Planet.MARS).Set);
            e["RTS.Duration"] = (c, p) => c.Get(Planet_RiseTransitSet, Planet.MARS).Duration;
        }

        public void GetInfo(CelestialObjectInfo<MarsMoon> info)
        {
            info.SetSubtitle("Satellite of Mars").SetTitle(info.Body.Names.First())

            .AddRow("Constellation")

            .AddHeader(Text.Get("MarsMoon.Horizontal"))
            .AddRow("Horizontal.Azimuth")
            .AddRow("Horizontal.Altitude")

            .AddHeader(Text.Get("MarsMoon.Equatorial"))
            .AddRow("Equatorial.Alpha")
            .AddRow("Equatorial.Delta")

            .AddHeader(Text.Get("MarsMoon.Rectangular"))
            .AddRow("Rectangular.X")
            .AddRow("Rectangular.Y")
            .AddRow("Rectangular.Z")

            .AddHeader(Text.Get("MarsMoon.RTS"))
            .AddRow("RTS.Rise")
            .AddRow("RTS.Transit")
            .AddRow("RTS.Set")
            .AddRow("RTS.Duration");

            //.AddHeader(Text.Get("MarsMoon.Appearance"))
            //.AddRow("Phase")
            //.AddRow("PhaseAngle")
            //.AddRow("Magnitude")
            //.AddRow("AngularDiameter")
            //.AddRow("Appearance.CM");
        }
    }
}
