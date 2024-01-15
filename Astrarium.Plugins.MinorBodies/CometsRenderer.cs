using Astrarium.Algorithms;
using Astrarium.Types;
using OpenTK.Graphics.OpenGL;
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
    public class CometsRenderer : BaseRenderer
    {
        private readonly ISkyMap map;
        private readonly CometsCalc cometsCalc;
        private readonly ISettings settings;
        private readonly Lazy<TextRenderer> textRenderer = new Lazy<TextRenderer>(() => new TextRenderer(256, 32));
        private readonly Color colorComet = Color.FromArgb(120, 28, 255, 186);

        public override RendererOrder Order => RendererOrder.SolarSystem;

        public CometsRenderer(ISkyMap map, CometsCalc cometsCalc, ISettings settings)
        {
            this.map = map;
            this.cometsCalc = cometsCalc;
            this.settings = settings;
        }

        public override void Render(ISkyMap map)
        {
            if (!settings.Get("Comets")) return;
            if (map.DaylightFactor == 1) return;

            var prj = map.Projection;
            var nightMode = settings.Get("NightMode");
            var colorNames = settings.Get<Color>("ColorCometsLabels").Tint(nightMode);
            Brush brushNames = new SolidBrush(colorNames);
            bool drawLabels = settings.Get("CometsLabels");
            bool drawAll = settings.Get<bool>("CometsDrawAll");
            decimal drawAllMagLimit = settings.Get<decimal>("CometsDrawAllMagLimit");
            bool drawLabelMag = settings.Get<bool>("CometsLabelsMag");
            var font = settings.Get<Font>("CometsLabelsFont");
            var eqCenter = prj.WithoutRefraction(prj.CenterEquatorial);
            double fov = prj.Fov * Math.Max(prj.ScreenWidth, prj.ScreenHeight) / Math.Min(prj.ScreenWidth, prj.ScreenHeight);
            var comets = cometsCalc.Comets.Where(a => Angle.Separation(eqCenter, a.Equatorial) < fov + Angle.Separation(a.Equatorial, a.TailEquatorial) + a.Semidiameter / 3600);

            foreach (var c in comets)
            {
                float diam = prj.GetDiskSize(c.Semidiameter);
                float size = prj.GetPointSize(c.Magnitude);

                // if "draw all" setting is enabled, draw comets brighter than limit
                if (drawAll && size < 1 && c.Magnitude <= (float)drawAllMagLimit)
                {
                    size = 1;
                }

                // comet center
                Vec2 p = prj.Project(c.Equatorial);

                // comet tail end
                Vec2 t = prj.Project(c.TailEquatorial);

                if (p == null || t == null) continue;

                double tail = p.Distance(t);

                if (diam > 5 || tail > 50)
                {
                    if (IsSegmentIntersectScreen(p, t, prj.ScreenWidth, prj.ScreenHeight) ||
                        Angle.Separation(c.Equatorial, eqCenter) < fov + c.Semidiameter / 3600)
                    {
                        GL.Enable(EnableCap.Blend);
                        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

                        GL.Begin(PrimitiveType.TriangleFan);

                        // center
                        GL.Color4(colorComet.Tint(nightMode));
                        GL.Vertex2(p.X, p.Y);

                        bool drawTail = tail > 2 * diam;
                        double steps = drawTail ? 32 : 64;
                        double arc = drawTail ? 180 : 360;

                        if (drawTail)
                        {
                            GL.Color4(Color.FromArgb(0, 0, 0, 0));
                            GL.Vertex2(t.X, t.Y);
                        }

                        double r = diam / 2;
                        double rotAxis = Math.Atan2(t.Y - p.Y, t.X - p.X) + Math.PI / 2;
                        for (int i = 0; i <= steps; i++)
                        {
                            double ang0 = Angle.ToRadians(i / steps * arc);
                            double ang = ang0 + rotAxis;
                            Vec2 v = new Vec2(p.X + r * Math.Cos(ang), p.Y + r * Math.Sin(ang));
                            GL.Color4(Color.FromArgb(0, 0, 0, 0));
                            GL.Vertex2(v.X, v.Y);
                        }

                        if (drawTail)
                        {
                            GL.Color4(Color.FromArgb(0, 0, 0, 0));
                            GL.Vertex2(t.X, t.Y);
                        }

                        GL.End();

                        if (drawLabels)
                        {
                            DrawLabel(c, font, brushNames, p, diam, drawLabelMag);
                        }

                        map.AddDrawnObject(p, c, diam);
                    }
                }
                else if ((int)size > 0 && prj.IsInsideScreen(p))
                {
                    GL.Enable(EnableCap.PointSmooth);
                    GL.Enable(EnableCap.Blend);
                    GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                    GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest);

                    GL.PointSize(size);
                    GL.Begin(PrimitiveType.Points);
                    GL.Color3(colorComet.Tint(nightMode));
                    GL.Vertex2(p.X, p.Y);
                    GL.End();

                    if (drawLabels)
                    {
                        DrawLabel(c, font, brushNames, p, size, drawLabelMag);
                    }

                    map.AddDrawnObject(p, c, size);
                }
            }
        }

        private void DrawLabel<T>(T body, Font font, Brush brush, Vec2 p, float size, bool drawMagInLabel) where T : CelestialObject, IMagnitudeObject
        {
            string name = body.Names.First();
            string label = drawMagInLabel ? $"{name} {Formatters.Magnitude.Format(body.Magnitude)}" : name;
            map.DrawObjectLabel(textRenderer.Value, label, font, brush, p, size);
        }

        private bool IsSegmentIntersectScreen(Vec2 p1, Vec2 p2, double width, double height)
        {
            double minX = p1.X;
            double maxX = p2.X;

            if (p1.X > p2.X)
            {
                minX = p2.X;
                maxX = p1.X;
            }

            if (maxX > width)
            {
                maxX = width;
            }

            if (minX < 0)
            {
                minX = 0;
            }

            if (minX > maxX)
            {
                return false;
            }

            double minY = p1.Y;
            double maxY = p2.Y;

            double dx = p2.X - p1.X;

            if (Math.Abs(dx) > 0.0000001)
            {
                double a = (p2.Y - p1.Y) / dx;
                double b = p1.Y - a * p1.X;
                minY = a * minX + b;
                maxY = a * maxX + b;
            }

            if (minY > maxY)
            {
                double tmp = maxY;
                maxY = minY;
                minY = tmp;
            }

            if (maxY > height)
            {
                maxY = height;
            }

            if (minY < 0)
            {
                minY = 0;
            }

            if (minY > maxY)
            {
                return false;
            }

            return true;
        }
    }
}
