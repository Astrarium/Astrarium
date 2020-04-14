using Astrarium.Algorithms;
using Astrarium.Types;
using Astrarium.Types.Localization;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Horizon
{
    public class GroundRenderer : BaseRenderer
    {
        private readonly ISettings settings;
        private readonly Font[] fontCardinalLabels = new[] { new Font("Arial", 12), new Font("Arial", 8) };

        private readonly Color colorGroundNight = Color.FromArgb(4, 10, 10);
        private readonly Color colorGroundDay = Color.FromArgb(116, 185, 139);

        private readonly string[] cardinalDirections = new string[] { "S", "SW", "W", "NW", "N", "NE", "E", "SE" };

        public GroundRenderer(ISettings settings)
        {
            this.settings = settings;
        }

        public override RendererOrder Order => RendererOrder.Terrestrial;

        public override void Render(IMapContext map)
        {
            double coeff = map.DiagonalCoefficient();

            if (settings.Get<bool>("Ground"))
            {
                const int POINTS_COUNT = 64;
                PointF[] hor = new PointF[POINTS_COUNT];
                double step = 2 * map.ViewAngle / (POINTS_COUNT - 1);
                SolidBrush brushGround = new SolidBrush(map.GetColor(colorGroundNight, colorGroundDay));

                // Bottom part of ground shape

                for (int i = 0; i < POINTS_COUNT; i++)
                {
                    var h = new CrdsHorizontal(map.Center.Azimuth - map.ViewAngle + step * i, 0);
                    hor[i] = map.Project(h);
                }
                if (hor[0].X >= 0) hor[0].X = -1;
                if (hor[POINTS_COUNT - 1].X <= map.Width) hor[POINTS_COUNT - 1].X = map.Width + 1;

                if (hor.Any(h => !map.IsOutOfScreen(h)))
                {
                    GraphicsPath gp = new GraphicsPath();

                    gp.AddCurve(hor);
                    gp.AddLines(new PointF[]
                    {
                        new PointF(map.Width + 1, map.Height + 1),
                        new PointF(-1, map.Height + 1)
                    });

                    map.Graphics.FillPath(brushGround, gp);
                }
                else if (map.Center.Altitude <= 0)
                {
                    map.Graphics.FillRectangle(brushGround, 0, 0, map.Width, map.Height);
                }

                // Top part of ground shape 

                if (map.Center.Altitude > 0)
                {
                    for (int i = 0; i < POINTS_COUNT; i++)
                    {
                        var h = new CrdsHorizontal(map.Center.Azimuth - map.ViewAngle - step * i, 0);
                        hor[i] = map.Project(h);
                    }

                    if (hor.Count(h => !map.IsOutOfScreen(h)) > 2)
                    {
                        GraphicsPath gp = new GraphicsPath();

                        gp.AddCurve(hor);
                        gp.AddLines(new PointF[]
                        {
                            new PointF(map.Width + 1, -1),
                            new PointF(-1, -1),
                        });

                        map.Graphics.FillPath(brushGround, gp);
                    }
                }
            }

            if (map.Schema == ColorSchema.White || (!settings.Get<bool>("Ground") && settings.Get<bool>("HorizonLine")))
            {
                const int POINTS_COUNT = 64;
                PointF[] hor = new PointF[POINTS_COUNT];
                double step = 2 * map.ViewAngle / (POINTS_COUNT - 1);

                for (int i = 0; i < POINTS_COUNT; i++)
                {
                    var h = new CrdsHorizontal(map.Center.Azimuth - map.ViewAngle + step * i, 0);
                    hor[i] = map.Project(h);
                }
                if (hor[0].X >= 0) hor[0].X = -1;
                if (hor[POINTS_COUNT - 1].X <= map.Width) hor[POINTS_COUNT - 1].X = map.Width + 1;

                if (hor.Any(h => !map.IsOutOfScreen(h)))
                {
                    Pen penHorizonLine = new Pen(map.GetColor("ColorHorizon"), 2);
                    map.Graphics.DrawCurve(penHorizonLine, hor);
                }
            }

            if (settings.Get<bool>("LabelCardinalDirections"))
            {
                Brush brushCardinalLabels = new SolidBrush(map.GetColor("ColorCardinalDirections"));
                StringFormat format = new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };
                for (int i = 0; i < cardinalDirections.Length; i++)
                {
                    var h = new CrdsHorizontal(i * 360 / cardinalDirections.Length, 0);
                    if (Angle.Separation(h, map.Center) < map.ViewAngle * coeff)
                    {                       
                        PointF p = map.Project(h);
                        p.Y += fontCardinalLabels[i % 2].Height;
                        using (var gp = new GraphicsPath())
                        {
                            map.Graphics.DrawString(Text.Get($"CardinalDirections.{cardinalDirections[i]}"), fontCardinalLabels[i % 2], brushCardinalLabels, p, format);
                        }                        
                    }
                }
            }
        }
    }
}
