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

namespace Astrarium.Plugins.DeepSky
{
    public class DeepSkyRenderer : BaseRenderer
    {
        private int limitLabels = 20;
        private double minAlpha = 10;
        private double maxAlpha = 255;
        private double minZoom = 50;
        private double maxZoom = 0.1;

        private double k;
        private double b;

        private Pen penOutlineDashed;
        private Pen penOutlineSolid;

        private Dictionary<DeepSkyStatus, IDrawingStrategy> drawingHandlers = null;

        private readonly DeepSkyCalc deepSkyCalc;
        private readonly ISettings settings;
        private readonly ITextureManager textureManager;

        private readonly string basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        private readonly Lazy<TextRenderer> textRenderer = new Lazy<TextRenderer>(() => new TextRenderer(128, 32));

        public DeepSkyRenderer(DeepSkyCalc deepSkyCalc, ITextureManager textureManager, ISettings settings)
        {
            this.deepSkyCalc = deepSkyCalc;
            this.textureManager = textureManager;
            this.settings = settings;

            k = -(minAlpha - maxAlpha) / (maxZoom - minZoom);
            b = -(minZoom * maxAlpha - maxZoom * minAlpha) / (maxZoom - minZoom);

            drawingHandlers = new Dictionary<DeepSkyStatus, IDrawingStrategy>()
            {
                { DeepSkyStatus.Galaxy, new GalaxyDrawingStrategy(this) },
                { DeepSkyStatus.GalacticNebula, new GalacticNebulaDrawingStrategy(this) },
                { DeepSkyStatus.PlanetaryNebula, new PlanetaryNebulaDrawingStrategy(this) },
                { DeepSkyStatus.OpenCluster, new ClusterDrawingStrategy(this) },
                { DeepSkyStatus.GlobularCluster, new ClusterDrawingStrategy(this) },
                { DeepSkyStatus.PartOfOther, new EmptyDrawingStrategy(this) },
                { DeepSkyStatus.Duplicate, new EmptyDrawingStrategy(this) },
                { DeepSkyStatus.DuplicateIC, new EmptyDrawingStrategy(this) },
                { DeepSkyStatus.Star, new EmptyDrawingStrategy(this) },
                { DeepSkyStatus.NotFound, new EmptyDrawingStrategy(this) }
            };
        }

