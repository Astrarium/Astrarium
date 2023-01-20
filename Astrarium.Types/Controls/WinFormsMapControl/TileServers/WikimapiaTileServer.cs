namespace System.Windows.Forms
{
    /// <summary>
    /// Represents <see href="https://wikimapia.org/">Wikimapia</see> web tile server.
    /// </summary>
    public class WikimapiaTileServer : WebTileServer
    {
        /// <summary>
        /// Used to access random tile subdomains.
        /// </summary>
        private readonly Random _Random = new Random();


        /// <summary>
        /// Gets displayable name of the Tile server.
        /// </summary>
        public override string Name => "Wikimapia";

        /// <summary>
        /// Gets attribution text.
        /// </summary>
        public override string AttributionText => "Map data: © <a href='https://wikimapia.org'>Wikimapia.org</a>";

        /// <summary>
        /// Minimal zoom level.
        /// </summary>
        public override int MinZoomLevel => 1;

        /// <summary>
        /// Maximal zoom level.
        /// </summary>
        public override int MaxZoomLevel => 19;

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
            int server = x % 4 + (y % 4) * 4;
            return new Uri($"http://i{server}.wikimapia.org/?x={x}&y={y}&zoom={z}&type=map&lng=0");
        }

        /// <summary>
        /// Creates new instance of <see cref="WikimapiaTileServer"/>.
        /// </summary>
        /// <param name="userAgent">User-Agent string used to dowload tile images from OpenTopoMapServer tile servers.</param>
        public WikimapiaTileServer(string userAgent)
        {
            UserAgent = userAgent;
        }
    }
}
