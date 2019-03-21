using ADK;
using Planetarium.Calculators;
using Planetarium.Config;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace Planetarium.Renderers
{
    /// <summary>
    /// Renders Milky Way filled outline on the map
    /// </summary>
    public class MilkyWayRenderer : BaseRenderer
    {
        private readonly IMilkyWayProvider milkyWayProvider;
        private readonly ISettings settings;

        /// <summary>
        /// Primary color to fill outline
        /// </summary>
        private Color colorMilkyWay = Color.FromArgb(20, 20, 20);

        private double minAlpha = 255;
        private double maxAlpha = 10;
        private double minZoom = 90;
        private double maxZoom = 5;
        private double k;
        private double b;

        public MilkyWayRenderer(IMilkyWayProvider milkyWayProvider, ISettings settings)
        {
            this.milkyWayProvider = milkyWayProvider;
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

                    for (int i = 0; i < milkyWayProvider.MilkyWay.Count(); i++)
                    {
                        var points = new List<PointF>();

                        for (int j = 0; j < milkyWayProvider.MilkyWay[i].Count; j++)
                        {
                            var h = milkyWayProvider.MilkyWay[i][j].Horizontal;
                            double ad = Angle.Separation(h, map.Center);

                            // 130 degrees value limit has been chosen experimentally
                            if (ad < 130)
                            {
                                points.Add(map.Project(h));
                            }                            
                        }

                        if (points.Count >= 3)
                        {
                            map.Graphics.FillPolygon(new SolidBrush(Color.FromArgb(alpha, colorMilkyWay)), points.ToArray(), FillMode.Winding);
                        }
                    }

                    map.Graphics.SmoothingMode = smoothing;
                }
            }
        }

        public override int ZOrder => 100;
    }
}
