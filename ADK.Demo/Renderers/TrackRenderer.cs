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
            var allPts = allPoints.Select(tp => Map.Projection.Project(tp.Horizontal)).ToList();

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

            DrawGroupOfPoints(g, Pens.Gray, pts.ToArray());
        }

        private void DrawGroupOfPoints(Graphics g, Pen penGrid, PointF[] points)
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
                float maxX = points.Select(p => Math.Abs(p.X)).Max();
                float maxY = points.Select(p => Math.Abs(p.Y)).Max();

                float f = 2 * (float)r / Math.Max(maxX, maxY);

                var scaledPoints = points.Select(p => new PointF(p.X * f, p.Y * f)).ToArray();

                using (GraphicsPath gp = new GraphicsPath())
                {
                    gp.AddCurve(scaledPoints);
                    gp.Flatten();
                    scaledPoints = gp.PathPoints.Select(p => new PointF(p.X / f, p.Y / f)).ToArray();

                    var segments = scaledPoints.Select(p => Geometry.DistanceBetweenPoints(p, origin) < r * 3 ? p : PointF.Empty)
                        .Split(p => p == PointF.Empty, true);

                    foreach (var segment in segments)
                    {
                        var p1 = scaledPoints.Prev(segment.First());
                        var p2 = scaledPoints.Next(segment.Last());

                        List<PointF> newPoints = new List<PointF>(segment);

                        if (p1 != PointF.Empty)
                        {
                            newPoints.Insert(0, p1);
                        }

                        if (p2 != PointF.Empty)
                        {
                            newPoints.Add(p2);
                        }

                        DrawGroupOfPoints(g, Pens.Red, newPoints.ToArray());
                    }

                    if (!segments.Any())
                    {
                        var p0 = scaledPoints.OrderBy(p => Geometry.DistanceBetweenPoints(p, origin)).First();

                        var p1 = scaledPoints.Prev(p0);
                        var p2 = scaledPoints.Next(p0);

                        List<PointF> newPoints = new List<PointF>() { p1, p0, p2 };

                        DrawGroupOfPoints(g, Pens.Red, newPoints.ToArray());



                    }

                    return;
                }

               
            }

            if (points.All(p => Geometry.DistanceBetweenPoints(p, origin) < r * 60))
            {
                // Draw the curve in regular way
                g.DrawCurve(penGrid, points);
            }
        }
    }
}
