using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Astrarium
{
    [DesignerCategory("code")]
    public class SkyView : GLControl
    {
        public SkyView() : base(new OpenTK.Graphics.GraphicsMode(new OpenTK.Graphics.ColorFormat(8, 8, 8, 8), 24, 8, 0), 3, 0, OpenTK.Graphics.GraphicsContextFlags.Default)
        {
            Cursor = Cursors.Cross;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Width, Height);
        }
    }
}
