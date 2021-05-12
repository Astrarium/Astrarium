using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Novae
{
    public class NovaeRenderer : BaseRenderer
    {
        public override RendererOrder Order => RendererOrder.Stars;

        private NovaeCalculator calc;
        private ISettings settings;

        private const int limitAllNames = 40;
        private Brush brushStarNames;

        public NovaeRenderer(NovaeCalculator calc, ISettings settings)
        {
            this.calc = calc;
            this.settings = settings;
        }

        public override void Render(IMapContext map)
        {
            Graphics g = map.Graphics;
            bool isGround = settings.Get<bool>("Ground");
            bool showNovae = settings.Get("Stars") && settings.Get<bool>("Novae");

            if (!showNovae) return;

            var novae = calc.Novae.Where(m => Angle.Separation(map.Center, m.Horizontal) < map.ViewAngle);
            if (isGround)
            {
                novae = novae.Where(m => m.Horizontal.Altitude >= 0);
            }

            var font = SystemFonts.DefaultFont;
            var brush = new SolidBrush(map.Schema == ColorSchema.White ? Color.Black : Color.White);

            foreach (var star in novae)
            {
                float diam = map.GetPointSize(star.Mag);
                if ((int)diam > 0)
                {
                    PointF p = map.Project(star.Horizontal);
                    if (!map.IsOutOfScreen(p))
                    {
                        if (map.Schema == ColorSchema.White)
                        {
                            g.FillEllipse(Brushes.White, p.X - diam / 2 - 1, p.Y - diam / 2 - 1, diam + 2, diam + 2);
                        }

                        g.FillEllipse(brush, p.X - diam / 2, p.Y - diam / 2, diam, diam);

                        map.AddDrawnObject(star);
                    }
                }
            }

            if (settings.Get<bool>("StarsLabels") && settings.Get<bool>("NovaeLabels") && map.ViewAngle <= limitAllNames)
            {
                brushStarNames = new SolidBrush(map.GetColor("ColorStarsLabels"));

                foreach (var nova in novae)
                {
                    float diam = map.GetPointSize(nova.Mag);
                    if ((int)diam > 0)
                    {
                        PointF p = map.Project(nova.Horizontal);
                        if (!map.IsOutOfScreen(p))
                        {
                            DrawStarName(map, p, nova, diam);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Draws nova name
        /// </summary>
        private void DrawStarName(IMapContext map, PointF point, Nova nova, float diam)
        {
            var fontStarNames = settings.Get<Font>("StarsLabelsFont");

            // Star has proper name
            if (map.ViewAngle < limitAllNames && settings.Get<bool>("StarsProperNames") && nova.ProperName != null)
            {
                map.DrawObjectCaption(fontStarNames, brushStarNames, nova.ProperName, point, diam);
                return;
            }
        }
    }
}
