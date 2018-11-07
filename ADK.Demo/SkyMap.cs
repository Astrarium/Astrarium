using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo
{
    public class SkyMap : ISkyMap
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public double ViewAngle { get; set; } = 90;
        public CrdsHorizontal Center { get; set; } = new CrdsHorizontal(0, 0);
        public bool Antialias { get; set; } = true;

        private double Rho = 0;

        private Sky sky;

        public SkyMap(Sky sky)
        {
            this.sky = sky;
        }

        public PointF Projection(CrdsHorizontal hor)
        {
            // ARC projection, AIPS MEMO 27
            // Zenith Equidistant Projection

            double X, Y, L, M;

            double d = Angle.ToRadians(hor.Altitude);
            double d0 = Angle.ToRadians(Center.Altitude);
            double da = Angle.ToRadians(hor.Azimuth - Center.Azimuth);
            double rho = Angle.ToRadians(Rho);

            double sin_da = Math.Sin(da);
            double cos_da = Math.Cos(da);

            double sin_d = Math.Sin(d);
            double cos_d = Math.Cos(d);

            double sin_d0 = Math.Sin(d0);
            double cos_d0 = Math.Cos(d0);

            double theta = Math.Acos(sin_d * sin_d0 + cos_d * cos_d0 * cos_da);

            if (theta == 0 || double.IsNaN(theta))
            {
                X = 0;
                Y = 0;
                return new PointF((float)(Width / 2.0 + X), (float)(Height / 2.0 - Y));
            }

            double k = theta / Math.Sin(theta);

            L = k * cos_d * sin_da;
            M = k * (sin_d * cos_d0 - cos_d * sin_d0 * cos_da);

            double sin_rho = Math.Sin(rho);
            double cos_rho = Math.Cos(rho);

            X = L * cos_rho + M * sin_rho;
            Y = M * cos_rho - L * sin_rho;

            X = Angle.ToDegrees(X) / ViewAngle * Width / 2;
            Y = Angle.ToDegrees(Y) / ViewAngle * Width / 2;

            return new Point((int)(Width / 2.0 + X), (int)(Height / 2.0 - Y));
        }

        

        /// <summary>
        /// Performs a correction of inverse projection. 
        /// Checks that horizontal coordinates of a point are correct, 
        /// and in case if not correct, applies iterative algorithm for searching correct values.
        /// </summary>
        /// <param name="p">Point to check</param>
        /// <param name="hor">Horizontal coordinates of the point</param>
        /// <returns>Corrected horizontal coordinates</returns>
        private CrdsHorizontal CorrectProjectionInv(PointF p, CrdsHorizontal hor)
        {
            PointF pLeftEdge = Projection(new CrdsHorizontal(Center.Azimuth - 90, hor.Altitude));
            PointF pRightEdge = Projection(new CrdsHorizontal(Center.Azimuth + 90, hor.Altitude));

            PointF pEdge;
            if (p.X < Width / 2.0)
            {
                pEdge = pLeftEdge;
            }
            else
            {
                pEdge = pRightEdge;
            }

            Point origin = new Point((int)(Width / 2.0), (int)(Height / 2.0));

            double edgeToCenter = Geometry.DistanceBetweenPoints(origin, pEdge);

            double currentToCenter = Geometry.DistanceBetweenPoints(origin, p);

            bool correctionNeeded = Math.Abs(Center.Altitude) == 90 || currentToCenter > edgeToCenter;

            if (correctionNeeded)
            {
                // projected coordinates of a horizontal grid pole (zenith or nadir point)
                PointF pole = Projection(new CrdsHorizontal(0, 90 * (Center.Altitude > 0 ? 1 : -1)));

                double angleWhole = 360 - Geometry.AngleBetweenVectors(pole, pLeftEdge, pRightEdge);

                double angleLeft = Geometry.AngleBetweenVectors(pole, p, pLeftEdge);

                double angleRight = Geometry.AngleBetweenVectors(pole, p, pRightEdge);

                int shiftSign = angleLeft < angleRight ? -1 : 1;

                int poleFix = 1;
                if (Center.Altitude == 90 && pole.Y < p.Y)
                {
                    poleFix = -1;
                }
                else if (Center.Altitude == -90 && pole.Y > p.Y)
                {
                    poleFix = -1;
                }

                double poleAngle = Math.Min(angleLeft, angleRight);

                double azimuthShift = poleAngle / angleWhole * 180;

                PointF pCorrected = new PointF(0, 0);

                double distOriginal = Geometry.DistanceBetweenPoints(p, pEdge);
                double distCorrected = 0;

                int iterations = 0;

                do
                {
                    hor = new CrdsHorizontal(Angle.To360(Center.Azimuth + shiftSign * 90 + poleFix * shiftSign * azimuthShift), hor.Altitude);

                    // corrected coordinates of a projected point
                    pCorrected = Projection(hor);

                    distCorrected = Geometry.DistanceBetweenPoints(pCorrected, pEdge);

                    azimuthShift *= distOriginal / distCorrected;

                    iterations++;
                }
                while (Geometry.DistanceBetweenPoints(p, pCorrected) > 2 && iterations < 5);
            }

            return hor;
        }

        public CrdsHorizontal ProjectionInv(PointF p)
        {
            double X = Angle.ToRadians((p.X - Width / 2.0) * ViewAngle / Width * 2);
            double Y = Angle.ToRadians((-p.Y + Height / 2.0) * ViewAngle / Width * 2);

            double L = X * Math.Cos(Angle.ToRadians(Rho)) - Y * Math.Sin(Angle.ToRadians(Rho));
            double M = Y * Math.Cos(Angle.ToRadians(Rho)) + X * Math.Sin(Angle.ToRadians(Rho));

            double theta = Math.Sqrt(L * L + M * M);


            double a;
            double A;

            a = Angle.ToDegrees(Math.Asin(M * Math.Cos(Angle.ToRadians(Center.Altitude)) / (theta / Math.Sin(theta)) + Math.Sin(Angle.ToRadians(Center.Altitude)) * Math.Cos(theta)));
            A = Center.Azimuth + Angle.ToDegrees(Math.Asin(Math.Sin(theta) * L / (theta * Math.Cos(Angle.ToRadians(a)))));
            A = Angle.To360(A);

            return CorrectProjectionInv(p, new CrdsHorizontal() { Altitude = a, Azimuth = A });
        }

        public CrdsHorizontal CoordinatesByPoint(PointF p)
        {
            return ProjectionInv(p);
        }

        public void Render(Graphics g)
        {           
            g.PageUnit = GraphicsUnit.Display;
            g.SmoothingMode = Antialias ? SmoothingMode.HighQuality : SmoothingMode.None;

            Color colorGridEquatorial = Color.FromArgb(0, 64, 64);
            Pen penEquatorialGrid = new Pen(Antialias ? colorGridEquatorial : Color.FromArgb(200, colorGridEquatorial));
            penEquatorialGrid.DashStyle = DashStyle.Dash;

            Color colorGridHorizontal = Color.FromArgb(0, 64, 0);
            Pen penHorizontalGrid = new Pen(Antialias ? colorGridHorizontal : Color.FromArgb(200, colorGridHorizontal));
            penHorizontalGrid.DashStyle = DashStyle.Dash;

            Color colorLineEcliptic = Color.FromArgb(128, 128, 0);
            Pen penEclipticLine = new Pen(Antialias ? colorLineEcliptic : Color.FromArgb(200, colorLineEcliptic));
            penEclipticLine.DashStyle = DashStyle.Dash;

            DrawGrid(g, penHorizontalGrid, sky.GridHorizontal);
            DrawGrid(g, penEquatorialGrid, sky.GridEquatorial);
            DrawGrid(g, penEclipticLine, sky.LineEcliptic);

            g.DrawString(Center.ToString(), SystemFonts.DefaultFont, Brushes.Red, 10, 10);
        }

        // TODO: move to separate renderer
        private void DrawGrid(Graphics g, Pen penGrid, CelestialGrid grid)
        {
            bool isAnyPoint = false;

            // Azimuths 
            for (int j = 0; j < grid.Columns; j++)
            {
                var segments = grid.Column(j)
                    .Select(p => Angle.Separation(grid.ToHorizontal(p), Center) < ViewAngle * 1.2 ? p : null)
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
                        var coord = grid.FromHorizontal(Center);
                        coord.Longitude = segment[0].Longitude;
                        coord.Latitude += -ViewAngle * 1.2 + k * (ViewAngle * 2 * 1.2);
                        coord.Latitude = Math.Min(coord.Latitude, 80);
                        coord.Latitude = Math.Max(coord.Latitude, -80);
                        var refHorizontal = grid.ToHorizontal(coord);
                        refPoints[k] = Projection(refHorizontal);
                    }

                    DrawGroupOfPoints(g, penGrid, segment.Select(s => Projection(grid.ToHorizontal(s))).ToArray(), refPoints);

                    isAnyPoint = true;
                }
            }

            // Altitude circles
            for (int i = 0; i < grid.Rows; i++)
            {
                var segments = grid.Row(i)
                    .Select(p => Angle.Separation(grid.ToHorizontal(p), Center) < ViewAngle * 1.2 ? p : null)
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
                        g.DrawClosedCurve(penGrid, segment.Select(s => Projection(grid.ToHorizontal(s))).ToArray());
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
                            var coord = grid.FromHorizontal(Center);
                            coord.Longitude += -ViewAngle * 1.2 + k * (ViewAngle * 1.2 * 2);
                            coord.Latitude = segment[0].Latitude;
                            var refHorizontal = grid.ToHorizontal(coord);
                            refPoints[k] = Projection(refHorizontal);
                        }

                        if (!Geometry.IsOutOfScreen(refPoints[0], Width, Height) || !Geometry.IsOutOfScreen(refPoints[1], Width, Height))
                        {
                            refPoints = Geometry.LineRectangleIntersection(refPoints[0], refPoints[1], Width, Height);
                        }

                        DrawGroupOfPoints(g, penGrid, segment.Select(s => Projection(grid.ToHorizontal(s))).ToArray(), refPoints);
                    }

                    isAnyPoint = true;
                }
            }

            // Special case: there are no points visible 
            // on the screen at the current position and zoom.
            // Then we select one point that is closest to screen senter. 
            if (!isAnyPoint)
            {
                GridPoint closestPoint = grid.Points.OrderBy(p => Angle.Separation(grid.ToHorizontal(p), Center)).First();

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
                        var coord = grid.FromHorizontal(Center);
                        coord.Longitude += -ViewAngle * 1.2 + k * (ViewAngle * 1.2 * 2);
                        coord.Latitude = segment[0].Latitude;
                        var refHorizontal = grid.ToHorizontal(coord);
                        refPoints[k] = Projection(refHorizontal);
                    }

                    if (!Geometry.IsOutOfScreen(refPoints[0], Width, Height) || !Geometry.IsOutOfScreen(refPoints[1], Width, Height))
                    {
                        refPoints = Geometry.LineRectangleIntersection(refPoints[0], refPoints[1], Width, Height);
                    }

                    DrawGroupOfPoints(g, penGrid, segment.Select(s => Projection(grid.ToHorizontal(s))).ToArray(), refPoints);
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
                        var coord = grid.FromHorizontal(Center);
                        coord.Longitude = segment[0].Longitude;
                        coord.Latitude += -ViewAngle * 1.2 + k * (ViewAngle * 2 * 1.2);
                        coord.Latitude = Math.Min(coord.Latitude, 80);
                        coord.Latitude = Math.Max(coord.Latitude, -80);
                        var refHorizontal = grid.ToHorizontal(coord);
                        refPoints[k] = Projection(refHorizontal);
                    }

                    DrawGroupOfPoints(g, penGrid, segment.Select(s => Projection(grid.ToHorizontal(s))).ToArray(), refPoints);
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
            var origin = new PointF(Width / 2, Height / 2);

            // Small radius is a screen diagonal
            double r = Math.Sqrt(Width * Width + Height * Height) / 2;

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
                    var circle = Geometry.FindCircle(points);
                    if (circle.R / r > 60)
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

        

    }

    
}
