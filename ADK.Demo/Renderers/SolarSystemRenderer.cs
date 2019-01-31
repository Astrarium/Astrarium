using ADK.Demo.Objects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;

namespace ADK.Demo.Renderers
{
    /// <summary>
    /// Draws solar system objects (Sun, Moon and planets) on the map.
    /// </summary>
    public class SolarSystemRenderer : BaseSkyRenderer
    {
        private Font fontCelestialObjectName = SystemFonts.DefaultFont;
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

        public SolarSystemRenderer(Sky sky, ISkyMap skyMap, ISettings settings) : base(sky, skyMap, settings)
        {
            
        }

        private void ImagesCache_OnRequestCompleted()
        {
            Map.Invalidate();
        }

        public override void Render(Graphics g)
        {
            Sun sun = Sky.Get<Sun>("Sun");
            Moon moon = Sky.Get<Moon>("Moon");

            // Flag indicated Sun is already rendered
            bool isSunRendered = false;

            // Get all planets esxept Earth, and sort them by distance from Earth (most distant planet is first)
            var planets = Sky.Get<ICollection<Planet>>("Planets")
                .Where(p => p.Number != Planet.EARTH)
                .OrderByDescending(p => p.Ecliptical.Distance);

            foreach (Planet p in planets)
            {
                if (!isSunRendered && p.Ecliptical.Distance < sun.Ecliptical.Distance)
                {
                    RenderSun(g, sun);
                    isSunRendered = true;
                }

                RenderPlanet(g, p);

                if (!isSunRendered)
                {
                    RenderSun(g, sun);
                    isSunRendered = true;
                }
            }

            RenderMoon(g, moon);
        }

        private void RenderSun(Graphics g, Sun sun)
        {
            double ad = Angle.Separation(sun.Horizontal, Map.Center);
            if (ad < 1.2 * Map.ViewAngle + sun.Semidiameter / 3600.0)
            {
                PointF p = Map.Projection.Project(sun.Horizontal);

                float inc = (float)GetRotationTowardsNorth(sun.Equatorial);

                g.TranslateTransform(p.X, p.Y);
                g.RotateTransform(inc);

                float size = GetDiskSize(sun.Semidiameter, 10);

                if (useTextures && size > 10)
                {
                    Image imageSun = imagesCache.RequestImage("Sun", true, SunImageProvider, Map.Invalidate);
                    if (imageSun != null)
                    {
                        g.FillEllipse(penSun.Brush, -size / 2, -size / 2, size, size);
                        g.DrawImage(imageSun, -size / 2, -size / 2, size, size);
                    }
                    else
                    {
                        g.FillEllipse(penSun.Brush, -size / 2, -size / 2, size, size);
                    }
                }
                else
                {
                    g.FillEllipse(penSun.Brush, -size / 2, -size / 2, size, size);
                }

                g.ResetTransform();

                DrawObjectCaption(g, fontCelestialObjectName, "Sun", p, size);
                Map.AddDrawnObject(sun, p);
            }
        }

