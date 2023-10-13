using Astrarium.Types;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using System.Windows;

namespace Astrarium
{
    public class TextureManager : ITextureManager
    {
        private class Texture
        {
            public string Path { get; set; }
            public int TextureId { get; set; }
            public bool IsPermanent { get; set; }
            public int UsageCounter { get; set; }
            public Action Action { get; set; }

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

        public Action FallbackAction { get; set; }

        public int GetTexture(string path, string fallbackPath = null, bool permanent = false, Action action = null)
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
                        Action = action
                    });
                    autoReset.Set();
                }

                // has fallback?
                if (fallbackPath != null)
                {
                    return GetTexture(fallbackPath);
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

        private void Disposing()
        {
            while (true)
            {
                int[] removedIds = null;
                lock (locker)
                {
                    var removed = textures.Where(x => x.UsageCounter > 10).ToArray();
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
                    using (Bitmap bmp = (Bitmap)Image.FromFile(texture.Path))
                    {
                        BitmapData data = bmp.LockBits(new Rectangle(System.Drawing.Point.Empty, bmp.Size), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                        Application.Current.Dispatcher.Invoke(() => BindTexture(texture, data));
                        bmp.UnlockBits(data);
                    }
                }
            }
        }

        private void BindTexture(Texture texture, BitmapData data)
        {
            texture.TextureId = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, texture.TextureId);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            texture.Action?.Invoke();

            FallbackAction?.Invoke();
        }
    }
}
