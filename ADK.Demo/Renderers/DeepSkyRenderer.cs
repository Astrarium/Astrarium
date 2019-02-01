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
    public class DeepSkyRenderer : BaseSkyRenderer
    {
        private int limitLabels = 20;
        private double minAlpha = 10;
        private double maxAlpha = 255;
        private double minZoom = 50;
        private double maxZoom = 0.1;

        private int lastAlpha = -1;
        private double k;
        private double b;

        private Pen penCluster;
        private Pen penNebula;
        private Font fontCaption = new Font("Arial", 7);

        private Color colorOutline = Color.FromArgb(50, 50, 50);
        private Brush brushCaption = new SolidBrush(Color.FromArgb(0, 64, 128));

        private Dictionary<DeepSkyStatus, IDrawingStrategy> drawingHandlers = null;

        public DeepSkyRenderer(Sky sky, ISkyMap skyMap, ISettings settings) : base(sky, skyMap, settings)
        {
            k = -(minAlpha - maxAlpha) / (maxZoom - minZoom);
            b = -(minZoom * maxAlpha - maxZoom * minAlpha) / (maxZoom - minZoom);

            drawingHandlers = new Dictionary<DeepSkyStatus, IDrawingStrategy>()
            {
                { DeepSkyStatus.Galaxy, new GalaxyDrawingStrategy(this) },
                { DeepSkyStatus.GalacticNebula, new NebulaDrawingStrategy(this) },
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

        private float GetDiameter(double diam)
        {
            return (float)(diam / 60 / Map.ViewAngle * Map.Width / 2);
        }

        public override void Render(Graphics g)
        {
            var allDeepSkies = Sky.Get<ICollection<DeepSky>>("DeepSky");
            bool isGround = Settings.Get<bool>("Ground");

            int alpha = Math.Max(0, Math.Min((int)(k * Map.ViewAngle + b), 255));

            if (lastAlpha != alpha)
            {
                lastAlpha = alpha;
                penNebula = new Pen(Color.FromArgb(alpha, colorOutline));
                penCluster = new Pen(Color.FromArgb(alpha, colorOutline));
                penCluster.DashStyle = DashStyle.Dash;
            }

            var deepSkies = allDeepSkies.Where(ds => !ds.Status.IsEmpty() && Angle.Separation(Map.Center, ds.Horizontal) < Map.ViewAngle * 1.2);
            if (isGround)
            {
                deepSkies = deepSkies.Where(ds => ds.Horizontal.Altitude + ds.Semidiameter / 3600 > 0);
            }

            foreach (var ds in deepSkies)
            {
                drawingHandlers[ds.Status].Draw(g, ds);
            }
        }

        private interface IDrawingStrategy
        {
            void Draw(Graphics g, DeepSky ds);
        }

        private class GalaxyDrawingStrategy : BaseDrawingStrategy
        {
            public GalaxyDrawingStrategy(DeepSkyRenderer renderer) : base(renderer) { }

            protected override void DrawEllipticObject(Graphics g, float diamA, float diamB)
            {
                g.DrawEllipse(Pens.DimGray, -diamA / 2, -diamB / 2, diamA, diamB);
            }

            protected override void DrawPointObject(Graphics g)
            {
                g.FillEllipse(Brushes.DimGray, -1, -1, 2, 2);
            }

            protected override void DrawRoundObject(Graphics g, float diamA)
            {
                g.DrawEllipse(Pens.DimGray, -diamA / 2, -diamA / 2, diamA, diamA);
            }
        }

        private class NebulaDrawingStrategy : BaseDrawingStrategy
        {
            public NebulaDrawingStrategy(DeepSkyRenderer renderer) : base(renderer) { }

            public override void Draw(Graphics g, DeepSky ds)
            {
                if (Renderer.Map.ViewAngle <= Renderer.minZoom)
                {
                    base.Draw(g, ds);
                }
            }

            protected override void DrawEllipticObject(Graphics g, float diamA, float diamB)
            {
                g.DrawRoundedRectangle(Renderer.penNebula, -diamA / 2, -diamB / 2, diamA, diamB, Math.Min(diamA, diamB) / 3);
            }

            protected override void DrawPointObject(Graphics g)
            {
                g.FillEllipse(Brushes.DimGray, -1, -1, 2, 2);
            }

            protected override void DrawRoundObject(Graphics g, float diamA)
            {
                g.DrawRoundedRectangle(Renderer.penNebula, -diamA / 2, -diamA / 2, diamA, diamA, diamA / 3);
            }
        }

        private class PlanetaryNebulaDrawingStrategy : BaseDrawingStrategy
        {
            public PlanetaryNebulaDrawingStrategy(DeepSkyRenderer renderer) : base(renderer) { }

            public override void Draw(Graphics g, DeepSky ds)
            {
                if (Renderer.Map.ViewAngle <= Renderer.minZoom)
                {
                    base.Draw(g, ds);
                }
            }

            protected override void DrawEllipticObject(Graphics g, float diamA, float diamB)
            {
                g.DrawEllipse(Renderer.penNebula, -diamA / 2, -diamB / 2, diamA, diamB);
            }

            protected override void DrawPointObject(Graphics g)
            {
                g.FillEllipse(Brushes.DimGray, -1, -1, 2, 2);
            }

            protected override void DrawRoundObject(Graphics g, float diamA)
            {
                g.DrawEllipse(Renderer.penNebula, -diamA / 2, -diamA / 2, diamA, diamA);
            }
        }

        private class ClusterDrawingStrategy : BaseDrawingStrategy
        {
            public ClusterDrawingStrategy(DeepSkyRenderer renderer) : base(renderer) { }

            public override void Draw(Graphics g, DeepSky ds)
            {
                if (Renderer.Map.ViewAngle <= Renderer.minZoom)
                {
                    base.Draw(g, ds);
                }
            }

            protected override void DrawEllipticObject(Graphics g, float diamA, float diamB)
            {
                g.DrawEllipse(Renderer.penCluster, -diamA / 2, -diamB / 2, diamA, diamB);
            }

            protected override void DrawPointObject(Graphics g)
            {
                g.FillEllipse(Brushes.DimGray, -1, -1, 2, 2);
            }

            protected override void DrawRoundObject(Graphics g, float diamA)
            {
                g.DrawEllipse(Renderer.penCluster, -diamA / 2, -diamA / 2, diamA, diamA);
            }
        }

        private class EmptyDrawingStrategy : IDrawingStrategy
        {
            protected DeepSkyRenderer Renderer { get; private set; }

            public EmptyDrawingStrategy(DeepSkyRenderer renderer)
            {
                Renderer = renderer;
            }

            public void Draw(Graphics g, DeepSky ds)
            {
                // Do nothing
            }
        }

        private abstract class BaseDrawingStrategy : IDrawingStrategy
        {
            protected DeepSkyRenderer Renderer { get; private set; }

            protected abstract void DrawEllipticObject(Graphics g, float diamA, float diamB);
            protected abstract void DrawRoundObject(Graphics g, float diamA);
            protected abstract void DrawPointObject(Graphics g);


            public BaseDrawingStrategy(DeepSkyRenderer renderer)
            {
                Renderer = renderer;
            }

            public virtual void Draw(Graphics g, DeepSky ds)
            {
                PointF p = Renderer.Map.Projection.Project(ds.Horizontal);

                float sizeA = GetDiameter(ds.SizeA);
                float sizeB = GetDiameter(ds.SizeB);

                // elliptic object with known size
                if (sizeB > 0)
                {
                    float diamA = GetDiameter(ds.SizeA);
                    if (diamA > 10)
                    {
                        float diamB = GetDiameter(ds.SizeB);
                        if (ds.Outline != null)
                        {
                            DrawOutline(g, ds.Outline);
                        }
                        else
                        {
                            float rotation = Renderer.GetRotationTowardsNorth(ds.Equatorial) + 90 - ds.PA;
                            g.TranslateTransform(p.X, p.Y);
                            g.RotateTransform(rotation);
                            DrawEllipticObject(g, diamA, diamB);
                            g.ResetTransform();
                        }
                        Renderer.Map.AddDrawnObject(ds, p);

                        if (Renderer.Map.ViewAngle <= Renderer.limitLabels)
                        {
                            Renderer.DrawObjectCaption(g, Renderer.fontCaption, Renderer.brushCaption, ds.DisplayName, p, Math.Min(diamA, diamB));
                        }
                    }
                }
                // round object
                else if (sizeA > 0)
                {
                    float diamA = GetDiameter(ds.SizeA);
                    if (diamA > 10)
                    {
                        if (ds.Outline != null)
                        {
                            DrawOutline(g, ds.Outline);
                        }
                        else
                        {
                            float rotation = Renderer.GetRotationTowardsNorth(ds.Equatorial) + 90 - ds.PA;
                            g.TranslateTransform(p.X, p.Y);
                            g.RotateTransform(rotation);
                            DrawRoundObject(g, diamA);
                            g.ResetTransform();
                        }
                        Renderer.Map.AddDrawnObject(ds, p);

                        if (Renderer.Map.ViewAngle <= Renderer.limitLabels)
                        {
                            Renderer.DrawObjectCaption(g, Renderer.fontCaption, Renderer.brushCaption, ds.DisplayName, p, diamA);
                        }
                    }
                }
                // point object
                else
                {
                    if (ds.Outline != null)
                    {
                        DrawOutline(g, ds.Outline);
                    }
                    else
                    {
                        g.TranslateTransform(p.X, p.Y);
                        DrawPointObject(g);
                        g.ResetTransform();
                    }
                    Renderer.Map.AddDrawnObject(ds, p);

                    if (Renderer.Map.ViewAngle <= Renderer.limitLabels)
                    {
                        Renderer.DrawObjectCaption(g, Renderer.fontCaption, Renderer.brushCaption, ds.DisplayName, p, 0);
                    }
                }
            }

            protected virtual void DrawOutline(Graphics g, ICollection<CelestialPoint> outline)
            {
                for (int i = 0; i < outline.Count - 1; i++)
                {
                    var h1 = outline.ElementAt(i).Horizontal;
                    var h2 = outline.ElementAt(i + 1).Horizontal;

                    double ad1 = Angle.Separation(h1, Renderer.Map.Center);
                    double ad2 = Angle.Separation(h2, Renderer.Map.Center);
                    PointF p1, p2;
                    if (ad1 < Renderer.Map.ViewAngle * 1.2 || 
                        ad2 < Renderer.Map.ViewAngle * 1.2)
                    {
                        p1 = Renderer.Map.Projection.Project(h1);
                        p2 = Renderer.Map.Projection.Project(h2);
                        g.DrawLine(Renderer.penCluster, p1, p2);
                    }
                }
            }

            private float GetDiameter(double diam)
            {
                return (float)(diam / 60 / Renderer.Map.ViewAngle * Renderer.Map.Width / 2);
            }
        }
    }
}
