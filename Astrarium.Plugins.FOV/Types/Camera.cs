using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.FOV
{
    /// <summary>
    /// Represents camera parameters
    /// </summary>
    public class Camera
    {
        /// <summary>
        /// Equipment id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Horizontal resolution, in pixels
        /// </summary>
        public int HorizontalResolution { get; set; }

        /// <summary>
        /// Vertical resolution, in pixels
        /// </summary>
        public int VerticalResolution { get; set; }

        /// <summary>
        /// Pixel size width, in micrometers (µm)
        /// </summary>
        public float PixelSizeWidth { get; set; }

        /// <summary>
        /// Pixel size height, in micrometers (µm)
        /// </summary>
        public float PixelSizeHeight { get; set; }

        /// <summary>
        /// Camera name, manufacturer, model, etc.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Copies properties from other instance
        /// </summary>
        /// <param name="other">Other Camera instance</param>
        public void CopyFrom(Camera other)
        {
            Id = other.Id;
            Name = other.Name;
            VerticalResolution = other.VerticalResolution;
            HorizontalResolution = other.HorizontalResolution;
            PixelSizeHeight = other.PixelSizeHeight;
            PixelSizeWidth = other.PixelSizeWidth;
        }
    }
}
