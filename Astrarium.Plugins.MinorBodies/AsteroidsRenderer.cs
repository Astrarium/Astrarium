using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.MinorBodies
{
    public class AsteroidsRenderer : BaseRenderer
    {
        private readonly AsteroidsCalc asteroidsCalc;
        private readonly ISettings settings;

        private readonly Color colorAsteroid = Color.FromArgb(100, 100, 100);

        public AsteroidsRenderer(AsteroidsCalc asteroidsCalc, ISettings settings)
        {
            this.asteroidsCalc = asteroidsCalc;
            this.settings = settings;
        }

        public override void Render(IMapContext map)
        {
            Graphics g = map.Graphics;
            var allAsteroids = asteroidsCalc.Asteroids;
            bool isGround = settings.Get<bool>("Ground");
            bool drawAsteroids = settings.Get<bool>("Asteroids");
            bool drawLabels = settings.Get<bool>("AsteroidsLabels");
            Brush brushNames = new SolidBrush(map.GetColor("ColorAsteroidsLabels"));

            if (drawAsteroids)
            {
                var asteroids = allAsteroids.Where(a => Angle.Separation(map.Center, a.Horizontal) < map.ViewAngle);

                foreach (var a in asteroids)
                {
                    double ad = Angle.Separation(a.Horizontal, map.Center);

                    if ((!isGround || a.Horizontal.Altitude + a.Semidiameter / 3600 > 0) &&
                        ad < map.ViewAngle + a.Semidiameter / 3600)
                    {
                        float diam = map.GetDiskSize(a.Semidiameter);

                        // asteroid should be rendered as disk
                        if ((int)diam > 0)
                        {
                            PointF p = map.Project(a.Horizontal);
                            map.Graphics.FillEllipse(new SolidBrush(map.GetColor(colorAsteroid)), p.X - diam / 2, p.Y - diam / 2, diam, diam);
                            DrawVolume(map, diam, 0);

                            if (drawLabels)
                            {
                                var font = settings.Get<Font>("AsteroidsLabelsFont");
                                map.DrawObjectCaption(font, brushNames, a.Name, p, diam);
                            }

                            map.AddDrawnObject(a);
                            continue;
                        }

                        // asteroid should be rendered as point
                        float size = map.GetPointSize(a.Magnitude, maxDrawingSize: 3);
                        if ((int)size > 0)
                        {
                            PointF p = map.Project(a.Horizontal);

                            if (!map.IsOutOfScreen(p))
                            {
                                g.FillEllipse(new SolidBrush(map.GetColor(Color.White)), p.X - size / 2, p.Y - size / 2, size, size);

                                if (drawLabels)
                                {
                                    var font = settings.Get<Font>("AsteroidsLabelsFont");
                                    map.DrawObjectCaption(font, brushNames, a.Name, p, size);
                                }

                                map.AddDrawnObject(a);
                                continue;
                            }
                        }
                    }
                }
            }
        }

        private void DrawVolume(IMapContext map, float diam, float flattening)
        {
            if (map.Schema == ColorSchema.White) return;

            Graphics g = map.Graphics;
            float diamEquat = diam * 1.01f;
            float diamPolar = (1 - flattening) * diam * 1.01f;

            using (GraphicsPath gpVolume = new GraphicsPath())
            {
                gpVolume.AddEllipse(-diamEquat, -diamPolar, 2 * diamEquat, 2 * diamPolar);
                using (PathGradientBrush brushVolume = new PathGradientBrush(gpVolume))
                {
                    brushVolume.CenterPoint = new PointF(0, 0);
                    brushVolume.CenterColor = map.GetSkyColor();
                    brushVolume.SetSigmaBellShape(0.3f, 1);
                    brushVolume.SurroundColors = new Color[] { Color.Transparent };
                    g.FillEllipse(brushVolume, -diamEquat / 2, -diamPolar / 2, diamEquat, diamPolar);
                }
            }
        }

        public override RendererOrder Order => RendererOrder.SolarSystem;
    }
}
