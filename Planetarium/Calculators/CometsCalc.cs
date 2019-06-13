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
                comets[i].Magnitude = 6;
            }
        }

        protected override OrbitalElements OrbitalElements(SkyContext c, int i)
        {
            return comets[i].Orbit;
        }

        public void ConfigureEphemeris(EphemerisConfig<Comet> config)
        {
            
        }

        public CelestialObjectInfo GetInfo(SkyContext context, Comet body)
        {
            return new CelestialObjectInfo();
        }

        public ICollection<SearchResultItem> Search(SkyContext context, string searchString, int maxCount = 50)
        {
            return Comets
                .Where(c => c.Name.StartsWith(searchString))
                .Select(p => new SearchResultItem(p, p.Name)).ToArray();
        }

        public string GetName(Comet body)
        {
            return body.Name;
        }
    }
}
