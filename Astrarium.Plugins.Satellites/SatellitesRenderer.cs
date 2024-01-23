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
            var satellites = calculator.Satellites;

            GL.Enable(EnableCap.PointSmooth);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest);


            Vec3 topocentricLocationVector = Norad.TopocentricLocationVector(prj.Context.GeoLocation, prj.Context.SiderealTime);
            double deltaTime = (prj.Context.JulianDay - calculator.JulianDay) * 24;

            //Vec3 sunVector = new Vec3(Sun.Rectangular.X * AstroUtils.AU, Sun.Rectangular.Y * AstroUtils.AU, Sun.Rectangular.Z * AstroUtils.AU);

            foreach (var s in satellites)
            {
                var pos = s.Position + deltaTime * s.Velocity;

                // topocentric vector
                var t = Norad.TopocentricSatelliteVector(topocentricLocationVector, pos);
                var h = Norad.HorizontalCoordinates(prj.Context.GeoLocation, t, prj.Context.SiderealTime);
                s.Equatorial = h.ToEquatorial(prj.Context.GeoLocation, prj.Context.SiderealTime);
                s.Magnitude = Norad.GetSatelliteMagnitude(s.StdMag, t.Length);

                //bool isEclipsed = Norad.IsSatelliteEclipsed(pos,)

                float size = prj.GetPointSize(s.Magnitude);

                

                if (Angle.Separation(prj.CenterEquatorial, s.Equatorial) < fov)
                {
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
                            //map.DrawObjectLabel(textRenderer.Value, s.Name, fontNames, brushLabel, p, size);
                        }

                        map.AddDrawnObject(p, s, size);
                    }
                }
            }
        }
    }
}
