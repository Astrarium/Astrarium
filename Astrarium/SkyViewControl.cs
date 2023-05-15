using OpenTK;
using System.ComponentModel;
using System.Windows.Forms;

namespace Astrarium
{
    [DesignerCategory("code")]
    public class SkyViewControl : GLControl
    {
        public SkyViewControl() : base(new OpenTK.Graphics.GraphicsMode(new OpenTK.Graphics.ColorFormat(8, 8, 8, 8), 24, 8, 0), 3, 0, OpenTK.Graphics.GraphicsContextFlags.Default)
        {
            Cursor = Cursors.Cross;
        }
    }
}
