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
        /// Width of the canvas, in pixels
        /// </summary>
        int Width { get; set; }

        /// <summary>
        /// Height of the canvas, in pixels
        /// </summary>
        int Height { get; set; }

        /// <summary>
        /// Horizontal coordinates of the central point of the canvas.
        /// </summary>
        CrdsHorizontal Center { get; set; }

        /// <summary>
        /// Current field of view angle
        /// </summary>
        double ViewAngle { get; set; }

        /// <summary>
        /// Gets horizontal coordinates of a graphics point
        /// </summary>
        /// <param name="point">Point to get horizontal coordinates</param>
        /// <returns>Horizontal coordinates of a graphics point</returns>
        CrdsHorizontal CoordinatesByPoint(PointF point);

        

        void Render(Graphics g);
    }
}
