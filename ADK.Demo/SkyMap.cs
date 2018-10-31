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
            for (int i = 0, a = 90; i < 19; ++i, a -= 10)
            {
                for (int j = 0, A = 0; j < 25; ++j, A += 15)
                {
                    GridHorizontal[i, j] = new CrdsHorizontal(A, a);
                }
            }
        }

        public Point Projection(CrdsHorizontal hor)
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
            Point pLeftEdge = Projection(new CrdsHorizontal() { Azimuth = Center.Azimuth - 90, Altitude = hor.Altitude });
            Point pRightEdge = Projection(new CrdsHorizontal() { Azimuth = Center.Azimuth + 90, Altitude = hor.Altitude });

            Point pEdge;
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
                Point pole = Projection(new CrdsHorizontal() { Altitude = 90 * (Center.Altitude > 0 ? 1 : -1), Azimuth = 0 });

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

                Point pCorrected = new Point(0, 0);

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

            // Azimuths 
            for (int j = 0; j < 24; ++j)
            {
                var col = GridHorizontal.GetColumn(j).Skip(1).Take(17);

                var groups = col
                    .Select(h => Angle.Separation(h, Center) < ViewAngle * 1.2 ? h : null)
                    .Select(h => h != null ? Projection(h) : (Point?)null)                        
                    .Split(p => p == null, true)
                    .ToArray();

                if (groups.Any())
                {
                    foreach (var group in groups)
                    {
                        var points = group.Select(p => (Point)p).ToArray();

                        if (points.Length > 1)
                        {
                            g.DrawCurve(penGrid, points);
                        }
                    }
                }
            }

            // Altitudes
            for (int i = 1; i < 19; ++i)
            {
                var row = GridHorizontal.GetRow(i);

                var groups = row
                    .Select(h => Angle.Separation(h, Center) < ViewAngle * 1.2 ? h : null)
                    .Select(h => h != null ? Projection(h) : (Point?)null)
                    .Split(p => p == null, true)
                    .ToArray();

                if (groups.Any())
                {
                    foreach (var group in groups)
                    {
                        var points = group.Select(p => (Point)p).ToArray();

                        if (points.Length > 1)
                        {
                            g.DrawCurve(penGrid, points);
                        }
                    }
                }
            }
        }
    }
}
