using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo.Renderers
{
    public class GroundRenderer : BaseSkyRenderer
    {
        public GroundRenderer(Sky sky, ISkyMap skyMap, ISettings settings) : base(sky, skyMap, settings)
        {

        }

        public override void Render(Graphics g)
        {
            const int POINTS_COUNT = 64;
            PointF[] hor = new PointF[POINTS_COUNT];
            double step = 2 * Map.ViewAngle / (POINTS_COUNT - 1);
            SolidBrush brushGround = new SolidBrush(Color.FromArgb(4, 10, 10));
            
            // Bottom part of ground shape

            for (int i = 0; i < POINTS_COUNT; i++)
            {
                var h = new CrdsHorizontal(Map.Center.Azimuth - Map.ViewAngle + step * i, 0);
                hor[i] = Map.Projection.Project(h);
            }
            if (hor[0].X >= 0) hor[0].X = -1;
            if (hor[POINTS_COUNT - 1].X <= Map.Width) hor[POINTS_COUNT - 1].X = Map.Width + 1;

            if (hor.Any(h => h.X > 0 && h.X < Map.Width && h.Y > 0 && h.Y < Map.Height))
            {
                GraphicsPath gp = new GraphicsPath();

                gp.AddCurve(hor);
                gp.AddLines(new PointF[]
                {
                    new PointF(Map.Width + 1, Map.Height + 1),
                    new PointF(-1, Map.Height + 1)
                });

                g.FillPath(brushGround, gp);
            }
            
            // Top part of ground shape 

            if (Map.Center.Altitude > 0)
            { 
                for (int i = 0; i < POINTS_COUNT; i++)
                {
                    var h = new CrdsHorizontal(Map.Center.Azimuth - Map.ViewAngle - step * i, 0);
                    hor[i] = Map.Projection.Project(h);
                }

                if (hor.Any(h => h.X > 0 && h.X < Map.Width && h.Y > 0 && h.Y < Map.Height))
                {
                    GraphicsPath gp = new GraphicsPath();

                    gp.AddCurve(hor);
                    gp.AddLines(new PointF[] 
                    {
                        new PointF(Map.Width + 1, -1),
                        new PointF(-1, -1),
                    });

                    g.FillPath(brushGround, gp);
                }
            }
        }
    }
}
