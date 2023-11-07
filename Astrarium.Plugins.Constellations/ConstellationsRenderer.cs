using Astrarium.Algorithms;
using Astrarium.Types;
using OpenTK.Graphics.OpenGL;
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
        private readonly Lazy<TextRenderer> textRenderer = new Lazy<TextRenderer>(() => new TextRenderer(512, 64));

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

        [Obsolete]
        public override void Render(IMapContext map) { }

        private void RenderLabels(ISkyMap map)
        {
            var prj = map.SkyProjection;

            var schema = settings.Get<ColorSchema>("Schema");
            Font defFont = settings.Get<Font>("ConstLabelsFont");
            float fontSize = Math.Max(8, (float)Math.Min((int)(800 / prj.Fov), defFont.Size));
            Font font = new Font(defFont.FontFamily, fontSize, defFont.Style);
            LabelType labelType = settings.Get<LabelType>("ConstLabelsType");
            Brush brushLabel = new SolidBrush(settings.Get<SkyColor>("ColorConstLabels").Night.Tint(schema));
            WF.TextFormatFlags formatFlags = WF.TextFormatFlags.HorizontalCenter | WF.TextFormatFlags.VerticalCenter;

            var mat = prj.MatEquatorialToVision * constellationsCalc.MatPrecession;

            var constellations = constellationsCalc.ConstLabels;

            foreach (var c in constellations)
            {
                var p = prj.Project(c.Cartesian, mat);
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
                    textRenderer.Value.DrawString(label, font, brushLabel, new Vec2(p.X - size.Width / 2, p.Y + size.Height / 2));
                    continue;
                }
            }
        }

        private void RenderBorders(ISkyMap map)
        {
            var prj = map.SkyProjection;
            var schema = settings.Get<ColorSchema>("Schema");

            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.LineSmooth);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // eq vision vector in J2000 coords
            var vecVision = constellationsCalc.MatPrecession0 * prj.VecEquatorialVision;

            // matrix for projection
            var mat = prj.MatEquatorialToVision * constellationsCalc.MatPrecession;

            // max angular distance from current vision vector
            // 0.7 coeff is an empyrical
            double fov = Angle.ToRadians(prj.Fov + 1);

            var color = settings.Get<SkyColor>("ColorConstBorders").GetColor(ColorSchema.Night).Tint(schema);

            GL.Color3(color);

            foreach (var block in constellationsCalc.Borders)
            {
                for (int i = 0; i < block.Count - 1; i++)
                {
                    if (vecVision.Angle(block[i]) > fov && vecVision.Angle(block[i + 1]) > fov)
                    {
                        continue;
                    }

                    var p1 = prj.Project(block[i], mat);
                    var p2 = prj.Project(block[i + 1], mat);
                    if (p1 != null && p2 != null)
                    {
                        GL.Begin(PrimitiveType.Lines);
                        GL.Vertex2(p1.X, p1.Y);
                        GL.Vertex2(p2.X, p2.Y);
                        GL.End();
                    }
                }
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
    }
}
