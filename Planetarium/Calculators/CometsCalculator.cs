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
    public class CometsCalculator : BaseCalc //, ICelestialObjectCalc<Comet>
    {
        private readonly string ORBITAL_ELEMENTS_FILE = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/Comets.dat");
        private readonly CometsReader reader = new CometsReader();
        private readonly List<Comet> comets = new List<Comet>();

        public override void Initialize()
        {
            comets.AddRange(reader.Read(ORBITAL_ELEMENTS_FILE));
        }

        public override void Calculate(SkyContext context)
        {
            
        }
    }
}
