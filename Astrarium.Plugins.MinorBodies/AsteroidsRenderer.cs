using Astrarium.Algorithms;
using Astrarium.Types;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using System.Linq;

namespace Astrarium.Plugins.MinorBodies
{
    public class AsteroidsRenderer : BaseRenderer
    {
        private readonly ISkyMap map;
        private readonly AsteroidsCalc asteroidsCalc;
        private readonly ISettings settings;
        private readonly Lazy<TextRenderer> textRenderer = new Lazy<TextRenderer>(() => new TextRenderer(256, 32));

        private readonly Color colorAsteroid = Color.FromArgb(100, 100, 100);
        private readonly Color colorAsteroidEdge = Color.FromArgb(50, 50, 50);

        public AsteroidsRenderer(ISkyMap map, AsteroidsCalc asteroidsCalc, ISettings settings)
        {
            this.map = map;
            this.asteroidsCalc = asteroidsCalc;
            this.settings = settings;
        }

        public override void Render(ISkyMap map)
        {
            if (!settings.Get("Asteroids")) return;
            if (map.DaylightFactor == 1) return;

            var prj = map.Projection;
            var schema = settings.Get<ColorSchema>("Schema");
            var colorNames = settings.Get<Color>("ColorAsteroidsLabels").Tint(schema);
            Brush brushNames = new SolidBrush(colorNames);
            bool drawLabels = settings.Get("AsteroidsLabels");
            bool drawAll = settings.Get<bool>("AsteroidsDrawAll");
            decimal drawAllMagLimit = settings.Get<decimal>("AsteroidsDrawAllMagLimit");
            bool drawLabelMag = settings.Get<bool>("AsteroidsLabelsMag");
            var font = settings.Get<Font>("AsteroidsLabelsFont");

            var asteroids = asteroidsCalc.Asteroids.Where(a => Angle.Separation(prj.CenterEquatorial, a.Equatorial) < prj.Fov);

            foreach (var a in asteroids)
            {
                double ad = Angle.Separation(a.Equatorial, prj.CenterEquatorial);

                if (ad < prj.Fov + a.Semidiameter / 3600)
                {
                    float diam = prj.GetDiskSize(a.Semidiameter);
                    float size = prj.GetPointSize(a.Magnitude);

                    // if "draw all" setting is enabled, draw asteroids brighter than limit
                    if (drawAll && size < 1 && a.Magnitude <= (float)drawAllMagLimit)
                    {
                        size = 1;
                    }

                    string label = drawLabelMag ? $"{a.Name} {Formatters.Magnitude.Format(a.Magnitude)}" : a.Name;

                    // asteroid should be rendered as disk
                    if ((int)diam > 0 && diam > size)
                    {
                        Vec2 p = prj.Project(a.Equatorial);

                        GL.Enable(EnableCap.Blend);
                        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

                        GL.Begin(PrimitiveType.TriangleFan);

                        // center
                        GL.Color3(colorAsteroid.Tint(schema));
                        GL.Vertex2(p.X, p.Y);
                        double r = diam / 2;
                        for (int i = 0; i <= 64; i++)
                        {
                            double ang = i / 32.0 * Math.PI;
                            Vec2 v = new Vec2(p.X + r * Math.Cos(ang), p.Y + r * Math.Sin(ang));
                            GL.Color3(colorAsteroidEdge.Tint(schema));
                            GL.Vertex2(v.X, v.Y);
                        }

                        GL.End();

                        if (drawLabels)
                        {
                            DrawLabel(a, font, brushNames, p, diam, drawLabelMag);
                        }

                        map.AddDrawnObject(p, a, diam);
                    }
                    // asteroid should be rendered as point
                    else if (size > 0)
                    {
                        if ((int)size == 0) size = 1;

                        Vec2 p = prj.Project(a.Equatorial);

                        if (prj.IsInsideScreen(p))
                        {
                            GL.Enable(EnableCap.PointSmooth);
                            GL.Enable(EnableCap.Blend);
                            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                            GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest);

                            GL.PointSize(size);
                            GL.Begin(PrimitiveType.Points);
                            GL.Color3(colorAsteroid.Tint(schema));
                            GL.Vertex2(p.X, p.Y);
                            GL.End();

                            if (drawLabels)
                            {
                                DrawLabel(a, font, brushNames, p, size, drawLabelMag);
                            }

                            map.AddDrawnObject(p, a, size);
                            continue;
                        }
                    }
                }
            }
        }

        private void DrawLabel<T>(T body, Font font, Brush brush, Vec2 p, float size, bool drawMagInLabel) where T : CelestialObject, IMagnitudeObject
        {
            string name = body.Names.First();
            string label = drawMagInLabel ? $"{name} {Formatters.Magnitude.Format(body.Magnitude)}" : name;
            map.DrawObjectLabel(textRenderer.Value, label, font, brush, p, size);
        }

        public override RendererOrder Order => RendererOrder.SolarSystem;
    }
}
