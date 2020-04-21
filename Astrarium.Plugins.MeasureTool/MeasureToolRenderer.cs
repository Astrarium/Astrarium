using Astrarium.Algorithms;
using Astrarium.Types;
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
        public CrdsHorizontal MeasureOrigin { get; set; }

        /// <summary>
        /// Map should be renderer on MouseMove only if measure tool is on
        /// </summary>
        public override bool OnMouseMove(CrdsHorizontal mouse, MouseButton mouseButton)
        {
            return IsMeasureToolOn;
        }

        /// <summary>
        /// Does the rendering logic
        /// </summary>
        /// <param name="map">Map instance</param>
        public override void Render(IMapContext map)
        {
            if (IsMeasureToolOn && map.MousePosition != null)
            {
                double coeff = map.DiagonalCoefficient();

                List<PointF> points = new List<PointF>();
                for (int f = 0; f <= 10; f++)
                {
                    CrdsHorizontal h = Angle.Intermediate(map.MousePosition, MeasureOrigin, f / 10.0);
                    points.Add(map.Project(h));
                    if (Angle.Separation(h, map.Center) > map.ViewAngle * coeff)
                    {
                        break;
                    }
                }

                if (points.Count > 1)
                {
                    map.Graphics.DrawCurve(new Pen(map.GetColor(Color.White)), points.ToArray());
                    double angle = Angle.Separation(map.MousePosition, MeasureOrigin);
                    PointF p = map.Project(map.MousePosition);
                    map.Graphics.DrawString(Formatters.Angle.Format(angle), fontAngleValue, new SolidBrush(map.GetColor(Color.White)), p.X + 5, p.Y + 5);
                }
            }
        }
    }
}
