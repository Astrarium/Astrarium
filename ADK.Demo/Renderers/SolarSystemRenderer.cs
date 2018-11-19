using ADK.Demo.Objects;
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

                // drawing size
                float size = Math.Max(10, GetDrawingSize(moon.Semidiameter));
             
                // rotation of image around North pole
                double inc = GetRotationTowardsNorth(moon.Equatorial);

                // final rotation of drawn image
                // cusp rotation is negated because measured counter-clockwise
                float rot = (float)(inc - moon.PAcusp);

                // signed value of Moon phase
                float phase = (float)moon.Phase * Math.Sign(moon.Elongation);

                // Moon phase shadow
                Region shadow = GetPhaseShadow(phase, size, rot);

                g.FillEllipse(Brushes.White, p.X - size / 2, p.Y - size / 2, size, size);
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

        /// <summary>
        /// Gets drawing rotation of image, measured clockwise from 
        /// a point oriented to top of the screen towards North celestial pole point 
        /// </summary>
        /// <param name="eq">Equatorial coordinates of a central point of a body.</param>
        /// <returns></returns>
        private float GetRotationTowardsNorth(CrdsEquatorial eq)
        {
            // Coordinates of center of a body (image) to be rotated
            PointF p = Map.Projection.Project(eq.ToHorizontal(Sky.GeoLocation, Sky.SiderealTime));

            // Point directed to North celestial pole
            PointF pNorth = Map.Projection.Project((eq + new CrdsEquatorial(0, 1)).ToHorizontal(Sky.GeoLocation, Sky.SiderealTime));

            // Clockwise rotation
            return (float)Geometry.LineInclinationY(p, pNorth);
        }
    }
}
