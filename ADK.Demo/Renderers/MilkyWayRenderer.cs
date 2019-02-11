using ADK.Demo.Calculators;
using ADK.Demo.Objects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace ADK.Demo.Renderers
{
    /// <summary>
    /// Renders Milky Way filled outline on the map
    /// </summary>
    public class MilkyWayRenderer : BaseSkyRenderer
    {
        private IMilkyWayProvider milkyWayProvider;

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

        public MilkyWayRenderer(Sky sky, IMilkyWayProvider milkyWayProvider, ISkyMap skyMap, ISettings settings) : base(sky, skyMap, settings)
        {
            this.milkyWayProvider = milkyWayProvider;

            k = -(minAlpha - maxAlpha) / (maxZoom - minZoom);
            b = -(minZoom * maxAlpha - maxZoom * minAlpha) / (maxZoom - minZoom);
        }

        public override void Render(Graphics g)
        {
            if (Settings.Get<bool>("MilkyWay"))
            {
                int alpha = Math.Min((int)(k * Map.ViewAngle + b), 255);
                if (alpha > maxAlpha)
                {
                    var smoothing = g.SmoothingMode;
                    g.SmoothingMode = SmoothingMode.None;

                    for (int i = 0; i < milkyWayProvider.MilkyWay.Count(); i++)
                    {
                        var points = new List<PointF>();
                        for (int j = 0; j < milkyWayProvider.MilkyWay[i].Count; j++)
                        {
                            var h = milkyWayProvider.MilkyWay[i][j].Horizontal;
                            if (Angle.Separation(h, Map.Center) < 90 * 1.2)
                            {
                                points.Add(Map.Projection.Project(h));
                            }
                        }

                        if (points.Count >= 3)
                        {
                            g.FillPolygon(new SolidBrush(Color.FromArgb(alpha, colorMilkyWay)), points.ToArray(), FillMode.Winding);
                        }
                    }

                    g.SmoothingMode = smoothing;
                }
            }
        }
    }
}
