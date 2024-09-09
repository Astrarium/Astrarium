using System.Drawing;

namespace System.Windows.Forms
{
    /// <summary>
    /// Defines Mercator map projection onto referenced onto the referenced ellipsoid WGS84.
    /// Used by some maps, like Yandex maps.
    /// </summary>
    public class WGS84MercatorProjection : IProjection
    {
        /// <summary>
        /// Lazy initializer for singleton projection instance.
        /// </summary>
        private static readonly Lazy<IProjection> instance = new Lazy<IProjection>(() => new WGS84MercatorProjection());

        /// <summary>
        /// Gets singleton instance of the projection.
        /// </summary>
        public static IProjection Instance => instance.Value;

        /// <inheritdoc />
        public GeoPoint TileToWorldPos(double x, double y, int z)
        {
            x *= 256;
            y *= 256;

            const double a = 6378137;
            const double c1 = 0.00335655146887969;
            const double c2 = 0.00000657187271079536;
            const double c3 = 0.00000001764564338702;
            const double c4 = 0.00000000005328478445;
            double z1 = 1 << (23 - z);

            double mercX = x * z1 / 53.5865938 - 20037508.342789;
            double mercY = 20037508.342789 - y * z1 / 53.5865938;

            double G = Math.PI / 2 - 2 * Math.Atan(1 / Math.Exp(mercY / a));
            double Z = G + c1 * Math.Sin(2 * G) + c2 * Math.Sin(4 * G) + c3 * Math.Sin(6 * G) + c4 * Math.Sin(8 * G);

            GeoPoint g = new GeoPoint();
            g.Latitude = (float)(Z * (180.0 / Math.PI));
            g.Longitude = (float)(mercX / a * (180.0 / Math.PI));
            return g;
        }

        /// <inheritdoc />
        public PointF WorldToTilePos(GeoPoint g, int zoomLevel)
        {
            double lon = g.Longitude * Math.PI / 180.0;
            double lat = g.Latitude * Math.PI / 180.0;

            const double a = 6378137;
            const double k = 0.0818191908426;

            double z = Math.Tan(Math.PI / 4 + lat / 2) / Math.Pow(Math.Tan(Math.PI / 4 + Math.Asin(k * Math.Sin(lat)) / 2), k);
            double z1 = 1 << (23 - zoomLevel);

            double dx = (20037508.342789 + a * lon) * 53.5865938 / z1;
            double dy = (20037508.342789 - a * Math.Log(z)) * 53.5865938 / z1;

            PointF p = new PointF();
            p.X = (float)(dx / 256);
            p.Y = (float)(dy / 256);

            return p;
        }
    }
}
