using ADK;
using Planetarium.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Calculators
{
    public interface ICometsProvider
    {
        ICollection<Comet> Comets { get; }
    }

    public class CometsCalc : MinorBodyCalc, ICelestialObjectCalc<Comet>, ICometsProvider
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

        public void ConfigureEphemeris(EphemerisConfig<Comet> e)
        {
            e.Add("Magnitude", (c, p) => c.Get(Magnitude, comets.IndexOf(p)));
            e.Add("Phase", (c, p) => c.Get(Phase, comets.IndexOf(p)));
            e.Add("PhaseAngle", (c, p) => c.Get(PhaseAngle, comets.IndexOf(p)));
            e.Add("DistanceFromEarth", (c, p) => c.Get(DistanceFromEarth, comets.IndexOf(p)));
            e.Add("DistanceFromSun", (c, p) => c.Get(DistanceFromSun, comets.IndexOf(p)));
            e.Add("Parallax", (c, p) => c.Get(Parallax, comets.IndexOf(p)));
            e.Add("Horizontal.Altitude", (c, p) => c.Get(Horizontal, comets.IndexOf(p)).Altitude);
            e.Add("Horizontal.Azimuth", (c, p) => c.Get(Horizontal, comets.IndexOf(p)).Azimuth);
            e.Add("EquatorialT.Alpha", (c, p) => c.Get(EquatorialT, comets.IndexOf(p)).Alpha);
            e.Add("EquatorialT.Delta", (c, p) => c.Get(EquatorialT, comets.IndexOf(p)).Delta);
            e.Add("Equatorial0.Alpha", (c, p) => c.Get(EquatorialJ2000, comets.IndexOf(p)).Alpha);
            e.Add("Equatorial0.Delta", (c, p) => c.Get(EquatorialJ2000, comets.IndexOf(p)).Delta);
            e.Add("EquatorialG.Alpha", (c, p) => c.Get(EquatorialG, comets.IndexOf(p)).Alpha);
            e.Add("EquatorialG.Delta", (c, p) => c.Get(EquatorialG, comets.IndexOf(p)).Delta);           
            e.Add("RTS.Rise", (c, p) => c.Get(RiseTransitSet, comets.IndexOf(p)).Rise);
            e.Add("RTS.Transit", (c, p) => c.Get(RiseTransitSet, comets.IndexOf(p)).Transit);
            e.Add("RTS.Set", (c, p) => c.Get(RiseTransitSet, comets.IndexOf(p)).Set);            
        }

        public CelestialObjectInfo GetInfo(SkyContext c, Comet comet)
        {
            int i = comets.IndexOf(comet);

            var rts = c.Get(RiseTransitSet, i);

            var info = new CelestialObjectInfo();
            info.SetSubtitle("Comet").SetTitle(GetName(comet))

            .AddRow("Constellation", Constellations.FindConstellation(c.Get(EquatorialT, i), c.JulianDay))

            .AddHeader("Equatorial coordinates (J2000.0)")
            .AddRow("Equatorial0.Alpha", c.Get(EquatorialJ2000, i).Alpha)
            .AddRow("Equatorial0.Delta", c.Get(EquatorialJ2000, i).Delta)

            .AddHeader("Equatorial coordinates (geocentrical)")
            .AddRow("Equatorial0.Alpha", c.Get(EquatorialG, i).Alpha)
            .AddRow("Equatorial0.Delta", c.Get(EquatorialG, i).Delta)

            .AddHeader("Equatorial coordinates (topocentrical)")
            .AddRow("Equatorial.Alpha", c.Get(EquatorialT, i).Alpha)
            .AddRow("Equatorial.Delta", c.Get(EquatorialT, i).Delta)

            .AddHeader("Horizontal coordinates")
            .AddRow("Horizontal.Azimuth", c.Get(Horizontal, i).Azimuth)
            .AddRow("Horizontal.Altitude", c.Get(Horizontal, i).Altitude)

            .AddHeader("Visibility")
            .AddRow("RTS.Rise", rts.Rise, c.JulianDayMidnight + rts.Rise)
            .AddRow("RTS.Transit", rts.Transit, c.JulianDayMidnight + rts.Transit)
            .AddRow("RTS.Set", rts.Set, c.JulianDayMidnight + rts.Set)

            .AddHeader("Appearance")
            .AddRow("Phase", c.Get(Phase, i))
            .AddRow("PhaseAngle", c.Get(PhaseAngle, i))
            .AddRow("Magnitude", c.Get(Magnitude, i))
            .AddRow("DistanceFromEarth", c.Get(DistanceFromEarth, i))
            .AddRow("DistanceFromSun", c.Get(DistanceFromSun, i))
            .AddRow("HorizontalParallax", c.Get(Parallax, i));

            return info;
        }

        public ICollection<SearchResultItem> Search(SkyContext context, string searchString, int maxCount = 50)
        {
            return Comets
                .Where(c => c.Name.Contains(searchString))
                .Select(p => new SearchResultItem(p, p.Name)).ToArray();
        }

        public string GetName(Comet body)
        {
            return body.Name;
        }
    }
}
