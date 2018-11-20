using ADK.Demo.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo.Calculators
{
    public class PlanetsCalc : BaseSkyCalc
    {
        private Planet[] Planets = new Planet[8];

        public PlanetsCalc(Sky sky) : base(sky)
        {
            for (int i = 0; i < Planets.Length; i++)
            {
                Planets[i] = new Planet() { Serial = i + 1 };
            }

            Sky.AddDataProvider("Planets", () => Planets);
        }

        public override void Calculate()
        {
            
        }
    }
}