        public override void Render(ISkyMap map)
        {
            if (!settings.Get<bool>("DeepSky")) return;
            if (map.DaylightFactor == 1) return;
            bool drawLabels = settings.Get("DeepSkyLabels");
            bool drawOutlines = settings.Get("DeepSkyOutlines");
            var schema = settings.Get<ColorSchema>("Schema");
            Color colorOutline = settings.Get<Color>("ColorDeepSkyOutline").Tint(schema);
            Color colorLabel = settings.Get<Color>("ColorDeepSkyLabel").Tint(schema);
            Font fontLabel = settings.Get<Font>("DeepSkyLabelsFont");
            string imagesPath = settings.Get<string>("DeepSkyImagesFolder");
            bool drawImages = settings.Get("DeepSkyImages") && Directory.Exists(imagesPath);
            Brush brushLabel = new SolidBrush(colorLabel);
            var prj = map.SkyProjection;

            // J2000 equatorial coordinates of screen center
            CrdsEquatorial eq = Precession.GetEquatorialCoordinates(prj.CenterEquatorial, deepSkyCalc.PrecessionalElements0);

            // real circular FOV with respect of screen borders
            double fov = prj.Fov * Math.Max(prj.ScreenWidth, prj.ScreenHeight) / Math.Min(prj.ScreenWidth, prj.ScreenHeight);

            // filter deep skies by:
            var deepSkies =
                // take existing objects only (obviously, do not draw objects that are catalog errors)
                deepSkyCalc.deepSkies.Where(ds => !ds.Status.IsEmpty() &&                
                // do not draw small objects for current FOV
                prj.GetDiskSize(ds.Semidiameter) > 10 &&
                // do not draw dim objects (exceeding mag limit for current FOV)
                ((float.IsNaN(ds.Magnitude) ? 6 : ds.Magnitude) <= prj.MagLimit) &&
                // do not draw object outside current FOV
                Angle.Separation(eq, ds.Equatorial0) < fov + ds.Semidiameter / 3600 * 2).ToList();

            // matrix for projection, with respect of precession
            var mat = prj.MatEquatorialToVision * deepSkyCalc.MatPrecession;

            foreach (var ds in deepSkies)
            {
                ds.Equatorial = prj.Context.Get(deepSkyCalc.Equatorial, ds);

                var p = prj.Project(ds.Equatorial);
                float sz = prj.GetDiskSize(ds.Semidiameter);

                if (drawImages)
                {
                    string path = Path.Combine(imagesPath, $"{ds.CatalogName}.jpg");

                    if (File.Exists(path))
                    {
                        int textureId = textureManager.GetTexture(path);

                        if (textureId > 0)
                        {
                            GL.Enable(EnableCap.Texture2D);
                            GL.Enable(EnableCap.Blend);
                            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

                            GL.BindTexture(TextureTarget.Texture2D, textureId);
                            GL.Begin(PrimitiveType.TriangleFan);

                            GL.TexCoord2(0.5, 0.5);
                            GL.Color4(Color.FromArgb(100, 255, 255, 255).Tint(schema));
                            GL.Vertex2(p.X, p.Y);

                            double sd = ds.Semidiameter / 3600 * 2;
                            double sdRA = sd / Math.Cos(Angle.ToRadians(ds.Equatorial.Delta));

                            Vec2 p0 = prj.Project(ds.Equatorial + new CrdsEquatorial(sdRA, sd));

                            GL.TexCoord2(0, 0);
                            GL.Color4(Color.FromArgb(0, 0, 0, 0));
                            GL.Vertex2(p0.X, p0.Y);

                            Vec2 p1 = prj.Project(ds.Equatorial + new CrdsEquatorial(sdRA, -sd));
                            GL.TexCoord2(0, 1);
                            GL.Color4(Color.FromArgb(0, 0, 0, 0));
                            GL.Vertex2(p1.X, p1.Y);

                            Vec2 p2 = prj.Project(ds.Equatorial + new CrdsEquatorial(-sdRA, -sd));
                            GL.TexCoord2(1, 1);
                            GL.Color4(Color.FromArgb(0, 0, 0, 0));
                            GL.Vertex2(p2.X, p2.Y);

                            Vec2 p3 = prj.Project(ds.Equatorial + new CrdsEquatorial(-sdRA, sd));
                            GL.TexCoord2(1, 0);
                            GL.Color4(Color.FromArgb(0, 0, 0, 0));
                            GL.Vertex2(p3.X, p3.Y);

                            GL.TexCoord2(0, 0);
                            GL.Color4(Color.FromArgb(0, 0, 0, 0));
                            GL.Vertex2(p0.X, p0.Y);

                            GL.End();

                            GL.Disable(EnableCap.Texture2D);
                            GL.Disable(EnableCap.Blend);
                        }
                    }
                }

                if (drawOutlines && ds.Outline != null)
                {
                    GL.Enable(EnableCap.Blend);
                    GL.Enable(EnableCap.LineSmooth);
                    GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
                    GL.Color4(colorOutline);
                    GL.Begin(PrimitiveType.LineLoop);

                    foreach (Vec3 ov in ds.Outline)
                    {
                        Vec2 op = prj.Project(ov, mat);
                        GL.Vertex2(op.X, op.Y);
                    }

                    GL.End();

                    if (drawLabels && sz > 20)
                    {
                        map.DrawObjectLabel(textRenderer.Value, ds.Names.First(), fontLabel, brushLabel, prj.Project(ds.Outline.First(), mat), 5);
                    }
                }
                else
                {
                    if (ds.Status == DeepSkyStatus.Galaxy)
                    {
                        float rx = ds.LargeDiameter.HasValue ? prj.GetDiskSize(ds.LargeDiameter.Value / 2 * 60) / 2 : 0;
                        float ry = ds.SmallDiameter.HasValue ? prj.GetDiskSize(ds.SmallDiameter.Value / 2 * 60) / 2 : 0;
                        double rot = ds.PA.HasValue ? prj.GetAxisRotation(ds.Equatorial, 90 + ds.PA.Value) : 0;
                        Pen pen = new Pen(colorOutline);
                        Primitives.DrawEllipse(p, pen, rx, ry, rot);
                    }
                    else
                    {
                        Pen pen = new Pen(colorOutline);
                        float r = prj.GetDiskSize(ds.Semidiameter, 4) / 2;
                        Primitives.DrawEllipse(p, pen, r);
                    }

                    if (drawLabels && sz > 20)
                    {
                        map.DrawObjectLabel(textRenderer.Value, ds.Names.First(), fontLabel, brushLabel, p, sz);
                    }
                }

                map.AddDrawnObject(p, ds, sz);
            }
        }

