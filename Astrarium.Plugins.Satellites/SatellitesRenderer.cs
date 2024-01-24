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
        private const double AU = 149597870.691;

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
            Color eclipsedLabelColor = settings.Get<Color>("ColorEclipsedSatellitesLabels").Tint(nightMode);
            Brush brushLabel = new SolidBrush(labelColor);
            Brush brushEclipsedLabel = new SolidBrush(eclipsedLabelColor);
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
            var sunR = calculator.SunRectangular;
            Vec3 sunVector = AU * new Vec3(sunR.X, sunR.Y, sunR.Z);

            foreach (var s in satellites)
            {
                // current satellite position vector
                var pos = s.Position + deltaTime * s.Velocity;

                // flag indicating satellite is eclipsed
                bool isEclipsed = Norad.IsSatelliteEclipsed(pos, sunVector);

                // topocentric vector
                var t = Norad.TopocentricSatelliteVector(topocentricLocationVector, pos);

                // visible magnitude
                s.Magnitude = Norad.GetSatelliteMagnitude(s.StdMag, t.Length);

                // horizontal coordinates of satellite
                var h = Norad.HorizontalCoordinates(prj.Context.GeoLocation, t, prj.Context.SiderealTime);

                // equatorial coordinates
                s.Equatorial = h.ToEquatorial(prj.Context.GeoLocation, prj.Context.SiderealTime);

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
                            var brush = isEclipsed ? brushEclipsedLabel : brushLabel;
                            map.DrawObjectLabel(textRenderer.Value, s.Name, fontNames, brush, p, size);
                        }

                        map.AddDrawnObject(p, s, size);
                    }
                }
            }
        }
    }
}
