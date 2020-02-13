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
        private CrdsRectangular UranusMoonRectangular(SkyContext c, int m)
        {
            CrdsEquatorial moonEq = c.Get(UranusMoonEquatorial, m);
            CrdsEcliptical moonEcl = c.Get(UranusMoonEcliptical, m);
            CrdsEquatorial uranusEq = c.Get(Equatorial, Planet.URANUS);
            CrdsEcliptical uranusEcl = c.Get(Ecliptical, Planet.URANUS);

            double sd = c.Get(Semidiameter, Planet.URANUS) / 3600;
            double P = c.Get(Appearance, Planet.URANUS).P;

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

        private CrdsRectangular[] UranusMoonsPositions(SkyContext c)
        {
            CrdsHeliocentrical earth = c.Get(EarthHeliocentrial);
            CrdsHeliocentrical uranus = c.Get(Heliocentrical, Planet.URANUS);
            return UranianMoons.Positions(c.JulianDay, earth, uranus);
        }

        private CrdsEcliptical UranusMoonEcliptical(SkyContext c, int m)
        {
            var ecliptical = c.Get(UranusMoonsPositions)[m - 1].ToEcliptical();

            // Correction for FK5 system
            ecliptical += PlanetPositions.CorrectionForFK5(c.JulianDay, ecliptical);

            // Take nutation into account
            ecliptical += Nutation.NutationEffect(c.NutationElements.deltaPsi);

            return ecliptical;
        }

        private CrdsEquatorial UranusMoonEquatorial(SkyContext c, int m)
        {
            return c.Get(UranusMoonEcliptical, m).ToEquatorial(c.Epsilon);
        }

        private CrdsHorizontal UranusMoonHorizontal(SkyContext c, int m)
        {
            return c.Get(UranusMoonEquatorial, m).ToHorizontal(c.GeoLocation, c.SiderealTime);
        }

        private double UranusMoonSemidiameter(SkyContext c, int m)
        {
            var distance = c.Get(UranusMoonEcliptical, m).Distance;
            return UranianMoons.Semidiameter(m, distance);
        }

        public void ConfigureEphemeris(EphemerisConfig<UranusMoon> e)
        {
            e["Constellation"] = (c, um) => Constellations.FindConstellation(c.Get(UranusMoonEquatorial, um.Number), c.JulianDay);
            e["Equatorial.Alpha"] = (c, um) => c.Get(UranusMoonEquatorial, um.Number).Alpha;
            e["Equatorial.Delta"] = (c, um) => c.Get(UranusMoonEquatorial, um.Number).Delta;

            e["Horizontal.Altitude"] = (c, um) => c.Get(UranusMoonHorizontal, um.Number).Altitude;
            e["Horizontal.Azimuth"] = (c, um) => c.Get(UranusMoonHorizontal, um.Number).Azimuth;

            e["Rectangular.X"] = (c, um) => c.Get(UranusMoonRectangular, um.Number).X;
            e["Rectangular.Y"] = (c, um) => c.Get(UranusMoonRectangular, um.Number).Y;
            e["Rectangular.Z"] = (c, um) => c.Get(UranusMoonRectangular, um.Number).Z;
            //e["Magnitude"] = (c, um) => c.Get(UranusMoonMagnitude, um.Number);

            e["RTS.Rise"] = (c, p) => c.GetDateFromTime(c.Get(RiseTransitSet, Planet.URANUS).Rise);
            e["RTS.Transit"] = (c, p) => c.GetDateFromTime(c.Get(RiseTransitSet, Planet.URANUS).Transit);
            e["RTS.Set"] = (c, p) => c.GetDateFromTime(c.Get(RiseTransitSet, Planet.URANUS).Set);
            e["RTS.Duration"] = (c, p) => c.Get(RiseTransitSet, Planet.URANUS).Duration;
        }

        public void GetInfo(CelestialObjectInfo<UranusMoon> info)
        {
            info.SetSubtitle("Satellite of Uranus").SetTitle(info.Body.Names.First())

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
            .AddRow("RTS.Duration");

            //.AddHeader(Text.Get("UranusMoon.Appearance"))
            //.AddRow("Phase")
            //.AddRow("PhaseAngle")
            //.AddRow("Magnitude")
            //.AddRow("AngularDiameter")
            //.AddRow("Appearance.CM");
        }
    }
}
