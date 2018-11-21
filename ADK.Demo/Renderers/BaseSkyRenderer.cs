using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo.Renderers
{
    /// <summary>
    /// Base class for all renderer classes which implement drawing logic of sky map.
    /// </summary>
    public abstract class BaseSkyRenderer
    {
        protected Sky Sky { get; private set; }
        protected ISkyMap Map { get; private set; }

        public BaseSkyRenderer(Sky sky, ISkyMap skyMap)
        {
            Sky = sky;
            Map = skyMap;
        }

        public abstract void Render(Graphics g);

        public virtual void Initialize() { }

        protected void DrawObjectCaption(Graphics g, string caption, PointF p, float size)
        {
            g.DrawString(caption, SystemFonts.DefaultFont, Brushes.DimGray, p.X + size / 2.8284f + 2, p.Y + size / 2.8284f + 2);
        }

        /// <summary>
        /// Gets size of a disk (circle) representing a solar system object on sky map.
        /// </summary>
        /// <param name="semidiameter">Semidiameter of a body, in seconds of arc.</param>
        /// <returns>Size (diameter) of a disk in screen pixels</returns>
        protected float GetDiskSize(double semidiameter, double minSize = 0)
        {
            return (float)Math.Max(minSize, semidiameter / 3600.0 / Map.ViewAngle * Map.Width);
        }

        /// <summary>
        /// Gets size of a point (small filled circle) representing a star or a planet
        /// or any other celestial object on sky map, depending of its magnitude.
        /// </summary>
        /// <param name="mag">Magnitude of a celestial body</param>
        /// <returns>Size (diameter) of a point in screen pixels</returns>
        protected float GetPointSize(float mag)
        {
            float maxMag = 0;
            float MAG_LIMIT_NARROW_ANGLE = 7f;
            const float MAG_LIMIT_WIDE_ANGLE = 5.5f;

            const float NARROW_ANGLE = 2;
            const float WIDE_ANGLE = 90;

            float K = (MAG_LIMIT_NARROW_ANGLE - MAG_LIMIT_WIDE_ANGLE) / (NARROW_ANGLE - WIDE_ANGLE);
            float B = MAG_LIMIT_WIDE_ANGLE - K * WIDE_ANGLE;

            float minMag = K * (float)Map.ViewAngle + B;

            if (Map.ViewAngle < 2 && mag > minMag)
                return 1;

            if (mag > minMag)
                return 0;

            if (mag <= maxMag)
                mag = maxMag;

            float range = minMag - maxMag;

            return (range - mag + 1);
        }
    }
}
