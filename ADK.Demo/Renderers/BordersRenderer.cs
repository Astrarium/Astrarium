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
    public class BordersRenderer : BaseSkyRenderer
    {
        private Pen penBorder = new Pen(Color.FromArgb(64, 32, 32));
        private const double maxSeparation = 90 * 1.2;

        public BordersRenderer(Sky sky, ISkyMap skyMap) : base(sky, skyMap)
        {

        }

        public override void Render(Graphics g)
        {
            PointF p1, p2;
            CrdsHorizontal h1, h2;
            var borders = Sky.Get<ICollection<ConstBorderPoint>>("Borders");
            
            for (int i = 0; i < borders.Count - 1; i++)
            {
                if (!borders.ElementAt(i + 1).Start)
                {
                    h1 = borders.ElementAt(i).Horizontal;
                    h2 = borders.ElementAt(i + 1).Horizontal;

                    if (Angle.Separation(Map.Center, h1) < maxSeparation &&
                        Angle.Separation(Map.Center, h2) < maxSeparation)
                    {
                        p1 = Map.Projection.Project(h1);
                        p2 = Map.Projection.Project(h2);

                        var points = Geometry.SegmentRectangleIntersection(p1, p2, Map.Width, Map.Height);
                        if (points.Length == 2)
                        {
                            g.DrawLine(penBorder, points[0], points[1]);
                        }
                    }
                }
            }
        }
    }
}
