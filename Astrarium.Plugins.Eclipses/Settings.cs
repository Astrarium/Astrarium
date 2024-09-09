using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Eclipses
{
    /// <summary>
    /// Holds settings names for the plugin
    /// </summary>
    public static class Settings
    {
        /// <summary>
        /// Name of the setting to store map's tile server
        /// </summary>
        public const string EclipseMapTileServer = "EclipseMapTileServer";

        /// <summary>
        /// Name of the setting to store map's overlay tile server
        /// </summary>
        public const string EclipseMapOverlayTileServer = "EclipseMapOverlayTileServer";

        /// <summary>
        /// Name of the setting to store map's overlay opacity
        /// </summary>
        public const string EclipseMapOverlayOpacity = "EclipseMapOverlayOpacity";
    }
}
