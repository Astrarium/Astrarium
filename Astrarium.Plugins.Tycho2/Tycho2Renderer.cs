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

        public override RendererOrder Order => RendererOrder.Stars;

        public Tycho2Renderer(ITycho2Catalog tycho2, ISettings settings)
        {
            this.tycho2 = tycho2;
            this.settings = settings;
        }


        public override void Render(ISkyMap map)
        {
            Projection prj = map.Projection;
            if (prj.MagLimit > 8 && settings.Get("Stars") && settings.Get("Tycho2"))
            {
                float daylightFactor = map.DaylightFactor;

                // no stars if the Sun above horizon
                if (daylightFactor == 1) return;

                float starDimming = 1 - daylightFactor;
                float minStarSize = Math.Max(0.5f, daylightFactor * 3); // empiric

                var schema = settings.Get<ColorSchema>("Schema");
                bool isLabels = settings.Get<bool>("StarsLabels");
                float starsScalingFactor = (float)settings.Get<decimal>("StarsScalingFactor", 1);
                Brush brushNames = new SolidBrush(settings.Get<SkyColor>("ColorStarsLabels").Night.Tint(schema));

                GL.Enable(EnableCap.PointSmooth);
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest);

                // J2000 equatorial coordinates of screen center
                CrdsEquatorial eq = Precession.GetEquatorialCoordinates(prj.CenterEquatorial, tycho2.PrecessionalElements0);

                // years since initial catalogue epoch
                double t = prj.Context.Get(tycho2.YearsSince2000);

                float magLimit = prj.MagLimit;
                magLimit = (float)(-1.44995 * Math.Log(0.000230685 * prj.Fov));

                var stars = tycho2.GetStars(prj.Context, eq, prj.Fov, m => m <= magLimit);

                foreach (var star in stars)
                {
                    float size = prj.GetPointSize(star.Magnitude) * starDimming;

                    if (size >= minStarSize)
                    {
                        var p = prj.Project(star.Equatorial);

                        if (prj.IsInsideScreen(p))
                        {
                            GL.PointSize(size * starsScalingFactor);
                            GL.Color3(GetColor(star.SpectralClass).Tint(schema));

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
