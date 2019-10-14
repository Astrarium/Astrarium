using ADK;
using Planetarium.Objects;
using Planetarium.Renderers;
using Planetarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Plugins.DeepSky
{
    public class DeepSkyRenderer : BaseRenderer
    {
        private int limitLabels = 20;
        private double minAlpha = 10;
        private double maxAlpha = 255;
        private double minZoom = 50;
        private double maxZoom = 0.1;

        private double k;
        private double b;

        private Pen penOutlineDashed;
        private Pen penOutlineSolid;
        private Font fontCaption = new Font("Arial", 7);
        private Brush brushCaption;

        private Dictionary<DeepSkyStatus, IDrawingStrategy> drawingHandlers = null;

        private readonly DeepSkyCalc deepSkyCalc;
        private readonly ISettings settings;

        public DeepSkyRenderer(DeepSkyCalc deepSkyCalc, ISettings settings)
        {
            this.deepSkyCalc = deepSkyCalc;
            this.settings = settings;

            k = -(minAlpha - maxAlpha) / (maxZoom - minZoom);
            b = -(minZoom * maxAlpha - maxZoom * minAlpha) / (maxZoom - minZoom);

            drawingHandlers = new Dictionary<DeepSkyStatus, IDrawingStrategy>()
            {
                { DeepSkyStatus.Galaxy, new GalaxyDrawingStrategy(this) },
                { DeepSkyStatus.GalacticNebula, new GalacticNebulaDrawingStrategy(this) },
                { DeepSkyStatus.PlanetaryNebula, new PlanetaryNebulaDrawingStrategy(this) },
                { DeepSkyStatus.OpenCluster, new ClusterDrawingStrategy(this) },
                { DeepSkyStatus.GlobularCluster, new ClusterDrawingStrategy(this) },
                { DeepSkyStatus.PartOfOther, new EmptyDrawingStrategy(this) },
                { DeepSkyStatus.Duplicate, new EmptyDrawingStrategy(this) },
                { DeepSkyStatus.DuplicateIC, new EmptyDrawingStrategy(this) },
                { DeepSkyStatus.Star, new EmptyDrawingStrategy(this) },
                { DeepSkyStatus.NotFound, new EmptyDrawingStrategy(this) }
            };
        }

        public override void Render(IMapContext map)
        {
            if (!settings.Get<bool>("DeepSky"))
            {
                return;
            }

            var allDeepSkies = deepSkyCalc.DeepSkies;
            bool isGround = settings.Get<bool>("Ground");
            brushCaption = new SolidBrush(map.GetColor(settings.Get<Color>("ColorDeepSkyLabel")));

            int alpha = Math.Max(0, Math.Min((int)(k * map.ViewAngle + b), 255));
            
            Color colorOutline = map.GetColor(settings.Get<Color>("ColorDeepSkyOutline"));
            penOutlineSolid = new Pen(Color.FromArgb(alpha, colorOutline));
            penOutlineDashed = new Pen(Color.FromArgb(alpha, colorOutline));
            penOutlineDashed.DashStyle = DashStyle.Dash;
            
            double coeff = map.DiagonalCoefficient();
            var deepSkies = allDeepSkies.Where(ds => !ds.Status.IsEmpty() && Angle.Separation(map.Center, ds.Horizontal) < map.ViewAngle * coeff);
            if (isGround)
            {
                deepSkies = deepSkies.Where(ds => ds.Horizontal.Altitude + ds.Semidiameter / 3600 > 0);
            }

            foreach (var ds in deepSkies)
            {
                drawingHandlers[ds.Status].Draw(map, settings, ds);
            }
        }

        private interface IDrawingStrategy
        {
            void Draw(IMapContext map, ISettings settings, DeepSky ds);
        }

        private class GalaxyDrawingStrategy : BaseDrawingStrategy
        {
            public GalaxyDrawingStrategy(DeepSkyRenderer renderer) : base(renderer) { }

            protected override void DrawEllipticObject(Graphics g, float diamA, float diamB)
            {
                g.DrawEllipse(Renderer.penOutlineSolid, -diamA / 2, -diamB / 2, diamA, diamB);
            }

            protected override void DrawPointObject(Graphics g, float size)
            {
                g.FillEllipse(Renderer.penOutlineSolid.Brush, -size / 2, - size / 2, size, size);
            }

            protected override void DrawRoundObject(Graphics g, float diamA)
            {
                g.DrawEllipse(Renderer.penOutlineSolid, -diamA / 2, -diamA / 2, diamA, diamA);
            }
        }

        private class GalacticNebulaDrawingStrategy : BaseDrawingStrategy
        {
            public GalacticNebulaDrawingStrategy(DeepSkyRenderer renderer) : base(renderer) { }

            public override void Draw(IMapContext map, ISettings settings, DeepSky ds)
            {
                if (map.ViewAngle <= Renderer.minZoom)
                {
                    base.Draw(map, settings, ds);
                }
            }

            protected override void DrawEllipticObject(Graphics g, float diamA, float diamB)
            {
                g.DrawRoundedRectangle(Renderer.penOutlineSolid, -diamA / 2, -diamB / 2, diamA, diamB, Math.Min(diamA, diamB) / 3);
            }

            protected override void DrawPointObject(Graphics g, float size)
            {
                g.FillEllipse(Renderer.penOutlineSolid.Brush, -size / 2, -size / 2, size, size);
            }

            protected override void DrawRoundObject(Graphics g, float diamA)
            {
                diamA /= 1.15f;
                g.DrawRoundedRectangle(Renderer.penOutlineSolid, -diamA / 2, -diamA / 2, diamA, diamA, diamA / 3);
            }
        }

        private class PlanetaryNebulaDrawingStrategy : BaseDrawingStrategy
        {
            public PlanetaryNebulaDrawingStrategy(DeepSkyRenderer renderer) : base(renderer) { }

            public override void Draw(IMapContext map, ISettings settings, DeepSky ds)
            {
                if (map.ViewAngle <= Renderer.minZoom)
                {
                    base.Draw(map, settings, ds);
                }
            }

            protected override void DrawEllipticObject(Graphics g, float diamA, float diamB)
            {
                g.DrawEllipse(Renderer.penOutlineSolid, -diamA / 2, -diamB / 2, diamA, diamB);
            }

            protected override void DrawPointObject(Graphics g, float size)
            {
                g.FillEllipse(Renderer.penOutlineSolid.Brush, -size / 2, -size / 2, size, size);
            }

            protected override void DrawRoundObject(Graphics g, float diamA)
            {
                g.DrawEllipse(Renderer.penOutlineSolid, -diamA / 2, -diamA / 2, diamA, diamA);
            }
        }

        private class ClusterDrawingStrategy : BaseDrawingStrategy
        {
            public ClusterDrawingStrategy(DeepSkyRenderer renderer) : base(renderer) { }

            public override void Draw(IMapContext map, ISettings settings, DeepSky ds)
            {
                if (map.ViewAngle <= Renderer.minZoom)
                {
                    base.Draw(map, settings, ds);
                }
            }

            protected override void DrawEllipticObject(Graphics g, float diamA, float diamB)
            {
                g.DrawEllipse(Renderer.penOutlineDashed, -diamA / 2, -diamB / 2, diamA, diamB);
            }

            protected override void DrawPointObject(Graphics g, float size)
            {
                g.FillEllipse(Renderer.penOutlineSolid.Brush, -size / 2, -size / 2, size, size);
            }

            protected override void DrawRoundObject(Graphics g, float diamA)
            {
                g.DrawEllipse(Renderer.penOutlineDashed, -diamA / 2, -diamA / 2, diamA, diamA);
            }
        }

        private class EmptyDrawingStrategy : IDrawingStrategy
        {
            protected DeepSkyRenderer Renderer { get; private set; }

            public EmptyDrawingStrategy(DeepSkyRenderer renderer)
            {
                Renderer = renderer;
            }

            public void Draw(IMapContext map, ISettings settings, DeepSky ds)
            {
                // Do nothing
            }
        }

        private abstract class BaseDrawingStrategy : IDrawingStrategy
        {
            protected DeepSkyRenderer Renderer { get; private set; }

            protected abstract void DrawEllipticObject(Graphics g, float diamA, float diamB);
            protected abstract void DrawRoundObject(Graphics g, float diamA);
            protected abstract void DrawPointObject(Graphics g, float size);

            public BaseDrawingStrategy(DeepSkyRenderer renderer)
            {
                Renderer = renderer;
            }

            public virtual void Draw(IMapContext map, ISettings settings, DeepSky ds)
            {
                PointF p = map.Project(ds.Horizontal);

                float sizeA = GetDiameter(map, ds.SizeA);
                float sizeB = GetDiameter(map, ds.SizeB);

                // elliptic object with known size
                if (sizeB > 0 && sizeB != sizeA)
                {
                    float diamA = GetDiameter(map, ds.SizeA);
                    if (diamA > 10)
                    {
                        float diamB = GetDiameter(map, ds.SizeB);
                        if (ds.Outline != null && settings.Get<bool>("DeepSkyOutlines"))
                        {
                            DrawOutline(map, ds.Outline);
                        }
                        else
                        {
                            float rotation = map.GetRotationTowardsNorth(ds.Equatorial) + 90 - ds.PA;
                            map.Graphics.TranslateTransform(p.X, p.Y);
                            map.Graphics.RotateTransform(rotation);
                            DrawEllipticObject(map.Graphics, diamA, diamB);
                            map.Graphics.ResetTransform();
                        }
                        map.AddDrawnObject(ds);

                        if (map.ViewAngle <= Renderer.limitLabels && settings.Get<bool>("DeepSkyLabels"))
                        {
                            map.DrawObjectCaption(Renderer.fontCaption, Renderer.brushCaption, ds.DisplayName, p, Math.Min(diamA, diamB));
                        }
                    }
                }
                // round object
                else if (sizeA > 0)
                {
                    float diamA = GetDiameter(map, ds.SizeA);
                    if (diamA > 10)
                    {
                        if (ds.Outline != null && settings.Get<bool>("DeepSkyOutlines"))
                        {
                            DrawOutline(map, ds.Outline);
                        }
                        else
                        {
                            float rotation = map.GetRotationTowardsNorth(ds.Equatorial) + 90 - ds.PA;
                            map.Graphics.TranslateTransform(p.X, p.Y);
                            map.Graphics.RotateTransform(rotation);
                            DrawRoundObject(map.Graphics, diamA);
                            map.Graphics.ResetTransform();
                        }
                        map.AddDrawnObject(ds);

                        if (map.ViewAngle <= Renderer.limitLabels && settings.Get<bool>("DeepSkyLabels"))
                        {
                            map.DrawObjectCaption(Renderer.fontCaption, Renderer.brushCaption, ds.DisplayName, p, diamA);
                        }
                    }
                }
                // point object
                else
                {
                    float size = map.GetPointSize(ds.Mag == null ? 15 : ds.Mag.Value);
                    if ((int)size > 0)
                    {
                        if (ds.Outline != null && settings.Get<bool>("DeepSkyOutlines"))
                        {
                            DrawOutline(map, ds.Outline);
                        }
                        else
                        {
                            map.Graphics.TranslateTransform(p.X, p.Y);
                            DrawPointObject(map.Graphics, size);
                            map.Graphics.ResetTransform();
                        }
                        map.AddDrawnObject(ds);

                        if (map.ViewAngle <= Renderer.limitLabels && settings.Get<bool>("DeepSkyLabels"))
                        {
                            map.DrawObjectCaption(Renderer.fontCaption, Renderer.brushCaption, ds.DisplayName, p, 0);
                        }
                    }
                }
            }

            protected virtual GraphicsPath DrawOutline(IMapContext map, ICollection<CelestialPoint> outline)
            {
                using (GraphicsPath gp = new GraphicsPath(FillMode.Winding))
                {
                    for (int i = 0; i < outline.Count - 1; i++)
                    {
                        var h1 = outline.ElementAt(i).Horizontal;
                        var h2 = outline.ElementAt(i + 1).Horizontal;

                        double ad1 = Angle.Separation(h1, map.Center);
                        double ad2 = Angle.Separation(h2, map.Center);

                        PointF p1, p2;
                        p1 = map.Project(h1);
                        p2 = map.Project(h2);
                        gp.AddLine(p1, p2);
                    }

                    map.Graphics.DrawPath(Renderer.penOutlineSolid, gp);

                    return gp;
                }
            }

            private float GetDiameter(IMapContext map, double diam)
            {
                double maxSize = Math.Max(map.Width, map.Height);
                return (float)(diam / 60 / map.ViewAngle * maxSize / 2);
            }
        }

        public override RendererOrder Order => RendererOrder.DeepSpace;
    }
}
