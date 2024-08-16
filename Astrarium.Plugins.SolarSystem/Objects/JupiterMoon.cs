using Astrarium.Algorithms;
using Astrarium.Types;

namespace Astrarium.Plugins.SolarSystem.Objects
{
    /// <summary>
    /// Contains coordinates and visual appearance data for the Galilean moon of Jupiter for given instant of time.
    /// </summary>
    public class JupiterMoon : SizeableCelestialObject, ISolarSystemObject, IMagnitudeObject, IObservableObject
    {
        public JupiterMoon(int number)
        {
            Number = number;
        }

        /// <summary>
        /// Common names of all Jupiter moons
        /// </summary>
        private static readonly string[] CommonNames = new string[] { "Io", "Europa", "Ganymede", "Callisto" };

        /// <inheritdoc />
        public override string Type => "PlanetMoon";

        /// <inheritdoc />
        public override string CommonName => CommonNames[Number - 1];

        /// <summary>
        /// Planetocentric rectangular coordinates of the Galilean moon
        /// </summary>
        public CrdsRectangular Rectangular { get; internal set; }

        /// <summary>
        /// Planetocentric rectangular coordinates of the Galilean moon, as seen from Sun
        /// </summary>
        public CrdsRectangular RectangularS { get; internal set; }

        /// <summary>
        /// Name of the Galilean moon
        /// </summary>
        public string Name => Text.Get($"JupiterMoon.{Number}.Name");

        /// <summary>
        /// Name of moon shadow
        /// </summary>
        public string ShadowName => Text.Get($"JupiterMoon.{Number}.Shadow");

        /// <summary>
        /// Gets Galilean moon names
        /// </summary>
        public override string[] Names => new[] { Name };

        /// <summary>
        /// Distance from Earth, in a.u.
        /// </summary>
        public double DistanceFromEarth { get; internal set; }

        /// <summary>
        /// Number of the moon
        /// </summary>
        public int Number { get; private set; }

        /// <summary>
        /// Longitude of central meridian
        /// </summary>
        public double CM { get; internal set; }

        /// <summary>
        /// Apparent magnitude
        /// </summary>
        public float Magnitude { get; internal set; }

        /// <summary>
        /// Flag indicating moon is eclipsed by the Jupiter
        /// </summary>
        public bool IsEclipsedByPlanet
        {
            get
            {
                return
                    RectangularS.Z > 0 && RectangularS.X * RectangularS.X + RectangularS.Y * RectangularS.Y * 1.14784224788 <= 1.1;
            }
        }

        /// <summary>
        /// Name of the setting(s) responsible for displaying the object
        /// </summary>
        public override string[] DisplaySettingNames => new[] { "Planets", "PlanetMoons" };
    }
}
