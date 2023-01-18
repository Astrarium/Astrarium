using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Forms
{
    public interface IOverlayTileServer { }

    public class LightPollutionTileServer : WebTileServer, IOverlayTileServer
    {
        public override string UserAgent { get; set; }

        public override string Name => "Light Pollution World Atlas 2005";

        public override string AttributionText => "(c)";

        protected string LayerId => "PostGIS:WA_2015";

        public LightPollutionTileServer(string userAgent)
        {
            UserAgent = userAgent;
        }

        public override Uri GetTileUri(int x, int y, int z)
        {
            return new Uri($"https://www.lightpollutionmap.info/geoserver/gwc/service/tms/1.0.0/{LayerId}@EPSG:900913@png/{z}/{x}/{y}.png?flipY=true");
        }
    }
}
