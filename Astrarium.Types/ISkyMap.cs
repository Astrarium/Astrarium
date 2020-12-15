using System;
using System.ComponentModel;
using System.Drawing;
using Astrarium.Algorithms;

namespace Astrarium.Types
{
    /// <summary>
    /// Defines an interface of Sky Map canvas to render celestial map.
    /// </summary>
    public interface ISkyMap : INotifyPropertyChanged
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

        CrdsHorizontal MousePosition { get; }

        /// <summary>
        /// Gets or sets projection which is used for converting celestial coordinates to the sky map plane.
        /// </summary>
        IProjection Projection { get; }

        void Invalidate();

        /// <summary>
        /// Renders the celestial map on provided Graphics object
        /// </summary>
        /// <param name="g">Graphics to render the map.</param>
        void Render(Graphics g);

        CelestialObject FindObject(PointF point);

        void GoToObject(CelestialObject body, double viewAngleTarget);
        void GoToObject(CelestialObject body, TimeSpan animationDuration);
        void GoToObject(CelestialObject body, TimeSpan animationDuration, double viewAngleTarget);
        void GoToPoint(CrdsHorizontal hor, double viewAngleTarget);
        void GoToPoint(CrdsHorizontal hor, TimeSpan animationDuration);
        void GoToPoint(CrdsHorizontal hor, TimeSpan animationDuration, double viewAngleTarget);

        void AddDrawnObject(CelestialObject obj);

        /// <summary>
        /// Occurs when map's View Angle is changed.
        /// </summary>
        event Action<double> ViewAngleChanged;

        /// <summary>
        /// Occurs when selected celestial object is changed
        /// </summary>
        event Action<CelestialObject> SelectedObjectChanged;

        event Action OnInvalidate;
    }
}
