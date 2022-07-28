using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Database.Entities
{
    public class OpticsDB : IEntity
    {
        /// <summary>
        /// Empty element (equals to "Not selected")
        /// </summary>
        public static OpticsDB Empty = new OpticsDB() { Id = null };
        
        /// <inheritdoc />
        public string Id { get; set; }

        /// <summary>
        /// Aperture, in mm
        /// </summary>
        public double Aperture { get; set; }

        /// <summary>
        /// Vendor name
        /// </summary>
        public string Vendor { get; set; }

        /// <summary>
        /// Model name
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Optical scheme of optics. Not restricted to an enumeration to cover exotic constructions ;-)
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
        public string Scheme { get; set; }

        /// <summary>
        /// Transmission factor, light efficiency
        /// </summary>
        public double? LightGrasp { get; set; }

        /// <summary>
        /// Optional flag indicating the telescope gives erect orientation
        /// </summary>
        public bool? OrientationErect { get; set; }

        /// <summary>
        /// Optional flag indicating the telescope gives true-sided (non-mirrored) orientation
        /// </summary>
        public bool? OrientationTrueSided { get; set; }

        /// <summary>
        /// Only 2 types are used: Telescope, Fixed
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Object-type specific details, in JSON-serialized form
        /// </summary>
        public string Details { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Vendor} {Model}".Trim();
        }
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
