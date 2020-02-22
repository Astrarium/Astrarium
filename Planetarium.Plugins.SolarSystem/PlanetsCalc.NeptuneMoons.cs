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
        private CrdsEcliptical NeptuneMoon_Ecliptical(SkyContext c)
        {
            var eclNeptune = c.Get(Planet_Ecliptical, Planet.NEPTUNE);
            return NeptunianMoons.TritonPosition(c.JulianDay, eclNeptune);
        }

        private CrdsEquatorial NeptuneMoon_Equatorial0(SkyContext c)
        {
            return c.Get(NeptuneMoon_Ecliptical).ToEquatorial(c.Epsilon);
        }

        private CrdsEquatorial NeptuneMoon_Equatorial(SkyContext c)
        {
            return c.Get(NeptuneMoon_Equatorial0).ToTopocentric(c.GeoLocation, c.SiderealTime, c.Get(Planet_Parallax, Planet.NEPTUNE));
        }

        private CrdsHorizontal NeptuneMoon_Horizontal(SkyContext c)
        {
            return c.Get(NeptuneMoon_Equatorial).ToHorizontal(c.GeoLocation, c.SiderealTime);
        }

        private CrdsRectangular NeptuneMoon_Rectangular(SkyContext c)
        {
            CrdsEquatorial moonEq = c.Get(NeptuneMoon_Equatorial);
            CrdsEcliptical moonEcl = c.Get(NeptuneMoon_Ecliptical);
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

        private double NeptuneMoon_Semidiameter(SkyContext c)
        {
            var ecl = c.Get(NeptuneMoon_Ecliptical);
            return NeptunianMoons.TritonSemidiameter(ecl.Distance);
        }

        public void ConfigureEphemeris(EphemerisConfig<NeptuneMoon> e)
        {
            e["Constellation"] = (c, jm) => Constellations.FindConstellation(c.Get(NeptuneMoon_Equatorial), c.JulianDay);
            e["Equatorial.Alpha"] = (c, jm) => c.Get(NeptuneMoon_Equatorial).Alpha;
            e["Equatorial.Delta"] = (c, jm) => c.Get(NeptuneMoon_Equatorial).Delta;

            e["Horizontal.Altitude"] = (c, jm) => c.Get(NeptuneMoon_Horizontal).Altitude;
            e["Horizontal.Azimuth"] = (c, jm) => c.Get(NeptuneMoon_Horizontal).Azimuth;
            e["Rectangular.X"] = (c, jm) => c.Get(NeptuneMoon_Rectangular).X;
            e["Rectangular.Y"] = (c, jm) => c.Get(NeptuneMoon_Rectangular).Y;
            e["Rectangular.Z"] = (c, jm) => c.Get(NeptuneMoon_Rectangular).Z;
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
            .AddRow("Rectangular.Z");

            //.AddHeader(Text.Get("NeptuneMoon.RTS"))
            //.AddRow("RTS.Rise")
            //.AddRow("RTS.Transit")
            //.AddRow("RTS.Set")
            //.AddRow("RTS.Duration");
        }
    }
}
