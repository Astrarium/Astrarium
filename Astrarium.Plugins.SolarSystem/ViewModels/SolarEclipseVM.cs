using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Astrarium.Plugins.SolarSystem
{
    public class SolarEclipseVM : ViewModelBase
    {
        public string CacheFolder { get; private set; }
        
        public ICollection<ITileServer> TileServers { get; private set; }

        public ITileServer TileServer
        {
            get => GetValue<ITileServer>(nameof(TileServer));
            set => SetValue(nameof(TileServer), value);
        }

        public SolarEclipseVM()
        {
            CacheFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Astrarium", "MapsCache");

            TileServers = new List<ITileServer>() 
            {
                new OfflineTileServer(),
                new OpenStreetMapTileServer("Astrarium v1.0 contact astrarium@astrarium.space"),
                new StamenTerrainTileServer(),
                new OpenTopoMapServer()
            };

            TileServer = TileServers.First();
        }
    }
}
