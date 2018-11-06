using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ADK;

namespace ADK.Demo
{
    /// <summary>
    /// Defines an interface of Sky Map canvas to render celestial map.
    /// </summary>
    public interface ISkyMap
    {
        /// <summary>
        /// Gets or sets width of the canvas, in pixels
        /// </summary>
        int Width { get; set; }

        /// <summary>
        /// Gets or sets height of the canvas, in pixels
        /// </summary>
        int Height { get; set; }

        /// <summary>
        /// Gets or sets current field of view, in degrees
        /// </summary>
        double ViewAngle { get; set; }

        /// <summary>
        /// Gets or sets horizontal coordinates of the central point of the canvas.
        /// </summary>
        CrdsHorizontal Center { get; set; }

        /// <summary>
        /// Gets horizontal coordinates of a graphics point
        /// </summary>
        /// <param name="point">Point to get horizontal coordinates</param>
        /// <returns>Horizontal coordinates of a graphics point</returns>
        CrdsHorizontal CoordinatesByPoint(PointF point);
        
        /// <summary>
        /// Renders the celestial map on provided Graphics object
        /// </summary>
        /// <param name="g">Graphics to render the map.</param>
        void Render(Graphics g);

        bool Antialias { get; set; }
    }
}
