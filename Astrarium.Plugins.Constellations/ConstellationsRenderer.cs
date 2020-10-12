using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;

namespace Astrarium.Plugins.Constellations
{
    public class ConstellationsRenderer : BaseRenderer
    {
        private readonly ConstellationsCalc constellationsCalc;
        private readonly ISettings settings;
        private readonly Func<string, Constellation> GetConstellation;

        public ConstellationsRenderer(ConstellationsCalc constellationsCalc, ISky sky, ISettings settings)
        {
            this.constellationsCalc = constellationsCalc;
            this.settings = settings;
            GetConstellation = sky.GetConstellation;
        }

        public override void Render(IMapContext map)
        {
            if (settings.Get<bool>("ConstBorders"))
            {
                RenderBorders(map);
            }
            if (settings.Get<bool>("ConstLabels"))
            {
                RenderConstLabels(map);
            }
        }

        public override RendererOrder Order => RendererOrder.Grids;

        /// <summary>
        /// Renders constellation borders on the map
        /// </summary>
        private void RenderBorders(IMapContext map)
        {
            PointF p1, p2;
            CrdsHorizontal h1, h2;
            var borders = constellationsCalc.ConstBorders;
            bool isGround = settings.Get<bool>("Ground");
            Pen penBorder = new Pen(map.GetColor("ColorConstBorders"));

            foreach (var block in borders)
            {
                for (int i = 0; i < block.Count - 1; i++)
                {
                    h1 = block.ElementAt(i).Horizontal;
                    h2 = block.ElementAt(i + 1).Horizontal;

                    if ((!isGround || h1.Altitude >= 0 || h2.Altitude >= 0) &&
                        Angle.Separation(map.Center, h1) < 90 &&
                        Angle.Separation(map.Center, h2) < 90)
                    {
                        p1 = map.Project(h1);
                        p2 = map.Project(h2);

                        var points = map.SegmentScreenIntersection(p1, p2);
                        if (points.Length == 2)
                        {
                            map.Graphics.DrawLine(penBorder, points[0], points[1]);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Renders constellations labels on the map
        /// </summary>
        private void RenderConstLabels(IMapContext map)
        {
            var constellations = constellationsCalc.ConstLabels;
            bool isGround = settings.Get<bool>("Ground");

            StringFormat format = new StringFormat();
            format.LineAlignment = StringAlignment.Center;
            format.Alignment = StringAlignment.Center;

            int fontSize = Math.Min((int)(800 / map.ViewAngle), 32);
            Font font = new Font(FontFamily.GenericSansSerif, fontSize);
            LabelType labelType = settings.Get<LabelType>("ConstLabelsType");
            Brush brushLabel = new SolidBrush(map.GetColor("ColorConstLabels"));

            foreach (var c in constellations)
            {
                var h = c.Horizontal;                
                if ((!isGround || h.Altitude > 0) && Angle.Separation(map.Center, h) < map.ViewAngle)
                {
                    var p = map.Project(h);
                    var constellation = GetConstellation(c.Code);
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

                    map.Graphics.DrawString(label, font, brushLabel, p, format);
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
