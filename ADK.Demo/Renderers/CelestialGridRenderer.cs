using ADK.Demo.Calculators;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;

namespace ADK.Demo.Renderers
{
    public class CelestialGridRenderer : BaseRenderer
    {
        private readonly ILunarProvider lunarProvider;
        private readonly ISettings settings;

        private PrecessionalElements peFrom1950 = null;
        private PrecessionalElements peTo1950 = null;

        private CelestialGrid LineEcliptic = new CelestialGrid("Ecliptic", 1, 24);
        private CelestialGrid LineGalactic = new CelestialGrid("Galactic", 1, 24);
        private CelestialGrid GridHorizontal = new CelestialGrid("Horizontal", 17, 24);
        private CelestialGrid GridEquatorial = new CelestialGrid("Equatorial", 17, 24);
        private CelestialGrid LineHorizon = new CelestialGrid("Horizon", 1, 24);

        private Pen penGridEquatorial = null;
        private Pen penGridHorizontal = null;
        private Pen penLineEcliptic = null;
        private Pen penLineGalactic = null;

        private Font fontNodeLabel = new Font("Arial", 8);
        private Font fontEquinoxLabel = new Font("Arial", 8);

        private string[] nodesLabels = new string[] { "\u260A", "\u260B" };
        private string[] equinoxLabels = new string[] { "\u2648", "\u264E" };
        private int[] equinoxRA = new int[] { 0, 12 };

        private string[] horizontalLabels = new string[] { "Zenith", "Nadir" };
        private CrdsHorizontal[] horizontalPoles = new CrdsHorizontal[2] { new CrdsHorizontal(0, 90), new CrdsHorizontal(0, -90) };

        private string[] equatorialLabels = new string[] { "NCP", "SCP" };
        private GridPoint[] polePoints = new GridPoint[] { new GridPoint(0, 90), new GridPoint(0, -90) };

        public CelestialGridRenderer(ILunarProvider lunarProvider, ISettings settings)
        {
            this.lunarProvider = lunarProvider;
            this.settings = settings;

            penGridEquatorial = new Pen(Brushes.Transparent);
            penGridHorizontal = new Pen(Brushes.Transparent);
            penLineEcliptic = new Pen(Brushes.Transparent);
            penLineGalactic = new Pen(Brushes.Transparent);
        }

        public override void Initialize()
        {
            // Ecliptic
            LineEcliptic.FromHorizontal = (h, ctx) =>
            {
                var eq = h.ToEquatorial(ctx.GeoLocation, ctx.SiderealTime);
                var ec = eq.ToEcliptical(ctx.Epsilon);
                return new GridPoint(ec.Lambda, ec.Beta);
            };
            LineEcliptic.ToHorizontal = (c, ctx) =>
            {
                var ec = new CrdsEcliptical(c.Longitude, c.Latitude);
                var eq = ec.ToEquatorial(ctx.Epsilon);
                return eq.ToHorizontal(ctx.GeoLocation, ctx.SiderealTime);
            };

            // Galactic equator
            LineGalactic.FromHorizontal = (h, ctx) =>
            {
                var eq = h.ToEquatorial(ctx.GeoLocation, ctx.SiderealTime);
                var eq1950 = Precession.GetEquatorialCoordinates(eq, peTo1950);
                var gal = eq1950.ToGalactical();
                return new GridPoint(gal.l, gal.b);
            };
            LineGalactic.ToHorizontal = (c, ctx) =>
            {
                var gal = new CrdsGalactical(c.Longitude, c.Latitude);
                var eq1950 = gal.ToEquatorial();
                var eq = Precession.GetEquatorialCoordinates(eq1950, peFrom1950);
                return eq.ToHorizontal(ctx.GeoLocation, ctx.SiderealTime);
            };

            // Horizontal grid
            GridHorizontal.FromHorizontal = (h, ctx) => new GridPoint(h.Azimuth, h.Altitude);
            GridHorizontal.ToHorizontal = (c, context) => new CrdsHorizontal(c.Longitude, c.Latitude);

            // Equatorial grid
            GridEquatorial.FromHorizontal = (h, ctx) =>
            {
                var eq = h.ToEquatorial(ctx.GeoLocation, ctx.SiderealTime);
                return new GridPoint(eq.Alpha, eq.Delta);
            };
            GridEquatorial.ToHorizontal = (c, ctx) =>
            {
                var eq = new CrdsEquatorial(c.Longitude, c.Latitude);
                return eq.ToHorizontal(ctx.GeoLocation, ctx.SiderealTime);
            };

            // Hozizon line            
            LineHorizon.FromHorizontal = (h, ctx) => new GridPoint(h.Azimuth, h.Altitude);
            LineHorizon.ToHorizontal = (c, ctx) => new CrdsHorizontal(c.Longitude, c.Latitude);
        }

