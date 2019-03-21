using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Objects
{
    public class Constellation
    {
        /// <summary>
        /// International abbreviated three-letter code of constellation.
        /// (+1 digit for "Ser1" and "Ser2" parts of Serpens constallation).
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Localized name of constellation
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Genitive name of constellation
        /// </summary>
        public string Genitive { get; set; }

        /// <summary>
        /// Coordinates of constellation label to be drawn on celestial map
        /// </summary>
        public CelestialPoint Label { get; set; }
    }
}
