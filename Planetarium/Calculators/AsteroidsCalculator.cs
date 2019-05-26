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
    public class AsteroidsCalculator : BaseCalc
    {
        private readonly string ORBITAL_ELEMENTS_FILE = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/Asteroids.dat");

        private AsteroidsReader reader = new AsteroidsReader();
        private ICollection<Asteroid> asteroids;

        public override void Initialize()
        {
            asteroids = reader.Read(ORBITAL_ELEMENTS_FILE);
        }

        public override void Calculate(SkyContext context)
        {
            
        }
    }
}
