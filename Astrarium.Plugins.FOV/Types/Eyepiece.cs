using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.FOV
{
    /// <summary>
    /// Represents eyepiece parameters
    /// </summary>
    public class Eyepiece
    {
        /// <summary>
        /// Equipment id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Eyepiece FOV, in degrees of arc
        /// </summary>
        public float FieldOfView { get; set; }

        /// <summary>
        /// Focal length, in mm
        /// </summary>
        public float FocalLength { get; set; }

        /// <summary>
        /// Eyepiece name, manufacturer, model, etc.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Copies properties from other instance
        /// </summary>
        /// <param name="other">Other Eyepiece instance</param>
        public void CopyFrom(Eyepiece other)
        {
            Id = other.Id;
            Name = other.Name;
            FieldOfView = other.FieldOfView;
            FocalLength = other.FocalLength;
        }
    }
}
