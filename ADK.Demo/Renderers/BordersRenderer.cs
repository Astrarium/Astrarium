using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo.Renderers
{
    public class BordersRenderer : BaseSkyRenderer
    {
        public BordersRenderer(Sky sky, ISkyMap skyMap) : base(sky, skyMap)
        {

        }

        public override void Render(Graphics g)
        {
            PointF p1;
            PointF p2;

            Pen pen = Pens.Brown;

            for (int i = 0; i < Sky.Borders.Count - 1; i++)
            {
                if (!Sky.Borders[i + 1].Start)
                {
                    var h1 = Sky.Borders[i].Horizontal;
                    var h2 = Sky.Borders[i + 1].Horizontal;

                    p1 = Map.Projection.Project(h1);
                    p2 = Map.Projection.Project(h2);

                    var points = Geometry.SegmentRectangleIntersection(p1, p2, Map.Width, Map.Height);
                    if (points.Length == 2)
                    {
                        g.DrawLine(pen, points[0], points[1]);
                    }
                }
            }
        }
    }
}