        private void RenderMoon(Graphics g, Moon moon)
        {
            double ad = Angle.Separation(moon.Horizontal, Map.Center);
            if (ad < 1.2 * Map.ViewAngle + moon.Semidiameter / 3600.0)
            {
                PointF p = Map.Projection.Project(moon.Horizontal);

                double inc = GetRotationTowardsNorth(moon.Equatorial);

                // final rotation of drawn image
                // axis rotation is negated because measured counter-clockwise
                float axisRotation = (float)(inc - moon.PAaxis);

                g.TranslateTransform(p.X, p.Y);
                g.RotateTransform(axisRotation);

                // drawing size
                float size = GetDiskSize(moon.Semidiameter, 10);

                if (useTextures && size > 10)
                {
                    Image textureMoon = imagesCache.RequestImage("Moon", new LonLatShift("Moon", moon.Libration.l, moon.Libration.b), MoonTextureProvider, Map.Invalidate);
                    if (textureMoon != null)
                    {
                        g.FillEllipse(Brushes.Gray, -size / 2, -size / 2, size, size);
                        g.DrawImage(textureMoon, -size / 2 * 1.01f, -size / 2 * 1.01f, size * 1.01f, size * 1.01f);
                    }
                    else
                    {
                        g.FillEllipse(Brushes.Gray, -size / 2, -size / 2, size, size);
                    }
                }
                else
                {
                    // Moon disk
                    g.FillEllipse(Brushes.Gray, -size / 2, -size / 2, size, size);
                }

                g.ResetTransform();

                float phase = (float)moon.Phase * Math.Sign(moon.Elongation);
                float rotation = GetRotationTowardsEclipticPole(moon.Ecliptical0);
                GraphicsPath shadow = GetPhaseShadow(phase, size + 1);

                // shadowed part of disk
                g.TranslateTransform(p.X, p.Y);
                g.RotateTransform(rotation);
                g.FillPath(brushShadow, shadow);
                g.ResetTransform();

                DrawObjectCaption(g, fontCelestialObjectName, "Moon", p, size);
                Map.AddDrawnObject(moon, p);
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

                    DrawObjectCaption(g, fontCelestialObjectName, planet.Name, p, size);
                    Map.AddDrawnObject(planet, p);
                }

                // planet should be rendered as disk
                else if (diam >= size && (int)diam > 0)
                {
                    PointF p = Map.Projection.Project(planet.Horizontal);

                    float rotation = GetRotationTowardsNorth(planet.Equatorial) + 360 - (float)planet.Appearance.P;

                    g.TranslateTransform(p.X, p.Y);
                    g.RotateTransform(rotation);

                    DrawRotationAxis(g, diam);

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

                                Image textureRings = imagesCache.RequestImage("Rings", true, t => Image.FromFile("Data\\Rings.png", true), Map.Invalidate);
                                if (textureRings != null)
                                {
                                    g.DrawImage(textureRings,
                                        // destination rectangle
                                        new RectangleF(-a, -b + h * b, a * 2, b),
                                        // source rectangle
                                        new RectangleF(0, h * textureRings.Height / 2f, textureRings.Width, textureRings.Height / 2f),
                                        GraphicsUnit.Pixel);
                                }
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
                                DrawPlanetGlobe(g, planet, diam);                                
                            }
                        }
                    }
                    else
                    {
                        DrawPlanetGlobe(g, planet, diam);                     
                    }

                    g.ResetTransform();

                    if (planet.Number <= Planet.MARS)
                    {
                        float phase = (float)planet.Phase * Math.Sign(planet.Elongation);

                        GraphicsPath shadow = GetPhaseShadow(phase, diam + 1, planet.Flattening);

                        // rotation of phase image
                        rotation = GetRotationTowardsEclipticPole(planet.Ecliptical);

                        g.TranslateTransform(p.X, p.Y);
                        g.RotateTransform(rotation);
                        g.FillPath(brushShadow, shadow);
                        g.ResetTransform();
                    }
                    
