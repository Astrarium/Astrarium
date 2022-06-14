using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Database.Entities
{
    /// <summary>
    /// Defines an Eyepiece entity
    /// </summary>
    public class EyepieceDB : IEntity
    {
        /// <inheritdoc />
        public string Id { get; set; }

        /// <summary>
        /// Eyepiece model name
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Eyepiece vendor name
        /// </summary>
        public string Vendor { get; set; }

        /// <summary>
        /// Focal length, in mm
        /// </summary>
        public double FocalLength { get; set; }

        /// <summary>
        /// Specified in case of zoom eyepiece, maximal focal length in mm. Minimal focal length is stored in <see cref="FocalLength"/>.
        /// </summary>
        public double? FocalLengthMax { get; set; }

        /// <summary>
        /// Apparent field of view, in degrees.
        /// </summary>
        public double? ApparentFOV { get; set; }
    }
}
