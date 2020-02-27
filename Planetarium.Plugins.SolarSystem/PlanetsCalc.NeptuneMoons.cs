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
        private CrdsEcliptical NeptuneMoon_Ecliptical(SkyContext c, int m)
        {
            var eclNeptune = c.Get(Planet_Ecliptical, Planet.NEPTUNE);
            return NeptunianMoons.Position(c.JulianDay, eclNeptune, m);
        }

        private CrdsEquatorial NeptuneMoon_Equatorial0(SkyContext c, int m)
        {
            return c.Get(NeptuneMoon_Ecliptical, m).ToEquatorial(c.Epsilon);
        }

        private CrdsEquatorial NeptuneMoon_Equatorial(SkyContext c, int m)
        {
            return c.Get(NeptuneMoon_Equatorial0, m).ToTopocentric(c.GeoLocation, c.SiderealTime, c.Get(Planet_Parallax, Planet.NEPTUNE));
        }

        private CrdsHorizontal NeptuneMoon_Horizontal(SkyContext c, int m)
        {
            return c.Get(NeptuneMoon_Equatorial, m).ToHorizontal(c.GeoLocation, c.SiderealTime);
        }

        private CrdsRectangular NeptuneMoon_Rectangular(SkyContext c, int m)
        {
            CrdsEquatorial moonEq = c.Get(NeptuneMoon_Equatorial, m);
            CrdsEcliptical moonEcl = c.Get(NeptuneMoon_Ecliptical, m);
            CrdsEquatorial neptuneEq = c.Get(Planet_Equatorial, Planet.NEPTUNE);
            CrdsEcliptical neptuneEcl = c.Get(Planet_Ecliptical, Planet.NEPTUNE);

            double sd = c.Get(Planet_Semidiameter, Planet.NEPTUNE) / 3600;
            double P = c.Get(Planet_Appearance, Planet.NEPTUNE).P;

            double[] alpha = new double[] { moonEq.Alpha, neptuneEq.Alpha };
            Angle.Align(alpha);

            double[] delta = new double[] { moonEq.Delta, neptuneEq.Delta };

            double x = (alpha[0] - alpha[1]) * Math.Cos(Angle.ToRadians(neptuneEq.Delta)) / sd;
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

            const double NEPTUNE_RADIUS = 24622;
            const double AU = 149597870;

            // z is expressed in Uranus equatorial radii
            double z = (moonEcl.Distance - neptuneEcl.Distance) / (2 * NEPTUNE_RADIUS / AU);

            return new CrdsRectangular(x, y, z);
        }

        private double NeptuneMoon_Semidiameter(SkyContext c, int m)
        {
            var ecl = c.Get(NeptuneMoon_Ecliptical, m);
            return NeptunianMoons.Semidiameter(ecl.Distance, m);
        }

        private float NeptuneMoon_Magnitude(SkyContext c, int m)
        {
            var delta = c.Get(Planet_DistanceFromEarth, Planet.NEPTUNE);
            double r = c.Get(Planet_DistanceFromSun, Planet.NEPTUNE);
            return NeptunianMoons.Magnitude(delta, r, m);
        }

        public void ConfigureEphemeris(EphemerisConfig<NeptuneMoon> e)
        {
            e["Constellation"] = (c, nm) => Constellations.FindConstellation(c.Get(NeptuneMoon_Equatorial, nm.Number), c.JulianDay);
            e["Equatorial.Alpha"] = (c, nm) => c.Get(NeptuneMoon_Equatorial, nm.Number).Alpha;
            e["Equatorial.Delta"] = (c, nm) => c.Get(NeptuneMoon_Equatorial, nm.Number).Delta;
            e["Horizontal.Altitude"] = (c, nm) => c.Get(NeptuneMoon_Horizontal, nm.Number).Altitude;
            e["Horizontal.Azimuth"] = (c, nm) => c.Get(NeptuneMoon_Horizontal, nm.Number).Azimuth;
            e["Rectangular.X"] = (c, nm) => c.Get(NeptuneMoon_Rectangular, nm.Number).X;
            e["Rectangular.Y"] = (c, nm) => c.Get(NeptuneMoon_Rectangular, nm.Number).Y;
            e["Rectangular.Z"] = (c, nm) => c.Get(NeptuneMoon_Rectangular, nm.Number).Z;
            e["AngularDiameter"] = (c, nm) => c.Get(NeptuneMoon_Semidiameter, nm.Number) * 2 / 3600.0;
            e["Magnitude"] = (c, nm) => c.Get(NeptuneMoon_Magnitude, nm.Number);
        }

        public void GetInfo(CelestialObjectInfo<NeptuneMoon> info)
        {
            info.SetSubtitle("Satellite of Neptune").SetTitle(info.Body.Names.First())

            .AddRow("Constellation")

            .AddHeader(Text.Get("NeptuneMoon.Horizontal"))
            .AddRow("Horizontal.Azimuth")
            .AddRow("Horizontal.Altitude")

            .AddHeader(Text.Get("NeptuneMoon.Equatorial"))
            .AddRow("Equatorial.Alpha")
            .AddRow("Equatorial.Delta")

            .AddHeader(Text.Get("NeptuneMoon.Rectangular"))
            .AddRow("Rectangular.X")
            .AddRow("Rectangular.Y")
            .AddRow("Rectangular.Z")

            .AddRow("Magnitude")
            .AddRow("AngularDiameter");
            //.AddHeader(Text.Get("NeptuneMoon.RTS"))
            //.AddRow("RTS.Rise")
            //.AddRow("RTS.Transit")
            //.AddRow("RTS.Set")
            //.AddRow("RTS.Duration");
        }
    }
}
