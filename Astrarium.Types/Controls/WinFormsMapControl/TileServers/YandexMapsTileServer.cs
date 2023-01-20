namespace System.Windows.Forms
{
    public abstract class YandexMapsTileServer : WebTileServer
    {
        /// <summary>
        /// Gets attribution text.
        /// </summary>
        public override string AttributionText => "© <a href='http://yandex.ru'>Yandex</a>";

        /// <summary>
        /// User-Agent string used to dowload tile images from the tile server.
        /// </summary>
        /// <remarks>
        /// OpenStreetMap requires valid HTTP User-Agent identifying application.
        /// Faking app's User-Agent may get you blocked.
        /// </remarks>
        public override string UserAgent { get; set; }

        /// <summary>
        /// Gets projection used by tile server.
        /// </summary>
        public override IProjection Projection => WGS84MercatorProjection.Instance;

        /// <summary>
        /// Creates new instance of <see cref="YandexMapsTileServer"/>.
        /// </summary>
        public YandexMapsTileServer(string userAgent)
        {
            UserAgent = userAgent;
        }
    }

    public class YandexSatelliteMapsTileServer : YandexMapsTileServer
    {
        public override string Name => "Yandex Satellite";

        /// <summary>
        /// Gets tile URI by X and Y indices of the tile and zoom level Z.
        /// </summary>
        /// <param name="x">X-index of the tile.</param>
        /// <param name="y">Y-index of the tile.</param>
        /// <param name="z">Zoom level.</param>
        /// <returns><see cref="Uri"/> instance.</returns>
        public override Uri GetTileUri(int x, int y, int z)
        {
            return new Uri($"https://core-sat.maps.yandex.net/tiles?l=sat&x={x}&y={y}&z={z}");
        }

        public YandexSatelliteMapsTileServer(string userAgent) : base(userAgent) { }
    }

    public class YandexRoadMapsTileServer : YandexMapsTileServer
    {
        public override string Name => "Yandex Road Maps";

        /// <summary>
        /// Gets tile URI by X and Y indices of the tile and zoom level Z.
        /// </summary>
        /// <param name="x">X-index of the tile.</param>
        /// <param name="y">Y-index of the tile.</param>
        /// <param name="z">Zoom level.</param>
        /// <returns><see cref="Uri"/> instance.</returns>
        public override Uri GetTileUri(int x, int y, int z)
        {
            return new Uri($"https://core-renderer-tiles.maps.yandex.net/tiles?l=map&x={x}&y={y}&z={z}");
        }

        public YandexRoadMapsTileServer(string userAgent) : base(userAgent) { }
    }

    public class YandexHybridTileServer : YandexMapsTileServer
    {
        public override string Name => "Yandex Hybrid";

        public override int LayersCount => 2;

        // this is not used
        public override Uri GetTileUri(int x, int y, int z)
        {
            throw new NotImplementedException();
        }

        public override Uri GetTileUri(int x, int y, int z, int layer)
        {
            if (layer == 0)
                return new Uri($"https://core-sat.maps.yandex.net/tiles?l=sat&x={x}&y={y}&z={z}");
            else
                return new Uri($"https://core-renderer-tiles.maps.yandex.net/tiles?l=skl&x={x}&y={y}&z={z}");
        }

        public YandexHybridTileServer(string userAgent) : base(userAgent) { }
    }
}
