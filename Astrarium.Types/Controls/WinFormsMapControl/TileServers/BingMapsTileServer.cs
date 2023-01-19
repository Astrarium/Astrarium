using System.Text;

namespace System.Windows.Forms
{
    /// <summary>
    /// Base class for Bing Maps tile providers
    /// </summary>
    public abstract class BingMapsTileServer : WebTileServer
    {
        /// <summary>
        /// Not used here
        /// </summary>
        public override string UserAgent { get; set; }

        /// <summary>
        /// Gets tile layer type: a = aerial, r = roads, h = hybrid
        /// </summary>
        protected abstract string Layer { get; }

        /// <inheritdoc />
        public override string AttributionText => "© <a href='https://www.bing.com/maps/'>Bing Maps</a>";

        /// <inheritdoc />
        public override int MinZoomLevel => 1;

        /// <summary>
        /// Used to access random tile subdomains.
        /// </summary>
        private readonly Random _Random = new Random();

        /// <inheritdoc />
        public override Uri GetTileUri(int x, int y, int z)
        {
            return new Uri($"https://ecn.t{_Random.Next(8)}.tiles.virtualearth.net/tiles/{Layer}{TileXYToQuadKey(x, y, z)}.jpeg?g=587");
        }

        /// <summary>
        /// Converts x,y,z to QuadKey string used by MS maps.
        /// </summary>
        /// <param name="x">X-index of the tile.</param>
        /// <param name="y">Y-index of the tile.</param>
        /// <param name="z">Zoom level.</param>
        /// <returns>QuadKey string</returns>
        /// <remarks>See details: <see href="https://docs.microsoft.com/en-us/bingmaps/articles/bing-maps-tile-system"/></remarks> 
        private string TileXYToQuadKey(int x, int y, int z)
        {
            var quadKey = new StringBuilder();
            for (int i = z; i > 0; i--)
            {
                char digit = '0';
                int mask = 1 << (i - 1);
                if ((x & mask) != 0)
                {
                    digit++;
                }
                if ((y & mask) != 0)
                {
                    digit++;
                    digit++;
                }
                quadKey.Append(digit);
            }
            return quadKey.ToString();
        }
    }

    /// <summary>
    /// Represents Bing Maps Aerial web tile server.
    /// </summary>
    public class BingMapsAerialTileServer : BingMapsTileServer
    {
        /// <inheritdoc />
        public override string Name => "Bing Maps (Aerial)";

        /// <inheritdoc />
        protected override string Layer => "a";
    }

    /// <summary>
    /// Represents Bing Maps Roads web tile server.
    /// </summary>
    public class BingMapsRoadsTileServer : BingMapsTileServer
    {
        /// <inheritdoc />
        public override string Name => "Bing Maps (Roads)";

        /// <inheritdoc />
        protected override string Layer => "r";
    }


    /// <summary>
    /// Represents Bing Maps Hybrid web tile server.
    /// </summary>
    public class BingMapsHybridTileServer : BingMapsTileServer
    {
        /// <inheritdoc />
        public override string Name => "Bing Maps (Hybrid)";

        /// <inheritdoc />
        protected override string Layer => "h";
    }
}
