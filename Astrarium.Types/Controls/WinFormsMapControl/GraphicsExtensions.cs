using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace System.Windows.Forms
{
    /// <summary>
    /// Set of useful graphics extensions
    /// </summary>
    internal static class GraphicsExtensions
    {
        /// <summary>
        /// Draws polyline. This method is more optimized than default <see cref="Graphics.DrawLines(Pen, PointF[])"/> 
        /// because it draws only visible segments of the line.
        /// </summary>
        /// <param name="gr">Graphics object</param>
        /// <param name="pen">Pen object</param>
        /// <param name="points">Array of points defining the polyline</param>
        /// <param name="isClosedLine">Flag indicating the line is closed one (last point should be connected with first point)</param>
        public static void DrawPolyline(this Graphics gr, Pen pen, PointF[] points, bool isClosedLine = false)
        {
            if (pen != null)
            {
                for (int i = 0; i < (isClosedLine ? points.Length : points.Length - 1); i++)
                {
                    PointF p0 = points[i];
                    PointF p1 = i < points.Length - 1 ? points[i + 1] : points[0];

                    var pts = gr.FindVisiblePartOfSegment(p0, p1);

                    if (pts.Length == 2)
                    {
                        gr.DrawLine(pen, pts[0], pts[1]);
                    }
                }
            }
        }

        /// <summary>
        /// Draws graphics path, optionally with fill and/or outline
        /// </summary>
        /// <param name="gr">Graphics object</param>
        /// <param name="path">Path to be drawn</param>
        /// <param name="brush">Fill brush</param>
        /// <param name="pen">Outline pen</param>
        public static void DrawGraphicsPath(this Graphics gr, GraphicsPath path, Brush brush, Pen pen)
        {
            if (brush != null)
            {
                gr.FillPath(brush, path);
            }
            if (pen != null)
            {
                gr.DrawPolyline(pen, path.PathPoints, isClosedLine: true);
            }
        }

        /// <summary>
        /// Finds nearest points for a point with specified coordinates
        /// </summary>
        /// <param name="point">Point to find nearest point for.</param>
        /// <param name="points">Array of points to select nearest one.</param>
        /// <returns>Nearest point from the array closest to the specified one.</returns>
        public static PointF Nearest(this PointF point, params PointF[] points)
        {
            return points.OrderBy(p => ((p.X - point.X) * (p.X - point.X) + (p.Y - point.Y) * (p.Y - point.Y))).First();
        }

        /// <summary>
        /// Finds visible part of segment
        /// </summary>
        /// <param name="g">Graphics object</param>
        /// <param name="p1">First point of a segment</param>
        /// <param name="p2">Last point of a segment</param>
        /// <returns>Visible part of segment, i.e. first and last points</returns>
        private static PointF[] FindVisiblePartOfSegment(this Graphics g, PointF p1, PointF p2)
        {
            List<PointF> crosses = new List<PointF>();

            if (g.IsVisible(p1))
            {
                crosses.Add(p1);
            }

            if (g.IsVisible(p2))
            {
                crosses.Add(p2);
            }

            if (crosses.Count != 2)
            {
                crosses.AddRange(g.FindClipBoundsIntersections(p1, p2));
            }

            return crosses.ToArray();
        }

        /// <summary>
        /// Finds intersections of a line segment with clip bounds
        /// </summary>
        /// <param name="g">Graphics object</param>
        /// <param name="p1">First point of a segment</param>
        /// <param name="p2">Last point of a segment</param>
        /// <returns>Intersection points. Count can be from 0 to 2.</returns>
        private static PointF[] FindClipBoundsIntersections(this Graphics g, PointF p1, PointF p2)
        {            
            int width = (int)g.ClipBounds.Width;
            int height = (int)g.ClipBounds.Height;

            PointF p00 = new PointF(g.ClipBounds.X, g.ClipBounds.Y);
            PointF pW0 = new PointF(g.ClipBounds.X + width, g.ClipBounds.Y);
            PointF pWH = new PointF(g.ClipBounds.X + width, g.ClipBounds.Y + height);
            PointF p0H = new PointF(g.ClipBounds.X, g.ClipBounds.Y + height);

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

        /// <summary>
        /// Finds point of intersection of two segments
        /// </summary>
        /// <param name="p1">First point of first segment</param>
        /// <param name="p2">Last point of first segment</param>
        /// <param name="p3">First point of second segment</param>
        /// <param name="p4">Last point of second segment</param>
        /// <returns>Point of intersection of two segments, or null if no intersection</returns>
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

        /// <summary>
        /// Multiplies two vectors defined by coordinates
        /// </summary>
        /// <param name="ax"></param>
        /// <param name="ay"></param>
        /// <param name="bx"></param>
        /// <param name="by"></param>
        /// <returns></returns>
        private static float VectorMult(float ax, float ay, float bx, float by)
        {
            return ax * by - bx * ay;
        }

        /// <summary>
        /// Calculates constants of a line equation by two points lie on that line.
        /// </summary>
        /// <param name="p1">First point lies on the line</param>
        /// <param name="p2">Second point lies on the line</param>
        /// <param name="A">A-constant</param>
        /// <param name="B">B-constant</param>
        /// <param name="C">C-constant</param>
        private static void LineEquation(PointF p1, PointF p2, ref float A, ref float B, ref float C)
        {
            A = p2.Y - p1.Y;
            B = p1.X - p2.X;
            C = -p1.X * (p2.Y - p1.Y) + p1.Y * (p2.X - p1.X);
        }
    }
}
