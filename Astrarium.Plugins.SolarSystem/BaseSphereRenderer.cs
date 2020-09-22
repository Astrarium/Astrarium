using Astrarium.Types;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Astrarium.Plugins.SolarSystem
{
    /// <summary>
    /// Base class for Sphere renderers.
    /// </summary>
    public abstract class BaseSphereRenderer
    {
        /// <summary>
        /// Creates image representing celestial body sphere as it visible from Earth.
        /// </summary>
        /// <param name="options">Rendering options</param>
        /// <returns>Image instance</returns>
        public abstract Image Render(RendererOptions options);

        /// <summary>
        /// Creates initial bitmap to be used as sphere texture
        /// </summary>
        /// <param name="options">Rendering options</param>
        /// <returns>Bitmap instance</returns>
        protected Bitmap CreateTextureBitmap(RendererOptions options)
        {
            Bitmap sourceBitmap = new Bitmap(options.TextureFilePath);

            // render martian polar caps
            if (options.RenderPolarCaps)
            {
                using (Graphics g = Graphics.FromImage(sourceBitmap))
                {
                    const float df = 2.5f / 180f;
                    {
                        float f = (float)(options.NorthernPolarCap / 180.0);
                        var br = new LinearGradientBrush(new Point(0, 0), new PointF(0, sourceBitmap.Height), Color.White, Color.Transparent);
                        br.Blend = new Blend(4) { Factors = new float[] { 0, 0, 1, 1 }, Positions = new float[] { 0, Math.Max(0, f - df), Math.Min(1, f + df), 1 } };
                        g.FillRectangle(br, 0, 0, sourceBitmap.Width, sourceBitmap.Height);
                    }
                    {
                        float f = (float)((180 - options.SouthernPolarCap) / 180.0);
                        var br = new LinearGradientBrush(new Point(0, 0), new PointF(0, sourceBitmap.Height), Color.Transparent, Color.White);
                        br.Blend = new Blend(4) { Factors = new float[] { 0, 0, 1, 1 }, Positions = new float[] { 0, Math.Max(0, f - df), Math.Min(1, f + df), 1 } };
                        g.FillRectangle(br, 0, 0, sourceBitmap.Width, sourceBitmap.Height);
                    }
                }
            }

            sourceBitmap.Colorize(options.ColorSchema);

            return sourceBitmap;
        }
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

        /// <summary>
        /// Color schema
        /// </summary>
        public ColorSchema ColorSchema { get; set; }
    }
}