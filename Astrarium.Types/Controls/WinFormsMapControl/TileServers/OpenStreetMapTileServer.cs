namespace System.Windows.Forms
{
    /// <summary>
    /// Represents <see href="https://www.openstreetmap.org/">OpenStreetMap</see> web tile server.
    /// </summary>
    public class OpenStreetMapTileServer : WebTileServer
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
        public override string Name => "OpenStreetMap";

        /// <summary>
        /// Gets attribution text.
        /// </summary>
        public override string AttributionText => "© <a href='https://www.openstreetmap.org/copyright'>OpenStreetMap</a> contributors";

        /// <summary>
        /// User-Agent string used to dowload tile images from the tile server.
        /// </summary>
        /// <remarks>
        /// OpenStreetMap requires valid HTTP User-Agent identifying application.
        /// Faking app's User-Agent may get you blocked.
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
            return new Uri($"https://{server}.tile.openstreetmap.org/{z}/{x}/{y}.png");
        }

        /// <summary>
        /// Creates new instance of <see cref="OpenStreetMapTileServer"/>.
        /// </summary>
        /// <param name="userAgent">User-Agent string used to dowload tile images from OpenStreetMap tile servers.</param>
        /// <remarks>
        /// OpenStreetMap usage policy requires valid HTTP User-Agent identifying application. 
        /// Faking another app’s User-Agent WILL get you blocked
        /// </remarks>
        public OpenStreetMapTileServer(string userAgent)
        {
            UserAgent = userAgent;
        }
    }
}
