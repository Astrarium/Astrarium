using Astrarium.Algorithms;
using Astrarium.Types;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;

namespace Astrarium.Plugins.Grids
{
    public class CelestialGridRenderer : BaseRenderer
    {
        private readonly CelestialGridCalculator calc;
        private readonly ISettings settings;

        private Font fontNodeLabel = new Font("Arial", 10);
        private Font fontEquinoxLabel = new Font("Arial", 10);

        private string[] nodesLabels = new string[] { "\u260A", "\u260B" };
        private string[] equinoxLabels = new string[] { "\u2648", "\u264E" };
        private string[] horizontalLabels = new string[] { Text.Get("CelestialGridRenderer.Zenith"), Text.Get("CelestialGridRenderer.Nadir") };
        private string[] equatorialLabels = new string[] { Text.Get("CelestialGridRenderer.NCP"), Text.Get("CelestialGridRenderer.SCP") };

        private Lazy<TextRenderer> textRenderer = new Lazy<TextRenderer>(() => new TextRenderer(64, 64));

        public CelestialGridRenderer(CelestialGridCalculator calc, ISettings settings)
        {
            this.calc = calc;
            this.settings = settings;
        }

        public override void Render(ISkyMap map)
        {
            var prj = map.Projection;
            var schema = settings.Get<ColorSchema>("Schema");

            Color colorGridEquatorial = settings.Get<SkyColor>("ColorEquatorialGrid").Night.Tint(schema);
            Color colorGridHorizontal = settings.Get<SkyColor>("ColorHorizontalGrid").Night.Tint(schema);
            Color colorLineEcliptic = settings.Get<SkyColor>("ColorEcliptic").Night.Tint(schema);
            Color colorLineGalactic = settings.Get<SkyColor>("ColorGalacticEquator").Night.Tint(schema);
            Color colorLineMeridian = settings.Get<SkyColor>("ColorMeridian").Night.Tint(schema);

            SolidBrush brushHorizontal = new SolidBrush(colorGridHorizontal);
            SolidBrush brushEquatorial = new SolidBrush(colorGridEquatorial);
            SolidBrush brushEcliptic = new SolidBrush(colorLineEcliptic);

            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.LineSmooth);
            GL.Enable(EnableCap.PointSmooth);
            GL.Enable(EnableCap.LineStipple);
            GL.Enable(EnableCap.CullFace);

            if (!prj.FlipVertical ^ prj.FlipHorizontal)
            {
                GL.CullFace(CullFaceMode.Back);
            }
            else
            {
                GL.CullFace(CullFaceMode.Front);
            }

            if (settings.Get("GalacticEquator"))
            {
                DrawLine(prj, prj.MatEquatorialToVision * calc.MatGalactic, colorLineGalactic);
            }

            if (settings.Get("EclipticLine"))
            {
                var mat = prj.MatEquatorialToVision * calc.MatEcliptic;
                DrawLine(prj, mat, colorLineEcliptic);

                if (settings.Get("LabelEquinoxPoints"))
                {
                    DrawLabels(prj, i => Math.PI * i, i => 0, mat, equinoxLabels, fontEquinoxLabel, brushEcliptic);
                }

                if (settings.Get("LabelLunarNodes"))
                {
                    DrawLabels(prj, i => calc.LunarAscendingNodeLongitude + i * Math.PI, i => 0, mat, nodesLabels, fontNodeLabel, brushEcliptic);
                }
            }

            if (settings.Get("HorizontalGrid"))
            {
                DrawGridLines(prj, prj.MatHorizontalToVision, prj.VecHorizontalVision, colorGridHorizontal);

                if (settings.Get("LabelHorizontalPoles"))
                {
                    DrawLabels(prj, i => 0, i => Math.PI / 2 * (1 - 2 * i), prj.MatHorizontalToVision, horizontalLabels, SystemFonts.DefaultFont, brushHorizontal);
                }
            }

