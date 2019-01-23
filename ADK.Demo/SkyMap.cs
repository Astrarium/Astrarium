using ADK.Demo.Objects;
using ADK.Demo.Projections;
using ADK.Demo.Renderers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo
{
    public class SkyMap : ISkyMap
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public double ViewAngle { get; set; } = 90;
        public CrdsHorizontal Center { get; set; } = new CrdsHorizontal(0, 0);
        public bool Antialias { get; set; } = true;
        public ICollection<BaseSkyRenderer> Renderers { get; } = new List<BaseSkyRenderer>();
        public ICollection<CelestialObject> VisibleObjects { get; } = new List<CelestialObject>();
        public CelestialObject SelectedObject { get; set; }
        public IProjection Projection { get; set; } = null;
        public event Action OnInvalidate;

        public SkyMap()
        {
            Projection = new ArcProjection(this);
        }

        public void Render(Graphics g)
        {
            g.PageUnit = GraphicsUnit.Display;
            g.SmoothingMode = Antialias ? SmoothingMode.HighQuality : SmoothingMode.HighSpeed;
            VisibleObjects.Clear();
            foreach (var renderer in Renderers)
            {
                renderer.Render(g);
            }
        }

        public void Initialize()
        {
            foreach (var renderer in Renderers)
            {
                renderer.Initialize();
            }
        }

        public void Invalidate()
        {
            OnInvalidate?.Invoke();
        }

        public CelestialObject FindObject(PointF point)
        {
            var hor = Projection.Invert(point);
            var body = VisibleObjects
                .OrderBy(c => Angle.Separation(hor, c.Horizontal))
                .FirstOrDefault();

            if (body != null)
            {
                double sd = (body is SizeableCelestialObject) ?
                    (body as SizeableCelestialObject).Semidiameter : 0;

                double size = Math.Max(10, sd / 3600.0 / ViewAngle * Width);

                PointF p = Projection.Project(body.Horizontal);

                if (Geometry.DistanceBetweenPoints(p, point) <= size / 2)
                {
                    return body;
                }
            }

            return null;
        }
    }
}
