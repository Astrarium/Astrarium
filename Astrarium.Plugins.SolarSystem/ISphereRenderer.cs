using System.Drawing;

namespace Astrarium.Renderers
{
    public interface ISphereRenderer
    {
        Image Render(RendererOptions options);
    }

    /// <summary>
    /// Rendering options
    /// </summary>
    public class RendererOptions
    {
        /// <summary>
        /// Full path to the spherical texture.
        /// </summary>
        public string TextureFilePath { get; set; }

        /// <summary>
        /// Desired size of output image, in pixels.
        /// </summary>
        public uint OutputImageSize { get; set; }

        /// <summary>
        /// Angle of texture rotation in latitude, in degrees
        /// </summary>
        public double LatitudeShift { get; set; }

        /// <summary>
        /// Angle of texture rotation in longitude, in degrees
        /// </summary>
        public double LongutudeShift { get; set; }

        /// <summary>
        /// Flag indicating polar caps rendering is needed
        /// </summary>
        public bool RenderPolarCaps { get; set; }

        /// <summary>
        /// Radius of northern polar cap, in degrees
        /// </summary>
        public double NorthernPolarCap { get; set; }

        /// <summary>
        /// Radius of southern polar cap, in degrees
        /// </summary>
        public double SouthernPolarCap { get; set; }
    }
}