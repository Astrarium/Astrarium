using Astrarium.Algorithms;

namespace Astrarium.Types
{
    /// <summary>
    /// Base class for all physical objects that can be operated by the Astrarium app
    /// </summary>
    public abstract class CelestialObject
    {
        /// <summary>
        /// Current equatorial coordinates of the object
        /// </summary>
        public CrdsEquatorial Equatorial { get; set; }

        /// <summary>
        /// Gets array of celestial object names
        /// </summary>
        public abstract string[] Names { get; }

        /// <summary>
        /// Name of the setting(s) responsible for displaying the object
        /// </summary>
        public abstract string[] DisplaySettingNames { get; }

        /// <summary>
        /// Gets celestial object type, probably with subtype separated with dot,
        /// for example, "Planet" or "DeepSky.Galaxy".
        /// </summary>
        public abstract string Type { get; }

        /// <summary>
        /// Common name of the object, language-independent.
        /// In combination with object type (<see cref="Type"/>) should give a unique object identifier on the sky.
        /// </summary>
        public abstract string CommonName { get; }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is CelestialObject celestialObject)
            {
                return celestialObject.Type == Type && celestialObject.CommonName == CommonName;
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Type}/{CommonName}";
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}
