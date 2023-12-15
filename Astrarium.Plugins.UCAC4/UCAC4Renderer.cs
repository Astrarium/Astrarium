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
            fontNames = new Font("Arial", 7);
        }

        private bool isDrag = false;
        public override void OnMouseMove(ISkyMap map, MouseButton mouseButton)
        {
            isDrag = mouseButton == MouseButton.Left;
        }

        public override void OnMouseUp(ISkyMap map, MouseButton mouseButton)
        {
            isDrag = false;
            map.Invalidate();
        }

        public override void Render(ISkyMap map)
        {
            float daylightFactor = map.DaylightFactor;

            // no stars if the Sun above horizon
            if (daylightFactor == 1) return;

            //if (isDrag) return;

            var prj = map.Projection;

            if (prj.Fov < 1.5 && settings.Get("Stars") && settings.Get("UCAC4"))
            {
                float starDimming = 1 - daylightFactor;
                float minStarSize = Math.Max(0.5f, daylightFactor * 3);

                bool nightMode = settings.Get("NightMode");
                bool isLabels = settings.Get("StarsLabels") && prj.Fov < 1 / 60d;
                Brush brushNames = new SolidBrush(settings.Get<Color>("ColorStarsLabels").Tint(nightMode));
                float starsScalingFactor = (float)settings.Get<decimal>("StarsScalingFactor", 1);

                GL.Enable(EnableCap.PointSmooth);
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest);

                float magLimit = prj.MagLimit;

                // J2000 equatorial coordinates of screen center
                CrdsEquatorial eq0 = Precession.GetEquatorialCoordinates(prj.WithoutRefraction(prj.CenterEquatorial), catalog.PrecessionElements0);

                var stars = catalog.GetStars(prj.Context, eq0, prj.Fov, m => m <= magLimit);

                foreach (var star in stars)
                {
                    float size = prj.GetPointSize(star.Magnitude) * starDimming;

                    if (size >= minStarSize)
                    {
                        var p = prj.Project(star.Equatorial);

                        if (prj.IsInsideScreen(p))
                        {
                            GL.PointSize(size * starsScalingFactor);
                            GL.Color3(GetColor(star.SpectralClass).Tint(nightMode));

                            GL.Begin(PrimitiveType.Points);
                            GL.Vertex2(p.X, p.Y);
                            GL.End();

                            map.AddDrawnObject(p, star, size);

                            if (isLabels && size > 2)
                            {
                                map.DrawObjectLabel(textRenderer.Value, star.Names.First(), fontNames, brushNames, p, size);
                            }
                        }
                    }
                }
            }
        }

        public override RendererOrder Order => RendererOrder.Stars;

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
