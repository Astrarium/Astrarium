using Astrarium.Types;
using System.Collections.Generic;

namespace Astrarium.Plugins.Satellites
{
    /// <summary>
    /// Artificial satellite
    /// </summary>
    public class Satellite : CelestialObject, IMagnitudeObject
    {
        /// <summary>
        /// Satellite primary name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Orbital data in TLE (two lines elements) format
        /// </summary>
        public TLE Tle { get; set; }

        /// <summary>
        /// TLE source names (files name without extension)
        /// </summary>
        public List<string> Sources { get; private set; } = new List<string>();
        
        /// <summary>
        /// Creates new instance of satellite
        /// </summary>
        /// <param name="name">Satellite primary name</param>
        /// <param name="tle">Orbital elements (two lines elements)</param>
        public Satellite(string name, TLE tle)
        {
            Name = name;
            Tle = tle;
        }

        /// <inheritdoc />
        public override string[] Names => new string[] { Name, Tle.SatelliteNumber, Tle.InternationalDesignator };

        /// <inheritdoc />
        public override string[] DisplaySettingNames => new string[] { "Satellites" };

        /// <inheritdoc />
        public override string Type => "Satellite";

        /// <inheritdoc />
        public override string CommonName => Tle.SatelliteNumber;

        /// <summary>
        /// Geocentric position vector
        /// </summary>
        public Vec3 Position { get; private set; } = new Vec3();

        /// <summary>
        /// Geocentric velocity vector
        /// </summary>
        public Vec3 Velocity { get; private set; } = new Vec3();

        /// <summary>
        /// Satellite magnitude
        /// </summary>
        public float Magnitude { get; set; }

        /// <summary>
        /// Standard magnitude of the satellite
        /// </summary>
        public float StdMag { get; set; }
    }
}
