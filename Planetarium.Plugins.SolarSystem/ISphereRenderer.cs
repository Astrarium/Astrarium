using System.Drawing;

namespace Planetarium.Renderers
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

        public double LatitudeShift { get; set; }

        public double LongutudeShift { get; set; }

        public double Flattening { get; set; }
    }
}