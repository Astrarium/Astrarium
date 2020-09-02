using System.Drawing;

namespace System.Windows.Forms
{
    /// <summary>
    /// Provides the functionality of a Tile Server implementations
    /// </summary>
    public interface ITileServer
    {
        /// <summary>
        /// Displayable name of the tile server, i.e. human-readable map name, for example, "Open Street Map".
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Attribution text that will be displayed in bottom-right corner of the map.
        /// Can be null (no attribution text) or can contain html links for navigating with default system web browser.
        /// </summary>
        /// <example>© <a href='https://www.openstreetmap.org/copyright'>OpenStreetMap</a> contributors</example>
        string AttributionText { get; }

        /// <summary>
        /// Gets minimal zoom level allowed for the tile server
        /// </summary>
        int MinZoomLevel { get; }

        /// <summary>
        /// Gets maximal zoom level allowed for the tile server
        /// </summary>
        int MaxZoomLevel { get; }

        /// <summary>
        /// Requests tile image by X and Y indices of the tile and zoom level Z.
        /// </summary>
        /// <param name="x">X-index of the tile.</param>
        /// <param name="y">Y-index coordinate of the tile.</param>
        /// <param name="z">Zoom level</param>
        /// <returns>
        /// Tile image
        /// </returns>
        /// <remarks>
        /// See about tile indexing schema here: <see href="https://wiki.openstreetmap.org/wiki/Slippy_map_tilenames" />
        /// </remarks>
        Image GetTile(int x, int y, int z);
    }
}
