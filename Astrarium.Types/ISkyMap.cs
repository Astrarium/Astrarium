using System;
using System.Collections.Generic;
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
        event Action<double> FovChanged;

        event Action ContextChanged;

        /// <summary>
        /// Value indicating sky darkness (daylight presence). 1 means Sun above horizon, 0 - totally dark sky.
        /// </summary>
        float DaylightFactor { get; set; }

        /// <summary>
        /// Gets or sets selected celestial object.
        /// </summary>
        CelestialObject SelectedObject { get; set; }

        /// <summary>
        /// Gets or sets celestial object the map is locked on (synchronized with visible daily motion). 
        /// </summary>
        CelestialObject LockedObject { get; set; }

        /// <summary>
        /// Gets mouse position in equatorial coordinates
        /// </summary>
        CrdsEquatorial MouseEquatorialCoordinates { get; }

        /// <summary>
        /// Gets mouse position in screen coordinates (x, y)
        /// </summary>
        PointF MouseScreenCoordinates { get; }

        /// <summary>
        /// Gets map projection
        /// </summary>
        Projection Projection { get; }

        /// <summary>
        /// Sets map projection by its type
        /// </summary>
        /// <param name="type"></param>
        void SetProjection(Type type);

        /// <summary>
        /// Gets or sets flag indicating time synchronization
        /// </summary>
        bool TimeSync { get; set; }

        /// <summary>
        /// Raises OnInvalidate (repaint) event that causes a graphical control to be repainted
        /// </summary>
        void Invalidate();

        CelestialObject FindObject(PointF point);
        IEnumerable<CelestialObject> FindObjects(PointF point);

        void GoToObject(CelestialObject body, double viewAngleTarget);
        void GoToObject(CelestialObject body, TimeSpan animationDuration);
        void GoToObject(CelestialObject body, TimeSpan animationDuration, double viewAngleTarget);
        void GoToPoint(CrdsEquatorial eq, double viewAngleTarget);
        void GoToPoint(CrdsEquatorial eq, TimeSpan animationDuration);
        void GoToPoint(CrdsEquatorial eq, TimeSpan animationDuration, double viewAngleTarget);

        void AddDrawnObject(PointF p, CelestialObject obj);

        /// <summary>
        /// Draws celestial object label
        /// </summary>
        /// <param name="label">Object label</param>
        /// <param name="font">Font for rendering label</param>
        /// <param name="brush">Brush for rendering label</param>
        /// <param name="p">Center of the body, in screen coordinates</param>
        /// <param name="size">Object size, in pixels</param>
        void DrawObjectLabel(string label, Font font, Brush brush, PointF point, float size);

        /// <summary>
        /// Occurs when selected celestial object is changed
        /// </summary>
        event Action<CelestialObject> SelectedObjectChanged;

        /// <summary>
        /// Occurs when locked celestial object is changed
        /// </summary>
        event Action<CelestialObject> LockedObjectChanged;
    }
}
