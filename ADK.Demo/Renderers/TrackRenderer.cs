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
        private Font fontLabel;
        private Color colorLabel;
        private Color colorTrack;
        private Brush brushLabel;
        private Pen penTrack;

        public TrackRenderer(Sky sky, ISkyMap skyMap, ISettings settings) : base(sky, skyMap, settings)
        {
            fontLabel = new Font("Arial", 8);
            colorTrack = Color.FromArgb(100, 100, 100);
            colorLabel = Color.FromArgb(100, 100, 100);
            penTrack = new Pen(colorTrack);
            brushLabel = new SolidBrush(colorLabel);
        }

        public override void Render(Graphics g)
        {
            var tracks = Sky.Get<ICollection<Track>>("Tracks");
            foreach (var track in tracks)
            {
                var segments = track.Points
                    .Select(p => Angle.Separation(p.Horizontal, Map.Center) < Map.ViewAngle * 1.2 ? p : null)
                    .Split(p => p == null, true);

                foreach (var segment in segments)
                {
                    var prevP = track.Points.Prev(segment.First());
                    if (prevP != null)
                    {
                        segment.Insert(0, prevP);
                    }

                    var nextP = track.Points.Next(segment.Last());
                    if (nextP != null)
                    {
                        segment.Add(nextP);
                    }

                    DrawTrackSegment(g, penTrack, segment.Select(p => Map.Projection.Project(p.Horizontal)).ToArray());
                }

                if (!segments.Any())
                {
                    var segment = new List<CelestialPoint>();
                    var p0 = track.Points.OrderBy(p => Angle.Separation(p.Horizontal, Map.Center)).First();
                    segment.Add(p0);

                    var p1 = track.Points.Prev(p0);
                    var p2 = track.Points.Next(p0);

                    if (p1 != null)
                    {
                        segment.Insert(0, p1);
                    }

                    if (p2 != null)
                    {
                        segment.Add(p2);
                    }

                    DrawTrackSegment(g, penTrack, segment.Select(p => Map.Projection.Project(p.Horizontal)).ToArray());
                }

                DrawLabels(g, track);
            }
        }

        private void DrawLabels(Graphics g, Track track)
        {
            double stepLabels = track.LabelsStep.TotalDays;
            double stepPoints = track.Duration / (track.Points.Count - 1);

            int each = (int)(stepLabels / stepPoints);

            double jd = track.FromJD;
            for (int i = 0; i < track.Points.Count; i++)
            {
                if (i % each == 0 || i == track.Points.Count - 1)
                {
                    var tp = track.Points[i];
                    double ad = Angle.Separation(tp.Horizontal, Map.Center);
                    if (ad < Map.ViewAngle * 1.2)
                    {
                        PointF p = Map.Projection.Project(tp.Horizontal);
                        if (!IsOutOfScreen(p))
                        {
                            g.FillEllipse(brushLabel, p.X - 2, p.Y - 2, 4, 4);
                            DrawObjectCaption(g, fontLabel, brushLabel, Formatters.DateTime.Format(Sky.Context.ToLocalDate(jd)), p, 4);
                        }
                    }
                }

                jd += stepPoints;
            }
        }

        private void DrawTrackSegment(Graphics g, Pen penGrid, PointF[] points)
        {
            // Do not draw figure containing less than 2 points
            if (points.Length < 2)
            {
                return;
            }
            // Two points can be simply drawn as a line
            else if (points.Length == 2)
            {
                g.DrawLine(penGrid, points[0], points[1]);
            }
            // From 3 to 6 points interpolation is needed
            else if (points.Length > 2 && points.Length < 7)
            {
                // Coordinates of the screen center
                var origin = new PointF(Map.Width / 2, Map.Height / 2);

                // Screen diagonal
                double diag = Math.Sqrt(Map.Width * Map.Width + Map.Height * Map.Height);

                float maxX = points.Select(p => Math.Abs(p.X)).Max();
                float maxY = points.Select(p => Math.Abs(p.Y)).Max();

                float f = (float)diag / Math.Max(maxX, maxY);

                var scaledPoints = points.Select(p => new PointF(p.X * f, p.Y * f)).ToArray();

                using (GraphicsPath gp = new GraphicsPath())
                {
                    gp.AddCurve(scaledPoints);
                    gp.Flatten();
                    scaledPoints = gp.PathPoints.Select(p => new PointF(p.X / f, p.Y / f)).ToArray();

                    var segments = scaledPoints.Select(p => Geometry.DistanceBetweenPoints(p, origin) < diag * 3 ? p : PointF.Empty)
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

                        DrawTrackSegment(g, penGrid, newPoints.ToArray());
                    }

                    if (!segments.Any())
                    {
                        var p0 = scaledPoints.OrderBy(p => Geometry.DistanceBetweenPoints(p, origin)).First();
                        var p1 = scaledPoints.Prev(p0);
                        var p2 = scaledPoints.Next(p0);

                        List<PointF> newPoints = new List<PointF>();
                        if (p1 != PointF.Empty)
                        {
                            newPoints.Insert(0, p1);
                        }
                        if (p2 != PointF.Empty)
                        {
                            newPoints.Add(p2);
                        }

                        DrawTrackSegment(g, penGrid, newPoints.ToArray());
                    }
                }
            }
            // More or equal than 7 points draw the curve in regular way
            else
            {
                g.DrawCurve(penGrid, points);
            }
        }
    }
}
