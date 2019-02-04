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
            if (Settings.Get<bool>("Ground"))
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

                if (hor.Any(h => !IsOutOfScreen(h)))
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
                else if (Map.Center.Altitude <= 0)
                {
                    g.FillRectangle(brushGround, 0, 0, Map.Width, Map.Height);
                }

                // Top part of ground shape 

                if (Map.Center.Altitude > 0)
                {
                    for (int i = 0; i < POINTS_COUNT; i++)
                    {
                        var h = new CrdsHorizontal(Map.Center.Azimuth - Map.ViewAngle - step * i, 0);
                        hor[i] = Map.Projection.Project(h);
                    }

                    if (hor.Count(h => !IsOutOfScreen(h)) > 2)
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

            if (Settings.Get<bool>("LabelCardinalDirections"))
            {
                Brush brushCardinalLabels = new SolidBrush(Settings.Get<Color>("CardinalDirections.Color"));
                string[] labels = new string[] { "S", "SW", "W", "NW", "N", "NE", "E", "SE" };
                StringFormat format = new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };
                for (int i = 0; i < labels.Length; i++)
                {
                    var h = new CrdsHorizontal(i * 360 / labels.Length, 0);
                    if (Angle.Separation(h, Map.Center) < Map.ViewAngle * 1.2)
                    {
                        PointF p = Map.Projection.Project(h);
                        g.DrawStringOpaque(labels[i], SystemFonts.DefaultFont, brushCardinalLabels, Brushes.Black, p, format);
                    }
                }
            }
        }
    }
}
