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
        protected ISettings Settings { get; private set; }

        public BaseSkyRenderer(Sky sky, ISkyMap skyMap, ISettings settings)
        {
            Sky = sky;
            Map = skyMap;
            Settings = settings;
        }

        public abstract void Render(Graphics g);

        public virtual void Initialize() { }

        protected void DrawObjectCaption(Graphics g, Font font, string caption, PointF p, float size)
        {
            SizeF b = g.MeasureString(caption, font);

            float s = size > 5 ? (size / 2.8284f + 2) : 1;
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    float dx = x == 0 ? s : -s - b.Width;
                    float dy = y == 0 ? s : -s - b.Height;
                    RectangleF r = new RectangleF(p.X + dx, p.Y + dy, b.Width, b.Height);
                    if (!Map.Labels.Any(l => l.IntersectsWith(r)) && !Map.DrawnPoints.Any(v => r.Contains(v)))
                    {
                        g.DrawString(caption, font, Brushes.DimGray, r.Location);
                        Map.Labels.Add(r);
                        return;
                    }
                }
            }
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

        /// <summary>
        /// Gets drawing rotation of image, measured clockwise from 
        /// a point oriented to top of the screen towards North celestial pole point 
        /// </summary>
        /// <param name="eq">Equatorial coordinates of a central point of a body.</param>
        /// <returns></returns>
        protected float GetRotationTowardsNorth(CrdsEquatorial eq)
        {
            // Coordinates of center of a body (image) to be rotated
            PointF p = Map.Projection.Project(eq.ToHorizontal(Sky.Context.GeoLocation, Sky.Context.SiderealTime));

            // Point directed to North celestial pole
            PointF pNorth = Map.Projection.Project((eq + new CrdsEquatorial(0, 1)).ToHorizontal(Sky.Context.GeoLocation, Sky.Context.SiderealTime));

            // Clockwise rotation
            return LineInclinationY(p, pNorth);
        }

        /// <summary>
        /// Gets drawing rotation of image, measured clockwise from 
        /// a point oriented to top of the screen towards North ecliptic pole point 
        /// </summary>
        /// <param name="ecl">Ecliptical coordinates of a central point of a body.</param>
        /// <returns></returns>
        protected float GetRotationTowardsEclipticPole(CrdsEcliptical ecl)
        {
            // Coordinates of center of a body (image) to be rotated
            PointF p = Map.Projection.Project(ecl.ToEquatorial(Sky.Context.Epsilon).ToHorizontal(Sky.Context.GeoLocation, Sky.Context.SiderealTime));

            // Point directed to North ecliptic pole
            PointF pNorth = Map.Projection.Project((ecl + new CrdsEcliptical(0, 1)).ToEquatorial(Sky.Context.Epsilon).ToHorizontal(Sky.Context.GeoLocation, Sky.Context.SiderealTime));

            // Clockwise rotation
            return LineInclinationY(p, pNorth);
        }

        /// <summary>
        /// Checks if the point is out of screen bounds
        /// </summary>
        /// <param name="p">Point to check</param>
        /// <returns>True if out from screen, false otherwise</returns>
        protected bool IsOutOfScreen(PointF p)
        {
            return p.Y < 0 || p.Y > Map.Height || p.X < 0 || p.X > Map.Width;
        }

        /// <summary>
        /// Gets points of intersection of a line segment with screen bounds.
        /// </summary>
        /// <param name="p1">First point of the segment</param>
        /// <param name="p2">Second point of the segment</param>
        /// <returns>Points of intersection, if any (0, 1 or 2 points)</returns>
        protected PointF[] SegmentScreenIntersection(PointF p1, PointF p2)
        {
            List<PointF> crosses = new List<PointF>();

            if (!IsOutOfScreen(p1))
            {
                crosses.Add(p1);
            }

            if (!IsOutOfScreen(p2))
            {
                crosses.Add(p2);
            }

            if (crosses.Count != 2)
            {
                crosses.AddRange(EdgeCrosspoints(p1, p2));
            }

            return crosses.ToArray();
        }

        /// <summary>
        /// Gets points of intersection of a line with screen bounds.
        /// </summary>
        /// <param name="p1">First point on the line</param>
        /// <param name="p2">Second point on the line</param>
        protected PointF[] LineScreenIntersection(PointF p1, PointF p2)
        {
            int width = Map.Width;
            int height = Map.Height;

            PointF p00 = new PointF(0, 0);
            PointF pW0 = new PointF(width, 0);
            PointF pWH = new PointF(width, height);
            PointF p0H = new PointF(0, height);

            List<PointF> crosses = new List<PointF>();

            PointF c1 = LinesIntersection(p1, p2, p00, pW0);
            if (c1.Y == 0 && c1.X >= 0 && c1.X <= width)
            {
                crosses.Add(c1);
            }

            PointF c2 = LinesIntersection(p1, p2, pW0, pWH);
            if (c2.X == width && c2.Y >= 0 && c2.Y <= height)
            {
                crosses.Add(c2);
            }

            PointF c3 = LinesIntersection(p1, p2, p0H, pWH);
            if (c3.Y == height && c3.X >= 0 && c3.X <= width)
            {
                crosses.Add(c3);
            }

            PointF c4 = LinesIntersection(p1, p2, p00, p0H);
            if (c4.X == 0 && c4.Y >= 0 && c4.Y <= height)
            {
                crosses.Add(c4);
            }

            return crosses.ToArray();
        }

        private PointF[] EdgeCrosspoints(PointF p1, PointF p2)
        {
            int width = Map.Width;
            int height = Map.Height;

            PointF p00 = new PointF(0, 0);
            PointF pW0 = new PointF(width, 0);
            PointF pWH = new PointF(width, height);
            PointF p0H = new PointF(0, height);

            List<PointF?> crossPoints = new List<PointF?>();

            // top edge
            crossPoints.Add(SegmentsIntersection(p1, p2, p00, pW0));

            // right edge
            crossPoints.Add(SegmentsIntersection(p1, p2, pW0, pWH));

            // bottom edge
            crossPoints.Add(SegmentsIntersection(p1, p2, pWH, p0H));

            // left edge
            crossPoints.Add(SegmentsIntersection(p1, p2, p0H, p00));

            return crossPoints.Where(p => p != null).Cast<PointF>().ToArray();
        }

        private static PointF LinesIntersection(PointF p1, PointF p2, PointF p3, PointF p4)
        {
            float x1 = p1.X;
            float x2 = p2.X;
            float x3 = p3.X;
            float x4 = p4.X;

            float y1 = p1.Y;
            float y2 = p2.Y;
            float y3 = p3.Y;
            float y4 = p4.Y;

            float x = ((x1 * y2 - y1 * x2) * (x3 - x4) - (x1 - x2) * (x3 * y4 - y3 * x4)) / ((x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4));
            float y = ((x1 * y2 - y1 * x2) * (y3 - y4) - (y1 - y2) * (x3 * y4 - y3 * x4)) / ((x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4));

            return new PointF() { X = x, Y = y };
        }

        private static void LineEquation(PointF p1, PointF p2, ref float A, ref float B, ref float C)
        {
            A = p2.Y - p1.Y;
            B = p1.X - p2.X;
            C = -p1.X * (p2.Y - p1.Y) + p1.Y * (p2.X - p1.X);
        }

        private static PointF? SegmentsIntersection(PointF p1, PointF p2, PointF p3, PointF p4)
        {
            float v1 = VectorMult(p4.X - p3.X, p4.Y - p3.Y, p1.X - p3.X, p1.Y - p3.Y);
            float v2 = VectorMult(p4.X - p3.X, p4.Y - p3.Y, p2.X - p3.X, p2.Y - p3.Y);
            float v3 = VectorMult(p2.X - p1.X, p2.Y - p1.Y, p3.X - p1.X, p3.Y - p1.Y);
            float v4 = VectorMult(p2.X - p1.X, p2.Y - p1.Y, p4.X - p1.X, p4.Y - p1.Y);

            if ((v1 * v2) < 0 && (v3 * v4) < 0)
            {
                float a1 = 0, b1 = 0, c1 = 0;
                LineEquation(p1, p2, ref a1, ref b1, ref c1);

                float a2 = 0, b2 = 0, c2 = 0;
                LineEquation(p3, p4, ref a2, ref b2, ref c2);

                double d = a1 * b2 - b1 * a2;

                double dx = -c1 * b2 + b1 * c2;
                double dy = -a1 * c2 + c1 * a2;

                return new PointF((float)(dx / d), (float)(dy / d));
            }

            return null;
        }

        private static float VectorMult(float ax, float ay, float bx, float by)
        {
            return ax * by - bx * ay;
        }

        /// <summary>
        /// Gets inclination angle of a line or segment from top-oriented Y axis.
        /// Measured from 0 (line or segment pointed top) to 360 clockwise.
        /// </summary>
        /// <param name="p1">First point of a line/segment</param>
        /// <param name="p2">Second point of a line/segment</param>
        /// <returns></returns>
        private static float LineInclinationY(PointF p1, PointF p2)
        {
            return (float)Angle.To360(90 - Angle.ToDegrees(Math.Atan2(p1.Y - p2.Y, p2.X - p1.X)));
        }
    }
}
