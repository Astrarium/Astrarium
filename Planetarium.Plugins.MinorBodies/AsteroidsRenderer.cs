using ADK;
using Planetarium.Config;
using Planetarium.Objects;
using Planetarium.Plugins.MinorBodies;
using Planetarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Renderers
{
    public class AsteroidsRenderer : BaseRenderer
    {
        private Font fontNames;
        private Brush brushNames;
        private readonly IAsteroidsProvider asteroidsProvider;
        private readonly ISettings settings;

        public AsteroidsRenderer(IAsteroidsProvider asteroidsProvider, ISettings settings)
        {
            this.asteroidsProvider = asteroidsProvider;
            this.settings = settings;

            fontNames = new Font("Arial", 8);
            brushNames = new SolidBrush(Color.FromArgb(10, 44, 37));
        }

        public override void Render(IMapContext map)
        {
            Graphics g = map.Graphics;
            var allAsteroids = asteroidsProvider.Asteroids;
            bool isGround = settings.Get<bool>("Ground");
            bool useTextures = settings.Get<bool>("UseTextures");
            double coeff = map.DiagonalCoefficient();
            bool drawAsteroids = settings.Get<bool>("Asteroids");
            bool drawLabels = settings.Get<bool>("AsteroidsLabels");

            if (drawAsteroids)
            {
                var asteroids = allAsteroids.Where(a => Angle.Separation(map.Center, a.Horizontal) < map.ViewAngle * coeff);

                foreach (var a in asteroids)
                {
                    double ad = Angle.Separation(a.Horizontal, map.Center);

                    if ((!isGround || a.Horizontal.Altitude + a.Semidiameter / 3600 > 0) &&
                        ad < coeff * map.ViewAngle + a.Semidiameter / 3600)
                    {
                        float diam = map.GetDiskSize(a.Semidiameter);


                        // asteroid should be rendered as disk
                        if ((int)diam > 0)
                        {
                            PointF p = map.Project(a.Horizontal);

                            if (useTextures)
                            {
                                using (GraphicsPath gpVolume = new GraphicsPath())
                                {
                                    gpVolume.AddEllipse(p.X - diam / 2, p.Y - diam / 2, diam, diam);

                                    PathGradientBrush brushVolume = new PathGradientBrush(gpVolume);
                                    brushVolume.CenterPoint = new PointF(p.X, p.Y);
                                    brushVolume.CenterColor = Color.Black;
                                    brushVolume.SetSigmaBellShape(0.0f, 0.5f);

                                    List<Color> clrs = new List<Color>();
                                    for (int i = 0; i < gpVolume.PathPoints.Length; i++)
                                    {
                                        clrs.Add(Color.Transparent);
                                    }
                                    brushVolume.SurroundColors = clrs.ToArray();

                                    map.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(100, 100, 100)), p.X - diam / 2, p.Y - diam / 2, diam, diam);
                                    map.Graphics.FillPath(brushVolume, gpVolume);
                                }
                            }
                            else
                            {
                                map.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(100, 100, 100)), p.X - diam / 2, p.Y - diam / 2, diam, diam);
                            }

                            if (drawLabels)
                            {
                                map.DrawObjectCaption(fontNames, brushNames, a.Name, p, diam);
                            }

                            map.AddDrawnObject(a, p);
                            continue;
                        }

                        // asteroid should be rendered as point
                        float size = map.GetPointSize(a.Magnitude, maxDrawingSize: 3);
                        if ((int)size > 0)
                        {
                            PointF p = map.Project(a.Horizontal);

                            if (!map.IsOutOfScreen(p))
                            {
                                g.FillEllipse(Brushes.White, p.X - size / 2, p.Y - size / 2, size, size);

                                if (drawLabels)
                                {
                                    map.DrawObjectCaption(fontNames, brushNames, a.Name, p, size);
                                }

                                map.AddDrawnObject(a, p);
                                continue;
                            }
                        }
                    }
                }
            }
        }
       
        public override int ZOrder => 699;
    }
}
