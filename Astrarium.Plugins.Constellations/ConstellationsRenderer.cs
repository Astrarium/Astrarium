using Astrarium.Algorithms;
using Astrarium.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Documents;

namespace Astrarium.Plugins.Constellations
{
    public class ConstellationsRenderer : BaseRenderer
    {
        private readonly ConstellationsCalc constellationsCalc;
        private readonly ISettings settings;
        private readonly ISky sky;
        private readonly ISkyMap map;

        private readonly FiguresManager figuresManager;

        private string currentConst = null;
        private int currentConstBrightness = 0;

        public override RendererOrder Order => RendererOrder.Grids;

        public ConstellationsRenderer(ConstellationsCalc constellationsCalc, FiguresManager figuresManager, ISky sky, ISkyMap map, ISettings settings)
        {
            this.constellationsCalc = constellationsCalc;
            this.figuresManager = figuresManager;
            this.sky = sky;
            this.map = map;
            this.settings = settings;
        }

        public override void Initialize()
        {
            figuresManager.Initialize();
            new Thread(DoWork) { IsBackground = true, Priority = ThreadPriority.Lowest }.Start();
        }

        private void DoWork()
        {
            do
            {
                if (currentConstBrightness < figuresManager.MaxBrightness)
                {
                    currentConstBrightness = Math.Min(figuresManager.MaxBrightness, currentConstBrightness + 5);
                    map.Invalidate();
                }
                Thread.Sleep(10);
            }
            while (true);
        }

        public override void Render(ISkyMap map)
        {
            if (settings.Get("ConstFigures"))
            {
                RenderFigures();
            }
            if (settings.Get("ConstBorders"))
            {
                RenderBorders();
            }
            if (settings.Get("ConstLabels"))
            {
                RenderLabels();
            }
        }

        private void RenderLabels()
        {
            var prj = map.Projection;
            var nightMode = settings.Get("NightMode");
            Font defFont = settings.Get<Font>("ConstLabelsFont");
            float fontSize = Math.Max(8, (float)Math.Min((int)(800 / prj.Fov), defFont.Size));
            Font font = new Font(defFont.FontFamily, fontSize, defFont.Style);
            LabelType labelType = settings.Get<LabelType>("ConstLabelsType");
            Brush brushLabel = new SolidBrush(settings.Get<Color>("ColorConstLabels").Tint(nightMode));

            var constellations = constellationsCalc.ConstLabels;

            foreach (var c in constellations)
            {
                var eq = Precession.GetEquatorialCoordinates(c.Equatorial0, prj.Context.PrecessionElements);
                var p = prj.Project(eq);
                if (prj.IsInsideScreen(p))
                {
                    var constellation = sky.GetConstellation(c.Code);
                    string label;
                    switch (labelType)
                    {
                        case LabelType.InternationalCode:
                            label = constellation.Code;
                            break;
                        case LabelType.LocalName:
                            label = constellation.LocalName;
                            break;
                        case LabelType.InternationalName:
                        default:
                            label = constellation.LatinName;
                            break;
                    }

                    GL.DrawString(label, font, brushLabel, p, horizontalAlign: StringAlignment.Center, verticalAlign: StringAlignment.Center);
                }
            }
        }

        private void RenderFigures()
        {
            var prj = map.Projection;
            var nightMode = settings.Get("NightMode");

            GL.Enable(GL.BLEND);
            GL.BlendFunc(GL.SRC_ALPHA, GL.ONE_MINUS_SRC_COLOR);
            GL.Enable(GL.LINE_SMOOTH);
            GL.Hint(GL.LINE_SMOOTH_HINT, GL.NICEST);
            GL.Enable(GL.TEXTURE_2D);
            GL.Enable(GL.CULL_FACE);

            if (!prj.FlipVertical ^ prj.FlipHorizontal)
            {
                GL.CullFace(GL.BACK);
            }
            else
            {
                GL.CullFace(GL.FRONT);
            }

            double fov = prj.RealFov;
            var eqCenter = prj.WithoutRefraction(prj.CenterEquatorial);

            var figures = figuresManager.Figures.OrderBy(x => x.Abbr.Split(',').Any(a => a.Trim().Equals(currentConst, StringComparison.OrdinalIgnoreCase)) ? 1 : 0).ToList();

            var group = settings.Get<FigureGroup>("ConstFiguresGroup");

            if (group == FigureGroup.Zodiac)
            {
                figures = figures.Where(x => "Ari,Tau,Gem,Cnc,Leo,Vir,Lib,Sco,Sgr,Cap,Aqr,Psc".Split(',').Contains(x.Abbr)).ToList();
            }
            else if (group == FigureGroup.Current)
            {
                figures = figures.Where(x => x.Abbr.Split(',').Any(a => a.Trim().Equals(currentConst, StringComparison.OrdinalIgnoreCase))).ToList();
            }


            foreach (var figure in figures)
            {
                if (File.Exists(figure.File))
                {
                    figure.TextureId = GL.GetTexture(figure.File, permanent: false, readyCallback: map.Invalidate);
                    GL.BindTexture(GL.TEXTURE_2D, figure.TextureId);
                }

                if (figure.TextureId > 0)
                {
                    var pSky = figure.TextureToSkyCoords(new PointF(0.5f, 0.5f));
                    if (Angle.Separation(eqCenter, pSky) > 130) continue;

                    double alpha = 0;
                    double z = 1;

                    // dimming on zoom
                    if (settings.Get("ConstFiguresDimOnZoom"))
                    {
                        const double maxFov = 90;
                        const double minFov = 0;

                        z = (Math.Sin(Angle.ToRadians( Math.Min(maxFov, prj.Fov))) - Math.Sin(Angle.ToRadians(minFov))) / (Math.Sin(Angle.ToRadians(maxFov)) - Math.Sin(Angle.ToRadians(minFov)));
                        if (z <= 0) return;
                        if (z >= 1) z = 1;
                    }

                    if (settings.Get("ConstFiguresHighlightHovered") &&
                        figure.Abbr.Split(',').Any(a => a.Trim().Equals(currentConst, StringComparison.OrdinalIgnoreCase)))
                    {
                        alpha = currentConstBrightness * z;
                    }
                    else
                    {
                        alpha = figuresManager.DefaultBrightness * z;
                    }

                    if (alpha > 1)
                    {
                        GL.Color4(Color.FromArgb((int)alpha, 191, 241, 255).Tint(nightMode));

                        const double steps = 4;
                        double step = 1 / steps;

                        for (int r = 0; r < steps; r++)
                        {
                            GL.Begin(GL.QUAD_STRIP);

                            double t1 = r * step;
                            double t2 = (r + 1) * step;

                            for (int c = 0; c <= steps; c++)
                            {
                                double s = c * step;

                                var s1 = figure.TextureToSkyCoords(new Vec2(s, t1));
                                var s2 = figure.TextureToSkyCoords(new Vec2(s, t2));

                                // need to convert from J2000 to current
                                s1 = Precession.GetEquatorialCoordinates(s1, constellationsCalc.PrecessionElementsJ2000ToCurrent);
                                s2 = Precession.GetEquatorialCoordinates(s2, constellationsCalc.PrecessionElementsJ2000ToCurrent);

                                var p1 = prj.Project(s1);
                                var p2 = prj.Project(s2);

                                if (p1 != null && p2 != null)
                                {
                                    GL.TexCoord2(s, t1);
                                    GL.Vertex2(p1.X, p1.Y);

                                    GL.TexCoord2(s, t2);
                                    GL.Vertex2(p2.X, p2.Y);
                                }
                                else
                                {
                                    GL.End();
                                    GL.Begin(GL.QUAD_STRIP);
                                }
                            }

                            GL.End();
                        }
                    }
                }
            }

            GL.Disable(GL.TEXTURE_2D);
            GL.Disable(GL.CULL_FACE);
            GL.Disable(GL.BLEND);
        }

