using Astrarium.Types;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Astrarium
{
    public class TextureManager : ITextureManager
    {
        private ConcurrentDictionary<string, int> textureIds = new ConcurrentDictionary<string, int>();
        private ConcurrentDictionary<string, DateTime> usageTimeStamps = new ConcurrentDictionary<string, DateTime>();
        private ConcurrentQueue<string> requests = new ConcurrentQueue<string>();
        private ConcurrentDictionary<string, Action> textureParameterActions = new ConcurrentDictionary<string, Action>();

        private AutoResetEvent autoReset = new AutoResetEvent(false);

        public TextureManager()
        {
            Thread worker = new Thread(ProcessPoll) { IsBackground = true };
            worker.Start();

            Thread disposer = new Thread(Disposing) { IsBackground = true };
            disposer.Start();
        }

        public Action FallbackAction { get; set; }

        public int GetTexture(string path, string fallbackPath = null, bool permanent = false)
        {
            if (textureIds.ContainsKey(path))
            {
                int textureId = textureIds[path];
                if (textureId == 0)
                {
                    if (fallbackPath != null)
                    {
                        return GetTexture(fallbackPath);
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    if (!permanent)
                    {
                        usageTimeStamps[path] = DateTime.Now;
                    }
                    return textureId;
                }
            }
            else // texture not loaded
            {
                if (!requests.Contains(path))
                {
                    requests.Enqueue(path);
                    autoReset.Set();
                }

                textureIds[path] = 0;
                // has fallback, process async
                if (fallbackPath != null)
                {
                    // get fallback
                    return GetTexture(fallbackPath);
                }
                else
                {
                    return 0;
                }
            }
        }

        public void SetTextureParams(string path, Action action)
        {
            textureParameterActions[path] = action;
        }

        public void DeleteTexture(string path)
        {
            if (textureIds.ContainsKey(path))
            {
                Application.Current.Dispatcher.Invoke(() => GL.DeleteTexture(textureIds[path]));
                textureIds.TryRemove(path, out int _);
                System.Diagnostics.Debug.WriteLine($"Delete texture: {path}");
            }
        }

        public void DeleteUnusedTextures()
        {
            
        }

        private void Disposing()
        {
            while (true)
            {
                var keys = usageTimeStamps.Where(x => DateTime.Now - x.Value > TimeSpan.FromSeconds(10)).Select(x => x.Key).ToArray();
                foreach (var key in keys)
                {
                    DeleteTexture(key);
                }

                Thread.Sleep(5000);
            }
        }

        private void ProcessPoll()
        {
            while (true)
            {
                autoReset.WaitOne();

                while (requests.TryDequeue(out string key))
                {
                    LoadTexture(key);
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

            if (textureParameterActions.ContainsKey(key))
            {
                textureParameterActions[key].Invoke();
            }

            textureIds[key] = textureId;
          
            if (FallbackAction != null)
            {
                FallbackAction.Invoke();
            }

            //Marshal.Release(data.Scan0);
        }
    }
}
