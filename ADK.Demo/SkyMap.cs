using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo
{

    public class GridPoint : CrdsHorizontal
    {
        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }
        public GridPoint(int row, int column, double azimuth, double altitude) : base(azimuth, altitude)
        {
            RowIndex = row;
            ColumnIndex = column;
        }
    }

    public class CelestialGrid
    {
        private GridPoint[,] Nodes = null;

        public int Rows { get; private set; }
        public int Columns { get; private set; }

        public CelestialGrid(int rows, int columns)
        {
            Nodes = new GridPoint[rows, columns];
            Rows = rows;
            Columns = columns;
       
        }

        public GridPoint this[int row, int column]
        {
            get { return Nodes[row, column]; }
            set { Nodes[row, column] = value; }
        }

        public IEnumerable<GridPoint> Column(int columnNumber)
        {
            return Enumerable.Range(0, Nodes.GetLength(0))
                    .Select(x => Nodes[x, columnNumber]);
        }

        public IEnumerable<GridPoint> Row(int rowNumber)
        {
            return Enumerable.Range(0, Nodes.GetLength(1))
                    .Select(x => Nodes[rowNumber, x]);
        }


        public GridPoint ClosestTo(CrdsHorizontal hor)
        {
            return Nodes.Cast<GridPoint>()
                .OrderBy(p => Angle.Separation(p, hor))
                .First();
        }

    }

    public class SkyMap : ISkyMap
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public double ViewAngle { get; set; } = 90;
        public CrdsHorizontal Center { get; set; } = new CrdsHorizontal(0, 0);

        private double Rho = 0;

        // TODO: this is temp
        private CelestialGrid GridHorizontal = new CelestialGrid(19, 24);

        private CelestialGrid GridEquatorial = new CelestialGrid(19, 24);

        public SkyMap()
        {
            var geo = new CrdsGeographical(56.3333, 44);

            for (int i = 0; i < 19; i++)
            {
                for (int j = 0; j < 24; j++)
                {
                    double a = i * 10 - 90;
                    double A = j * 15;
                    GridHorizontal[i, j] = new GridPoint(i, j, A, a);

                    var hor = new CrdsEquatorial(A, a).ToHorizontal(geo, 17);

                    GridEquatorial[i, j] = new GridPoint(i, j, hor.Azimuth, hor.Altitude);
                }
            }
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
            g.SmoothingMode = SmoothingMode.AntiAlias;

            DrawGrid(g, GridEquatorial);

            g.DrawString(Center.ToString(), SystemFonts.DefaultFont, Brushes.Red, 10, 10);
        }

        // TODO: move to separate renderer
        private void DrawGrid(Graphics g, CelestialGrid grid)
        {
            Pen penGrid = new Pen(Color.Green, 1);
            penGrid.DashStyle = DashStyle.Dash;

            bool isAnyPoint = false;

            // Azimuths 
            for (int j = 0; j < 24; j++)
            {
                var segments = grid.Column(j).Skip(1).Take(17)
                    .Select(p => Angle.Separation(p, Center) < ViewAngle * 1.2 ? p : null)
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
                    
                    DrawGroupOfPoints(g, segment.Select(s => Projection(s)).ToArray(), penGrid);

                    isAnyPoint = true;
                }
            }

        
            // Altitude circles
            for (int i = 0; i < 18; i++)
            {
                var segments = grid.Row(i)
                    .Select(p => Angle.Separation(p, Center) < ViewAngle * 1.2 ? p : null)
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
                        g.DrawClosedCurve(Pens.Azure, segment.Select(s => Projection(s)).ToArray());
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


                        DrawGroupOfPoints(g, segment.Select(s => Projection(s)).ToArray(), penGrid);
                    }

                    isAnyPoint = true;
                }
            }

            // Special case: there are no points visible 
            // on the screen at the current position and zoom.
            // Then we select one point that is closest to screen senter. 
            if (!isAnyPoint)
            {
                GridPoint closestPoint = grid.ClosestTo(Center);

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

                    DrawGroupOfPoints(g, segment.Select(s => Projection(s)).ToArray(), penGrid);
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

                    DrawGroupOfPoints(g, segment.Select(s => Projection(s)).ToArray(), penGrid);
                }
            }
        }

        private void DrawGroupOfPoints(Graphics g, PointF[] points, Pen penGrid)
        {
            try
            {            
                var origin = new PointF(Width / 2, Height / 2);

                if (points.Length == 2)
                {
                    g.DrawLine(Pens.Blue, points[0], points[1]);
                    return;
                }

                

                if (points.Length == 5)
                {
                    double alpha = AngleBetweenVectors(points[2], points[1], points[3]);

                    if (alpha > 179)
                    {
                        g.DrawLine(Pens.Violet, points[0], points[1]);
                        g.DrawLine(Pens.Violet, points[1], points[2]);
                        g.DrawLine(Pens.Violet, points[2], points[3]);
                        g.DrawLine(Pens.Violet, points[3], points[4]);
                        return;
                    }


                    if (IsOutOfScreen(points[0]) && IsOutOfScreen(points[4]) &&
                        DistanceBetweenPoints(points[0], origin) > Width * 2 &&
                        DistanceBetweenPoints(points[4], origin) > Width * 2)
                    {
                        double r = Math.Sqrt(Width * Width + Height * Height) / 2;

                        var bigCircle = FindCircle(points);
                        var smallCircle = new Circle() { X = origin.X, Y = origin.Y, R = r };

                        if (bigCircle.R / smallCircle.R > 60)
                        {
                            var cross = CirclesIntersection(bigCircle, smallCircle);

                            g.DrawLine(Pens.Red, cross[0], cross[1]);
                            
                           
                            Console.WriteLine($"cross[0]={cross[0]}, cross[1] = {cross[1]}");

                            return;
                        }
                    }
                    else
                    {
                        g.DrawCurve(Pens.Brown, points);
                    }
                }

                if (points.Length == 3)
                {
                    double alpha = AngleBetweenVectors(points[1], points[0], points[2]);

                    if (alpha > 179)
                    {
                        g.DrawLine(Pens.Orange, points[0], points[1]);
                        g.DrawLine(Pens.Orange, points[1], points[2]);
                        return;
                    }

                    if (IsOutOfScreen(points[0]) && IsOutOfScreen(points[2]) &&
                        DistanceBetweenPoints(points[0], origin) > Width * 2 &&
                        DistanceBetweenPoints(points[2], origin) > Width * 2)
                    {

                        double r = Math.Sqrt(Width * Width + Height * Height) / 2;

                        var bigCircle = FindCircle(points);
                        var smallCircle = new Circle() { X = Width / 2, Y = Height / 2, R = r };


                        if (bigCircle.R / smallCircle.R > 60)
                        {
                            var cross = CirclesIntersection(bigCircle, smallCircle);
                            g.DrawLine(Pens.Yellow, cross[0], cross[1]);

                            Console.WriteLine($"p1={ProjectionInv(points[1])}, p2={ProjectionInv(points[2])}");


                            //Console.WriteLine($"cross[0]={cross[0]}, cross[1] = {cross[1]}");

                            return;
                        }
                    }
                    else
                    {
                        g.DrawCurve(Pens.Coral, points);
                    }
                }

                g.DrawCurve(penGrid, points);
            }
            catch (Exception ex)
            {


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

        private PointF? EdgeCrosspoint(PointF p1, PointF p2, int width, int height)
        {
            PointF p00 = new PointF(0, 0);
            PointF pW0 = new PointF(Width, 0);
            PointF pWH = new PointF(Width, Height);
            PointF p0H = new PointF(0, Height);

            List<PointF> crossPoints = new List<PointF>();

            PointF? pCross = null;

            // top edge
            pCross = CrossingPoint(p1, p2, p00, pW0);
            if (pCross != null)
                crossPoints.Add(pCross.Value);

            // right edge
            pCross = CrossingPoint(p1, p2, pW0, pWH);
            if (pCross != null)
                crossPoints.Add(pCross.Value);

            // bottom edge
            pCross = CrossingPoint(p1, p2, pWH, p0H);
            if (pCross != null)
                crossPoints.Add(pCross.Value);

            // left edge
            pCross = CrossingPoint(p1, p2, p0H, p00);
            if (pCross != null)
                crossPoints.Add(pCross.Value);

            if (crossPoints.Any())
                return crossPoints.OrderByDescending(p => DistanceBetweenPoints(p1, p)).First();
            else
                return null;
        }

        //private IEnumerable<PointF> EdgeCrosspoints(PointF p1, PointF p2, int width, int height)
        //{
        //    PointF p00 = new PointF(0, 0);
        //    PointF pW0 = new PointF(Width, 0);
        //    PointF pWH = new PointF(Width, Height);
        //    PointF p0H = new PointF(0, Height);

        //    List<PointF?> crossPoints = new List<PointF?>();

        //    // top edge
        //    crossPoints.Add(CrossingPoint(p1, p2, p00, pW0));
        //    if (crossPoints.Any())

        //    // right edge
        //    crossPoints.Add(CrossingPoint(p1, p2, pW0, pWH));

        //    // bottom edge
        //    crossPoints.Add(CrossingPoint(p1, p2, pWH, p0H));

        //    // left edge
        //    crossPoints.Add(CrossingPoint(p1, p2, p0H, p00));

        //    return crossPoints.Where(p => p != null).Cast<PointF>();
        //}

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

        PointEqualityComparer pointEqComparer = new PointEqualityComparer();


        private IEnumerable<PointF> Straighten(IEnumerable<PointF> points)
        {
            PointF prev = points.First();
            foreach (var p in points)
            {
                if (!(Math.Abs(p.X - prev.X) < 10 && Math.Abs(p.Y - prev.Y) < 10))
                {
                    prev = p;
                    yield return p;
                }
                else
                {
                    prev = p;
                }
            }
            yield break;
        }

        private class PointEqualityComparer : IEqualityComparer<PointF>
        {
            public bool Equals(PointF p1, PointF p2)
            {
                return Math.Abs(p1.X - p2.X) < 10 && Math.Abs(p1.Y - p2.Y) < 10;
            }

            public int GetHashCode(PointF obj)
            {
                return obj.GetHashCode();
            }
        }
    }

    
}
