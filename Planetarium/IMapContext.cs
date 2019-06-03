using ADK;
using Planetarium.Objects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium
{
    public interface IMapContext
    {
        /// <summary>
        /// Gets drawing surface
        /// </summary>
        Graphics Graphics { get; }

        /// <summary>
        /// Gets width of the canvas, in pixels
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Gets height of the canvas, in pixels
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Gets current field of view, in degrees
        /// </summary>
        double ViewAngle { get; }

        /// <summary>
        /// Gets horizontal coordinates of the central point of the canvas.
        /// </summary>
        CrdsHorizontal Center { get; }

        /// <summary>
        /// Gets celestial object the map is locked on
        /// </summary>
        CelestialObject LockedObject { get; }

        bool IsDragging { get; }

        /// <summary>
        /// Origin of measure tool. Not null if measure tool is on.
        /// </summary>
        CrdsHorizontal MeasureOrigin { get; }

        CrdsHorizontal MousePosition { get; }

        void AddDrawnObject(CelestialObject obj, PointF p);

        /// <summary>
        /// Projects horizontal coordinates to the point on the map
        /// </summary>
        /// <param name="hor">Horizontal coordinates to be projected to the map.</param>
        /// <returns><see cref="PointF"/> instance - is a projection of horizontal corrdinates on the map.</returns>
        PointF Project(CrdsHorizontal hor);

        double JulianDay { get; }

        double Epsilon { get; }

        CrdsGeographical GeoLocation { get; }

        double SiderealTime { get; }

        void DrawObjectCaption(Font font, Brush brush, string caption, PointF p, float size);

        void Redraw();
    }

    public static class MapContextExtensions
    {
        public static double DiagonalCoefficient(this IMapContext map)
        {
            return Math.Sqrt(map.Width * map.Width + map.Height * map.Height) / Math.Max(map.Width, map.Height);
        }

        /// <summary>
        /// Gets size of a disk (circle) representing a solar system object on sky map.
        /// </summary>
        /// <param name="semidiameter">Semidiameter of a body, in seconds of arc.</param>
        /// <returns>Size (diameter) of a disk in screen pixels</returns>
        public static float GetDiskSize(this IMapContext map, double semidiameter, double minSize = 0)
        {
            double maxSize = Math.Max(map.Width, map.Height);
            return (float)Math.Max(minSize, semidiameter / 3600.0 / map.ViewAngle * maxSize);
        }

        /// <summary>
        /// Gets size of a point (small filled circle) representing a star or a planet
        /// or any other celestial object on sky map, depending of its magnitude.
        /// </summary>
        /// <param name="mag">Magnitude of a celestial body</param>
        /// <returns>Size (diameter) of a point in screen pixels</returns>
        public static float GetPointSize(this IMapContext map, float mag)
        {
            float mag0 = 8;
            float maxSize = 5;

            if (map.ViewAngle <= 90) { mag0 = 8.0f; }
            if (map.ViewAngle <= 70) {mag0 = 8.5f; }
            if (map.ViewAngle < 50) {mag0 = 9.0f; }
            if (map.ViewAngle < 30) {mag0 = 9.5f; }
            if (map.ViewAngle < 20) {mag0 = 10.0f; }
            if (map.ViewAngle < 15) {mag0 = 10.5f; }
            if (map.ViewAngle < 10) {mag0 = 11.0f; }
            if (map.ViewAngle < 5) {mag0 = 11.5f; }
            if (map.ViewAngle < 2) {mag0 = 12.0f; }

            if (map.ViewAngle < 2 && mag > mag0)
            {
                // if star is faint than drawing limit and FOV is small enough, 
                // draw it as single point
                return 1;
            }
            else
            {
                // drawing diameter if star with 'mag0' magnitude, in pixels
                float size0 = 1;

                // drawing area of star with 'mag0' magnitude, in square pixels
                float area0 = 3.1415f * (size0 / 2) * (size0 / 2);

                // drawing area of star with 'mag' magnitude, according to Pogson formula
                double area = Math.Pow(10, (mag - mag0) / -2.5) * area0;

                // drawing size of star with 'mag' magnitude
                float size = (float)(Math.Sqrt(area) / 3.1415);

                // do not exceed drawing size
                size = Math.Min(maxSize, size);

                // compensate faint stars on the drawing limit edge
                if (size > 0.9 && size < 1) size = 1;

                return size;
            }
        }

        /// <summary>
        /// Gets drawing rotation of image, measured clockwise from 
        /// a point oriented to top of the screen towards North celestial pole point 
        /// </summary>
        /// <param name="eq">Equatorial coordinates of a central point of a body.</param>
        /// <returns></returns>
        public static float GetRotationTowardsNorth(this IMapContext map, CrdsEquatorial eq)
        {
            // Coordinates of center of a body (image) to be rotated
            PointF p = map.Project(eq.ToHorizontal(map.GeoLocation, map.SiderealTime));

            // Point directed to North celestial pole
            PointF pNorth = map.Project((eq + new CrdsEquatorial(0, 1)).ToHorizontal(map.GeoLocation, map.SiderealTime));

            // Clockwise rotation
            return LineInclinationY(p, pNorth);
        }

        /// <summary>
        /// Gets drawing rotation of image, measured clockwise from 
        /// a point oriented to top of the screen towards North ecliptic pole point 
        /// </summary>
        /// <param name="ecl">Ecliptical coordinates of a central point of a body.</param>
        /// <returns></returns>
        public static float GetRotationTowardsEclipticPole(this IMapContext map, CrdsEcliptical ecl)
        {
            // Coordinates of center of a body (image) to be rotated
            PointF p = map.Project(ecl.ToEquatorial(map.Epsilon).ToHorizontal(map.GeoLocation, map.SiderealTime));

            // Point directed to North ecliptic pole
            PointF pNorth = map.Project((ecl + new CrdsEcliptical(0, 1)).ToEquatorial(map.Epsilon).ToHorizontal(map.GeoLocation, map.SiderealTime));

            // Clockwise rotation
            return LineInclinationY(p, pNorth);
        }

        /// <summary>
        /// Checks if the point is out of screen bounds
        /// </summary>
        /// <param name="p">Point to check</param>
        /// <returns>True if out from screen, false otherwise</returns>
        public static bool IsOutOfScreen(this IMapContext map, PointF p)
        {
            return p.Y < 0 || p.Y > map.Height || p.X < 0 || p.X > map.Width;
        }

        /// <summary>
        /// Gets points of intersection of a line segment with screen bounds.
        /// </summary>
        /// <param name="p1">First point of the segment</param>
        /// <param name="p2">Second point of the segment</param>
        /// <returns>Points of intersection, if any (0, 1 or 2 points)</returns>
        public static PointF[] SegmentScreenIntersection(this IMapContext map, PointF p1, PointF p2)
        {
            List<PointF> crosses = new List<PointF>();

            if (!IsOutOfScreen(map, p1))
            {
                crosses.Add(p1);
            }

            if (!IsOutOfScreen(map, p2))
            {
                crosses.Add(p2);
            }

            if (crosses.Count != 2)
            {
                crosses.AddRange(EdgeCrosspoints(map, p1, p2));
            }

            return crosses.ToArray();
        }

        /// <summary>
        /// Gets points of intersection of a line with screen bounds.
        /// </summary>
        /// <param name="p1">First point on the line</param>
        /// <param name="p2">Second point on the line</param>
        public static PointF[] LineScreenIntersection(this IMapContext map, PointF p1, PointF p2)
        {
            int width = map.Width;
            int height = map.Height;

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

        private static PointF[] EdgeCrosspoints(IMapContext map, PointF p1, PointF p2)
        {
            int width = map.Width;
            int height = map.Height;

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
