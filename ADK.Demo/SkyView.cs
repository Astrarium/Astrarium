using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ADK.Demo
{
    [DesignerCategory("code")]
    public partial class SkyView : Control
    {
        private Point pOld;
        private Point pNew;
        private bool isMouseMoving = false;

        public SkyView()
        {
            InitializeComponent();
            DoubleBuffered = true;
            Cursor = Cursors.Cross;
        }

        [Description("Gets or sets ISkyMap object to be rendered in the control.")]
        private ISkyMap mSkyMap = null;
        public ISkyMap SkyMap
        {
            get { return mSkyMap; }
            set
            {
                mSkyMap = value;
                if (mSkyMap != null)
                {
                    mSkyMap.Width = Width;
                    mSkyMap.Height = Height;
                }
            }
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            if (DesignMode || SkyMap == null)
            {
                pe.Graphics.FillRectangle(Brushes.Black, 0, 0, Width, Height);
                pe.Graphics.DrawString("SkyView", SystemFonts.DefaultFont, Brushes.White, 10, 10);
                System.Diagnostics.Trace.WriteLine("OnPaint");
            }
            else
            {
                pe.Graphics.SetClip(new Rectangle(0, 0, Width, Height));
                SkyMap.Antialias = !isMouseMoving;
                SkyMap.Render(pe.Graphics);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (SkyMap != null)
            {
                SkyMap.Width = Width;
                SkyMap.Height = Height;
                Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            bool shift = (ModifierKeys & Keys.Shift) != Keys.None;

            if (e.Button == MouseButtons.Left && !shift)
            {
                pOld.X = e.X;
                pOld.Y = e.Y;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            isMouseMoving = false;
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            Select();

            if (SkyMap != null)
            {
                bool shift = (ModifierKeys & Keys.Shift) != Keys.None;

                if (e.Button == MouseButtons.Left && !shift)
                {
                    isMouseMoving = true;

                    pNew.X = e.X;
                    pNew.Y = e.Y;
                    double dx = pNew.X - pOld.X;
                    double dy = pNew.Y - pOld.Y;

                    double f = SkyMap.Width / (SkyMap.ViewAngle * 2);

                    if (Math.Abs(SkyMap.Center.Altitude) < 30 || SkyMap.ViewAngle > 80)
                    {
                        SkyMap.Center.Azimuth = (SkyMap.Center.Azimuth - dx / f + 360) % 360;
                    }
                    else
                    {
                        CrdsHorizontal cpNew = SkyMap.CoordinatesByPoint(pNew);
                        CrdsHorizontal cpOld = SkyMap.CoordinatesByPoint(pOld);
                        double da = Math.Abs(cpNew.Azimuth - cpOld.Azimuth);
                        da = Math.Abs(da) * Math.Sign(dx);
                        SkyMap.Center.Azimuth -= da;
                        SkyMap.Center.Azimuth %= 360;
                    }

                    SkyMap.Center.Altitude += dy / f;

                    if (SkyMap.Center.Altitude > 90) SkyMap.Center.Altitude = 90;
                    if (SkyMap.Center.Altitude < -90) SkyMap.Center.Altitude = -90;

                    if (double.IsNaN(SkyMap.Center.Azimuth))
                    {
                        SkyMap.Center.Azimuth = 0;
                    }

                    pOld.X = pNew.X;
                    pOld.Y = pNew.Y;

                    Invalidate();
                }
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (SkyMap != null)
            {
                double v = SkyMap.ViewAngle;

                if (e.Delta < 0)
                {
                    v *= 1.1;
                }
                else
                {
                    v /= 1.1;
                }

                if (v >= 90)
                {
                    v = 90;
                }
                if (v < 1.0 / 1024.0)
                {
                    v = 1.0 / 1024.0;
                }

                SkyMap.ViewAngle = v;            
                Invalidate();
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            
            // Add = Zoom In
            if (e.KeyCode == Keys.Add)
            {
                OnMouseWheel(new MouseEventArgs(MouseButtons.None, 0, 0, 0, 1));
            }

            // Subtract = Zoom Out
            if (e.KeyCode == Keys.Subtract)
            {
                OnMouseWheel(new MouseEventArgs(MouseButtons.None, 0, 0, 0, -1));
            }
        }
    }
}