                    DrawObjectCaption(g, fontCelestialObjectName, planet.Name, p, diam);
                    Map.AddDrawnObject(planet, p);
                }
            }
        }

        private void DrawPlanetGlobe(Graphics g, Planet planet, float diam)
        {
            float diamEquat = diam;
            float diamPolar = (1 - planet.Flattening) * diam;

            if (useTextures)
            {
                Image texturePlanet = imagesCache.RequestImage(planet.Number.ToString(), new LonLatShift(planet.Number.ToString(), planet.Appearance.CM, planet.Appearance.D), PlanetTextureProvider, Map.Invalidate);
                if (texturePlanet != null)
                {
                    g.DrawImage(texturePlanet, -diamEquat / 2 * 1.01f, -diamPolar / 2 * 1.01f, diamEquat * 1.01f, diamPolar * 1.01f);
                    g.FillEllipse(GetVolumeBrush(diam, planet.Flattening), -diamEquat / 2 - 1, -diamPolar / 2 - 1, diamEquat + 2, diamPolar + 2);
                }
                else
                {
                    g.FillEllipse(GetPlanetColor(planet.Number), -diamEquat / 2, -diamPolar / 2, diamEquat, diamPolar);
                }
            }
            else
            {
                g.FillEllipse(GetPlanetColor(planet.Number), -diamEquat / 2, -diamPolar / 2, diamEquat, diamPolar);
            }
        }

        private void DrawRotationAxis(Graphics g, float diam)
        {
            var p1 = new PointF(0, -(diam / 2 + 10));
            var p2 = new PointF(0, diam / 2 + 10);
            g.DrawLine(Pens.Gray, p1, p2);
        }

        private Image PlanetTextureProvider(LonLatShift token)
        {
            return sphereRenderer.Render(new RendererOptions()
            {
                LatitudeShift = token.Latitude,
                LongutudeShift = 180 + token.Longitude,
                OutputImageSize = 1024,
                TextureFilePath = $"Data\\{token.TextureName}.jpg"
            });
        }

        private Image MoonTextureProvider(LonLatShift token)
        {
            return sphereRenderer.Render(new RendererOptions()
            {
                LatitudeShift = token.Latitude,
                LongutudeShift = 180 - token.Longitude,
                OutputImageSize = 1024,
                TextureFilePath = "Data\\Moon.jpg"
            });
        }

        private Image SunImageProvider(bool token)
        {
            // TODO: take from settings
            float cropFactor = 0.93f;
            string url = Settings.Get<string>("TextureSunPath");

            string tempFile = Path.Combine(Path.GetTempPath(), "Sun.jpg");
            try
            {
                // Download latest Solar image from provided URL
                using (var client = new WebClient())
                {
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = 
                        SecurityProtocolType.Tls |
                        SecurityProtocolType.Tls11 |
                        SecurityProtocolType.Tls12 |
                        SecurityProtocolType.Ssl3;
                    client.DownloadFile(new Uri(url), tempFile);                
                }

                // Prepare resulting circle image with transparent background
                using (var image = Image.FromFile(tempFile))
                {
                    Image result = new Bitmap(
                        (int)(image.Width * cropFactor), 
                        (int)(image.Height * cropFactor), 
                        System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    using (var g = Graphics.FromImage(result))
                    {
                        g.Clear(Color.Transparent);
                        g.SmoothingMode = SmoothingMode.AntiAlias;

                        using (var crop = new GraphicsPath())
                        {
                            g.TranslateTransform(
                                image.Width * cropFactor / 2,
                                image.Height * cropFactor / 2);

                            float cropMargin = 1e-3f;

                            crop.AddEllipse(
                                -image.Width * cropFactor / 2 * (1 - cropMargin), 
                                -image.Height * cropFactor / 2 * (1 - cropMargin), 
                                image.Width * cropFactor * (1 - cropMargin), 
                                image.Height * cropFactor * (1 - cropMargin));

                            g.SetClip(crop);
                           
                            g.DrawImage(image, -image.Width / 2, -image.Height / 2, image.Width, image.Height);
                        }
                    }

                    return result;
                }
            }
            catch
            {
                return null;
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    try
                    {
                        File.Delete(tempFile);
                    }
                    catch { }
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

        

        private struct LonLatShift
        {
            public string TextureName { get; private set; }
            public double Longitude { get; private set; }
            public double Latitude { get; private set; }

            public LonLatShift(string name, double longitude, double latitude)
            {
                TextureName = name;
                Longitude = longitude;
                Latitude = latitude;
            }

            public override bool Equals(object obj)
            {
                if (obj is LonLatShift)
                {
                    LonLatShift other = (LonLatShift)obj;
                    return
                        Math.Abs(Longitude - other.Longitude) < 1 &&
                        Math.Abs(Latitude - other.Latitude) < 1;
                }
                else
                {
                    return false;
                }
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }
    }
}
