using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Drawing;
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

        public TrackRenderer(TrackCalc trackCalc, ISettings settings)
        {
            this.trackCalc = trackCalc;
            this.settings = settings;
        }

        public override void Render(ISkyMap map)
        {
            var prj = map.Projection;
            var nightMode = settings.Get("NightMode");
            var tracks = trackCalc.Tracks;

            GL.Enable(GL.BLEND);
            GL.BlendFunc(GL.SRC_ALPHA, GL.ONE_MINUS_SRC_ALPHA);
            GL.Enable(GL.LINE_SMOOTH);
            GL.Enable(GL.POINT_SMOOTH);

            foreach (var track in tracks)
            {
                if (track.Points.Any())
                {
                    var color = track.Color.Tint(nightMode);

                    GL.Color3(color);

                    GL.Begin(GL.LINE_STRIP);

                    for (int i = 0; i < track.Points.Count; i++)
                    {
                        Vec2 p = prj.Project(track.Points[i]);
                        if (p != null)
                        {
                            GL.Vertex2(p.X, p.Y);
                        }
                        else
                        {
                            GL.End();
                            GL.Begin(GL.LINE_STRIP);
                        }
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
                                    GL.Begin(GL.POINTS);
                                    GL.Color3(color);
                                    GL.Vertex2(p.X, p.Y);
                                    GL.End();

                                    var label = Formatters.DateTime.Format(new Date(jd, prj.Context.GeoLocation.UtcOffset));
                                    map.DrawObjectLabel(label, fontLabel, brush, p, 4);
                                }
                            }

                            jd += trackStep;
                        }
                    }
                }
            }
        }

        public override RendererOrder Order => RendererOrder.SolarSystem - 1;
    }
}
