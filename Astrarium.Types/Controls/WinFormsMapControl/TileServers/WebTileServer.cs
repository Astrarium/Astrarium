using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;

namespace System.Windows.Forms
{
    /// <summary>
    /// Base class for all web tile servers
    /// </summary>
    public abstract class WebTileServer : IFileCacheTileServer
    {
        /// <summary>
        /// Gets tile URI by X and Y indices of the tile and zoom level Z.
        /// </summary>
        /// <param name="x">X-index of the tile.</param>
        /// <param name="y">Y-index of the tile.</param>
        /// <param name="z">Zoom level.</param>
        /// <returns><see cref="Uri"/> instance.</returns>
        public abstract Uri GetTileUri(int x, int y, int z);

        /// <summary>
        /// User-Agent string used to dowload tile images from the tile server.
        /// </summary>
        /// <remarks>
        /// Some web tile servers (for example OpenStreetMap) require valid HTTP User-Agent identifying application.
        /// Faking app's User-Agent may get you blocked.
        /// </remarks>
        public abstract string UserAgent { get; set; }

        /// <summary>
        /// Tile expiration period.
        /// </summary>
        /// <remarks>
        /// Different tile servers have various tile usage policies, so do not set small values here to prevent loading same tiles from the server frequently.
        /// For example, for OpenStretMap tile expiration period should not be smaller than 7 days: <see href="https://operations.osmfoundation.org/policies/tiles/"/>
        /// </remarks>
        public virtual TimeSpan TileExpirationPeriod { get; set; } = TimeSpan.FromDays(30);

        /// <summary>
        /// Displayable name of the tile server, i.e. human-readable map name, for example, "Open Street Map".
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Attribution text that will be displayed in bottom-right corner of the map.
        /// Can be null (no attribution text) or can contain html links for navigating with default system web browser.
        /// </summary>
        /// <example>© <a href='https://www.openstreetmap.org/copyright'>OpenStreetMap</a> contributors</example>
        public abstract string AttributionText { get; }

        /// <summary>
        /// Gets minimal zoom level allowed for the tile server
        /// </summary>
        public virtual int MinZoomLevel => 0;

        /// <summary>
        /// Gets maximal zoom level allowed for the tile server
        /// </summary>
        public virtual int MaxZoomLevel => 19;

        /// <summary>
        /// Gets tile image by X and Y coordinates of the tile and zoom level Z.
        /// </summary>
        /// <param name="x">X-coordinate of the tile.</param>
        /// <param name="y">Y-coordinate of the tile.</param>
        /// <param name="z">Zoom level</param>
        /// <returns></returns>
        public Image GetTile(int x, int y, int z)
        {
            try
            {
                Uri uri = GetTileUri(x, y, z);
                var request = (HttpWebRequest)WebRequest.Create(uri);
                request.UserAgent = UserAgent;
                using (var response = request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                {
                    return Image.FromStream(stream);
                }
            }
            catch (Exception ex)
            {
                if (ex is WebException wex)
                {
                    if (wex.Response == null)
                    {
                        Thread.Sleep(1000);
                    }
                }

                throw new Exception($"Unable to download tile.\n{ex.Message}");
            }
        }

        /// <summary>
        /// Base constructor for initializing <see cref="WebTileServer"/>.
        /// </summary>
        protected WebTileServer()
        {
            ServicePointManager.ServerCertificateValidationCallback = new Net.Security.RemoteCertificateValidationCallback(AcceptAllCertificates);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        /// <summary>
        /// Function to handle accepting HTTPs certificates 
        /// </summary>
        private bool AcceptAllCertificates(object sender, Security.Cryptography.X509Certificates.X509Certificate certification, Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
