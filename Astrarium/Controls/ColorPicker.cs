using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Astrarium.Controls
{
    /// <summary>
    /// Implements simple color picker based on Hue-Saturation color wheel and Value slider
    /// </summary>
    [DesignerCategory("Code")]
    public class ColorPicker : Control
    {
        /// <summary>
        /// Flag indicating mouse captured inside the color wheel
        /// </summary>
        private bool mouseInWheelCapture = false;

        /// <summary>
        /// Flag indicating mouse captured inside the value slider
        /// </summary>
        private bool mouseInSliderCapture = false;

        /// <summary>
        /// Value slider bounding rectangle
        /// </summary>
        private Rectangle slider = Rectangle.Empty;

        /// <summary>
        /// Color wheel bounding rectangle
        /// </summary>
        private Rectangle colorWheel = Rectangle.Empty;

        /// <summary>
        /// Color wheel radius
        /// </summary>
        private int radius = 0;

        /// <summary>
        /// Color wheel center point
        /// </summary>
        private Point center = Point.Empty;

        /// <summary>
        /// Coordinates of selected color inside the color wheel
        /// </summary>
        private Point colorPoint = Point.Empty;

        /// <summary>
        /// Current mouse coordinates
        /// </summary>
        private Point mousePoint = Point.Empty;

        /// <summary>
        /// Backing field for <see cref="SelectedColor"/> property.
        /// </summary>
        private Color _SelectedColor = Color.White;

        /// <summary>
        /// Backing field for <see cref="Hue"/> property.
        /// </summary>
        private double _Hue = 0;

        /// <summary>
        /// Backing field for <see cref="Saturation"/> property.
        /// </summary>
        private double _Saturation = 0;

        /// <summary>
        /// Backing field for <see cref="Value"/> property.
        /// </summary>
        private double _Value = 0;

        /// <summary>
        /// Backing field for <see cref="SliderOnly"/> property.
        /// </summary>
        private bool _SliderOnly = false;

        /// <summary>
        /// Color to highlight value slider when mouse is over
        /// </summary>
        public Color SliderThumbsHighlightColor { get; set; } = Color.White;

        /// <summary>
        /// Color of slider thumbs
        /// </summary>
        public Color SliderThumbsColor { get; set; } = Color.Black;

        /// <summary>
        /// Color of slider border
        /// </summary>
        public Color SliderBorderColor { get; set; } = Color.Gray;

        /// <summary>
        /// Color of wheel border
        /// </summary>
        public Color WheelBorderColor { get; set; } = Color.Transparent;

        /// <summary>
        /// Inner color of selection circle (ringlet) on the wheel
        /// </summary>
        public Color RingletInnerColor { get; set; } = Color.Black;

        /// <summary>
        /// Outer color of selection circle (ringlet) on the wheel
        /// </summary>
        public Color RingletOuterColor { get; set; } = Color.White;

        /// <summary>
        /// Gets or sets current color that selected in the picker
        /// </summary>
        public Color SelectedColor
        {
            get => _SelectedColor;
            set
            {
                _SelectedColor = value;
                ColorToHSV(_SelectedColor, out _Hue, out _Saturation, out _Value);
                Invalidate();
                SelectedColorChanged?.Invoke();
            }
        }

        /// <summary>
        /// Gets hue component of selected color in HSV system.
        /// </summary>
        public double Hue => _Hue;

        /// <summary>
        /// Gets saturation component of selected color in HSV system.
        /// </summary>
        public double Saturation => _Saturation;

        /// <summary>
        /// Gets value component of selected color in HSV system.
        /// </summary>
        public double Value => _Value;

        /// <summary>
        /// Flag indicating to display or not the color wheel
        /// </summary>
        public bool SliderOnly
        {
            get => _SliderOnly;
            set
            {
                _SliderOnly = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Fired when selected color is changed
        /// </summary>
        public event Action SelectedColorChanged;

        /// <summary>
        /// Creates new instance of the picker
        /// </summary>
        public ColorPicker()
        {
            DoubleBuffered = true;
            SelectedColor = Color.White;
        }

        protected override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);
            Invalidate();
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            Invalidate();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            colorWheel.Width = Width - 20;
            colorWheel.Height = colorWheel.Width;
            colorWheel.X = (Width - colorWheel.Width) / 2;
            colorWheel.Y = 10;

            radius = colorWheel.Width / 2;
            center.X = colorWheel.X + radius;
            center.Y = 10 + radius;

            slider.X = colorWheel.X - 5;
            slider.Y = SliderOnly ? 20 : colorWheel.Height + 20;
            slider.Width = colorWheel.Width + 10;
            slider.Height = 10;

            double phi = -_Hue * Math.PI / 180;
            double r = _Saturation * radius;
            double cos = Math.Cos(phi);
            double sin = Math.Sin(phi);
            colorPoint.X = (int)(center.X + r * cos);
            colorPoint.Y = (int)(center.Y + r * sin);

            if (!SliderOnly)
            {
                DrawColorWheel(e.Graphics);
            }

            DrawValueSlider(e.Graphics);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left && IsInsideColorWheel(e.Location))
            {
                mouseInWheelCapture = true;
                PickColorFromWheel(e.Location);
                Invalidate();
            }
            else
            {
                mouseInWheelCapture = false;
            }

            if (e.Button == MouseButtons.Left && IsInsideValueSlider(e.Location))
            {
                mouseInSliderCapture = true;
                PickColorFromSlider(e.Location);
                Invalidate();
            }
            else
            {
                mouseInSliderCapture = false;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            mouseInWheelCapture = false;
            mouseInSliderCapture = false;
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            mousePoint = e.Location;

            if (mouseInWheelCapture)
            {
                PickColorFromWheel(mousePoint);
            }

            if (mouseInSliderCapture)
            {
                PickColorFromSlider(mousePoint);
            }

            if (IsInsideColorWheel(mousePoint))
            {
                Cursor = Cursors.Cross;
            }
            else
            {
                Cursor = Cursors.Default;
            }

            Invalidate();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (IsInsideValueSlider(e.Location))
            {
                _Value = (_Value * 256 - (double)e.Delta / SystemInformation.MouseWheelScrollDelta) / 256;
                if (_Value > 1) _Value = 1;
                if (_Value < 0) _Value = 0;
                _SelectedColor = ColorFromHSV(_Hue, _Saturation, _Value);
                Invalidate();
                SelectedColorChanged?.Invoke();
            }
        }

        private void PickColorFromWheel(Point p)
        {
            double cos = p.X - center.X;
            double sin = p.Y - center.Y;
            double phi = Math.Atan2(sin, cos);

            if (IsInsideColorWheel(p))
            {
                colorPoint = p;
            }
            else
            {
                colorPoint = new Point(center.X + (int)(Math.Cos(phi) * radius), center.Y + (int)(Math.Sin(phi) * radius));
            }

            double h = -phi / Math.PI * 180;
            h = (h % 360 + 360) % 360;

            double s = Math.Sqrt((colorPoint.X - center.X) * (colorPoint.X - center.X) + (colorPoint.Y - center.Y) * (colorPoint.Y - center.Y)) / radius;

            _SelectedColor = ColorFromHSV(h, s, _Value);
            _Hue = h;
            _Saturation = s;
            SelectedColorChanged?.Invoke();
        }

        private void PickColorFromSlider(Point p)
        {
            if (p.X <= slider.Left)
                p.X = slider.Left;

            if (p.X >= slider.Right)
                p.X = slider.Right;

            if (p.Y <= slider.Top)
                p.Y = slider.Top;

            if (p.Y >= slider.Bottom)
                p.Y = slider.Bottom;

            _Value = (double)(p.X - slider.Left) / slider.Width;
            _SelectedColor = ColorFromHSV(_Hue, _Saturation, _Value);
            SelectedColorChanged?.Invoke();
        }

        private bool IsInsideColorWheel(PointF p)
        {
            return !SliderOnly && Math.Sqrt((p.X - center.X) * (p.X - center.X) + (p.Y - center.Y) * (p.Y - center.Y)) <= radius;
        }

        private bool IsInsideValueSlider(PointF p)
        {
            return p.X >= slider.Left && p.X <= slider.Right && p.Y >= slider.Top - 10 && p.Y <= slider.Bottom + 10;
        }

        private void DrawValueSlider(Graphics gr)
        {
            Color toColor = ColorFromHSV(_Hue, _Saturation, 1);
            gr.FillRectangle(new LinearGradientBrush(slider, Color.Black, toColor, 0f), slider);
            gr.DrawRectangle(new Pen(SliderBorderColor), slider);

            int val_x = slider.X + (int)(_Value * slider.Width);

            // triangle size
            int sz = 5;

            // triangle offset
            int offset = 2;

            // down triangle
            using (GraphicsPath gp = new GraphicsPath())
            {
                gp.AddLines(new[] { new Point(val_x, slider.Bottom + offset), new Point(val_x - sz, slider.Bottom + sz + offset), new Point(val_x + sz, slider.Bottom + sz + offset) });
                gp.CloseFigure();
                gr.FillPath(new SolidBrush(SliderThumbsColor), gp);
                if (IsInsideValueSlider(mousePoint) || mouseInSliderCapture)
                {
                    gr.DrawPath(new Pen(SliderThumbsHighlightColor), gp);
                }
            }

            // up triangle
            using (GraphicsPath gp = new GraphicsPath())
            {
                gp.AddLines(new[] { new Point(val_x, slider.Y - offset), new Point(val_x - sz, slider.Y - sz - offset), new Point(val_x + sz, slider.Y - sz - offset) });
                gp.CloseFigure();
                gr.FillPath(new SolidBrush(SliderThumbsColor), gp);
                if (IsInsideValueSlider(mousePoint) || mouseInSliderCapture)
                {
                    gr.DrawPath(new Pen(SliderThumbsHighlightColor), gp);
                }
            }
        }

        private void DrawColorWheel(Graphics gr)
        {
            using (var wheelPath = new GraphicsPath())
            {
                wheelPath.AddEllipse(colorWheel);
                wheelPath.Flatten();

                float pointsCount = (wheelPath.PointCount - 1) / 6;
                var surroundColors = new Color[wheelPath.PointCount];

                int v = (int)(_Value * 255);

                int index = 0;
                InterpolateColors(surroundColors, ref index, 1 * pointsCount, Color.FromArgb(v, 0, 0), Color.FromArgb(v, 0, v));
                InterpolateColors(surroundColors, ref index, 2 * pointsCount, Color.FromArgb(v, 0, v), Color.FromArgb(0, 0, v));
                InterpolateColors(surroundColors, ref index, 3 * pointsCount, Color.FromArgb(0, 0, v), Color.FromArgb(0, v, v));
                InterpolateColors(surroundColors, ref index, 4 * pointsCount, Color.FromArgb(0, v, v), Color.FromArgb(0, v, 0));
                InterpolateColors(surroundColors, ref index, 5 * pointsCount, Color.FromArgb(0, v, 0), Color.FromArgb(v, v, 0));
                InterpolateColors(surroundColors, ref index, wheelPath.PointCount, Color.FromArgb(v, v, 0), Color.FromArgb(v, 0, 0));

                using (PathGradientBrush brush = new PathGradientBrush(wheelPath))
                {
                    brush.CenterColor = Color.FromArgb(v, v, v);
                    brush.SurroundColors = surroundColors;

                    gr.FillPath(brush, wheelPath);

                    // It looks better if we outline the wheel
                    using (Pen pen = new Pen(BackColor, 2))
                    {
                        gr.DrawEllipse(pen, colorWheel);
                    }

                    // Draw wheel border
                    using (Pen pen = new Pen(WheelBorderColor))
                    {
                        gr.DrawEllipse(pen, new RectangleF(colorWheel.X + 1, colorWheel.Y + 1, colorWheel.Width - 2, colorWheel.Height - 2));
                    }
                }
            }

            // draw ringlet
            gr.FillEllipse(new SolidBrush(_SelectedColor), colorPoint.X - 5, colorPoint.Y - 5, 10, 10);
            gr.DrawEllipse(new Pen(RingletInnerColor), colorPoint.X - 5, colorPoint.Y - 5, 10, 10);
            gr.DrawEllipse(new Pen(RingletOuterColor), colorPoint.X - 5.5f, colorPoint.Y - 5.5f, 11, 11);
        }

        private void InterpolateColors(Color[] surround_colors, ref int index, float stop_pt, Color from, Color to)
        {
            int from_a = from.A;
            int from_r = from.R;
            int from_g = from.G;
            int from_b = from.B;

            int to_a = to.A;
            int to_r = to.R;
            int to_g = to.G;
            int to_b = to.B;

            int num_pts = (int)stop_pt - index;
            float a = from_a, r = from_r, g = from_g, b = from_b;
            float da = (to_a - from_a) / (num_pts - 1);
            float dr = (to_r - from_r) / (num_pts - 1);
            float dg = (to_g - from_g) / (num_pts - 1);
            float db = (to_b - from_b) / (num_pts - 1);

            for (int i = 0; i < num_pts; i++)
            {
                surround_colors[index++] = Color.FromArgb((int)a, (int)r, (int)g, (int)b);
                a += da;
                r += dr;
                g += dg;
                b += db;
            }
        }

        private static void ColorToHSV(Color color, out double hue, out double saturation, out double value)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            hue = color.GetHue();
            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }

        private static Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }
    }
}
