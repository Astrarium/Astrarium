using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

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

        private int GetDeepSkyBrightness(ISkyMap map, DeepSky ds)
        {
            var prj = map.Projection;

            // extinction factor
            double ext = 1;
            if (prj.UseExtinction)
            {
                var alt = prj.ToHorizontal(ds.Equatorial).Altitude;
                float deltaMag = prj.GetMagExtinction(alt);
                ext = Math.Pow(10, deltaMag / -2.5);
            }

            // displayed magnitude
            double mag = float.IsNaN(ds.Magnitude) ? 12 : ds.Magnitude;

            // zoom level factor
            double zf = Math.Min(1, Math.Pow(10, (mag - prj.MagLimit) / -2.5));

            // daylight factor
            double df = Math.Pow(1 - map.DaylightFactor, 10);

            // displayed brightness, as byte
            return (int)(100 * zf * df * ext);
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
            bool hideOutline = settings.Get("DeepSkyHideOutline");
            Brush brushLabel = new SolidBrush(colorLabel);
            var prj = map.Projection;

            double fov = prj.RealFov;
            var eqCenter = prj.WithoutRefraction(prj.CenterEquatorial);

            // filter deep skies by:
            var deepSkies =
                // take existing objects only (obviously, do not draw objects that are catalog errors)
                deepSkyCalc.deepSkies.Where(ds => /*!ds.Status.IsEmpty() && */
                // do not draw small objects for current FOV
                (ds.Semidiameter == 0 || prj.GetDiskSize(ds.Semidiameter) > 20) &&
                // do not draw dim objects (exceeding mag limit for current FOV)
                ((float.IsNaN(ds.Magnitude) ? 10 : ds.Magnitude) <= prj.MagLimit) &&
                // do not draw object outside current FOV
                Angle.Separation(eqCenter, ds.Equatorial) < fov + ds.Semidiameter / 3600 * 2).ToList();

            foreach (var ds in deepSkies)
            {
                var p = prj.Project(ds.Equatorial);
                if (p == null) continue;

                float sz = prj.GetDiskSize(ds.Semidiameter);

                bool hasImage = false;

                if (drawImages)
                {
                    string path = Path.Combine(imagesPath, $"{ds.CatalogName}.jpg");
                    hasImage = File.Exists(path);

                    if (hasImage)
                    {
                        int textureId = GL.GetTexture(path);

                        if (textureId > 0)
                        {
                            int brightness = GetDeepSkyBrightness(map, ds);

                            if (brightness > 0)
                            {
                                Color centralColor = Color.FromArgb(brightness, 255, 255, 255).Tint(nightMode);
                                Color edgeColor = Color.FromArgb(0, 255, 255, 255).Tint(nightMode);

                                GL.Enable(GL.TEXTURE_2D);
                                GL.Enable(GL.BLEND);
                                GL.BlendFunc(GL.SRC_ALPHA, GL.ONE_MINUS_SRC_ALPHA);

                                GL.BindTexture(GL.TEXTURE_2D, textureId);
                                GL.Begin(GL.TRIANGLE_FAN);

                                GL.TexCoord2(0.5, 0.5);
                                GL.Color4(centralColor);
                                GL.Vertex2(p.X, p.Y);

                                // diameter in radians
                                double dia = Angle.ToRadians(ds.Semidiameter / 3600 * 2);

                                // R.A. of the deep sky object center, in radians
                                double ra0 = Angle.ToRadians(ds.Equatorial.Alpha);

                                // Dec. of the deep sky object center, in radians
                                double dec0 = Angle.ToRadians(ds.Equatorial.Delta);

                                double sinDia = Math.Sin(dia);
                                double cosDia = Math.Cos(dia);
                                double sinDec0 = Math.Sin(dec0);
                                double cosDec0 = Math.Cos(dec0);

                                const int steps = 16;
                                for (int i = 0; i <= steps; i++)
                                {
                                    // iterable angle, from 0 to 2*PI
                                    double a = (double)i / steps * Math.PI * 2;
                                    double sinA = Math.Sin(a);
                                    double cosA = Math.Cos(a);

                                    // texture coordinates
                                    double tx = 0.5 + 0.5 * sinA;
                                    double ty = 0.5 + 0.5 * cosA;

                                    // find R.A. and Dec. of the deep sky object' edge points
                                    // based on:
                                    // https://www.movable-type.co.uk/scripts/latlong.html
                                    // "Destination point given distance and bearing from start point"
                                    double dec = Math.Asin(sinDec0 * cosDia + cosDec0 * sinDia * Math.Cos(a + Math.PI));
                                    double ra = ra0 + Math.Atan2(Math.Sin(a + Math.PI) * sinDia * cosDec0, cosDia - sinDec0 * Math.Sin(dec));

                                    // on-screen coordinates of the edge point
                                    var pEdge = prj.Project(new CrdsEquatorial(Angle.ToDegrees(ra), Angle.ToDegrees(dec)));

                                    if (pEdge != null)
                                    {
                                        GL.TexCoord2(tx, ty);
                                        GL.Color4(edgeColor);
                                        GL.Vertex2(pEdge.X, pEdge.Y);
                                    }
                                }

                                GL.End();

                                GL.Disable(GL.TEXTURE_2D);
                                GL.Disable(GL.BLEND);
                            }
                        }
                    }
                }

                if (!(hasImage && hideOutline))
                {
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
                    }
                    else
                    {
                        if (ds.Semidiameter > 0)
                        {
                            float rx = prj.GetDiskSize(ds.LargeSemidiameter.GetValueOrDefault(ds.Semidiameter)) / 2;
                            float ry = prj.GetDiskSize(ds.SmallSemidiameter.GetValueOrDefault(ds.Semidiameter)) / 2;
                            double rot = prj.GetAxisRotation(ds.Equatorial, 90 + ds.PositionAngle.GetValueOrDefault());
                            GL.DrawEllipse(p, penOutline, rx, ry, rot);
                        }
                        else
                        {
                            GL.DrawLine(p + new Vec2(-4, -4), p + new Vec2(4, 4), penOutline);
                            GL.DrawLine(p + new Vec2(4, -4), p + new Vec2(-4, 4), penOutline);
                        }
                    }
                }

                if (drawLabels && (sz > 20 || fov < 1))
                {
                    map.DrawObjectLabel(ds.Names.First(), fontLabel, brushLabel, p, sz);
                }

                map.AddDrawnObject(p, ds);
            }
        }

        public override RendererOrder Order => RendererOrder.DeepSpace;
    }
}
