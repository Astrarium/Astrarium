﻿using Astrarium.Algorithms;
using Astrarium.Projections;
using Astrarium.Types;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading;

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

        private Font fontMapInformationText = new Font("Arial", 14);

        private Font fontMag = new Font("Arial", 8);

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
                timeSyncEvent.Set();
                Invalidate();
            }
        }

        private bool dateTimeSync = false;
        public bool TimeSync
        {
            get => dateTimeSync;
            set
            {
                dateTimeSync = value;

                if (value)
                {
                    timeSyncEvent.Set();
                    timeSyncResetEvent.Set();
                }
                else
                {
                    timeSyncEvent.Set();
                    timeSyncResetEvent.Reset();
                }
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
                // no limit
                float limitMag = float.MaxValue;

                // log fit {90,6},{45,7},{8,9},{1,12},{0.25,17}
                return Math.Min(limitMag, (float)(-1.73494 * Math.Log(0.000462398 * ViewAngle)));

                // OLD formula:
                // log fit {90,6},{45,6.5},{20,7.3},{8,9},{4,10.5}
                // return (float)(-1.44995 * Math.Log(0.000230685 * ViewAngle));
            }
        }

        /// <summary>
        /// Occurs when map's View Angle is changed.
        /// </summary>
        public event Action<double> ViewAngleChanged;

        public float DaylightFactor { get; set; }
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

        private PointF mouseCoordinates;

        public PointF MouseCoordinates
        {
            get => mouseCoordinates;
            set
            {
                mouseCoordinates = value;
                bool needRedraw = renderers.Any(r => r.OnMouseMove(this, mouseCoordinates, MouseButton));
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


        private Projection projection;

        public Projection SkyProjection => projection;

        [Obsolete]
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
        /// Collection of celestial objects drawn on the map
        /// </summary>
        private ICollection<Tuple<CelestialObject, PointF, float>> celestialObjects = new List<Tuple<CelestialObject, PointF, float>>();

        /// <summary>
        /// <see cref="MapContext"/> instance
        /// </summary>
        private MapContext mapContext = null;

        /// <summary>
        /// Application settings
        /// </summary>
        private ISettings settings = null;

        /// <summary>
        /// Command line args
        /// </summary>
        private ICommandLineArgs commandLineArgs = null;

        /// <summary>
        /// Texture manager instance
        /// </summary>
        private ITextureManager textureManager = null;

        public SkyMap(ITextureManager textureManager, ISettings settings, ICommandLineArgs commandLineArgs)
        {
            this.textureManager = textureManager;
            this.settings = settings;
            this.commandLineArgs = commandLineArgs;
        }

        public SkyContext Context { get; private set; }

        public void Initialize(SkyContext skyContext, ICollection<BaseRenderer> renderers)
        {
            Context = new SkyContext(skyContext.JulianDay, skyContext.GeoLocation);
            mapContext = new MapContext(this, skyContext);

            projection = Types.Projection.Create<StereographicProjection>(Context);
            projection.Fov = 90;
            projection.SetVision(new CrdsHorizontal(0, 0));
            projection.FlipVertical = !settings.Get("IsInverted");
            projection.FlipHorizontal = settings.Get("IsMirrored");

            Projection = new ArcProjection(mapContext);
            Projection.IsInverted = settings.Get("IsInverted");
            Projection.IsMirrored = settings.Get("IsMirrored");

            this.renderers.AddRange(renderers);
            this.renderers.ForEach(r => r.Initialize());

            Schema = settings.Get<ColorSchema>("Schema");

            // get saved rendering orders
            RenderingOrder renderingOrder = settings.Get<RenderingOrder>("RenderingOrder");

            // sort renderers according saved orders
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

                if (name == "IsMirrored")
                {
                    SkyProjection.FlipHorizontal = settings.Get("IsMirrored");
                    Invalidate();
                }

                if (name == "IsInverted")
                {
                    SkyProjection.FlipVertical = !settings.Get("IsInverted");
                    Invalidate();
                }
            };

            new Thread(TimeSyncWorker) { IsBackground = true }.Start();
        }

        private ManualResetEvent timeSyncResetEvent = new ManualResetEvent(false);
        private ManualResetEvent timeSyncEvent = new ManualResetEvent(false);

        private void TimeSyncWorker()
        {
            while (true)
            {
                timeSyncEvent.WaitOne();
                timeSyncResetEvent.WaitOne();
                double rate = Math.Min(5000, Math.Max(100, SkyProjection.Fov * 100));
                Context.JulianDay = new Date(DateTime.Now).ToJulianEphemerisDay();
                Invalidate();
                Thread.Sleep((int)rate);
            }
        }

        public ColorSchema Schema { get; private set; } = ColorSchema.Night;

        public void Render()
        {
            textureManager.Cleanup();
            celestialObjects.Clear();
            labels.Clear();

            for (int i = 0; i < renderers.Count(); i++)
            {
                try
                {
                    renderers.ElementAt(i).Render(this);
                }
                catch (Exception ex)
                {
                    Log.Error($"Rendering error: {ex}");
                }
            }

            DrawSelectedObject();
        }

        private void DrawSelectedObject()
        {
            if (SelectedObject != null && celestialObjects.Any())
            {
                var bodyAndPosition = celestialObjects.FirstOrDefault(x => x.Item1.Equals(SelectedObject));

                if (bodyAndPosition != null)
                {
                    PointF pos = bodyAndPosition.Item2;
                    CelestialObject body = bodyAndPosition.Item1;

                    double sd = (body is SizeableCelestialObject) ? (body as SizeableCelestialObject).Semidiameter : 0;

                    float mag = (body is IMagnitudeObject) ? (body as IMagnitudeObject).Magnitude : projection.MagLimit;

                    double diskSize = projection.GetDiskSize(sd, 10);
                    double pointSize = projection.GetPointSize(double.IsNaN(mag) ? projection.MagLimit : mag);

                    double size = Math.Max(diskSize, pointSize);

                    Vec2 p = new Vec2(pos.X, pos.Y);

                    Primitives.DrawEllipse(p, Pens.Red, (size + 8) / 2);
                }
            }
        }

        public void Render(Graphics g)
        {

        }

        public void Invalidate()
        {
            if (!renderStopWatch.IsRunning)
            {
                OnInvalidate?.Invoke();
            }
        }

        public CelestialObject FindObject(PointF point)
        {
            /*
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
            */

            foreach (var x in celestialObjects.OrderBy(c => (point.X - c.Item2.X) * (point.X - c.Item2.X) + (projection.ScreenHeight - point.Y - c.Item2.Y) * (projection.ScreenHeight - point.Y - c.Item2.Y)))
            {
                double sd = (x.Item1 is SizeableCelestialObject) ?
                    (x.Item1 as SizeableCelestialObject).Semidiameter : 0;

                double size = projection.GetDiskSize(sd, 10);

                if (Math.Sqrt((x.Item2.X - point.X) * (x.Item2.X - point.X) + (projection.ScreenHeight - x.Item2.Y - point.Y) * (projection.ScreenHeight - x.Item2.Y - point.Y)) < size / 2)
                {
                    return x.Item1;
                }
            }

            return null;
        }

        public void GoToObject(CelestialObject body, TimeSpan animationDuration)
        {
            double sd = (body is SizeableCelestialObject) ?
                        (body as SizeableCelestialObject).Semidiameter / 3600 : 0;

            double viewAngleTarget = sd == 0 ? 1 : Math.Max(sd * 10, MIN_VIEW_ANGLE);

            GoToPoint(body.Horizontal, animationDuration, viewAngleTarget);
        }

        public void GoToObject(CelestialObject body, double viewAngleTarget)
        {
            GoToPoint(body.Horizontal, TimeSpan.Zero, viewAngleTarget);
        }

        public void GoToObject(CelestialObject body, TimeSpan animationDuration, double viewAngleTarget)
        {
            GoToPoint(body.Horizontal, animationDuration, viewAngleTarget);
        }

        public void GoToPoint(CrdsHorizontal hor, TimeSpan animationDuration)
        {
            GoToPoint(hor, animationDuration, Math.Min(viewAngle, 90));
        }

        public void GoToPoint(CrdsHorizontal hor, double viewAngleTarget)
        {
            GoToPoint(hor, TimeSpan.Zero, viewAngleTarget);
        }

        public void GoToPoint(CrdsHorizontal hor, TimeSpan animationDuration, double viewAngleTarget)
        {
            if (viewAngleTarget == 0)
            {
                viewAngleTarget = ViewAngle;
            }

            if (animationDuration.Equals(TimeSpan.Zero))
            {
                Center.Set(hor);
                ViewAngle = viewAngleTarget;
            }
            else
            {
                CrdsHorizontal centerOriginal = new CrdsHorizontal(Center);
                double ad = Angle.Separation(hor, centerOriginal);
                double steps = Math.Ceiling(animationDuration.TotalMilliseconds / meanRenderTime);
                double[] x = new double[] { 0, steps / 2, steps };
                double[] y = (ad < ViewAngle) ?
                    // linear zooming if body is already on the screen:
                    new double[] { ViewAngle, (ViewAngle + viewAngleTarget) / 2, viewAngleTarget } :
                    // parabolic zooming with jumping to 90 degrees view angle at the middle of path:
                    new double[] { ViewAngle, 90, viewAngleTarget };

                for (int i = 0; i <= steps; i++)
                {
                    Center.Set(Angle.Intermediate(centerOriginal, hor, i / steps));
                    ViewAngle = Math.Min(90, Interpolation.Lagrange(x, y, i));
                }
            }
        }

        [Obsolete]
        public void AddDrawnObject(CelestialObject obj)
        {
            drawnObjects.Add(obj);
        }

        public void AddDrawnObject(PointF p, CelestialObject obj, float size)
        {
            celestialObjects.Add(new Tuple<CelestialObject, PointF, float>(obj, p, size));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="textRenderer"></param>
        /// <param name="label"></param>
        /// <param name="font"></param>
        /// <param name="brush"></param>
        /// <param name="p"></param>
        /// <param name="size">Object size, in pixels</param>
        public void DrawObjectLabel(TextRenderer textRenderer, string label, Font font, Brush brush, PointF p, float size) 
        {
            SizeF b = System.Windows.Forms.TextRenderer.MeasureText(label, font);

            float s = size > 5 ? (size / 2.8284f + 2) : 1;
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    float dx = x == 0 ? s : -s - b.Width;
                    float dy = y == 0 ? -s : s + b.Height;
                    RectangleF r = new RectangleF(p.X + dx, p.Y + dy, b.Width, b.Height);
                    if (!labels.Any(l => l.IntersectsWith(r)))
                    {
                        textRenderer.DrawString(label, font, brush, r.Location);
                        labels.Add(r);
                        return;
                    }
                }
            }
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
            public bool IsMirrored
            {
                get => map.Projection.IsMirrored;
                set => map.Projection.IsMirrored = value;
            }

            public bool IsInverted
            {
                get => map.Projection.IsInverted;
                set => map.Projection.IsInverted = value;
            }

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
                return map.settings.Get<SkyColor>(colorName).GetColor(Schema, DayLightFactor);
            }

            public Color GetColor(Color colorNight)
            {
                return SkyColor.GetColor(Schema, colorNight, DayLightFactor);
            }

            public Color GetColor(Color colorNight, Color colorDay)
            {
                return SkyColor.GetColor(Schema, colorNight, colorDay, DayLightFactor);
            }

            public Color GetSkyColor()
            {
                // TODO: move to settings
                return GetColor(Color.Black);
            }

            public void Redraw()
            {
                map.Invalidate();
                map.OnRedraw();
            }
        }
    }
}
