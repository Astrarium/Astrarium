using Astrarium.Algorithms;
using Astrarium.Calculators;
using Astrarium.Config;
using Astrarium.Objects;
using Astrarium.Renderers;
using Astrarium.Types;
using Astrarium.Types.Localization;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using static Astrarium.Plugins.SolarSystem.Plugin;

namespace Astrarium.Plugins.SolarSystem
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
        private readonly Planet mars;
        private readonly Pluto pluto;

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
        private Pen penShadowOutline = new Pen(clrShadowOutline) { DashStyle = DashStyle.Dot };
        private Brush brushShadowLabel = new SolidBrush(clrShadowOutline);

        private Brush[] brushRings = new Brush[] 
        {
            new SolidBrush(Color.FromArgb(200, 224, 224, 195)),
            new SolidBrush(Color.FromArgb(200, 224, 224, 195)),
            new SolidBrush(Color.FromArgb(32, 0, 0, 0))
        };

        private readonly SolarTextureDownloader solarTextureDownloader = new SolarTextureDownloader();
        private readonly ISphereRenderer sphereRenderer = new GLSphereRenderer();
        private readonly ImagesCache imagesCache = new ImagesCache();
        private readonly ICollection<SurfaceFeature> lunarFeatures;
        private readonly ICollection<SurfaceFeature> martianFeatures;

        public SolarSystemRenderer(LunarCalc lunarCalc, SolarCalc solarCalc, PlanetsCalc planetsCalc, ISettings settings)
        {
            this.planetsCalc = planetsCalc;
            this.settings = settings;

            this.sun = solarCalc.Sun;
            this.moon = lunarCalc.Moon;
            this.mars = planetsCalc.Planets.ElementAt(Planet.MARS - 1);
            this.pluto = planetsCalc.Pluto;

            var featuresReader = new SurfaceFeaturesReader();
            lunarFeatures = featuresReader.Read(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/LunarFeatures.dat"));
            martianFeatures = featuresReader.Read(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/MartianFeatures.dat"));
        }

        public override RendererOrder Order => RendererOrder.SolarSystem;

        public override void Render(IMapContext map)
        {
            brushLabel = new SolidBrush(map.GetColor("ColorSolarSystemLabel"));

            var bodies = planetsCalc.Planets
                .Where(p => p.Number != Planet.EARTH)
                .Cast<ISolarSystemObject>()
                .Concat(new[] { pluto })
                .Concat(new[] { sun })
                .Concat(planetsCalc.MarsMoons)
                .Concat(planetsCalc.JupiterMoons)
                .Concat(planetsCalc.SaturnMoons)
                .Concat(planetsCalc.UranusMoons)
                .Concat(planetsCalc.NeptuneMoons)
                .Concat(planetsCalc.GenericMoons)
                .OrderByDescending(body => body.DistanceFromEarth)
                .ToArray();

            foreach (var body in bodies)
            {
                if (body is Planet planet)
                {
                    RenderPlanet(map, planet);
                }
                if (body is Pluto pluto)
                {
                    RenderPlanet(map, pluto);
                }
                else if (body is MarsMoon mm)
                {
                    RenderPlanetMoon(map, planetsCalc.Planets.ElementAt(Planet.MARS - 1), mm, hasTexture: false);
                }
                else if (body is JupiterMoon jm)
                {
                    RenderPlanetMoon(map, planetsCalc.Planets.ElementAt(Planet.JUPITER - 1), jm);
                    RenderJupiterMoonShadow(map, jm, jm.RectangularS);
                    RenderJupiterShadow(map, jm);
                }
                else if (body is SaturnMoon sm)
                {
                    RenderPlanetMoon(map, planetsCalc.Planets.ElementAt(Planet.SATURN - 1), sm, hasTexture: false);
                }
                else if (body is UranusMoon um)
                {
                    RenderPlanetMoon(map, planetsCalc.Planets.ElementAt(Planet.URANUS - 1), um, hasTexture: false);
                }
                else if (body is NeptuneMoon nm)
                {
                    RenderPlanetMoon(map, planetsCalc.Planets.ElementAt(Planet.NEPTUNE - 1), nm, hasTexture: false);
                }
                else if (settings.Get("GenericMoons") && body is GenericMoon gm)
                {
                    if (gm.Data.planet == 9)
                    {
                        RenderPlanetMoon(map, planetsCalc.Pluto, gm, hasTexture: false);
                    }
                    else
                    {
                        RenderPlanetMoon(map, planetsCalc.Planets.ElementAt(gm.Data.planet - 1), gm, hasTexture: false);
                    }
                }
                else if (body is Sun)
                {
                    RenderSun(map);
                }
            }

            RenderMoon(map);

            if (map.Schema == ColorSchema.Day)
            {
                DrawHalo(map);
            }

            RenderEarthShadow(map);
            DrawLunarSurfaceFeatures(map);
        }

        public override bool OnMouseMove(CrdsHorizontal mouse, MouseButton mouseButton)
        {
            return mouseButton == MouseButton.None && 
                (Angle.Separation(mouse, moon.Horizontal) < moon.Semidiameter / 3600 ||
                 Angle.Separation(mouse, mars.Horizontal) < mars.Semidiameter / 3600);
        }

        private void RenderSun(IMapContext map)
        {
            if (!settings.Get("Sun")) return;

            bool isGround = settings.Get("Ground");
            bool useTextures = settings.Get("SunTexture");
            double ad = Angle.Separation(sun.Horizontal, map.Center);
            double coeff = map.DiagonalCoefficient();
            Graphics g = map.Graphics;
            Color colorSun = map.GetColor(clrSunNight, clrSunDaylight);

            if ((!isGround || sun.Horizontal.Altitude + sun.Semidiameter / 3600 > 0) && 
                ad < coeff * map.ViewAngle + sun.Semidiameter / 3600)
            {
                PointF p = map.Project(sun.Horizontal);

                float inc = map.GetRotationTowardsNorth(sun.Equatorial);

                g.TranslateTransform(p.X, p.Y);
                g.RotateTransform(inc);

                float size = map.GetDiskSize(sun.Semidiameter, 10);

                if (map.Schema == ColorSchema.Night && useTextures && size > 10)
                {
                    Date date = new Date(map.JulianDay);
                    DateTime dt = new DateTime(date.Year, date.Month, (int)date.Day, 0, 0, 0, DateTimeKind.Utc);
                    Brush brushSun = new SolidBrush(clrSunNight);
                    Image imageSun = imagesCache.RequestImage("Sun", dt, SunImageProvider, map.Redraw);
                    g.FillEllipse(brushSun, -size / 2, -size / 2, size, size);
                    if (imageSun != null)
                    {
                        g.DrawImage(imageSun, -size / 2, -size / 2, size, size);
                    }
                }
                else
                {
                    if (map.Schema == ColorSchema.White)
                    {
                        g.FillEllipse(Brushes.White, -size / 2, -size / 2, size, size);
                        g.DrawEllipse(Pens.Black, -size / 2, -size / 2, size, size);
                    }
                    else
                    {
                        Brush brushSun = new SolidBrush(colorSun);
                        g.FillEllipse(brushSun, -size / 2, -size / 2, size, size);
                    }
                }

                g.ResetTransform();

                if (settings.Get("SunLabel"))
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
            Graphics g = map.Graphics;

            if (settings.Get("Ground") && size > 0 && alpha > 0 && 2 * size > sunSize)
            {
                bool isEclipse = sun.Horizontal.Altitude > 0 && map.DayLightFactor < 1;

                using (var halo = new GraphicsPath())
                {
                    PointF p = map.Project(sun.Horizontal);                  
                    halo.AddEllipse(p.X - size, p.Y - size, 2 * size, 2 * size);

                    Region reg = new Region(halo);

                    using (GraphicsPath gpMoon = new GraphicsPath())
                    {
                        if (isEclipse)
                        {
                            PointF pMoon = map.Project(moon.Horizontal);
                            float szMoon = map.GetDiskSize(moon.Semidiameter, 10) / 2;
                            gpMoon.AddEllipse(pMoon.X - szMoon, pMoon.Y - szMoon, 2 * szMoon, 2 * szMoon);
                            reg.Exclude(gpMoon);
                        }

                        var brush = new PathGradientBrush(halo);
                        brush.CenterPoint = p;
                        brush.CenterColor = Color.FromArgb(alpha, clrSunDaylight);
                        brush.SurroundColors = new Color[] { Color.Transparent };
                        
                        g.FillRegion(brush, reg);

                        if (isEclipse)
                        {
                            g.FillPath(new SolidBrush(Color.FromArgb((int)(alpha * map.DayLightFactor), brush.CenterColor)), gpMoon);
                        }
                    }
                }                
            }
        }

        private void RenderMoon(IMapContext map)
        {
            if (!settings.Get("Moon")) return;

            bool isGround = settings.Get("Ground");
            double ad = Angle.Separation(moon.Horizontal, map.Center);
            double coeff = map.DiagonalCoefficient();
            
            if ((!isGround || moon.Horizontal.Altitude + moon.Semidiameter / 3600 > 0) && 
                ad < coeff * map.ViewAngle + moon.Semidiameter / 3600.0)
            {
                // drawing size
                float size = map.GetDiskSize(moon.Semidiameter, 10);

                Graphics g = map.Graphics;
                bool useTextures = settings.Get("MoonTexture");
                int q = Math.Min((int)settings.Get<TextureQuality>("MoonTextureQuality"), size < 256 ? 2 : (size < 1024 ? 4 : 8));
                string textureName = $"Moon-{q}k";

                PointF p = map.Project(moon.Horizontal);

                double inc = map.GetRotationTowardsNorth(moon.Equatorial);

                // final rotation of drawn image
                // axis rotation is negated because measured counter-clockwise
                float axisRotation = (float)(inc - moon.PAaxis);

                g.TranslateTransform(p.X, p.Y);
                g.RotateTransform(axisRotation);

                SolidBrush brushMoon = new SolidBrush(map.GetColor(Color.Gray));

                if (useTextures && size > 10)
                {
                    Image textureMoon = imagesCache.RequestImage("Moon", new PlanetTextureToken(textureName, moon.Libration.l, moon.Libration.b), MoonTextureProvider, map.Redraw);
                    if (textureMoon != null)
                    {
                        var gs = g.Save();
                        using (GraphicsPath gp = new GraphicsPath())
                        {
                            gp.AddEllipse(-size / 2, -size / 2, size, size);
                            g.SetClip(gp);
                        }
                        g.FillEllipse(brushMoon, -size / 2, -size / 2, size, size);
                        map.DrawImage(textureMoon, -size / 2, -size / 2, size, size);
                        g.Restore(gs);
                    }
                    else
                    {
                        g.FillEllipse(brushMoon, -size / 2, -size / 2, size, size);
                    }
                }
                else
                {
                    // Moon disk
                    g.FillEllipse(brushMoon, -size / 2, -size / 2, size, size);
                }

                g.ResetTransform();

                float phase = (float)moon.Phase * Math.Sign(moon.Elongation);
                float rotation = map.GetRotationTowardsEclipticPole(moon.Ecliptical0);
                GraphicsPath shadow = GetPhaseShadow(phase, size + 1);

                // shadowed part of disk
                g.TranslateTransform(p.X, p.Y);
                g.RotateTransform(rotation);
                g.FillPath(GetShadowBrush(map), shadow);
                g.ResetTransform();

                if (settings.Get("MoonLabel"))
                {
                    map.DrawObjectCaption(fontLabel, brushLabel, moon.Name, p, size);
                }

                map.AddDrawnObject(moon);
            }
        }

        private void DrawLunarSurfaceFeatures(IMapContext map)
        {
            if (settings.Get("Moon") && settings.Get("MoonTexture") && settings.Get("MoonSurfaceFeatures"))
            {
                float rotation = map.GetRotationTowardsNorth(moon.Equatorial) - (float)moon.PAaxis;
                DrawSurfaceFeatures(map, lunarFeatures, moon, 3474, rotation, moon.Libration.b, moon.Libration.l, map.GetColor(Color.AntiqueWhite));
            }
        }

        private void DrawMartianSurfaceFeatures(IMapContext map)
        {
            if (settings.Get("Planets") && settings.Get("PlanetsTextures") && settings.Get("PlanetsSurfaceFeatures"))
            {
                float rotation = map.GetRotationTowardsNorth(mars.Equatorial) - (float)mars.Appearance.P;
                DrawSurfaceFeatures(map, martianFeatures, mars, 6779, rotation, mars.Appearance.D, -mars.Appearance.CM, map.GetColor(Color.Wheat));
            }
        }

        private void DrawSurfaceFeatures(IMapContext map, ICollection<SurfaceFeature> features, SizeableCelestialObject body, float bodyDiameter, float axisRotation, double latitudeShift, double longitudeShift, Color featuresColor)
        {
            if (map.MouseButton == MouseButton.None &&
                Angle.Separation(map.MousePosition, body.Horizontal) < body.Semidiameter / 3600 &&
                (!settings.Get("Ground") || body.Horizontal.Altitude + body.Semidiameter / 3600 > 0))
            {
                PointF pMouse = map.Project(map.MousePosition);
                Graphics g = map.Graphics;

                // radius of celestial body disk, in pixels
                float r = map.GetDiskSize(body.Semidiameter, 10) / 2;

                // do not draw if size of disk is too small
                if (r > 100)
                {
                    PointF p = map.Project(body.Horizontal);
                    Brush brush = new SolidBrush(featuresColor);
                    Pen pen = new Pen(featuresColor);

                    // minimal diameter of feature, in pixels, that is allowed to be drawn on current magnification
                    const float minDiameterPx = 5;

                    // minimal diameter of feature, converted to km
                    float minDiameterKm = bodyDiameter / (2 * r) * minDiameterPx;

                    // visible coordinates of body disk center, assume as zero point 
                    CrdsGeographical c = new CrdsGeographical(0, 0);

                    foreach (var feature in features.TakeWhile(f => f.Diameter > minDiameterKm)) 
                    {
                        // feature outline radius, in pixels
                        float fr = feature.Diameter / bodyDiameter * r;

                        // visible coordinates of the feature
                        CrdsGeographical v = GetVisibleFeatureCoordinates(feature.Latitude, feature.Longitude, latitudeShift, longitudeShift);

                        // angular separation between visible center of the body disk and center of the feature
                        // expressed in degrees of arc, from 0 (center) to 90 (disk edge)
                        double sep = Angle.Separation(v, c);
                        if (sep < 85)
                        {
                            PointF pFeature = GetCartesianFeatureCoordinates(r, v, axisRotation);

                            // distance, in pixels, between center of the feature and current mouse position
                            double d = Math.Sqrt(Math.Pow(pMouse.X - pFeature.X - p.X, 2) + Math.Pow(pMouse.Y - pFeature.Y - p.Y, 2));

                            if (fr > 100 || d < fr)
                            {
                                // visible flattening of feature outline,
                                // depends on angular distance between feature and visible center of the body disk
                                float f = (float)Math.Cos(Angle.ToRadians(sep));

                                float labelDist = 3;
                                StringFormat format = null;

                                // draw feature outline (for craters only)
                                if (feature.TypeCode == "AA")
                                {
                                    g.TranslateTransform(p.X + pFeature.X, p.Y + pFeature.Y);
                                    g.RotateTransform(90 + (float)Angle.ToDegrees(Math.Atan2(pFeature.Y, pFeature.X)));
                                    g.DrawEllipse(pen, -fr, -fr * f, fr * 2, fr * f * 2);
                                    g.ResetTransform();
                                    labelDist = fr * 2;
                                }

                                // center label for maria, oceanus, sinus, lacus, palus 
                                if (feature.TypeCode == "ME" ||
                                    feature.TypeCode == "OC" ||
                                    feature.TypeCode == "SI" ||
                                    feature.TypeCode == "LC" ||
                                    feature.TypeCode == "PA")
                                {
                                    format = new StringFormat();
                                    format.Alignment = StringAlignment.Center;
                                    format.LineAlignment = StringAlignment.Center;
                                }
                                // fill central dot
                                else if (feature.TypeCode != "AA")
                                {
                                    g.TranslateTransform(p.X + pFeature.X, p.Y + pFeature.Y);
                                    g.FillEllipse(brush, -1, -1, 3, 3);
                                    g.ResetTransform();
                                }

                                // draw feature label
                                g.ResetTransform();
                                map.DrawObjectCaption(fontLabel, brush, feature.Name, new PointF(p.X + pFeature.X, p.Y + pFeature.Y), labelDist, format);                        
                            }
                        }
                    }
                }
            }
        }

        private PointF GetCartesianFeatureCoordinates(float r, CrdsGeographical c, float axisRotation)
        { 
            // convert to orthographic polar coordinates 
            float Y = r * (float)(-Math.Sin(Angle.ToRadians(c.Latitude)));
            float X = r * (float)(Math.Cos(Angle.ToRadians(c.Latitude)) * Math.Sin(Angle.ToRadians(c.Longitude)));

            // polar coordinates rotated around of visible center of the body disk
            float X_ = (float)(X * Math.Cos(Angle.ToRadians(axisRotation)) - Y * Math.Sin(Angle.ToRadians(axisRotation)));
            float Y_ = (float)(X * Math.Sin(Angle.ToRadians(axisRotation)) + Y * Math.Cos(Angle.ToRadians(axisRotation)));

            return new PointF(X_, Y_);
        }

        private CrdsGeographical GetVisibleFeatureCoordinates(double latitude, double longitude, double latitudeShift, double longitudeShift)
        {
            double theta = Angle.ToRadians(90 - latitude); // [0...180]
            double phi = Angle.ToRadians(Angle.To360(longitude)); // [0...360]

            // Cartesian coordinates
            double x = Math.Sin(theta) * Math.Cos(phi);
            double y = Math.Sin(theta) * Math.Sin(phi);
            double z = Math.Cos(theta);
            double[] v = new double[] { x, y, z };

            // rotate around Z axis (longitude / phi)
            double aZ = Angle.ToRadians(longitudeShift);
            double[,] mZ = new double[3, 3] { { Math.Cos(aZ), -Math.Sin(aZ), 0 }, { Math.Sin(aZ), Math.Cos(aZ), 0 }, { 0, 0, 1 } };
            Rotate(v, mZ);

            // rotate around Y axis (latitude / theta)
            double aY = Angle.ToRadians(-latitudeShift);
            double[,] mY = new double[3, 3] { { Math.Cos(aY), 0, Math.Sin(aY) }, { 0, 1, 0 }, { -Math.Sin(aY), 0, Math.Cos(aY) } };
            Rotate(v, mY);

            x = v[0];
            y = v[1];
            z = v[2];

            // back to spherical
            theta = 90 - Angle.ToDegrees(Math.Acos(z / Math.Sqrt(x * x + y * y + z * z)));
            phi = Angle.ToDegrees(Math.Atan2(y, x));

            return new CrdsGeographical(phi, theta);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        /// <param name="m">First index: row, second: column</param>
        private void Rotate(double[] v, double[,] m)
        {
            double x = v[0] * m[0, 0] + v[1] * m[1, 0] + v[2] * m[2, 0];
            double y = v[0] * m[0, 1] + v[1] * m[1, 1] + v[2] * m[2, 1];
            double z = v[0] * m[0, 2] + v[1] * m[1, 2] + v[2] * m[2, 2];
            v[0] = x;
            v[1] = y;
            v[2] = z;
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

            bool isGround = settings.Get("Ground");
            double coeff = map.DiagonalCoefficient();
            Graphics g = map.Graphics;

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

                        g.FillRegion(brushP, regionP);
                        g.FillRegion(brushU, regionU);
                    }

                    // outline circles
                    if (settings.Get("EarthShadowOutline") && map.ViewAngle > 0.5)
                    {
                        var brush = new SolidBrush(map.GetColor(clrShadowOutline));
                        var pen = new Pen(brush) { DashStyle = DashStyle.Dot };

                        g.TranslateTransform(p.X, p.Y);
                        g.DrawEllipse(pen, -szP / 2, -szP / 2, szP, szP);
                        g.DrawEllipse(pen, -szU / 2, -szU / 2, szU, szU);
                        g.ResetTransform();
                        if (map.ViewAngle <= 10)
                        {
                            map.DrawObjectCaption(fontShadowLabel, brush, Text.Get("EarthShadow.Label"), p, szP);
                        }
                    }
                }
            }
        }

        private void RenderPlanet<TPlanet>(IMapContext map, TPlanet planet) where TPlanet : SizeableCelestialObject, IPlanet
        {
            if (!settings.Get("Planets"))
            {
                return;
            }

            Graphics g = map.Graphics;
            double ad = Angle.Separation(planet.Horizontal, map.Center);
            bool isGround = settings.Get("Ground");
            bool useTextures = settings.Get("PlanetsTextures");
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

                    if (planet.Number == Planet.MARS)
                    {
                        DrawMartianSurfaceFeatures(map);
                    }

                    if (planet.Number == Planet.JUPITER)
                    {
                        // render shadows on Jupiter
                        RenderJupiterMoonShadow(map, planet);
                    }
                }
            }
        }

        private void RenderPlanetMoon<TPlanet, TPlanetMoon>(IMapContext map, TPlanet planet, TPlanetMoon moon, bool hasTexture = true) where TPlanet : SizeableCelestialObject, IPlanet where TPlanetMoon : SizeableCelestialObject, IPlanetMoon
        {
            if (!settings.Get("Planets")) return;
            if (!settings.Get("PlanetMoons")) return;

            bool isGround = settings.Get("Ground");
            bool useTextures = settings.Get("PlanetsTextures");
            double coeff = map.DiagonalCoefficient();
            double ad = Angle.Separation(moon.Horizontal, map.Center);
            Graphics g = map.Graphics;

            if ((!isGround || moon.Horizontal.Altitude + moon.Semidiameter / 3600 > 0) &&
                ad < coeff * map.ViewAngle + moon.Semidiameter / 3600)
            {
                PointF p = map.Project(moon.Horizontal);
                PointF pPlanet = map.Project(planet.Horizontal);

                float size = map.GetPointSize(moon.Magnitude, 2);
                float diam = map.GetDiskSize(moon.Semidiameter);

                // diameter is to small to render moon disk, 
                // but point size caclulated from magnitude is enough to be drawn
                if (size > diam && (int)size > 0)
                {
                    // do not draw moon point if eclipsed
                    if (!moon.IsEclipsedByPlanet)
                    {
                        // satellite is distant enough from the planet
                        // but too small to be drawn as disk
                        if (map.DistanceBetweenPoints(p, pPlanet) >= 5)
                        {
                            g.TranslateTransform(p.X, p.Y);
                            g.FillEllipse(new SolidBrush(map.GetColor(Color.Wheat)), -size / 2, -size / 2, size, size);
                            g.ResetTransform();

                            map.DrawObjectCaption(fontLabel, brushLabel, moon.Name, p, 2);
                            map.AddDrawnObject(moon);
                        }
                    }
                }
                // moon should be rendered as disk
                else if (diam >= size && (int)diam > 0)
                {
                    float rotation = map.GetRotationTowardsNorth(planet.Equatorial) + 360 - (float)planet.Appearance.P;

                    g.TranslateTransform(p.X, p.Y);
                    g.RotateTransform(rotation);

                    if (hasTexture && useTextures)
                    {
                        Image texture = imagesCache.RequestImage($"{planet.Number}-{moon.Number}", new PlanetTextureToken($"{planet.Number}-{moon.Number}", moon.CM, planet.Appearance.D), PlanetTextureProvider, map.Redraw);
                        if (texture != null)
                        {
                            var gs = g.Save();
                            using (GraphicsPath gp = new GraphicsPath())
                            {
                                gp.AddEllipse(-diam / 2, -diam / 2, diam, diam);
                                g.SetClip(gp);
                            }
                            map.DrawImage(texture, -diam / 2, -diam / 2, diam, diam);
                            g.Restore(gs);
                        }
                        else
                        {
                            g.FillEllipse(new SolidBrush(map.GetColor(Color.Gray)), -diam / 2, -diam / 2, diam, diam);
                        }
                        DrawVolume(map, diam, 0);
                    }
                    else
                    {
                        g.FillEllipse(new SolidBrush(map.GetColor(Color.Gray)), -diam / 2, -diam / 2, diam, diam);
                        if (diam > 2)
                        {
                            g.DrawEllipse(new Pen(map.GetSkyColor()), -diam / 2, -diam / 2, diam, diam);
                        }
                        DrawVolume(map, diam, 0);
                    }

                    g.ResetTransform();
                    
                    map.DrawObjectCaption(fontLabel, brushLabel, moon.Name, p, diam);
                    map.AddDrawnObject(moon);
                }
            }
        }

        private void RenderJupiterShadow(IMapContext map, JupiterMoon moon)
        {
            if (!moon.IsEclipsedByPlanet) return;

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

            Graphics g = map.Graphics;

            g.TranslateTransform(pMoon.X, pMoon.Y);
            g.RotateTransform(rotation);

            var gpM = new GraphicsPath();
            var gpU = new GraphicsPath();

            gpU.AddEllipse(p.X - diamEquat / 2 - 1, p.Y - diamPolar / 2 - 1, diamEquat + 2, diamPolar + 2);
            gpM.AddEllipse(-szB / 2 - 0.5f, -szB / 2 - 0.5f, szB + 1, szB + 1);

            var regionU = new Region(gpU);
            regionU.Intersect(gpM);

            if (!regionU.IsEmpty(map.Graphics))
            {
                g.FillRegion(new SolidBrush(clrJupiterShadow), regionU);
            }

            g.ResetTransform();
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
            Graphics g = map.Graphics;

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

                    g.TranslateTransform(pBody.X, pBody.Y);
                    g.RotateTransform(rotation);

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

                            g.FillRegion(brushP, regionP);
                            g.FillRegion(brushU, regionU);

                            // outline circles
                            if (settings.Get("JupiterMoonsShadowOutline") && szP > 20)
                            {
                                g.DrawEllipse(penShadowOutline, p.X - (szP + szU) / 4, p.Y - (szP + szU) / 4, (szP + szU) / 2, (szP + szU) / 2);
                                g.DrawEllipse(penShadowOutline, p.X - szU / 2, p.Y - szU / 2, szU, szU);

                                PointF[] points = new PointF[] { new PointF(p.X, p.Y) };
                                g.TransformPoints(CoordinateSpace.Page, CoordinateSpace.World, points);
                                g.ResetTransform();
                                map.DrawObjectCaption(fontShadowLabel, brushShadowLabel, moon.ShadowName, points[0], szP);
                            }
                        }
                    }

                    g.ResetTransform();
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

        private void DrawPlanetGlobe<TPlanet>(IMapContext map, TPlanet planet, float diam) where TPlanet : SizeableCelestialObject, IPlanet
        {
            Graphics g = map.Graphics;
            float diamEquat = diam;
            float diamPolar = (1 - planet.Flattening) * diam;
            bool useTextures = settings.Get("PlanetsTextures");

            if (!(useTextures && (map.Schema == ColorSchema.Red || map.Schema == ColorSchema.White)))
            {
                g.FillEllipse(GetPlanetColor(map, planet.Number), -diamEquat / 2, -diamPolar / 2, diamEquat, diamPolar);
            }

            if (useTextures)
            {
                PlanetTextureToken token;
                if (planet.Number == Planet.MARS && settings.Get("PlanetsMartianPolarCaps"))
                {
                    token = new PlanetTextureToken(planet.Number.ToString(), -planet.Appearance.CM, planet.Appearance.D, planetsCalc.MarsNPCWidth, planetsCalc.MarsSPCWidth);
                }
                else if (planet.Number == Planet.JUPITER)
                {
                    token = new PlanetTextureToken(planet.Number.ToString(), planetsCalc.GreatRedSpotLongitude - planet.Appearance.CM, planet.Appearance.D);
                }
                else
                {
                    token = new PlanetTextureToken(planet.Number.ToString(), -planet.Appearance.CM, planet.Appearance.D);
                }

                Image texturePlanet = imagesCache.RequestImage(planet.Number.ToString(), token, PlanetTextureProvider, map.Redraw);

                if (texturePlanet != null)
                {
                    var gs = g.Save();                    
                    using (GraphicsPath gp = new GraphicsPath())
                    {
                        gp.AddEllipse(-diamEquat / 2, -diamPolar / 2, diamEquat, diamPolar);                        
                        g.SetClip(gp);
                    }
                    g.FillEllipse(GetPlanetColor(map, planet.Number), -diamEquat / 2, -diamPolar / 2, diamEquat, diamPolar);
                    map.DrawImage(texturePlanet, -diamEquat / 2, -diamPolar / 2, diamEquat, diamPolar);
                    g.Restore(gs);
                    DrawVolume(map, diam, planet.Flattening);
                }
            }
            else
            {
                DrawVolume(map, diam, planet.Flattening);
            }
        }

        private void DrawVolume(IMapContext map, float diam, float flattening)
        {
            if (map.Schema == ColorSchema.White) return;

            Image textureVolume = imagesCache.RequestImage("volume", map.GetSkyColor(), VolumeTextureProvider, map.Redraw);
            if (textureVolume != null)
            {
                float diamEquat = diam;
                float diamPolar = (1 - flattening) * diam;
                Graphics g = map.Graphics;                

                var gs = g.Save();
                using (GraphicsPath gp = new GraphicsPath())
                {
                    gp.AddEllipse(-diamEquat / 2 - 1, -diamPolar / 2 - 1, diamEquat + 2, diamPolar + 2);
                    g.SetClip(gp);
                }
                map.DrawImage(textureVolume, -diamEquat / 2 * 1.2f, -diamPolar / 2 * 1.2f, diamEquat * 1.2f, diamPolar * 1.2f);
                g.Restore(gs);
            }
        }

        private void DrawRotationAxis(Graphics g, float diam)
        {
            if (settings.Get("ShowRotationAxis"))
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
                LongutudeShift = token.Longitude,
                OutputImageSize = 1024,
                RenderPolarCaps = token.RenderPolarCaps,
                NorthernPolarCap = token.NorthernPolarCap,
                SouthernPolarCap = token.SouthernPolarCap,
                TextureFilePath = $"Data\\{token.TextureName}.jpg"
            });
        }

        private Image MoonTextureProvider(PlanetTextureToken token)
        {
            uint imageSize;
            switch (token.TextureName)
            {
                case "Moon-2k":
                    imageSize = 1024;
                    break;
                default:
                case "Moon-4k":
                    imageSize = 2048;
                    break;
                case "Moon-8k":
                    imageSize = 4096;
                    break;
            }

            return sphereRenderer.Render(new RendererOptions()
            {
                LatitudeShift = token.Latitude,
                LongutudeShift = token.Longitude,
                OutputImageSize = imageSize,
                TextureFilePath = $"Data\\{token.TextureName}.jpg"
            });
        }

        private Image SunImageProvider(DateTime date)
        {
            string template = settings.Get<string>("SunTexturePath");
            string format = Regex.Replace(template, "{([^}]*)}", match => "{0:" + match.Groups[1].Value + "}");
            string url = string.Format(format, date);
            return solarTextureDownloader.Download(url);
        }

        private Image VolumeTextureProvider(Color skyColor)
        {
            Bitmap bitmap = new Bitmap(256, 256);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath gpVolume = new GraphicsPath())
                {
                    gpVolume.AddEllipse(-128, -128, 512, 512);
                    using (PathGradientBrush brushVolume = new PathGradientBrush(gpVolume))
                    {
                        brushVolume.CenterPoint = new PointF(128, 128);
                        brushVolume.CenterColor = skyColor;
                        brushVolume.SetSigmaBellShape(0.3f, 1);
                        brushVolume.SurroundColors = new Color[] { Color.Transparent };
                        g.FillEllipse(brushVolume, 0, 0, 256, 256);
                    }
                }
            }
            return bitmap;
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
                case 9:
                    color = Color.FromArgb(207, 192, 162);
                    break;
                default:
                    color = Color.Gray;
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
            
            /// <summary>
            /// Flag indicating polar caps rendering is needed
            /// </summary>
            public bool RenderPolarCaps { get; private set; }

            /// <summary>
            /// Radius of northern polar cap, in degrees
            /// </summary>
            public double NorthernPolarCap { get; private set; }

            /// <summary>
            /// Radius of southern polar cap, in degrees
            /// </summary>
            public double SouthernPolarCap { get; private set; }

            public PlanetTextureToken(string name, double longitude, double latitude)
            {
                TextureName = name;
                Longitude = longitude;
                Latitude = latitude;
                RenderPolarCaps = false;
                NorthernPolarCap = 0;
                SouthernPolarCap = 0;
            }

            public PlanetTextureToken(string name, double longitude, double latitude, double northernPolarCap, double southernPolarCap)
            {
                TextureName = name;
                Longitude = longitude;
                Latitude = latitude;
                RenderPolarCaps = true;
                NorthernPolarCap = northernPolarCap;
                SouthernPolarCap = southernPolarCap;
            }

            public override bool Equals(object obj)
            {
                if (obj is PlanetTextureToken other)
                {
                    return 
                        TextureName.Equals(other.TextureName) &&
                        RenderPolarCaps == other.RenderPolarCaps &&
                        Math.Abs(NorthernPolarCap - other.NorthernPolarCap) < 1 &&
                        Math.Abs(SouthernPolarCap - other.SouthernPolarCap) < 1 &&
                        Math.Abs(Longitude - other.Longitude) < 0.01 &&
                        Math.Abs(Latitude - other.Latitude) < 0.01;
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
