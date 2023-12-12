using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Types
{
    public class TextRenderer : IDisposable
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
            //gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
            //gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            texture = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
        }

        #region Public Members

        /// <summary>
        /// Draws the specified string to the backing store.
        /// </summary>
        /// <param name="text">The <see cref="System.String"/> to draw.</param>
        /// <param name="font">The <see cref="System.Drawing.Font"/> that will be used.</param>
        /// <param name="brush">The <see cref="System.Drawing.Brush"/> that will be used.</param>
        /// <param name="point">The location of the text on the backing store, in 2d pixel coordinates.
        /// The origin (0, 0) lies at the top-left corner of the backing store.</param>
        public void DrawString(string text, Font font, Brush brush, PointF point)
        {
            gfx.Clear(Color.Transparent);
            gfx.DrawString(text, font, brush, new Point());

            GL.Color3(Color.Transparent);
            GL.Enable(EnableCap.Blend);

            // this needed for proper texture overlapping
            //GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.BlendFunc(BlendingFactor.One, BlendingFactor.One);

            GL.Enable(EnableCap.Texture2D);

            GetBitmapData();

            GL.Begin(PrimitiveType.Quads);

            GL.TexCoord2(0, 1); GL.Vertex2(point.X, point.Y - bmp.Height);
            GL.TexCoord2(1, 1); GL.Vertex2(point.X + bmp.Width, point.Y - bmp.Height);

            GL.TexCoord2(1, 0); GL.Vertex2(point.X + bmp.Width, point.Y);
            GL.TexCoord2(0, 0); GL.Vertex2(point.X, point.Y);

            GL.End();
            GL.Disable(EnableCap.Texture2D);

            // revert to "default" blending func
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }


        #endregion

        #region Private Members

        private void GetBitmapData()
        {
            System.Drawing.Imaging.BitmapData data = bmp.LockBits(new Rectangle(Point.Empty, bmp.Size),
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.BindTexture(TextureTarget.Texture2D, texture);

            GL.TexImage2D(TextureTarget.Texture2D, 0,
                PixelInternalFormat.Rgba, bmp.Width, bmp.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

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
                    if (GraphicsContext.CurrentContext != null)
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
