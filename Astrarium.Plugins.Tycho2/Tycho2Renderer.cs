using Astrarium.Algorithms;
using Astrarium.Types;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Tycho2
{
    public class Tycho2Renderer : BaseRenderer
    {
        private readonly ITycho2Catalog tycho2;
        private readonly ISettings settings;

        private readonly Font fontNames = new Font("Arial", 7);
        private readonly Lazy<TextRenderer> textRenderer = new Lazy<TextRenderer>(() => new TextRenderer(128, 32));

        public Tycho2Renderer(ITycho2Catalog tycho2, ISettings settings)
        {
            this.tycho2 = tycho2;
            this.settings = settings;
        }

        public override void Render(ISkyMap map)
        {
            Projection prj = map.SkyProjection;
            if (prj.MagLimit > 8 && settings.Get("Stars") && settings.Get("Tycho2"))
            {
                float daylightFactor = map.DaylightFactor;

                // no stars if the Sun above horizon
                if (daylightFactor == 1) return;

                float starDimming = 1 - daylightFactor;
                float minStarSize = Math.Max(0.5f, daylightFactor * 3); // empiric

                bool isLabels = settings.Get<bool>("StarsLabels");
                Brush brushNames = new SolidBrush(settings.Get<SkyColor>("ColorStarsLabels").Night);

                GL.Enable(EnableCap.PointSmooth);
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest);

                // J2000 equatorial coordinates of screen center
                CrdsEquatorial eq = Precession.GetEquatorialCoordinates(prj.CenterEquatorial, tycho2.PrecessionalElements0);

                // years since initial catalogue epoch
                double t = prj.Context.Get(tycho2.YearsSince2000);

                // matrix for projection, with respect of precession
                var mat = prj.MatEquatorialToVision * tycho2.MatPrecession;

                float magLimit = prj.MagLimit;
                magLimit = (float)(-1.44995 * Math.Log(0.000230685 * prj.Fov));

                var stars = tycho2.GetStars(t, eq, prj.Fov, m => m <= magLimit);

                foreach (var star in stars)
                {
                    float size = prj.GetPointSize(star.Magnitude) * starDimming;

                    if (size >= minStarSize)
                    {
                        var p = prj.Project(star.Cartesian, mat);

                        if (prj.IsInsideScreen(p))
                        {
                            GL.PointSize(size);
                            GL.Color3(GetColor(star.SpectralClass));

                            GL.Begin(PrimitiveType.Points);
                            GL.Vertex2(p.X, p.Y);
                            GL.End();

                            map.AddDrawnObject(p, star, size);

                            if (isLabels && prj.Fov < 1 && size > 3)
                            {
                                map.DrawObjectLabel(textRenderer.Value, star.Names.First(), fontNames, brushNames, p, size);
                            }
                        }
                    }
                }
            }

            
        }

        public override void Render(IMapContext map)
        {

            Graphics g = map.Graphics;
            bool isGround = settings.Get<bool>("Ground");
            bool isLabels = settings.Get<bool>("StarsLabels");
            Brush brushNames = new SolidBrush(map.GetColor("ColorStarsLabels"));

            if (map.MagLimit > 8 && settings.Get<bool>("Stars") && settings.Get<bool>("Tycho2"))
            {
                PrecessionalElements pe = Precession.ElementsFK5(map.JulianDay, Date.EPOCH_J2000);

                var eq0 = map.Center.ToEquatorial(map.GeoLocation, map.SiderealTime);

                CrdsEquatorial eq = Precession.GetEquatorialCoordinates(eq0, pe);

                SkyContext context = new SkyContext(map.JulianDay, map.GeoLocation);

                tycho2.LockedStar = map.LockedObject as Tycho2Star;
                tycho2.SelectedStar = map.SelectedObject as Tycho2Star;

                double t = context.Get(tycho2.YearsSince2000);
                float magLimit = (float)(-1.44995 * Math.Log(0.000230685 * map.ViewAngle));
                var stars = tycho2.GetStars(t, eq, map.ViewAngle, m => m <= magLimit);

                foreach (var star in stars)
                {
                    if (!isGround || star.Horizontal.Altitude > 0)
                    {
                        float size = map.GetPointSize(star.Magnitude);
                        if (size > 0)
                        {
                            PointF p = map.Project(star.Horizontal);
                            if (!map.IsOutOfScreen(p))
                            {
                                Brush brushStar = new SolidBrush(GetColor(map, star.SpectralClass));

                                if (map.Schema == ColorSchema.White)
                                {
                                    g.FillEllipse(Brushes.White, p.X - size / 2 - 1, p.Y - size / 2 - 1, size + 2, size + 2);
                                }
                                g.FillEllipse(brushStar, p.X - size / 2, p.Y - size / 2, size, size);

                                if (isLabels && map.ViewAngle < 1 && size > 3)
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
