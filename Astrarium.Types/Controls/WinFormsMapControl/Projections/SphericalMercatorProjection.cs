using System.Drawing;

namespace System.Windows.Forms
{
    /// <summary>
    /// Defines spherical Mercator projection used by most maps, like OpenStreetMap, Bing, Google
    /// </summary>
    public class SphericalMercatorProjection : IProjection
    {
        /// <summary>
        /// Lazy initializer for singleton projection instance.
        /// </summary>
        private static readonly Lazy<IProjection> instance = new Lazy<IProjection>(() => new SphericalMercatorProjection());

        /// <summary>
        /// Gets singleton instance of the projection.
        /// </summary>
        public static IProjection Instance => instance.Value;

        /// <inheritdoc />
        public GeoPoint TileToWorldPos(double x, double y, int z)
        {
            GeoPoint g = new GeoPoint();
            double z1 = 1 << z;
            double n = Math.PI - (2 * Math.PI * y / z1);
            g.Longitude = (float)((x / z1 * 360.0) - 180.0);
            g.Latitude = (float)(180.0 / Math.PI * Math.Atan(Math.Sinh(n)));
            return g;
        }

        /// <inheritdoc />
        public PointF WorldToTilePos(GeoPoint g, int z)
        {
            var p = new PointF();
            double z1 = 1 << z;
            p.X = (float)((g.Longitude + 180.0) / 360.0 * z1);
            p.Y = (float)((1.0 - Math.Log(Math.Tan(g.Latitude * Math.PI / 180.0) + 1.0 / Math.Cos(g.Latitude * Math.PI / 180.0)) / Math.PI) / 2.0 * z1);
            return p;
        }
    }
}
