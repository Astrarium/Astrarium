using Astrarium.Algorithms;
using Astrarium.Types;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Satellites
{
    public class SatellitesRenderer : BaseRenderer
    {
        private readonly Lazy<TextRenderer> textRenderer = new Lazy<TextRenderer>(() => new TextRenderer(256, 32));

        private readonly ISettings settings;
        private readonly SatellitesCalculator calculator;

        public override RendererOrder Order => RendererOrder.EarthOrbit;

        public SatellitesRenderer(ISettings settings, SatellitesCalculator calculator)
        {
            this.settings = settings;
            this.calculator = calculator;
        }

        public override void Render(ISkyMap map)
        {
            if (!settings.Get("Satellites")) return;
            if (map.DaylightFactor == 1) return;

            var prj = map.Projection;
            var nightMode = settings.Get("NightMode");
            bool drawLabels = settings.Get("SatellitesLabels");
            Color labelColor = settings.Get<Color>("ColorSatellitesLabels").Tint(nightMode);
            Brush brushLabel = new SolidBrush(labelColor);
            var fontNames = settings.Get<Font>("SatellitesLabelsFont");

            // real circular FOV with respect of screen borders
            double fov = prj.Fov * Math.Max(prj.ScreenWidth, prj.ScreenHeight) / Math.Min(prj.ScreenWidth, prj.ScreenHeight);

            // filter satellites
            var satellites = calculator.Satellites.Where(n => /*n.Magnitude < prj.MagLimit &&*/ Angle.Separation(prj.CenterEquatorial, n.Equatorial) < fov);

            GL.Enable(EnableCap.PointSmooth);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest);

            foreach (var s in satellites)
            {
                float size = 1;// prj.GetPointSize(star.Magnitude);
                if (size > 0)
                {
                    if ((int)size == 0) size = 1;

                    // screen coordinates, for current epoch
                    Vec2 p = prj.Project(s.Equatorial);

                    if (prj.IsInsideScreen(p))
                    {
                        GL.PointSize(size);
                        GL.Begin(PrimitiveType.Points);
                        GL.Color3(Color.White.Tint(nightMode));
                        GL.Vertex2(p.X, p.Y);
                        GL.End();

                        if (drawLabels)
                        {
                            map.DrawObjectLabel(textRenderer.Value, s.Name, fontNames, brushLabel, p, size);
                        }

                        map.AddDrawnObject(p, s, size);
                    }
                }
            }
        }
    }
}
