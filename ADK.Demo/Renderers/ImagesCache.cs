using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ADK.Demo.Renderers
{
    public class ImagesCache
    {
        private class ImageData
        {
            public Image Image { get; set; }
            public object InvalidateToken { get; set; }
            public bool IsRequestInProgress { get; set; }
        }

        private Dictionary<string, ImageData> Cache = new Dictionary<string, ImageData>();

        public Image GetImage<T>(string key, T token, Func<T, Image> getImage, Action onComplete = null)
        {
            if (Cache.ContainsKey(key))
            {
                var data = Cache[key];
                if (data.InvalidateToken.Equals(token))
                {
                    return Cache[key].Image;
                }
                else
                {
                    if (!Cache[key].IsRequestInProgress)
                    {
                        Cache[key].Image = null;
                        Cache[key].InvalidateToken = token;
                        Cache[key].IsRequestInProgress = true;
                        GetImageInBackground(key, token, getImage, onComplete);
                    }
                    return null;
                }
            }
            else
            {
                ImageData data = new ImageData()
                {
                    Image = null,
                    InvalidateToken = token,
                    IsRequestInProgress = true
                };

                Cache.Add(key, data);
                GetImageInBackground(key, token, getImage);

                return null;
            }
        }

        /// <summary>
        /// Gets image in background thread
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="token"></param>
        /// <param name="getImage"></param>
        private void GetImageInBackground<T>(string key, T token, Func<T, Image> getImage, Action onComplete = null)
        {
            Thread thread = new Thread(() => 
            {
                Cache[key].Image = getImage.Invoke(token);
                Cache[key].IsRequestInProgress = false;
                onComplete?.BeginInvoke(null, null);
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }
    }
}
