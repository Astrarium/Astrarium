using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Renderers
{
    /// <summary>
    /// Class for rendering spherical images of celestial objects.
    /// </summary>
    /// <remarks>
    /// Implementation of the class is based on the solution from article <see href="http://www.100byte.ru/stdntswrks/cshrp/sphTK/sphTK.html"/>.
    /// </remarks>
    internal class GLSphereRenderer : ISphereRenderer
    {
        private GameWindow window;

        private Bitmap GraphicsContextToBitmap(int size)
        {
            GL.Flush();
            Bitmap bitmap = new Bitmap(size, size);
            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.PixelStore(PixelStoreParameter.PackRowLength, data.Stride / 4);
            GL.ReadPixels(0, 0, size, size, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            GL.Finish();
            bitmap.UnlockBits(data);
            bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
            return bitmap;
        }

        public Image Render(RendererOptions options)
        {
            if (window == null)
            {
                int size = 1;
                window = new GameWindow(size, size, new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 0, 0, ColorFormat.Empty, 1), "", GameWindowFlags.Default, DisplayDevice.Default, 3, 0, GraphicsContextFlags.Default);
                window.Visible = false;
                window.WindowState = WindowState.Fullscreen;
                window.Visible = false;
            }

            GL.ClearColor(Color.Transparent);

            using (Bitmap sourceBitmap = new Bitmap(options.TextureFilePath))
            {
                BitmapData data;
                int size = (int)options.OutputImageSize;
                window.ClientSize = new Size(size, size);
                Rectangle rect = new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height);
                data = sourceBitmap.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                double crds = 1;
                GL.Viewport(0, 0, size, size);
                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();
       
                GL.Ortho(-crds, crds, -crds, crds, -crds, crds);

                GL.Rotate(90, new Vector3d(0, 0, 1));
                GL.Rotate(90 - options.LatitudeShift, new Vector3d(0, 1, 0));                
                GL.Rotate(-options.LongutudeShift, new Vector3d(0, 0, 1));                

                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                GL.Viewport(0, 0, size, size);
                GL.Color3(Color.White);
                GL.Disable(EnableCap.Lighting);

                GL.DepthMask(true);
                GL.Enable(EnableCap.DepthTest);
                //GL.ClearDepth(1.0f);
                GL.DepthFunc(DepthFunction.Lequal);

                int nx, ny;

                nx = 64;
                ny = 64;

                int texture;

                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                GL.Enable(EnableCap.Texture2D);
                GL.GenTextures(1, out texture);
                GL.BindTexture(TextureTarget.Texture2D, texture);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

                int ix, iy;
                double x, y, z, sy, cy, sy1, cy1, sx, cx, piy, pix, ay, ay1, ax, tx, ty, ty1, dnx, dny, diy;
                dnx = 1.0 / nx;
                dny = 1.0 / ny;

                GL.Begin(PrimitiveType.QuadStrip);
                piy = Math.PI * dny;
                pix = Math.PI * dnx;
                for (iy = 0; iy < ny; iy++)
                {
                    diy = iy;
                    ay = diy * piy;
                    sy = Math.Sin(ay);
                    cy = Math.Cos(ay);
                    ty = diy * dny;
                    ay1 = ay + piy;
                    sy1 = Math.Sin(ay1);
                    cy1 = Math.Cos(ay1);
                    ty1 = ty + dny;
                    for (ix = 0; ix <= nx; ix++)
                    {
                        ax = 2.0 * ix * pix;
                        sx = Math.Sin(ax);
                        cx = Math.Cos(ax);
                        x = sy * cx;
                        y = sy * sx;
                        z = cy;
                        tx = ix * dnx;

                        GL.TexCoord2(tx, ty);
                        GL.Vertex3(x, y, z);
                        x = sy1 * cx;
                        y = sy1 * sx;
                        z = cy1;

                        GL.TexCoord2(tx, ty1);
                        GL.Vertex3(x, y, z);
                    }
                }
                GL.End();
                GL.Disable(EnableCap.Texture2D);
                GL.DeleteTexture(texture);

                sourceBitmap.UnlockBits(data);
                return GraphicsContextToBitmap(size);
            }
        }
    }
}
