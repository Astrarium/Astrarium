using Astrarium.Algorithms;
using Astrarium.Types;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.UCAC4
{
    public class UCAC4Renderer : BaseRenderer
    {
        private Font fontNames;
        private readonly UCAC4Catalog catalog;
        private readonly ISettings settings;
        private readonly Lazy<TextRenderer> textRenderer = new Lazy<TextRenderer>(() => new TextRenderer(128, 32));

        public UCAC4Renderer(UCAC4Catalog catalog, ISettings settings)
        {
            this.catalog = catalog;
            this.settings = settings;
            fontNames = new Font("Arial", 8);
        }

        public override void Render(ISkyMap map)
        {
            var prj = map.SkyProjection;
            if (prj.Fov < 1.5 && settings.Get("Stars") && settings.Get("UCAC4"))
            {
                bool isLabels = settings.Get("StarsLabels") && prj.Fov < 1 / 60d;
                Brush brushNames = new SolidBrush(settings.Get<SkyColor>("ColorStarsLabels").Night);

                GL.Enable(EnableCap.PointSmooth);
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest);

                float magLimit = prj.MagLimit;

                PrecessionalElements pe = Precession.ElementsFK5(prj.Context.JulianDay, Date.EPOCH_J2000);

                var eq0 = prj.CenterEquatorial;

                CrdsEquatorial eq = Precession.GetEquatorialCoordinates(eq0, pe);

                double t = prj.Context.Get(catalog.YearsSince2000);

                // matrix for projection, with respect of precession
                var mat = prj.MatEquatorialToVision * catalog.MatPrecession;

                // equatorial vision vector in J2000 coords
                var eqVision0 = catalog.MatPrecession0 * prj.VecEquatorialVision;

                var stars = catalog.GetStars(t, eq, prj.Fov, m => m <= magLimit);

                //catalog.LockedStar = map.LockedObject as UCAC4Star;
                //catalog.SelectedStar = map.SelectedObject as UCAC4Star;

                foreach (var star in stars)
                {
                    float size = prj.GetPointSize(star.Magnitude);

                    if (size > 1)
                    {
                        var p = prj.Project(star.Cartesian, mat);

                        if (prj.IsInsideScreen(p))
                        {
                            GL.PointSize(size);
                            GL.Color3(GetColor(star.SpectralClass));

                            GL.Begin(PrimitiveType.Points);
                            GL.Vertex2(p.X, p.Y);
                            GL.End();

                            map.AddDrawnObject(p, star);

                            if (isLabels && size > 2)
                            {
                                textRenderer.Value.DrawString(star.Names.First(), fontNames, brushNames, new PointF((float)p.X + size / 2, (float)p.Y - size / 2));
                            }
                        }
                    }
                }
            }
        }

        public override void Render(IMapContext map)
        {
            if (map.ViewAngle < 1.5 && settings.Get("Stars") && settings.Get("UCAC4"))
            {
                Graphics g = map.Graphics;
                bool isGround = settings.Get("Ground");
                bool isLabels = settings.Get("StarsLabels");
                Brush brushNames = new SolidBrush(map.GetColor("ColorStarsLabels"));
                float magLimit = (float)(-1.73494 * Math.Log(0.000462398 * map.ViewAngle));

                PrecessionalElements pe = Precession.ElementsFK5(map.JulianDay, Date.EPOCH_J2000);

                var eq0 = map.Center.ToEquatorial(map.GeoLocation, map.SiderealTime);

                CrdsEquatorial eq = Precession.GetEquatorialCoordinates(eq0, pe);

                SkyContext context = new SkyContext(map.JulianDay, map.GeoLocation);

                catalog.LockedStar = map.LockedObject as UCAC4Star;
                catalog.SelectedStar = map.SelectedObject as UCAC4Star;

                var stars = catalog.GetStars(0, eq, map.ViewAngle, m => m <= magLimit);

                foreach (var star in stars)
                {
                    if (!isGround || star.Horizontal.Altitude > 0)
                    {
                        PointF p = map.Project(star.Horizontal);
                        if (!map.IsOutOfScreen(p))
                        {
                            float size = map.GetPointSize(star.Magnitude);
                            if (size > 0)
                            {
                                if (map.Schema == ColorSchema.White)
                                {
                                    g.FillEllipse(Brushes.White, p.X - size / 2 - 1, p.Y - size / 2 - 1, size + 2, size + 2);
                                }
                                g.FillEllipse(new SolidBrush(GetColor(map, star.SpectralClass)), p.X - size / 2, p.Y - size / 2, size, size);

                                if (isLabels && map.ViewAngle < 1.0 / 60.0 && size > 2)
                                {
                                    map.DrawObjectCaption(fontNames, brushNames, star.Names.First(), p, size);
                                }
                                map.AddDrawnObject(star);
                            }
                        }
                    }
                }
            }
        }

        public override RendererOrder Order => RendererOrder.Stars;

        private Brush GetColor(IMapContext map)
        {
            switch (map.Schema)
            {
                default:
                case ColorSchema.Night:
                    return Brushes.White;
                case ColorSchema.Red:
                    return Brushes.DarkRed;
                case ColorSchema.White:
                    return Brushes.Black;
            }
        }

        private Color GetColor(IMapContext map, char spClass)
        {
            Color starColor;

            if (map.Schema == ColorSchema.White)
            {
                return Color.Black;
            }

            if (settings.Get("StarsColors"))
            {
                switch (spClass)
                {
                    case 'O':
                    case 'W':
                        starColor = Color.LightBlue;
                        break;
                    case 'B':
                        starColor = Color.LightCyan;
                        break;
                    case 'A':
                        starColor = Color.White;
                        break;
                    case 'F':
                        starColor = Color.LightYellow;
                        break;
                    case 'G':
                        starColor = Color.Yellow;
                        break;
                    case 'K':
                        starColor = Color.Orange;
                        break;
                    case 'M':
                        starColor = Color.OrangeRed;
                        break;
                    default:
                        starColor = Color.White;
                        break;
                }
            }
            else
            {
                starColor = Color.White;
            }

            return map.GetColor(starColor, Color.Transparent);
        }

        private Color GetColor(char spClass)
        {
            Color starColor;
            if (settings.Get("StarsColors"))
            {
                switch (spClass)
                {
                    case 'O':
                    case 'W':
                        starColor = Color.LightBlue;
                        break;
                    case 'B':
                        starColor = Color.LightCyan;
                        break;
                    case 'A':
                        starColor = Color.White;
                        break;
                    case 'F':
                        starColor = Color.LightYellow;
                        break;
                    case 'G':
                        starColor = Color.Yellow;
                        break;
                    case 'K':
                        starColor = Color.Orange;
                        break;
                    case 'M':
                        starColor = Color.OrangeRed;
                        break;
                    default:
                        starColor = Color.White;
                        break;
                }
            }
            else
            {
                starColor = Color.White;
            }

            return starColor;
        }
    }
}
