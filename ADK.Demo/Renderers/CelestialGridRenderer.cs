using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ADK.Demo.Renderers
{
    public class CelestialGridRenderer : BaseSkyRenderer
    {
        private CelestialGrid gridEquatorial = null;
        private CelestialGrid gridHorizontal = null;
        private CelestialGrid lineEcliptic = null;
        private CelestialGrid lineGalactic = null;
        private CelestialGrid lineHorizon = null;

        private Pen penGridEquatorial = null;
        private Pen penGridHorizontal = null;
        private Pen penLineEcliptic = null;
        private Pen penLineGalactic = null;

        private string[] equinoxLabels = new string[] { "\u2648", "\u264E" };
        private int[] equinoxRA = new int[] { 0, 12 };

        private string[] horizontalLabels = new string[] { "Zenith", "Nadir" };
        private CrdsHorizontal[] horizontalPoles = new CrdsHorizontal[2] { new CrdsHorizontal(0, 90), new CrdsHorizontal(0, -90) };

        private string[] equatorialLabels = new string[] { "NCP", "SCP" };
        private GridPoint[] polePoints = new GridPoint[] { new GridPoint(0, 90), new GridPoint(0, -90) };

        public CelestialGridRenderer(Sky sky, ISkyMap skyMap, ISettings settings) : base(sky, skyMap, settings)
        {
            gridEquatorial = Sky.Get<CelestialGrid>("GridEquatorial");
            gridHorizontal = Sky.Get<CelestialGrid>("GridHorizontal");
            lineEcliptic = Sky.Get<CelestialGrid>("LineEcliptic");
            lineGalactic = Sky.Get<CelestialGrid>("LineGalactic");
            lineHorizon = Sky.Get<CelestialGrid>("LineHorizon");

            penGridEquatorial = new Pen(Brushes.Transparent);
            penGridHorizontal = new Pen(Brushes.Transparent);
            penLineEcliptic = new Pen(Brushes.Transparent);
            penLineGalactic = new Pen(Brushes.Transparent);
        }

        public override void Render(Graphics g)
        {
            Color colorGridEquatorial = Color.FromArgb(200, 0, 64, 64);
            Color colorGridHorizontal = Settings.Get<Color>("HorizontalGrid.Color.Night");
            Color colorLineEcliptic = Settings.Get<Color>("Ecliptic.Color.Night");
            Color colorLineGalactic = Color.FromArgb(200, 64, 0, 64);

            penGridEquatorial.Color = Map.Antialias ? colorGridEquatorial : Color.FromArgb(200, colorGridEquatorial);            
            penGridHorizontal.Color = Map.Antialias ? colorGridHorizontal : Color.FromArgb(200, colorGridHorizontal);
            penLineEcliptic.Color = Map.Antialias ? colorLineEcliptic : Color.FromArgb(200, colorLineEcliptic);
            penLineGalactic.Color = Map.Antialias ? colorLineGalactic : Color.FromArgb(200, colorLineGalactic);

            if (Settings.Get<bool>("GalacticEquator"))
            {
                DrawGrid(g, penLineGalactic, lineGalactic);
            }
            if (Settings.Get<bool>("EquatorialGrid"))
            {
                DrawGrid(g, penGridEquatorial, gridEquatorial);
                DrawEquatorialPoles(g);
            }
            if (Settings.Get<bool>("HorizontalGrid"))
            {
                DrawGrid(g, penGridHorizontal, gridHorizontal);
                DrawHorizontalPoles(g);
            }
            if (Settings.Get<bool>("EclipticLine"))
            {
                DrawGrid(g, penLineEcliptic, lineEcliptic);
                DrawEquinoxLabels(g);
            }
            if (Settings.Get<bool>("HorizonLine"))
            {
                DrawGrid(g, penGridHorizontal, lineHorizon);
            }
        }

        private void DrawGrid(Graphics g, Pen penGrid, CelestialGrid grid)
        {
            bool isAnyPoint = false;

            // Azimuths 
            for (int j = 0; j < grid.Columns; j++)
            {
                var segments = grid.Column(j)
                    .Select(p => Angle.Separation(grid.ToHorizontal(p), Map.Center) < Map.ViewAngle * 1.2 ? p : null)
                    .Split(p => p == null, true);

                foreach (var segment in segments)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        if (segment.First().RowIndex > 1)
                            segment.Insert(0, grid[segment.First().RowIndex - 1, j]);
                    }

                    for (int k = 0; k < 2; k++)
                    {
                        if (segment.Last().RowIndex < grid.Rows - 2)
                            segment.Add(grid[segment.Last().RowIndex + 1, j]);
                    }

                    PointF[] refPoints = new PointF[2];
                    for (int k = 0; k < 2; k++)
                    {
                        var coord = grid.FromHorizontal(Map.Center);
                        coord.Longitude = segment[0].Longitude;
                        coord.Latitude += -Map.ViewAngle * 1.2 + k * (Map.ViewAngle * 2 * 1.2);
                        coord.Latitude = Math.Min(coord.Latitude, 80);
                        coord.Latitude = Math.Max(coord.Latitude, -80);
                        var refHorizontal = grid.ToHorizontal(coord);
                        refPoints[k] = Map.Projection.Project(refHorizontal);
                    }

                    DrawGroupOfPoints(g, penGrid, segment.Select(s => Map.Projection.Project(grid.ToHorizontal(s))).ToArray(), refPoints);

                    isAnyPoint = true;
                }
            }

            // Altitude circles
            for (int i = 0; i < grid.Rows; i++)
            {
                var segments = grid.Row(i)
                    .Select(p => Angle.Separation(grid.ToHorizontal(p), Map.Center) < Map.ViewAngle * 1.2 ? p : null)
                    .Split(p => p == null, true).ToList();

                // segment that starts with point "0 degrees"
                var seg0 = segments.FirstOrDefault(s => s.First().ColumnIndex == 0);

                // segment that ends with point "345 degrees"
                var seg23 = segments.FirstOrDefault(s => s.Last().ColumnIndex == 23);

                // join segments into one
                if (seg0 != null && seg23 != null && seg0 != seg23)
                {
                    segments.Remove(seg0);
                    seg23.AddRange(seg0);
                }

                foreach (var segment in segments)
                {
                    if (segment.Count == 24)
                    {
                        g.DrawClosedCurve(penGrid, segment.Select(s => Map.Projection.Project(grid.ToHorizontal(s))).ToArray());
                    }
                    else
                    {
                        for (int k = 0; k < 2; k++)
                        {
                            int col = segment.First().ColumnIndex;
                            if (col == 0)
                                segment.Insert(0, grid[i, 23]);
                            else
                                segment.Insert(0, grid[i, col - 1]);
                        }

                        for (int k = 0; k < 2; k++)
                        {
                            int col = segment.Last().ColumnIndex;

                            if (col < 23)
                                segment.Add(grid[i, col + 1]);
                            else if (col == 23)
                                segment.Add(grid[i, 0]);
                        }

                        PointF[] refPoints = new PointF[2];
                        for (int k = 0; k < 2; k++)
                        {
                            var coord = grid.FromHorizontal(Map.Center);
                            coord.Longitude += -Map.ViewAngle * 1.2 + k * (Map.ViewAngle * 1.2 * 2);
                            coord.Latitude = segment[0].Latitude;
                            var refHorizontal = grid.ToHorizontal(coord);
                            refPoints[k] = Map.Projection.Project(refHorizontal);
                        }

                        if (!IsOutOfScreen(refPoints[0]) || !IsOutOfScreen(refPoints[1]))
                        {
                            refPoints = LineScreenIntersection(refPoints[0], refPoints[1]);
                        }

                        DrawGroupOfPoints(g, penGrid, segment.Select(s => Map.Projection.Project(grid.ToHorizontal(s))).ToArray(), refPoints);
                    }

                    isAnyPoint = true;
                }
            }

            // Special case: there are no points visible 
            // on the screen at the current position and zoom.
            // Then we select one point that is closest to screen senter. 
            if (!isAnyPoint)
            {
                GridPoint closestPoint = grid.Points.OrderBy(p => Angle.Separation(grid.ToHorizontal(p), Map.Center)).First();
                {
                    var segment = new List<GridPoint>();
                    segment.Add(closestPoint);
                    int i = closestPoint.RowIndex;

                    for (int k = 0; k < 2; k++)
                    {
                        int col = segment.First().ColumnIndex;
                        if (col == 0)
                            segment.Insert(0, grid[i, 23]);
                        else
                            segment.Insert(0, grid[i, col - 1]);
                    }

                    for (int k = 0; k < 2; k++)
                    {
                        int col = segment.Last().ColumnIndex;

                        if (col < 23)
                            segment.Add(grid[i, col + 1]);
                        else if (col == 23)
                            segment.Add(grid[i, 0]);
                    }

                    PointF[] refPoints = new PointF[2];
                    for (int k = 0; k < 2; k++)
                    {
                        var coord = grid.FromHorizontal(Map.Center);
                        coord.Longitude += -Map.ViewAngle * 1.2 + k * (Map.ViewAngle * 1.2 * 2);
                        coord.Latitude = segment[0].Latitude;
                        var refHorizontal = grid.ToHorizontal(coord);
                        refPoints[k] = Map.Projection.Project(refHorizontal);
                    }

                    if (!IsOutOfScreen(refPoints[0]) || !IsOutOfScreen(refPoints[1]))
                    {
                        refPoints = LineScreenIntersection(refPoints[0], refPoints[1]);
                    }

                    DrawGroupOfPoints(g, penGrid, segment.Select(s => Map.Projection.Project(grid.ToHorizontal(s))).ToArray(), refPoints);
                }

                {
                    var segment = new List<GridPoint>();
                    segment.Add(closestPoint);
                    int j = closestPoint.ColumnIndex;

                    for (int k = 0; k < 2; k++)
                    {
                        if (segment.First().RowIndex > 1)
                            segment.Insert(0, grid[segment.First().RowIndex - 1, j]);
                    }

                    for (int k = 0; k < 2; k++)
                    {
                        if (segment.Last().RowIndex < grid.Rows - 2)
                            segment.Add(grid[segment.Last().RowIndex + 1, j]);
                    }

                    PointF[] refPoints = new PointF[2];
                    for (int k = 0; k < 2; k++)
                    {
                        var coord = grid.FromHorizontal(Map.Center);
                        coord.Longitude = segment[0].Longitude;
                        coord.Latitude += -Map.ViewAngle * 1.2 + k * (Map.ViewAngle * 2 * 1.2);
                        coord.Latitude = Math.Min(coord.Latitude, 80);
                        coord.Latitude = Math.Max(coord.Latitude, -80);
                        var refHorizontal = grid.ToHorizontal(coord);
                        refPoints[k] = Map.Projection.Project(refHorizontal);
                    }

                    DrawGroupOfPoints(g, penGrid, segment.Select(s => Map.Projection.Project(grid.ToHorizontal(s))).ToArray(), refPoints);
                }
            }
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
                    // Check the at lease one last point of the curve 
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

        private void DrawEquinoxLabels(Graphics g)
        {
            if (Settings.Get<bool>("LabelEquinoxPoints"))
            {
                for (int i = 0; i < 2; i++)
                {
                    var h = lineEcliptic.ToHorizontal(lineEcliptic.Column(equinoxRA[i]).ElementAt(0));
                    if (Angle.Separation(h, Map.Center) < Map.ViewAngle * 1.2)
                    {
                        PointF p = Map.Projection.Project(h);
                        g.DrawStringOpaque(equinoxLabels[i], SystemFonts.DefaultFont, penLineEcliptic.Brush, Brushes.Black, p);
                    }
                }
            }
        }

        private void DrawHorizontalPoles(Graphics g)
        {
            if (Settings.Get<bool>("LabelHorizontalPoles"))
            {
                for (int i = 0; i < 2; i++)
                {
                    if (Angle.Separation(horizontalPoles[i], Map.Center) < Map.ViewAngle * 1.2)
                    {
                        PointF p = Map.Projection.Project(horizontalPoles[i]);
                        g.DrawXCross(penGridHorizontal, p, 3);
                        g.DrawString(horizontalLabels[i], SystemFonts.DefaultFont, penGridHorizontal.Brush, p.X + 5, p.Y + 5);
                    }
                }
            }
        }

        private void DrawEquatorialPoles(Graphics g)
        {
            if (Settings.Get<bool>("LabelEquatorialPoles"))
            {
                for (int i = 0; i < 2; i++)
                {
                    var h = gridEquatorial.ToHorizontal(polePoints[i]);
                    if (Angle.Separation(h, Map.Center) < Map.ViewAngle * 1.2)
                    {
                        PointF p = Map.Projection.Project(h);
                        g.DrawXCross(penGridEquatorial, p, 3);
                        g.DrawString(equatorialLabels[i], SystemFonts.DefaultFont, penGridEquatorial.Brush, p.X + 5, p.Y + 5);
                    }
                }
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
