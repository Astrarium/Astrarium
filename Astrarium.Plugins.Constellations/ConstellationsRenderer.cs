using Astrarium.Algorithms;
using Astrarium.Types;
using OpenTK.Graphics.OpenGL;
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

        public override void Render(Projection projection)
        {
            if (settings.Get<bool>("ConstBorders"))
            {
                RenderBorders(projection);
            }
            if (settings.Get<bool>("ConstLabels"))
            {
                //RenderConstLabels(map);
            }
        }

        public override void Render(IMapContext map)
        {
            //if (settings.Get<bool>("ConstBorders"))
            //{
            //    RenderBorders(map);
            //}
            if (settings.Get<bool>("ConstLabels"))
            {
                RenderConstLabels(map);
            }
        }

        public override RendererOrder Order => RendererOrder.Grids;

        ///// <summary>
        ///// Renders constellation borders on the map
        ///// </summary>
        //private void RenderBorders(IMapContext map)
        //{
        //    PointF p1, p2;
        //    CrdsHorizontal h1, h2;
        //    var borders = constellationsCalc.ConstBorders;
        //    bool isGround = settings.Get<bool>("Ground");
        //    Pen penBorder = new Pen(map.GetColor("ColorConstBorders"));

        //    foreach (var block in borders)
        //    {
        //        for (int i = 0; i < block.Count - 1; i++)
        //        {
        //            h1 = block.ElementAt(i).Horizontal;
        //            h2 = block.ElementAt(i + 1).Horizontal;

        //            if ((!isGround || h1.Altitude >= 0 || h2.Altitude >= 0) &&
        //                Angle.Separation(map.Center, h1) < 90 &&
        //                Angle.Separation(map.Center, h2) < 90)
        //            {
        //                p1 = map.Project(h1);
        //                p2 = map.Project(h2);

        //                var points = map.SegmentScreenIntersection(p1, p2);
        //                if (points.Length == 2)
        //                {
        //                    map.Graphics.DrawLine(penBorder, points[0], points[1]);
        //                }
        //            }
        //        }
        //    }
        //}

        private void RenderBorders(Projection projection)
        {
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.LineSmooth);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // eq vision vector in J2000 coords
            var equ_vision0 = constellationsCalc.MatPrecession0 * projection.VecEquatorialVision;

            // matrix for projection
            var mat = projection.MatEquatorialToVision * constellationsCalc.MatPrecession;

            // max angular distance from current vision vector
            // 0.7 coeff is an empyrical
            double fov = Angle.ToRadians(projection.MaxFov * 0.7);

            GL.Color3(Color.Brown);

            foreach (var block in constellationsCalc.Borders)
            {
                for (int i = 0; i < block.Count - 1; i++)
                {
                    if (equ_vision0.Angle(block[i]) > fov && equ_vision0.Angle(block[i + 1]) > fov)
                    {
                        continue;
                    }

                    var p1 = projection.Project(block[i], mat);
                    if (p1 == null)
                    {
                        continue;
                    }

                    var p2 = projection.Project(block[i + 1], mat);
                    if (p2 == null)
                    {
                        continue;
                    }

                    GL.Begin(PrimitiveType.Lines);
                    GL.Vertex2(p1.X, p1.Y);
                    GL.Vertex2(p2.X, p2.Y);
                    GL.End();
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

            Font defFont = settings.Get<Font>("ConstLabelsFont");
            float fontSize = (float)Math.Min((int)(800 / map.ViewAngle), defFont.Size);
            Font font = new Font(defFont.FontFamily, fontSize, defFont.Style);
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
