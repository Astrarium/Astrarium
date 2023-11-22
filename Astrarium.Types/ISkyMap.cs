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
        event Action<double> FovChanged;

        float DaylightFactor { get; set; }

        /// <summary>
        /// Selected celestial object
        /// </summary>
        CelestialObject SelectedObject { get; set; }

        /// <summary>
        /// Locked Object. If it set, map moving is denied and it always centered on this body. 
        /// </summary>
        CelestialObject LockedObject { get; set; }

        void Move(Vec2 screenPosOld, Vec2 screenPosNew);

        double LockedObjectDeltaLongitude { get; set; }
        double LockedObjectDeltaLatitude { get; set; }

        CrdsEquatorial MousePosition { get; }

        PointF MouseCoordinates { get; }

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
    }
}
