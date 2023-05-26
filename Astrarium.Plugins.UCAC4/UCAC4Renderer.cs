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

        public override void Render(ISkyMap map)
        {
            var prj = map.SkyProjection;
            if (prj.Fov < 1.5 && settings.Get("Stars") && settings.Get("UCAC4"))
            {
                float daylightFactor = map.DaylightFactor;

                // no stars if the Sun above horizon
                if (daylightFactor == 1) return;

                float starDimming = 1 - daylightFactor;

                float minStarSize = daylightFactor * 3; // empiric

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
                    float size = prj.GetPointSize(star.Magnitude) * starDimming;

                    if (size > minStarSize)
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

        [Obsolete]
        public override void Render(IMapContext map) { }

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
