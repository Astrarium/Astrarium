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
            bool showEclipsed = settings.Get("SatellitesShowEclipsed");
            bool showBelowHorizon = settings.Get("SatellitesShowBelowHorizon");
            bool showOrbit = settings.Get("SatellitesShowOrbit");
            Color satelliteColor = Color.White.Tint(nightMode);
            Color labelColor = settings.Get<Color>("ColorSatellitesLabels").Tint(nightMode);
            Color eclipsedLabelColor = settings.Get<Color>("ColorEclipsedSatellitesLabels").Tint(nightMode);
            Color orbitColor = settings.Get<Color>("ColorSatellitesOrbit").Tint(nightMode);
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

            Vec3 topocentricLocationVector = calculator.GetTopocentricLocationVector(prj.Context);

            // diff, in hours
            double deltaTime = (prj.Context.JulianDay - calculator.JulianDay) * 24;

            foreach (var s in satellites)
            {
                // current satellite position vector
                var pos = s.Position + deltaTime * s.Velocity;

                // flag indicating satellite is eclipsed
                bool isEclipsed = Norad.IsSatelliteEclipsed(pos, calculator.SunVector);

                if (!showEclipsed && isEclipsed) continue;

                // topocentric vector
                var t = Norad.TopocentricSatelliteVector(topocentricLocationVector, pos);

                // visible magnitude
                s.Magnitude = Norad.GetSatelliteMagnitude(s.StdMag, t.Length);

                // horizontal coordinates of satellite
                var h = Norad.HorizontalCoordinates(prj.Context.GeoLocation, t, prj.Context.SiderealTime);

                if (!showBelowHorizon && h.Altitude < 0) continue;

                isEclipsed = isEclipsed || h.Altitude < 0;

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
                        GL.Color3(satelliteColor);
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

                // show orbit for selected satellite
                if (showOrbit && map.SelectedObject == s)
                {
                    // period, in hours
                    double period = s.Tle.Period / 60.0;

                    var track = new List<CrdsEquatorial>();

                    double deltaT = Date.DeltaT(prj.Context.JulianDay);

                    for (double diff = -period / 2; diff <= period / 2; diff += period / 256.0)
                    {
                        double jd = prj.Context.JulianDay - deltaT / 86400.0 + diff / 24.0;

                        Vec3 vel = new Vec3();
                        Norad.SGP4(s.Tle, jd, pos, vel);

                        // current satellite position vector
                        //pos = s.Position + (deltaTime + diff) * s.Velocity;

                        double siderealTime = prj.Context.SiderealTime + diff / period / 24.0 * 360 - deltaT / 86400.0 / 360.0;

                        // topocentric location vector for current instant
                        Vec3 tlv = Norad.TopocentricLocationVector(prj.Context.GeoLocation, siderealTime);

                        // topocentric vector
                        t = Norad.TopocentricSatelliteVector(tlv, pos);

                        // horizontal coordinates of satellite
                        h = Norad.HorizontalCoordinates(prj.Context.GeoLocation, t, siderealTime);

                        // equatorial coordinates
                        var eq = h.ToEquatorial(prj.Context.GeoLocation, prj.Context.SiderealTime);

                        track.Add(eq);
                    }

                    GL.Color3(orbitColor);

                    GL.Begin(PrimitiveType.LineStrip);
                    for (int i = 0; i < track.Count; i++)
                    {
                        Vec2 p = prj.Project(track[i]);
                        if (p != null)
                        {
                            GL.Vertex2(p.X, p.Y);
                        }
                        else
                        {
                            GL.End();
                            GL.Begin(PrimitiveType.LineStrip);
                        }
                    }
                    GL.End();
                }
            }
        }
    }
}
