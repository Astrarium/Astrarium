using Astrarium.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Types
{
    /// <summary>
    /// Base class for all physical objects that can be operated by the Astrarium app
    /// </summary>
    public abstract class CelestialObject
    {
        /// <summary>
        /// Equatorial coordinates of the object
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
    }
}
