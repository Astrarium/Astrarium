﻿using Astrarium.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.SolarSystem.Objects
{
    public interface IPlanet
    {
        int Number { get; }

        string Name { get; }

        CrdsEquatorial Equatorial { get; }

        CrdsEcliptical Ecliptical { get; }

        float Flattening { get; }

        double Elongation { get; }

        double Phase { get; }

        float Magnitude { get; }

        PlanetAppearance Appearance { get; }
    }
}
