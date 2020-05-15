using Astrarium.Algorithms;
using Astrarium.Projections;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace Astrarium
{
    public class SkyMap : ISkyMap
    {
        /// <summary>
        /// Minimal allowed field of view, in degrees
        /// </summary>
        private const double MIN_VIEW_ANGLE = 1.0 / 1024;

        /// <summary>
        /// Max allowed field of view, in degrees
        /// </summary>
        private const double MAX_VIEW_ANGLE = 90;

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
        /// Collection of bounding rectangles of labels displayed on the map
        /// </summary>
        private ICollection<RectangleF> labels = new List<RectangleF>();

        /// <summary>
        /// Collection of renderers
        /// </summary>
        private readonly RenderersCollection renderers = new RenderersCollection();

        /// <summary>
        /// Font used to display diagnostic info
        /// </summary>
        private Font fontDiagnosticText = new Font("Monospace", 8);

        /// <summary>
        /// Font used to display "Object is locked" message
        /// </summary>
        private Font fontLockMessage = new Font("Arial", 8);

        public int Width { get; set; }
        public int Height { get; set; }

        /// <summary>
        /// Backing field for <see cref="ViewAngle"/> property
        /// </summary>
        private double viewAngle = 90;

        /// <summary>
        /// Gets or sets current FOV of the map
        /// </summary>
        public double ViewAngle
        {
            get
            {
                return viewAngle;
            }
            set
            {
                viewAngle = value;
                if (value >= MAX_VIEW_ANGLE)
                {
                    viewAngle = MAX_VIEW_ANGLE;
                }
                if (value < MIN_VIEW_ANGLE)
                {
                    viewAngle = MIN_VIEW_ANGLE;
                }

                ViewAngleChanged?.Invoke(viewAngle);
                Invalidate();
            }
        }

        /// <summary>
        /// Gets magnitude limit depending on current field of view (zoom level).
        /// </summary>
        /// <param name="map">IMapContext instance</param>
        /// <returns>Magnitude limit</returns>
        /// <remarks>
        /// This method based on empiric formula, coefficients found with https://www.wolframalpha.com
        /// </remarks>
        public float MagLimit
        {
            get
            {
                // log fit {90,6},{45,6.5},{20,7.3},{8,9},{4,10.5}
                return (float)(-1.44995 * Math.Log(0.000230685 * ViewAngle));
            }
        }

        /// <summary>
        /// Occurs when map's View Angle is changed.
        /// </summary>
        public event Action<double> ViewAngleChanged;

        public CrdsHorizontal Center { get; } = new CrdsHorizontal(0, 0);
        public bool Antialias { get; set; } = true;
       
        private CelestialObject selectedObject;
        public CelestialObject SelectedObject
        {
            get
            {
                return selectedObject;
            }
            set
            {
                if (value != selectedObject)
                {
                    selectedObject = value;
                    SelectedObjectChanged?.Invoke(selectedObject);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedObject)));
                }
            }
        }

        /// <summary>
        /// Locked Object. If it set, map moving is denied and it always centered on this body. 
        /// </summary>
        public CelestialObject LockedObject { get; set; }

        /// <summary>
        /// Backing field for <see cref="MousePosition"/> property.
        /// </summary>
        private CrdsHorizontal mousePosition = new CrdsHorizontal(0, 0);

        /// <summary>
        /// Last result of needRedraw flag
        /// </summary>
        private bool lastNeedRedraw = false;

        /// <summary>
        /// Gets or sets current coordinates of mouse, converted to Horizontal coordinates on the map.
        /// Setting new value can raise redraw of map
        /// </summary>
        public CrdsHorizontal MousePosition 
        { 
            get { return mousePosition; } 
            set 
            { 
                mousePosition = value;
                bool needRedraw = renderers.Any(r => r.OnMouseMove(mousePosition, MouseButton));
                if (MouseButton == MouseButton.None && (needRedraw || lastNeedRedraw))
                {
                    lastNeedRedraw = needRedraw;
                    Invalidate();
                }
            }
        }

        public MouseButton MouseButton { get; set; }

        /// <summary>
        /// Occurs when selected celestial object is changed
        /// </summary>
        public event Action<CelestialObject> SelectedObjectChanged;

        /// <summary>
        /// Projection used to render the map
        /// </summary>
        public IProjection Projection { get; set; } = null;
              
        public event Action OnInvalidate;

        public event Action OnRedraw;
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Collection of celestial objects drawn on the map
        /// </summary>
        private ICollection<CelestialObject> drawnObjects = new List<CelestialObject>();

        /// <summary>
        /// <see cref="MapContext"/> instance
        /// </summary>
        private MapContext mapContext = null;

        private ISettings settings = null;

        public SkyMap(ISettings settings)
        {
            this.settings = settings;
        }

        public void Initialize(SkyContext skyContext, ICollection<BaseRenderer> renderers)
        {
            mapContext = new MapContext(this, skyContext);

            Projection = new ArcProjection(mapContext);

            this.renderers.AddRange(renderers);
            this.renderers.ForEach(r => r.Initialize());

            Schema = settings.Get<ColorSchema>("Schema");

            // get saved rendering orders
            RenderingOrder renderingOrder = settings.Get<RenderingOrder>("RenderingOrder");

            // sort renderers according saving orders
            this.renderers.Sort(renderingOrder.Select(r => r.RendererTypeName));

            // build rendering order based on existing renderers
            renderingOrder = new RenderingOrder(this.renderers.Select(r => new RenderingOrderItem(r)));

            // save actual rendering order
            settings.Set("RenderingOrder", renderingOrder);
            
            settings.SettingValueChanged += (name, value) =>
            {
                // redraw if rendering order changed
                if (name == "RenderingOrder")
                {
                    this.renderers.Sort(settings.Get<RenderingOrder>("RenderingOrder").Select(r => r.RendererTypeName));
                    Invalidate();
                }

                if (name == "Schema")
                {
                    Schema = settings.Get<ColorSchema>("Schema");
                    Invalidate();
                }
            };
        }

        public ColorSchema Schema { get; private set; } = ColorSchema.Night;

        public void Render(Graphics g)
        {
            try
            {
                renderStopWatch.Restart();

                mapContext.Graphics = g;

                g.Clear(mapContext.GetSkyColor());
                g.PageUnit = GraphicsUnit.Display;
                g.SmoothingMode = Antialias ? SmoothingMode.HighQuality : SmoothingMode.HighSpeed;
                drawnObjects.Clear();
                labels.Clear();

                bool needDrawSelectedObject = true;

                if (LockedObject != null)
                {
                    Center.Altitude = LockedObject.Horizontal.Altitude;
                    Center.Azimuth = LockedObject.Horizontal.Azimuth;
                }

                for (int i = 0; i < renderers.Count(); i++)
                {
                    try
                    {
                        renderers.ElementAt(i).Render(mapContext);
                    }
                    catch (Exception ex)
                    {
                        g.DrawString($"Error:\n{ex}", fontDiagnosticText, Brushes.Red, new RectangleF(10, 10, Width - 20, Height - 20));
                    }
                    if (needDrawSelectedObject)
                    {
                        needDrawSelectedObject = !DrawSelectedObject(g);
                    }
                }

                renderStopWatch.Stop();
                rendersCount++;

                int fps = (int)(1000f / renderStopWatch.ElapsedMilliseconds);

                // Calculate mean time of rendering with Cumulative Moving Average formula
                meanRenderTime = (renderStopWatch.ElapsedMilliseconds + rendersCount * meanRenderTime) / (rendersCount + 1);

                // Locked object
                if (LockedObject != null && MouseButton == MouseButton.Left)
                {
                    var format = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    string text = $"Map is locked on {LockedObject.Names.First()}";

                    PointF center = new PointF(Width / 2, Height / 2);
                    var size = g.MeasureString(text, fontLockMessage, center, format);
                    int margin = 4;
                    var box = new Rectangle((int)(center.X - size.Width / 2 - margin), (int)(center.Y - size.Height / 2 - margin), (int)size.Width + 2 * margin, (int)size.Height + 2 * margin);

                    g.FillRectangle(new SolidBrush(mapContext.GetColor(Color.Black)), box);
                    g.DrawRectangle(new Pen(Color.FromArgb(100, mapContext.GetColor(Color.White))), box);
                    g.DrawString(text, fontLockMessage, new SolidBrush(mapContext.GetColor(Color.White)), center, format);
                }

                // Diagnostic info
                g.DrawString($"FOV: {Formatters.Angle.Format(ViewAngle)}\nMag limit: {Formatters.Magnitude.Format(MagLimit)}\nFPS: {fps}\nDaylight factor: {mapContext.DayLightFactor:F2}", fontDiagnosticText, Brushes.Red, new PointF(10, 10));
            }
            catch (Exception ex)
            {
                g.DrawString($"Error:\n{ex}", fontDiagnosticText, Brushes.Red, new RectangleF(10, 10, Width - 20, Height - 20));
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

                if (mapContext.DistanceBetweenPoints(p, point) <= size / 2)
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

            double viewAngleTarget = sd == 0 ? 1 : Math.Max(sd * 10, MIN_VIEW_ANGLE);

            if (animationDuration.Equals(TimeSpan.Zero))
            {
                Center.Set(body.Horizontal);
                ViewAngle = viewAngleTarget;
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
                }
            }            
        }

        public void AddDrawnObject(CelestialObject obj)
        {
            drawnObjects.Add(obj);
        }

        private bool DrawSelectedObject(Graphics g)
        {
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
            public float MagLimit => map.MagLimit;
            public CrdsHorizontal Center => map.Center;
            public double JulianDay => skyContext.JulianDay;
            public double Epsilon => skyContext.Epsilon;
            public CrdsGeographical GeoLocation => skyContext.GeoLocation;
            public double SiderealTime => skyContext.SiderealTime;
            public float DayLightFactor => skyContext.DayLightFactor;
            public ColorSchema Schema => map.Schema;
            public CrdsHorizontal MousePosition => map.MousePosition;
            public MouseButton MouseButton => map.MouseButton;
            public CelestialObject LockedObject => map.LockedObject;
            public CelestialObject SelectedObject => map.SelectedObject;

            public PointF Project(CrdsHorizontal hor)
            {
                return map.Projection.Project(hor);
            }

            public void AddDrawnObject(CelestialObject obj)
            {
                map.AddDrawnObject(obj);
            }

            public void DrawObjectCaption(Font font, Brush brush, string caption, PointF p, float size, StringFormat format = null)
            {
                if (format != null)
                {
                    SizeF b = Graphics.MeasureString(caption, font, p, format);
                    RectangleF r = new RectangleF(p.X, p.Y, b.Width, b.Height);

                    if (format.Alignment == StringAlignment.Center)
                        r.X = p.X - b.Width / 2;
                    if (format.Alignment == StringAlignment.Far)
                        r.X = p.X - b.Width / 2;

                    if (format.LineAlignment == StringAlignment.Center)
                        r.Y = p.Y - b.Height / 2;
                    if (format.LineAlignment == StringAlignment.Far)
                        r.Y = p.Y - b.Height / 2;

                    Graphics.DrawString(caption, font, brush, p, format);
                    map.labels.Add(r);
                }
                else
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
                            if (!map.labels.Any(l => l.IntersectsWith(r)))
                            {
                                Graphics.DrawString(caption, font, brush, r.Location);
                                map.labels.Add(r);
                                return;
                            }
                        }
                    }
                }
            }

            public Color GetColor(string colorName)
            {
                return GetColor(map.settings.Get<Color>(colorName));
            }

            public Color GetColor(Color colorNight)
            {
                switch (Schema)
                {
                    default:
                    case ColorSchema.Night:
                        return colorNight;
                    case ColorSchema.Red:
                        return GetNightModeColor(colorNight);
                    case ColorSchema.White:
                        return GetWhiteMapColor(colorNight);
                    case ColorSchema.Day:
                        return GetIntermediateColor(DayLightFactor, colorNight, GetDaylightColor(colorNight));
                }
            }

            public Color GetColor(Color colorNight, Color colorDay)
            {
                switch (Schema)
                {
                    default:
                    case ColorSchema.Night:
                        return colorNight;
                    case ColorSchema.Red:
                        return GetNightModeColor(colorNight);
                    case ColorSchema.White:
                        return GetWhiteMapColor(colorNight);
                    case ColorSchema.Day:
                        return GetIntermediateColor(DayLightFactor, colorNight, colorDay);
                }
            }

            private Color GetNightModeColor(Color night)
            {
                int brightness = GetBrightness(night);
                return Color.FromArgb(night.A, brightness, 0, 0);
            }

            private Color GetWhiteMapColor(Color night)
            {
                int brightness = 255 - GetBrightness(night);
                return Color.FromArgb(night.A, brightness, brightness, brightness);
            }

            public Color GetSkyColor()
            {
                return GetColor(Color.Black);
            }

            private Color COLOR_DAY_SKY = Color.FromArgb(116, 184, 255);

            private Color GetDaylightColor(Color night)
            {
                float brightness = GetBrightness(night) / 255f;

                return Color.FromArgb(
                    (int)(COLOR_DAY_SKY.R + brightness * (255 - COLOR_DAY_SKY.R)),
                    (int)(COLOR_DAY_SKY.G + brightness * (255 - COLOR_DAY_SKY.G)),
                    (int)(COLOR_DAY_SKY.B + brightness * (255 - COLOR_DAY_SKY.B))
                    );
            }

            private int GetBrightness(Color night)
            {
                return (int)(0.299 * night.R + 0.587 * night.G + 0.114 * night.B);
            }

            private Color GetIntermediateColor(float factor, Color from, Color to)
            {
                if (factor == 0)
                    return from;
                else if (factor == 1)
                    return to;
                else
                {
                    int rMax = to.R;
                    int rMin = from.R;
                    int gMax = to.G;
                    int gMin = from.G;
                    int bMax = to.B;
                    int bMin = from.B;
                    int aMax = to.A;
                    int aMin = from.A;

                    int a = aMin + (int)((aMax - aMin) * factor);
                    int r = rMin + (int)((rMax - rMin) * factor);
                    int g = gMin + (int)((gMax - gMin) * factor);
                    int b = bMin + (int)((bMax - bMin) * factor);

                    return Color.FromArgb(a, r, g, b);
                }
            }

            public void Redraw()
            {
                map.Invalidate();
                map.OnRedraw();
            }
        }
    }
}
