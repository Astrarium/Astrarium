using ADK.Demo.Objects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace ADK.Demo.Renderers
{
    /// <summary>
    /// Renders celestial bodies motion tracks on the map
    /// </summary>
    public class TrackRenderer : BaseSkyRenderer
    {
        private Font fontLabel = new Font("Arial", 8);

        public TrackRenderer(Sky sky, ISkyMap skyMap, ISettings settings) : base(sky, skyMap, settings)
        {
            
        }



        public override void Render(Graphics g)
        {
            var tracks = Sky.Get<ICollection<Track>>("Tracks");
            foreach (var track in tracks)
            {
                int currentPointIndex = (int)((Sky.Context.JulianDay - track.FromJD) / track.Duration * track.Points.Count);
                var trackPoints = new List<CelestialPoint>(track.Points);
                if (currentPointIndex < track.Points.Count - 1)
                {
                    trackPoints[currentPointIndex + 1] = new CelestialPoint() { Horizontal = track.Body.Horizontal };
                }

                var segments = trackPoints
                    .Select(p => Angle.Separation(p.Horizontal, Map.Center) < Map.ViewAngle * 1.2 ? p : null)
                    .Split(p => p == null, true);

                bool isAnyPoint = false;

                foreach (var segment in segments)
                {
                    DrawSegment(g, segment, trackPoints);
                    isAnyPoint = true;
                }

                // Special case: there are no points visible 
                // on the screen at the current position and zoom.
                // Then we select one point that is closest to screen senter. 
                if (!isAnyPoint)
                {
                    var closestPoint = trackPoints.OrderBy(p => Angle.Separation(p.Horizontal, Map.Center)).First();

                    DrawSegment(g, new List<CelestialPoint>() { closestPoint }, trackPoints);
                }

                double step = track.LabelsStep.TotalDays;
                double jdLastLabel = 0;

                for (int i = 0; i < track.Points.Count - 1; i++)
                {
                    double jd = track.FromJD + (double)i / track.Points.Count * track.Duration;

                    if (Math.Abs(jd - jdLastLabel) >= step)
                    {
                        jdLastLabel = jd;

                        var tp = track.Points[i];
                        double ad = Angle.Separation(tp.Horizontal, Map.Center);
                        if (ad < Map.ViewAngle * 1.2)
                        {
                            PointF p = Map.Projection.Project(tp.Horizontal);
                            if (!IsOutOfScreen(p))
                            {
                                DrawObjectCaption(g, fontLabel, Brushes.Gray, Formatters.DateTime.Format(Sky.Context.ToLocalDate(jd)), p, 0);
                            }
                        }
                    }
                }
            }
        }

        private void DrawSegment(Graphics g, List<CelestialPoint> segment, IList<CelestialPoint> allPoints)
        {
            var pts = segment.Select(tp => Map.Projection.Project(tp.Horizontal)).ToList();

            var firstP = segment.First();
            var prevP = allPoints.Prev(firstP);
            if (prevP != null)
            {
                var p1 = Map.Projection.Project(prevP.Horizontal);
                pts.Insert(0, p1);
            }

            var lastP = segment.Last();
            var nextP = allPoints.Next(lastP);
            if (nextP != null)
            {
                var p1 = Map.Projection.Project(nextP.Horizontal);
                pts.Add(p1);
            }

            PointF[] refPoints = new PointF[2];


            if (segment.Count > 0)
            {
                var centerP = segment[segment.Count / 2];
                var h0 = centerP.Horizontal;

                var h1 = allPoints.Prev(centerP).Horizontal;
                var h2 = allPoints.Next(centerP).Horizontal;

                double f = Angle.Separation(h1, h2);

                //if (f / 4  < 1 && f * 3 / 4.0 < 1)
                {
                    double[] alt = new double[] { h1.Altitude, h0.Altitude, h2.Altitude };
                    double[] az = new double[] { h1.Azimuth, h0.Azimuth, h2.Azimuth };

                    double[] x = new double[] { 0, 0.5, 1 };

                    Angle.Align(alt);
                    Angle.Align(az);

                    double alt1 = Interpolation.Lagrange(x, alt, 1 / 8.0);
                    double az1 = Interpolation.Lagrange(x, az, f / 8.0);

                    double alt2 = Interpolation.Lagrange(x, alt, 0.5 + 1 / 8.0);
                    double az2 = Interpolation.Lagrange(x, az, 0.5 + 1 / 8.0);

                    refPoints[0] = Map.Projection.Project(new CrdsHorizontal(az1, alt1));
                    refPoints[1] = Map.Projection.Project(new CrdsHorizontal(az2, alt2));
                }
            }

            DrawGroupOfPoints(g, Pens.Gray, pts.ToArray(), refPoints);
        }

        private void DrawGroupOfPoints(Graphics g, Pen penGrid, PointF[] points, PointF[] refPoints)
        {
            // Do not draw figure containing less than 2 points
            if (points.Length < 2)
            {
                return;
            }

            // Two points can be simply drawn as a line
            if (points.Length == 2)
            {
                g.DrawLine(penGrid, points[0], points[1]);
                return;
            }

            // Coordinates of the screen center
            var origin = new PointF(Map.Width / 2, Map.Height / 2);

            // Small radius is a screen diagonal
            double r = Math.Sqrt(Map.Width * Map.Width + Map.Height * Map.Height) / 2;

            // From 3 to 5 points. Probably we can straighten curve to line.
            // Apply some calculations to detect conditions when it's possible.
            if (points.Length > 2 && points.Length < 6)
            {
                // Determine start, middle and end points of the curve
                PointF pStart = points[0];
                PointF pMid = points[points.Length / 2];
                PointF pEnd = points[points.Length - 1];

                // Get angle between middle and last points of the curve
                double alpha = Geometry.AngleBetweenVectors(pMid, pStart, pEnd);

                double d1 = Geometry.DistanceBetweenPoints(pStart, origin);
                double d2 = Geometry.DistanceBetweenPoints(pEnd, origin);

                // It's almost a straight line
                if (alpha > 179)
                {
                    // Check the at least one last point of the curve 
                    // is far enough from the screen center
                    if (d1 > r * 2 || d2 > r * 2)
                    {
                        g.DrawLine(penGrid, refPoints[0], refPoints[1]);
                        return;
                    }
                }

                // If both of last points of the line are far enough from the screen center 
                // then assume that the curve is an arc of a big circle.
                // Check the curvature of that circle by comparing its radius with small radius
                if (d1 > r * 2 && d2 > r * 2)
                {
                    var R = FindCircleRadius(points);
                    if (R / r > 60)
                    {
                        g.DrawLine(penGrid, refPoints[0], refPoints[1]);
                        return;
                    }
                }
            }

            if (points.All(p => Geometry.DistanceBetweenPoints(p, origin) < r * 60))
            {
                // Draw the curve in regular way
                g.DrawCurve(penGrid, points);
            }
        }

        private double FindCircleRadius(PointF[] l)
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

            return r;
        }
    }
}
