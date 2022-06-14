using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Database.Entities
{
    public class OpticsDB : IEntity
    {
        public string Id { get; set; }
        public double Aperture { get; set; }
        public string Model { get; set; }
        public string Vendor { get; set; }

        /// <summary>
        /// Type of optics. Not restricted to an enumeration to cover exotic constructions ;-)
        /// The type is optional but should be given if known!
        /// When applicable, the following coding(according to the DSL) should be used:
        /// A: Naked eye
        /// C: Cassegrain
        /// B: Binoculars
        /// S: Schmidt-Cassegrain
        /// N: Newton 
        /// K: Kutter (Schiefspiegler)
        /// R: Refractor 
        /// M: Maksutov
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Transmission factor, light efficiency
        /// </summary>
        public double? LightGrasp { get; set; }

        public bool? OrientationErect { get; set; }
        
        public bool? OrientationTrueSided { get; set; }

        /// <summary>
        /// Only 2 types are used: Telescope, Fixed
        /// </summary>
        public string OpticsType { get; set; }

        /// <summary>
        /// Object-type specific details
        /// </summary>
        public string Details { get; set; }
    }

    public class ScopeDetails
    {
        public double FocalLength { get; set; }
    }

    public class FixedOpticsDetails
    {
        public double Magnification { get; set; }
        public double? TrueField { get; set; }
    }
}
