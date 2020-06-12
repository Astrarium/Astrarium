using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.FOV
{
    /// <summary>
    /// Represents binocular parameters
    /// </summary>
    public class Binocular
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
        /// Magnification
        /// </summary>
        public float Magnification { get; set; }

        /// <summary>
        /// Field of view, in degrees of arc
        /// </summary>
        public float FieldOfView { get; set; }

        /// <summary>
        /// Binocular name, manufacturer, model, etc.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Copies properties from other instance
        /// </summary>
        /// <param name="other">Other Binocular instance</param>
        public void CopyFrom(Binocular other)
        {
            Id = other.Id;
            Name = other.Name;
            Aperture = other.Aperture;
            Magnification = other.Magnification;
            FieldOfView = other.FieldOfView;
        }
    }
}
