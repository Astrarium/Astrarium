namespace System.Windows.Forms
{
    /// <summary>
    /// Represents <see href="https://maps.2gis.com/">2GIS</see> web tile server.
    /// </summary>
    public class DoubleGisTileServer : WebTileServer
    {
        /// <summary>
        /// Used to access random tile subdomains.
        /// </summary>
        private readonly Random _Random = new Random();

        /// <summary>
        /// Gets displayable name of the Tile server.
        /// </summary>
        public override string Name => "2GIS";

        /// <summary>
        /// Gets attribution text.
        /// </summary>
        public override string AttributionText => "© <a href='https://maps.2gis.com'>2GIS</a>";

        /// <summary>
        /// Minimal zoom level.
        /// </summary>
        public override int MinZoomLevel => 1;

        /// <summary>
        /// Maximal zoom level.
        /// </summary>
        public override int MaxZoomLevel => 8;

        /// <summary>
        /// User-Agent string used to dowload tile images from the tile server.
        /// </summary>
        /// <remarks>
        /// </remarks>
        public override string UserAgent { get; set; }

        /// <summary>
        /// Gets tile URI by X and Y indices of the tile and zoom level Z.
        /// </summary>
        /// <param name="x">X-index of the tile.</param>
        /// <param name="y">Y-index of the tile.</param>
        /// <param name="z">Zoom level.</param>
        /// <returns><see cref="Uri"/> instance.</returns>
        public override Uri GetTileUri(int x, int y, int z)
        {
            int server = _Random.Next(0, 5);
            return new Uri($"https://tile{server}.maps.2gis.com/tiles?x={x}&y={y}&z={z}");
        }

        /// <summary>
        /// Creates new instance of <see cref="DoubleGisTileServer"/>.
        /// </summary>
        /// <param name="userAgent">User-Agent string used to dowload tile images from 2GIS tile servers.</param>
        public DoubleGisTileServer(string userAgent)
        {
            UserAgent = userAgent;
        }
    }
}
