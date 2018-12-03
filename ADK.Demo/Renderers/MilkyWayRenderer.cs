using ADK.Demo.Objects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo.Renderers
{
    public class MilkyWayRenderer : BaseSkyRenderer
    {
        private List<MilkyWayPoint>[] milkyWay = null;
        private Brush brushMilkyWay = new SolidBrush(Color.FromArgb(20, 20, 20));

        public MilkyWayRenderer(Sky sky, ISkyMap skyMap) : base(sky, skyMap)
        {
            milkyWay = Sky.Get<List<MilkyWayPoint>[]>("MilkyWay");
        }

        public override void Render(Graphics g)
        {
            var smoothing = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.None;

            for (int i = 0; i < milkyWay.Count(); i++)
            {
                List<PointF> points = new List<PointF>();

                PointF p0 = Map.Projection.Project(milkyWay[i][0].Horizontal);
                points.Add(p0);

                bool isPartVisible = p0.X >= 0 && p0.X <= Map.Width && p0.Y >= 0 && p0.Y <= Map.Height;
                
                for (int j = 1; j < milkyWay[i].Count; j++)
                {
                    PointF p1 = Map.Projection.Project(milkyWay[i][j].Horizontal);
                    if (p1.X >= 0 && p1.X <= Map.Width && p1.Y >= 0 && p1.Y <= Map.Height)
                    {
                        isPartVisible = true;
                    }
                    points.Add(p1);
                }

                if (isPartVisible)
                {
                    g.FillPolygon(brushMilkyWay, points.ToArray(), FillMode.Alternate);
                }
            }

            g.SmoothingMode = smoothing;
        }
    }
}
