using Astrarium.Types;

namespace Astrarium.Plugins.Satellites
{
    public class Satellite : CelestialObject, IMagnitudeObject
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

        public override string CommonName => Tle.SatelliteNumber;

        /// <summary>
        /// Geocentric position vector
        /// </summary>
        public Vec3 Position { get; private set; } = new Vec3();

        /// <summary>
        /// Geocentric velocity vector
        /// </summary>
        public Vec3 Velocity { get; private set; } = new Vec3();

        public float Magnitude { get; set; }

        public float StdMag { get; set; }
    }
}
