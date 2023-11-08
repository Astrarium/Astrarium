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
        /// Gets or sets horizontal coordinates of the central point of the canvas.
        /// </summary>
        CrdsHorizontal Center { get; }

        float DaylightFactor { get; set; }

        /// <summary>
        /// Selected celestial object
        /// </summary>
        CelestialObject SelectedObject { get; set; }

        /// <summary>
        /// Locked Object. If it set, map moving is denied and it always centered on this body. 
        /// </summary>
        CelestialObject LockedObject { get; set; }

        CrdsHorizontal MousePosition { get; }

        PointF MouseCoordinates { get; }

        Projection Projection { get; }

        bool TimeSync { get; set; }

        void Invalidate();

        /// <summary>
        /// Renders the celestial map on provided Graphics object
        /// </summary>
        /// <param name="g">Graphics to render the map.</param>
        [Obsolete]
        void Render(Graphics g);

        void Render();

        CelestialObject FindObject(PointF point);

        void GoToObject(CelestialObject body, double viewAngleTarget);
        void GoToObject(CelestialObject body, TimeSpan animationDuration);
        void GoToObject(CelestialObject body, TimeSpan animationDuration, double viewAngleTarget);
        void GoToPoint(CrdsHorizontal hor, double viewAngleTarget);
        void GoToPoint(CrdsHorizontal hor, TimeSpan animationDuration);
        void GoToPoint(CrdsHorizontal hor, TimeSpan animationDuration, double viewAngleTarget);

        [Obsolete]
        void AddDrawnObject(CelestialObject obj);

        void AddDrawnObject(PointF p, CelestialObject obj, float size);

        void DrawObjectLabel(TextRenderer textRenderer, string label, Font font, Brush brush, PointF point, float size);

        /// <summary>
        /// Occurs when selected celestial object is changed
        /// </summary>
        event Action<CelestialObject> SelectedObjectChanged;

        event Action OnInvalidate;
    }
}
