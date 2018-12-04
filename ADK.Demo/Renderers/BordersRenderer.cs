using ADK.Demo.Objects;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

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
            var borders = Sky.Get<List<List<CelestialPoint>>>("Borders");

            foreach (var block in borders)
            {
                for (int i = 0; i < block.Count - 1; i++)
                {
                    h1 = block.ElementAt(i).Horizontal;
                    h2 = block.ElementAt(i + 1).Horizontal;

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