        public override void Render(IMapContext map)
        {
            if (!settings.Get<bool>("DeepSky"))
            {
                return;
            }

            var allDeepSkies = deepSkyCalc.deepSkies;
            bool isGround = settings.Get<bool>("Ground");
            //brushCaption = new SolidBrush(map.GetColor("ColorDeepSkyLabel"));

            int alpha = Math.Max(0, Math.Min((int)(k * map.ViewAngle + b), 255));
            
            Color colorOutline = map.GetColor("ColorDeepSkyOutline");
            penOutlineSolid = new Pen(Color.FromArgb(alpha, colorOutline));
            penOutlineDashed = new Pen(Color.FromArgb(alpha, colorOutline));
            penOutlineDashed.DashStyle = DashStyle.Dash;
            
            var deepSkies = allDeepSkies.Where(ds => !ds.Status.IsEmpty() && Angle.Separation(map.Center, ds.Horizontal) < map.ViewAngle);
            if (isGround)
            {
                deepSkies = deepSkies.Where(ds => ds.Horizontal.Altitude + ds.Semidiameter / 3600 > 0);
            }

            foreach (var ds in deepSkies)
            {
                drawingHandlers[ds.Status].Draw(map, settings, ds);
            }
        }

        private interface IDrawingStrategy
        {
            void Draw(IMapContext map, ISettings settings, DeepSky ds);
        }

        private class GalaxyDrawingStrategy : BaseDrawingStrategy
        {
            public GalaxyDrawingStrategy(DeepSkyRenderer renderer) : base(renderer) { }

            protected override void DrawEllipticObject(Graphics g, float diamA, float diamB)
            {
                g.DrawEllipse(Renderer.penOutlineSolid, -diamA / 2, -diamB / 2, diamA, diamB);
            }

            protected override void DrawPointObject(Graphics g, float size)
            {
                g.FillEllipse(Renderer.penOutlineSolid.Brush, -size / 2, - size / 2, size, size);
            }

            protected override void DrawRoundObject(Graphics g, float diamA)
            {
                g.DrawEllipse(Renderer.penOutlineSolid, -diamA / 2, -diamA / 2, diamA, diamA);
            }
        }

        private class GalacticNebulaDrawingStrategy : BaseDrawingStrategy
        {
            public GalacticNebulaDrawingStrategy(DeepSkyRenderer renderer) : base(renderer) { }

            public override void Draw(IMapContext map, ISettings settings, DeepSky ds)
            {
                if (map.ViewAngle <= Renderer.minZoom)
                {
                    base.Draw(map, settings, ds);
                }
            }

