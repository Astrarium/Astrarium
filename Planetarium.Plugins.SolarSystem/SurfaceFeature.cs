using ADK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Plugins.SolarSystem
{
    internal class SurfaceFeature
    {
        public CrdsGeographical Coordinates { get; private set; }
        public string Name { get; private set; }
        public float Diameter { get; private set; }
        public string TypeCode { get; private set; }

        public SurfaceFeature(string name, string type, double longitude, double latitude, double diameter)
        {
            Name = name;
            TypeCode = type;
            Coordinates = new CrdsGeographical(longitude, latitude);
            Diameter = (float)diameter;
        }
    }
}
