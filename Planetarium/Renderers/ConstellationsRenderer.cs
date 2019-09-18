using ADK;
using Planetarium.Calculators;
using Planetarium.Config;
using Planetarium.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;

namespace Planetarium.Renderers
{
    public class ConstellationsRenderer : BaseRenderer
    {
        private readonly IConstellationsBordersProvider bordersProvider;
        private readonly IConstellationsProvider constellationsProvider;
        private readonly ISettings settings;

        private Pen penBorder = new Pen(Color.FromArgb(64, 32, 32));
        private Brush brushLabel = new SolidBrush(Color.FromArgb(64, 32, 32));

        public ConstellationsRenderer(IConstellationsProvider constellationsProvider, IConstellationsBordersProvider bordersProvider, ISettings settings)
        {
            this.constellationsProvider = constellationsProvider;
            this.bordersProvider = bordersProvider;
            this.settings = settings;
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
            var borders = bordersProvider.ConstBorders;
            bool isGround = settings.Get<bool>("Ground");
            double coeff = map.DiagonalCoefficient();

            foreach (var block in borders)
            {
                for (int i = 0; i < block.Count - 1; i++)
                {
                    h1 = block.ElementAt(i).Horizontal;
                    h2 = block.ElementAt(i + 1).Horizontal;

                    if ((!isGround || h1.Altitude >= 0 || h2.Altitude >= 0) &&
                        Angle.Separation(map.Center, h1) < 90 * coeff &&
                        Angle.Separation(map.Center, h2) < 90 * coeff)
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
            var constellations = constellationsProvider.Constellations;
            bool isGround = settings.Get<bool>("Ground");
            double coeff = map.DiagonalCoefficient();

            StringFormat format = new StringFormat();
            format.LineAlignment = StringAlignment.Center;
            format.Alignment = StringAlignment.Center;

            int fontSize = Math.Min((int)(800 / map.ViewAngle), 32);
            Font font = new Font(FontFamily.GenericSansSerif, fontSize);
            LabelType labelType = settings.Get<LabelType>("ConstLabelsType");

            foreach (var c in constellations)
            {
                var h = c.Label.Horizontal;                
                if ((!isGround || h.Altitude > 0) && Angle.Separation(map.Center, h) < map.ViewAngle * coeff)
                {
                    var p = map.Project(h);

                    string label = null;
                    switch (labelType)
                    {
                        case LabelType.InternationalCode:
                            label = c.Code;
                            break;
                        case LabelType.InternationalName:
                        default:
                            label = c.Name;
                            break;
                    }

                    map.Graphics.DrawString(label, font, brushLabel, p, format);
                }
            }
        }

        public enum LabelType
        {
            [Description("International Name")]
            InternationalName = 0,

            [Description("International Abbreviation")]
            InternationalCode = 1,
        }
    }
}
