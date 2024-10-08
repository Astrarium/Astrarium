﻿using System.Drawing;

namespace System.Windows.Forms
{
    /// <summary>
    /// Defines map layer.
    /// </summary>
    public class Layer
    {
        /// <summary>
        /// Layers with highest ZIndex rendered above layers with lowest indices.
        /// </summary>
        public int ZIndex { get; set; }

        /// <summary>
        /// Opacity of the map layer.
        /// </summary>
        public float Opacity { get; set; } = 1;

        /// <summary>
        /// Tile server instance. If set is null, the layer will not be rendered.
        /// </summary>
        public ITileServer TileServer { get; set; }

        /// <summary>
        /// Defines the X-offset of the first tile of the layer.
        /// </summary>
        internal int OffsetX { get; set; }

        /// <summary>
        /// Defines the Y-offset of the first tile of the layer.
        /// </summary>
        internal int OffsetY { get; set; }
    }
}