        public override void Render(IMapContext map)
        {
            peFrom1950 = Precession.ElementsFK5(Date.EPOCH_B1950, map.JulianDay);
            peTo1950 = Precession.ElementsFK5(map.JulianDay, Date.EPOCH_B1950);

            Color colorGridEquatorial = Color.FromArgb(200, 0, 64, 64);
            Color colorGridHorizontal = settings.Get<Color>("HorizontalGrid.Color.Night");
            Color colorLineEcliptic = settings.Get<Color>("Ecliptic.Color.Night");
            Color colorLineGalactic = Color.FromArgb(200, 64, 0, 64);

            penGridEquatorial.Color = colorGridEquatorial;
            penGridHorizontal.Color = colorGridHorizontal;
            penLineEcliptic.Color = colorLineEcliptic;
            penLineGalactic.Color = colorLineGalactic;

            if (settings.Get<bool>("GalacticEquator"))
            {
                DrawGrid(map, penLineGalactic, LineGalactic);
            }
            if (settings.Get<bool>("EquatorialGrid"))
            {
                DrawGrid(map, penGridEquatorial, GridEquatorial);
                DrawEquatorialPoles(map);
            }
            if (settings.Get<bool>("HorizontalGrid"))
            {
                DrawGrid(map, penGridHorizontal, GridHorizontal);
                DrawHorizontalPoles(map);
            }
            if (settings.Get<bool>("EclipticLine"))
            {
                DrawGrid(map, penLineEcliptic, LineEcliptic);
                DrawEquinoxLabels(map);
                DrawLunarNodes(map);
            }
            if (settings.Get<bool>("HorizonLine"))
            {
                DrawGrid(map, penGridHorizontal, LineHorizon);
            }
        }

        public override int ZOrder => 400;

        private void DrawGrid(IMapContext map, Pen penGrid, CelestialGrid grid)
        {
            bool isAnyPoint = false;

            // Azimuths 
            for (int j = 0; j < grid.Columns; j++)
            {
                var segments = grid.Column(j)
                    .Select(p => Angle.Separation(grid.ToHorizontal(p, map), map.Center) < map.ViewAngle * 1.2 ? p : null)
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
                        var coord = grid.FromHorizontal(map.Center, map);
                        coord.Longitude = segment[0].Longitude;
                        coord.Latitude += -map.ViewAngle * 1.2 + k * (map.ViewAngle * 2 * 1.2);
                        coord.Latitude = Math.Min(coord.Latitude, 80);
                        coord.Latitude = Math.Max(coord.Latitude, -80);
                        var refHorizontal = grid.ToHorizontal(coord, map);
                        refPoints[k] = map.Project(refHorizontal);
                    }

                    DrawGroupOfPoints(map, penGrid, segment.Select(s => map.Project(grid.ToHorizontal(s, map))).ToArray(), refPoints);

                    isAnyPoint = true;
                }
            }

            // Altitude circles
            for (int i = 0; i < grid.Rows; i++)
            {
                var segments = grid.Row(i)
                    .Select(p => Angle.Separation(grid.ToHorizontal(p, map), map.Center) < map.ViewAngle * 1.2 ? p : null)
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
                        map.Graphics.DrawClosedCurve(penGrid, segment.Select(s => map.Project(grid.ToHorizontal(s, map))).ToArray());
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
                            var coord = grid.FromHorizontal(map.Center, map);
                            coord.Longitude += -map.ViewAngle * 1.2 + k * (map.ViewAngle * 1.2 * 2);
                            coord.Latitude = segment[0].Latitude;
                            var refHorizontal = grid.ToHorizontal(coord, map);
                            refPoints[k] = map.Project(refHorizontal);
                        }

                        if (!map.IsOutOfScreen(refPoints[0]) || !map.IsOutOfScreen(refPoints[1]))
                        {
                            refPoints = map.LineScreenIntersection(refPoints[0], refPoints[1]);
                        }

