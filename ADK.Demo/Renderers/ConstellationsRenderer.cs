using ADK.Demo.Calculators;
using ADK.Demo.Objects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;

namespace ADK.Demo.Renderers
{
    public class ConstellationsRenderer : BaseSkyRenderer
    {
        private IConstellationsBordersProvider bordersProvider;
        private IConstellationsProvider constellationsProvider;

        private Pen penBorder = new Pen(Color.FromArgb(64, 32, 32));
        private Brush brushLabel = new SolidBrush(Color.FromArgb(64, 32, 32));

        private const double maxSeparation = 90 * 1.2;

        public ConstellationsRenderer(Sky sky, IConstellationsProvider constellationsProvider, IConstellationsBordersProvider bordersProvider, ISkyMap skyMap, ISettings settings) : base(sky, skyMap, settings)
        {
            this.constellationsProvider = constellationsProvider;
            this.bordersProvider = bordersProvider;
        }

        public override void Render(IMapContext map)
        {
            if (Settings.Get<bool>("ConstBorders"))
            {
                RenderBorders(map);
            }
            if (Settings.Get<bool>("ConstLabels"))
            {
                RenderConstLabels(map);
            }
        }

        /// <summary>
        /// Renders constellation borders on the map
        /// </summary>
        private void RenderBorders(IMapContext map)
        {
            PointF p1, p2;
            CrdsHorizontal h1, h2;
            var borders = bordersProvider.ConstBorders;
            bool isGround = Settings.Get<bool>("Ground");

            foreach (var block in borders)
            {
                for (int i = 0; i < block.Count - 1; i++)
                {
                    h1 = block.ElementAt(i).Horizontal;
                    h2 = block.ElementAt(i + 1).Horizontal;

                    if ((!isGround || h1.Altitude >= 0 || h2.Altitude >= 0) &&
                        Angle.Separation(map.Center, h1) < maxSeparation &&
                        Angle.Separation(map.Center, h2) < maxSeparation)
                    {
                        p1 = map.Projection.Project(h1);
                        p2 = map.Projection.Project(h2);

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
            bool isGround = Settings.Get<bool>("Ground");

            StringFormat format = new StringFormat();
            format.LineAlignment = StringAlignment.Center;
            format.Alignment = StringAlignment.Center;

            int fontSize = Math.Min((int)(800 / map.ViewAngle), 32);
            Font font = new Font(FontFamily.GenericSansSerif, fontSize);
            LabelType labelType = Settings.Get<LabelType>("ConstLabelsType");

            foreach (var c in constellations)
            {
                var h = c.Label.Horizontal;                
                if ((!isGround || h.Altitude > 0) && Angle.Separation(map.Center, h) < map.ViewAngle * 1.2)
                {
                    var p = map.Projection.Project(h);

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
                    var sz = map.Graphics.MeasureString(label, font);
                    map.Labels.Add(new RectangleF(new PointF(p.X - sz.Width / 2, p.Y - sz.Height / 2), sz));
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
