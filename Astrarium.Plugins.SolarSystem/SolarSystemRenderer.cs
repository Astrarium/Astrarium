using Astrarium.Algorithms;
using Astrarium.Plugins.SolarSystem.Objects;
using Astrarium.Types;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
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
        private readonly ITextureManager textureManager;
        private readonly Lazy<TextRenderer> textRenderer = new Lazy<TextRenderer>(() => new TextRenderer(256, 32));

        private readonly Sun sun;
        private readonly Moon moon;
        private readonly Planet mars;
        private readonly Pluto pluto;

        private Font fontShadowLabel = new Font("Arial", 8);
        private Brush brushLabel;

        private readonly Color clrSunDaylight = Color.FromArgb(255, 255, 200);
        private readonly Color clrSunNight = Color.FromArgb(250, 210, 10);

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

        private readonly SolarTextureManager solarTextureManager;
        private readonly BaseSphereRenderer sphereRenderer;
        private readonly ImagesCache imagesCache = new ImagesCache();
        private readonly ICollection<SurfaceFeature> lunarFeatures;
        private readonly ICollection<SurfaceFeature> martianFeatures;

        private readonly string dataPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data");

        public SolarSystemRenderer(ISkyMap map, LunarCalc lunarCalc, SolarCalc solarCalc, PlanetsCalc planetsCalc, ITextureManager textureManager, ISettings settings)
        {
            this.planetsCalc = planetsCalc;
            this.textureManager = textureManager;
            this.settings = settings;

            sun = solarCalc.Sun;
            moon = lunarCalc.Moon;
            mars = planetsCalc.Planets.ElementAt(Planet.MARS - 1);
            pluto = planetsCalc.Pluto;

            var featuresReader = new SurfaceFeaturesReader();
            lunarFeatures = featuresReader.Read(Path.Combine(dataPath, "LunarFeatures.dat"));
            martianFeatures = featuresReader.Read(Path.Combine(dataPath, "MartianFeatures.dat"));

            solarTextureManager = new SolarTextureManager();
            solarTextureManager.FallbackAction += () => map.Invalidate();

            textureManager.FallbackAction += () => map.Invalidate();
            textureManager.SetTextureParams(Path.Combine(dataPath, "PolarCap.png"), SetPolarCapTextureParameters);

            sphereRenderer = new SphereRendererFactory().CreateRenderer();
        }


        public override RendererOrder Order => RendererOrder.SolarSystem;

        public override void Render(ISkyMap map)
        {
            brushLabel = new SolidBrush(settings.Get<SkyColor>("ColorSolarSystemLabel").Night);
            bool drawLabelMag = settings.Get("PlanetsLabelsMag");
            var prj = map.SkyProjection;

            var bodies = planetsCalc.Planets
                .Where(p => p.Number != Planet.EARTH)
                .Cast<ISolarSystemObject>()
                .Concat(new[] { pluto })
                .Concat(new[] { sun })
                .Concat(new[] { moon })
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
                if (body is Planet planet && settings.Get("Planets"))
                {
                    float size = prj.GetPointSize(planet.Magnitude, maxDrawingSize: 7);
                    double diam = prj.GetDiskSize(planet.Semidiameter);

                    // draw planets regardless zoom level
                    if (size < 1)
                    {
                        if (settings.Get("PlanetsDrawAll"))
                            size = 1;
                        else
                            continue;
                    }

                    // draw as point
                    if (size >= diam)
                    {
                        var p = prj.Project(planet.Equatorial);
                        if (prj.IsInsideScreen(p))
                        {
                            GL.Enable(EnableCap.PointSmooth);
                            GL.Enable(EnableCap.Blend);
                            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                            GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest);

                            GL.PointSize(size);
                            GL.Begin(PrimitiveType.Points);
                            GL.Color3(GetPlanetColor(planet.Number));
                            GL.Vertex2(p.X, p.Y);
                            GL.End();

                            map.AddDrawnObject(p, planet);

                            if (settings.Get("PlanetsLabels"))
                            {
                                string label = drawLabelMag ? $"{planet.Name} {Formatters.Magnitude.Format(planet.Magnitude)}" : planet.Name;
                                var fontLabel = settings.Get<Font>("SolarSystemLabelsFont");
                                textRenderer.Value.DrawString(label, fontLabel, brushLabel, p);
                            }
                        }
                    }
                    // draw as sphere
                    else
                    {
                        double rotAxis = prj.GetAxisRotation(planet.Equatorial, planet.Appearance.P);
                        double rotPhase = prj.GetPhaseRotation(planet.Ecliptical);

                        DrawPlanet(map, planet, new SphereParameters()
                        {
                            Equatorial = planet.Equatorial,
                            TextureName = Path.Combine(dataPath, $"{planet.Number}.jpg"),
                            Semidiameter = planet.Semidiameter,
                            PhaseAngle = planet.PhaseAngle,
                            Flattening = planet.Flattening,
                            LatitudeShift = -planet.Appearance.D,
                            LongitudeShift = planet.Appearance.CM - (planet.Number == Planet.JUPITER ? planetsCalc.GreatRedSpotLongitude : 0),
                            RotationAxis = rotAxis,
                            RotationPhase = rotPhase,
                            BodyPhysicalDiameter = 2 * Planet.EQUATORIAL_RADIUS[planet.Number - 1],
                            SurfaceFeatures = planet.Number == Planet.MARS && settings.Get("PlanetsSurfaceFeatures") ? martianFeatures : null,
                            SmoothShadow = planet.Number > Planet.MARS,
                            DrawRings = planet.Number == Planet.SATURN,
                            DrawPolarCap = planet.Number == Planet.MARS && settings.Get("PlanetsMartianPolarCaps"),
                            NorthernPolarCap = planet.Number == Planet.MARS ? planetsCalc.MarsNPCWidth : 0,
                            SouthernPolarCap = planet.Number == Planet.MARS ? planetsCalc.MarsSPCWidth : 0,
                            DrawLabel = settings.Get("PlanetsLabels")
                        });

                        if (planet.Number == Planet.JUPITER)
                        {
                            // draw moon shadows over Jupiter
                            RenderJupiterMoonShadow(map, planet);
                        }
                    }
                }
                else if (body is MarsMoon mm)
                {
                    float size = prj.GetPointSize(mm.Magnitude, 2f);
                    if (size >= 1)
                    {
                        var p = prj.Project(mm.Equatorial);
                        if (prj.IsInsideScreen(p))
                        {
                            GL.Enable(EnableCap.PointSmooth);
                            GL.Enable(EnableCap.Blend);
                            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                            GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest);

                            GL.PointSize(size);
                            GL.Begin(PrimitiveType.Points);
                            GL.Color3(Color.White);
                            GL.Vertex2(p.X, p.Y);
                            GL.End();

                            map.AddDrawnObject(p, mm);

                            if (settings.Get("PlanetsLabels"))
                            {
                                var fontLabel = settings.Get<Font>("SolarSystemLabelsFont");
                                textRenderer.Value.DrawString(mm.Name, fontLabel, brushLabel, p);
                            }
                        }

                    }
                }
                else if (body is JupiterMoon jupiterMoon)
                {
                    float size = prj.GetPointSize(jupiterMoon.Magnitude, 3);
                    double diam = prj.GetDiskSize(jupiterMoon.Semidiameter);

                    if (size < 1) continue;

                    // draw as point
                    if (size >= diam)
                    {
                        var p = prj.Project(jupiterMoon.Equatorial);
                        if (prj.IsInsideScreen(p))
                        {
                            GL.Enable(EnableCap.PointSmooth);
                            GL.Enable(EnableCap.Blend);
                            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                            GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest);

                            GL.PointSize(size);
                            GL.Begin(PrimitiveType.Points);
                            GL.Color3(Color.White);
                            GL.Vertex2(p.X, p.Y);
                            GL.End();

                            map.AddDrawnObject(p, jupiterMoon);
                        }
                    }
                    else
                    {
                        var jupiter = planetsCalc.Planets.ElementAt(Planet.JUPITER - 1);
                        double rotAxis = prj.GetAxisRotation(jupiterMoon.Equatorial, jupiter.Appearance.P);
                        double rotPhase = prj.GetPhaseRotation(jupiter.Ecliptical);
                        DrawPlanet(map, jupiterMoon, new SphereParameters()
                        {
                            Equatorial = jupiterMoon.Equatorial,
                            TextureName = Path.Combine(dataPath, $"5-{jupiterMoon.Number}.jpg"),
                            Semidiameter = jupiterMoon.Semidiameter,
                            LongitudeShift = jupiterMoon.CM,
                            PhaseAngle = jupiter.PhaseAngle,
                            RotationAxis = rotAxis,
                            RotationPhase = rotPhase,
                            DrawLabel = settings.Get("PlanetsLabels")
                        });

                        // shadow of other moons above current
                        RenderJupiterMoonShadow(map, jupiterMoon, jupiterMoon.RectangularS);

                        // shadow of jupiter above current
                        RenderJupiterShadow(map, jupiterMoon);
                    }
                }
                else if (body is SaturnMoon saturnMoon)
                {
                    float size = prj.GetPointSize(saturnMoon.Magnitude, 1.5f);
                    double diam = prj.GetDiskSize(saturnMoon.Semidiameter);

                    if (size < 1) continue;

                    // draw as point
                    if (size >= diam)
                    {
                        var p = prj.Project(saturnMoon.Equatorial);
                        if (prj.IsInsideScreen(p))
                        {
                            GL.Enable(EnableCap.PointSmooth);
                            GL.Enable(EnableCap.Blend);
                            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                            GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest);

                            GL.PointSize(size);
                            GL.Begin(PrimitiveType.Points);
                            GL.Color3(Color.White);
                            GL.Vertex2(p.X, p.Y);
                            GL.End();

                            map.AddDrawnObject(p, saturnMoon);
                        }
                    }
                    else
                    {
                        var saturn = planetsCalc.Planets.ElementAt(Planet.SATURN - 1);
                        double rotAxis = prj.GetAxisRotation(saturnMoon.Equatorial, saturn.Appearance.P);
                        double rotPhase = prj.GetPhaseRotation(saturn.Ecliptical);
                        DrawPlanet(map, saturnMoon, new SphereParameters()
                        {
                            Equatorial = saturnMoon.Equatorial,
                            TextureName = Path.Combine(dataPath, $"6-{saturnMoon.Number}.jpg"),
                            FallbackTextureName = Path.Combine(dataPath, $"Unknown.jpg"),
                            Semidiameter = saturnMoon.Semidiameter,
                            LongitudeShift = saturnMoon.CM,
                            PhaseAngle = saturn.PhaseAngle,
                            RotationAxis = rotAxis,
                            RotationPhase = rotPhase,
                            DrawLabel = settings.Get("PlanetsLabels")
                        });
                    }
                }
                else if (body is UranusMoon uranusMoon)
                {
                    float size = prj.GetPointSize(uranusMoon.Magnitude, 1.5f);
                    
                    if (size < 1) continue;

                    // draw as point
                    var p = prj.Project(uranusMoon.Equatorial);
                    if (prj.IsInsideScreen(p))
                    {
                        GL.Enable(EnableCap.PointSmooth);
                        GL.Enable(EnableCap.Blend);
                        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                        GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest);

                        GL.PointSize(size);
                        GL.Begin(PrimitiveType.Points);
                        GL.Color3(Color.White);
                        GL.Vertex2(p.X, p.Y);
                        GL.End();

                        map.AddDrawnObject(p, uranusMoon);
                    }
                    
                }
                else if (body is NeptuneMoon neptuneMoon)
                {
                    float size = prj.GetPointSize(neptuneMoon.Magnitude, 2f);
                    if (size < 1) continue;

                    // draw as point
                    var p = prj.Project(neptuneMoon.Equatorial);
                    if (prj.IsInsideScreen(p))
                    {
                        GL.Enable(EnableCap.PointSmooth);
                        GL.Enable(EnableCap.Blend);
                        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                        GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest);

                        GL.PointSize(size);
                        GL.Begin(PrimitiveType.Points);
                        GL.Color3(Color.Gray);
                        GL.Vertex2(p.X, p.Y);
                        GL.End();

                        if (settings.Get("PlanetsLabels"))
                        {
                            var fontLabel = settings.Get<Font>("SolarSystemLabelsFont");
                            textRenderer.Value.DrawString(neptuneMoon.Name, fontLabel, brushLabel, p + (0.35f * size));
                        }

                        map.AddDrawnObject(p, neptuneMoon);
                    }

                }

                else if (body is Sun && settings.Get("Sun"))
                {
                    double rotAxis = Angle.ToRadians(prj.GetAxisRotation(sun.Equatorial, -prj.Context.Epsilon));

                    double size = prj.GetDiskSize(sun.Semidiameter, 10);
                    double r = size / 2;
                    Vec2 p = prj.Project(sun.Equatorial);

                    GL.Enable(EnableCap.Texture2D);
                    GL.Enable(EnableCap.Blend);
                    GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

                    int textureId = -1;

                    if (settings.Get("SunTexture"))
                    {
                        textureId = solarTextureManager.GetTexture(prj.Context.JulianDay);
                        GL.BindTexture(TextureTarget.Texture2D, textureId);
                    }

                    GL.PushMatrix();
                    GL.Translate(p.X, p.Y, 0);

                    GL.Begin(PrimitiveType.TriangleFan);

                    if (textureId > 0)
                    {
                        // TODO: tint color
                        GL.Color4(Color.White);
                    }
                    else
                    {
                        GL.Color4(Color.Orange);
                    }

                    for (int i = 0; i <= 64; i++)
                    {
                        double ang0 = Angle.ToRadians(i / 64.0 * 360);
                        double ang = ang0 + rotAxis;
                        Vec2 v = new Vec2(r * Math.Cos(ang), r * Math.Sin(ang));

                        if (textureId > 0)
                        {
                            double tx = (prj.FlipHorizontal ? -1 : 1) * Math.Cos(ang0);
                            double ty = (prj.FlipVertical ? -1 : 1) * Math.Sin(ang0);
                            Vec2 vt = new Vec2(tx, ty);
                            GL.TexCoord2(0.5f + 0.499f * vt.X, 0.5f + 0.499f * vt.Y);
                        }
                        GL.Vertex2(v.X, v.Y);
                    }

                    GL.End();

                    GL.PopMatrix();

                    GL.Disable(EnableCap.Texture2D);
                    GL.Disable(EnableCap.Blend);
                }
                else if (body is Moon && settings.Get("Moon"))
                {
                    double rotAxis = prj.GetAxisRotation(moon.Equatorial, moon.PAaxis);
                    double rotPhase = prj.GetPhaseRotation(moon.Ecliptical0);

                    double size = prj.GetDiskSize(moon.Semidiameter, 10);
                    int q = Math.Min((int)settings.Get<TextureQuality>("MoonTextureQuality"), size < 256 ? 2 : (size < 1024 ? 4 : 8));
                    string textureName = $"Moon-{q}k.jpg";

                    DrawPlanet(map, moon, new SphereParameters()
                    {
                        Equatorial = moon.Equatorial,
                        TextureName = Path.Combine(dataPath, textureName),
                        FallbackTextureName = Path.Combine(dataPath, "Moon-2k.jpg"),
                        MinimalSize = 10,
                        Semidiameter = moon.Semidiameter,
                        PhaseAngle = moon.PhaseAngle,
                        LatitudeShift = -moon.Libration.b,
                        LongitudeShift = -moon.Libration.l,
                        RotationAxis = rotAxis,
                        RotationPhase = rotPhase,
                        BodyPhysicalDiameter = 3474,
                        SurfaceFeatures = settings.Get("MoonSurfaceFeatures") ? lunarFeatures : null,
                        EarthShadowApperance = moon.EarthShadow,
                        EarthShadowCoordinates = moon.EarthShadowCoordinates,
                        DrawLabel = settings.Get("MoonLabel")
                    });
                }
            }
        }

        private class SphereParameters
        {
            public bool SmoothShadow { get; set; }
            public CrdsEquatorial Equatorial { get; set; }
            public string TextureName { get; set; }
            public string FallbackTextureName { get; set; }

            public float MinimalSize { get; set; }

            public ICollection<SurfaceFeature> SurfaceFeatures { get; set; }

            /// <summary>
            /// Physical diameter of celestial body, in kilometers
            /// </summary>
            public double BodyPhysicalDiameter { get; set; }

            /// <summary>
            /// Body semidiameter, in seconds of arc
            /// </summary>
            public double Semidiameter { get; set; }
            public ShadowAppearance EarthShadowApperance { get; set; }
            public CrdsEquatorial EarthShadowCoordinates { get; set; }
            public bool DrawRings { get; set; }

            /// <summary>
            /// Rotation of axis, measured counter-clockwise from top of screen
            /// </summary>
            public double RotationAxis { get; set; }

            /// <summary>
            /// Rotation of phase, measured counter-clockwise from top of screen
            /// </summary>
            public double RotationPhase { get; set; }

            public double PhaseAngle { get; set; }
            public double Flattening { get; set; }

            public double LongitudeShift { get; set; }
            public double LatitudeShift { get; set; }

            public bool DrawLabel { get; set; }

            public bool DrawPolarCap { get; set; }
            public double NorthernPolarCap { get; set; }
            public double SouthernPolarCap { get; set; }
        }

        private void SetPolarCapTextureParameters()
        {
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.MirroredRepeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.MirroredRepeat);
        }

        private void DrawPlanet(ISkyMap map, SizeableCelestialObject body, SphereParameters data)
        {
            var prj = map.SkyProjection;

            // do not draw if out of screen
            double fov = prj.Fov * Math.Max(prj.ScreenWidth, prj.ScreenHeight) / Math.Min(prj.ScreenWidth, prj.ScreenHeight);

            if (Angle.Separation(prj.CenterEquatorial, data.Equatorial) > fov + body.Semidiameter / 3600 * 2) return;

            GL.Enable(EnableCap.Texture2D);

            float[] zero = new float[4] { 0, 0, 0, 0 };
            float[] ambient;
            float[] diffuse;

            if (settings.Get<ColorSchema>("Schema") == ColorSchema.Red)
            {
                diffuse = new float[4] { 0.5f, 0, 0, 1f };
                ambient = new float[4] { 0.5f, 0, 0, 0.5f };
            }
            else
            {
                // color of illuminated part
                diffuse = new float[4] { 1, 1, 1, 1 };

                // color of unilluminated part
                ambient = new float[4] { 0.25f, 0.25f, 0.25f, 1f };
            }

            GL.Light(LightName.Light0, LightParameter.Ambient, new float[4] { 0, 0, 0, 1 });
            GL.Light(LightName.Light0, LightParameter.Diffuse, diffuse);
            GL.Light(LightName.Light0, LightParameter.Specular, zero);
            GL.Light(LightName.Light0, LightParameter.ConstantAttenuation, 1.0f);

            GL.Material(MaterialFace.Front, MaterialParameter.Ambient, ambient);
            GL.Material(MaterialFace.Front, MaterialParameter.Diffuse, diffuse);
            GL.Material(MaterialFace.Front, MaterialParameter.Emission, zero);
            GL.Material(MaterialFace.Front, MaterialParameter.Shininess, zero);
            GL.Material(MaterialFace.Front, MaterialParameter.Specular, zero);

            GL.Enable(EnableCap.Light0);
            GL.Enable(EnableCap.Lighting);
            

            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);
            GL.CullFace(CullFaceMode.Front);

            DrawPlanetSphere(map, body, data);

            GL.Disable(EnableCap.Light0);
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.Texture2D);


            if (data.EarthShadowApperance != null && data.EarthShadowCoordinates != null)
            {
                DrawEarthShadow(map, data);
            }

            if (data.SurfaceFeatures != null)
            {
                DrawPlanetFeatures(map, data);
            }

            if (data.DrawLabel)
            {
                var fontLabel = settings.Get<Font>("SolarSystemLabelsFont");
                double r = prj.GetDiskSize(data.Semidiameter, 10) / 2;
                Vec2 p = prj.Project(data.Equatorial);
                textRenderer.Value.DrawString(body.Names.First(), fontLabel, brushLabel, new Vec2(p.X + 0.72 * r, p.Y - 0.72 * r));
            }
        }

        private void DrawEarthShadow(ISkyMap map, SphereParameters data)
        {
            var prj = map.SkyProjection;

            // moon radius in pixels (1 extra pixel added for better rendering)
            float rMoon = prj.GetDiskSize(data.Semidiameter) / 2 + 1;

            if (rMoon > 5)
            {
                // center of the moon in screen coordinates
                var pMoon = prj.Project(data.Equatorial);

                // center of the shadow
                var pShadow = prj.Project(data.EarthShadowCoordinates);

                // moon semidiameter in seconds of arc
                double sdMoon = data.Semidiameter;

                // semidiameter of penumbra in seconds of arc
                double sdPenumbra = data.EarthShadowApperance.PenumbraRadius * 6378.0 / 1738.0 * sdMoon;

                // semidiameter of umbra in seconds of arc
                double sdUmbra = sdPenumbra / data.EarthShadowApperance.Ratio;

                // semidiameter of penumbra in pixels
                double sdPenumbraPixels = prj.GetDiskSize(sdPenumbra) / 2;

                // semidiameter of umbra in pixels
                double sdUmbraPixels = sdPenumbraPixels / data.EarthShadowApperance.Ratio;

                // distance, in degrees, between lunar center and center of the Earth shadow
                double dist = Angle.Separation(data.Equatorial, data.EarthShadowCoordinates);

                // color of umbra center
                Color colorCenter = Color.FromArgb(220, 10, 0, 0);

                // color of umbra edge
                Color colorEdge = Color.FromArgb(230, Color.Black);

                double maxDist = (sdUmbra - sdMoon) / 3600.0;

                // full eclipse (moon inside umbra)
                if (dist < maxDist)
                {
                    // center of umbra stays black
                    colorCenter = Color.FromArgb(220, 10, 0, 0);

                    // whereas the umbra edges are dark red 
                    colorEdge = GradientColor(Color.FromArgb(220, 120, 40, 0), colorEdge, dist / maxDist);
                }

                double[] shadowRadii = new double[] { 0, sdUmbraPixels * 0.99, sdUmbraPixels, sdUmbraPixels * 1.01, sdPenumbraPixels };
                Color[] shadowColors = new Color[] { colorCenter, colorEdge, colorEdge, Color.FromArgb(200, Color.Black), Color.FromArgb(0, 0, 0, 0) };

                // render shadow
                RenderEclipseShadow(pMoon, pShadow, rMoon, shadowRadii, shadowColors);

                // draw shadow outline
                if (settings.Get("EarthShadowOutline"))
                {
                    var pen = new Pen(clrShadowOutline) { DashStyle = DashStyle.Dot };

                    Primitives.DrawEllipse(pShadow, pen, sdPenumbraPixels);
                    Primitives.DrawEllipse(pShadow, pen, sdUmbraPixels);

                    if (map.SkyProjection.Fov <= 10)
                    {
                        var brush = new SolidBrush(clrShadowOutline);
                        textRenderer.Value.DrawString(Text.Get("EarthShadow.Label"), fontShadowLabel, brush, new Vec2(sdPenumbraPixels * 0.71, -sdPenumbraPixels * 0.71));
                    }
                }
            }
        }

        private void DrawPlanetFeatures(ISkyMap map, SphereParameters data)
        {
            var prj = map.SkyProjection;

            // radius of celestial body disk, in pixels
            float r = prj.GetDiskSize(data.Semidiameter) / 2;

            if (r > 100)
            {
                // feature types that should be drawn with outline
                string[] outlinedFeatures = new string[] { "AA", "SF" };

                // feature types that should be labeled in central point
                string[] centeredFeatures = new string[] { "ME", "OC", "SI", "LC", "PA", "PR", "MO", "VA", "RU", "RI", "DO", "CA", "AL", "LF", "PL" };

                // center of the Moon in screen coordinates
                var p = prj.Project(data.Equatorial);

                // TODO: move color to settings
                Brush brush = new SolidBrush(Color.AntiqueWhite);
                Pen pen = new Pen(Color.AntiqueWhite);

                // TODO: create separate setting for feature labels font
                var fontLabel = settings.Get<Font>("SolarSystemLabelsFont");

                // minimal diameter of feature, in pixels, that is allowed to be drawn on current zoom
                const float minDiameterPx = 5;

                // minimal diameter of feature, converted to km
                double minDiameterKm = data.BodyPhysicalDiameter / (2 * r) * minDiameterPx;

                // visible coordinates of body disk center, assume as zero point 
                CrdsGeographical c = new CrdsGeographical(0, 0);

                foreach (SurfaceFeature feature in data.SurfaceFeatures.TakeWhile(f => f.Diameter == 0 || f.Diameter > minDiameterKm))
                {
                    // visible coordinates of the feature relative to body disk center
                    CrdsGeographical v = GetVisibleFeatureCoordinates(feature.Latitude, feature.Longitude, data.LatitudeShift, data.LongitudeShift);

                    // angular distance between disk center and feature
                    double sep = Angle.Separation(v, c);

                    // if feature is not too close to disk edge
                    if (sep < 85)
                    {
                        Vec2 pFeature = GetCartesianFeatureCoordinates(prj, r, v, data.RotationAxis);

                        if (prj.IsInsideScreen(p + pFeature))
                        {
                            // feature outline radius, in pixels
                            double fr = (feature.Diameter > 0 ? feature.Diameter : data.BodyPhysicalDiameter / 6) / data.BodyPhysicalDiameter * r;

                            // distance, in pixels, between center of the feature and current mouse position
                            double d = Math.Sqrt(Math.Pow(map.MouseCoordinates.X - pFeature.X - p.X, 2) + Math.Pow(map.MouseCoordinates.Y - pFeature.Y - p.Y, 2));

                            if (fr > 100 || d < fr)
                            {
                                GL.PushMatrix();
                                GL.Translate(p.X, p.Y, 0);

                                if (outlinedFeatures.Contains(feature.TypeCode))
                                {
                                    // visible flattening of feature outline,
                                    // depends on angular distance between feature and visible center of the body disk
                                    float f = (float)Math.Cos(Angle.ToRadians(sep));

                                    // rotation of a feature outline
                                    double rot = 90 + Angle.ToDegrees(Math.Atan2(pFeature.Y, pFeature.X));

                                    Primitives.DrawEllipse(new Vec2(pFeature.X, pFeature.Y), pen, fr, fr * f, rot);
                                    textRenderer.Value.DrawString(feature.Name, fontLabel, brush, new Vec2(pFeature.X, pFeature.Y));
                                }
                                else if (centeredFeatures.Contains(feature.TypeCode))
                                {
                                    string label = feature.Name.Contains("Mare") || feature.TypeCode == "MA" || feature.TypeCode == "OC" ? feature.Name.ToUpper() : feature.Name;
                                    var size = System.Windows.Forms.TextRenderer.MeasureText(label, fontLabel, Size.Empty, System.Windows.Forms.TextFormatFlags.HorizontalCenter | System.Windows.Forms.TextFormatFlags.VerticalCenter);
                                    textRenderer.Value.DrawString(label, fontLabel, brush, new Vec2(pFeature.X - size.Width / 2, pFeature.Y + size.Height / 2));
                                }

                                GL.PopMatrix();
                            }
                        }
                    }
                }
            }
        }

        private void DrawRings(ISkyMap map, SphereParameters data, int sign)
        {
            int j;
            double x, y;

            // number or segments
            const int segments = 64;

            // radius of outer ring relative to Saturn equatorial radius
            const double ringsRatio = 2.320;

            var prj = map.SkyProjection;

            GL.Disable(EnableCap.Lighting);
            GL.BindTexture(TextureTarget.Texture2D, textureManager.GetTexture(Path.Combine(dataPath, "Rings.png")));

            if (settings.Get<ColorSchema>("Schema") == ColorSchema.Red)
            {
                GL.Color3(Color.DarkRed);
            }
            else
            {
                GL.Color3(Color.White);
            }

            GL.Begin(PrimitiveType.TriangleFan);

            GL.TexCoord2(1, 0);
            GL.Vertex3(0, 0, 0);

            // radius of outer ring, in pixels
            double r = prj.GetDiskSize(data.Semidiameter, 10) / 2 * ringsRatio;

            // rotation of axis
            double rotAxis = Angle.ToRadians(data.RotationAxis);

            // rotation matrix for rings vision
            var matRings = Mat4.ZRotation(rotAxis) * Mat4.XRotation(Angle.ToRadians(data.LatitudeShift - 90));

            for (j = 0; j <= segments / 2; j++)
            {
                double ang = j / (double)segments * 2 * Math.PI - sign * Math.Sign(data.LatitudeShift) * Math.PI / 2 * (prj.FlipVertical ? 1 : -1);
                x = -Math.Sin(ang);
                y = -Math.Sign(data.LatitudeShift) * Math.Cos(ang);
                Vec3 vecVision = matRings * new Vec3(x, y, 0);
                GL.TexCoord2(0, 0);
                GL.Vertex3(r * vecVision.X, r * vecVision.Y, 0);
            }
            GL.End();

            GL.Enable(EnableCap.Lighting);
        }

        private void DrawPlanetSphere(ISkyMap map, CelestialObject body, SphereParameters data)
        {
            var prj = map.SkyProjection;

            var p = prj.Project(data.Equatorial);

            GL.PushMatrix();
            GL.Translate(p.X, p.Y, 0);

            double x, y, z;
            double s, t;
            int i, j;

            // radius of sphere, in pixels
            double radius = prj.GetDiskSize(data.Semidiameter, data.MinimalSize) / 2;

            int segments = radius < 20 ? 16 : 64;
            double drho = Math.PI / segments;
            double dtheta = 2.0 * Math.PI / segments;

            double delta = 1.0 / segments;

            // rotation matrix to proper orient sphere 
            var matVision = Mat4.XRotation(-Math.PI / 2 + Angle.ToRadians((prj.FlipVertical ? 1 : -1) * data.LatitudeShift)) * Mat4.ZRotation(Math.PI + Angle.ToRadians(-data.LongitudeShift) * (prj.FlipHorizontal ? -1 : 1));

            // illumination matrix (phase)
            var matLight = Mat4.YRotation(Angle.ToRadians(data.PhaseAngle) * (prj.FlipHorizontal ? -1 : 1)) * matVision;

            // rotation of axis
            double rotAxis = Angle.ToRadians(data.RotationAxis);

            // rotation of phase
            double rotPhase = Angle.ToRadians(data.RotationPhase);

            matVision = Mat4.ZRotation(rotAxis) * matVision;
            matLight = Mat4.ZRotation(rotPhase) * matLight;

            float shadowSmoothness = data.SmoothShadow ? 1 : 5;

            GL.Enable(EnableCap.Texture2D);

            Vec3 vecVision;
            Vec3 vecLight;

            if (data.DrawRings)
            {
                DrawRings(map, data, 1);
            }

            const int LAYER_PLANET = 0;
            const int LAYER_POLAR_CAP = 1;
            int layers = data.DrawPolarCap ? 2 : 1;

            double cap1 = 0;
            double cap2 = 0;

            for (int layer = 0; layer < layers; layer++)
            {
                t = 1;

                // polar cap limits, in fractions of planet diameter
                if (layer == LAYER_POLAR_CAP)
                {
                    cap1 = prj.FlipVertical ? 1 - data.NorthernPolarCap / 180 : 1 - data.SouthernPolarCap / 180;
                    cap2 = prj.FlipVertical ? data.SouthernPolarCap / 180 : data.NorthernPolarCap / 180;
                }

                if (layer == LAYER_PLANET)
                {
                    GL.BindTexture(TextureTarget.Texture2D, textureManager.GetTexture(data.TextureName, data.FallbackTextureName));
                }
                else if (layer == LAYER_POLAR_CAP)
                {
                    GL.BindTexture(TextureTarget.Texture2D, textureManager.GetTexture(Path.Combine(dataPath, "PolarCap.png")));
                }

                for (i = 0; i < segments; i++)
                {
                    GL.Begin(PrimitiveType.QuadStrip);
                    s = 0;

                    for (j = 0; j <= segments; j++)
                    {
                        x = -Math.Sin(j * dtheta) * Math.Sin(i * drho);
                        y = Math.Cos(j * dtheta) * Math.Sin(i * drho);
                        z = Math.Cos(i * drho) * (1 - data.Flattening);

                        vecVision = matVision * new Vec3(x, y, z);
                        vecLight = matLight * new Vec3(-x, -y, -z);
                        GL.Normal3(vecLight.X * shadowSmoothness, vecLight.Y * shadowSmoothness, vecLight.Z * shadowSmoothness);

                        if (layer == LAYER_PLANET)
                        {
                            GL.TexCoord2(-s * (prj.FlipHorizontal ? -1 : 1), t * (prj.FlipVertical ? -1 : 1));
                        }
                        else if (layer == LAYER_POLAR_CAP)
                        {
                            if (t > cap1 || t < cap2)
                            {
                                GL.TexCoord2(0, 1);
                            }
                            else
                            {
                                GL.TexCoord2(0, 0);
                            }
                        }

                        GL.Vertex3(vecVision.X * radius, vecVision.Y * radius, 0);

                        x = -Math.Sin(j * dtheta) * Math.Sin((i + 1) * drho);
                        y = Math.Cos(j * dtheta) * Math.Sin((i + 1) * drho);
                        z = Math.Cos((i + 1) * drho) * (1 - data.Flattening);

                        vecVision = matVision * new Vec3(x, y, z);
                        vecLight = matLight * new Vec3(-x, -y, -z);
                        GL.Normal3(vecLight.X * shadowSmoothness, vecLight.Y * shadowSmoothness, vecLight.Z * shadowSmoothness);

                        if (layer == LAYER_PLANET)
                        {
                            GL.TexCoord2(-s * (prj.FlipHorizontal ? -1 : 1), (t - delta) * (prj.FlipVertical ? -1 : 1));
                        }
                        else if (layer == LAYER_POLAR_CAP)
                        {
                            if (t - delta > cap1 || t - delta < cap2)
                            {
                                GL.TexCoord2(0, 1);
                            }
                            else
                            {
                                GL.TexCoord2(0, 0);
                            }
                        }

                        GL.Vertex3(vecVision.X * radius, vecVision.Y * radius, 0);

                        s += delta;
                    }
                    GL.End();

                    t -= delta;
                }
            }

            if (data.DrawRings)
            {
                DrawRings(map, data, -1);
            }

            GL.PopMatrix();

            GL.Disable(EnableCap.Texture2D);

            map.AddDrawnObject(p, body);
        }

        private Color GradientColor(Color color1, Color color2, double percent)
        {
            double r = color1.R + percent * (color2.R - color1.R);
            double g = color1.G + percent * (color2.G - color1.G);
            double b = color1.B + percent * (color2.B - color1.B);
            double a = color1.A + percent * (color2.A - color1.A);
            return Color.FromArgb((byte)a, (byte)r, (byte)g, (byte)b);
        }

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
                    if (RenderPlanetMoon(map, planetsCalc.Planets.ElementAt(Planet.JUPITER - 1), jm))
                    {
                        RenderJupiterMoonShadow(map, jm, jm.RectangularS);
                        RenderJupiterShadow(map, jm);
                    }
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
                    if (gm.Data.planet == Planet.PLUTO)
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

            if (map.Schema == ColorSchema.Day && settings.Get("Sun"))
            {
                DrawHalo(map);
            }

            //RenderEarthShadow(map);
            DrawLunarSurfaceFeatures(map);
        }

        public override bool OnMouseMove(CrdsHorizontal mouse, MouseButton mouseButton)
        {
            

            return mouseButton == MouseButton.None &&
                (Angle.Separation(mouse, moon.Horizontal) < moon.Semidiameter / 3600 ||
                 Angle.Separation(mouse, mars.Horizontal) < mars.Semidiameter / 3600);
        }

        public override bool OnMouseMove(ISkyMap map, PointF mouse, MouseButton mouseButton)
        {
            // TODO: use CelestialObject.Equatorial for this

            if (mouseButton != MouseButton.None) return false;
            
            {
                var p = map.SkyProjection.Project(moon.Equatorial);
                double r = map.SkyProjection.GetDiskSize(moon.Semidiameter) / 2;
                bool needDraw = (mouse.X - p.X) * (mouse.X - p.X) + (mouse.Y - p.Y) * (mouse.Y - p.Y) < r * r;
                if (needDraw) return true;
            }

            {
                var p = map.SkyProjection.Project(mars.Equatorial);
                double r = map.SkyProjection.GetDiskSize(mars.Semidiameter) / 2;
                bool needDraw = (mouse.X - p.X) * (mouse.X - p.X) + (mouse.Y - p.Y) * (mouse.Y - p.Y) < r * r;
                if (needDraw) return true;
            }

            return false;
        }

        private void RenderSun(IMapContext map)
        {
            if (!settings.Get("Sun")) return;

            bool isGround = settings.Get("Ground");
            bool useTextures = settings.Get("SunTexture");
            double ad = Angle.Separation(sun.Horizontal, map.Center);
            Graphics g = map.Graphics;
            Color colorSun = map.GetColor(clrSunNight, clrSunDaylight);

            if ((!isGround || sun.Horizontal.Altitude + sun.Semidiameter / 3600 > 0) &&
                ad < map.ViewAngle + sun.Semidiameter / 3600)
            {
                PointF p = map.Project(sun.Horizontal);
                map.Rotate(p, sun.Equatorial, 0);
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
                        map.DrawImage(imageSun, -size / 2, -size / 2, size, size);
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
                    var fontLabel = settings.Get<Font>("SolarSystemLabelsFont");
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

        //private PointF pMoon;
        //private double sizeMoon;

        private void RenderMoon(IMapContext map)
        {
            if (!settings.Get("Moon")) return;

            bool isGround = settings.Get("Ground");
            double ad = Angle.Separation(moon.Horizontal, map.Center);

            if ((!isGround || moon.Horizontal.Altitude + moon.Semidiameter / 3600 > 0) &&
                ad < map.ViewAngle + moon.Semidiameter / 3600.0)
            {
                // drawing size
                float size = map.GetDiskSize(moon.Semidiameter, 10);

                Graphics g = map.Graphics;
                bool useTextures = settings.Get("MoonTexture");
                int q = Math.Min((int)settings.Get<TextureQuality>("MoonTextureQuality"), size < 256 ? 2 : (size < 1024 ? 4 : 8));
                string textureName = $"Moon-{q}k";

                PointF p = map.Project(moon.Horizontal);

                map.Rotate(p, moon.Equatorial, moon.PAaxis);

                SolidBrush brushMoon = new SolidBrush(map.GetColor(Color.Gray));

                if (useTextures && size > 10)
                {
                    Image textureMoon = imagesCache.RequestImage("Moon", new PlanetTextureToken(textureName, moon.Libration.l, moon.Libration.b, map.Schema), MoonTextureProvider, map.Redraw);
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
                GraphicsPath shadow = GetPhaseShadow(phase, size + 1);

                map.Rotate(p, moon.Ecliptical0);
                map.Flip();

                if (settings.Get("MoonPhase"))
                {
                    g.FillPath(GetShadowBrush(map), shadow);
                }
                else
                {
                    g.DrawPath(new Pen(GetShadowBrush(map), 1) { DashStyle = DashStyle.Custom, DashPattern = new float[] { 10, 5 } }, shadow);
                }

                g.ResetTransform();

                if (settings.Get("MoonLabel"))
                {
                    var fontLabel = settings.Get<Font>("SolarSystemLabelsFont");
                    map.DrawObjectCaption(fontLabel, brushLabel, moon.Name, p, size);
                }

                map.AddDrawnObject(moon);
            }
        }

        private void DrawLunarSurfaceFeatures(IMapContext map)
        {
            if (settings.Get("Moon") && settings.Get("MoonTexture") && settings.Get("MoonSurfaceFeatures"))
            {
                var p = map.Project(moon.Horizontal);
                var rotation = map.Rotate(p, moon.Equatorial, moon.PAaxis);
                DrawSurfaceFeatures(map, lunarFeatures, moon, 3474, rotation, moon.Libration.b, moon.Libration.l, map.GetColor(Color.AntiqueWhite));
            }
        }

        private void DrawMartianSurfaceFeatures(IMapContext map)
        {
            if (settings.Get("Planets") && settings.Get("PlanetsTextures") && settings.Get("PlanetsSurfaceFeatures"))
            {
                var p = map.Project(mars.Horizontal);
                var rotation = map.Rotate(p, mars.Equatorial, mars.Appearance.P);
                DrawSurfaceFeatures(map, martianFeatures, mars, 6779, rotation, mars.Appearance.D, -mars.Appearance.CM, map.GetColor(Color.Wheat));
            }
        }

        private void DrawSurfaceFeatures(IMapContext map, ICollection<SurfaceFeature> features, SizeableCelestialObject body, float bodyDiameter, float axisRotation, double latitudeShift, double longitudeShift, Color featuresColor)
        {
            map.Graphics.ResetTransform();

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
                        CrdsGeographical v = GetVisibleFeatureCoordinates((map.IsInverted ? -1 : 1) * feature.Latitude, (map.IsMirrored ? -1 : 1) * feature.Longitude, (map.IsInverted ? -1 : 1) * latitudeShift, (map.IsMirrored ? -1 : 1) * longitudeShift);

                        // angular separation between visible center of the body disk and center of the feature
                        // expressed in degrees of arc, from 0 (center) to 90 (disk edge)
                        double sep = Angle.Separation(v, c);
                        if (sep < 85)
                        {
                            PointF pFeature = GetCartesianFeatureCoordinates(r, v, axisRotation);

                            // do not draw if feature is out of screen
                            if (!map.IsOutOfScreen(new PointF(pFeature.X + p.X, pFeature.Y + p.Y)))
                            {
                                // distance, in pixels, between center of the feature and current mouse position
                                double d = Math.Sqrt(Math.Pow(pMouse.X - pFeature.X - p.X, 2) + Math.Pow(pMouse.Y - pFeature.Y - p.Y, 2));

                                // visible flattening of feature outline,
                                // depends on angular distance between feature and visible center of the body disk
                                float f = (float)Math.Cos(Angle.ToRadians(sep));

                                bool needDrawLabel = true;

                                if (fr > 100 || d < fr)
                                {
                                    float labelDist = 3;
                                    StringFormat format = null;

                                    // draw feature outline (for craters and satellite features only)
                                    if (feature.TypeCode == "AA" || feature.TypeCode == "SF")
                                    {
                                        g.TranslateTransform(p.X + pFeature.X, p.Y + pFeature.Y);
                                        g.RotateTransform(90 + (float)Angle.ToDegrees(Math.Atan2(pFeature.Y, pFeature.X)));

                                        using (GraphicsPath gp = new GraphicsPath())
                                        {
                                            gp.AddEllipse(-fr, -fr * f, fr * 2, fr * f * 2);
                                            gp.Transform(g.Transform);

                                            if (gp.IsVisible(pMouse))
                                            {
                                                g.DrawEllipse(pen, -fr, -fr * f, fr * 2, fr * f * 2);
                                            }
                                            else
                                            {
                                                needDrawLabel = false;
                                            }
                                        }

                                        g.ResetTransform();
                                        labelDist = fr * 2 * f;
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
                                    else if (feature.TypeCode != "AA" && feature.TypeCode != "SF")
                                    {
                                        g.TranslateTransform(p.X + pFeature.X, p.Y + pFeature.Y);
                                        g.FillEllipse(brush, -1, -1, 3, 3);
                                        g.ResetTransform();
                                    }

                                    g.ResetTransform();

                                    // draw feature label
                                    if (needDrawLabel)
                                    {
                                        var fontLabel = settings.Get<Font>("SolarSystemLabelsFont");
                                        map.DrawObjectCaption(fontLabel, brush, feature.Name, new PointF(p.X + pFeature.X, p.Y + pFeature.Y), labelDist, format);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private Vec2 GetCartesianFeatureCoordinates(Projection prj, float r, CrdsGeographical c, double axisRotation)
        {
            // rotation of axis
            axisRotation = Angle.ToRadians(axisRotation);

            // convert to orthographic polar coordinates 
            double Y = r * Math.Sin(Angle.ToRadians(c.Latitude));
            double X = r * Math.Cos(Angle.ToRadians(c.Latitude)) * Math.Sin(Angle.ToRadians(c.Longitude));

            Y = -Y * (prj.FlipVertical ? -1 : 1);
            X = X * (prj.FlipHorizontal ? -1 : 1);

            // polar coordinates rotated around of visible center of the body disk
            double X_ = X * Math.Cos(axisRotation) - Y * Math.Sin(axisRotation);
            double Y_ = X * Math.Sin(axisRotation) + Y * Math.Cos(axisRotation);

            return new Vec2(X_, Y_);
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
            double aZ = Angle.ToRadians(-longitudeShift);
            double[,] mZ = new double[3, 3] { { Math.Cos(aZ), -Math.Sin(aZ), 0 }, { Math.Sin(aZ), Math.Cos(aZ), 0 }, { 0, 0, 1 } };
            Rotate(v, mZ);

            // rotate around Y axis (latitude / theta)
            double aY = Angle.ToRadians(latitudeShift);
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

        private void RenderPlanet<TPlanet>(IMapContext map, TPlanet planet) where TPlanet : SizeableCelestialObject, IPlanet
        {
            if (!settings.Get("Planets")) return;

            Graphics g = map.Graphics;
            double ad = Angle.Separation(planet.Horizontal, map.Center);
            bool isGround = settings.Get("Ground");
            bool useTextures = settings.Get("PlanetsTextures");
            bool drawAll = settings.Get("PlanetsDrawAll");
            bool drawLabelMag = settings.Get("PlanetsLabelsMag");

            if ((!isGround || planet.Horizontal.Altitude + planet.Semidiameter / 3600 > 0) &&
                ad < map.ViewAngle + planet.Semidiameter / 3600)
            {
                float size = map.GetPointSize(planet.Magnitude, maxDrawingSize: 7);
                float diam = map.GetDiskSize(planet.Semidiameter);

                // if "draw all" setting is enabled, draw planets anyway
                if (drawAll && size < 1)
                {
                    size = 1;
                }

                string label = drawLabelMag ? $"{planet.Name} {Formatters.Magnitude.Format(planet.Magnitude)}" : planet.Name;

                // diameter is to small to render as planet disk, 
                // but point size caclulated from magnitude is enough to be drawn
                if (size > diam && (int)size > 0)
                {
                    PointF p = map.Project(planet.Horizontal);
                    g.FillEllipse(GetPlanetColor(map, planet.Number), p.X - size / 2, p.Y - size / 2, size, size);
                    if (settings.Get("PlanetsLabels"))
                    {
                        var fontLabel = settings.Get<Font>("SolarSystemLabelsFont");
                        map.DrawObjectCaption(fontLabel, brushLabel, label, p, size);
                    }
                    map.AddDrawnObject(planet);
                }

                // planet should be rendered as disk
                else if (diam >= size && (int)diam > 0)
                {
                    PointF p = map.Project(planet.Horizontal);
                    map.Rotate(p, planet.Equatorial, (float)planet.Appearance.P);
                    DrawRotationAxis(map, diam);

                    if (planet.Number == Planet.SATURN)
                    {
                        var rings = planetsCalc.SaturnRings;

                        double r = Math.Sqrt(map.Width * map.Width + map.Height * map.Height);

                        // scale value to convert visible size of ring to screen pixels
                        double scale = 1.0 / 3600 / map.ViewAngle * r / 4;

                        // draw rings by halfs arcs, first half is farther one
                        for (int half = 0; half < 2; half++)
                        {
                            // draw planets textures
                            if (useTextures)
                            {
                                float a = (float)(rings.GetRingSize(0, RingEdge.Outer, RingAxis.Major) * scale);
                                float b = (float)(rings.GetRingSize(0, RingEdge.Outer, RingAxis.Minor) * scale);

                                Image textureRings = imagesCache.RequestImage("Rings", map.Schema, RingsTextureProvider, map.Redraw);
                                if (textureRings != null)
                                {
                                    // half of source image: 0 = top, 1 = bottom
                                    int h = (half + (rings.B > 0 ? 0 : 1)) % 2;

                                    map.DrawImage(textureRings,
                                        // destination rectangle
                                        new RectangleF(-a, -b + h * b, a * 2, b),
                                        // source rectangle
                                        new RectangleF(0, h * textureRings.Height / 2f, textureRings.Width, textureRings.Height / 2f));
                                }
                                else
                                {
                                    int h = (half + (rings.B * (map.IsInverted ? -1 : 1) > 0 ? 0 : 1)) % 2;
                                    DrawRingsUntextured(map.Graphics, rings, h, scale);
                                }
                            }
                            // do not use textures
                            else
                            {
                                int h = (half + (rings.B * (map.IsInverted ? -1 : 1) > 0 ? 0 : 1)) % 2;
                                DrawRingsUntextured(map.Graphics, rings, h, scale);
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
                        map.Rotate(p, planet.Ecliptical);
                        map.Flip();
                        g.FillPath(GetShadowBrush(map), shadow);
                        g.ResetTransform();
                    }

                    if (settings.Get("PlanetsLabels"))
                    {
                        var fontLabel = settings.Get<Font>("SolarSystemLabelsFont");
                        map.DrawObjectCaption(fontLabel, brushLabel, planet.Name, p, diam);
                    }
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

        private bool RenderPlanetMoon<TPlanet, TPlanetMoon>(IMapContext map, TPlanet planet, TPlanetMoon moon, bool hasTexture = true) where TPlanet : SizeableCelestialObject, IPlanet where TPlanetMoon : SizeableCelestialObject, IPlanetMoon
        {
            if (!settings.Get("Planets")) return false;
            if (!settings.Get("PlanetMoons")) return false;

            bool isDrawn = false;
            bool isGround = settings.Get("Ground");
            bool useTextures = settings.Get("PlanetsTextures");
            double ad = Angle.Separation(moon.Horizontal, map.Center);
            Graphics g = map.Graphics;

            if ((!isGround || moon.Horizontal.Altitude + moon.Semidiameter / 3600 > 0) &&
                ad < map.ViewAngle + moon.Semidiameter / 3600)
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

                            var fontLabel = settings.Get<Font>("SolarSystemLabelsFont");
                            map.DrawObjectCaption(fontLabel, brushLabel, moon.Name, p, 2);
                            map.AddDrawnObject(moon);
                            isDrawn = true;
                        }
                    }
                }
                // moon should be rendered as disk
                else if (diam >= size && (int)diam > 0)
                {
                    map.Rotate(p, planet.Equatorial, (float)planet.Appearance.P);
                    if (hasTexture && useTextures)
                    {
                        Image texture = imagesCache.RequestImage($"{planet.Number}-{moon.Number}", new PlanetTextureToken($"{planet.Number}-{moon.Number}", moon.CM, planet.Appearance.D, map.Schema), PlanetTextureProvider, map.Redraw);
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

                    var fontLabel = settings.Get<Font>("SolarSystemLabelsFont");
                    map.DrawObjectCaption(fontLabel, brushLabel, moon.Name, p, diam);
                    map.AddDrawnObject(moon);
                    isDrawn = true;
                }
            }

            return isDrawn;
        }

        private void RenderJupiterShadow(ISkyMap map, JupiterMoon moon)
        {
            if (!moon.IsEclipsedByPlanet) return;
            if (!settings.Get("Planets")) return;
            if (!settings.Get("PlanetMoons")) return;

            var prj = map.SkyProjection;
            Planet jupiter = planetsCalc.Planets.ElementAt(Planet.JUPITER - 1);

            // Jupiter radius, in pixels, and also radius of its shadow
            float sd = prj.GetDiskSize(jupiter.Semidiameter) / 2;

            // Center of shadow, relative to Jupiter center
            Vec2 pShadow = new Vec2(-moon.RectangularS.X * sd * (prj.FlipHorizontal ? -1 : 1), -moon.RectangularS.Y * sd * (prj.FlipVertical ? 1 : -1));

            // rotation angle of shadow center respect to center of Jupiter
            double rot = prj.GetAxisRotation(jupiter.Equatorial, jupiter.Appearance.P);

            // Center of Moon
            var pMoon = prj.Project(moon.Horizontal); // TODO: change to equatorial

            // Center of Jupiter shadow
            pShadow = Mat4.ZRotation(Angle.ToRadians(rot)) * pShadow + pMoon;

            // Radius of eclipsing moon
            float sdMoon = prj.GetDiskSize(moon.Semidiameter) / 2;

            RenderEclipseShadow(pMoon, pShadow, sdMoon + 1, new double[] { 0, sd }, new Color[] { Color.Black, Color.Black }, rot, jupiter.Flattening);

            // Jupiter shadow outline
            Primitives.DrawEllipse(pShadow, Pens.Red, sd, sd * (1 - jupiter.Flattening), rot);
        }

        private void RenderJupiterShadow(IMapContext map, JupiterMoon moon)
        {
            if (!moon.IsEclipsedByPlanet) return;
            if (!settings.Get("Planets")) return;
            if (!settings.Get("PlanetMoons")) return;

            Planet jupiter = planetsCalc.Planets.ElementAt(Planet.JUPITER - 1);

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
            PointF p = new PointF(-(float)moon.RectangularS.X * sd * (map.IsMirrored ? -1 : 1), (float)moon.RectangularS.Y * sd * (map.IsInverted ? -1 : 1));

            Graphics g = map.Graphics;

            map.Rotate(pMoon, jupiter.Equatorial, (float)jupiter.Appearance.P);

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

        private void RenderEclipseShadow(Vec2 pBody, Vec2 pShadow, float radiusBody, double[] shadowRadii, Color[] shadowColors, double rotAngle = 0, double flattening = 0)
        {
            GL.PushMatrix();
            GL.Translate(pBody.X, pBody.Y, 0);

            // initiate stencil
            GL.Enable(EnableCap.StencilTest);
            GL.Clear(ClearBufferMask.StencilBufferBit);
            GL.StencilMask(0xFF);
            GL.ColorMask(false, false, false, false);
            GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
            GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);

            // draw stencil pattern (body outline)

            GL.Begin(PrimitiveType.TriangleFan);

            for (int i = 0; i <= 64; i++)
            {
                double ang = i / 64.0 * 2 * Math.PI;
                Vec2 v = new Vec2(radiusBody * Math.Cos(ang), radiusBody * Math.Sin(ang));
                GL.Vertex2(v.X, v.Y);
            }

            GL.End();
            GL.PopMatrix();

            // draw shadow

            GL.StencilFunc(StencilFunction.Equal, 1, 0xFF);
            GL.StencilOp(StencilOp.Keep, StencilOp.Replace, StencilOp.Replace);
            GL.ColorMask(true, true, true, true);

            GL.PushMatrix();
            GL.Translate(pShadow.X, pShadow.Y, 0);

            // enable blending because shadow is semitransparent
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // rotation angle of the shadow
            double rot = Angle.ToRadians(rotAngle);
            double cosRot = Math.Cos(rot);
            double sinRot = Math.Sin(rot);

            for (int i = 0; i < shadowRadii.Length; i++)
            {
                GL.Begin(PrimitiveType.TriangleStrip);

                for (int j = 0; j <= 63; j++)
                {
                    double t = j / (double)63 * (2 * Math.PI);
                    double cost = Math.Cos(t);
                    double sint = Math.Sin(t);

                    // outer
                    {
                        double rx = shadowRadii[i];
                        double ry = rx * (1 - flattening);

                        GL.Color4(shadowColors[i]);

                        double x = rx * cost * cosRot - ry * sint * sinRot;
                        double y = ry * sint * cosRot + rx * cost * sinRot;

                        GL.Vertex2(x, y);
                    }

                    // inner
                    if (i > 0)
                    {
                        double rx = shadowRadii[i - 1];
                        double ry = rx * (1 - flattening);

                        GL.Color4(shadowColors[i - 1]);

                        double x = rx * cost * cosRot - ry * sint * sinRot;
                        double y = ry * sint * cosRot + rx * cost * sinRot;

                        GL.Vertex2(x, y);
                    }
                    else
                    {
                        GL.Color4(shadowColors[0]);
                        GL.Vertex2(0, 0);
                    }
                }

                GL.End();
            }

            // disable stencil

            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.StencilTest);

            GL.PopMatrix();
        }

        private void RenderJupiterMoonShadow(ISkyMap map, SizeableCelestialObject eclipsedBody, CrdsRectangular rect = null)
        {
            if (!settings.Get("Planets")) return;
            if (!settings.Get("PlanetMoons")) return;

            Projection prj = map.SkyProjection;

            if (rect == null)
            {
                rect = new CrdsRectangular();
            }

            // collect moons than can produce a shadow
            var ecliptingMoons = planetsCalc.JupiterMoons.Where(m => m.RectangularS.Z < rect.Z);

            if (ecliptingMoons.Any())
            {
                Planet jupiter = planetsCalc.Planets.ElementAt(Planet.JUPITER - 1);

                // Jupiter radius, in pixels
                float sd = prj.GetDiskSize(jupiter.Semidiameter) / 2;

                // Center of eclipsed body
                Vec2 pBody = prj.Project(eclipsedBody.Horizontal); // TODO: change to Equatorial

                // elipsed body size, in pixels
                float radiusBody = prj.GetDiskSize(eclipsedBody.Semidiameter) / 2;

                foreach (var moon in ecliptingMoons)
                {
                    // umbra and penumbra radii, in acrseconds
                    var shadow = GalileanMoons.Shadow(jupiter.Ecliptical.Distance, jupiter.DistanceFromSun, moon.Number - 1, moon.RectangularS, rect);

                    // umbra and penumbra size, in pixels
                    float radiusUmbra = prj.GetDiskSize(shadow.Umbra) / 2;
                    float radiusPenumbra = prj.GetDiskSize(shadow.Penumbra) / 2;

                    // coordinates of shadow relative to eclipsed body
                    CrdsRectangular shadowRelative = moon.RectangularS - rect;

                    // Center of shadow, relative to Jupiter center
                    Vec2 p = new Vec2(shadowRelative.X * sd * (prj.FlipHorizontal ? -1 : 1), shadowRelative.Y * sd * (prj.FlipVertical ? 1 : -1));

                    // rotation angle of shadow center respect to center of Jupiter
                    double rot = prj.GetAxisRotation(jupiter.Equatorial, jupiter.Appearance.P);

                    // Center of shadow
                    p = Mat4.ZRotation(Angle.ToRadians(rot)) * p + pBody;

                    // shadow has enough size to be rendered
                    if ((int)radiusPenumbra > 0)
                    {
                        double[] shadowRadii = new double[] { 0, radiusUmbra * 0.99, radiusUmbra, radiusPenumbra * 1.01, radiusPenumbra };
                        Color[] shadowColors = new Color[] { Color.FromArgb(250, 0, 0, 0), Color.FromArgb(250, 0, 0, 0), Color.FromArgb(150, 0, 0, 0), Color.FromArgb(0, 0, 0, 0), Color.FromArgb(0, 0, 0, 0) };

                        RenderEclipseShadow(pBody, p, radiusBody, shadowRadii, shadowColors);
                    }
                }
            }
        }

        private void RenderJupiterMoonShadow(IMapContext map, SizeableCelestialObject eclipsedBody, CrdsRectangular rect = null)
        {
            if (!settings.Get("Planets")) return;
            if (!settings.Get("PlanetMoons")) return;

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
                    PointF p = new PointF((map.IsMirrored ? -1 : 1) * (float)shadowRelative.X * sd, (map.IsInverted ? -1 : 1) * -(float)shadowRelative.Y * sd);

                    map.Rotate(pBody, jupiter.Equatorial, (float)jupiter.Appearance.P);

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

        [Obsolete]
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
                    token = new PlanetTextureToken(planet.Number.ToString(), -planet.Appearance.CM, planet.Appearance.D, map.Schema, planetsCalc.MarsNPCWidth, planetsCalc.MarsSPCWidth);
                }
                else if (planet.Number == Planet.JUPITER)
                {
                    token = new PlanetTextureToken(planet.Number.ToString(), planetsCalc.GreatRedSpotLongitude - planet.Appearance.CM, planet.Appearance.D, map.Schema);
                }
                else
                {
                    token = new PlanetTextureToken(planet.Number.ToString(), -planet.Appearance.CM, planet.Appearance.D, map.Schema);
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

        private void DrawRotationAxis(IMapContext map, float diam)
        {
            if (settings.Get("ShowRotationAxis"))
            {
                var p1 = new PointF(0, -(diam / 2 + 10));
                var p2 = new PointF(0, diam / 2 + 10);
                map.Graphics.DrawLine(new Pen(map.GetColor("ColorSolarSystemLabel")), p1, p2);
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
                TextureFilePath = Path.Combine(dataPath, $"{token.TextureName}.jpg"),
                ColorSchema = token.ColorSchema
            });
        }

        private Image RingsTextureProvider(ColorSchema schema)
        {
            Image image = Image.FromFile(Path.Combine(dataPath, "Rings.png"), true);
            image.Colorize(schema);
            return image;
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
                TextureFilePath = Path.Combine(dataPath, $"{token.TextureName}.jpg"),
                ColorSchema = token.ColorSchema
            });
        }

        private Image SunImageProvider(DateTime date)
        {
            return null;
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

        private Color GetPlanetColor(int planet)
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

            return color;
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

            /// <summary>
            /// Color schema
            /// </summary>
            public ColorSchema ColorSchema { get; private set; }

            public PlanetTextureToken(string name, double longitude, double latitude, ColorSchema colorSchema)
            {
                TextureName = name;
                Longitude = longitude;
                Latitude = latitude;
                RenderPolarCaps = false;
                NorthernPolarCap = 0;
                SouthernPolarCap = 0;
                ColorSchema = colorSchema;
            }

            public PlanetTextureToken(string name, double longitude, double latitude, ColorSchema colorSchema, double northernPolarCap, double southernPolarCap)
            {
                TextureName = name;
                Longitude = longitude;
                Latitude = latitude;
                RenderPolarCaps = true;
                NorthernPolarCap = northernPolarCap;
                SouthernPolarCap = southernPolarCap;
                ColorSchema = colorSchema;
            }

            public override bool Equals(object obj)
            {
                if (obj is PlanetTextureToken other)
                {
                    return
                        TextureName.Equals(other.TextureName) &&
                        ColorSchema == other.ColorSchema &&
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
