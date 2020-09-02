namespace System.Windows.Forms
{
    /// <summary>
    /// Represents <see href="https://opentopomap.org/">OpenTopoMap</see> web tile server.
    /// </summary>
    public class OpenTopoMapServer : WebTileServer
    {
        /// <summary>
        /// Used to access random tile subdomains.
        /// </summary>
        private readonly Random _Random = new Random();

        /// <summary>
        /// Tile server subdomains.
        /// </summary>
        private readonly string[] _Subdomains = new[] { "a", "b", "c" };

        /// <summary>
        /// Gets displayable name of the Tile server.
        /// </summary>
        public override string Name => "OpenTopoMap";

        /// <summary>
        /// Gets attribution text.
        /// </summary>
        public override string AttributionText => "Map data: © <a href='https://openstreetmap.org/copyright'>OpenStreetMap</a> contributors, <a href='http://viewfinderpanoramas.org'>SRTM</a> | map style: © <a href='https://opentopomap.org'>OpenTopoMap</a> (<a href='https://creativecommons.org/licenses/by-sa/3.0/'>CC-BY-SA</a>)";

        /// <summary>
        /// Minimal zoom level.
        /// </summary>
        public override int MinZoomLevel => 1;

        /// <summary>
        /// Maximal zoom level.
        /// </summary>
        public override int MaxZoomLevel => 17;

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
            string server = _Subdomains[_Random.Next(_Subdomains.Length)];
            return new Uri($"https://{server}.tile.opentopomap.org/{z}/{x}/{y}.png");
        }
    }
}
