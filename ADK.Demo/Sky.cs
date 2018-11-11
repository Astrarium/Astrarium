﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo
{
    public class Sky
    {
        public double JulianDay { get; set; }
        public CrdsGeographical GeoLocation { get; set; }
        public double LocalSiderealTime { get; private set; }
        public NutationElements NutationElements { get; private set; }
        public double Epsilon { get; private set; }

        public ICollection<CelestialGrid> Grids { get; private set; } = new List<CelestialGrid>();

        public ICollection<BaseSkyCalc> Calculators { get; private set; } = new List<BaseSkyCalc>();

        public Sky()
        {
            JulianDay = new Date(DateTime.Now).ToJulianDay();
            GeoLocation = new CrdsGeographical(56.3333, 44);
            Calculate();
        }

        public void Calculate()
        {
            NutationElements = Nutation.NutationElements(JulianDay);
            Epsilon = Date.TrueObliquity(JulianDay, NutationElements.deltaEpsilon);
            LocalSiderealTime = Date.ApparentSiderealTime(JulianDay, NutationElements.deltaPsi, Epsilon);

            foreach (var calc in Calculators)
            {
                calc.Calculate();
            }
        }
    }
}