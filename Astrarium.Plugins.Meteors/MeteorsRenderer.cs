﻿using Astrarium.Algorithms;
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

        private readonly MeteorsCalculator calc;
        private readonly ISettings settings;

        public MeteorsRenderer(MeteorsCalculator calc, ISettings settings)
        {
            this.calc = calc;
            this.settings = settings;
        }

        public override void Render(ISkyMap map)
        {
            if (!settings.Get("Meteors")) return;
            if (map.DaylightFactor == 1) return;

            var prj = map.Projection;
            var nightMode = settings.Get("NightMode");
            bool onlyActive = settings.Get("MeteorsOnlyActive");
            bool showLabels = settings.Get("MeteorsLabels");
            int activityClassLimit = (int)settings.Get("MeteorsActivityClassLimit", MeteorActivityClass.IV);
            var labelsType = settings.Get<MeteorLabelType>("MeteorsLabelsType");
            double fov = prj.RealFov;

            var meteors = calc.GetCelestialObjects().Where(m => Angle.Separation(prj.CenterEquatorial, m.Equatorial) < fov);
            if (onlyActive)
            {
                meteors = meteors.Where(m => m.IsActive);
            }

            meteors = meteors.Where(m => m.ActivityClass <= activityClassLimit);

            var color = settings.Get<Color>("ColorMeteors").Tint(nightMode);
            var pen = new Pen(color);
            var brush = new SolidBrush(color);
            var font = settings.Get<Font>("MeteorsLabelsFont");

            foreach (var meteor in meteors)
            {
                var p = prj.Project(meteor.Equatorial);

                if (prj.IsInsideScreen(p))
                {
                    var p1 = new Vec2(p.X - 5, p.Y - 5);
                    var p3 = new Vec2(p.X + 5, p.Y + 5);

                    var p2 = new Vec2(p.X - 5, p.Y + 5);
                    var p4 = new Vec2(p.X + 5, p.Y - 5);

                    GL.DrawLine(p1, p3, pen);
                    GL.DrawLine(p2, p4, pen);

                    if (showLabels)
                    {
                        string label = labelsType == MeteorLabelType.Name ? meteor.Name : meteor.Code;
                        map.DrawObjectLabel(label, font, brush, p, 10);
                    }

                    map.AddDrawnObject(p, meteor);
                }
            }
        }
    }
}
