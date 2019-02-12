using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo.Renderers
{
    /// <summary>
    /// Base class for all renderer classes which implement drawing logic of sky map.
    /// </summary>
    public abstract class BaseSkyRenderer
    {
        public event Action OnInvalidateRequested;

        protected Sky Sky { get; private set; }
        protected ISettings Settings { get; private set; }

        protected void RaiseInvalidateRequest()
        {
            OnInvalidateRequested?.Invoke();
        }

        public BaseSkyRenderer(Sky sky, ISkyMap skyMap, ISettings settings)
        {
            Sky = sky;
            //Map = skyMap;
            Settings = settings;
        }

        public abstract void Render(IMapContext map);

        public virtual void Initialize() { }

        protected void DrawObjectCaption(IMapContext map, Font font, Brush brush, string caption, PointF p, float size)
        {
            SizeF b = map.Graphics.MeasureString(caption, font);

            float s = size > 5 ? (size / 2.8284f + 2) : 1;
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    float dx = x == 0 ? s : -s - b.Width;
                    float dy = y == 0 ? s : -s - b.Height;
                    RectangleF r = new RectangleF(p.X + dx, p.Y + dy, b.Width, b.Height);
                    if (!map.Labels.Any(l => l.IntersectsWith(r)) && !map.DrawnPoints.Any(v => r.Contains(v)))
                    {
                        map.Graphics.DrawString(caption, font, brush, r.Location);
                        map.Labels.Add(r);
                        return;
                    }
                }
            }
        }
    }
}
