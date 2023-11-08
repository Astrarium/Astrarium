using Astrarium.Algorithms;
using Astrarium.Types;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace Astrarium.Plugins.Tracks
{
    /// <summary>
    /// Renders celestial bodies motion tracks on the map
    /// </summary>
    public class TrackRenderer : BaseRenderer
    {
        private readonly TrackCalc trackCalc;
        private readonly ISettings settings;

        // TODO: move to settings
        private readonly Font fontLabel = new Font("Arial", 8);
        private readonly Lazy<TextRenderer> textRenderer = new Lazy<TextRenderer>(() => new TextRenderer(256, 32));

        public TrackRenderer(TrackCalc trackCalc, ISettings settings)
        {
            this.trackCalc = trackCalc;
            this.settings = settings;
        }

        public override void Render(ISkyMap map)
        {
            var prj = map.Projection;
            var schema = settings.Get<ColorSchema>("Schema");
            var tracks = trackCalc.Tracks;

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.LineSmooth);
            GL.Enable(EnableCap.PointSmooth);

            foreach (var track in tracks)
            {
                if (track.Points.Any())
                {
                    var color = track.Color.Night.Tint(schema);

                    GL.Color3(color);

                    GL.Begin(PrimitiveType.LineStrip);

                    for (int i = 0; i < track.Points.Count; i++)
                    {
                        Vec2 p = prj.Project(track.Points[i]);
                        GL.Vertex2(p.X, p.Y);
                    }

                    GL.End();

                    if (track.DrawLabels)
                    {
                        var brush = new SolidBrush(color);

                        double trackStep = track.Step;
                        double stepLabels = track.LabelsStep.TotalDays;

                        int each = (int)(stepLabels / trackStep);

                        double jd = track.From;

                        GL.Color3(color);
                        GL.PointSize(4);
                        for (int i = 0; i < track.Points.Count; i++)
                        {
                            if (i % each == 0 || i == track.Points.Count - 1)
                            {
                                var tp = track.Points[i];
                                var p = prj.Project(tp);
                                if (prj.IsInsideScreen(p))
                                {
                                    GL.Begin(PrimitiveType.Points);
                                    GL.Color3(color);
                                    GL.Vertex2(p.X, p.Y);
                                    GL.End();

                                    var label = Formatters.DateTime.Format(new Date(jd, prj.Context.GeoLocation.UtcOffset));
                                    map.DrawObjectLabel(textRenderer.Value, label, fontLabel, brush, p, 4);
                                }
                            }

                            jd += trackStep;
                        }
                    }
                }
            }

        }

        [Obsolete]
        public override void Render(IMapContext map) { }

        public override RendererOrder Order => RendererOrder.SolarSystem - 1;
    }
}
