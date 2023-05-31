using Astrarium.Types;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Astrarium
{
    public class TextureManager : ITextureManager
    {
        private ConcurrentDictionary<string, int> textureIds = new ConcurrentDictionary<string, int>();

        private ConcurrentQueue<string> requests = new ConcurrentQueue<string>();

        private AutoResetEvent autoReset = new AutoResetEvent(false);

        public TextureManager()
        {
            Thread worker = new Thread(ProcessPoll) { IsBackground = true };
            worker.Start();
        }

        public Action FallbackAction { get; set; }

        public int GetTexture(string path, string fallbackPath = null)
        {
            if (textureIds.ContainsKey(path))
            {
                return textureIds[path];
            }
            else // texture not loaded
            {
                // has fallback, process async
                if (fallbackPath != null)
                {
                    if (!requests.Contains(path))
                    {
                        requests.Enqueue(path);
                        autoReset.Set();
                    }

                    // get fallback
                    return GetTexture(fallbackPath);
                }
                else
                {
                    // process sync
                    LoadTexture(path);

                    // get itself
                    return GetTexture(path);
                }
            }
        }

        private void ProcessPoll()
        {
            while (true)
            {
                autoReset.WaitOne();

                while (requests.TryDequeue(out string path))
                {
                    LoadTexture(path);
                }
            }
        }

        private void LoadTexture(string path)
        {
            using (Bitmap bmp = (Bitmap)Image.FromFile(path))
            {
                BitmapData data = bmp.LockBits(new Rectangle(System.Drawing.Point.Empty, bmp.Size), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                Application.Current.Dispatcher.Invoke(() => BindTexture(path, data));
                bmp.UnlockBits(data);
            }
        }

        private void BindTexture(string key, BitmapData data)
        {
            int textureId = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, textureId);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            textureIds[key] = textureId;

            if (FallbackAction != null)
            {
                FallbackAction.Invoke();
            }
        }
    }
}
