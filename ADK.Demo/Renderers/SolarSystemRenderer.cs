﻿using ADK.Demo.Objects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo.Renderers
{
    public class SolarSystemRenderer : BaseSkyRenderer
    {
        private Pen penSun = new Pen(Color.FromArgb(250, 210, 10));
        private Brush brushMoon = new SolidBrush(Color.FromArgb(100, 100, 100));

        public SolarSystemRenderer(Sky sky, ISkyMap skyMap) : base(sky, skyMap)
        {

        }

        public override void Render(Graphics g)
        {
            RenderSun(g);
            RenderMoon(g);
        }

        private void RenderSun(Graphics g)
        {
            Sun sun = Sky.Get<Sun>("Sun");

            double ad = Angle.Separation(sun.Horizontal, Map.Center);
            if (ad < 1.2 * Map.ViewAngle + sun.Semidiameter / 3600.0)
            {
                PointF p = Map.Projection.Project(sun.Horizontal);

                float size = Math.Max(10, GetDrawingSize(sun.Semidiameter));

                g.FillEllipse(penSun.Brush, p.X - size / 2, p.Y - size / 2, size, size);
            }
        }

        private void RenderMoon(Graphics g)
        {
            Moon moon = Sky.Get<Moon>("Moon");

            double ad = Angle.Separation(moon.Horizontal, Map.Center);
            if (ad < 1.2 * Map.ViewAngle + moon.Semidiameter / 3600.0)
            {
                PointF p = Map.Projection.Project(moon.Horizontal);

                float size = Math.Max(10, GetDrawingSize(moon.Semidiameter));

                g.FillEllipse(Brushes.White, p.X - size / 2, p.Y - size / 2, size, size);

                // TODO: elongation/phase should have sign
                float phase = (float)moon.Phase * Math.Sign(moon.Elongation);

                PointF pNorth = Map.Projection.Project((moon.Equatorial + new CrdsEquatorial(0, 1)).ToHorizontal(Sky.GeoLocation, Sky.SiderealTime));

                double inc = Geometry.LineInclinationY(p, pNorth);

                g.DrawString(inc.ToString(), SystemFonts.DefaultFont, Brushes.Red, pNorth);

                g.DrawLine(Pens.Red, p, pNorth);
 



                // TODO: PA of cusps is needed
                float rot = (float)(inc + (360 - (moon.PositionAngleBrightLimb + 90)));

                Region shadow = GetPhaseShadow(phase, size, rot);

                g.TranslateTransform(p.X - size / 2, p.Y - size / 2);
                g.FillRegion(brushMoon, shadow);
                g.ResetTransform();                
            }
        }

        /// <summary>
        /// Gets drawing size of a celestial body
        /// </summary>
        /// <param name="semidiameter">Semidiameter of a body, in seconds of arc.</param>
        /// <returns></returns>
        private float GetDrawingSize(double semidiameter)
        {          
            return (float)(semidiameter / 3600.0 / Map.ViewAngle * Map.Width);
        }

        public Region GetPhaseShadow(float phase, float size, float rotation)
        {
            GraphicsPath gp_rect = new GraphicsPath();
            GraphicsPath gp_ell = new GraphicsPath();
            GraphicsPath gp_circle = new GraphicsPath();

            gp_circle.AddEllipse(0, 0, size, size);

            Region region_rect = new Region();
            Region region_ell;

            if (phase > 0 && phase <= 0.5)
            {
                gp_rect.AddRectangle(new RectangleF(0, 0, size / 2, size));
                gp_ell.AddEllipse(phase * size, 0, (0.5f - phase) * size * 2, size);
                region_rect = new Region(gp_rect);
                region_ell = new Region(gp_ell);
                region_rect.Union(region_ell);
            }
            if (phase > 0.5 && phase <= 1.0)
            {
                gp_rect.AddRectangle(new RectangleF(0, 0, size / 2, size));
                gp_ell.AddEllipse(phase * size, 0, (0.5f - phase) * size * 2, size);
                region_rect = new Region(gp_rect);
                region_ell = new Region(gp_ell);
                region_rect.Exclude(region_ell);
            }

            if (phase > -1.0 && phase <= -0.5)
            {
                gp_rect.AddRectangle(new RectangleF(size / 2, 0, size / 2, size));
                gp_ell.AddEllipse(-phase * size, 0, (0.5f + phase) * size * 2, size);
                region_rect = new Region(gp_rect);
                region_ell = new Region(gp_ell);
                region_rect.Exclude(region_ell);
            }

            if (phase > -0.5 && phase <= 0)
            {
                gp_rect.AddRectangle(new RectangleF(size / 2, 0, size / 2, size));
                gp_ell.AddEllipse(-phase * size, 0, (0.5f + phase) * size * 2, size);
                region_rect = new Region(gp_rect);
                region_ell = new Region(gp_ell);
                region_rect.Union(region_ell);
            }

            region_rect.Intersect(gp_circle);

            region_rect.Transform(RotateAroundPoint(rotation, new PointF(size / 2, size / 2)));

            return region_rect;
        }

        // Return a rotation matrix to rotate clockwise around a point.
        private Matrix RotateAroundPoint(float rotation, PointF center)
        {
            Matrix result = new Matrix();
            result.RotateAt(rotation, center);
            return result;
        }
    }
}