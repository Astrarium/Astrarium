using ADK;
using Planetarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Projections
{
    /// <summary>
    /// ARC projection, AIPS MEMO 27
    /// Zenith Equidistant Projection
    /// </summary>
    public class SinProjection : IProjection
    {
        private readonly IMapContext Map = null;

        public SinProjection(IMapContext map)
        {
            Map = map;
        }

        public PointF Project(CrdsHorizontal hor)
        {
            double X, Y;

            double d = Angle.ToRadians(hor.Altitude);
            double d0 = Angle.ToRadians(Map.Center.Altitude);
            double da = Angle.ToRadians(hor.Azimuth - Map.Center.Azimuth);

            double sin_da = Math.Sin(da);
            double cos_da = Math.Cos(da);

            double sin_d = Math.Sin(d);
            double cos_d = Math.Cos(d);

            double sin_d0 = Math.Sin(d0);
            double cos_d0 = Math.Cos(d0);

            X = cos_d * sin_da;
            Y = sin_d * cos_d0 - cos_d * sin_d0 * cos_da;

            double maxSize = Math.Max(Map.Width, Map.Height);
            X = X * 90 / Map.ViewAngle / 2 * maxSize;
            Y = Y * 90 / Map.ViewAngle / 2* maxSize;

            return new Point((int)(Map.Width / 2.0 + X), (int)(Map.Height / 2.0 - Y));
        }

        public CrdsHorizontal Invert(PointF p)
        {
            double maxSize = Math.Max(Map.Width, Map.Height);
            double L = Angle.ToRadians((p.X - Map.Width / 2.0) * Map.ViewAngle / maxSize * 2);
            double M = Angle.ToRadians((-p.Y + Map.Height / 2.0) * Map.ViewAngle / maxSize * 2);

            double theta = Math.Sqrt(L * L + M * M);

            if (theta == 0 || double.IsNaN(theta))
            {
                return new CrdsHorizontal(0, 0);
            }

            double a;
            double A;

            a = Angle.ToDegrees(Math.Asin(M * Math.Cos(Angle.ToRadians(Map.Center.Altitude)) / (theta / Math.Sin(theta)) + Math.Sin(Angle.ToRadians(Map.Center.Altitude)) * Math.Cos(theta)));
            A = Map.Center.Azimuth + Angle.ToDegrees(Math.Asin(Math.Sin(theta) * L / (theta * Math.Cos(Angle.ToRadians(a)))));
            A = Angle.To360(A);

            return CorrectInverse(p, new CrdsHorizontal() { Altitude = a, Azimuth = A });
        }

        /// <summary>
        /// Performs a correction of inverse projection. 
        /// Checks that horizontal coordinates of a point are correct, 
        /// and in case if not correct, applies iterative algorithm for searching correct values.
        /// </summary>
        /// <param name="p">Point to check</param>
        /// <param name="hor">Horizontal coordinates of the point</param>
        /// <returns>Corrected horizontal coordinates</returns>
        private CrdsHorizontal CorrectInverse(PointF p, CrdsHorizontal hor)
        {
            PointF pLeftEdge = Project(new CrdsHorizontal(Map.Center.Azimuth - 90, hor.Altitude));
            PointF pRightEdge = Project(new CrdsHorizontal(Map.Center.Azimuth + 90, hor.Altitude));

            PointF pEdge;
            if (p.X < Map.Width / 2.0)
            {
                pEdge = pLeftEdge;
            }
            else
            {
                pEdge = pRightEdge;
            }

            Point origin = new Point((int)(Map.Width / 2.0), (int)(Map.Height / 2.0));

            double edgeToCenter = Map.DistanceBetweenPoints(origin, pEdge);

            double currentToCenter = Map.DistanceBetweenPoints(origin, p);

            bool correctionNeeded = Math.Abs(Map.Center.Altitude) == 90 || currentToCenter > edgeToCenter;

            if (correctionNeeded)
            {
                // projected coordinates of a horizontal grid pole (zenith or nadir point)
                PointF pole = Project(new CrdsHorizontal(0, 90 * (Map.Center.Altitude > 0 ? 1 : -1)));

                double angleWhole = 360 - Map.AngleBetweenVectors(pole, pLeftEdge, pRightEdge);

                double angleLeft = Map.AngleBetweenVectors(pole, p, pLeftEdge);

                double angleRight = Map.AngleBetweenVectors(pole, p, pRightEdge);

                int shiftSign = angleLeft < angleRight ? -1 : 1;

                int poleFix = 1;
                if (Map.Center.Altitude == 90 && pole.Y < p.Y)
                {
                    poleFix = -1;
                }
                else if (Map.Center.Altitude == -90 && pole.Y > p.Y)
                {
                    poleFix = -1;
                }

                double poleAngle = Math.Min(angleLeft, angleRight);

                double azimuthShift = poleAngle / angleWhole * 180;

                PointF pCorrected = new PointF(0, 0);

                double distOriginal = Map.DistanceBetweenPoints(p, pEdge);
                double distCorrected = 0;

                int iterations = 0;

                do
                {
                    hor = new CrdsHorizontal(Angle.To360(Map.Center.Azimuth + shiftSign * 90 + poleFix * shiftSign * azimuthShift), hor.Altitude);

                    // corrected coordinates of a projected point
                    pCorrected = Project(hor);

                    distCorrected = Map.DistanceBetweenPoints(pCorrected, pEdge);

                    if (distCorrected > 0)
                    {
                        azimuthShift *= distOriginal / distCorrected;
                    }
                    iterations++;
                }
                while (Map.DistanceBetweenPoints(p, pCorrected) > 2 && iterations < 5);
            }

            return hor;
        }
    }
}
