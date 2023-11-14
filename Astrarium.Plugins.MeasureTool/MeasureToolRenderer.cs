using Astrarium.Algorithms;
using Astrarium.Types;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.MeasureTool
{
    /// <summary>
    /// Renders ruler tool over the celestial map
    /// </summary>
    public class MeasureToolRenderer : BaseRenderer
    {
        private readonly ISkyMap map;
        private readonly ISettings settings;
        private readonly Lazy<TextRenderer> textRenderer = new Lazy<TextRenderer>(() => new TextRenderer(128, 32));

        /// <summary>
        /// Font to print angular separation value
        /// </summary>
        private Font fontAngleValue = new Font("Arial", 8);

        /// <summary>
        /// Topmost rendering layer
        /// </summary>
        public override RendererOrder Order => RendererOrder.Foreground;

        /// <summary>
        /// Backing field for IsMeasureToolOn property.
        /// </summary>
        private bool _IsMeasureToolOn = false;

        /// <summary>
        /// Flag indicating the ruler is on
        /// </summary>
        public bool IsMeasureToolOn
        {
            get { return _IsMeasureToolOn; } 
            set
            {
                _IsMeasureToolOn = value;
                NotifyPropertyChanged(nameof(IsMeasureToolOn));
            }
        }

        /// <summary>
        /// Measure tool origin
        /// </summary>
        public CrdsEquatorial MeasureOrigin { get; set; }

        public MeasureToolRenderer(ISkyMap map, ISettings settings)
        {
            this.map = map;
            this.settings = settings;
        }

        ///// <summary>
        ///// Map should be renderered on MouseMove only if measure tool is on
        ///// </summary>
        public override bool OnMouseMove(ISkyMap map, PointF mouse, MouseButton mouseButton)
        {
            return IsMeasureToolOn;
        }

        public override void Render(ISkyMap map)
        {
            if (IsMeasureToolOn)
            {
                var prj = map.Projection;
                var schema = settings.Get<ColorSchema>("Schema");
                var m = map.MouseCoordinates;
                var mouse = prj.UnprojectEquatorial(m.X, m.Y);
                const int segmentsCount = 32;
                var color = Color.White.Tint(schema);
                var brush = new SolidBrush(color);

                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                GL.Enable(EnableCap.LineSmooth);
                GL.Enable(EnableCap.LineStipple);

                GL.Color3(color);
                GL.LineWidth(2);
                GL.LineStipple(1, 0xAAAA);

                GL.Begin(PrimitiveType.LineStrip);

                for (int i = 0; i <= segmentsCount; i++)
                {
                    CrdsEquatorial eq = Angle.Intermediate(mouse, MeasureOrigin, i / (float)segmentsCount);
                    Vec2 p = prj.Project(eq);
                    if (p != null)
                    {
                        GL.Vertex2(p.X, p.Y);
                    }
                }

                GL.End();
                GL.LineWidth(1);
                GL.Disable(EnableCap.LineStipple);

                // draw text with angle separation value
                double angle = Angle.Separation(mouse, MeasureOrigin);
                textRenderer.Value.DrawString(Formatters.Angle.Format(angle), fontAngleValue, brush, m);

            }
        }
    }
}
