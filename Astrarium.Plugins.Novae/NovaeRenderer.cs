using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Drawing;
using System.Linq;

namespace Astrarium.Plugins.Novae
{
    public class NovaeRenderer : BaseRenderer
    {
        public override RendererOrder Order => RendererOrder.Stars;

        private NovaeCalculator calc;
        private ISettings settings;

        private const int limitAllNames = 40;
       
        public NovaeRenderer(NovaeCalculator calc, ISettings settings)
        {
            this.calc = calc;
            this.settings = settings;
        }

        public override void Render(ISkyMap map)
        {
            if (!settings.Get("Stars") || !settings.Get("Novae")) return;
            if (map.DaylightFactor == 1) return;

            var prj = map.Projection;
            var nightMode = settings.Get("NightMode");
            bool drawLabels = settings.Get("StarsLabels") && settings.Get("NovaeLabels") && prj.Fov <= limitAllNames;
            Color labelColor = settings.Get<Color>("ColorStarsLabels").Tint(nightMode);
            Brush brushLabel = new SolidBrush(labelColor);
            var fontStarNames = settings.Get<Font>("StarsLabelsFont");

            // real circular FOV with respect of screen borders
            double fov = prj.Fov * Math.Max(prj.ScreenWidth, prj.ScreenHeight) / Math.Min(prj.ScreenWidth, prj.ScreenHeight);

            // filter novae by magnitude and FOV
            var novae = calc.Novae.Where(n => n.Magnitude < prj.MagLimit && Angle.Separation(prj.CenterEquatorial, n.Equatorial) < fov);

            GL.Enable(GL.POINT_SMOOTH);
            GL.Enable(GL.BLEND);
            GL.BlendFunc(GL.SRC_ALPHA, GL.ONE_MINUS_SRC_ALPHA);
            GL.Hint(GL.POINT_SMOOTH_HINT, GL.NICEST);

            foreach (var star in novae)
            {
                double alt = prj.ToHorizontal(star.Equatorial).Altitude;
                float size = prj.GetPointSize(star.Magnitude, altitude: alt);
                if (size > 0)
                {
                    if ((int)size == 0) size = 1;

                    // screen coordinates, for current epoch
                    Vec2 p = prj.Project(star.Equatorial);

                    if (prj.IsInsideScreen(p))
                    {
                        GL.PointSize(size);
                        GL.Begin(GL.POINTS);
                        GL.Color3(Color.White.Tint(nightMode));
                        GL.Vertex2(p.X, p.Y);
                        GL.End();

                        if (drawLabels)
                        {
                            map.DrawObjectLabel(star.ProperName, fontStarNames, brushLabel, p, size);
                        }

                        map.AddDrawnObject(p, star);
                    }
                }
            }
        }
    }
}
