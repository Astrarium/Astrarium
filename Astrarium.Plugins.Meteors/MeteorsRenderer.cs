using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Meteors
{
    public class MeteorsRenderer : BaseRenderer
    {
        public override RendererOrder Order => RendererOrder.DeepSpace;

        private MeteorsCalculator calc;
        private ISettings settings;

        public MeteorsRenderer(MeteorsCalculator calc, ISettings settings)
        {
            this.calc = calc;
            this.settings = settings;
        }

        public override void Render(IMapContext map)
        {
            Graphics g = map.Graphics;
            bool isGround = settings.Get<bool>("Ground");
            bool showMeteors = settings.Get("Meteors");
            bool onlyActive = settings.Get("MeteorsOnlyActive");
            bool showLabels = settings.Get("MeteorsLabels");
            int activityClassLimit = 4;

            if (!showMeteors) return;

            var meteors = calc.Meteors.Where(m => Angle.Separation(map.Center, m.Horizontal) < map.ViewAngle);
            if (isGround)
            {
                meteors = meteors.Where(m => m.Horizontal.Altitude >= 0);
            }

            if (onlyActive)
            {
                meteors = meteors.Where(m => m.IsActive);
            }

            meteors = meteors.Where(m => m.ActivityClass <= activityClassLimit);

            var color = map.GetColor("ColorMeteors");
            var pen = new Pen(color);
            var brush = new SolidBrush(color);
            var font = SystemFonts.DefaultFont;

            foreach (var meteor in meteors)
            {
                PointF p = map.Project(meteor.Horizontal);
                if (!map.IsOutOfScreen(p))
                {
                    g.DrawXCross(pen, p, 5);
                    map.AddDrawnObject(meteor);

                    if (showLabels)
                    {
                        map.DrawObjectCaption(font, brush, meteor.Name, p, 10);
                    }
                }
            }
        }
    }
}
