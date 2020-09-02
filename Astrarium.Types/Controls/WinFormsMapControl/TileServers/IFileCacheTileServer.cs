namespace System.Windows.Forms
{
    /// <summary>
    /// Defines interface for a tile server that supports file system caching of tiles.
    /// </summary>
    public interface IFileCacheTileServer : ITileServer
    {
        /// <summary>
        /// Gets tile validity period of a tile image. 
        /// Tile will be requested again from the tile server 
        /// if the tile's image file from the file system cache is older than specified value. 
        /// </summary>
        TimeSpan TileExpirationPeriod { get; set; }
    }
}
