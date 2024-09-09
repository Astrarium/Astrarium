using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Forms
{
    public class EsriSatelliteMapsTileServer : WebTileServer
    {
        /// <inheritdoc />
        public override string UserAgent { get; set; }

        public override string AttributionText => "© <a href='https://www.arcgis.com/'>ESRI Maps</a>";

        public override string Name => "Esri Imagery";

        public override Uri GetTileUri(int x, int y, int z)
        {
            return new Uri($"https://server.arcgisonline.com/arcgis/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}");
        }

        public EsriSatelliteMapsTileServer(string userAgent)
        {
            UserAgent = userAgent;
        }
    }
}
