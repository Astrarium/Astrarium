namespace System.Windows.Forms
{
    /// <summary>
    /// Base class for all light pollition maps tile servers
    /// </summary>
    public abstract class LightPollutionTileServer : WebTileServer, IOverlayTileServer
    {
        /// <inheritdoc />
        public override string UserAgent { get; set; }

        /// <inheritdoc />
        public override string AttributionText => null;

        /// <summary>
        /// Id of light pollution layer
        /// </summary>
        protected abstract string LayerId { get; }

        /// <inheritdoc />
        public override Uri GetTileUri(int x, int y, int z)
        {
            return new Uri($"https://www.lightpollutionmap.info/geoserver/gwc/service/tms/1.0.0/{LayerId}@EPSG:900913@png/{z}/{x}/{y}.png?flipY=true");
        }

        /// <summary>
        /// Creates new instance of the tile server
        /// </summary>
        /// <param name="userAgent"></param>
        public LightPollutionTileServer(string userAgent)
        {
            UserAgent = userAgent;
        }

        /// <inheritdoc />
        public override string ToString() => Name;
    }

    public class LightPollutionWA2015TileServer : LightPollutionTileServer
    {
        public override string Name => "WA 2015";
        protected override string LayerId => "PostGIS:WA_2015";
        public LightPollutionWA2015TileServer(string userAgent) : base(userAgent) { }
    }

    public class LightPollutionVIIRS2024TileServer : LightPollutionTileServer
    {
        public override string Name => "VIIRS 2024";
        protected override string LayerId => "PostGIS:VIIRS_2024";
        public LightPollutionVIIRS2024TileServer(string userAgent) : base(userAgent) { }
    }

    public class LightPollutionVIIRS2023TileServer : LightPollutionTileServer
    {
        public override string Name => "VIIRS 2023";
        protected override string LayerId => "PostGIS:VIIRS_2023";
        public LightPollutionVIIRS2023TileServer(string userAgent) : base(userAgent) { }
    }

    public class LightPollutionVIIRS2022TileServer : LightPollutionTileServer
    {
        public override string Name => "VIIRS 2022";
        protected override string LayerId => "PostGIS:VIIRS_2022";
        public LightPollutionVIIRS2022TileServer(string userAgent) : base(userAgent) { }
    }

    public class LightPollutionVIIRS2021TileServer : LightPollutionTileServer
    {
        public override string Name => "VIIRS 2021";
        protected override string LayerId => "PostGIS:VIIRS_2021";
        public LightPollutionVIIRS2021TileServer(string userAgent) : base(userAgent) { }
    }

    public class LightPollutionVIIRS2020TileServer : LightPollutionTileServer
    {
        public override string Name => "VIIRS 2020";
        protected override string LayerId => "PostGIS:VIIRS_2020";
        public LightPollutionVIIRS2020TileServer(string userAgent) : base(userAgent) { }
    }

    public class LightPollutionVIIRS2019TileServer : LightPollutionTileServer
    {
        public override string Name => "VIIRS 2019";
        protected override string LayerId => "PostGIS:VIIRS_2019";
        public LightPollutionVIIRS2019TileServer(string userAgent) : base(userAgent) { }
    }

    public class LightPollutionVIIRS2018TileServer : LightPollutionTileServer
    {
        public override string Name => "VIIRS 2018";
        protected override string LayerId => "PostGIS:VIIRS_2018";
        public LightPollutionVIIRS2018TileServer(string userAgent) : base(userAgent) { }
    }

    public class LightPollutionVIIRS2017TileServer : LightPollutionTileServer
    {
        public override string Name => "VIIRS 2017";
        protected override string LayerId => "PostGIS:VIIRS_2017";
        public LightPollutionVIIRS2017TileServer(string userAgent) : base(userAgent) { }
    }

    public class LightPollutionVIIRS2016TileServer : LightPollutionTileServer
    {
        public override string Name => "VIIRS 2016";
        protected override string LayerId => "PostGIS:VIIRS_2016";
        public LightPollutionVIIRS2016TileServer(string userAgent) : base(userAgent) { }
    }

    public class LightPollutionVIIRS2015TileServer : LightPollutionTileServer
    {
        public override string Name => "VIIRS 2015";
        protected override string LayerId => "PostGIS:VIIRS_2015";
        public LightPollutionVIIRS2015TileServer(string userAgent) : base(userAgent) { }
    }

    public class LightPollutionVIIRS2014TileServer : LightPollutionTileServer
    {
        public override string Name => "VIIRS 2014";
        protected override string LayerId => "PostGIS:VIIRS_2014";
        public LightPollutionVIIRS2014TileServer(string userAgent) : base(userAgent) { }
    }

    public class LightPollutionVIIRS2013TileServer : LightPollutionTileServer
    {
        public override string Name => "VIIRS 2013";
        protected override string LayerId => "PostGIS:VIIRS_2013";
        public LightPollutionVIIRS2013TileServer(string userAgent) : base(userAgent) { }
    }

    public class LightPollutionVIIRS2012TileServer : LightPollutionTileServer
    {
        public override string Name => "VIIRS 2012";
        protected override string LayerId => "PostGIS:VIIRS_2012";
        public LightPollutionVIIRS2012TileServer(string userAgent) : base(userAgent) { }
    }
}
