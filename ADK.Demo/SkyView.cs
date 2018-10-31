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
    public partial class SkyView : PictureBox
    {
        private Point pOld;
        private Point pNew;

        public SkyView()
        {
            InitializeComponent();
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

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            bool shift = (ModifierKeys & Keys.Shift) != Keys.None;

            if (e.Button == MouseButtons.Left && !shift)
            {
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
}
