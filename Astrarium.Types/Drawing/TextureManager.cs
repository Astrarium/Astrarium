using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using System.Windows;

namespace Astrarium.Types
{
    internal class TextureManager
    {
        /// <summary>
        /// Holds texture info
        /// </summary>
        private class Texture
        {
            public string Path { get; set; }
            public int TextureId { get; set; }
            public bool IsPermanent { get; set; }
            public int UsageCounter { get; set; }
            public Action ReadyCallback { get; set; }

            public static bool operator ==(Texture t1, Texture t2)
            {
                return t1?.Path == t2?.Path;
            }

            public static bool operator !=(Texture t1, Texture t2)
            {
                return t1?.Path != t2?.Path;
            }
        }

        private object locker = new object();

        private List<Texture> textures = new List<Texture>();

        private AutoResetEvent autoReset = new AutoResetEvent(false);

        public TextureManager()
        {
            Thread worker = new Thread(ProcessPoll) { IsBackground = true };
            worker.Start();

            Thread disposer = new Thread(Disposing) { IsBackground = true };
            disposer.Start();
        }

        public int GetTexture(string path, string fallbackPath = null, bool permanent = false, Action readyCallback = null)
        {
            lock (locker)
            {
                var texture = textures.FirstOrDefault(x => x.Path == path);

                // texture is loaded or already requested
                if (texture != null)
                {
                    // texture already loaded
                    if (texture.TextureId != 0)
                    {
                        texture.UsageCounter = 0;
                        return texture.TextureId;
                    }
                }
                // new texture, make request
                else
                {
                    textures.Add(new Texture()
                    {
                        Path = path,
                        IsPermanent = permanent,
                        ReadyCallback = readyCallback,
                    });
                    autoReset.Set();
                }

                // has fallback?
                if (fallbackPath != null)
                {
                    return GetTexture(fallbackPath, permanent: true);
                }

                return 0;
            }
        }

        public void Cleanup()
        {
            lock (locker)
            {
                textures.ForEach(x => x.UsageCounter++);
            }
        }

        public void RemoveTexture(string path)
        {
            lock (locker)
            {
                var texture = textures.FirstOrDefault(x => x.Path == path);
                if (texture != null)
                {
                    textures.Remove(texture);
                    Application.Current.Dispatcher.Invoke(() => GL.DeleteTexture(texture.TextureId));
                }
            }
        }

        private void Disposing()
        {
            while (true)
            {
                int[] removedIds = null;
                lock (locker)
                {
                    var removed = textures.Where(x => !x.IsPermanent && x.UsageCounter > 10).ToArray();
                    if (removed.Any())
                    {
                        removedIds = removed.Select(x => x.TextureId).ToArray();
                        textures.RemoveAll(x => removed.Contains(x));
                    }
                }

                if (removedIds != null)
                {
                    Application.Current.Dispatcher.Invoke(() => GL.DeleteTextures(removedIds.Length, removedIds));
                    Debug.WriteLine($"Unloaded textures: {removedIds.Length}, remaining: {textures.Count()}");
                }

                Thread.Sleep(5000);
            }
        }

        private void ProcessPoll()
        {
            while (true)
            {
                autoReset.WaitOne();

                List<Texture> pending = null;

                lock (locker)
                {
                    pending = textures.Where(x => x.TextureId == 0).ToList();
                }

                foreach (var texture in pending)
                {
                    try
                    {
                        using (Bitmap bmp = (Bitmap)Image.FromFile(texture.Path))
                        {
                            BitmapData data = bmp.LockBits(new Rectangle(System.Drawing.Point.Empty, bmp.Size), ImageLockMode.ReadOnly, bmp.PixelFormat);
                            Application.Current.Dispatcher.Invoke(() => BindTexture(texture, data));
                            bmp.UnlockBits(data);
                        }
                    }
                    catch 
                    {
                        Thread.Sleep(500);
                    }
                }
            }
        }

        private void BindTexture(Texture texture, BitmapData data)
        {
            texture.TextureId = GL.GenTexture();

            bool hasAlphaChannel = Image.IsAlphaPixelFormat(data.PixelFormat);

            int internalFormat = hasAlphaChannel ? GL.RGBA : GL.RGB;
            int pixelFormat = hasAlphaChannel ? GL.BGRA : GL.BGR;

            GL.BindTexture(GL.TEXTURE_2D, texture.TextureId);
            GL.TexImage2D(GL.TEXTURE_2D, 0, internalFormat, data.Width, data.Height, 0, pixelFormat, GL.UNSIGNED_BYTE, data.Scan0);

            GL.TexParameter(GL.TEXTURE_2D, GL.TEXTURE_MIN_FILTER, GL.LINEAR);
            GL.TexParameter(GL.TEXTURE_2D, GL.TEXTURE_MAG_FILTER, GL.LINEAR);

            texture.ReadyCallback?.Invoke();
        }
    }
}
