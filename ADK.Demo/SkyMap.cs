using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo
{

    public class CrdsSpherical
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public CrdsSpherical(double longitude, double latitude)
        {
            Longitude = longitude;
            Latitude = latitude;
        }
    }

    public class GridPoint
    {
        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }

        public double Longitude { get; set; }
        public double Latitude { get; set; }

        public GridPoint(double longitude, double latitude)
        {
            Longitude = longitude;
            Latitude = latitude;
        }
    }

    public class CelestialGrid
    {
        protected GridPoint[,] points = null;

        public int Rows { get; private set; }

        public int Columns { get; private set; }
        
        public CelestialGrid(int rows, int columns)
        {
            points = new GridPoint[rows, columns];
            Rows = rows;
            Columns = columns;

            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    double latitude = i * 10 - Rows / 2 * 10;
                    double longitude = j * 15;
                    points[i, j] = new GridPoint(longitude, latitude);
                    points[i, j].RowIndex = i;
                    points[i, j].ColumnIndex = j;
                }
            }
        }

        public GridPoint this[int row, int column]
        {
            get { return points[row, column]; }
            set { points[row, column] = value; }
        }

        public IEnumerable<GridPoint> Points
        {
            get
            {
                return points.Cast<GridPoint>();
            }
        }

        public IEnumerable<GridPoint> Column(int columnNumber)
        {
            return Enumerable.Range(0, points.GetLength(0))
                    .Select(x => points[x, columnNumber]);
        }

        public IEnumerable<GridPoint> Row(int rowNumber)
        {
            return Enumerable.Range(0, points.GetLength(1))
                    .Select(x => points[rowNumber, x]);
        }

        public Func<CrdsHorizontal, GridPoint> FromHorizontal { get; set; }
        public Func<GridPoint, CrdsHorizontal> ToHorizontal { get; set; }
    }

    public class SkyMap : ISkyMap
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public double ViewAngle { get; set; } = 90;
        public CrdsHorizontal Center { get; set; } = new CrdsHorizontal(0, 0);
        public bool Antialias { get; set; } = true;

        private double Rho = 0;

        // TODO: this is temp
        private CelestialGrid GridHorizontal = new CelestialGrid(17, 24);

        private CelestialGrid GridEquatorial = new CelestialGrid(17, 24);

        private CelestialGrid LineEcliptic = new CelestialGrid(1, 24);

        private CrdsGeographical GeoLocation = new CrdsGeographical(56.3333, 44);

        private double Epsilon = Date.MeanObliquity(new Date(DateTime.Now).ToJulianDay());

        private double LocalSiderealTime = 17; 

        public SkyMap()
        {
            LineEcliptic.FromHorizontal = (h) =>
            {
                var eq = h.ToEquatorial(GeoLocation, LocalSiderealTime);
                var ec = eq.ToEcliptical(Epsilon);
                return new GridPoint(ec.Lambda, ec.Beta);
            };
            LineEcliptic.ToHorizontal = (c) =>
            {
                var ec = new CrdsEcliptical(c.Longitude, c.Latitude);
                var eq = ec.ToEquatorial(Epsilon);
                return eq.ToHorizontal(GeoLocation, LocalSiderealTime);
            };

            // Horizontal grid
            GridHorizontal.FromHorizontal = (h) => new GridPoint(h.Azimuth, h.Altitude);
            GridHorizontal.ToHorizontal = (c) => new CrdsHorizontal(c.Longitude, c.Latitude);

            // Equatorial grid
            GridEquatorial.FromHorizontal = (h) =>
            {
                var eq = h.ToEquatorial(GeoLocation, LocalSiderealTime);
                return new GridPoint(eq.Alpha, eq.Delta);
            };
            GridEquatorial.ToHorizontal = (c) =>
            {
                var eq = new CrdsEquatorial(c.Longitude, c.Latitude);
                return eq.ToHorizontal(GeoLocation, LocalSiderealTime);
            };
        }

        public PointF Projection(CrdsHorizontal hor)
        {
            // ARC projection, AIPS MEMO 27
            // Zenith Equidistant Projection

            double da = hor.Azimuth - Center.Azimuth;
            double X, Y;

            double horAltitudeRadian = Angle.ToRadians(hor.Altitude);
            double centerHorAltitudeRadian = Angle.ToRadians(Center.Altitude);
            double daRadian = Angle.ToRadians(da);
            double rhoRadian = Angle.ToRadians(Rho);

            double sinDaRadian = Math.Sin(daRadian);
            double cosDaRadian = Math.Cos(daRadian);

            double sinHorAltitudeRadian = Math.Sin(horAltitudeRadian);
            double cosHorAltitudeRadian = Math.Cos(horAltitudeRadian);

            double sinCenterHorAltitudeRadian = Math.Sin(centerHorAltitudeRadian);
            double cosCenterHorAltitudeRadian = Math.Cos(centerHorAltitudeRadian);

            double theta = Angle.ToDegrees(Math.Acos(sinHorAltitudeRadian * sinCenterHorAltitudeRadian + cosHorAltitudeRadian * cosCenterHorAltitudeRadian * cosDaRadian));

            double L, M;

            if (theta == 0 || Double.IsNaN(theta))
            {
                X = 0;
                Y = 0;
                return new Point((int)(Width / 2.0 + X), (int)(Height / 2.0 - Y));
            }

            double thetaRadian = Angle.ToRadians(theta);

            double k = thetaRadian / Math.Sin(thetaRadian);

            L = k * cosHorAltitudeRadian * sinDaRadian;
            M = k * (sinHorAltitudeRadian * cosCenterHorAltitudeRadian - cosHorAltitudeRadian * sinCenterHorAltitudeRadian * cosDaRadian);

            double sinRhoRadian = Math.Sin(rhoRadian);
            double cosRhoRadian = Math.Cos(rhoRadian);

            X = L * cosRhoRadian + M * sinRhoRadian;
            Y = M * cosRhoRadian - L * sinRhoRadian;

            X = Angle.ToDegrees(X) / ViewAngle * Width / 2;
            Y = Angle.ToDegrees(Y) / ViewAngle * Width / 2;

            return new Point((int)(Width / 2.0 + X), (int)(Height / 2.0 - Y));
        }

        /// <summary>
        /// Gets angle between two vectors starting with same point.
        /// </summary>
        /// <param name="p0">Common point of two vectors (starting point for both vectors).</param>
        /// <param name="p1">End point of first vector</param>
        /// <param name="p2">End point of first vector</param>
        /// <returns>Angle between two vectors, in degrees, in range [0...180]</returns>
        private double AngleBetweenVectors(PointF p0, PointF p1, PointF p2)
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
        private double DistanceBetweenPoints(PointF p1, PointF p2)
        {
            double deltaX = p1.X - p2.X;
            double deltaY = p1.Y - p2.Y;
            return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
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
            PointF pLeftEdge = Projection(new CrdsHorizontal() { Azimuth = Center.Azimuth - 90, Altitude = hor.Altitude });
            PointF pRightEdge = Projection(new CrdsHorizontal() { Azimuth = Center.Azimuth + 90, Altitude = hor.Altitude });

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

            double edgeToCenter = DistanceBetweenPoints(origin, pEdge);

            double currentToCenter = DistanceBetweenPoints(origin, p);

            bool correctionNeeded = Math.Abs(Center.Altitude) == 90 || currentToCenter > edgeToCenter;

            if (correctionNeeded)
            {
                // projected coordinates of a horizontal grid pole (zenith or nadir point)
                PointF pole = Projection(new CrdsHorizontal() { Altitude = 90 * (Center.Altitude > 0 ? 1 : -1), Azimuth = 0 });

                double angleWhole = 360 - AngleBetweenVectors(pole, pLeftEdge, pRightEdge);

                double angleLeft = AngleBetweenVectors(pole, p, pLeftEdge);

                double angleRight = AngleBetweenVectors(pole, p, pRightEdge);

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

                double distOriginal = DistanceBetweenPoints(p, pEdge);
                double distCorrected = 0;

                int iterations = 0;

                do
                {
                    hor = new CrdsHorizontal() { Altitude = hor.Altitude, Azimuth = Angle.To360(Center.Azimuth + shiftSign * 90 + poleFix * shiftSign * azimuthShift) };

                    // corrected coordinates of a projected point
                    pCorrected = Projection(hor);

                    distCorrected = DistanceBetweenPoints(pCorrected, pEdge);

                    azimuthShift *= distOriginal / distCorrected;

                    iterations++;
                }
                while (DistanceBetweenPoints(p, pCorrected) > 2 && iterations < 5);
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

            g.Clear(Color.Black);

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

            //DrawGrid(g, penHorizontalGrid, GridHorizontal);
            DrawGrid(g, penEquatorialGrid, GridEquatorial);
            DrawGrid(g, penEclipticLine, LineEcliptic);

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

                        if (!IsOutOfScreen(refPoints[0]) || !IsOutOfScreen(refPoints[1]))
                        {
                            refPoints = LineRectangleIntersection(refPoints[0], refPoints[1], Width, Height);
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

                    if (!IsOutOfScreen(refPoints[0]) || !IsOutOfScreen(refPoints[1]))
                    {
                        refPoints = LineRectangleIntersection(refPoints[0], refPoints[1], Width, Height);
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
                double alpha = AngleBetweenVectors(pMid, pStart, pEnd);

                double d1 = DistanceBetweenPoints(pStart, origin);
                double d2 = DistanceBetweenPoints(pEnd, origin);

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
                    var circle = FindCircle(points);
                    if (circle.R / r > 60)
                    {
                        g.DrawLine(penGrid, refPoints[0], refPoints[1]);
                        return;
                    }
                }
            }

            if (points.All(p => DistanceBetweenPoints(p, origin) < r * 60))
            {
                // Draw the curve in regular way
                g.DrawCurve(penGrid, points);
            }
        }

        public Circle FindCircle(PointF[] l)
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

        public Circle FindCircle(PointF p1, PointF p2, PointF p3)
        {
            double ma = (p2.Y - p1.Y) / (p2.X - p1.X);
            double mb = (p3.Y - p2.Y) / (p3.X - p2.X);

            double x = (ma * mb * (p1.Y - p3.Y) + mb * (p1.X + p2.X) - ma * (p2.X + p3.X)) / (2 * mb - ma);
            double y = -1.0 / ma * (x - (p1.X + p2.X) / 2) + (p1.Y + p2.Y) / 2;

            double r = DistanceBetweenPoints(new PointF((float)x, (float)y), p1);

            return new Circle() { X = x, Y = y, R = r };
        }

        // https://www.xarg.org/2016/07/calculate-the-intersection-points-of-two-circles/
        public PointF[] CirclesIntersection(Circle A, Circle B)
        {
            double d = DistanceBetweenPoints(new PointF((float)A.X, (float)A.Y), new PointF((float)B.X, (float)B.Y));

            double ex = (B.X - A.X) / d;
            double ey = (B.Y - A.Y) / d;

            double x = (A.R * A.R - B.R * B.R + d * d) / (2 * d);
            double y = Math.Sqrt(A.R * A.R - x * x);

            PointF p1 = new PointF(
                (float)(A.X + x * ex - y * ey),
                (float)(A.Y + x * ey + y * ex));

            PointF p2 = new PointF(
                (float)(A.X + x * ex + y * ey),
                (float)(A.Y + x * ey - y * ex));

            return new PointF[] { p1, p2 };
        } 

        /// <summary>
        /// Checks if the point is out of screen bounds
        /// </summary>
        /// <param name="p">Point to check</param>
        /// <returns>True if out from screen, false otherwise</returns>
        private bool IsOutOfScreen(PointF p, float margin = 0)
        {
            return p.Y < -margin || p.Y > Height + margin || p.X < -margin || p.X > Width + margin;
        }

        private PointF? LinesIntersection(PointF p1, PointF p2, PointF p3, PointF p4)
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

        private PointF[] LineRectangleIntersection(PointF p1, PointF p2, int width, int height)
        {
            PointF p00 = new PointF(0, 0);
            PointF pW0 = new PointF(Width, 0);
            PointF pWH = new PointF(Width, Height);
            PointF p0H = new PointF(0, Height);

            List<PointF> crosses = new List<PointF>();

            PointF? c1 = LinesIntersection(p1, p2, p00, pW0);
            if (c1 != null && c1.Value.Y == 0 && c1.Value.X >= 0 && c1.Value.X <= Width)
            {
                crosses.Add(c1.Value);
            }

            PointF? c2 = LinesIntersection(p1, p2, pW0, pWH);
            if (c2 != null && c2.Value.X == Width && c2.Value.Y >= 0 && c2.Value.Y <= Height)
            {
                crosses.Add(c2.Value);
            }

            PointF? c3 = LinesIntersection(p1, p2, p0H, pWH);
            if (c3 != null && c3.Value.Y == Height && c3.Value.X >= 0 && c3.Value.X <= Width)
            {
                crosses.Add(c3.Value);
            }

            PointF? c4 = LinesIntersection(p1, p2, p00, p0H);
            if (c4 != null && c4.Value.X == 0 && c4.Value.Y >= 0 && c4.Value.Y <= Height)
            {
                crosses.Add(c4.Value);
            }

            return crosses.ToArray();
        }

        private PointF[] EdgeCrosspoints(PointF p1, PointF p2, int width, int height)
        {
            PointF p00 = new PointF(0, 0);
            PointF pW0 = new PointF(Width, 0);
            PointF pWH = new PointF(Width, Height);
            PointF p0H = new PointF(0, Height);

            List<PointF?> crossPoints = new List<PointF?>();

            // top edge
            crossPoints.Add(CrossingPoint(p1, p2, p00, pW0));
            if (crossPoints.Any())

            // right edge
            crossPoints.Add(CrossingPoint(p1, p2, pW0, pWH));

            // bottom edge
            crossPoints.Add(CrossingPoint(p1, p2, pWH, p0H));

            // left edge
            crossPoints.Add(CrossingPoint(p1, p2, p0H, p00));

            return crossPoints.Where(p => p != null).Cast<PointF>().ToArray();
        }

        private float VectorMult(float ax, float ay, float bx, float by) //векторное произведение
        {
            return ax * by - bx * ay;
        }

        private void LineEquation(PointF p1, PointF p2, ref float A, ref float B, ref float C)
        {
            A = p2.Y - p1.Y;
            B = p1.X - p2.X;
            C = -p1.X * (p2.Y - p1.Y) + p1.Y * (p2.X - p1.X);
        }

        //поиск точки пересечения
        private PointF? CrossingPoint(PointF p1, PointF p2, PointF p3, PointF p4)
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

                PointF pt = new PointF();
                double d = (a1 * b2 - b1 * a2);
                double dx = (-c1 * b2 + b1 * c2);
                double dy = (-a1 * c2 + c1 * a2);
                pt.X = (int)(dx / d);
                pt.Y = (int)(dy / d);
                return pt;
            }

            return null;
        }

    }

    
}
