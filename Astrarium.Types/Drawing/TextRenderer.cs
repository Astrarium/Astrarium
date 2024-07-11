using System;
using System.Drawing;

namespace Astrarium.Types
{
    internal class TextRenderer : IDisposable
    {
        private Bitmap bmp;
        private Graphics gfx;
        private int texture;
        private bool disposed;

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="width">The width of the backing store in pixels.</param>
        /// <param name="height">The height of the backing store in pixels.</param>
        public TextRenderer(int width, int height)
        {
            bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            gfx = Graphics.FromImage(bmp);

            texture = GL.GenTexture();

            GL.BindTexture(GL.TEXTURE_2D, texture);
            GL.TexParameter(GL.TEXTURE_2D, GL.TEXTURE_MIN_FILTER, GL.LINEAR);
            GL.TexParameter(GL.TEXTURE_2D, GL.TEXTURE_MAG_FILTER, GL.LINEAR);
            GL.TexImage2D(GL.TEXTURE_2D, 0, GL.RGBA, width, height, 0, GL.RGBA, GL.UNSIGNED_BYTE, IntPtr.Zero);
        }

        #region Public Members

        /// <summary>
        /// Draws the specified string to the backing store.
        /// </summary>
        /// <param name="text">The <see cref="String"/> to draw.</param>
        /// <param name="font">The <see cref="Font"/> that will be used.</param>
        /// <param name="brush">The <see cref="Brush"/> that will be used.</param>
        /// <param name="point">The location of the text on the backing store, in 2d pixel coordinates.
        /// The origin (0, 0) lies at the top-left corner of the backing store.</param>
        public void DrawString(string text, Font font, Brush brush, PointF point, bool antiAlias = false)
        {
            gfx.Clear(Color.Transparent);
            

            if (antiAlias)
            {
                gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            }
            else
            {
                gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
                gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
            }

            gfx.DrawString(text, font, brush, new Point());

            GL.Color3(Color.Transparent);
            GL.Enable(GL.BLEND);

            if (antiAlias)
            {
                GL.BlendFunc(GL.SRC_ALPHA, GL.ONE_MINUS_SRC_ALPHA);
            }
            else
            {
                GL.BlendFunc(GL.ONE, GL.ONE);
            }

            GL.Enable(GL.TEXTURE_2D);

            GetBitmapData();

            GL.Begin(GL.QUADS);

            GL.TexCoord2(0, 1); GL.Vertex2(point.X, point.Y - bmp.Height);
            GL.TexCoord2(1, 1); GL.Vertex2(point.X + bmp.Width, point.Y - bmp.Height);

            GL.TexCoord2(1, 0); GL.Vertex2(point.X + bmp.Width, point.Y);
            GL.TexCoord2(0, 0); GL.Vertex2(point.X, point.Y);

            GL.End();
            GL.Disable(GL.TEXTURE_2D);

            // revert to "default" blending func
            GL.BlendFunc(GL.SRC_ALPHA, GL.ONE_MINUS_SRC_ALPHA);
        }

        #endregion

        #region Private Members

        private void GetBitmapData()
        {
            System.Drawing.Imaging.BitmapData data = bmp.LockBits(new Rectangle(Point.Empty, bmp.Size),
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.BindTexture(GL.TEXTURE_2D, texture);

            GL.TexImage2D(GL.TEXTURE_2D, 0, GL.RGBA, bmp.Width, bmp.Height, 0, GL.BGRA, GL.UNSIGNED_BYTE, data.Scan0);

            bmp.UnlockBits(data);
        }

        #endregion

        #region IDisposable Members

        void Dispose(bool manual)
        {
            if (!disposed)
            {
                if (manual)
                {
                    bmp.Dispose();
                    gfx.Dispose();
                    // TODO: check this
                    //if (GraphicsContext.CurrentContext != null)
                    {
                        GL.DeleteTexture(texture);
                    }
                }
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
