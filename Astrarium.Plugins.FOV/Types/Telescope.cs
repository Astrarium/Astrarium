using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.FOV
{
    /// <summary>
    /// Represents telescope parameters
    /// </summary>
    public class Telescope
    {
        /// <summary>
        /// Equipment id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Aperture, in mm
        /// </summary>
        public int Aperture { get; set; }

        /// <summary>
        /// Focal length, in mm
        /// </summary>
        public int FocalLength { get; set; }

        /// <summary>
        /// Telescope name, manufacturer, model, etc.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Copies properties from other instance
        /// </summary>
        /// <param name="other">Other Telescope instance</param>
        public void CopyFrom(Telescope other)
        {
            Id = other.Id;
            Name = other.Name;
            Aperture = other.Aperture;
            FocalLength = other.FocalLength;
        }
    }
}
