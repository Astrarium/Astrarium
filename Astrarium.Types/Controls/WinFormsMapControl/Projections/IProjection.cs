using System.Drawing;

namespace System.Windows.Forms
{
    /// <summary>
    /// Defines cartographical projection
    /// </summary>
    public interface IProjection
    {
        /// <summary>
        /// Converts tile indices to geographical coordinates.
        /// </summary>
        /// <param name="x">X-index of the tile.</param>
        /// <param name="y">Y-index of the tile.</param>
        /// <param name="z">Zoom level.</param>
        /// <returns>Point representing geographical coordinates.</returns>
        GeoPoint TileToWorldPos(double x, double y, int z);

        /// <summary>
        /// Converts geographical coordinates to tile indices with fractions.
        /// </summary>
        /// <param name="g">Point with geographical coordinates.</param>
        /// <param name="z">Zoom level.</param>
        /// <returns>Point representing X/Y indices of the specified geographical coordinates in Slippy map scheme.</returns>
        PointF WorldToTilePos(GeoPoint g, int z);
    }
}
