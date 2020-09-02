using System.Drawing.Drawing2D;

namespace System.Windows.Forms
{
    /// <summary>
    /// Provides data for <see cref="MapControl.DrawPolygon"/> event.
    /// </summary>
    public class DrawPolygonEventArgs : MapControlDrawEventArgs
    {
        /// <summary>
        /// <see cref="Forms.Polygon"/> instance to be drawn.
        /// </summary>
        public Polygon Polygon { get; internal set; }

        /// <summary>
        /// <see cref="GraphicsPath"/> instance describing polygon interior. 
        /// </summary>
        public GraphicsPath Path { get; internal set; }

        /// <summary>
        /// Creates new instance of <see cref="DrawPolygonEventArgs"/>.
        /// </summary>
        internal DrawPolygonEventArgs() { }
    }
}
