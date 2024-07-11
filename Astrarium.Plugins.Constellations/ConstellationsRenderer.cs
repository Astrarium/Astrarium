using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.ComponentModel;
using System.Drawing;
using WF = System.Windows.Forms;

namespace Astrarium.Plugins.Constellations
{
    public class ConstellationsRenderer : BaseRenderer
    {
        private readonly ConstellationsCalc constellationsCalc;
        private readonly ISettings settings;
        private readonly ISky sky;

        public override RendererOrder Order => RendererOrder.Grids;

        public ConstellationsRenderer(ConstellationsCalc constellationsCalc, ISky sky, ISettings settings)
        {
            this.constellationsCalc = constellationsCalc;
            this.sky = sky;
            this.settings = settings;
        }

        public override void Render(ISkyMap map)
        {
            if (settings.Get("ConstBorders"))
            {
                RenderBorders(map);
            }
            if (settings.Get("ConstLabels"))
            {
                RenderLabels(map);
            }
        }

        private void RenderLabels(ISkyMap map)
        {
            var prj = map.Projection;
            var nightMode = settings.Get("NightMode");
            Font defFont = settings.Get<Font>("ConstLabelsFont");
            float fontSize = Math.Max(8, (float)Math.Min((int)(800 / prj.Fov), defFont.Size));
            Font font = new Font(defFont.FontFamily, fontSize, defFont.Style);
            LabelType labelType = settings.Get<LabelType>("ConstLabelsType");
            Brush brushLabel = new SolidBrush(settings.Get<Color>("ColorConstLabels").Tint(nightMode));
            WF.TextFormatFlags formatFlags = WF.TextFormatFlags.HorizontalCenter | WF.TextFormatFlags.VerticalCenter;

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

                    var size = WF.TextRenderer.MeasureText(label, font, Size.Empty, formatFlags);
                    GL.DrawString(label, font, brushLabel, new Vec2(p.X - size.Width / 2, p.Y + size.Height / 2));
                }
            }
        }

        private void RenderBorders(ISkyMap map)
        {
            var prj = map.Projection;
            var nightMode = settings.Get("NightMode");

            GL.Enable(GL.BLEND);
            GL.Enable(GL.LINE_SMOOTH);
            GL.BlendFunc(GL.SRC_ALPHA, GL.ONE_MINUS_SRC_ALPHA);

            double w = Math.Max(prj.ScreenWidth, prj.ScreenHeight) / (double)Math.Min(prj.ScreenWidth, prj.ScreenHeight);
            double h = Math.Min(prj.ScreenWidth, prj.ScreenHeight) / (double)Math.Min(prj.ScreenWidth, prj.ScreenHeight);
            double fov = prj.Fov * Math.Sqrt(h * h + w * w) / 2;

            var color = settings.Get<Color>("ColorConstBorders").Tint(nightMode);

            GL.Color3(color);

            CrdsEquatorial eqCenter = prj.WithoutRefraction(prj.CenterEquatorial);
            CrdsEquatorial eqCenter0 = Precession.GetEquatorialCoordinates(eqCenter, constellationsCalc.PrecessionElementsCurrentToB1950);

            GL.Begin(GL.LINES);

            foreach (var block in constellationsCalc.Borders)
            {
                for (int i = 0; i < block.Count - 1; i++)
                {
                    if (Angle.Separation(eqCenter0, block[i]) < fov ||
                        Angle.Separation(eqCenter0, block[i + 1]) < fov)
                    {
                        var eq1 = Precession.GetEquatorialCoordinates(block[i], constellationsCalc.PrecessionElementsB1950ToCurrent);
                        var eq2 = Precession.GetEquatorialCoordinates(block[i + 1], constellationsCalc.PrecessionElementsB1950ToCurrent);

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
    }
}
