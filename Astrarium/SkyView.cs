using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Astrarium
{
    [DesignerCategory("code")]
    internal partial class SkyView : Control
    {
        private Point pOld;
        private Point pNew;

        public SkyView()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, false);
            this.SetStyle(ControlStyles.Opaque, false);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            DoubleBuffered = true;
            Cursor = Cursors.Cross;
            ForeColor = Color.White;
            BackColor = Color.Black;
        }

        [Description("Gets or sets ISkyMap object to be rendered in the control.")]
        private SkyMap mSkyMap = null;
        public SkyMap SkyMap
        {
            get { return mSkyMap; }
            set
            {
                mSkyMap = value;
                if (mSkyMap != null)
                {
                    mSkyMap.Width = Width;
                    mSkyMap.Height = Height;
                    mSkyMap.OnInvalidate += InvalidateWithDoEvents;
                    mSkyMap.OnRedraw += () => Invoke(Redraw);
                }
            }
        }

        public event Action Redraw;

        protected override void OnPaint(PaintEventArgs pe)
        {
            if (DesignMode || SkyMap == null)
            {
                pe.Graphics.DrawString("SkyView", Font, new SolidBrush(ForeColor), 10, 10);
            }
            else
            {
                SkyMap.Render(pe.Graphics);
                
                /*
                // Below is a snippet how to draw mirrored part of image
                  
                var gr = pe.Graphics;
                using (var img = new Bitmap(Width, Height))
                using (var g = Graphics.FromImage(img))                
                {
                    SkyMap.Render(g);

                    // Draw original bitmap
                    gr.DrawImageUnscaled(img, 0, 0);

                    using (var gp = new GraphicsPath())
                    {
                        gp.AddEllipse(new RectangleF(Width / 2 - 128, Height / 2 - 128, 256, 256));
                        img.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        gr.SetClip(gp);
                        gr.DrawImageUnscaled(img, 0, 0);
                        gr.ResetClip();
                    }
                }
                */
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
                SkyMap.MouseButton = MouseButton.Left;
                pOld.X = e.X;
                pOld.Y = e.Y;
            }

            SkyMap.MousePosition = SkyMap.Projection.Invert(new PointF(e.X, e.Y));
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);          
            SkyMap.Antialias = true;
            SkyMap.MouseButton = MouseButton.None;
            SkyMap.MousePosition = SkyMap.Projection.Invert(new PointF(e.X, e.Y));
            Cursor = Cursors.Cross;
            Invalidate();
            pOld = Point.Empty;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);            
            //SkyMap.MousePosition = null;
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (SkyMap != null)
            {
                switch (e.Button)
                {
                    case MouseButtons.Left:
                        SkyMap.MouseButton = MouseButton.Left;
                        break;
                    case MouseButtons.Right:
                        SkyMap.MouseButton = MouseButton.Right;
                        break;
                    default:
                        SkyMap.MouseButton = MouseButton.None;
                        break;
                }

                CrdsHorizontal newMousePosition = SkyMap.Projection.Invert(new PointF(e.X, e.Y));

                if (SkyMap.MouseButton == MouseButton.Left)
                {
                    if (SkyMap.LockedObject == null)
                    {
                        if (pOld == Point.Empty)
                        {
                            pOld = new Point(e.X, e.Y);
                        }

                        pNew.X = e.X;
                        pNew.Y = e.Y;
                        double dx = (SkyMap.Projection.IsMirrored ? -1 : 1) * (pNew.X - pOld.X);
                        double dy = (SkyMap.Projection.IsInverted ? -1 : 1) * (pNew.Y - pOld.Y);

                        SkyMap.Antialias = Math.Sqrt(dx * dx + dy * dy) < 30;

                        double maxSize = Math.Max(SkyMap.Width, SkyMap.Height);
                        double f = maxSize / (SkyMap.ViewAngle * 2);

                        if (Math.Abs(SkyMap.Center.Altitude) < 30 || SkyMap.ViewAngle > 80)
                        {
                            SkyMap.Center.Azimuth = (SkyMap.Center.Azimuth - dx / f + 360) % 360;
                        }
                        else
                        {
                            CrdsHorizontal cpNew = SkyMap.Projection.Invert(pNew);
                            CrdsHorizontal cpOld = SkyMap.Projection.Invert(pOld);
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
                    else
                    {
                        if (Cursor != Cursors.No)
                        {
                            Invalidate();
                            Cursor = Cursors.No;
                        }
                    }

                    SkyMap.MousePosition = newMousePosition;
                }
                else
                {
                    SkyMap.MousePosition = newMousePosition;
                }
            }
        }

        private void InvalidateWithDoEvents()
        {
            Invalidate();
            Application.DoEvents();
        }
    }
}
