using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Astrarium.Plugins.SolarSystem
{
    public class ImagesCache
    {
        private class ImageData
        {
            public Image Image { get; set; }
            public object InvalidateToken { get; set; }
            public bool IsRequestInProgress { get; set; }
        }

        private Dictionary<string, ImageData> cache = new Dictionary<string, ImageData>();
        private Thread worker = null;
        private AutoResetEvent waitSignal = new AutoResetEvent(true);
        private ConcurrentQueue<Action> requests = new ConcurrentQueue<Action>(); 

        public ImagesCache()
        {
            worker = new Thread(DoWork);
            worker.Name = "SphereRendererWorker";
            worker.IsBackground = true;
            worker.SetApartmentState(ApartmentState.STA);
            worker.Start();
        }

        public void Clear()
        {
            cache.Clear();
        }

        public Image RequestImage<T>(string key, T token, Func<T, Image> getImage, Action onComplete = null)
        {
            if (cache.ContainsKey(key))
            {
                var data = cache[key];
                if (data.InvalidateToken.Equals(token))
                {
                    return cache[key].Image;
                }
                else
                {
                    if (!cache[key].IsRequestInProgress)
                    {
                        cache[key].InvalidateToken = token;
                        cache[key].IsRequestInProgress = true;
                        EnqueueRequest(key, token, getImage, onComplete);
                    }

                    return cache[key].Image;
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

                cache.Add(key, data);
                EnqueueRequest(key, token, getImage, onComplete);

                return null;
            }
        }

        private void EnqueueRequest<T>(string key, T token, Func<T, Image> getImage, Action onComplete)
        {
            requests.Enqueue(() => 
            {
                cache[key].Image = getImage.Invoke(token);
                cache[key].IsRequestInProgress = false;
                if (onComplete != null)
                {
                    Task.Run(onComplete);
                }
            });
            waitSignal.Set();
        }

        private void DoWork()
        {
            while (waitSignal.WaitOne())
            {
                while (requests.Any())
                {
                    if (requests.TryDequeue(out Action action)) 
                    {
                        action?.Invoke();
                    }
                }
            }
        }
    }
}
