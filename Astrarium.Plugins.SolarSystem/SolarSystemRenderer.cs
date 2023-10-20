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
        private readonly SolarTextureManager solarTextureManager;
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
                else if (settings.Get("GenericMoons") && body is GenericMoon gm)
                {
                    float size = prj.GetPointSize(gm.Magnitude, 2f);
                    if (size < 1) continue;

                    // draw as point
                    var p = prj.Project(gm.Equatorial);
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
                            textRenderer.Value.DrawString(gm.Name, fontLabel, brushLabel, p + (0.35f * size));
                        }

                        map.AddDrawnObject(p, gm);
                    }
                }
                else if (body is Pluto pluto)
                {
                    float size = prj.GetPointSize(pluto.Magnitude, maxDrawingSize: 7);
                    double diam = prj.GetDiskSize(pluto.Semidiameter);

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
                        var p = prj.Project(pluto.Equatorial);
                        if (prj.IsInsideScreen(p))
                        {
                            GL.Enable(EnableCap.PointSmooth);
                            GL.Enable(EnableCap.Blend);
                            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                            GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest);

                            GL.PointSize(size);
                            GL.Begin(PrimitiveType.Points);
                            GL.Color3(GetPlanetColor(pluto.Number));
                            GL.Vertex2(p.X, p.Y);
                            GL.End();

                            map.AddDrawnObject(p, pluto);

                            if (settings.Get("PlanetsLabels"))
                            {
                                string label = drawLabelMag ? $"{pluto.Name} {Formatters.Magnitude.Format(pluto.Magnitude)}" : pluto.Name;
                                var fontLabel = settings.Get<Font>("SolarSystemLabelsFont");
                                textRenderer.Value.DrawString(label, fontLabel, brushLabel, p);
                            }
                        }
                    }
                    // draw as sphere
                    else
                    {
                        double rotAxis = prj.GetAxisRotation(pluto.Equatorial, pluto.Appearance.P);
                        double rotPhase = prj.GetPhaseRotation(pluto.Ecliptical);

                        DrawPlanet(map, pluto, new SphereParameters()
                        {
                            Equatorial = pluto.Equatorial,
                            TextureName = Path.Combine(dataPath, $"{pluto.Number}.jpg"),
                            Semidiameter = pluto.Semidiameter,
                            PhaseAngle = 0,
                            Flattening = pluto.Flattening,
                            LatitudeShift = -pluto.Appearance.D,
                            LongitudeShift = pluto.Appearance.CM - (pluto.Number == Planet.JUPITER ? planetsCalc.GreatRedSpotLongitude : 0),
                            RotationAxis = rotAxis,
                            RotationPhase = rotPhase,
                            BodyPhysicalDiameter = 2 * Planet.EQUATORIAL_RADIUS[pluto.Number - 1],                            
                            SmoothShadow = false,
                            DrawLabel = settings.Get("PlanetsLabels")
                        });
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
            /// <summary>
            /// Equatorial coordinates of the body
            /// </summary>
            public CrdsEquatorial Equatorial { get; set; }

            /// <summary>
            /// Texture name (path) to be used
            /// </summary>
            public string TextureName { get; set; }

            /// <summary>
            /// Fallback texture, if any. Can be null.
            /// </summary>
            public string FallbackTextureName { get; set; }

            public bool SmoothShadow { get; set; }

            public float MinimalSize { get; set; }

            /// <summary>
            /// Collection of surface features of the body
            /// </summary>
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

            // color of unilluminated part 
            float[] ambient;

            // color of illuminated part
            float[] diffuse;

            if (settings.Get<ColorSchema>("Schema") == ColorSchema.Red)
            {
                diffuse = new float[4] { 0.5f, 0, 0, 1f };
                ambient = new float[4] { 0.5f, 0, 0, 0.5f };
            }
            else
            {
                diffuse = new float[4] { 1, 1, 1, 1 };
                ambient = new float[4] { 0.25f, 0.25f, 0.25f, 1f };
            }

            GL.Light(LightName.Light0, LightParameter.Ambient, new float[4] { 0, 0, 0, 1 });
            GL.Light(LightName.Light0, LightParameter.Diffuse, diffuse);
            GL.Light(LightName.Light0, LightParameter.Specular, zero);
            GL.Light(LightName.Light0, LightParameter.ConstantAttenuation, 1f);

            GL.Material(MaterialFace.Front, MaterialParameter.Ambient, ambient);
            GL.Material(MaterialFace.Front, MaterialParameter.Diffuse, diffuse);
            GL.Material(MaterialFace.Front, MaterialParameter.Emission, zero);
            GL.Material(MaterialFace.Front, MaterialParameter.Shininess, zero);
            GL.Material(MaterialFace.Front, MaterialParameter.Specular, zero);

            GL.Enable(EnableCap.Light0);
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.Texture2D);

            GL.CullFace(CullFaceMode.Front);

            Vec2 p = prj.Project(data.Equatorial);

            GL.PushMatrix();
            GL.Translate(p.X, p.Y, 0);

            double x, y, z;
            double s, t;
            int i, j;

            // radius of sphere, in pixels
            float radius = prj.GetDiskSize(data.Semidiameter, data.MinimalSize) / 2;

            // number of segments to build the sphere
            int segments = radius < 20 ? 16 : 64;

            // delta rho, step by latitude
            double drho = Math.PI / segments;

            // delta theta, step by longitude
            double dtheta = 2.0 * Math.PI / segments;

            // step by segments count
            double delta = 1.0 / segments;

            // rotation matrix to proper orient sphere 
            Mat4 matVision = Mat4.XRotation(-Math.PI / 2 + Angle.ToRadians((prj.FlipVertical ? 1 : -1) * data.LatitudeShift)) * Mat4.ZRotation(Math.PI + Angle.ToRadians(-data.LongitudeShift) * (prj.FlipHorizontal ? -1 : 1));

            // illumination matrix (phase)
            Mat4 matLight = Mat4.YRotation(Angle.ToRadians(data.PhaseAngle) * (prj.FlipHorizontal ? -1 : 1)) * matVision;

            // rotation of axis
            double rotAxis = Angle.ToRadians(data.RotationAxis);

            // rotation of phase
            double rotPhase = Angle.ToRadians(data.RotationPhase);

            matVision = Mat4.ZRotation(rotAxis) * matVision;
            matLight = Mat4.ZRotation(rotPhase) * matLight;

            float shadowSmoothness = data.SmoothShadow ? 1 : 5;

            bool drawCaps = body == mars && settings.Get("PlanetsMartianPolarCaps");
            bool drawRings = body is Planet saturn && saturn.Number == Planet.SATURN;

            Vec3 vecVision;
            Vec3 vecLight;

            if (drawRings)
            {
                DrawRings(map, data, 1);
            }

            const int LAYER_PLANET = 0;
            const int LAYER_POLAR_CAP = 1;
            int layers = drawCaps ? 2 : 1;

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
                    GL.BindTexture(TextureTarget.Texture2D, textureManager.GetTexture(Path.Combine(dataPath, "PolarCap.png"), fallbackPath: null, permanent: false, action: SetPolarCapTextureParameters));
                }

                GL.ShadeModel(ShadingModel.Smooth);

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

            if (drawRings)
            {
                DrawRings(map, data, -1);
            }

            GL.PopMatrix();

            map.AddDrawnObject(p, body);

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
                textRenderer.Value.DrawString(body.Names.First(), fontLabel, brushLabel, p + (0.7f * radius));
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
                    Color clrShadowOutline = Color.FromArgb(100, 50, 0);
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

        private Color GradientColor(Color color1, Color color2, double percent)
        {
            double r = color1.R + percent * (color2.R - color1.R);
            double g = color1.G + percent * (color2.G - color1.G);
            double b = color1.B + percent * (color2.B - color1.B);
            double a = color1.A + percent * (color2.A - color1.A);
            return Color.FromArgb((byte)a, (byte)r, (byte)g, (byte)b);
        }

        public override void Render(IMapContext map) { }

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

        private CrdsGeographical GetVisibleFeatureCoordinates(double latitude, double longitude, double latitudeShift, double longitudeShift)
        {
            double theta = Angle.ToRadians(90 - latitude); // [0...180]
            double phi = Angle.ToRadians(Angle.To360(longitude)); // [0...360]

            // Cartesian coordinates
            double x = Math.Sin(theta) * Math.Cos(phi);
            double y = Math.Sin(theta) * Math.Sin(phi);
            double z = Math.Cos(theta);
            Vec3 v = new Vec3(x, y, z);

            v = Mat4.YRotation(Angle.ToRadians(latitudeShift)).Transpose() * Mat4.ZRotation(Angle.ToRadians(-longitudeShift)).Transpose() * v;

            x = v[0];
            y = v[1];
            z = v[2];

            // back to spherical
            theta = 90 - Angle.ToDegrees(Math.Acos(z / Math.Sqrt(x * x + y * y + z * z)));
            phi = Angle.ToDegrees(Math.Atan2(y, x));

            return new CrdsGeographical(phi, theta);
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

        private Color GetPlanetColor(int planet)
        {
            switch (planet)
            {
                case 1: return Color.FromArgb(132, 131, 131);
                case 2: return Color.FromArgb(228, 189, 127);
                case 4: return Color.FromArgb(183, 98, 71);
                case 5: return Color.FromArgb(166, 160, 149);
                case 6: return Color.FromArgb(207, 192, 162);
                case 7: return Color.FromArgb(155, 202, 209);
                case 8: return Color.FromArgb(54, 79, 167);
                case 9: return Color.FromArgb(207, 192, 162);
                default: return Color.Gray;
            }
        }
    }
}