            protected override void DrawEllipticObject(Graphics g, float diamA, float diamB)
            {
                g.DrawRoundedRectangle(Renderer.penOutlineSolid, -diamA / 2, -diamB / 2, diamA, diamB, Math.Min(diamA, diamB) / 3);
            }

            protected override void DrawPointObject(Graphics g, float size)
            {
                g.FillEllipse(Renderer.penOutlineSolid.Brush, -size / 2, -size / 2, size, size);
            }

            protected override void DrawRoundObject(Graphics g, float diamA)
            {
                diamA /= 1.15f;
                g.DrawRoundedRectangle(Renderer.penOutlineSolid, -diamA / 2, -diamA / 2, diamA, diamA, diamA / 3);
            }
        }

        private class PlanetaryNebulaDrawingStrategy : BaseDrawingStrategy
        {
            public PlanetaryNebulaDrawingStrategy(DeepSkyRenderer renderer) : base(renderer) { }

            public override void Draw(IMapContext map, ISettings settings, DeepSky ds)
            {
                if (map.ViewAngle <= Renderer.minZoom)
                {
                    base.Draw(map, settings, ds);
                }
            }

            protected override void DrawEllipticObject(Graphics g, float diamA, float diamB)
            {
                g.DrawEllipse(Renderer.penOutlineSolid, -diamA / 2, -diamB / 2, diamA, diamB);
            }

            protected override void DrawPointObject(Graphics g, float size)
            {
                g.FillEllipse(Renderer.penOutlineSolid.Brush, -size / 2, -size / 2, size, size);
            }

            protected override void DrawRoundObject(Graphics g, float diamA)
            {
                g.DrawEllipse(Renderer.penOutlineSolid, -diamA / 2, -diamA / 2, diamA, diamA);
            }
        }

        private class ClusterDrawingStrategy : BaseDrawingStrategy
        {
            public ClusterDrawingStrategy(DeepSkyRenderer renderer) : base(renderer) { }

            public override void Draw(IMapContext map, ISettings settings, DeepSky ds)
            {
                if (map.ViewAngle <= Renderer.minZoom)
                {
                    base.Draw(map, settings, ds);
                }
            }

            protected override void DrawEllipticObject(Graphics g, float diamA, float diamB)
            {
                g.DrawEllipse(Renderer.penOutlineDashed, -diamA / 2, -diamB / 2, diamA, diamB);
            }

            protected override void DrawPointObject(Graphics g, float size)
            {
                g.FillEllipse(Renderer.penOutlineSolid.Brush, -size / 2, -size / 2, size, size);
            }

            protected override void DrawRoundObject(Graphics g, float diamA)
            {
                g.DrawEllipse(Renderer.penOutlineDashed, -diamA / 2, -diamA / 2, diamA, diamA);
            }
        }

        private class EmptyDrawingStrategy : IDrawingStrategy
        {
            protected DeepSkyRenderer Renderer { get; private set; }

            public EmptyDrawingStrategy(DeepSkyRenderer renderer)
            {
                Renderer = renderer;
            }

            public void Draw(IMapContext map, ISettings settings, DeepSky ds)
            {
                // Do nothing
            }
        }

        private abstract class BaseDrawingStrategy : IDrawingStrategy
        {
            protected DeepSkyRenderer Renderer { get; private set; }

            protected abstract void DrawEllipticObject(Graphics g, float diamA, float diamB);
            protected abstract void DrawRoundObject(Graphics g, float diamA);
            protected abstract void DrawPointObject(Graphics g, float size);

            public BaseDrawingStrategy(DeepSkyRenderer renderer)
            {
                Renderer = renderer;
            }

