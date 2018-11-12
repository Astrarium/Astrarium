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

        ICollection<BaseSkyRenderer> Renderers { get; }

        /// <summary>
        /// Gets or sets projection which is used for converting celestial coordinates to the sky map plane.
        /// </summary>
        IProjection Projection { get; set; }
        
        /// <summary>
        /// Renders the celestial map on provided Graphics object
        /// </summary>
        /// <param name="g">Graphics to render the map.</param>
        void Render(Graphics g);

        void Initialize();

        bool Antialias { get; set; }
    }
}