                        DrawGroupOfPoints(map, penGrid, segment.Select(s => map.Project(grid.ToHorizontal(s, map))).ToArray(), refPoints);
                    }

                    isAnyPoint = true;
                }
            }

            // Special case: there are no points visible 
            // on the screen at the current position and zoom.
            // Then we select one point that is closest to screen senter. 
            if (!isAnyPoint)
            {
                GridPoint closestPoint = grid.Points.OrderBy(p => Angle.Separation(grid.ToHorizontal(p, map), map.Center)).First();
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
                        var coord = grid.FromHorizontal(map.Center, map);
                        coord.Longitude += -map.ViewAngle * 1.2 + k * (map.ViewAngle * 1.2 * 2);
                        coord.Latitude = segment[0].Latitude;
                        var refHorizontal = grid.ToHorizontal(coord, map);
                        refPoints[k] = map.Project(refHorizontal);
                    }

                    if (!map.IsOutOfScreen(refPoints[0]) || !map.IsOutOfScreen(refPoints[1]))
                    {
                        refPoints = map.LineScreenIntersection(refPoints[0], refPoints[1]);
                    }

                    DrawGroupOfPoints(map, penGrid, segment.Select(s => map.Project(grid.ToHorizontal(s, map))).ToArray(), refPoints);
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
                        var coord = grid.FromHorizontal(map.Center, map);
                        coord.Longitude = segment[0].Longitude;
                        coord.Latitude += -map.ViewAngle * 1.2 + k * (map.ViewAngle * 2 * 1.2);
                        coord.Latitude = Math.Min(coord.Latitude, 80);
                        coord.Latitude = Math.Max(coord.Latitude, -80);
                        var refHorizontal = grid.ToHorizontal(coord, map);
                        refPoints[k] = map.Project(refHorizontal);
                    }

                    DrawGroupOfPoints(map, penGrid, segment.Select(s => map.Project(grid.ToHorizontal(s, map))).ToArray(), refPoints);
                }
            }
        }

        private void DrawGroupOfPoints(IMapContext map, Pen penGrid, PointF[] points, PointF[] refPoints)
        {
            // Do not draw figure containing less than 2 points
            if (points.Length < 2)
            {
                return;
            }

            // Two points can be simply drawn as a line
            if (points.Length == 2)
            {
                map.Graphics.DrawLine(penGrid, points[0], points[1]);
                return;
            }

            // Coordinates of the screen center
            var origin = new PointF(map.Width / 2, map.Height / 2);

            // Small radius is a screen diagonal
            double r = Math.Sqrt(map.Width * map.Width + map.Height * map.Height) / 2;

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
                        map.Graphics.DrawLine(penGrid, refPoints[0], refPoints[1]);
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
                        map.Graphics.DrawLine(penGrid, refPoints[0], refPoints[1]);
                        return;
                    }
                }
            }

            if (points.All(p => Geometry.DistanceBetweenPoints(p, origin) < r * 60))
            {
                // Draw the curve in regular way
                map.Graphics.DrawCurve(penGrid, points);
            }
        }

        private void DrawEquinoxLabels(IMapContext map)
        {
            if (settings.Get<bool>("LabelEquinoxPoints"))
            {
                for (int i = 0; i < 2; i++)
                {
                    var h = LineEcliptic.ToHorizontal(LineEcliptic.Column(equinoxRA[i]).ElementAt(0), map);
                    if (Angle.Separation(h, map.Center) < map.ViewAngle * 1.2)
                    {
                        PointF p = map.Project(h);

                        var hint = map.Graphics.TextRenderingHint;
                        map.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                        map.Graphics.DrawStringOpaque(equinoxLabels[i], fontEquinoxLabel, penLineEcliptic.Brush, Brushes.Black, p);
                        map.Graphics.TextRenderingHint = hint;
                    }
                }
            }
        }

        private void DrawLunarNodes(IMapContext map)
        {
            if (settings.Get<bool>("LabelLunarNodes"))
            {
                double ascNode = lunarProvider.Moon.AscendingNode;

                for (int i = 0; i < 2; i++)
                {
                    var h = LineEcliptic.ToHorizontal(new GridPoint(ascNode + (i > 0 ? 180 : 0), 0), map);
                    if (Angle.Separation(h, map.Center) < map.ViewAngle * 1.2)
                    {
                        PointF p = map.Project(h);

                        var hint = map.Graphics.TextRenderingHint;
                        map.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                        map.Graphics.FillEllipse(penLineEcliptic.Brush, p.X - 1.5f, p.Y - 1.5f, 3, 3);
                        map.Graphics.DrawStringOpaque(nodesLabels[i], fontNodeLabel, penLineEcliptic.Brush, Brushes.Black, p);
                        map.Graphics.TextRenderingHint = hint;
                    }
                }
            }
        }

        private void DrawHorizontalPoles(IMapContext map)
        {
            if (settings.Get<bool>("LabelHorizontalPoles"))
            {
                for (int i = 0; i < 2; i++)
                {
                    if (Angle.Separation(horizontalPoles[i], map.Center) < map.ViewAngle * 1.2)
                    {
                        PointF p = map.Project(horizontalPoles[i]);
                        map.Graphics.DrawXCross(penGridHorizontal, p, 3);
                        map.Graphics.DrawString(horizontalLabels[i], SystemFonts.DefaultFont, penGridHorizontal.Brush, p.X + 5, p.Y + 5);
                    }
                }
            }
        }

        private void DrawEquatorialPoles(IMapContext map)
        {
            if (settings.Get<bool>("LabelEquatorialPoles"))
            {
                for (int i = 0; i < 2; i++)
                {
                    var h = GridEquatorial.ToHorizontal(polePoints[i], map);
                    if (Angle.Separation(h, map.Center) < map.ViewAngle * 1.2)
                    {
                        PointF p = map.Project(h);
                        map.Graphics.DrawXCross(penGridEquatorial, p, 3);
                        map.Graphics.DrawString(equatorialLabels[i], SystemFonts.DefaultFont, penGridEquatorial.Brush, p.X + 5, p.Y + 5);
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