            public virtual void Draw(IMapContext map, ISettings settings, DeepSky ds)
            {
                PointF p = map.Project(ds.Horizontal);

                float sizeA = GetDiameter(map, ds.LargeDiameter);
                float sizeB = GetDiameter(map, ds.SmallDiameter);

                // elliptic object with known size
                if (sizeB > 0 && sizeB != sizeA)
                {
                    float diamA = GetDiameter(map, ds.LargeDiameter);
                    if (diamA > 10)
                    {
                        float diamB = GetDiameter(map, ds.SmallDiameter);
                        if (ds.Outline != null && settings.Get<bool>("DeepSkyOutlines"))
                        {
                            //DrawOutline(map, ds.Outline);
                        }
                        else
                        {
                            map.Rotate(p, ds.Equatorial, ds.PA ?? 0 + 90);
                            DrawEllipticObject(map.Graphics, diamA, diamB);
                            map.Graphics.ResetTransform();
                        }
                        map.AddDrawnObject(ds);

                        if (map.ViewAngle <= Renderer.limitLabels && settings.Get<bool>("DeepSkyLabels"))
                        {
                            var font = settings.Get<Font>("DeepSkyLabelsFont");
                            //map.DrawObjectCaption(font, Renderer.brushCaption, ds.DisplayName, p, Math.Min(diamA, diamB));
                        }
                    }
                }
                // round object
                else if (sizeA > 0)
                {
                    float diamA = GetDiameter(map, ds.LargeDiameter);
                    if (diamA > 10)
                    {
                        if (ds.Outline != null && settings.Get<bool>("DeepSkyOutlines"))
                        {
                            //DrawOutline(map, ds.Outline);
                        }
                        else
                        {
                            map.Rotate(p, ds.Equatorial, ds.PA ?? 0 + 90);
                            DrawRoundObject(map.Graphics, diamA);
                            map.Graphics.ResetTransform();
                        }
                        map.AddDrawnObject(ds);

                        if (map.ViewAngle <= Renderer.limitLabels && settings.Get<bool>("DeepSkyLabels"))
                        {
                            var font = settings.Get<Font>("DeepSkyLabelsFont");
                            //map.DrawObjectCaption(font, Renderer.brushCaption, ds.DisplayName, p, diamA);
                        }
                    }
                }
                // point object
                else
                {
                    float size = map.GetPointSize(ds.Magnitude == float.NaN ? 20 : ds.Magnitude);
                    if ((int)size > 0)
                    {
                        if (ds.Outline != null && settings.Get<bool>("DeepSkyOutlines"))
                        {
                            //DrawOutline(map, ds.Outline);
                        }
                        else
                        {
                            map.Graphics.TranslateTransform(p.X, p.Y);
                            DrawPointObject(map.Graphics, size);
                            map.Graphics.ResetTransform();
                        }
                        map.AddDrawnObject(ds);

                        if (map.ViewAngle <= Renderer.limitLabels && settings.Get<bool>("DeepSkyLabels"))
                        {
                            var font = settings.Get<Font>("DeepSkyLabelsFont");
                           // map.DrawObjectCaption(font, Renderer.brushCaption, ds.DisplayName, p, 0);
                        }
                    }
                }
            }

            protected virtual GraphicsPath DrawOutline(IMapContext map, ICollection<CelestialPoint> outline)
            {
                using (GraphicsPath gp = new GraphicsPath(FillMode.Winding))
                {
                    for (int i = 0; i < outline.Count - 1; i++)
                    {
                        var h1 = outline.ElementAt(i).Horizontal;
                        var h2 = outline.ElementAt(i + 1).Horizontal;

                        PointF p1, p2;
                        p1 = map.Project(h1);
                        p2 = map.Project(h2);
                        gp.AddLine(p1, p2);
                    }

                    map.Graphics.DrawPath(Renderer.penOutlineSolid, gp);

                    return gp;
                }
            }

            private float GetDiameter(IMapContext map, double? diam)
            {
                if (diam == null)
                {
                    return 0;
                }
                else
                {
                    double r = Math.Sqrt(map.Width * map.Width + map.Height * map.Height);
                    return (float)(diam / 60 / map.ViewAngle * r / 2);
                }
            }
        }

        public override RendererOrder Order => RendererOrder.DeepSpace;
    }
}
