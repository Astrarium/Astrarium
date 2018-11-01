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

        private double Rho = 0;

        // TODO: this is temp
        private CrdsHorizontal[,] GridHorizontal = new CrdsHorizontal[19, 25];

        public SkyMap()
        {
            //for (int i = 0, a = 90; i < 19; ++i, a -= 10)
            //{
            //    for (int j = 0, A = 0; j < 25; ++j, A += 15)
            //    {
            //        GridHorizontal[i, j] = new CrdsHorizontal(A, a);
            //    }
            //}

            for (int i = 0; i < 19; i++)
            {
                for (int j = 0; j < 25; j++)
                {
                    double a = i * 10 - 90;
                    double A = j * 15;
                    GridHorizontal[i, j] = new CrdsHorizontal(A, a);
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

            return Angle.ToDegrees(Math.Acos(ab / (moda * modb)));
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

            DrawGrid(g);

            g.DrawString(Center.ToString(), SystemFonts.DefaultFont, Brushes.Red, 10, 10);
        }

        // TODO: move to separate renderer
        private void DrawGrid(Graphics g)
        {
            Pen penGrid = new Pen(Color.Green, 1);
            penGrid.DashStyle = DashStyle.Dash;

            double screenDiag = Math.Sqrt(Width * Width + Height * Height);

            // Azimuths 
            for (int j = 0; j < 24; ++j)
            {
                var col = GridHorizontal.GetColumn(j).Skip(1).Take(17);
                DrawLine(g, penGrid, col);                
            }

            // Altitudes
            for (int i = 0; i < 19; i++)
            {
                var row = GridHorizontal.GetRow(i);
                DrawLine(g, penGrid, row);
            }
        }

        private void DrawLine(Graphics g, Pen penGrid, IEnumerable<CrdsHorizontal> col)
        {
            var segments = col
                    .Select(h => Angle.Separation(h, Center) <= 90 * 1.2 ? (PointF?)Projection(h) : null)
                    //.Select(p => p != null && !IsOutOfScreen(p.Value) ? p : null)
                    .Split(p => p == null, true);

            foreach (var segment in segments)
            {
                var points = segment.Cast<PointF>().ToArray();

                if (points.Length > 1)
                {
                    var checkPoints = points.OrderBy(p => DistanceBetweenPoints(p, new PointF(Width / 2, Height / 2))).Take(2).ToArray();

                    if (DistanceBetweenPoints(checkPoints[0], checkPoints[1]) < Width * 3)
                    {
                        GraphicsPath gp = new GraphicsPath();
                        gp.AddCurve(points);
                        gp.Flatten();
                        points = gp.PathPoints;

                        Console.WriteLine("Path points : " + points.Length);
                    }
                    else
                    {
                        continue;
                    }

                    var pp = new List<PointF?>();

                    for (int i = 0; i < points.Length; i++)
                    {
                        if (!IsOutOfScreen(points[i]))
                        {
                            pp.Add(points[i]);
                            continue;
                        }

                        if (i < points.Length - 1)
                        {
                            var pCross = EdgeCrosspoint(points[i + 1], points[i], Width, Height);
                            if (pCross != null)
                            {
                                pp.Add(pCross);
                                continue;
                            }
                        }

                        if (i > 0)
                        {
                            var pCross = EdgeCrosspoint(points[i - 1], points[i], Width, Height);
                            if (pCross != null)
                            {
                                pp.Add(pCross);
                                continue;
                            }
                        }
                

                        pp.Add(null);
                    }

                    var groups = pp.Split(p => p == null, true);

                    foreach (var group in groups)
                    {
                        var points2 = group.Cast<PointF>().ToArray();

                        if (points2.Length > 1)
                        {
                            g.DrawLines(penGrid, points2);
                        }
                    }
                }
            }

        }


        /// <summary>
        /// Checks if the point is out of screen bounds
        /// </summary>
        /// <param name="p">Point to check</param>
        /// <returns>True if out from screen, false otherwise</returns>
        private bool IsOutOfScreen(PointF p)
        {
            return p.Y < 0 || p.Y > Height || p.X < 0 || p.X > Width;
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

    }
}
