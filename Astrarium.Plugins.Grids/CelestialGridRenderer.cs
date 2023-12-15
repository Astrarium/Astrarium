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
            var nightMode = settings.Get("NightMode");

            Color colorGridEquatorial = settings.Get<Color>("ColorEquatorialGrid").Tint(nightMode);
            Color colorGridHorizontal = settings.Get<Color>("ColorHorizontalGrid").Tint(nightMode);
            Color colorLineEcliptic = settings.Get<Color>("ColorEcliptic").Tint(nightMode);
            Color colorLineGalactic = settings.Get<Color>("ColorGalacticEquator").Tint(nightMode);
            Color colorLineMeridian = settings.Get<Color>("ColorMeridian").Tint(nightMode);

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

            // TODO: refactor below code in order to support refraction

            if (settings.Get("GalacticEquator"))
            {
                int segments = prj.Fov < 45 ? 128 : 64;
                CrdsGalactical gal = new CrdsGalactical(0, 0);
                Func<int, Vec2> project = (int i) =>
                {
                    gal.l = (double)i / segments * 360;
                    var eq1950 = gal.ToEquatorial();
                    var eq = Precession.GetEquatorialCoordinates(eq1950, calc.PrecessionalElementsB1950ToCurrent);
                    return prj.Project(eq);
                };

                DrawLine(prj, colorLineGalactic, segments, project);
            }

            if (settings.Get("EclipticLine"))
            {
                int segments = prj.Fov < 45 ? 128 : 64;
                CrdsEcliptical ecl = new CrdsEcliptical(0, 0);
                Func<int, Vec2> project = (int i) =>
                {
                    ecl.Lambda = (double)i / segments * 360;
                    return prj.Project(ecl.ToEquatorial(prj.Context.Epsilon));
                };

                DrawLine(prj, colorLineEcliptic, segments, project);

                if (settings.Get("LabelEquinoxPoints"))
                {
                    // DrawLabels(prj, i => Math.PI * i, i => 0, mat, equinoxLabels, fontEquinoxLabel, brushEcliptic);
                }

                if (settings.Get("LabelLunarNodes"))
                {
                    // DrawLabels(prj, i => calc.LunarAscendingNodeLongitude + i * Math.PI, i => 0, mat, nodesLabels, fontNodeLabel, brushEcliptic);
                }
            }

            if (settings.Get("HorizontalGrid"))
            {
                DrawGridLines(prj, prj.MatHorizontalToVision, colorGridHorizontal);

                if (settings.Get("LabelHorizontalPoles"))
                {
                    DrawLabels(prj, i => 0, i => Math.PI / 2 * (1 - 2 * i), prj.MatHorizontalToVision, horizontalLabels, SystemFonts.DefaultFont, brushHorizontal);
                }
            }

            if (settings.Get("EquatorialGrid"))
            {
                CrdsEquatorial eq = new CrdsEquatorial();
                Func<double, double, Vec2> project = (double lon, double lat) =>
                {
                    eq.Alpha = lon;
                    eq.Delta = lat;
                    return prj.Project(eq);
                };

                DrawGrid(prj, colorGridEquatorial, project);

                //DrawGridLines(prj, prj.MatEquatorialToVision, colorGridEquatorial);

                //if (settings.Get("LabelEquatorialPoles"))
                //{
                //    DrawLabels(prj, i => 0, i => Math.PI / 2 * (1 - 2 * i), prj.MatEquatorialToVision, equatorialLabels, SystemFonts.DefaultFont, brushEquatorial);
                //}
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

        private void DrawLine(Projection prj, Color color, int segments, Func<int, Vec2> projectPoint)
        {
            GL.Color3(color);
            GL.LineStipple(1, 0xAAAA);

            GL.Begin(PrimitiveType.LineStrip);

            for (int i = 0; i <= segments; i++)
            {
                var p = projectPoint(i);

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

        private void DrawGrid(Projection prj, Color color, Func<double, double, Vec2> projectPoint)
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
                        double lon = i / (double)segments * 360;
                        double lat = alt;

                        var p = projectPoint(lon, lat);

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
                        double lon = i / 24.0 * 360;
                        double lat = alt;

                        var p = projectPoint(lon, lat);
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

        private void DrawGridLines(Projection prj, Mat4 mat, Color color)
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
