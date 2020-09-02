namespace System.Windows.Forms
{
    /// <summary>
    /// Represents <see href="http://maps.stamen.com/terrain/">Stamen Terrain</see> web tile server. 
    /// </summary>
    public class StamenTerrainTileServer : WebTileServer
    {
        /// <summary>
        /// Used to access random tile subdomains.
        /// </summary>
        private readonly Random _Random = new Random();

        /// <summary>
        /// Tile server subdomains.
        /// </summary>
        private readonly string[] _Subdomains = new[] { "a", "b", "c", "d" };

        /// <summary>
        /// Gets displayable name of the Tile server.
        /// </summary>
        public override string Name => "Stamen Terrain";

        /// <summary>
        /// Gets attribution text.
        /// </summary>
        public override string AttributionText => "<a href='http://maps.stamen.com/'>Map tiles</a> by <a href='http://stamen.com'>Stamen Design</a>, under <a href='http://creativecommons.org/licenses/by/3.0'>CC BY 3.0</a>. Data © <a href='http://www.openstreetmap.org/copyright'>OpenStreetMap contributors</a>.";

        /// <summary>
        /// Maximal zoom level.
        /// </summary>
        public override int MaxZoomLevel => 13;

        /// <summary>
        /// User-Agent string used to dowload tile images from the tile server.
        /// </summary>
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
            return new Uri($"http://{server}.tile.stamen.com/terrain/{z}/{x}/{y}.png");
        }
    }
}
