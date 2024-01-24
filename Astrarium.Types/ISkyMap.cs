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
        // TODO: remove if not used
        event Action<double> FovChanged;

        event Action ContextChanged;

        /// <summary>
        /// Value indicating sky darkness (daylight presence). 1 means Sun above horizon, 0 - totally dark sky.
        /// </summary>
        float DaylightFactor { get; set; }

        /// <summary>
        /// Gets or sets selected celestial object
        /// </summary>
        CelestialObject SelectedObject { get; set; }

        /// <summary>
        /// Locked Object. If it set, map moving is denied and it always centered on this body. 
        /// </summary>
        CelestialObject LockedObject { get; set; }

        /// <summary>
        /// Moves map from one point to another (screen positions used)
        /// </summary>
        /// <param name="screenPosOld">Old screen position</param>
        /// <param name="screenPosNew">New screen position</param>
        void Move(Vec2 screenPosOld, Vec2 screenPosNew);

        /// <summary>
        /// Gets mouse position in equatorial coordinates
        /// </summary>
        CrdsEquatorial MouseEquatorialCoordinates { get; }

        /// <summary>
        /// Gets mouse position in screen coordinates (x, y)
        /// </summary>
        PointF MouseScreenCoordinates { get; }

        Projection Projection { get; }

        void SetProjection(Type type);

        bool TimeSync { get; set; }

        void Invalidate();

        void Render();

        CelestialObject FindObject(PointF point);

        void GoToObject(CelestialObject body, double viewAngleTarget);
        void GoToObject(CelestialObject body, TimeSpan animationDuration);
        void GoToObject(CelestialObject body, TimeSpan animationDuration, double viewAngleTarget);
        void GoToPoint(CrdsEquatorial eq, double viewAngleTarget);
        void GoToPoint(CrdsEquatorial eq, TimeSpan animationDuration);
        void GoToPoint(CrdsEquatorial eq, TimeSpan animationDuration, double viewAngleTarget);

        void AddDrawnObject(PointF p, CelestialObject obj, float size);

        void DrawObjectLabel(TextRenderer textRenderer, string label, Font font, Brush brush, PointF point, float size);

        /// <summary>
        /// Occurs when selected celestial object is changed
        /// </summary>
        event Action<CelestialObject> SelectedObjectChanged;

        event Action OnInvalidate;

        event Action<CelestialObject> LockedObjectChanged;
    }
}
