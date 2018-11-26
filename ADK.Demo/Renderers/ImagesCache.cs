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
            public object InvalidateToken { get; set; }
        }

        private Dictionary<string, ImageData> DictImages = new Dictionary<string, ImageData>();

        public ImagesCache() { }

        public Image GetImage<T>(string key, T token, Func<T, Image> getImage)
        {
            if (DictImages.ContainsKey(key))
            {
                var data = DictImages[key];
                if (data.InvalidateToken.Equals(token))
                {
                    return DictImages[key].Image;
                }
                else
                {
                    Image image = getImage.Invoke(token);
                    DictImages[key].Image = image;
                    DictImages[key].InvalidateToken = token;
                    return image;
                }
            }
            else
            {
                Image image = getImage.Invoke(token);

                ImageData data = new ImageData()
                {
                    Image = image,
                    InvalidateToken = token
                };

                DictImages.Add(key, data);
                return image;
            }
        }
    }
}
