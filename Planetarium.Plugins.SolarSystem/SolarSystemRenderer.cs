using ADK;
using Planetarium.Calculators;
using Planetarium.Config;
using Planetarium.Objects;
using Planetarium.Renderers;
using Planetarium.Types;
using Planetarium.Types.Localization;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Planetarium.Plugins.SolarSystem
{
    /// <summary>
    /// Draws solar system objects (Sun, Moon and planets) on the map.
    /// </summary>
    public class SolarSystemRenderer : BaseRenderer
    {
        private readonly PlanetsCalc planetsCalc;
        private readonly ISettings settings;

        private readonly Sun sun;
        private readonly Moon moon;

        private Font fontLabel = new Font("Arial", 8);
        private Font fontShadowLabel = new Font("Arial", 8);
        private Brush brushLabel;

        private readonly Color clrSunDaylight = Color.FromArgb(255, 255, 200);
        private readonly Color clrSunNight = Color.FromArgb(250, 210, 10);
        private static Color clrShadow = Color.FromArgb(10, 10, 10);

        private Color clrPenumbraTransp = Color.Transparent;
        private Color clrPenumbraGrayLight = Color.FromArgb(100, clrShadow);
        private Color clrPenumbraGrayDark = Color.FromArgb(200, clrShadow);
        private Color clrUmbraGray = Color.FromArgb(230, clrShadow);
        private Color clrUmbraRed = Color.FromArgb(200, 50, 0, 0);
        private Color clrJupiterShadow = Color.FromArgb(200, 0, 0, 0);
        private Color clrJupiterMoonShadowLight = Color.FromArgb(128, 0, 0, 0);
        private Color clrJupiterMoonShadowDark = Color.FromArgb(64, 0, 0, 0);
        private static Color clrShadowOutline = Color.FromArgb(100, 50, 0);
        private Pen penShadowOutline = new Pen(clrShadowOutline);
        private Brush brushShadowLabel = new SolidBrush(clrShadowOutline);

        private Brush[] brushRings = new Brush[] 
        {
            new SolidBrush(Color.FromArgb(200, 224, 224, 195)),
            new SolidBrush(Color.FromArgb(200, 224, 224, 195)),
            new SolidBrush(Color.FromArgb(32, 0, 0, 0))
        };

        private readonly SolarTextureDownloader solarTextureDownloader = null;
        private SphereRenderer sphereRenderer = new SphereRenderer();
        private ImagesCache imagesCache = new ImagesCache();

        public SolarSystemRenderer(LunarCalc lunarCalc, SolarCalc solarCalc, PlanetsCalc planetsCalc, SolarTextureDownloader solarTextureDownloader, ISettings settings)
        {
            this.planetsCalc = planetsCalc;
            this.sun = solarCalc.Sun;
            this.moon = lunarCalc.Moon;

            this.solarTextureDownloader = solarTextureDownloader;
            this.settings = settings;
            penShadowOutline.DashStyle = DashStyle.Dot;
        }

        public override RendererOrder Order => RendererOrder.SolarSystem;

        public override void Render(IMapContext map)
        {
            brushLabel = new SolidBrush(map.GetColor(settings.Get<Color>("ColorSolarSystemLabel")));

            // Flag indicated Sun is already rendered
            bool isSunRendered = false;

            // Get all planets except Earth, and sort them by distance from Earth (most distant planet is first)
            var planets = planetsCalc.Planets
                .Where(p => p.Number != Planet.EARTH)
                .OrderByDescending(p => p.Ecliptical.Distance);

            foreach (Planet p in planets)
            {
                if (!isSunRendered && p.Ecliptical.Distance < sun.Ecliptical.Distance)
                {
                    RenderSun(map);
                    isSunRendered = true;
                }

                RenderPlanet(map, p);

                if (!isSunRendered)
                {
                    RenderSun(map);
                    isSunRendered = true;
                }
            }

            RenderMoon(map);

            if (map.Schema == ColorSchema.Day)
            {
                DrawHalo(map);
            }

            RenderEarthShadow(map);
        }

        private void RenderSun(IMapContext map)
        {
            if (!settings.Get<bool>("Sun")) return;

            bool isGround = settings.Get<bool>("Ground");
            bool useTextures = settings.Get<bool>("SunTexture");
            double ad = Angle.Separation(sun.Horizontal, map.Center);
            double coeff = map.DiagonalCoefficient();

            Color colorSun = map.GetColor(clrSunNight, clrSunDaylight);

            if ((!isGround || sun.Horizontal.Altitude + sun.Semidiameter / 3600 > 0) && 
                ad < coeff * map.ViewAngle + sun.Semidiameter / 3600)
            {
                PointF p = map.Project(sun.Horizontal);

                float inc = map.GetRotationTowardsNorth(sun.Equatorial);

                map.Graphics.TranslateTransform(p.X, p.Y);
                map.Graphics.RotateTransform(inc);

                float size = map.GetDiskSize(sun.Semidiameter, 10);

                if (map.Schema == ColorSchema.Night && useTextures && size > 10)
                {
                    Date date = new Date(map.JulianDay);
                    DateTime dt = new DateTime(date.Year, date.Month, (int)date.Day, 0, 0, 0, DateTimeKind.Utc);
                    Brush brushSun = new SolidBrush(clrSunNight);
                    Image imageSun = imagesCache.RequestImage("Sun", dt, SunImageProvider, map.Redraw);
                    map.Graphics.FillEllipse(brushSun, -size / 2, -size / 2, size, size);
                    if (imageSun != null)
                    {
                        map.Graphics.DrawImage(imageSun, -size / 2, -size / 2, size, size);
                    }
                }
                else
                {
                    Brush brushSun = new SolidBrush(colorSun);
                    map.Graphics.FillEllipse(brushSun, -size / 2, -size / 2, size, size);
                    if (map.Schema == ColorSchema.White)
                    {
                        map.Graphics.DrawEllipse(new Pen(Color.Black, 1), -size / 2, -size / 2, size, size);
                    }
                }

                map.Graphics.ResetTransform();

                if (settings.Get<bool>("SunLabel"))
                {
                    map.DrawObjectCaption(fontLabel, brushLabel, sun.Name, p, size);
                }

                map.AddDrawnObject(sun);
            }
        }

        private void DrawHalo(IMapContext map)
        {
            int size = (int)(map.DayLightFactor * 100);
            float sunSize = map.GetDiskSize(sun.Semidiameter);
            int alpha = (int)(map.DayLightFactor * (1 - sunSize / (2 * size)) * 255);

            if (settings.Get<bool>("Ground") && size > 0 && alpha > 0 && 2 * size > sunSize)
            {
                using (var halo = new GraphicsPath())
                {
                    PointF p = map.Project(sun.Horizontal);                  
                    halo.AddEllipse(p.X - size, p.Y - size, 2 * size, 2 * size);
                    var brush = new PathGradientBrush(halo);
                    brush.CenterPoint = p;
                    brush.CenterColor = Color.FromArgb(alpha, clrSunDaylight);
                    brush.SurroundColors = new Color[] { Color.Transparent };
                    map.Graphics.FillPath(brush, halo);
                }                
            }
        }

        private void RenderMoon(IMapContext map)
        {
            bool isGround = settings.Get<bool>("Ground");
            bool useTextures = settings.Get<bool>("UseTextures");
            double ad = Angle.Separation(moon.Horizontal, map.Center);
            double coeff = map.DiagonalCoefficient();

            if ((!isGround || moon.Horizontal.Altitude + moon.Semidiameter / 3600 > 0) && 
                ad < coeff * map.ViewAngle + moon.Semidiameter / 3600.0)
            {
                PointF p = map.Project(moon.Horizontal);

                double inc = map.GetRotationTowardsNorth(moon.Equatorial);

                // final rotation of drawn image
                // axis rotation is negated because measured counter-clockwise
                float axisRotation = (float)(inc - moon.PAaxis);

                map.Graphics.TranslateTransform(p.X, p.Y);
                map.Graphics.RotateTransform(axisRotation);

                // drawing size
                float size = map.GetDiskSize(moon.Semidiameter, 10);

                SolidBrush brushMoon = new SolidBrush(map.GetColor(Color.Gray));

                if (useTextures && size > 10)
                {
                    Image textureMoon = imagesCache.RequestImage("Moon", new PlanetTextureToken("Moon", moon.Libration.l, moon.Libration.b), MoonTextureProvider, map.Redraw);
                    if (textureMoon != null)
                    {
                        map.Graphics.FillEllipse(brushMoon, -size / 2, -size / 2, size, size);
                        map.DrawImage(textureMoon, -size / 2 * 1.01f, -size / 2 * 1.01f, size * 1.01f, size * 1.01f);                        
                    }
                    else
                    {
                        map.Graphics.FillEllipse(brushMoon, -size / 2, -size / 2, size, size);
                    }
                }
                else
                {
                    // Moon disk
                    map.Graphics.FillEllipse(brushMoon, -size / 2, -size / 2, size, size);
                }

                map.Graphics.ResetTransform();

                float phase = (float)moon.Phase * Math.Sign(moon.Elongation);
                float rotation = map.GetRotationTowardsEclipticPole(moon.Ecliptical0);
                GraphicsPath shadow = GetPhaseShadow(phase, size + 1);

                // shadowed part of disk
                map.Graphics.TranslateTransform(p.X, p.Y);
                map.Graphics.RotateTransform(rotation);
                map.Graphics.FillPath(GetShadowBrush(map), shadow);
                map.Graphics.ResetTransform();

                map.DrawObjectCaption(fontLabel, brushLabel, moon.Name, p, size);
                map.AddDrawnObject(moon);
            }
        }

        private Brush GetShadowBrush(IMapContext map)
        {
            return new SolidBrush(Color.FromArgb(220, map.GetSkyColor()));
        }

        private void RenderEarthShadow(IMapContext map)
        {
            // here and below suffixes meanings are: "M" = Moon, "P" = penumbra, "U" = umbra

            // angular distance from center of map to earth shadow center
            double ad = Angle.Separation(moon.EarthShadowCoordinates, map.Center);

            // semidiameter of penumbra in seconds of arc
            double sdP = moon.EarthShadow.PenumbraRadius * 6378.0 / 1738.0 * moon.Semidiameter;

            bool isGround = settings.Get<bool>("Ground");
            double coeff = map.DiagonalCoefficient();

            if ((!isGround || moon.EarthShadowCoordinates.Altitude + sdP / 3600 > 0) &&
                ad < coeff * map.ViewAngle + sdP / 3600)
            {
                PointF p = map.Project(moon.EarthShadowCoordinates);
                PointF pMoon = map.Project(moon.Horizontal);

                // size of penumbra, in pixels
                float szP = map.GetDiskSize(sdP);

                // size of umbra, in pixels
                float szU = szP / (float)moon.EarthShadow.Ratio;

                // size of Moon, in pixels 
                float szM = map.GetDiskSize(moon.Semidiameter);

                // fraction of the penumbra ring (without umbra part)
                float fr = 1 - szU / szP;

                // do not render on large view angle
                if (szM >= 10)
                {
                    // if eclipse takes place
                    if (Angle.Separation(moon.Horizontal, moon.EarthShadowCoordinates) <= sdP / 3600)
                    {
                        var gpM = new GraphicsPath();
                        var gpP = new GraphicsPath();
                        var gpU = new GraphicsPath();

                        gpP.AddEllipse(p.X - szP / 2, p.Y - szP / 2, szP, szP);
                        gpU.AddEllipse(p.X - szU / 2, p.Y - szU / 2, szU, szU);
                        gpM.AddEllipse(pMoon.X - szM / 2 - 0.5f, pMoon.Y - szM / 2 - 0.5f, szM + 1, szM + 1);

                        var brushP = new PathGradientBrush(gpP);
                        brushP.CenterPoint = p;
                        brushP.CenterColor = clrPenumbraGrayDark;
                        brushP.SurroundColors = new Color[] { clrPenumbraTransp };

                        var blendP = new ColorBlend();
                        blendP.Colors = new Color[] { clrPenumbraTransp, clrPenumbraTransp, clrPenumbraGrayLight, clrPenumbraGrayDark, clrPenumbraGrayDark };
                        blendP.Positions = new float[] { 0, fr / 2, fr * 0.95f, fr, 1 };
                        brushP.InterpolationColors = blendP;

                        var brushU = new PathGradientBrush(gpU);
                        brushU.CenterColor = clrUmbraRed;
                        brushU.SurroundColors = new Color[] { clrUmbraGray };
                        brushU.Blend.Factors = new float[] { 0, 0.8f, 1 };
                        brushU.Blend.Positions = new float[] { 0, 0.8f, 1 };

                        var regionP = new Region(gpP);
                        var regionU = new Region(gpU);

                        regionP.Exclude(regionU);
                        regionP.Intersect(gpM);
                        regionU.Intersect(gpM);

                        map.Graphics.FillRegion(brushP, regionP);
                        map.Graphics.FillRegion(brushU, regionU);
                    }

                    // outline circles
                    if (settings.Get<bool>("EarthShadowOutline"))
                    {
                        map.Graphics.TranslateTransform(p.X, p.Y);
                        map.Graphics.DrawEllipse(penShadowOutline, -szP / 2, -szP / 2, szP, szP);
                        map.Graphics.DrawEllipse(penShadowOutline, -szU / 2, -szU / 2, szU, szU);
                        map.Graphics.ResetTransform();
                        if (map.ViewAngle <= 10)
                        {
                            map.DrawObjectCaption(fontShadowLabel, brushShadowLabel, Text.Get("EarthShadow.Label"), p, szP);
                        }
                    }
                }
            }
        }

        private void RenderPlanet(IMapContext map, Planet planet)
        {
            if (!settings.Get<bool>("Planets"))
            {
                return;
            }

            if (planet.Number == Planet.JUPITER)
            {
                // render moons behind Jupiter
                var moons = planetsCalc.JupiterMoons.Where(m => m.Rectangular.Z >= 0).OrderByDescending(m => m.Rectangular.Z);
                RenderJupiterMoons(map, planet, moons);
            }

            Graphics g = map.Graphics;
            double ad = Angle.Separation(planet.Horizontal, map.Center);
            bool isGround = settings.Get<bool>("Ground");
            bool useTextures = settings.Get<bool>("UseTextures");
            double coeff = map.DiagonalCoefficient();

            if ((!isGround || planet.Horizontal.Altitude + planet.Semidiameter / 3600 > 0) && 
                ad < coeff * map.ViewAngle + planet.Semidiameter / 3600)
            {
                float size = map.GetPointSize(planet.Magnitude, maxDrawingSize: 7);
                float diam = map.GetDiskSize(planet.Semidiameter);

                // diameter is to small to render as planet disk, 
                // but point size caclulated from magnitude is enough to be drawn
                if (size > diam && (int)size > 0)
                {
                    PointF p = map.Project(planet.Horizontal);
                    g.FillEllipse(GetPlanetColor(map, planet.Number), p.X - size / 2, p.Y - size / 2, size, size);

                    map.DrawObjectCaption(fontLabel, brushLabel, planet.Name, p, size);
                    map.AddDrawnObject(planet);
                }

                // planet should be rendered as disk
                else if (diam >= size && (int)diam > 0)
                {
                    PointF p = map.Project(planet.Horizontal);

                    float rotation = map.GetRotationTowardsNorth(planet.Equatorial) + 360 - (float)planet.Appearance.P;

                    g.TranslateTransform(p.X, p.Y);
                    g.RotateTransform(rotation);

                    DrawRotationAxis(g, diam);

                    if (planet.Number == Planet.SATURN)
                    {
                        var rings = planetsCalc.SaturnRings;

                        double maxSize = Math.Max(map.Width, map.Height);

                        // scale value to convert visible size of ring to screen pixels
                        double scale = 1.0 / 3600 / map.ViewAngle * maxSize / 4;

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

                                Image textureRings = imagesCache.RequestImage("Rings", true, t => Image.FromFile("Data\\Rings.png", true), map.Redraw);
                                if (textureRings != null)
                                {
                                    map.DrawImage(textureRings,
                                        // destination rectangle
                                        new RectangleF(-a, -b + h * b, a * 2, b),
                                        // source rectangle
                                        new RectangleF(0, h * textureRings.Height / 2f, textureRings.Width, textureRings.Height / 2f));
                                }
                                else
                                {
                                    DrawRingsUntextured(g, rings, half, scale);
                                }
                            }
                            // do not use textures
                            else
                            {
                                DrawRingsUntextured(g, rings, half, scale);
                            }

                            // draw planet disk after first half of rings
                            if (half == 0)
                            {
                                DrawPlanetGlobe(map, planet, diam);                                
                            }
                        }
                    }
                    else
                    {
                        DrawPlanetGlobe(map, planet, diam);                     
                    }

                    g.ResetTransform();

                    if (planet.Number <= Planet.MARS)
                    {
                        float phase = (float)planet.Phase * Math.Sign(planet.Elongation);

                        GraphicsPath shadow = GetPhaseShadow(phase, diam + 1, planet.Flattening);

                        // rotation of phase image
                        rotation = map.GetRotationTowardsEclipticPole(planet.Ecliptical);

                        g.TranslateTransform(p.X, p.Y);
                        g.RotateTransform(rotation);
                        g.FillPath(GetShadowBrush(map), shadow);
                        g.ResetTransform();
                    }
                    
                    map.DrawObjectCaption(fontLabel, brushLabel, planet.Name, p, diam);
                    map.AddDrawnObject(planet);

                    if (planet.Number == Planet.JUPITER)
                    {
                        // render shadows on Jupiter
                        RenderJupiterMoonShadow(map, planet);
                    }
                }
            }

            // render moons over Jupiter
            if (planet.Number == Planet.JUPITER)
            {
                var moons = planetsCalc.JupiterMoons.Where(m => m.Rectangular.Z < 0).OrderByDescending(m => m.Rectangular.Z);
                RenderJupiterMoons(map, planet, moons);
            }
        }

        private void RenderJupiterMoons(IMapContext map, Planet jupiter, IEnumerable<JupiterMoon> moons)
        {
            bool isGround = settings.Get<bool>("Ground");
            bool useTextures = settings.Get<bool>("UseTextures");
            double coeff = map.DiagonalCoefficient();

            foreach (var moon in moons)
            {
                double ad = Angle.Separation(moon.Horizontal, map.Center);

                if ((!isGround || moon.Horizontal.Altitude + moon.Semidiameter / 3600 > 0) &&
                    ad < coeff * map.ViewAngle + moon.Semidiameter / 3600)
                {                  
                    PointF p = map.Project(moon.Horizontal);
                    PointF pJupiter = map.Project(jupiter.Horizontal);

                    float size = map.GetPointSize(moon.Magnitude, 2);
                    float diam = map.GetDiskSize(moon.Semidiameter);

                    // diameter is to small to render moon disk, 
                    // but point size caclulated from magnitude is enough to be drawn
                    if (size > diam && (int)size > 0)
                    {
                        // do not draw moon point if eclipsed
                        if (!moon.IsEclipsedByJupiter)
                        {
                            // satellite is distant enough from the Jupiter
                            // but too small to be drawn as disk
                            if (map.DistanceBetweenPoints(p, pJupiter) >= 5)
                            {
                                map.Graphics.TranslateTransform(p.X, p.Y);
                                map.Graphics.FillEllipse(new SolidBrush(map.GetColor(Color.Wheat)), -size / 2, -size / 2, size, size);
                                map.Graphics.ResetTransform();

                                map.DrawObjectCaption(fontLabel, brushLabel, moon.Name, p, 2);
                                map.AddDrawnObject(moon);
                            }                                                       
                        }
                    }
                    // moon should be rendered as disk
                    else if (diam >= size && (int)diam > 0)
                    {
                        float rotation = map.GetRotationTowardsNorth(jupiter.Equatorial) + 360 - (float)jupiter.Appearance.P;

                        map.Graphics.TranslateTransform(p.X, p.Y);
                        map.Graphics.RotateTransform(rotation);

                        if (useTextures)
                        {
                            Image texture = imagesCache.RequestImage($"5-{moon.Number}", new PlanetTextureToken($"5-{moon.Number}", moon.CM, jupiter.Appearance.D), PlanetTextureProvider, map.Redraw);
                            if (texture != null)
                            {
                                map.DrawImage(texture, -diam / 2 * 1.01f, -diam / 2 * 1.01f, diam * 1.01f, diam * 1.01f);
                                DrawVolume(map, diam, 0);
                            }
                            else
                            {
                                map.Graphics.FillEllipse(new SolidBrush(map.GetColor(Color.Wheat)), -diam / 2, -diam / 2, diam, diam);
                            }
                        }
                        else
                        {
                            map.Graphics.FillEllipse(new SolidBrush(map.GetColor(Color.Wheat)), -diam / 2, -diam / 2, diam, diam);
                            if (diam > 2)
                            {
                                map.Graphics.DrawEllipse(new Pen(map.GetSkyColor()), -diam / 2, -diam / 2, diam, diam);
                            }
                        }

                        map.Graphics.ResetTransform();

                        // render another moon shadows on the moon
                        RenderJupiterMoonShadow(map, moon, moon.RectangularS);

                        // render Jupiter shadow on the moon
                        if (moon.IsEclipsedByJupiter) RenderJupiterShadow(map, moon);

                        map.DrawObjectCaption(fontLabel, brushLabel, moon.Name, p, diam);
                        map.AddDrawnObject(moon);
                    }
                }
            }
        }

        private void RenderJupiterShadow(IMapContext map, JupiterMoon moon)
        {
            Planet jupiter = planetsCalc.Planets.ElementAt(Planet.JUPITER - 1);

            float rotation = map.GetRotationTowardsNorth(jupiter.Equatorial) + 360 - (float)jupiter.Appearance.P;

            float diam = map.GetDiskSize(jupiter.Semidiameter);
            float diamEquat = diam;
            float diamPolar = (1 - jupiter.Flattening) * diam;

            // Jupiter radius, in pixels
            float sd = diam / 2;

            // Center of eclipsed moon
            PointF pMoon = map.Project(moon.Horizontal);

            // elipsed moon size, in pixels
            float szB = map.GetDiskSize(moon.Semidiameter);

            // Center of shadow
            PointF p = new PointF(-(float)moon.RectangularS.X * sd, (float)moon.RectangularS.Y * sd);

            map.Graphics.TranslateTransform(pMoon.X, pMoon.Y);
            map.Graphics.RotateTransform(rotation);

            var gpM = new GraphicsPath();
            var gpU = new GraphicsPath();

            gpU.AddEllipse(p.X - diamEquat / 2 - 1, p.Y - diamPolar / 2 - 1, diamEquat + 2, diamPolar + 2);
            gpM.AddEllipse(-szB / 2 - 0.5f, -szB / 2 - 0.5f, szB + 1, szB + 1);

            var regionU = new Region(gpU);
            regionU.Intersect(gpM);

            if (!regionU.IsEmpty(map.Graphics))
            {
                map.Graphics.FillRegion(new SolidBrush(clrJupiterShadow), regionU);
            }

            map.Graphics.ResetTransform();
            if (!regionU.IsEmpty(map.Graphics))
            {
                map.DrawObjectCaption(fontShadowLabel, brushShadowLabel, Text.Get("EclipsedByJupiter"), pMoon, szB);
            }
        }

        private void RenderJupiterMoonShadow(IMapContext map, SizeableCelestialObject eclipsedBody, CrdsRectangular rect = null)
        {
            if (rect == null)
            {
                rect = new CrdsRectangular();
            }

            // collect moons than can produce a shadow
            var ecliptingMoons = planetsCalc.JupiterMoons.Where(m => m.RectangularS.Z < rect.Z);

            if (ecliptingMoons.Any())
            {
                Planet jupiter = planetsCalc.Planets.ElementAt(Planet.JUPITER - 1);

                float rotation = map.GetRotationTowardsNorth(jupiter.Equatorial) + 360 - (float)jupiter.Appearance.P;

                // Jupiter radius, in pixels
                float sd = map.GetDiskSize(jupiter.Semidiameter) / 2;

                // Center of eclipsed body
                PointF pBody = map.Project(eclipsedBody.Horizontal);

                // elipsed body size, in pixels
                float szB = map.GetDiskSize(eclipsedBody.Semidiameter);

                foreach (var moon in ecliptingMoons)
                {
                    // umbra and penumbra radii, in acrseconds
                    var shadow = GalileanMoons.Shadow(jupiter.Ecliptical.Distance, jupiter.DistanceFromSun, moon.Number - 1, moon.RectangularS, rect);

                    // umbra and penumbra size, in pixels
                    float szU = map.GetDiskSize(shadow.Umbra);
                    float szP = map.GetDiskSize(shadow.Penumbra);

                    // coordinates of shadow relative to eclipsed body
                    CrdsRectangular shadowRelative = moon.RectangularS - rect;

                    // Center of shadow
                    PointF p = new PointF((float)shadowRelative.X * sd, -(float)shadowRelative.Y * sd);

                    map.Graphics.TranslateTransform(pBody.X, pBody.Y);
                    map.Graphics.RotateTransform(rotation);

                    // shadow has enough size to be rendered
                    if ((int)szP > 0)
                    {
                        var gpB = new GraphicsPath();
                        var gpP = new GraphicsPath();
                        var gpU = new GraphicsPath();

                        gpU.AddEllipse(p.X - szU / 2, p.Y - szU / 2, szU, szU);
                        gpP.AddEllipse(p.X - szP / 2, p.Y - szP / 2, szP, szP);
                        gpB.AddEllipse(-szB / 2 - 0.5f, -szB / 2 - 0.5f, szB + 1, szB + 1);

                        var regionP = new Region(gpP);
                        regionP.Intersect(gpB);

                        if (!regionP.IsEmpty(map.Graphics))
                        {
                            float f1 = 1 - (szU + szP) / 2 / szP;
                            float f2 = 1 - szU / szP;

                            var brushP = new PathGradientBrush(gpP);
                            brushP.CenterPoint = p;
                            brushP.CenterColor = clrJupiterMoonShadowLight;

                            brushP.InterpolationColors = new ColorBlend()
                            {
                                Colors = new[]
                                {
                                    Color.Transparent,
                                    clrJupiterMoonShadowDark,
                                    clrJupiterMoonShadowLight,
                                    clrJupiterMoonShadowLight
                                },
                                Positions = new float[] { 0, f1, f2, 1 }
                            };

                            var regionU = new Region(gpU);
                            regionU.Intersect(gpB);

                            var brushU = new SolidBrush(clrJupiterMoonShadowLight);

                            map.Graphics.FillRegion(brushP, regionP);
                            map.Graphics.FillRegion(brushU, regionU);

                            // outline circles
                            if (settings.Get<bool>("JupiterMoonsShadowOutline") && szP > 20)
                            {
                                map.Graphics.DrawEllipse(penShadowOutline, p.X - (szP + szU) / 4, p.Y - (szP + szU) / 4, (szP + szU) / 2, (szP + szU) / 2);
                                map.Graphics.DrawEllipse(penShadowOutline, p.X - szU / 2, p.Y - szU / 2, szU, szU);

                                PointF[] points = new PointF[] { new PointF(p.X, p.Y) };
                                map.Graphics.TransformPoints(CoordinateSpace.Page, CoordinateSpace.World, points);
                                map.Graphics.ResetTransform();
                                map.DrawObjectCaption(fontShadowLabel, brushShadowLabel, moon.ShadowName, points[0], szP);
                            }
                        }
                    }

                    map.Graphics.ResetTransform();
                }
            }
        }

        private void DrawRingsUntextured(Graphics g, RingsAppearance rings, int half, double scale)
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

        private void DrawPlanetGlobe(IMapContext map, Planet planet, float diam)
        {
            Graphics g = map.Graphics;
            float diamEquat = diam;
            float diamPolar = (1 - planet.Flattening) * diam;
            bool useTextures = settings.Get<bool>("UseTextures");

            if (!(useTextures && map.Schema == ColorSchema.Red))
            {
                g.FillEllipse(GetPlanetColor(map, planet.Number), -diamEquat / 2, -diamPolar / 2, diamEquat, diamPolar);
            }

            if (useTextures)
            {
                double grs = planet.Number == Planet.JUPITER ? planetsCalc.GreatRedSpotLongitude : 0;
                Image texturePlanet = imagesCache.RequestImage(planet.Number.ToString(), new PlanetTextureToken(planet.Number.ToString(), planet.Appearance.CM - grs, planet.Appearance.D), PlanetTextureProvider, map.Redraw);

                if (texturePlanet != null)
                {
                    map.DrawImage(texturePlanet, -diamEquat / 2 * 1.01f, -diamPolar / 2 * 1.01f, diamEquat * 1.01f, diamPolar * 1.01f);
                    DrawVolume(map, diam, planet.Flattening);
                }
            }
        }

        private void DrawVolume(IMapContext map, float diam, float flattening)
        {
            Graphics g = map.Graphics;
            float diamEquat = diam * 1.01f;
            float diamPolar = (1 - flattening) * diam * 1.01f;

            using (GraphicsPath gpVolume = new GraphicsPath())
            {
                gpVolume.AddEllipse(-diamEquat, -diamPolar, 2 * diamEquat, 2 * diamPolar);
                using (PathGradientBrush brushVolume = new PathGradientBrush(gpVolume))
                {
                    brushVolume.CenterPoint = new PointF(0, 0);
                    brushVolume.CenterColor = map.GetSkyColor();
                    brushVolume.SetSigmaBellShape(0.3f, 1);
                    brushVolume.SurroundColors = new Color[] { Color.Transparent };
                    g.FillEllipse(brushVolume, -diamEquat / 2, -diamPolar / 2, diamEquat, diamPolar);
                }
            }
        }

        private void DrawRotationAxis(Graphics g, float diam)
        {
            if (settings.Get<bool>("ShowRotationAxis"))
            {
                var p1 = new PointF(0, -(diam / 2 + 10));
                var p2 = new PointF(0, diam / 2 + 10);
                g.DrawLine(Pens.Gray, p1, p2);
            }
        }

        private Image PlanetTextureProvider(PlanetTextureToken token)
        {
            return sphereRenderer.Render(new RendererOptions()
            {
                LatitudeShift = token.Latitude,
                LongutudeShift = 180 + token.Longitude,
                OutputImageSize = 1024,
                TextureFilePath = $"Data\\{token.TextureName}.jpg"
            });
        }

        private Image MoonTextureProvider(PlanetTextureToken token)
        {
            return sphereRenderer.Render(new RendererOptions()
            {
                LatitudeShift = token.Latitude,
                LongutudeShift = 180 - token.Longitude,
                OutputImageSize = 1024,
                TextureFilePath = "Data\\Moon.jpg"
            });
        }

        private Image SunImageProvider(DateTime date)
        {
            string template = settings.Get<string>("SunTexturePath");
            string format = Regex.Replace(template, "{([^}]*)}", match => "{0:" + match.Groups[1].Value + "}");
            string url = string.Format(format, date);
            return solarTextureDownloader.Download(url);
        }

        private Brush GetPlanetColor(IMapContext map, int planet)
        {
            Color color = Color.Empty;

            switch (planet)
            {
                case 1:
                    color = Color.FromArgb(132, 131, 131);
                    break;
                case 2:
                    color = Color.FromArgb(228, 189, 127);
                    break;
                case 4:
                    color = Color.FromArgb(183, 98, 71);
                    break;
                case 5:
                    color = Color.FromArgb(166, 160, 149);
                    break;
                case 6:
                    color = Color.FromArgb(207, 192, 162);
                    break;
                case 7:
                    color = Color.FromArgb(155, 202, 209);
                    break;
                case 8:
                    color = Color.FromArgb(54, 79, 167);
                    break;
                default:
                    color = Color.White;
                    break;
            }

            return new SolidBrush(map.GetColor(color));
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
        
        private struct PlanetTextureToken
        {
            public string TextureName { get; private set; }
            public double Longitude { get; private set; }
            public double Latitude { get; private set; }
            public double Flattening { get; private set; }

            public PlanetTextureToken(string name, double longitude, double latitude, double flattening = 1)
            {
                TextureName = name;
                Longitude = longitude;
                Latitude = latitude;
                Flattening = flattening;
            }

            public override bool Equals(object obj)
            {
                if (obj is PlanetTextureToken)
                {
                    PlanetTextureToken other = (PlanetTextureToken)obj;
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
