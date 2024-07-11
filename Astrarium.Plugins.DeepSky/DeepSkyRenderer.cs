using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Astrarium.Plugins.DeepSky
{
    public class DeepSkyRenderer : BaseRenderer
    {
        private readonly DeepSkyCalc deepSkyCalc;
        private readonly ISettings settings;

        public DeepSkyRenderer(DeepSkyCalc deepSkyCalc, ISettings settings)
        {
            this.deepSkyCalc = deepSkyCalc;
            this.settings = settings;
        }

        public override void Render(ISkyMap map)
        {
            if (!settings.Get<bool>("DeepSky")) return;
            if (map.DaylightFactor == 1) return;

            bool drawLabels = settings.Get("DeepSkyLabels");
            var nightMode = settings.Get("NightMode");
            Color colorOutline = settings.Get<Color>("ColorDeepSkyOutline").Tint(nightMode);
            Pen penOutline = new Pen(colorOutline);
            Color colorLabel = settings.Get<Color>("ColorDeepSkyLabel").Tint(nightMode);
            Font fontLabel = settings.Get<Font>("DeepSkyLabelsFont");
            string imagesPath = settings.Get<string>("DeepSkyImagesFolder");
            bool drawImages = settings.Get("DeepSkyImages") && Directory.Exists(imagesPath);
            Brush brushLabel = new SolidBrush(colorLabel);
            var prj = map.Projection;

            // real circular FOV with respect of screen borders

            double w = Math.Max(prj.ScreenWidth, prj.ScreenHeight) / (double)Math.Min(prj.ScreenWidth, prj.ScreenHeight);
            double h = Math.Min(prj.ScreenWidth, prj.ScreenHeight) / (double)Math.Min(prj.ScreenWidth, prj.ScreenHeight);
            double fov = prj.Fov * Math.Sqrt(h * h + w * w) / 2;
            var eqCenter = prj.WithoutRefraction(prj.CenterEquatorial);

            // filter deep skies by:
            var deepSkies =
                // take existing objects only (obviously, do not draw objects that are catalog errors)
                deepSkyCalc.deepSkies.Where(ds => !ds.Status.IsEmpty() &&
                // do not draw small objects for current FOV
                prj.GetDiskSize(ds.Semidiameter) > 20 &&
                // do not draw dim objects (exceeding mag limit for current FOV)
                ((float.IsNaN(ds.Magnitude) ? 6 : ds.Magnitude) <= prj.MagLimit) &&
                // do not draw object outside current FOV
                Angle.Separation(eqCenter, ds.Equatorial) < fov + ds.Semidiameter / 3600 * 2).ToList();

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
                        int textureId = GL.GetTexture(path);

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
                                GL.Enable(GL.TEXTURE_2D);
                                GL.Enable(GL.BLEND);
                                GL.BlendFunc(GL.SRC_ALPHA, GL.ONE_MINUS_SRC_ALPHA);

                                GL.BindTexture(GL.TEXTURE_2D, textureId);
                                GL.Begin(GL.TRIANGLE_FAN);

                                GL.TexCoord2(0.5, 0.5);
                                GL.Color4(Color.FromArgb((int)(100 * (1 - map.DaylightFactor)), 255, 255, 255).Tint(nightMode));
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

                                GL.Disable(GL.TEXTURE_2D);
                                GL.Disable(GL.BLEND);
                            }
                        }
                    }
                }

                if (ds.Shape != null && ds.Shape.Any())
                {
                    GL.Enable(GL.BLEND);
                    GL.Enable(GL.LINE_SMOOTH);
                    GL.Hint(GL.LINE_SMOOTH_HINT, GL.NICEST);
                    GL.Color4(colorOutline);
                    GL.Begin(GL.LINE_LOOP);

                    foreach (var oc in ds.Shape)
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
                        var p0 = prj.Project(Precession.GetEquatorialCoordinates(ds.Shape.First(), prj.Context.PrecessionElements));
                        if (p0 != null)
                        {
                            map.DrawObjectLabel(ds.Names.First(), fontLabel, brushLabel, p0, 5);
                        }
                    }
                }
                else
                {
                    float rx = prj.GetDiskSize(ds.LargeSemidiameter.GetValueOrDefault(ds.Semidiameter)) / 2;
                    float ry = prj.GetDiskSize(ds.SmallSemidiameter.GetValueOrDefault(ds.Semidiameter)) / 2;
                    double rot = prj.GetAxisRotation(ds.Equatorial, 90 + ds.PositionAngle.GetValueOrDefault());
                    GL.DrawEllipse(p, penOutline, rx, ry, rot);

                    if (drawLabels && sz > 20)
                    {
                        map.DrawObjectLabel(ds.Names.First(), fontLabel, brushLabel, p, sz);
                    }
                }

                map.AddDrawnObject(p, ds);
            }
        }

        public override RendererOrder Order => RendererOrder.DeepSpace;
    }
}
