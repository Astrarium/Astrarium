using Astrarium.Algorithms;
using Astrarium.Plugins.SolarSystem.Objects;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.SolarSystem
{
    public partial class PlanetsCalc
    {
        private CrdsEcliptical GenericMoon_Ecliptical(SkyContext c, int id)
        {
            var moon = genericMoons.FirstOrDefault(gm => gm.Id == id);
            var eclPlanet = moon.Planet == Planet.PLUTO ? c.Get(Pluto_Ecliptical) : c.Get(Planet_Ecliptical, moon.Planet);
            var orbit = moon.Data;
            return GenericSatellite.Position(c.JulianDay, orbit, eclPlanet);
        }

        private CrdsEquatorial GenericMoon_Equatorial0(SkyContext c, int id)
        {
            return c.Get(GenericMoon_Ecliptical, id).ToEquatorial(c.Epsilon);
        }

        private CrdsEquatorial GenericMoon_Equatorial(SkyContext c, int id)
        {
            var moon = genericMoons.FirstOrDefault(gm => gm.Id == id);
            double parallax = moon.Planet == Planet.PLUTO ? c.Get(Pluto_Parallax) : c.Get(Planet_Parallax, moon.Planet);
            return c.Get(GenericMoon_Equatorial0, id).ToTopocentric(c.GeoLocation, c.SiderealTime, parallax);
        }

        private CrdsHorizontal GenericMoon_Horizontal(SkyContext c, int id)
        {
            return c.Get(GenericMoon_Equatorial, id).ToHorizontal(c.GeoLocation, c.SiderealTime);
        }

        private float GenericMoon_Semidiameter(SkyContext c, int id)
        {
            var ecl = c.Get(GenericMoon_Ecliptical, id);
            var radius = genericMoons.FirstOrDefault(gm => gm.Id == id).Data.radius;
            return (float)GenericSatellite.Semidiameter(ecl.Distance, radius);
        }

        private float GenericMoon_Magnitude(SkyContext c, int id)
        {
            var moon = genericMoons.FirstOrDefault(gm => gm.Id == id);
            var delta = moon.Planet == Planet.PLUTO ? c.Get(Pluto_DistanceFromEarth) : c.Get(Planet_DistanceFromEarth, moon.Planet);
            double r = moon.Planet == Planet.PLUTO ? c.Get(Pluto_DistanceFromSun) : c.Get(Planet_DistanceFromSun, moon.Planet);
            var mag0 = moon.Data.mag;
            return GenericSatellite.Magnitude(mag0, delta, r);
        }

        private float GenericMoon_RiseTransitSet(SkyContext c, int id)
        {
            var moon = genericMoons.FirstOrDefault(gm => gm.Id == id);
            var delta = moon.Planet == Planet.PLUTO ? c.Get(Pluto_DistanceFromEarth) : c.Get(Planet_DistanceFromEarth, moon.Planet);
            double r = moon.Planet == Planet.PLUTO ? c.Get(Pluto_DistanceFromSun) : c.Get(Planet_DistanceFromSun, moon.Planet);
            var mag0 = moon.Data.mag;
            return GenericSatellite.Magnitude(mag0, delta, r);
        }

        public void ConfigureEphemeris(EphemerisConfig<GenericMoon> e)
        {
            e["Constellation"] = (c, nm) => Constellations.FindConstellation(c.Get(GenericMoon_Equatorial, nm.Id), c.JulianDay);
            e["Equatorial.Alpha"] = (c, nm) => c.Get(GenericMoon_Equatorial, nm.Id).Alpha;
            e["Equatorial.Delta"] = (c, nm) => c.Get(GenericMoon_Equatorial, nm.Id).Delta;
            e["Horizontal.Altitude"] = (c, nm) => c.Get(GenericMoon_Horizontal, nm.Id).Altitude;
            e["Horizontal.Azimuth"] = (c, nm) => c.Get(GenericMoon_Horizontal, nm.Id).Azimuth;            
            e["AngularDiameter"] = (c, nm) => c.Get(GenericMoon_Semidiameter, nm.Id) * 2 / 3600.0;
            e["Magnitude"] = (c, nm) => c.Get(GenericMoon_Magnitude, nm.Id);
            e["RTS.Rise"] = (c, nm) => c.GetDateFromTime(c.Get(Pluto_RiseTransitSet).Rise);
            e["RTS.Transit"] = (c, nm) => c.GetDateFromTime(c.Get(Pluto_RiseTransitSet).Transit);
            e["RTS.Set"] = (c, nm) => c.GetDateFromTime(c.Get(Pluto_RiseTransitSet).Set);
            e["RTS.Duration"] = (c, nm) => c.Get(Pluto_RiseTransitSet).Duration;
        }

        public void GetInfo(CelestialObjectInfo<GenericMoon> info)
        {
            info
            .SetSubtitle(Text.Get("Satellite.Subtitle", ("planetName", Text.Get($"Planet.{info.Body.Data.planet}.GenitiveName"))))
            .SetTitle(info.Body.Names.First())

            .AddRow("Constellation")
            .AddHeader(Text.Get("GenericMoon.Horizontal"))
            .AddRow("Horizontal.Azimuth")
            .AddRow("Horizontal.Altitude")

            .AddHeader(Text.Get("GenericMoon.Equatorial"))
            .AddRow("Equatorial.Alpha")
            .AddRow("Equatorial.Delta")

            .AddHeader(Text.Get("GenericMoon.RTS"))
            .AddRow("RTS.Rise")
            .AddRow("RTS.Transit")
            .AddRow("RTS.Set")
            .AddRow("RTS.Duration")

            .AddHeader(Text.Get("GenericMoon.Appearance"))
            .AddRow("Magnitude")
            .AddRow("AngularDiameter")

            .AddHeader(Text.Get("GenericMoon.OrbitalElements"));

            decimal validityPeriod = settings.Get<decimal>("GenericMoonsOrbitalElementsValidity");
            //if (Math.Abs(info.Body.Data.jd - new Date(DateTime.Today).ToJulianDay()) > (double)validityPeriod)
            {
                info.AddRow(Text.Get("GenericMoon.OrbitalElements.Obsolete"), () => { 
                    // TODO: update
                }, Text.Get("GenericMoon.OrbitalElements.Update"));
            }

            info
            .AddRow("OrbitalElements.Epoch", Formatters.DateTime.Format(new Date(info.Body.Data.jd)))
            .AddRow("OrbitalElements.M", OrbitalElementsFormatters.M.Format(info.Body.Data.M))
            .AddRow("OrbitalElements.P", OrbitalElementsFormatters.P.Format(1 / info.Body.Data.n))
            .AddRow("OrbitalElements.n", OrbitalElementsFormatters.n.Format(info.Body.Data.n))
            .AddRow("OrbitalElements.e", OrbitalElementsFormatters.e.Format(info.Body.Data.e))
            .AddRow("OrbitalElements.a", OrbitalElementsFormatters.a.Format(info.Body.Data.a))
            .AddRow("OrbitalElements.i", OrbitalElementsFormatters.i.Format(info.Body.Data.i))
            .AddRow("OrbitalElements.w", OrbitalElementsFormatters.w.Format(info.Body.Data.w))
            .AddRow("OrbitalElements.Om", OrbitalElementsFormatters.Om.Format(info.Body.Data.Om))
            .AddRow("OrbitalElements.Pw", OrbitalElementsFormatters.Pw.Format(info.Body.Data.Pw))
            .AddRow("OrbitalElements.POm", OrbitalElementsFormatters.POm.Format(info.Body.Data.POm));
        }
    }
}
