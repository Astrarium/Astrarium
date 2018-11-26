using ADK.Demo.Objects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ADK.Demo.Renderers
{
    public class SolarSystemRenderer : BaseSkyRenderer
    {
        private Pen penSun = new Pen(Color.FromArgb(250, 210, 10));
        private Brush brushShadow = new SolidBrush(Color.FromArgb(200, 0, 0, 0));
        private Brush[] brushRings = new Brush[] 
        {
            new SolidBrush(Color.FromArgb(200, 224, 224, 195)),
            new SolidBrush(Color.FromArgb(200, 224, 224, 195)),
            new SolidBrush(Color.FromArgb(32, 0, 0, 0))
        };

        private SphereRenderer sphereRenderer = new SphereRenderer();
        private ImagesCache imagesCache = new ImagesCache();

        private bool useTextures = true;

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

                if (useTextures && size > 10)
                {
                    double inc = GetRotationTowardsNorth(moon.Equatorial);

                    // final rotation of drawn image
                    // axis rotation is negated because measured counter-clockwise
                    float axisRotation = (float)(inc - moon.PAaxis);

                    g.TranslateTransform(p.X, p.Y);
                    g.RotateTransform(axisRotation);

                    // TODO: libration
                    Image textureMoon = imagesCache.GetImage("Moon", 0, MoonTextureProvider);
                    g.FillEllipse(Brushes.Gray, -size / 2, -size / 2, size, size);
                    g.DrawImage(textureMoon, -size / 2 * 1.01f, -size / 2 * 1.01f, size * 1.01f, size * 1.01f);

                    g.ResetTransform();
                }
                else
                {
                    // Moon disk
                    g.FillEllipse(Brushes.Gray, p.X - size / 2, p.Y - size / 2, size, size);
                }

                float phase = (float)moon.Phase * Math.Sign(moon.Elongation);
                float rotation = GetRotationTowardsEclipticPole(moon.Ecliptical);
                GraphicsPath shadow = GetPhaseShadow(phase, size + 1);

                // shadowed part of disk
                g.TranslateTransform(p.X, p.Y);
                g.RotateTransform(rotation);
                g.FillPath(brushShadow, shadow);
                g.ResetTransform();

                DrawObjectCaption(g, "Moon", p, size);

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

                    float diamEquat = diam;
                    float diamPolar = (1 - planet.Flattening) * diam;

                    float rotation = 0;

                    if (planet.Number == Planet.SATURN)
                    {
                        rotation = GetRotationTowardsNorth(planet.Equatorial) + 360 - (float)planet.PAaxis;
                    }
                    else
                    {
                        rotation = GetRotationTowardsEclipticPole(planet.Ecliptical);
                    }

                    g.TranslateTransform(p.X, p.Y);
                    g.RotateTransform(rotation);

                    if (planet.Number == Planet.SATURN)
                    {
                        var rings = Sky.Get<RingsAppearance>("SaturnRings");

                        // scale value to convert visible size of ring to screen pixels
                        double scale = 1.0 / 3600 / Map.ViewAngle * Map.Width / 4;

                        // draw rings by halfs arcs, first half is farther one
                        for (int half = 0; half < 2; half++)
                        {                         
                            // draw planets textures
                            if (useTextures)
                            {
                                float a = (float)(rings.GetRingSize(0, RingEdge.Outer, RingAxis.Major) * scale);
                                float b = (float)(rings.GetRingSize(0, RingEdge.Outer, RingAxis.Minor) * scale);

                                // half of source image: 0 = top, 1 = bottom
                                int h = (half + (rings.B > 0 ? 0 : 1)) % 2;

                                Image textureRings = imagesCache.GetImage("Rings", true, t => Image.FromFile("Data\\Rings.png", true));                               

                                g.DrawImage(textureRings,
                                    // destination rectangle
                                    new RectangleF(-a, -b + h * b, a * 2, b),
                                    // source rectangle
                                    new RectangleF(0, h * textureRings.Height / 2f, textureRings.Width, textureRings.Height / 2f), 
                                    GraphicsUnit.Pixel);
                            }
                            // do not use textures
                            else
                            {
                                float startAngle = -180 * half + ((rings.B > 0) ? 180 : 0) - 1e-2f;

                                // three rings
                                for (int r = 0; r < 3; r++)
                                {
                                    float aOut = (float)(rings.GetRingSize(r, RingEdge.Outer, RingAxis.Major) * scale);
                                    float bOut = (float)(rings.GetRingSize(r, RingEdge.Outer, RingAxis.Minor) * scale);

                                    float aIn = (float)(rings.GetRingSize(r, RingEdge.Inner, RingAxis.Major) * scale);
                                    float bIn = (float)(rings.GetRingSize(r, RingEdge.Inner, RingAxis.Minor) * scale);

                                    GraphicsPath gp = new GraphicsPath();
                                    gp.AddArc(-aOut, -bOut, aOut * 2, bOut * 2, startAngle, 180 + 1e-2f * 2);
                                    gp.Reverse();
                                    gp.AddArc(-aIn, -bIn, aIn * 2, bIn * 2, startAngle, 180 + 1e-2f * 2);
                                    g.FillPath(brushRings[r], gp);
                                }
                            }

                            // draw planet disk after first half of rings
                            if (half == 0)
                            {
                                if (useTextures)
                                {
                                    Image textureSaturn = imagesCache.GetImage("Saturn", (int)rings.B, SaturnImageProvider);
                                    g.DrawImage(textureSaturn, -diamEquat / 2 * 1.01f, -diamPolar / 2 * 1.01f, diamEquat * 1.01f, diamPolar * 1.01f);
                                    g.FillEllipse(GetVolumeBrush(diam, planet.Flattening), -diamEquat / 2 - 1, -diamPolar / 2 - 1, diamEquat + 2, diamPolar + 2);
                                }
                                else
                                {
                                    g.FillEllipse(GetPlanetColor(planet.Number), -diamEquat / 2, -diamPolar / 2, diamEquat, diamPolar);                                    
                                }
                            }
                        }
                    }
                    else
                    {
                        g.FillEllipse(GetPlanetColor(planet.Number), -diamEquat / 2, -diamPolar / 2, diamEquat, diamPolar);                        
                    }

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

        private Image SaturnImageProvider(int ringsB)
        {
            return sphereRenderer.Render(new RendererOptions()
            {
                LatitudeShift = Math.Abs(ringsB) + 180 * (ringsB > 0 ? 0 : 1),
                LongutudeShift = 0,
                OutputImageSize = 1024,
                TextureFilePath = "Data\\Saturn.jpg"
            });
        }

        private Image MoonTextureProvider(int token)
        {
            return sphereRenderer.Render(new RendererOptions()
            {
                LatitudeShift = token,
                LongutudeShift = 180,
                OutputImageSize = 1024,
                TextureFilePath = "Data\\Moon.jpg"
            });
        }

        private Brush GetPlanetColor(int planet)
        {
            switch (planet)
            {
                case 1:
                    return Brushes.LightGray;
                case 2:
                    return new SolidBrush(Color.FromArgb(229, 216, 200));
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
            gpVolume.AddEllipse(-sizeEquat, -sizePolar, sizeEquat * 2, sizePolar * 2);

            PathGradientBrush brushVolume = new PathGradientBrush(gpVolume);
            brushVolume.CenterPoint = new PointF(0, 0);
            brushVolume.CenterColor = Color.Black;
            brushVolume.SetSigmaBellShape(0.3f, 1);

            List<Color> clrs = new List<Color>();
            for (int i = 0; i < gpVolume.PathPoints.Length; i++)
            {
                clrs.Add(Color.Transparent);
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
