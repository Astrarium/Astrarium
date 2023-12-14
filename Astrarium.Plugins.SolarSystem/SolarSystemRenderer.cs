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

namespace Astrarium.Plugins.SolarSystem
{
    /// <summary>
    /// Draws solar system objects (Sun, Moon and planets) on the map.
    /// </summary>
    public class SolarSystemRenderer : BaseRenderer
    {
        private readonly ISkyMap map;
        private readonly PlanetsCalc planetsCalc;
        private readonly ISettings settings;
        private readonly ITextureManager textureManager;
        private readonly Lazy<TextRenderer> textRenderer = new Lazy<TextRenderer>(() => new TextRenderer(256, 32));

        private readonly Sun sun;
        private readonly Moon moon;
        private readonly Planet mars;
        private readonly Planet jupiter;
        private readonly Planet saturn;
        private readonly Pluto pluto;

        private Font fontShadowLabel = new Font("Arial", 8);
        private Brush brushLabel;
        private readonly SolarTextureManager solarTextureManager;
        private readonly ICollection<SurfaceFeature> lunarFeatures;
        private readonly ICollection<SurfaceFeature> martianFeatures;

        private readonly string dataPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data");

        public SolarSystemRenderer(ISkyMap map, LunarCalc lunarCalc, SolarCalc solarCalc, PlanetsCalc planetsCalc, ITextureManager textureManager, ISettings settings)
        {
            this.map = map;
            this.planetsCalc = planetsCalc;
            this.textureManager = textureManager;
            this.settings = settings;

            sun = solarCalc.Sun;
            moon = lunarCalc.Moon;
            mars = planetsCalc.Planets.ElementAt(Planet.MARS - 1);
            jupiter = planetsCalc.Planets.ElementAt(Planet.JUPITER - 1);
            saturn = planetsCalc.Planets.ElementAt(Planet.SATURN - 1);
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
            bool drawLabelMag = settings.Get("PlanetsLabelsMag");
            var prj = map.Projection;
            var nightMode = settings.Get("NightMode");
            brushLabel = new SolidBrush(settings.Get<Color>("ColorSolarSystemLabel").Tint(nightMode));

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
                .Cast<CelestialObject>()
                .ToArray();

            foreach (var body in bodies)
            {
                // do not draw if display setting is off
                if (!body.DisplaySettingNames.All(x => settings.Get(x)))
                {
                    continue;
                }

                CrdsEquatorial eq = body.Equatorial;
                double rotZenith = 0;
                double mu = 1;

                if (settings.Get("Refraction"))
                {
                    CrdsHorizontal hor = eq.ToHorizontal(prj.Context.GeoLocation, prj.Context.SiderealTime);
                    rotZenith = prj.GetAxisRotation(hor, 0);
                    mu = Refraction.Flattening(hor.Altitude, moon.Semidiameter / 3600 * 2);
                    hor.Altitude += Refraction.Correction(hor.Altitude);
                    eq = hor.ToEquatorial(prj.Context.GeoLocation, prj.Context.SiderealTime);
                }

                if (body is Planet planet)
                {
                    double rotAxis = prj.GetAxisRotation(planet.Equatorial, planet.Appearance.P);
                    double rotPhase = prj.GetPhaseRotation(planet.Ecliptical);
                    string label = settings.Get("PlanetsLabelsMag") ? $"{planet.Name} {Formatters.Magnitude.Format(planet.Magnitude)}" : planet.Name;

                    RenderSolarSystemObject(planet, new SphereParameters()
                    {
                        Equatorial = eq,
                        Color = GetPlanetColor(planet.Number),
                        MinimalPointSize = settings.Get("PlanetsDrawAll") ? 1 : 0,
                        MaximalPointSize = 7,
                        TextureName = Path.Combine(dataPath, $"{planet.Number}.jpg"),
                        PhaseAngle = planet.PhaseAngle,
                        Flattening = planet.Flattening,
                        LatitudeShift = -planet.Appearance.D,
                        LongitudeShift = planet.Appearance.CM - (planet.Number == Planet.JUPITER ? planetsCalc.GreatRedSpotLongitude : 0),
                        RotationZenith = rotZenith,
                        RotationAxis = rotAxis,
                        RotationPhase = rotPhase,
                        BodyPhysicalDiameter = 2 * Planet.EQUATORIAL_RADIUS[planet.Number - 1],
                        SurfaceFeatures = planet.Number == Planet.MARS && settings.Get("PlanetsSurfaceFeatures") ? martianFeatures : null,
                        SmoothShadow = planet.Number > Planet.MARS,
                        DrawLabel = settings.Get("PlanetsLabels"),
                        Label = label
                    });

                    if (planet.Number == Planet.JUPITER)
                    {
                        // draw moon shadows over Jupiter
                        RenderJupiterMoonShadow(planet);
                    }
                }
                else if (body is MarsMoon mm)
                {
                    RenderSolarSystemObject(mm, new SphereParameters()
                    {
                        DrawLabel = settings.Get("PlanetsLabels"),
                        Equatorial = eq,
                        MaximalDiskSize = 0,
                        MaximalPointSize = 2
                    });
                }
                else if (body is JupiterMoon jupiterMoon)
                {
                    double rotAxis = prj.GetAxisRotation(jupiterMoon.Equatorial, jupiter.Appearance.P);
                    double rotPhase = prj.GetPhaseRotation(jupiter.Ecliptical);
                    RenderSolarSystemObject(jupiterMoon, new SphereParameters()
                    {
                        TextureName = Path.Combine(dataPath, $"5-{jupiterMoon.Number}.jpg"),
                        LongitudeShift = jupiterMoon.CM,
                        PhaseAngle = jupiter.PhaseAngle,
                        RotationAxis = rotAxis,
                        RotationPhase = rotPhase,
                        Equatorial = eq,
                        DrawLabel = settings.Get("PlanetsLabels") && prj.Fov <= 1,
                        MaximalPointSize = 3
                    });

                    // shadow of other moons above current
                    RenderJupiterMoonShadow(jupiterMoon, jupiterMoon.RectangularS);

                    // shadow of jupiter above current
                    RenderJupiterShadow(jupiterMoon);
                }
                else if (body is SaturnMoon saturnMoon)
                {
                    double rotAxis = prj.GetAxisRotation(saturnMoon.Equatorial, saturn.Appearance.P);
                    double rotPhase = prj.GetPhaseRotation(saturn.Ecliptical);
                    RenderSolarSystemObject(saturnMoon, new SphereParameters()
                    {
                        Equatorial = eq,
                        MaximalPointSize = 1.5f,
                        TextureName = Path.Combine(dataPath, $"6-{saturnMoon.Number}.jpg"),
                        LongitudeShift = saturnMoon.CM,
                        PhaseAngle = saturn.PhaseAngle,
                        RotationAxis = rotAxis,
                        RotationPhase = rotPhase,
                        DrawLabel = settings.Get("PlanetsLabels")
                    });
                }
                else if (body is UranusMoon uranusMoon)
                {
                    RenderSolarSystemObject(uranusMoon, new SphereParameters()
                    {
                        DrawLabel = settings.Get("PlanetsLabels"),
                        Equatorial = eq,
                        MaximalDiskSize = 0,
                        MaximalPointSize = 2
                    });
                }
                else if (body is NeptuneMoon neptuneMoon)
                {
                    RenderSolarSystemObject(neptuneMoon, new SphereParameters()
                    {
                        DrawLabel = settings.Get("PlanetsLabels"),
                        Equatorial = eq,
                        MaximalDiskSize = 0,
                        MaximalPointSize = 2
                    });

                }
                else if (settings.Get("GenericMoons") && body is GenericMoon gm)
                {
                    RenderSolarSystemObject(gm, new SphereParameters()
                    {
                        DrawLabel = settings.Get("PlanetsLabels"),
                        Equatorial = eq,
                        MaximalDiskSize = 0,
                        MaximalPointSize = 2
                    });
                }
                else if (body is Pluto pluto)
                {
                    double rotAxis = prj.GetAxisRotation(pluto.Equatorial, pluto.Appearance.P);
                    double rotPhase = prj.GetPhaseRotation(pluto.Ecliptical);
                    string label = settings.Get("PlanetsLabelsMag") ? $"{pluto.Name} {Formatters.Magnitude.Format(pluto.Magnitude)}" : pluto.Name;

                    RenderSolarSystemObject(pluto, new SphereParameters()
                    {
                        Equatorial = eq,
                        Color = GetPlanetColor(pluto.Number),
                        MinimalPointSize = settings.Get("PlanetsDrawAll") ? 1 : 0,
                        MaximalPointSize = 7,
                        TextureName = Path.Combine(dataPath, $"{pluto.Number}.jpg"),
                        LatitudeShift = -pluto.Appearance.D,
                        LongitudeShift = pluto.Appearance.CM,
                        RotationAxis = rotAxis,
                        RotationPhase = rotPhase,
                        BodyPhysicalDiameter = 2 * Planet.EQUATORIAL_RADIUS[pluto.Number - 1],
                        DrawLabel = settings.Get("PlanetsLabels"),
                        Label = label
                    });
                }
                else if (body is Sun)
                {
                    RenderSun();
                }
                else if (body is Moon)
                {
                    double rotAxis = prj.GetAxisRotation(moon.Equatorial, moon.PAaxis);
                    double rotPhase = prj.GetPhaseRotation(moon.Ecliptical0);
                    double size = prj.GetDiskSize(moon.Semidiameter, 10);
                    int q = Math.Min((int)settings.Get<TextureQuality>("MoonTextureQuality"), size < 256 ? 2 : (size < 1024 ? 4 : 8));
                    string textureName = $"Moon-{q}k.jpg";

                    RenderSolarSystemObject(moon, new SphereParameters()
                    {
                        Equatorial = eq,
                        TextureName = Path.Combine(dataPath, textureName),
                        FallbackTextureName = Path.Combine(dataPath, "Moon-2k.jpg"),
                        MinimalDiskSize = 10,
                        MaximalPointSize = 0,
                        PhaseAngle = moon.PhaseAngle,
                        LatitudeShift = -moon.Libration.b,
                        LongitudeShift = -moon.Libration.l,
                        Refraction = mu,
                        RotationZenith = rotZenith,
                        RotationAxis = rotAxis,
                        RotationPhase = rotPhase,
                        BodyPhysicalDiameter = 3474,
                        SurfaceFeatures = settings.Get("MoonSurfaceFeatures") ? lunarFeatures : null,
                        EarthShadowApperance = moon.EarthShadow,
                        EarthShadowCoordinates = moon.EarthShadowCoordinates,
                        DrawLabel = settings.Get("MoonLabel"),
                        Color = Color.Gray
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
            /// Color of the celestial body when it's drawn as a point
            /// </summary>
            public Color Color { get; set; } = Color.White;

            /// <summary>
            /// Texture name (path) to be used
            /// </summary>
            public string TextureName { get; set; }

            /// <summary>
            /// Fallback texture, if any. Can be null.
            /// </summary>
            public string FallbackTextureName { get; set; }

            public bool SmoothShadow { get; set; }

            public float MinimalPointSize { get; set; }
            public float MaximalPointSize { get; set; }
            public float MinimalDiskSize { get; set; }
            public float MaximalDiskSize { get; set; } = float.MaxValue;

            /// <summary>
            /// Collection of surface features of the body
            /// </summary>
            public ICollection<SurfaceFeature> SurfaceFeatures { get; set; }

            /// <summary>
            /// Refraction flattening of the body
            /// </summary>
            public double Refraction { get; set; } = 1;

            /// <summary>
            /// Physical diameter of celestial body, in kilometers
            /// </summary>
            public double BodyPhysicalDiameter { get; set; }

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

            /// <summary>
            /// Rotation angle of vector pointed to zenith, measured counter-clockwise from top of screen
            /// </summary>
            public double RotationZenith { get; set; }

            /// <summary>
            /// Phase angle of the body
            /// </summary>
            public double PhaseAngle { get; set; }

            /// <summary>
            /// Body flattening
            /// </summary>
            public double Flattening { get; set; }

            public double LongitudeShift { get; set; }
            public double LatitudeShift { get; set; }

            /// <summary>
            /// Flag indicating body should be rendered with label.
            /// </summary>
            public bool DrawLabel { get; set; }

            /// <summary>
            /// Body label (name). If not set, body's primary name will be used.
            /// </summary>
            public string Label { get; set; }
        }

        private void SetPolarCapTextureParameters()
        {
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.MirroredRepeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.MirroredRepeat);
        }

        private void RenderSolarSystemObject<T>(T body, SphereParameters data) where T : SizeableCelestialObject, IMagnitudeObject
        {
            var prj = map.Projection;
            var nightMode = settings.Get("NightMode");
            float starsScalingFactor = (float)settings.Get<decimal>("StarsScalingFactor", 1);

            // size of object when it's drawn as point (in pixels)
            float size = Math.Max(prj.GetPointSize(body.Magnitude, data.MaximalPointSize), data.MinimalPointSize);

            // size of object when it's drawn as sphere (in pixels)
            float diam = Math.Min(prj.GetDiskSize(body.Semidiameter, data.MinimalDiskSize), data.MaximalDiskSize);

            // take into account dimming during twilight
            float starDimming = 1 - map.DaylightFactor;
            size *= starDimming;

            Vec2 p = prj.Project(data.Equatorial);
            if (p == null) return;

            // DRAW AS POINT
            if (size >= diam && size >= data.MinimalPointSize && size > 0 && data.MaximalPointSize > 0 && map.DaylightFactor < 1)
            {
                // out of screen
                if (!prj.IsInsideScreen(p)) return;

                GL.Enable(EnableCap.PointSmooth);
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest);

                GL.PointSize(size * starsScalingFactor);
                GL.Begin(PrimitiveType.Points);
                GL.Color3(data.Color.Tint(nightMode));
                GL.Vertex2(p.X, p.Y);
                GL.End();
            }
            // DRAW AS TEXTURED SPHERE
            else if (diam >= data.MinimalDiskSize && diam >= data.MaximalPointSize)
            {
                // do not draw if out of screen
                double fov = prj.Fov * Math.Max(prj.ScreenWidth, prj.ScreenHeight) / Math.Min(prj.ScreenWidth, prj.ScreenHeight);

                if (Angle.Separation(prj.CenterEquatorial, data.Equatorial) > fov + body.Semidiameter / 3600 * 2) return;

                GL.Enable(EnableCap.Texture2D);

                float[] zero = new float[4] { 0, 0, 0, 0 };

                // color of unilluminated part 
                float[] ambient;

                // color of illuminated part
                float[] diffuse;

                if (settings.Get("NightMode"))
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

                GL.PushMatrix();
                GL.Translate(p.X, p.Y, 0);

                double x, y, z;
                double s, t;
                int i, j;

                // radius of sphere, in pixels
                float radius = prj.GetDiskSize(body.Semidiameter, data.MinimalDiskSize) / 2;

                // number of segments to build the sphere
                int segments = radius < 20 ? 16 : 64;

                // delta rho, step by latitude
                double drho = Math.PI / segments;

                // delta theta, step by longitude
                double dtheta = 2.0 * Math.PI / segments;

                // step by segments count
                double delta = 1.0 / segments;

                // rotation matrix to proper orient sphere 
                Mat4 matVision = Mat4.XRotation(-Math.PI / 2 + Angle.ToRadians((prj.FlipVertical ? -1 : 1) * data.LatitudeShift)) * Mat4.ZRotation(Math.PI + Angle.ToRadians(-data.LongitudeShift) * (prj.FlipHorizontal ? -1 : 1));

                // illumination matrix (phase)
                Mat4 matLight = Mat4.YRotation(Angle.ToRadians(data.PhaseAngle) * (prj.FlipHorizontal ? -1 : 1)) * matVision;

                // rotation of axis
                double rotAxis = Angle.ToRadians(data.RotationAxis);

                // rotation of phase
                double rotPhase = Angle.ToRadians(data.RotationPhase);

                matVision = Mat4.ZRotation(rotAxis) * matVision;
                matLight = Mat4.ZRotation(rotPhase) * matLight;

                // take refraction into account
                if (settings.Get("Refraction"))
                {
                    // rotation angle around Zenith direction
                    double rotZenith = Angle.ToRadians(data.RotationZenith);

                    // flattening a disk due to refraction
                    double mu = data.Refraction;

                    // See explanation here: https://math.stackexchange.com/questions/567254/how-to-create-a-2d-geometric-transformation-matrix-to-stretch-an-image-along-a-g
                    var matRefraction = Mat4.ZRotation(rotZenith) * Mat4.StretchY(mu) * Mat4.ZRotation(-rotZenith);

                    // How to calculate refraction in equatorial coordinates:
                    // https://articles.adsabs.harvard.edu/cgi-bin/nph-iarticle_query?bibcode=1996PASP..108.1051S&db_key=AST&page_ind=5&data_type=GIF&type=SCREEN_VIEW&classic=YES

                    matVision = matRefraction * matVision;
                    matLight = matRefraction * matLight;
                }

                float shadowSmoothness = data.SmoothShadow ? 1 : 5;

                bool drawCaps = body == mars && settings.Get("PlanetsMartianPolarCaps");
                bool drawRings = body is Planet saturn && saturn.Number == Planet.SATURN;

                Vec3 vecVision;
                Vec3 vecLight;

                if (drawRings)
                {
                    RenderSaturnRings(data, 1);
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
                        cap1 = prj.FlipVertical ? 1 - planetsCalc.MarsSPCWidth / 180 : 1 - planetsCalc.MarsNPCWidth / 180;
                        cap2 = prj.FlipVertical ? planetsCalc.MarsNPCWidth / 180 : planetsCalc.MarsSPCWidth / 180;
                    }

                    int texture = 0;

                    if (layer == LAYER_PLANET)
                    {
                        texture = textureManager.GetTexture(data.TextureName, data.FallbackTextureName, permanent: data.FallbackTextureName != null);
                    }
                    else if (layer == LAYER_POLAR_CAP)
                    {
                        texture = textureManager.GetTexture(Path.Combine(dataPath, "PolarCap.png"), fallbackPath: null, permanent: false, action: SetPolarCapTextureParameters, alphaChannel: true);
                    }

                    if (texture > 0)
                    {
                        GL.Enable(EnableCap.Texture2D);
                        GL.BindTexture(TextureTarget.Texture2D, texture);
                    }
                    else
                    {
                        float r = data.Color.R / 255f;
                        float g = data.Color.G / 255f;
                        float b = data.Color.B / 255f;
                        GL.Disable(EnableCap.Texture2D);
                        GL.Light(LightName.Light0, LightParameter.Ambient, new float[4] { r, g, b, 0.5f });
                        GL.Light(LightName.Light0, LightParameter.Diffuse, new float[4] { r * 0.25f, g * 0.25f, b * 0.25f, 1f });
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
                                GL.TexCoord2(-s * (prj.FlipHorizontal ? -1 : 1), -t * (prj.FlipVertical ? -1 : 1));
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
                                GL.TexCoord2(-s * (prj.FlipHorizontal ? -1 : 1), (delta - t) * (prj.FlipVertical ? -1 : 1));
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
                    RenderSaturnRings(data, -1);
                }

                GL.PopMatrix();

                GL.Disable(EnableCap.Light0);
                GL.Disable(EnableCap.Lighting);
                GL.Disable(EnableCap.CullFace);
                GL.Disable(EnableCap.Blend);
                GL.Disable(EnableCap.Texture2D);

                if (data.EarthShadowApperance != null && data.EarthShadowCoordinates != null)
                {
                    RenderEarthShadow(data);
                }

                if (data.SurfaceFeatures != null)
                {
                    RenderPlanetFeatures(body, data);
                }
            }
            else
            {
                return;
            }

            map.AddDrawnObject(p, body, diam);

            if (data.DrawLabel)
            {
                string label = data.Label ?? body.Names.First();
                var fontLabel = settings.Get<Font>("SolarSystemLabelsFont");

                map.DrawObjectLabel(textRenderer.Value, label, fontLabel, brushLabel, p, Math.Max(size, diam));
            }
        }

        private void RenderSun()
        {
            var prj = map.Projection;
            var nightMode = settings.Get("NightMode");
            float diam = prj.GetDiskSize(sun.Semidiameter, 10);

            // do not draw if out of screen
            double fov = prj.Fov * Math.Max(prj.ScreenWidth, prj.ScreenHeight) / Math.Min(prj.ScreenWidth, prj.ScreenHeight);
            if (Angle.Separation(prj.CenterEquatorial, sun.Equatorial) > fov + sun.Semidiameter / 3600 * 2) return;

            float r = diam / 2;
            Vec2 p = prj.Project(sun.Equatorial);
            if (p == null) return;

            double rotAxis = Angle.ToRadians(prj.GetAxisRotation(sun.Equatorial, -prj.Context.Epsilon));

            int textureId = -1;

            if (settings.Get("SunTexture") && r > 5)
            {
                GL.Enable(EnableCap.Texture2D);
                textureId = solarTextureManager.GetTexture(prj.Context.JulianDay);
                GL.BindTexture(TextureTarget.Texture2D, textureId);
            }

            GL.PushMatrix();
            GL.Translate(p.X, p.Y, 0);

            if (nightMode)
            {
                GL.Color4(Color.Red);
            }
            else if (textureId > 0)
            {
                GL.Color4(Color.White);
            }
            else
            {
                if (map.DaylightFactor == 1)
                {
                    GL.Color4(Color.White);
                }
                else
                {
                    var f = map.DaylightFactor;
                    var c1 = Color.Orange;
                    var c2 = Color.White;
                    float R = c1.R + f * (c2.R - c1.R);
                    float G = c1.G + f * (c2.G - c1.G);
                    float B = c1.B + f * (c2.B - c1.B);
                    GL.Color4(Color.FromArgb((byte)R, (byte)G, (byte)B));
                }
            }

            GL.Begin(PrimitiveType.TriangleFan);

            for (int i = 0; i <= 64; i++)
            {
                double ang0 = Angle.ToRadians(i / 64.0 * 360);
                double ang = ang0 + rotAxis;
                Vec2 v = new Vec2(r * Math.Cos(ang), r * Math.Sin(ang));

                if (textureId > 0)
                {
                    double tx = (prj.FlipHorizontal ? -1 : 1) * Math.Cos(ang0);
                    double ty = -(prj.FlipVertical ? -1 : 1) * Math.Sin(ang0);
                    GL.TexCoord2(0.5f + 0.499f * tx, 0.5f + 0.499f * ty);
                }
                
                GL.Vertex2(v.X, v.Y);
            }

            GL.End();

            GL.PopMatrix();

            GL.Disable(EnableCap.Texture2D);

            map.AddDrawnObject(p, sun, diam);

            if (settings.Get("SunLabel"))
            {
                var fontLabel = settings.Get<Font>("SolarSystemLabelsFont");
                textRenderer.Value.DrawString(sun.Name, fontLabel, brushLabel, p + (0.7f * r));
            }
        }

        private void RenderEarthShadow(SphereParameters data)
        {
            var prj = map.Projection;

            // moon radius in pixels (1 extra pixel added for better rendering)
            float rMoon = prj.GetDiskSize(moon.Semidiameter) / 2 + 1;

            if (rMoon > 5)
            {
                // center of the moon in screen coordinates
                var pMoon = prj.Project(data.Equatorial);
                if (pMoon == null) return;

                // center of the shadow
                var pShadow = prj.Project(data.EarthShadowCoordinates);
                if (pShadow == null) return;

                // moon semidiameter in seconds of arc
                double sdMoon = moon.Semidiameter;

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
                RenderEclipseShadow(pMoon, pShadow, Text.Get("EarthShadow.Label"), rMoon, shadowRadii, shadowColors);

                // draw shadow outline
                if (settings.Get("EarthShadowOutline"))
                {
                    Color clrShadowOutline = Color.FromArgb(100, 50, 0);
                    var pen = new Pen(clrShadowOutline) { DashStyle = DashStyle.Dot };

                    Primitives.DrawEllipse(pShadow, pen, sdPenumbraPixels);
                    Primitives.DrawEllipse(pShadow, pen, sdUmbraPixels);

                    if (map.Projection.Fov <= 10)
                    {
                        var brush = new SolidBrush(clrShadowOutline);
                        textRenderer.Value.DrawString(Text.Get("EarthShadow.Label"), fontShadowLabel, brush, new Vec2(sdPenumbraPixels * 0.71, -sdPenumbraPixels * 0.71));
                    }
                }
            }
        }

        private void RenderPlanetFeatures(SizeableCelestialObject body, SphereParameters data)
        {
            var prj = map.Projection;

            // radius of celestial body disk, in pixels
            float r = prj.GetDiskSize(body.Semidiameter) / 2;

            if (r > 100)
            {
                // feature types that should be drawn with outline
                string[] outlinedFeatures = new string[] { "AA", "SF" };

                // feature types that should be labeled in central point
                string[] centeredFeatures = new string[] { "ME", "OC", "SI", "LC", "PA", "PR", "MO", "VA", "RU", "RI", "DO", "CA", "AL", "LF", "PL" };

                // center of the body in screen coordinates
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
                    CrdsGeographical v = GetVisibleFeatureCoordinates(feature.Latitude, feature.Longitude, data);

                    // angular distance between disk center and feature
                    double sep = Angle.Separation(v, c);

                    // if feature is not too close to disk edge
                    if (sep < 85)
                    {
                        Vec2 pFeature = GetCartesianFeatureCoordinates(prj, r, v, data);

                        if (prj.IsInsideScreen(p + pFeature))
                        {
                            // feature outline radius, in pixels
                            double fr = (feature.Diameter > 0 ? feature.Diameter : data.BodyPhysicalDiameter / 6) / data.BodyPhysicalDiameter * r;

                            // distance, in pixels, between center of the feature and current mouse position
                            double d = Math.Sqrt(Math.Pow(map.MouseScreenCoordinates.X - pFeature.X - p.X, 2) + Math.Pow(map.MouseScreenCoordinates.Y - pFeature.Y - p.Y, 2));

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

                                    Primitives.DrawEllipse(pFeature, pen, fr, fr * f, rot);
                                    textRenderer.Value.DrawString(feature.Name, fontLabel, brush, pFeature);
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

        private void RenderSaturnRings(SphereParameters data, int sign)
        {
            int j;
            double x, y;

            // number or segments
            const int segments = 64;

            // radius of outer ring relative to Saturn equatorial radius
            const double ringsRatio = 2.320;

            var prj = map.Projection;

            GL.Disable(EnableCap.Lighting);
            int textureId = textureManager.GetTexture(Path.Combine(dataPath, "Rings.png"), fallbackPath: null, permanent: false, action: null, alphaChannel: true);
            if (textureId > 0)
            {
                GL.Enable(EnableCap.Texture2D);
                GL.BindTexture(TextureTarget.Texture2D, textureId);
            }
            else
            {
                float r = data.Color.R / 255f;
                float g = data.Color.G / 255f;
                float b = data.Color.B / 255f;
                GL.Disable(EnableCap.Texture2D);
                GL.Enable(EnableCap.Lighting);
                GL.Light(LightName.Light0, LightParameter.Ambient, new float[4] { r, g, b, 0.5f });
                GL.Light(LightName.Light0, LightParameter.Diffuse, new float[4] { r * 0.25f, g * 0.25f, b * 0.25f, 1f });
            }

            if (settings.Get("NightMode"))
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
            double sd = prj.GetDiskSize(saturn.Semidiameter, data.MinimalDiskSize) / 2 * ringsRatio;

            // rotation of axis
            double rotAxis = Angle.ToRadians(data.RotationAxis);

            // rotation matrix for rings vision
            var matRings = Mat4.ZRotation(rotAxis) * Mat4.XRotation(Angle.ToRadians(data.LatitudeShift - 90));

            for (j = 0; j <= segments / 2; j++)
            {
                double ang = j / (double)segments * 2 * Math.PI - sign * Math.Sign(data.LatitudeShift) * Math.PI / 2 * (prj.FlipVertical ? -1 : 1);
                x = -Math.Sin(ang);
                y = -Math.Sign(data.LatitudeShift) * Math.Cos(ang);
                Vec3 vecVision = matRings * new Vec3(x, y, 0);
                GL.TexCoord2(0, 0);
                GL.Vertex3(sd * vecVision.X, sd * vecVision.Y, 0);
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

        public override void OnMouseMove(ISkyMap map, MouseButton mouseButton)
        {
            if (mouseButton == MouseButton.None)
            {
                PointF mouse = map.MouseScreenCoordinates;

                // Moon surface features
                {
                    var p = map.Projection.Project(moon.Equatorial);
                    if (p != null)
                    {
                        double r = map.Projection.GetDiskSize(moon.Semidiameter) / 2;
                        bool needDraw = (mouse.X - p.X) * (mouse.X - p.X) + (mouse.Y - p.Y) * (mouse.Y - p.Y) < r * r;
                        if (needDraw)
                        {
                            map.Invalidate();
                        }
                    }
                }

                // Mars surface features
                {
                    var p = map.Projection.Project(mars.Equatorial);
                    if (p != null)
                    {
                        double r = map.Projection.GetDiskSize(mars.Semidiameter) / 2;
                        bool needDraw = (mouse.X - p.X) * (mouse.X - p.X) + (mouse.Y - p.Y) * (mouse.Y - p.Y) < r * r;
                        if (needDraw)
                        {
                            map.Invalidate();
                        }
                    }
                }
            }
        }

        private Vec2 GetCartesianFeatureCoordinates(Projection prj, float r, CrdsGeographical c, SphereParameters data)
        {
            // rotation of axis
            double axisRotation = Angle.ToRadians(data.RotationAxis);

            // rotation of vector pointed to zenith (for refraction)
            double zenithRotation = -Angle.ToRadians(data.RotationZenith);

            // convert to orthographic polar coordinates 
            double Y = r * Math.Sin(Angle.ToRadians(c.Latitude));
            double X = r * Math.Cos(Angle.ToRadians(c.Latitude)) * Math.Sin(Angle.ToRadians(c.Longitude));

            Y = Y * (prj.FlipVertical ? -1 : 1);
            X = X * (prj.FlipHorizontal ? -1 : 1);

            // polar coordinates rotated around of visible center of the body disk
            double X_ = X * Math.Cos(axisRotation) - Y * Math.Sin(axisRotation);
            double Y_ = X * Math.Sin(axisRotation) + Y * Math.Cos(axisRotation);

            double Xr = X_;
            double Yr = Y_;

            if (settings.Get("Refraction"))
            {
                double X__ = X_ * Math.Cos(zenithRotation) - Y_ * Math.Sin(zenithRotation);
                double Y__ = X_ * Math.Sin(zenithRotation) + Y_ * Math.Cos(zenithRotation);

                Y__ = Y__ * 5.0 / 6.0; // refraction flattening

                Xr = X__ * Math.Cos(-zenithRotation) - Y__ * Math.Sin(-zenithRotation);
                Yr = X__ * Math.Sin(-zenithRotation) + Y__ * Math.Cos(-zenithRotation);
            }
            return new Vec2(Xr, Yr);
        }

        private CrdsGeographical GetVisibleFeatureCoordinates(double latitude, double longitude, SphereParameters data)
        {
            double theta = Angle.ToRadians(90 - latitude); // [0...180]
            double phi = Angle.ToRadians(Angle.To360(longitude)); // [0...360]

            double latitudeShift = Angle.ToRadians(data.LatitudeShift);
            double longitudeShift = Angle.ToRadians(data.LongitudeShift);

            // Cartesian coordinates
            double x = Math.Sin(theta) * Math.Cos(phi);
            double y = Math.Sin(theta) * Math.Sin(phi);
            double z = Math.Cos(theta);
            Vec3 v = new Vec3(x, y, z);

            v = Mat4.YRotation(latitudeShift).Transpose() * Mat4.ZRotation(-longitudeShift).Transpose() * v;

            x = v[0];
            y = v[1];
            z = v[2];

            // back to spherical
            theta = 90 - Angle.ToDegrees(Math.Acos(z / Math.Sqrt(x * x + y * y + z * z)));
            phi = Angle.ToDegrees(Math.Atan2(y, x));

            return new CrdsGeographical(phi, theta);
        }

        private void RenderJupiterShadow(JupiterMoon moon)
        {
            if (!moon.IsEclipsedByPlanet) return;

            var prj = map.Projection;
            Planet jupiter = planetsCalc.Planets.ElementAt(Planet.JUPITER - 1);

            // Jupiter radius, in pixels, and also radius of its shadow
            float sd = prj.GetDiskSize(jupiter.Semidiameter) / 2;

            // Center of shadow, relative to Jupiter center
            Vec2 pShadow = new Vec2(-moon.RectangularS.X * sd * (prj.FlipHorizontal ? -1 : 1), -moon.RectangularS.Y * sd * (prj.FlipVertical ? -1 : 1));

            // rotation angle of shadow center respect to center of Jupiter
            double rot = prj.GetAxisRotation(jupiter.Equatorial, jupiter.Appearance.P);

            // Center of Moon
            var pMoon = prj.Project(moon.Equatorial);
            if (pMoon == null) return;

            // Center of Jupiter shadow
            pShadow = Mat4.ZRotation(Angle.ToRadians(rot)) * pShadow + pMoon;

            // Radius of eclipsing moon
            float sdMoon = prj.GetDiskSize(moon.Semidiameter) / 2;

            RenderEclipseShadow(pMoon, pShadow, Text.Get("EclipsedByJupiter"), sdMoon + 1, new double[] { 0, sd }, new Color[] { Color.Black, Color.Black }, rot, jupiter.Flattening);

            // Jupiter shadow outline
            // TODO: change color!
            Primitives.DrawEllipse(pShadow, Pens.Brown, sd, sd * (1 - jupiter.Flattening), rot);
        }

        private void RenderEclipseShadow(Vec2 pBody, Vec2 pShadow, string shadowLabel, float radiusBody, double[] shadowRadii, Color[] shadowColors, double rotAngle = 0, double flattening = 0)
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

            //map.DrawObjectLabel(textRenderer.Value, shadowLabel, fontShadowLabel, Brushes.Red, pShadow, 1);
        }

        private void RenderJupiterMoonShadow(SizeableCelestialObject eclipsedBody, CrdsRectangular rect = null)
        {
            Projection prj = map.Projection;

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
                Vec2 pBody = prj.Project(eclipsedBody.Equatorial);

                if (pBody == null) return;

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
                    Vec2 p = new Vec2(shadowRelative.X * sd * (prj.FlipHorizontal ? -1 : 1), shadowRelative.Y * sd * (prj.FlipVertical ? -1 : 1));

                    // rotation angle of shadow center respect to center of Jupiter
                    double rot = prj.GetAxisRotation(jupiter.Equatorial, jupiter.Appearance.P);

                    // Center of shadow
                    p = Mat4.ZRotation(Angle.ToRadians(rot)) * p + pBody;

                    // shadow has enough size to be rendered
                    if ((int)radiusPenumbra > 0)
                    {
                        double[] shadowRadii = new double[] { 0, radiusUmbra * 0.99, radiusUmbra, radiusPenumbra * 1.01, radiusPenumbra };
                        Color[] shadowColors = new Color[] { Color.FromArgb(250, 0, 0, 0), Color.FromArgb(250, 0, 0, 0), Color.FromArgb(150, 0, 0, 0), Color.FromArgb(0, 0, 0, 0), Color.FromArgb(0, 0, 0, 0) };

                        RenderEclipseShadow(pBody, p, Text.Get($"JupiterMoon.{moon.Number}.Shadow"), radiusBody, shadowRadii, shadowColors);
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
