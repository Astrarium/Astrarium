using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo
{
    public static class Geometry
    {
        /// <summary>
        /// Gets angle between two vectors starting with same point.
        /// </summary>
        /// <param name="p0">Common point of two vectors (starting point for both vectors).</param>
        /// <param name="p1">End point of first vector</param>
        /// <param name="p2">End point of first vector</param>
        /// <returns>Angle between two vectors, in degrees, in range [0...180]</returns>
        public static double AngleBetweenVectors(PointF p0, PointF p1, PointF p2)
        {
            float[] a = new float[] { p1.X - p0.X, p1.Y - p0.Y };
            float[] b = new float[] { p2.X - p0.X, p2.Y - p0.Y };

            float ab = a[0] * b[0] + a[1] * b[1];
            double moda = Math.Sqrt(a[0] * a[0] + a[1] * a[1]);
            double modb = Math.Sqrt(b[0] * b[0] + b[1] * b[1]);

            double cos = ab / (moda * modb);

            if (cos < -1)
                cos = -1;

            if (cos > 1)
                cos = 1;

            return Angle.ToDegrees(Math.Acos(cos));
        }

        /// <summary>
        /// Gets distance between two points in pixels
        /// </summary>
        /// <param name="p1">First point</param>
        /// <param name="p2">Second point</param>
        /// <returns>Distance between two points, in pixels</returns>
        public static double DistanceBetweenPoints(PointF p1, PointF p2)
        {
            double deltaX = p1.X - p2.X;
            double deltaY = p1.Y - p2.Y;
            return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }

        public static Circle FindCircle(PointF[] l)
        {
            // https://www.scribd.com/document/14819165/Regressions-coniques-quadriques-circulaire-spherique
            // via http://math.stackexchange.com/questions/662634/find-the-approximate-center-of-a-circle-passing-through-more-than-three-points

            var n = l.Count();
            var sumx = l.Sum(p => p.X);
            var sumxx = l.Sum(p => p.X * p.X);
            var sumy = l.Sum(p => p.Y);
            var sumyy = l.Sum(p => p.Y * p.Y);

            var d11 = n * l.Sum(p => p.X * p.Y) - sumx * sumy;

            var d20 = n * sumxx - sumx * sumx;
            var d02 = n * sumyy - sumy * sumy;

            var d30 = n * l.Sum(p => p.X * p.X * p.X) - sumxx * sumx;
            var d03 = n * l.Sum(p => p.Y * p.Y * p.Y) - sumyy * sumy;

            var d21 = n * l.Sum(p => p.X * p.X * p.Y) - sumxx * sumy;
            var d12 = n * l.Sum(p => p.Y * p.Y * p.X) - sumyy * sumx;

            var x = ((d30 + d12) * d02 - (d03 + d21) * d11) / (2 * (d20 * d02 - d11 * d11));
            var y = ((d03 + d21) * d20 - (d30 + d12) * d11) / (2 * (d20 * d02 - d11 * d11));

            var c = (sumxx + sumyy - 2 * x * sumx - 2 * y * sumy) / n;
            var r = Math.Sqrt(c + x * x + y * y);

            return new Circle() { X = x, Y = y, R = r };
        }


        /// <summary>
        /// Checks if the point is out of screen bounds
        /// </summary>
        /// <param name="p">Point to check</param>
        /// <returns>True if out from screen, false otherwise</returns>
        public static bool IsOutOfScreen(PointF p, int width, int height)
        {
            return p.Y < 0 || p.Y > height || p.X < 0 || p.X > width;
        }

        public static PointF LinesIntersection(PointF p1, PointF p2, PointF p3, PointF p4)
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

        public static PointF[] SegmentRectangleIntersection(PointF p1, PointF p2, int width, int height)
        {
            List<PointF> crosses = new List<PointF>();

            if (!IsOutOfScreen(p1, width, height))
            {
                crosses.Add(p1);
            }

            if (!IsOutOfScreen(p2, width, height))
            {
                crosses.Add(p2);
            }

            if (crosses.Count != 2)
            {
                crosses.AddRange(EdgeCrosspoints(p1, p2, width, height));
            }

            return crosses.ToArray();
        }

        private static PointF[] EdgeCrosspoints(PointF p1, PointF p2, int width, int height)
        {
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

        private static float VectorMult(float ax, float ay, float bx, float by)
        {
            return ax * by - bx * ay;
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

                if (d < 1e-10)
                {
                    return null;
                }
                else
                {
                    double dx = -c1 * b2 + b1 * c2;
                    double dy = -a1 * c2 + c1 * a2;

                    return new PointF((float)(dx / d), (float)(dy / d));
                }
            }

            return null;
        }

        public static PointF[] LineRectangleIntersection(PointF p1, PointF p2, int width, int height)
        {
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
    }

    public class Circle
    {
        public double X;
        public double Y;
        public double R;
    }
}
