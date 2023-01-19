using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Forms
{
    public abstract class GoogleMapsTileServer : WebTileServer
    {
        /// <summary>
        /// Used to access random tile subdomains.
        /// </summary>
        private readonly Random _Random = new Random();

        /// <inheritdoc />
        public override string UserAgent { get; set; }

        protected abstract string LayerType { get; }

        public override string AttributionText => "© <a href='https://www.google.com/maps'>Google Maps</a>";

        public override Uri GetTileUri(int x, int y, int z)
        {
            int ind = _Random.Next(0, 4);
            return new Uri($"http://mt{ind}.google.com/vt/lyrs={LayerType}&hl=en&x={x}&y={y}&z={z}");
        }

        public GoogleMapsTileServer(string userAgent)
        {
            UserAgent = userAgent;
        }
    }

    public class GoogleMapsHybridTileServer : GoogleMapsTileServer
    {
        public override string Name => "Google Hybrid";
        protected override string LayerType => "y";
        public GoogleMapsHybridTileServer(string userAgent) : base(userAgent) { }
    }

    public class GoogleMapsSatelliteTileServer : GoogleMapsTileServer
    {
        public override string Name => "Google Satellite";
        protected override string LayerType => "s";
        public GoogleMapsSatelliteTileServer(string userAgent) : base(userAgent) { }
    }

    public class GoogleMapsRoadmapTileServer : GoogleMapsTileServer
    {
        public override string Name => "Google Roadmap";
        protected override string LayerType => "m";
        public GoogleMapsRoadmapTileServer(string userAgent) : base(userAgent) { }
    }
}
