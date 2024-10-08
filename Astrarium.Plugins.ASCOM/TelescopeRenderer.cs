﻿using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WF = System.Windows.Forms;

namespace Astrarium.Plugins.ASCOM
{
    /// <summary>
    /// Draws ASCOM telescope marker on the map
    /// </summary>
    public class TelescopeRenderer : BaseRenderer
    {
        private readonly IAscomProxy ascom;
        private readonly ISkyMap map;
        private readonly ISettings settings;
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

        public override void Render(ISkyMap map)
        {
            if (ascom.IsConnected)
            {
                try
                {
                    var prj = map.Projection;
                    bool nightMode = settings.Get("NightMode");
                    var color = settings.Get<Color>("TelescopeMarkerColor").Tint(nightMode);
                    Pen marker = new Pen(color, 2);

                    Vec2 p = prj.Project(ascom.Position);
                    if (p == null) return;

                    float r = 16;
                    float a0 = (float)(Math.PI / 4);
                    for (double a = 0; a < 2 * Math.PI; a += Math.PI / 2)
                    {
                        double x0 = p.X + r / 2 * (float)Math.Cos(a + a0);
                        double y0 = p.Y + r / 2 * (float)Math.Sin(a + a0);
                        double x1 = p.X + r * (float)Math.Cos(a + a0);
                        double y1 = p.Y + r * (float)Math.Sin(a + a0);
                        GL.DrawLine(new Vec2(x0, y0), new Vec2(x1, y1), marker);
                    }

                    GL.DrawEllipse(p, marker, r);

                    if (settings.Get("TelescopeMarkerLabel"))
                    {
                        string label = ascom.TelescopeName;
                        var font = settings.Get<Font>("TelescopeMarkerFont");
                        var labelSize = WF.TextRenderer.MeasureText(label, font, Size.Empty);
                        var brush = new SolidBrush(color);
                        GL.DrawString(label, font, brush, new PointF((float)p.X, (float)(p.Y - r - font.Size)), horizontalAlign: StringAlignment.Center, antiAlias: true);
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
