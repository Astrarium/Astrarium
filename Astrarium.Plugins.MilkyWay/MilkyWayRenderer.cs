using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Astrarium.Plugins.MilkyWay
{
    /// <summary>
    /// Renders Milky Way filled outline on the map
    /// </summary>
    public class MilkyWayRenderer : BaseRenderer
    {
        private readonly MilkyWayCalc milkyWayCalc;
        private readonly ISettings settings;

        private double minAlpha = 255;
        private double maxAlpha = 10;
        private double minZoom = 90;
        private double maxZoom = 5;
        private double k;
        private double b;

        public MilkyWayRenderer(MilkyWayCalc milkyWayCalc, ISettings settings)
        {
            this.milkyWayCalc = milkyWayCalc;
            this.settings = settings;

            k = -(minAlpha - maxAlpha) / (maxZoom - minZoom);
            b = -(minZoom * maxAlpha - maxZoom * minAlpha) / (maxZoom - minZoom);
        }

        public override void Render(IMapContext map)
        {
            if (settings.Get<bool>("MilkyWay"))
            {
                int alpha = Math.Min((int)(k * map.ViewAngle + b), 255);
                if (alpha > maxAlpha)
                {
                    var smoothing = map.Graphics.SmoothingMode;
                    map.Graphics.SmoothingMode = SmoothingMode.None;

                    if (map.Schema == ColorSchema.Day)
                    {
                        alpha = (int)(alpha * (1 - map.DayLightFactor));
                    }

                    for (int i = 0; i < milkyWayCalc.MilkyWay.Count(); i++)
                    {
                        var points = new List<PointF>();

                        for (int j = 0; j < milkyWayCalc.MilkyWay[i].Count; j++)
                        {
                            var h = milkyWayCalc.MilkyWay[i][j].Horizontal;
                            double ad = Angle.Separation(h, map.Center);

                            // 130 degrees value limit has been chosen experimentally
                            if (ad < 130)
                            {
                                points.Add(map.Project(h));
                            }                            
                        }

                        if (points.Count >= 3)
                        {
                            Color color = Color.FromArgb(alpha, map.GetColor("ColorMilkyWay"));
                            map.Graphics.FillPolygon(new SolidBrush(color), points.ToArray(), FillMode.Winding);
                        }
                    }

                    map.Graphics.SmoothingMode = smoothing;
                }
            }
        }

        public override RendererOrder Order => RendererOrder.Background;
    }
}
