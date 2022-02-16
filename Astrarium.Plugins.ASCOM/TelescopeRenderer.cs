using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.ASCOM
{
    /// <summary>
    /// Draws ASCOM telescope marker on the map
    /// </summary>
    public class TelescopeRenderer : BaseRenderer
    {
        private IAscomProxy ascom;
        private ISkyMap map;
        private ISettings settings;

        public override RendererOrder Order => RendererOrder.Foreground;
       
        public TelescopeRenderer(ISkyMap map, ISettings settings)
        {
            ascom = Ascom.Proxy;
            ascom.PropertyChanged += Ascom_PropertyChanged;
            this.map = map;
            this.settings = settings;
        }

        private void Ascom_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (ascom.IsConnected && e.PropertyName == nameof(IAscomProxy.Position))
            {
                map.Invalidate();
            }

            if (e.PropertyName == nameof(IAscomProxy.IsConnected))
            {
                map.Invalidate();
            }
        }

        public override void Render(IMapContext map)
        {
            if (ascom.IsConnected)
            {
                try
                {
                    var hor = ascom.Position.ToHorizontal(map.GeoLocation, map.SiderealTime);
                    var p = map.Project(hor);

                    var color = map.GetColor("TelescopeMarkerColor");
                    Pen marker = new Pen(color, 2);
                    marker.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;

                    float r = 16;
                    float a0 = (float)(Math.PI / 4);
                    for (double a = 0; a < 2 * Math.PI; a += Math.PI / 2)
                    {
                        float x0 = p.X + (r / 2) * (float)Math.Cos(a + a0);
                        float y0 = p.Y + (r / 2) * (float)Math.Sin(a + a0);

                        float x1 = p.X + r * (float)Math.Cos(a + a0);
                        float y1 = p.Y + r * (float)Math.Sin(a + a0);

                        map.Graphics.DrawLine(marker, new PointF(x0, y0), new PointF(x1, y1));
                    }
                    map.Graphics.DrawEllipse(marker, p.X - 16, p.Y - 16, 32, 32);

                    if (settings.Get("TelescopeMarkerLabel"))
                    {
                        var font = settings.Get<Font>("TelescopeMarkerFont");
                        var brush = new SolidBrush(map.GetColor("TelescopeMarkerColor"));
                        map.Graphics.DrawString(ascom.TelescopeName, font, brush, new PointF(p.X + 16 * 0.8f, p.Y + 16 * 0.8f));
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Rendering error in {nameof(TelescopeRenderer)}: {ex}");
                }
            }
        }
    }
}
