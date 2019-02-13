using ADK.Demo.Objects;
using ADK.Demo.Projections;
using ADK.Demo.Renderers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo
{
    public class SkyMap : ISkyMap
    {
        /// <summary>
        /// Stopwatch to measure rendering time
        /// </summary>
        private Stopwatch renderStopWatch = new Stopwatch();

        /// <summary>
        /// Mean rendering time, in milliseconds
        /// </summary>
        private double meanRenderTime = 0;

        /// <summary>
        /// Total count of calls of <see cref="Render(Graphics)"/> method.
        /// Needed for calculating mean rendering time.
        /// </summary>
        private long rendersCount = 0;

        /// <summary>
        /// Collection of points of celestial bodies centers drawn on the map
        /// </summary>
        private ICollection<PointF> drawnPoints = new List<PointF>();

        /// <summary>
        /// Collection of bounding rectangles of labels displayed on the map
        /// </summary>
        private ICollection<RectangleF> labels = new List<RectangleF>();

        private readonly List<IRenderer> renderers = new List<IRenderer>();

        public int Width { get; set; }
        public int Height { get; set; }
        public double ViewAngle { get; set; } = 90;
        public CrdsHorizontal Center { get; } = new CrdsHorizontal(0, 0);
        public bool Antialias { get; set; } = true;

        public CelestialObject SelectedObject { get; set; }
        public IProjection Projection { get; set; } = null;
        public event Action OnInvalidate;

        /// <summary>
        /// Collection of celestial objects drawn on the map
        /// </summary>
        private ICollection<CelestialObject> drawnObjects = new List<CelestialObject>();

        private MapContext mapContext = null;

        public SkyMap(SkyContext skyContext, ICollection<IRenderer> renderers)
        {
            Projection = new ArcProjection(this);

            this.renderers.AddRange(renderers);
            this.mapContext = new MapContext(this, skyContext);
        }

        public void Render(Graphics g)
        {
            renderStopWatch.Restart();

            g.PageUnit = GraphicsUnit.Display;
            g.SmoothingMode = Antialias ? SmoothingMode.HighQuality : SmoothingMode.HighSpeed;
            drawnPoints.Clear();
            drawnObjects.Clear();
            labels.Clear();

            bool needDrawSelectedObject = true;

            mapContext.Graphics = g;

            foreach (var renderer in renderers)
            {
                renderer.Render(mapContext);
                if (needDrawSelectedObject)
                {
                    needDrawSelectedObject = !DrawSelectedObject(g);
                }
            }

            renderStopWatch.Stop();
            rendersCount++;

            // Calculate mean time of rendering with Cumulative Moving Average formula
            meanRenderTime = (renderStopWatch.ElapsedMilliseconds + rendersCount * meanRenderTime) / (rendersCount + 1);
        }

        public void Initialize()
        {
            foreach (var renderer in renderers)
            {
                renderer.Initialize();
            }
        }

        private void Invalidate()
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

        public void GoToObject(CelestialObject body, TimeSpan animationDuration)
        {
            double sd = (body is SizeableCelestialObject) ?
                        (body as SizeableCelestialObject).Semidiameter / 3600 : 0;

            double viewAngleTarget = sd == 0 ? 1 : sd * 10;

            if (animationDuration.Equals(TimeSpan.Zero))
            {
                Center.Set(body.Horizontal);
                ViewAngle = viewAngleTarget;
                Invalidate();
            }
            else
            {
                CrdsHorizontal centerOriginal = new CrdsHorizontal(Center);
                double ad = Angle.Separation(body.Horizontal, centerOriginal);
                double steps = Math.Round(animationDuration.TotalMilliseconds / meanRenderTime);
                double[] x = new double[] { 0, steps / 2, steps };
                double[] y = (ad < ViewAngle) ?
                    // linear zooming if body is already on the screen:
                    new double[] { ViewAngle, (ViewAngle + viewAngleTarget) / 2, viewAngleTarget } :
                    // parabolic zooming with jumping to 90 degrees view angle at the middle of path:
                    new double[] { ViewAngle, 90, viewAngleTarget };

                for (int i = 0; i <= steps; i++)
                {
                    Center.Set(Angle.Intermediate(centerOriginal, body.Horizontal, i / steps));
                    ViewAngle = Math.Min(90, Interpolation.Lagrange(x, y, i));
                    Invalidate();
                }
            }            
        }

        public void AddDrawnObject(CelestialObject obj, PointF p)
        {
            drawnObjects.Add(obj);
            drawnPoints.Add(p);
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

        private class MapContext : IMapContext
        {
            private readonly SkyMap map;
            private readonly SkyContext skyContext;

            public MapContext(SkyMap map, SkyContext skyContext)
            {                
                this.map = map;
                this.skyContext = skyContext;
            }

            public Graphics Graphics { get; set; }
            public int Width => map.Width;
            public int Height => map.Height;
            public double ViewAngle => map.ViewAngle;
            public CrdsHorizontal Center => map.Center;
            public double JulianDay => skyContext.JulianDay;
            public double Epsilon => skyContext.Epsilon;
            public CrdsGeographical GeoLocation => skyContext.GeoLocation;
            public double SiderealTime => skyContext.SiderealTime;

            public PointF Project(CrdsHorizontal hor)
            {
                return map.Projection.Project(hor);
            }

            public void AddDrawnObject(CelestialObject obj, PointF p)
            {
                map.AddDrawnObject(obj, p);
            }

            public void DrawObjectCaption(Font font, Brush brush, string caption, PointF p, float size)
            {
                SizeF b = Graphics.MeasureString(caption, font);

                float s = size > 5 ? (size / 2.8284f + 2) : 1;
                for (int x = 0; x < 2; x++)
                {
                    for (int y = 0; y < 2; y++)
                    {
                        float dx = x == 0 ? s : -s - b.Width;
                        float dy = y == 0 ? s : -s - b.Height;
                        RectangleF r = new RectangleF(p.X + dx, p.Y + dy, b.Width, b.Height);
                        if (!map.labels.Any(l => l.IntersectsWith(r)) && !map.drawnPoints.Any(v => r.Contains(v)))
                        {
                            Graphics.DrawString(caption, font, brush, r.Location);
                            map.labels.Add(r);
                            return;
                        }
                    }
                }
            }

            public void Redraw()
            {
                map.Invalidate();
            }
        }
    }
}
