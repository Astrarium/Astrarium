using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms
{
    /// <summary>
    /// Represents base class for all <see cref="MapControl"/> drawing events.
    /// </summary>
    public abstract class MapControlDrawEventArgs : HandledEventArgs
    {
        /// <summary>
        /// <see cref="System.Drawing.Graphics"/> instance to draw on.
        /// </summary>
        public Graphics Graphics { get; internal set; }
    }
}
