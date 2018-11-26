using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo.Renderers
{
    public class ImagesCache
    {
        private class ImageData
        {
            public Image Image { get; set; }
            public Func<Image> ImageProvider { get; set; }
        }

        private Dictionary<string, ImageData> DictImages = new Dictionary<string, ImageData>();

        public ImagesCache() { }

        public Image GetImage(string key)
        {
            if (DictImages.ContainsKey(key))
            {
                return DictImages[key].Image;
            }
            else
            {
                return null;
            }
        }

        public void AddImageProvider(string key, Func<Image> imageProvider)
        {
            DictImages.Add(key, new ImageData()
            {
                Image = null,
                ImageProvider = imageProvider,
            });
        }
    }
}
