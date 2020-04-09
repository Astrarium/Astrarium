using Astrarium.Algorithms;
using Astrarium.Objects;
using Astrarium.Plugins.SolarSystem.Objects;
using Astrarium.Types;
using Astrarium.Types.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.SolarSystem
{
    public partial class PlanetsCalc
    {
        private CrdsRectangular UranusMoon_Rectangular(SkyContext c, int m)
        {
            CrdsEquatorial moonEq = c.Get(UranusMoon_Equatorial, m);
            CrdsEcliptical moonEcl = c.Get(UranusMoon_Ecliptical, m);
            CrdsEquatorial uranusEq = c.Get(Planet_Equatorial, Planet.URANUS);
            CrdsEcliptical uranusEcl = c.Get(Planet_Ecliptical, Planet.URANUS);

            double sd = c.Get(Planet_Semidiameter, Planet.URANUS) / 3600;
            double P = c.Get(Planet_Appearance, Planet.URANUS).P;

            double[] alpha = new double[] { moonEq.Alpha, uranusEq.Alpha };
            Angle.Align(alpha);

            double[] delta = new double[] { moonEq.Delta, uranusEq.Delta };

            double x = (alpha[0] - alpha[1]) * Math.Cos(Angle.ToRadians(uranusEq.Delta)) / sd;
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

            const double URANUS_RADIUS = 25559;
            const double AU = 149597870;

            // z is expressed in Uranus equatorial radii
            double z = (moonEcl.Distance - uranusEcl.Distance) / (2 * URANUS_RADIUS / AU);

            return new CrdsRectangular(x, y, z);
        }

        private CrdsRectangular[] UranusMoons_Positions(SkyContext c)
        {
            CrdsHeliocentrical earth = c.Get(Earth_Heliocentrial);
            CrdsHeliocentrical uranus = c.Get(Planet_Heliocentrical, Planet.URANUS);
            return UranianMoons.Positions(c.JulianDay, earth, uranus);
        }

        private CrdsEcliptical UranusMoon_Ecliptical(SkyContext c, int m)
        {
            var ecliptical = c.Get(UranusMoons_Positions)[m - 1].ToEcliptical();

            // Correction for FK5 system
            ecliptical += PlanetPositions.CorrectionForFK5(c.JulianDay, ecliptical);

            // Take nutation into account
            ecliptical += Nutation.NutationEffect(c.NutationElements.deltaPsi);

            return ecliptical;
        }

        private CrdsEquatorial UranusMoon_Equatorial0(SkyContext c, int m)
        {
            return c.Get(UranusMoon_Ecliptical, m).ToEquatorial(c.Epsilon);
        }

        private CrdsEquatorial UranusMoon_Equatorial(SkyContext c, int m)
        {
            return c.Get(UranusMoon_Equatorial0, m).ToTopocentric(c.GeoLocation, c.SiderealTime, c.Get(Planet_Parallax, Planet.URANUS));
        }

        private CrdsHorizontal UranusMoon_Horizontal(SkyContext c, int m)
        {
            return c.Get(UranusMoon_Equatorial, m).ToHorizontal(c.GeoLocation, c.SiderealTime);
        }

        private double UranusMoon_Semidiameter(SkyContext c, int m)
        {
            var distance = c.Get(UranusMoon_Ecliptical, m).Distance;
            return UranianMoons.Semidiameter(m, distance);
        }

        private float UranusMoon_Magnitude(SkyContext c, int m)
        {
            var distanceFromEarth = c.Get(Planet_DistanceFromEarth, Planet.URANUS);
            var distanceFromSun = c.Get(Planet_DistanceFromSun, Planet.URANUS);
            return UranianMoons.Magnitude(distanceFromEarth, distanceFromSun, m - 1);
        }

        public void ConfigureEphemeris(EphemerisConfig<UranusMoon> e)
        {
            e["Constellation"] = (c, um) => Constellations.FindConstellation(c.Get(UranusMoon_Equatorial, um.Number), c.JulianDay);
            e["Equatorial.Alpha"] = (c, um) => c.Get(UranusMoon_Equatorial, um.Number).Alpha;
            e["Equatorial.Delta"] = (c, um) => c.Get(UranusMoon_Equatorial, um.Number).Delta;

            e["Horizontal.Altitude"] = (c, um) => c.Get(UranusMoon_Horizontal, um.Number).Altitude;
            e["Horizontal.Azimuth"] = (c, um) => c.Get(UranusMoon_Horizontal, um.Number).Azimuth;

            e["Rectangular.X"] = (c, um) => c.Get(UranusMoon_Rectangular, um.Number).X;
            e["Rectangular.Y"] = (c, um) => c.Get(UranusMoon_Rectangular, um.Number).Y;
            e["Rectangular.Z"] = (c, um) => c.Get(UranusMoon_Rectangular, um.Number).Z;

            e["AngularDiameter"] = (c, um) => c.Get(UranusMoon_Semidiameter, um.Number) * 2 / 3600.0;
            e["Magnitude"] = (c, um) => c.Get(UranusMoon_Magnitude, um.Number);

            e["RTS.Rise"] = (c, p) => c.GetDateFromTime(c.Get(Planet_RiseTransitSet, Planet.URANUS).Rise);
            e["RTS.Transit"] = (c, p) => c.GetDateFromTime(c.Get(Planet_RiseTransitSet, Planet.URANUS).Transit);
            e["RTS.Set"] = (c, p) => c.GetDateFromTime(c.Get(Planet_RiseTransitSet, Planet.URANUS).Set);
            e["RTS.Duration"] = (c, p) => c.Get(Planet_RiseTransitSet, Planet.URANUS).Duration;
        }

        public void GetInfo(CelestialObjectInfo<UranusMoon> info)
        {
            info
            .SetSubtitle(Text.Get("Satellite.Subtitle", ("planetName", Text.Get($"Planet.7.GenitiveName"))))
            .SetTitle(info.Body.Names.First())

            .AddRow("Constellation")

            .AddHeader(Text.Get("UranusMoon.Horizontal"))
            .AddRow("Horizontal.Azimuth")
            .AddRow("Horizontal.Altitude")

            .AddHeader(Text.Get("UranusMoon.Equatorial"))
            .AddRow("Equatorial.Alpha")
            .AddRow("Equatorial.Delta")

            .AddHeader(Text.Get("UranusMoon.Rectangular"))
            .AddRow("Rectangular.X")
            .AddRow("Rectangular.Y")
            .AddRow("Rectangular.Z")

            .AddHeader(Text.Get("UranusMoon.RTS"))
            .AddRow("RTS.Rise")
            .AddRow("RTS.Transit")
            .AddRow("RTS.Set")
            .AddRow("RTS.Duration")

            .AddHeader(Text.Get("UranusMoon.Appearance"))
            //.AddRow("Phase")
            //.AddRow("PhaseAngle")
            .AddRow("Magnitude")
            .AddRow("AngularDiameter");
            //.AddRow("Appearance.CM");
        }
    }
}
