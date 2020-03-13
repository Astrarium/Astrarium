using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Astrarium.Algorithms;
using Astrarium.Objects;
using Astrarium.Types;

namespace Astrarium.Types
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

        float MagLimit { get; }

        /// <summary>
        /// Gets or sets current field of view, in degrees
        /// </summary>
        double ViewAngle { get; set; }

        /// <summary>
        /// Occurs when map's View Angle is changed.
        /// </summary>
        event Action<double> ViewAngleChanged;

        /// <summary>
        /// Gets or sets horizontal coordinates of the central point of the canvas.
        /// </summary>
        CrdsHorizontal Center { get; }

        /// <summary>
        /// Selected celestial object
        /// </summary>
        CelestialObject SelectedObject { get; set; }

        /// <summary>
        /// Locked Object. If it set, map moving is denied and it always centered on this body. 
        /// </summary>
        CelestialObject LockedObject { get; set; }

        /// <summary>
        /// Occurs when selected celestial object is changed
        /// </summary>
        event Action<CelestialObject> SelectedObjectChanged;

        CrdsHorizontal MousePosition { get; }

        /// <summary>
        /// Gets or sets projection which is used for converting celestial coordinates to the sky map plane.
        /// </summary>
        IProjection Projection { get; }
        
        /// <summary>
        /// Renders the celestial map on provided Graphics object
        /// </summary>
        /// <param name="g">Graphics to render the map.</param>
        void Render(Graphics g);

        void Invalidate();

        CelestialObject FindObject(PointF point);

        void GoToObject(CelestialObject body, TimeSpan animationDuration);

        void AddDrawnObject(CelestialObject obj);

        event Action OnInvalidate;
    }
}
