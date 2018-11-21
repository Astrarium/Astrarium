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
        private Brush brushShadow = new SolidBrush(Color.FromArgb(100, 100, 100));

        public SolarSystemRenderer(Sky sky, ISkyMap skyMap) : base(sky, skyMap)
        {

        }

        public override void Render(Graphics g)
        {
            RenderSun(g);
            RenderMoon(g);

            var planets = Sky.Get<ICollection<Planet>>("Planets");

            for (int i = 0; i < planets.Count; i++)
            {
                if (i + 1 != 3)
                {
                    RenderPlanet(g, planets.ElementAt(i));
                }
            }
        }

        private void RenderSun(Graphics g)
        {
            Sun sun = Sky.Get<Sun>("Sun");

            double ad = Angle.Separation(sun.Horizontal, Map.Center);
            if (ad < 1.2 * Map.ViewAngle + sun.Semidiameter / 3600.0)
            {
                PointF p = Map.Projection.Project(sun.Horizontal);

                float size = GetDiskSize(sun.Semidiameter, 10);

                g.FillEllipse(penSun.Brush, p.X - size / 2, p.Y - size / 2, size, size);

                Map.VisibleObjects.Add(sun);
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
                float size = GetDiskSize(moon.Semidiameter, 10);
             
                // rotation of image around North pole
                // double inc = GetRotationTowardsNorth(moon.Equatorial);
                // final rotation of drawn image
                // cusp rotation is negated because measured counter-clockwise
                // float rot = (float)(inc - moon.PAcusp);

                // signed value of Moon phase
                float phase = (float)moon.Phase * Math.Sign(moon.Elongation);

                // Moon phase shadow
                // Region shadow = GetPhaseShadow(phase, size, rot);

                Region shadow = GetPhaseShadow(phase, size + 2, GetRotationTowardsEclipticPole(moon.Ecliptical));

                g.FillEllipse(Brushes.White, p.X - size / 2, p.Y - size / 2, size, size);

                // first method
                // g.TranslateTransform(p.X - size / 2, p.Y - size / 2);
                // g.FillRegion(brushMoon, shadow);
                // g.ResetTransform();

                // second method
                g.TranslateTransform(p.X - size / 2 - 1, p.Y - size / 2 - 1);
                g.FillRegion(brushShadow, shadow);
                g.ResetTransform();

                Map.VisibleObjects.Add(moon);
            }
        }

        private void RenderPlanet(Graphics g, Planet planet)
        {
            double ad = Angle.Separation(planet.Horizontal, Map.Center);
            
            if (ad < 1.2 * Map.ViewAngle + planet.Semidiameter / 3600.0)
            {
                float size = GetPointSize(planet.Magnitude);
                float diam = GetDiskSize(planet.Semidiameter);

                // diameter is to small to render as planet disk, 
                // but point size caclulated from magnitude is enough to be drawn
                if (size > diam && (int)size > 0)
                {
                    PointF p = Map.Projection.Project(planet.Horizontal);
                    g.FillEllipse(GetPlanetColor(planet.Serial), p.X - size / 2, p.Y - size / 2, size, size);

                    DrawObjectCaption(g, planet.Names.ElementAt(0), p, size);

                    Map.VisibleObjects.Add(planet);
                }

                // planet should be rendered as disk
                else if (diam >= size && (int)diam > 0)
                {
                    PointF p = Map.Projection.Project(planet.Horizontal);

                    // TODO: Saturn rings, rotation of planets

                    float diamEquat = diam;
                    float diamPolar = (1 - planet.Flattening) * diam;

                    float rotation = GetRotationTowardsEclipticPole(planet.Ecliptical);

                    g.TranslateTransform(p.X - diamEquat / 2, p.Y - diamPolar / 2);
                    g.RotateTransform(rotation);
                    g.FillEllipse(GetPlanetColor(planet.Serial), 0, 0, diamEquat, diamPolar);
                    g.ResetTransform();

                    // drawing shadow on the almost full phase makes no sense
                    if (planet.Phase < 0.99)
                    {
                        float phase = (float)planet.Phase * Math.Sign(planet.Elongation);

                        Region shadow = GetPhaseShadow(phase, diam + 2, rotation);

                        g.TranslateTransform(p.X - diamEquat / 2 - 1, p.Y - diamPolar / 2 - 1);
                        g.FillRegion(brushShadow, shadow);
                        g.ResetTransform();
                    }

                    DrawObjectCaption(g, planet.Names.ElementAt(0), p, diam);

                    Map.VisibleObjects.Add(planet);
                }
            }
        }

        private Brush GetPlanetColor(int planet)
        {
            switch (planet)
            {
                case 1:
                    return Brushes.LightGray;
                case 2:
                    return Brushes.White;
                case 4:
                    return Brushes.DarkRed;
                case 5:
                    return Brushes.LightYellow;
                case 6:
                    return Brushes.LightYellow;
                case 7:
                    return Brushes.LightGreen;
                case 8:
                    return Brushes.LightSkyBlue;
                default:
                    return Brushes.White;
            }
        }

        private Region GetPhaseShadow(float phase, float size, float rotation)
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

        /// <summary>
        /// Gets drawing rotation of image, measured clockwise from 
        /// a point oriented to top of the screen towards North ecliptic pole point 
        /// </summary>
        /// <param name="ecl">Ecliptical coordinates of a central point of a body.</param>
        /// <returns></returns>
        private float GetRotationTowardsEclipticPole(CrdsEcliptical ecl)
        {
            // Coordinates of center of a body (image) to be rotated
            PointF p = Map.Projection.Project(ecl.ToEquatorial(Sky.Epsilon).ToHorizontal(Sky.GeoLocation, Sky.SiderealTime));

            // Point directed to North ecliptic pole
            PointF pNorth = Map.Projection.Project((ecl + new CrdsEcliptical(0, 1)).ToEquatorial(Sky.Epsilon).ToHorizontal(Sky.GeoLocation, Sky.SiderealTime));

            // Clockwise rotation
            return (float)Geometry.LineInclinationY(p, pNorth);
        }
    }
}