            if (settings.Get("EquatorialGrid"))
            {
                DrawGridLines(prj, prj.MatEquatorialToVision, prj.VecEquatorialVision, colorGridEquatorial);
            
                if (settings.Get("LabelEquatorialPoles"))
                {
                    DrawLabels(prj, i => 0, i => Math.PI / 2 * (1 - 2 * i), prj.MatEquatorialToVision, equatorialLabels, SystemFonts.DefaultFont, brushEquatorial);
                }
            }

            if (settings.Get("MeridianLine"))
            {
                DrawLine(prj, prj.MatHorizontalToVision * calc.MatMeridian, colorLineMeridian);
            }

            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.LineSmooth);
            GL.Disable(EnableCap.PointSmooth);
            GL.Disable(EnableCap.LineStipple);
            GL.Disable(EnableCap.CullFace);
        }

        private void DrawLabels(Projection prj, Func<int, double> lon, Func<int, double> lat, Mat4 mat, string[] labels, Font font, SolidBrush brush)
        {
            for (int i = 0; i < 2; i++)
            {
                Vec3 v = Projection.SphericalToCartesian(lon(i), lat(i));
                Vec2 p = prj.Project(v, mat);
                if (prj.IsInsideScreen(p))
                {
                    GL.Color3(brush.Color);
                    GL.PointSize(5);
                    GL.Begin(PrimitiveType.Points);
                    GL.Vertex2(p.X, p.Y);
                    GL.End();
                    textRenderer.Value.DrawString(labels[i], font, brush, new Vec2(p.X + 3, p.Y - 3));
                }
            }
        }

        private void DrawLine(Projection prj, Mat4 mat, Color color)
        {
            int segments = prj.Fov < 45 ? 128 : 64;

            GL.Color3(color);
            GL.LineStipple(1, 0xAAAA);

            GL.Begin(PrimitiveType.LineStrip);

            for (int i = 0; i <= segments; i++)
            {
                Vec3 v = Projection.SphericalToCartesian(Angle.ToRadians(i / (double)segments * 360), 0);
                var p = prj.Project(v, mat);
                if (p != null)
                {
                    GL.Vertex2(p.X, p.Y);
                }
                else
                {
                    GL.End();
                    GL.Begin(PrimitiveType.LineStrip);
                }
            }

            GL.End();
        }

        private void DrawGridLines(Projection prj, Mat4 mat, Vec3 vision, Color color)
        {
            int segments = prj.Fov < 45 ? 128 : 64;

            GL.Color3(color);
            GL.LineStipple(1, 0xAAAA);

            // HOR. GRID
            {
                // parallels
                for (int alt = -80; alt <= 80; alt += 10)
                {
                    GL.Begin(PrimitiveType.LineStrip);

                    for (int i = 0; i <= segments; i++)
                    {
                        Vec3 v = Projection.SphericalToCartesian(Angle.ToRadians(i / (double)segments * 360), Angle.ToRadians(alt));
                        var p = prj.Project(v, mat);

                        if (p != null)
                        {
                            GL.Vertex2(p.X, p.Y);
                        }
                        else
                        {
                            GL.End();
                            GL.Begin(PrimitiveType.LineStrip);
                        }
                    }

                    GL.End();
                }

                // meridians
                for (int i = 0; i < 24; i++)
                {
                    GL.Begin(PrimitiveType.LineStrip);

                    for (int alt = -80; alt <= 80; alt += 2)
                    {
                        Vec3 v = Projection.SphericalToCartesian(Angle.ToRadians(i / 24.0 * 360), Angle.ToRadians(alt));
                        var p = prj.Project(v, mat);
                        if (p != null)
                        {
                            GL.Vertex2(p.X, p.Y);
                        }
                        else
                        {
                            GL.End();
                            GL.Begin(PrimitiveType.LineStrip);
                        }
                    }

                    GL.End();
                }
            }
        }

        public override RendererOrder Order => RendererOrder.Grids;
    }
}
