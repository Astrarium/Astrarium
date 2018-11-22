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
            Sun sun = Sky.Get<Sun>("Sun");
            Moon moon = Sky.Get<Moon>("Moon");

            // Flag indicated Sun is already rendered
            bool isSunRendered = false;

            // Get all planets esxept Earth, and sort them by distance from Earth (most distant planet is first)
            var planets = Sky.Get<ICollection<Planet>>("Planets")
                .Where(p => p.Number != 3)
                .OrderByDescending(p => p.Ecliptical.Distance);

            foreach (Planet p in planets)
            {
                if (!isSunRendered && p.Ecliptical.Distance < sun.Ecliptical.Distance)
                {
                    RenderSun(g, sun);
                    isSunRendered = true;
                }

                RenderPlanet(g, p);
            }

            RenderMoon(g, moon);
        }

        private void RenderSun(Graphics g, Sun sun)
        {
            double ad = Angle.Separation(sun.Horizontal, Map.Center);
            if (ad < 1.2 * Map.ViewAngle + sun.Semidiameter / 3600.0)
            {
                PointF p = Map.Projection.Project(sun.Horizontal);

                float size = GetDiskSize(sun.Semidiameter, 10);

                g.FillEllipse(penSun.Brush, p.X - size / 2, p.Y - size / 2, size, size);

                Map.VisibleObjects.Add(sun);
            }
        }

        private void RenderMoon(Graphics g, Moon moon)
        {
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
                // float rotation = (float)(inc - moon.PAcusp);

                // Moon disk
                g.FillEllipse(Brushes.White, p.X - size / 2, p.Y - size / 2, size, size);

                float phase = (float)moon.Phase * Math.Sign(moon.Elongation);
                float rotation = GetRotationTowardsEclipticPole(moon.Ecliptical);
                GraphicsPath shadow = GetPhaseShadow(phase, size + 1);

                // shadowed part of disk
                g.TranslateTransform(p.X, p.Y);
                g.RotateTransform(rotation);
                g.FillPath(brushShadow, shadow);
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
                    g.FillEllipse(GetPlanetColor(planet.Number), p.X - size / 2, p.Y - size / 2, size, size);

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

                    g.TranslateTransform(p.X, p.Y);
                    g.RotateTransform(rotation);
                    g.FillEllipse(GetPlanetColor(planet.Number), -diamEquat / 2, -diamPolar / 2, diamEquat, diamPolar);

                    //g.FillEllipse(GetVolumeBrush(diam, planet.Flattening), -diamEquat / 2, -diamPolar / 2, diamEquat, diamPolar);

                    g.ResetTransform();

                    float phase = (float)planet.Phase * Math.Sign(planet.Elongation);

                    GraphicsPath shadow = GetPhaseShadow(phase, diam + 1, planet.Flattening);

                    g.TranslateTransform(p.X, p.Y);
                    g.RotateTransform(rotation);
                    g.FillPath(brushShadow, shadow);
                    g.ResetTransform();
                    
                    // TODO: Remove marker on center of the disk. For testing only.
                    g.FillEllipse(Brushes.Red, p.X - 2, p.Y - 2, 4, 4);

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

        /// <summary>
        /// Gets graphics path for drawing shadowed part of a planet / Moon.
        /// </summary>
        /// <param name="phase">Phase of celestial object (signed).</param>
        /// <param name="rotation">
        /// Rotation angle in degrees. 
        /// Resulting graphics path will be rotated clockwise on this angle around central point of the planet / Moon disk.</param>
        /// <param name="size">Size of a drawn planet / Moon disk</param>
        /// <param name="flattening">Flattening value of a planet globe.</param>
        /// <returns>Graphics path for drawing shadowed part of a planet / Moon.</returns>
        private GraphicsPath GetPhaseShadow(float phase, float size, float flattening = 0)
        {
            float sizeEquat = size;
            float sizePolar = (1 - flattening) * size;

            GraphicsPath gp = new GraphicsPath();
            
            // растущий серп
            if (phase >= 0 && phase <= 0.5)
            {
                float width = (0.5f - phase) * sizeEquat * 2;
                float height = sizePolar;
                float x = -width / 2;
                float y = -height / 2;

                // terminator arc
                gp.AddArc(x, y, width, height, -90, 180);

                // dark side arc
                gp.AddArc(-sizeEquat / 2, -sizePolar / 2, sizeEquat, sizePolar, 90, 180);
            }

            // растущая горбушка
            if (phase > 0.5 && phase <= 1.0)
            {
                float width = (phase - 0.5f) * sizeEquat * 2;
                float height = sizePolar;
                float x = -width / 2;
                float y = -height / 2;

                // terminator arc 
                gp.AddArc(x, y, width, height, 90, 180);
                gp.Reverse();

                // dark side arc 
                gp.AddArc(-sizeEquat / 2, -sizePolar / 2, sizeEquat, sizePolar, 90, 180);
            }

            // убывающая горбушка 
            if (phase > -1.0 && phase <= -0.5)
            {
                float width = -(phase + 0.5f) * sizeEquat * 2;
                float height = sizePolar;
                float x = -width / 2;
                float y = -height / 2;

                // terminator arc
                gp.AddArc(x, y, width, height, -90, 180);
                gp.Reverse();

                // dark side arc
                gp.AddArc(-sizeEquat / 2, -sizePolar / 2, sizeEquat, sizePolar, -90, 180);
            }

            // убывающий серп
            if (phase > -0.5 && phase <= 0)
            {
                float width = (phase + 0.5f) * sizeEquat * 2;
                float height = sizePolar;
                float x = -width / 2;
                float y = -height / 2;

                // dark side arc
                gp.AddArc(-sizeEquat / 2, -sizePolar / 2, sizeEquat, sizePolar, -90, 180);

                // terminator arc
                gp.AddArc(x, y, width, height, 90, 180);
            }

            gp.CloseAllFigures();

            return gp;
        }

        private PathGradientBrush GetVolumeBrush(float size, float flattening = 0)
        {
            float sizeEquat = size;
            float sizePolar = (1 - flattening) * size;

            GraphicsPath gpVolume = new GraphicsPath();
            gpVolume.AddEllipse(-sizeEquat / 2, -sizePolar / 2, sizeEquat, sizePolar);

            PathGradientBrush brushVolume = new PathGradientBrush(gpVolume);
            brushVolume.CenterPoint = new PointF(0, 0);
            brushVolume.CenterColor = Color.Transparent;

            //Blend blnd = new Blend();
            //blnd.Positions = new float[] { 0, 0.3f, 0.4f, 1 };
            //blnd.Factors = new float[] {  0, 1, 1, 1 };
            //brushVolume.Blend = blnd;

            //brushVolume.SetBlendTriangularShape(1, 0.5f);
            brushVolume.SetSigmaBellShape((float)0.3, (float)1.0);
            List<Color> clrs = new List<Color>();
            for (int i = 0; i < gpVolume.PathPoints.Length; i++)
            {
                clrs.Add(Color.FromArgb(255, Color.Black));
            }
            brushVolume.SurroundColors = clrs.ToArray();

            return brushVolume;
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
