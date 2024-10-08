﻿using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Drawing;

namespace Astrarium.Plugins.MeasureTool
{
    /// <summary>
    /// Renders ruler tool over the celestial map
    /// </summary>
    public class MeasureToolRenderer : BaseRenderer
    {
        private readonly ISkyMap map;
        private readonly ISettings settings;

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

        /// <summary>
        /// Map should be renderered on MouseMove only if measure tool is on
        /// </summary>
        public override void OnMouseMove(ISkyMap map, MouseButton mouseButton)
        {
            if (IsMeasureToolOn)
            {
                map.Invalidate();
            }
        }

        public override void Render(ISkyMap map)
        {
            if (IsMeasureToolOn)
            {
                var prj = map.Projection;
                var nightMode = settings.Get("NightMode");
                var mouse = prj.WithoutRefraction(map.MouseEquatorialCoordinates);
                const int segmentsCount = 32;
                var color = Color.White.Tint(nightMode);
                var brush = new SolidBrush(color);

                GL.Enable(GL.BLEND);
                GL.BlendFunc(GL.SRC_ALPHA, GL.ONE_MINUS_SRC_ALPHA);
                GL.Enable(GL.LINE_SMOOTH);
                GL.Enable(GL.LINE_STIPPLE);

                GL.Color3(color);
                GL.LineWidth(2);
                GL.LineStipple(1, 0xAAAA);

                GL.Begin(GL.LINE_STRIP);

                for (int i = 0; i <= segmentsCount; i++)
                {
                    CrdsEquatorial eq = Angle.Intermediate(mouse, MeasureOrigin, i / (float)segmentsCount);
                    Vec2 p = prj.Project(eq);
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
                GL.LineWidth(1);
                GL.Disable(GL.LINE_STIPPLE);

                // draw text with angle separation value
                double angle = Angle.Separation(mouse, MeasureOrigin);
                GL.DrawString(Formatters.Angle.Format(angle), fontAngleValue, brush, map.MouseScreenCoordinates);
            }
        }
    }
}
