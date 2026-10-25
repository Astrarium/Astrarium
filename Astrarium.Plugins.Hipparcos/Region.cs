using Astrarium.Algorithms;
using System;

namespace Astrarium.Plugins.Hipparcos
{
    public class Region
    {
        private double LatMin;
        private double LatMax;
        private double LonMin;
        private double LonMax;

        public CrdsEquatorial Center { get; private set; } = new CrdsEquatorial();

        public int FirstStarIndex { get; private set; }
        public int StarsCount { get; private set; }

        public float Radius { get; private set; }

        public Region(int firstStarIndex, int starsCount, double latMin, double latMax, double lonMin, double lonMax)
        {
            FirstStarIndex  = firstStarIndex;
            StarsCount = starsCount;

            LatMin = latMin;
            LatMax = latMax;
            LonMin = lonMin;
            LonMax = lonMax;

            double sinLatMin = Math.Sin(Angle.ToRadians(LatMin));
            double sinLatMax = Math.Sin(Angle.ToRadians(LatMax));
            Center.Alpha = (LonMin + LonMax) * 0.5;
            Center.Delta = Angle.ToDegrees(Math.Asin((sinLatMin + sinLatMax) * 0.5));

            Radius = RegionRadiusDeg();
        }

        private float RegionRadiusDeg()
        {
            double latCRad = Center.Delta * Math.PI / 180.0;
            double lonCRad = Center.Alpha * Math.PI / 180.0;

            double maxAngle = 0.0;
            var corners = new (double lat, double lon)[]
            {
                (LatMin, LonMin), (LatMin, LonMax),
                (LatMax, LonMin), (LatMax, LonMax)
            };

            foreach (var (lat, lon) in corners)
            {
                double latRad = lat * Math.PI / 180.0;
                double lonRad = lon * Math.PI / 180.0;

                double cosAngle = Math.Sin(latCRad) * Math.Sin(latRad)
                                + Math.Cos(latCRad) * Math.Cos(latRad)
                                  * Math.Cos(lonRad - lonCRad);

                cosAngle = Math.Max(-1.0, Math.Min(1.0, cosAngle));
                double angle = Math.Acos(cosAngle) * 180.0 / Math.PI;
                if (angle > maxAngle) maxAngle = angle;
            }

            return (float)maxAngle;
        }
    }
}
