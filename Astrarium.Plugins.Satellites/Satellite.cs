using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Satellites
{
    public class Satellite : CelestialObject
    {
        public string Name { get; private set; }
        public TLE Tle { get; private set; }
       
        public Satellite(string name, TLE tle)
        {
            Name = name;
            Tle = tle;
        }

        public override string[] Names => new string[] { Name, Tle.SatelliteNumber, Tle.InternationalDesignator };

        public override string[] DisplaySettingNames => new string[] { "Satellites" };

        public override string Type => "Satellite";

        public override string CommonName => "";

        public Vec3 Geocentric { get; set; }
        public Vec3 Topocentric { get; set; }
    }
}
