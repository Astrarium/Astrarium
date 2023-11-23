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
        private readonly DeepSkyCalc deepSkyCalc;
        private readonly ISettings settings;
        private readonly ITextureManager textureManager;

        private readonly Lazy<TextRenderer> textRenderer = new Lazy<TextRenderer>(() => new TextRenderer(256, 32));

        public DeepSkyRenderer(DeepSkyCalc deepSkyCalc, ITextureManager textureManager, ISettings settings)
        {
            this.deepSkyCalc = deepSkyCalc;
            this.textureManager = textureManager;
            this.settings = settings;
        }

        public override void Render(ISkyMap map)
        {
            if (!settings.Get<bool>("DeepSky")) return;
            if (map.DaylightFactor == 1) return;
            bool drawLabels = settings.Get("DeepSkyLabels");
            bool drawOutlines = settings.Get("DeepSkyOutlines");
            var schema = settings.Get<ColorSchema>("Schema");
            Color colorOutline = settings.Get<Color>("ColorDeepSkyOutline").Tint(schema);
            Pen penOutline = new Pen(colorOutline);
            Color colorLabel = settings.Get<Color>("ColorDeepSkyLabel").Tint(schema);
            Font fontLabel = settings.Get<Font>("DeepSkyLabelsFont");
            string imagesPath = settings.Get<string>("DeepSkyImagesFolder");
            bool drawImages = settings.Get("DeepSkyImages") && Directory.Exists(imagesPath);
            Brush brushLabel = new SolidBrush(colorLabel);
            var prj = map.Projection;

            // real circular FOV with respect of screen borders

            double w = Math.Max(prj.ScreenWidth, prj.ScreenHeight) / (double)Math.Min(prj.ScreenWidth, prj.ScreenHeight);
            double h = Math.Min(prj.ScreenWidth, prj.ScreenHeight) / (double)Math.Min(prj.ScreenWidth, prj.ScreenHeight);
            double fov = prj.Fov * Math.Sqrt(h * h + w * w) / 2;

            // filter deep skies by:
            var deepSkies =
                // take existing objects only (obviously, do not draw objects that are catalog errors)
                deepSkyCalc.deepSkies.Where(ds => !ds.Status.IsEmpty() &&
                // do not draw small objects for current FOV
                prj.GetDiskSize(ds.Semidiameter) > 20 &&
                // do not draw dim objects (exceeding mag limit for current FOV)
                ((float.IsNaN(ds.Magnitude) ? 6 : ds.Magnitude) <= prj.MagLimit) &&
                // do not draw object outside current FOV
                Angle.Separation(prj.CenterEquatorial, ds.Equatorial) < fov + ds.Semidiameter / 3600 * 2).ToList();

            foreach (var ds in deepSkies)
            {
                var p = prj.Project(ds.Equatorial);
                if (p == null) continue;

                float sz = prj.GetDiskSize(ds.Semidiameter);

                if (drawImages)
                {
                    string path = Path.Combine(imagesPath, $"{ds.CatalogName}.jpg");

                    if (File.Exists(path))
                    {
                        int textureId = textureManager.GetTexture(path);

                        if (textureId > 0)
                        {
                            double sd = ds.Semidiameter / 3600 * 2;
                            double sdRA = sd / Math.Cos(Angle.ToRadians(ds.Equatorial.Delta));

                            Vec2 p0 = prj.Project(ds.Equatorial + new CrdsEquatorial(sdRA, sd));
                            Vec2 p1 = prj.Project(ds.Equatorial + new CrdsEquatorial(sdRA, -sd));
                            Vec2 p2 = prj.Project(ds.Equatorial + new CrdsEquatorial(-sdRA, -sd));
                            Vec2 p3 = prj.Project(ds.Equatorial + new CrdsEquatorial(-sdRA, sd));

                            if (p0 != null && p1 != null && p2 != null && p3 != null)
                            {
                                GL.Enable(EnableCap.Texture2D);
                                GL.Enable(EnableCap.Blend);
                                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

                                GL.BindTexture(TextureTarget.Texture2D, textureId);
                                GL.Begin(PrimitiveType.TriangleFan);

                                GL.TexCoord2(0.5, 0.5);
                                GL.Color4(Color.FromArgb(100, 255, 255, 255).Tint(schema));
                                GL.Vertex2(p.X, p.Y);

                                GL.TexCoord2(0, 0);
                                GL.Color4(Color.FromArgb(0, 0, 0, 0));
                                GL.Vertex2(p0.X, p0.Y);

                                GL.TexCoord2(0, 1);
                                GL.Color4(Color.FromArgb(0, 0, 0, 0));
                                GL.Vertex2(p1.X, p1.Y);

                                GL.TexCoord2(1, 1);
                                GL.Color4(Color.FromArgb(0, 0, 0, 0));
                                GL.Vertex2(p2.X, p2.Y);

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
                }

                if (drawOutlines && ds.Outline != null)
                {
                    GL.Enable(EnableCap.Blend);
                    GL.Enable(EnableCap.LineSmooth);
                    GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
                    GL.Color4(colorOutline);
                    GL.Begin(PrimitiveType.LineLoop);

                    foreach (var oc in ds.Outline)
                    {
                        Vec2 op = prj.Project(Precession.GetEquatorialCoordinates(oc, prj.Context.PrecessionElements));
                        if (op != null)
                        {
                            GL.Vertex2(op.X, op.Y);
                        }
                    }

                    GL.End();

                    if (drawLabels && sz > 20)
                    {
                        var p0 = prj.Project(Precession.GetEquatorialCoordinates(ds.Outline.First(), prj.Context.PrecessionElements));
                        if (p0 != null)
                        {
                            map.DrawObjectLabel(textRenderer.Value, ds.Names.First(), fontLabel, brushLabel, p0, 5);
                        }
                    }
                }
                else
                {
                    if (ds.Status == DeepSkyStatus.Galaxy)
                    {
                        float rx = ds.LargeDiameter.HasValue ? prj.GetDiskSize(ds.LargeDiameter.Value / 2 * 60) / 2 : 0;
                        float ry = ds.SmallDiameter.HasValue ? prj.GetDiskSize(ds.SmallDiameter.Value / 2 * 60) / 2 : 0;
                        double rot = ds.PA.HasValue ? prj.GetAxisRotation(ds.Equatorial, 90 + ds.PA.Value) : 0;
                        Primitives.DrawEllipse(p, penOutline, rx, ry, rot);
                    }
                    else
                    {
                        float r = prj.GetDiskSize(ds.Semidiameter, 4) / 2;
                        Primitives.DrawEllipse(p, penOutline, r);
                    }

                    if (drawLabels && sz > 20)
                    {
                        map.DrawObjectLabel(textRenderer.Value, ds.Names.First(), fontLabel, brushLabel, p, sz);
                    }
                }

                map.AddDrawnObject(p, ds, sz);
            }
        }

        public override RendererOrder Order => RendererOrder.DeepSpace;
    }
}