        private void RenderBorders()
        {
            var prj = map.Projection;
            var nightMode = settings.Get("NightMode");

            GL.Enable(GL.BLEND);
            GL.BlendFunc(GL.SRC_ALPHA, GL.ONE_MINUS_SRC_ALPHA);
            GL.Enable(GL.LINE_SMOOTH);
            GL.Hint(GL.LINE_SMOOTH_HINT, GL.NICEST);

            double fov = prj.RealFov;

            var color = settings.Get<Color>("ColorConstBorders").Tint(nightMode);

            GL.Color3(color);

            CrdsEquatorial eqCenter = prj.WithoutRefraction(prj.CenterEquatorial);
            CrdsEquatorial eqCenter0 = Precession.GetEquatorialCoordinates(eqCenter, constellationsCalc.PrecessionElementsCurrentToJ2000);

            GL.LineWidth(0.1f);
            GL.Begin(GL.LINES);

            foreach (var block in constellationsCalc.Borders)
            {
                for (int i = 0; i < block.Count - 1; i++)
                {
                    if (Angle.Separation(eqCenter0, block[i]) < fov ||
                        Angle.Separation(eqCenter0, block[i + 1]) < fov)
                    {
                        var eq1 = Precession.GetEquatorialCoordinates(block[i], constellationsCalc.PrecessionElementsJ2000ToCurrent);
                        var eq2 = Precession.GetEquatorialCoordinates(block[i + 1], constellationsCalc.PrecessionElementsJ2000ToCurrent);

                        var p1 = prj.Project(eq1);
                        var p2 = prj.Project(eq2);

                        if (p1 != null && p2 != null)
                        {
                            GL.Vertex2(p1.X, p1.Y);
                            GL.Vertex2(p2.X, p2.Y);
                        }
                    }
                }
            }
            GL.End();
        }

        public override void OnMouseMove(ISkyMap map, MouseButton mouseButton)
        {
            CrdsEquatorial eq1875 = Precession.GetEquatorialCoordinates(map.MouseEquatorialCoordinates, constellationsCalc.PrecessionElementsCurrentToB1875);

            string con = Algorithms.Constellations.FindConstellation(eq1875);
            if (con != currentConst)
            {
                currentConstBrightness = settings.Get<FigureGroup>("ConstFiguresGroup") == FigureGroup.Current ? 0 : figuresManager.DefaultBrightness;
                currentConst = con;
                map.Invalidate();
            }
        }

        /// <summary>
        /// Type of constellation label
        /// </summary>
        public enum LabelType
        {
            [Description("Settings.ConstLabelsType.InternationalName")]
            InternationalName = 0,

            [Description("Settings.ConstLabelsType.InternationalCode")]
            InternationalCode = 1,

            [Description("Settings.ConstLabelsType.LocalName")]
            LocalName = 2
        }

        public enum FigureType
        {
            [Description("Settings.ConstFiguresType.Hevelius")]
            Hevelius = 0,

            [Description("Settings.ConstLabelsType.Modern")]
            Modern = 1,
        }

        public enum FigureGroup
        {
            [Description("Settings.ConstFiguresGroup.All")]
            All = 0,

            [Description("Settings.ConstLabelsType.Zodiac")]
            Zodiac = 1,

            [Description("Settings.ConstLabelsType.Current")]
            Current = 2,
        }
    }
}
