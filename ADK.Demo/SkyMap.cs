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
        public ICollection<PointF> DrawnPoints { get; } = new List<PointF>();
        public ICollection<RectangleF> Labels { get; } = new List<RectangleF>();
        public CelestialObject SelectedObject { get; set; }
        public IProjection Projection { get; set; } = null;
        public event Action OnInvalidate;

        /// <summary>
        /// Collection of celestial objects drawn on the map
        /// </summary>
        private ICollection<CelestialObject> drawnObjects = new List<CelestialObject>();

        public SkyMap()
        {
            Projection = new ArcProjection(this);
        }

        public void Render(Graphics g)
        {
            g.PageUnit = GraphicsUnit.Display;
            g.SmoothingMode = Antialias ? SmoothingMode.HighQuality : SmoothingMode.HighSpeed;
            DrawnPoints.Clear();
            drawnObjects.Clear();
            Labels.Clear();

            bool needDrawSelectedObject = true;

            foreach (var renderer in Renderers)
            {
                renderer.Render(g);
                if (needDrawSelectedObject)
                {
                    needDrawSelectedObject = !DrawSelectedObject(g);
                }
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

            foreach (var body in drawnObjects.OrderBy(c => Angle.Separation(hor, c.Horizontal)))
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

        public void AddDrawnObject(CelestialObject obj, PointF p)
        {
            drawnObjects.Add(obj);
            DrawnPoints.Add(p);
        }

        private bool DrawSelectedObject(Graphics g)
        {           
            // screen diagonal, in pixels
            double diag = Math.Sqrt(Width * Width / 4 + Height * Height / 4);

            if (SelectedObject != null && drawnObjects.Contains(SelectedObject))
            {
                var body = SelectedObject;

                double sd = (body is SizeableCelestialObject) ?
                    (body as SizeableCelestialObject).Semidiameter : 0;

                double size = Math.Max(10, sd / 3600.0 / ViewAngle * Width);

                // do not draw selection circle if image is too large
                bool drawCircle = true; // diam / 2 < diag;

                if (drawCircle)
                {
                    PointF p = Projection.Project(body.Horizontal);
                    Pen pen = new Pen(Brushes.DarkRed, 2);
                    pen.DashStyle = DashStyle.Dash;

                    g.DrawEllipse(pen, (float)(p.X - (size + 6) / 2), (float)(p.Y - (size + 6) / 2), (float)(size + 6), (float)(size + 6));

                    return true;
                }
            }

            return false;
        }
    }
}
