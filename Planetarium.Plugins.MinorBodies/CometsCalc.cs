using ADK;
using Planetarium.Objects;
using Planetarium.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Planetarium.Plugins.MinorBodies
{
    public class CometsCalc : MinorBodyCalc, ICelestialObjectCalc<Comet>
    {
        private readonly string ORBITAL_ELEMENTS_FILE = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/Comets.dat");
        private readonly CometsReader reader = new CometsReader();
        private readonly List<Comet> comets = new List<Comet>();

        public ICollection<Comet> Comets => comets;

        public override void Initialize()
        {
            comets.AddRange(reader.Read(ORBITAL_ELEMENTS_FILE));
        }

        public override void Calculate(SkyContext c)
        {
            for (int i = 0; i < comets.Count; i++)
            {
                comets[i].Horizontal = c.Get(Horizontal, i);
                comets[i].Magnitude = c.Get(Magnitude, i);
                comets[i].Semidiameter = c.Get(Appearance, i).Coma;
                comets[i].TailHorizontal = c.Get(TailHorizontal, i);
            }
        }

        protected override OrbitalElements OrbitalElements(SkyContext c, int i)
        {
            return comets[i].Orbit;
        }

        private float Magnitude(SkyContext c, int i)
        {
            var delta = c.Get(DistanceFromEarth, i);
            var r = c.Get(DistanceFromSun, i);
            return (float)(comets[i].H + 5 * Math.Log10(delta) + comets[i].G * Math.Log(r));
        } 

        private CometAppearance Appearance(SkyContext c, int i)
        {
            double H = comets[i].H;
            double K = comets[i].G;
            double r = c.Get(DistanceFromSun, i);
            double delta = c.Get(DistanceFromEarth, i);
            return MinorBodyEphem.CometAppearance(H, K, r, delta);
        }

        /// <summary>
        /// Gets equatorial coordinates of comet tail end
        /// </summary>
        private CrdsEquatorial TailEquatorial(SkyContext c, int i)
        {
            var rBody = c.Get(Rectangular, i);
            var rSun = c.Get(SunRectangular);

            // distance from Sun
            double r = Math.Sqrt(rBody.X * rBody.X + rBody.Y * rBody.Y + rBody.Z * rBody.Z);

            double k = (r + c.Get(Appearance, i).Tail) / r;

            double x = rSun.X + k * rBody.X;
            double y = rSun.Y + k * rBody.Y;
            double z = rSun.Z + k * rBody.Z;

            // distance from Earth of tail end
            double Delta = Math.Sqrt(x * x + y * y + z * z);

            double alpha = Angle.ToDegrees(Math.Atan2(y, x));
            double delta = Angle.ToDegrees(Math.Asin(z / Delta));

            // geocentric equatoral for J2000.0
            var eq0 = new CrdsEquatorial(alpha, delta);

            // Precessinal elements to convert between epochs
            var pe = c.Get(GetPrecessionalElements);

            // Equatorial coordinates for the mean equinox and epoch of the target date
            // No nutation an aberration corrections here, because we do not need high precision
            return Precession.GetEquatorialCoordinates(eq0, pe);
        }

        /// <summary>
        /// Calculates horizontal coordinates of comet tail end
        /// </summary>
        private CrdsHorizontal TailHorizontal(SkyContext c, int i)
        {
            var eq = c.Get(TailEquatorial, i);
            return eq.ToHorizontal(c.GeoLocation, c.SiderealTime);
        }

        public void ConfigureEphemeris(EphemerisConfig<Comet> e)
        {
            

            e["Constellation"] = (c, p) => Constellations.FindConstellation(c.Get(EquatorialT, comets.IndexOf(p)), c.JulianDay);
            e["Magnitude"] = (c, p) => c.Get(Magnitude, comets.IndexOf(p));
            e["Phase"] = (c, p) => c.Get(Phase, comets.IndexOf(p));
            e["PhaseAngle"] = (c, p) => c.Get(PhaseAngle, comets.IndexOf(p));
            e["DistanceFromEarth"] = (c, p) => c.Get(DistanceFromEarth, comets.IndexOf(p));
            e["DistanceFromSun"] = (c, p) => c.Get(DistanceFromSun, comets.IndexOf(p));
            e["HorizontalParallax"] = (c, p) => c.Get(Parallax, comets.IndexOf(p));
            e["Horizontal.Altitude"] = (c, p) => c.Get(Horizontal, comets.IndexOf(p)).Altitude;
            e["Horizontal.Azimuth"] = (c, p) => c.Get(Horizontal, comets.IndexOf(p)).Azimuth;
            e["Equatorial.Alpha"] = (c, p) => c.Get(EquatorialT, comets.IndexOf(p)).Alpha;
            e["Equatorial.Delta"] = (c, p) => c.Get(EquatorialT, comets.IndexOf(p)).Delta;
            e["Equatorial0.Alpha"] = (c, p) => c.Get(EquatorialJ2000, comets.IndexOf(p)).Alpha;
            e["Equatorial0.Delta"] = (c, p) => c.Get(EquatorialJ2000, comets.IndexOf(p)).Delta;
            e["Equatorial0T.Alpha", Formatters.RA] = (c, p) => c.Get(EquatorialJ2000T, comets.IndexOf(p)).Alpha;
            e["Equatorial0T.Delta", Formatters.Dec] = (c, p) => c.Get(EquatorialJ2000T, comets.IndexOf(p)).Delta;
            e["EquatorialG.Alpha", Formatters.RA] = (c, p) => c.Get(EquatorialG, comets.IndexOf(p)).Alpha;
            e["EquatorialG.Delta", Formatters.Dec] = (c, p) => c.Get(EquatorialG, comets.IndexOf(p)).Delta;           
            e["RTS.Rise"] = (c, p) => c.GetDateFromTime(c.Get(RiseTransitSet, comets.IndexOf(p)).Rise);
            e["RTS.Transit"] = (c, p) => c.GetDateFromTime(c.Get(RiseTransitSet, comets.IndexOf(p)).Transit);
            e["RTS.Set"] = (c, p) => c.GetDateFromTime(c.Get(RiseTransitSet, comets.IndexOf(p)).Set);
            e["RTS.Duration"] = (c, p) => c.Get(RiseTransitSet, comets.IndexOf(p)).Duration;
        }

        public void GetInfo(CelestialObjectInfo<Comet> info)
        {
            info
            .SetTitle(info.Body.Names.First())
            .SetSubtitle("Comet")

            .AddRow("Constellation")

            .AddHeader("Horizontal coordinates")
            .AddRow("Horizontal.Azimuth")
            .AddRow("Horizontal.Altitude")

            .AddHeader("Equatorial coordinates (topocentrical)")
            .AddRow("Equatorial.Alpha")
            .AddRow("Equatorial.Delta")

            .AddHeader("Equatorial coordinates (geocentrical)")
            .AddRow("EquatorialG.Alpha")
            .AddRow("EquatorialG.Delta")

            .AddHeader("Equatorial coordinates (topocentrical, J2000.0)")
            .AddRow("Equatorial0T.Alpha")
            .AddRow("Equatorial0T.Delta")

            .AddHeader("Equatorial coordinates (J2000.0)")
            .AddRow("Equatorial0.Alpha")
            .AddRow("Equatorial0.Delta")

            .AddHeader("Visibility")
            .AddRow("RTS.Rise")
            .AddRow("RTS.Transit")
            .AddRow("RTS.Set")
            .AddRow("RTS.Duration")

            .AddHeader("Appearance")
            .AddRow("Phase")
            .AddRow("PhaseAngle")
            .AddRow("Magnitude")
            .AddRow("DistanceFromEarth")
            .AddRow("DistanceFromSun")
            .AddRow("HorizontalParallax");
        }

        public ICollection<SearchResultItem> Search(SkyContext context, string searchString, int maxCount = 50)
        {
            return Comets
                .Where(c => GetNames(c.Name).Any(n => n.StartsWith(searchString, StringComparison.OrdinalIgnoreCase)))
                .Select(p => new SearchResultItem(p, p.Name)).ToArray();
        }

        private string[] GetNames(string name)
        {
            var match = Regex.Match(name, "^(.+)\\((.+)\\)$");
            if (match.Success)
            {
                return new string[] { match.Groups[1].Value.Trim(), match.Groups[2].Value.Trim() };
            }
            else
            {
                return name.Split('/').Select(n => n.Trim()).ToArray();
            }
        }
    }
}
